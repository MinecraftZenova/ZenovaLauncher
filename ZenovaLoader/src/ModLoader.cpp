#define _CRT_SECURE_NO_WARNINGS

#include <Windows.h>
#include <ShObjIdl.h>
#include <AclAPI.h> // ModLoader::AdjustGroupPolicy
#include <sddl.h> // ConverStringSidToSid
#include <Psapi.h> // GetModuleFileNameEx
#include <atlbase.h> // CComPtr


#include <string>

/* including for archival purposes
// need to interface with ZenovaLauncher logger once implemented
#define FMT_HEADER_ONLY
#include <fmt/format.h>
#include <fmt/xchar.h>

template<typename... Args>
void print(const std::string& format, const Args&... args) {
	std::string s = "[ZenovaLoader] " + fmt::format(format, args...) + '\n';
	OutputDebugStringA(s.c_str());
}

template<typename... Args>
void print(const std::wstring& format, const Args&... args) {
	std::wstring s = L"[ZenovaLoader] " + fmt::format(format, args...) + L'\n';
	OutputDebugStringW(s.c_str());
}
*/

template<typename Str, typename... Args>
void print(const Str& format, const Args&... args) {}

namespace AppUtils {

	PACKAGE_EXECUTION_STATE GetPackageExecutionState(const std::wstring& PackageFullName) {
		PACKAGE_EXECUTION_STATE packageState = PES_UNKNOWN;

		CComPtr<IPackageDebugSettings> debugSettings;
		HRESULT result = debugSettings.CoCreateInstance(CLSID_PackageDebugSettings, NULL, CLSCTX_ALL);
		if (result == S_OK && debugSettings) {
			debugSettings->GetPackageExecutionState(PackageFullName.c_str(), &packageState);
		}
		else {
			print("Debug Settings is null\n");
		}

		return packageState;
	}

} // namespace AppUtils

namespace ProcessUtils {

	// Windows Internal Functions
	void ResumeProcess(HANDLE processHandle) {
		using NtResumeProcess = LONG(NTAPI*)(IN HANDLE ProcessHandle);
		NtResumeProcess pfnNtResumeProcess = (NtResumeProcess)GetProcAddress(GetModuleHandle(L"ntdll"), "NtResumeProcess");
		pfnNtResumeProcess(processHandle);
	}

} // namespace ProcessUtils

// helper type
class unique_handle {
	HANDLE mHandle = NULL;

public:
	unique_handle(HANDLE handle) : mHandle(handle) {}
	~unique_handle() { CloseHandle(mHandle); }

	operator HANDLE&() { return mHandle; }
};

namespace ModLoader {

BOOL InjectDLL(DWORD dwProcessId, const std::wstring& dllPath) {
	class Memory {
		LPVOID mMem = NULL;
		HANDLE mHandle = NULL;

	public:
		Memory(LPVOID mem, HANDLE handle) : mMem(mem), mHandle(handle) {}
		~Memory() { VirtualFreeEx(mHandle, mMem, 0, MEM_RELEASE); }

		operator LPVOID& () { return mMem; }
	};

	/* Open the process with all access */
	unique_handle hProc = OpenProcess(PROCESS_ALL_ACCESS, FALSE, dwProcessId);
	if (hProc == NULL) {
		print("Could not open the process ({}) HRESULT: {}", dwProcessId, GetLastError());
		return FALSE;
	}

	/* Allocate memory to hold the path to the DLL File in the process's memory */
	SIZE_T dllPathSize = dllPath.size() * sizeof(wchar_t);
	Memory hRemoteMem(VirtualAllocEx(hProc, NULL, dllPathSize, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE), hProc);
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

	WaitForSingleObject(hRemoteThread, INFINITE);

	DWORD exitCode = NULL;
	GetExitCodeThread(hRemoteThread, &exitCode);
	if (exitCode == NULL) {
		print("Failed to load dll in the process ({}) HRESULT: {}", dwProcessId, GetLastError());
	}

	return TRUE;
}

} // namespace ModLoader

// Turning this into a normal Windows program hides the GUI
int APIENTRY WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int nShowCmd) {
	DWORD dwProcessId = 0;
	bool debug = false;
	std::wstring packageid = L"";
	std::wstring zenova_data = L"";

	for (int i = 1; i < __argc; i += 2) {
		const char* arg = __argv[i];
		const char* value = __argv[i + 1];

		print("{}, {}", arg, value);
		if (arg[0] != '-') {
			continue;
		}

		switch (arg[1]) {
			case 'p': {
				dwProcessId = atoi(value);
			} break;
			case 'd': {
				debug = atoi(value);
			} break;
			case 'i': {
				packageid = std::wstring(value, value + strlen(value));
			} break;

			// todo: need to change how I'm handling this if zenova is installed in a multi-byte location
			case 'f': {
				zenova_data = std::wstring(value, value + strlen(value));
			} break;
			default: break;
		}
	}

	if (dwProcessId == 0) {
		return S_OK;
	}

	print(L"Parsed Args\n\tDebug: {}\n\tPackageId: {}\n\tZenova Folder: {}", debug, packageid, zenova_data);

	wchar_t filename[_MAX_FNAME];
	unique_handle processhandle = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION | PROCESS_SUSPEND_RESUME, FALSE, dwProcessId);
	GetModuleFileNameExW(processhandle, NULL, filename, _MAX_FNAME);

	std::wstring_view svName = filename;
	// RuntimeBroker.exe is called with UWP Packages, this avoids being attached to it
	if (svName.substr(svName.rfind('\\') + 1) != L"RuntimeBroker.exe" && SUCCEEDED(CoInitializeEx(NULL, COINIT_APARTMENTTHREADED))) {
		print("CoInitialize succeeded");
		
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

		if (AppUtils::GetPackageExecutionState(packageid) != PES_UNKNOWN) {
			//Assume the game is suspended and inject ZenovaAPI
			ModLoader::InjectDLL(dwProcessId, zenova_data + L"\\ZenovaAPI.dll");
		}
		else {
			print("App PES is unknown");
		}

		CoUninitialize();
	}

	// Resume the game
	ProcessUtils::ResumeProcess(processhandle);

	print("Gracefully exit");

	return S_OK;
}