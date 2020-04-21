using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace ZenovaLauncher
{
    public class VersionDownloader
    {
        public static VersionDownloader standard;
        public static VersionDownloader user;

        private HttpClient _client = new HttpClient();
        private WUProtocol _protocol = new WUProtocol();

        private async Task<XDocument> PostXmlAsync(string url, XDocument data)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            using (var stringWriter = new StringWriter())
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { Indent = false, OmitXmlDeclaration = true }))
                {
                    data.Save(xmlWriter);
                }
                request.Content = new StringContent(stringWriter.ToString(), Encoding.UTF8, "application/soap+xml");
            }
            using (var resp = await _client.SendAsync(request))
            {
                string str = await resp.Content.ReadAsStringAsync();
                return XDocument.Parse(str);
            }
        }

        private async Task DownloadFileChunk(string url, DownloadProgress progress, long? totalSize, CancellationToken cancellationToken, Tuple<long, long> readRange, int index, ConcurrentDictionary<int, String> tempFilesDictionary)
        {
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            httpRequestMessage.Headers.Range = new RangeHeaderValue(readRange.Item1, readRange.Item2);
            using (var resp = await _client.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
                string tempFilePath = Path.GetTempFileName();
                Debug.WriteLine("DownloadChunk" + index + ": " + tempFilePath);
                using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.Write))
                using (var inStream = await resp.Content.ReadAsStreamAsync())
                {
                    byte[] buf = new byte[1024 * 1024];
                    while (true)
                    {
                        int n = await inStream.ReadAsync(buf, 0, buf.Length, cancellationToken);
                        if (n == 0)
                            break;
                        await fileStream.WriteAsync(buf, 0, n, cancellationToken);
                        progress(n, totalSize);
                    }
                    tempFilesDictionary.TryAdd((int)index, tempFilePath);
                }
            }
        }

        private async Task DownloadFile(string url, string to, DownloadProgress progress, CancellationToken cancellationToken, int parallelDownloads = 0)
        {
            if (parallelDownloads <= 0)
                parallelDownloads = Environment.ProcessorCount;
            long? totalSize;
            using (var resp = await _client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
                totalSize = resp.Content.Headers.ContentLength;
                Debug.WriteLine("TotalSize of Download: " + totalSize);
                progress(0, totalSize);
            }
            using (var outStream = new FileStream(to, FileMode.Create))
            {
                ConcurrentDictionary<int, string> tempFilesDictionary = new ConcurrentDictionary<int, string>();

                List<Tuple<long, long>> readRanges = new List<Tuple<long, long>>();
                for (int chunk = 0; chunk < parallelDownloads - 1; chunk++)
                {
                    var range = new Tuple<long, long>
                    (   
                        chunk * (totalSize.GetValueOrDefault() / parallelDownloads),
                        ((chunk + 1) * (totalSize.GetValueOrDefault() / parallelDownloads)) - 1
                    );
                    readRanges.Add(range);
                }

                readRanges.Add(new Tuple<long, long>
                (
                    readRanges.Any() ? readRanges.Last().Item2 + 1 : 0,
                    totalSize.GetValueOrDefault() - 1
                ));

                int index = 0;
                var DownloadTasks = readRanges.Select(readRange =>
                {
                    var task = DownloadFileChunk(url, progress, totalSize, cancellationToken, readRange, index, tempFilesDictionary);
                    index++;
                    return task;
                });
                await Task.WhenAll(DownloadTasks);

                foreach (var tempFile in tempFilesDictionary.OrderBy(b => b.Key))
                {
                    byte[] tempFileBytes = File.ReadAllBytes(tempFile.Value);
                    await outStream.WriteAsync(tempFileBytes, 0, tempFileBytes.Length, cancellationToken);
                    File.Delete(tempFile.Value);
                }
            }
        }

        private async Task<string> GetDownloadUrl(string updateIdentity, string revisionNumber)
        {
            XDocument result = await PostXmlAsync(_protocol.GetDownloadUrl(),
                _protocol.BuildDownloadRequest(updateIdentity, revisionNumber));
            foreach (string s in _protocol.ExtractDownloadResponseUrls(result))
            {
                if (s.StartsWith("http://tlu.dl.delivery.mp.microsoft.com/"))
                    return s;
            }
            return null;
        }

        public async Task EnableUserAuthorization()
        {
            if (_protocol.MSAUserToken == null)
                _protocol.MSAUserToken = await WUTokenHelper.GetWUToken(Preferences.instance.SelectedAccount.AccountId);
        }

        public async Task Download(string updateIdentity, string revisionNumber, string destination, DownloadProgress progress, CancellationToken cancellationToken)
        {
            string link = await GetDownloadUrl(updateIdentity, revisionNumber);
            if (link == null)
                throw new ArgumentException("Bad updateIdentity");
            Debug.WriteLine("Resolved download link: " + link);
            await DownloadFile(link, destination, progress, cancellationToken);
        }

        public delegate void DownloadProgress(long current, long? total);
    }
}
