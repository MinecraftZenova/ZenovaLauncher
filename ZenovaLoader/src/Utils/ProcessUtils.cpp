#include "ProcessUtils.h"

namespace ProcessUtils {

// Windows Internal Functions
typedef LONG(NTAPI* NtSuspendProcess)(IN HANDLE ProcessHandle);
typedef LONG(NTAPI* NtResumeProcess)(IN HANDLE ProcessHandle);
typedef LONG(NTAPI* NtTerminateProcess)(IN HANDLE ProcessHandle);

void SuspendProcess(DWORD processId) {
	HANDLE processHandle = OpenProcess(PROCESS_ALL_ACCESS, FALSE, processId);

	NtSuspendProcess pfnNtSuspendProcess = (NtSuspendProcess)GetProcAddress(GetModuleHandle(L"ntdll"), "NtSuspendProcess");

	pfnNtSuspendProcess(processHandle);
	CloseHandle(processHandle);
}

void ResumeProcess(DWORD processId) {
	HANDLE processHandle = OpenProcess(PROCESS_ALL_ACCESS, FALSE, processId);

	NtResumeProcess pfnNtResumeProcess = (NtResumeProcess)GetProcAddress(GetModuleHandle(L"ntdll"), "NtResumeProcess");

	pfnNtResumeProcess(processHandle);
	CloseHandle(processHandle);
}

void TerminateProcess(DWORD processId) {
	HANDLE processHandle = OpenProcess(PROCESS_ALL_ACCESS, FALSE, processId);

	NtTerminateProcess pfnNtTerminateProcess = (NtTerminateProcess)GetProcAddress(GetModuleHandle(L"ntdll"), "NtTerminateProcess");

	pfnNtTerminateProcess(processHandle);
	CloseHandle(processHandle);
}

} // namespace ProcessUtils