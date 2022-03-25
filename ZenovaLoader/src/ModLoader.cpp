#define _CRT_SECURE_NO_WARNINGS

#include <Windows.h>
#include <ShObjIdl.h>
#include <AclAPI.h> // ModLoader::AdjustGroupPolicy
#include <sddl.h> // ConverStringSidToSid
#include <Psapi.h> // GetModuleFileNameEx

#include "Utils/AppUtils.h"
#include "Utils/ProcessUtils.h"

/* including for archival purposes 
// need to interface with ZenovaLauncher logger once implemented
#define FMT_HEADER_ONLY
#include <fmt/format.h>
#include <fmt/xchar.h>

template<typename... Args>
void print(const std::string& format, const Args&... args) {
	std::string s = "[ZenovaLoader] " + fmt::format(format, args...) + '\n';
	OutputDebugStringA(s.c_str());
	std::cout << s;
}

template<typename... Args>
void print(const std::wstring& format, const Args&... args) {
	std::wstring s = L"[ZenovaLoader] " + fmt::format(format, args...) + L'\n';
	OutputDebugStringW(s.c_str());
	std::wcout << s;
}
*/

template<typename Str, typename... Args>
void print(const Str& format, const Args&... args) {}

namespace ModLoader {

// helper type
class unique_handle {
	HANDLE mHandle = NULL;

public:
	unique_handle(HANDLE handle) : mHandle(handle) {}

	~unique_handle() {
		CloseHandle(mHandle);
	}

	operator HANDLE&() {
		return mHandle;
	}
};

BOOL InjectDLL(DWORD dwProcessId, const std::wstring& dllPath) {
	/* Open the process with all access */
	unique_handle hProc = OpenProcess(PROCESS_ALL_ACCESS, FALSE, dwProcessId);
	if (hProc == NULL) {
		print("Could not open the process ({}) HRESULT: {}", dwProcessId, GetLastError());
		return FALSE;
	}

	/* Allocate memory to hold the path to the DLL File in the process's memory */
	SIZE_T dllPathSize = dllPath.size() * sizeof(wchar_t);
	LPVOID hRemoteMem = VirtualAllocEx(hProc, NULL, dllPathSize, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
	if (hRemoteMem == NULL) {
		print("Could not allocate memory in the process ({}) HRESULT: {}", dwProcessId, GetLastError());
		return FALSE;
	}

	/* Write the path to the DLL File in the memory just allocated */
	if (!WriteProcessMemory(hProc, hRemoteMem, dllPath.data(), dllPathSize, NULL)) {
		print("Could not write memory in the process ({}) HRESULT: {}", dwProcessId, GetLastError());
		return FALSE;
	}

	/* Find the address of the LoadLibrary API */
	HMODULE hLocKernel32 = GetModuleHandleW(L"Kernel32");
	if (hLocKernel32 == NULL) {
		print("Could not get a handle on Kernel32 in the process ({}) HRESULT: {}", dwProcessId, GetLastError());
		return FALSE;
	}

	FARPROC hLocLoadLibrary = GetProcAddress(hLocKernel32, "LoadLibraryW");
	if (hLocLoadLibrary == NULL) {
		print("Could not find the locatin of LoadLibraryW in the process ({}) HRESULT: {}", dwProcessId, GetLastError());
		return FALSE;
	}
	/* Create a remote thread that invokes LoadLibrary for our DLL */
	unique_handle hRemoteThread = CreateRemoteThread(hProc, NULL, 0, (LPTHREAD_START_ROUTINE)hLocLoadLibrary, hRemoteMem, 0, NULL);
	if (hRemoteThread == NULL) {
		print("Could not create a remote thread in the process ({}) HRESULT: {}", dwProcessId, GetLastError());
		return FALSE;
	}

	return TRUE;
}

} // namespace ModLoader

// Turning this into a normal Windows program hides the GUI
int APIENTRY WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int nShowCmd) {
	DWORD dwProcessId = 0;
	bool debug = false;

	for(int i = 1; i < __argc; i += 2) {
		print("{}, {}", __argv[i], __argv[i+1]);

		std::string arg(__argv[i]);
		if(arg == "-p") {
			dwProcessId = atoi(__argv[i + 1]);
		}
		else if(arg == "-d") {
			debug = atoi(__argv[i + 1]);
		}
	}

	if (dwProcessId == 0) {
		return S_OK;
	}

	wchar_t filename[_MAX_FNAME];
	HANDLE processhandle = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, FALSE, dwProcessId);
	GetModuleFileNameExW(processhandle, NULL, filename, _MAX_FNAME);
	CloseHandle(processhandle);

	std::wstring_view svName = filename;
	// RuntimeBroker.exe is called with UWP Packages, this avoids being attached to it
	if (svName.substr(svName.rfind('\\') + 1) != L"RuntimeBroker.exe" && SUCCEEDED(CoInitializeEx(NULL, COINIT_APARTMENTTHREADED))) {
		print("CoInitialize succeeded");
		
		bool failed = false; // hack

		{ // Introduce scope so AppUtils::AppDebugger can cleanup COM objects before CoUninitialize
			if (debug) {
				print("Launching VS JIT Debugger");

				// https://docs.microsoft.com/en-us/windows/win32/procthread/creating-processes
				// Prepare handles.
				STARTUPINFOA si;
				PROCESS_INFORMATION pi; // The function returns this
				ZeroMemory(&si, sizeof(si));
				si.cb = sizeof(si);
				ZeroMemory(&pi, sizeof(pi));

				// Start the child process.
				std::string args = " -p " + std::to_string(dwProcessId);
				if (CreateProcessA("vsjitdebugger.exe", args.data(), NULL, NULL, FALSE, 0, NULL, NULL, &si, &pi)) {
					WaitForSingleObject(pi.hProcess, INFINITE);
					CloseHandle(pi.hProcess);
					CloseHandle(pi.hThread);
				}
				else {
					print("CreateProcess failed {}.", GetLastError());
				}
			}

			std::wstring minecraftPID = AppUtils::GetMinecraftPackageId();
			print(L"Got PackageID: {}", minecraftPID);

			if (AppUtils::GetPackageExecutionState(minecraftPID) == PES_UNKNOWN) {
				print("App PES is unknown");
				failed = true;
			}
			else {
				std::wstring zenovaPath = _wgetenv(L"ZENOVA_DATA");
				print(L"Path: {}", zenovaPath);

				//Assume the game is suspended and inject ZenovaAPI
				ModLoader::InjectDLL(dwProcessId, zenovaPath + L"\\ZenovaAPI.dll");
			}
		}

		CoUninitialize();

		if (failed) {
			return E_FAIL; // should we return here?
		}
	}

	// Resume the game
	ProcessUtils::ResumeProcess(dwProcessId);

	print("Gracefully exit");

	return S_OK;
}