#pragma once

#include <Windows.h>
#include <ShObjIdl.h>

#include <string>

namespace AppUtils {

// Returns the PackageId found in the registry location:
// HKEY_CLASSES_ROOT\AppX[MinecraftPackage]\Shell\Open\PackageId
// 
// May look something like this:
// Microsoft.MinecraftUWP_0.141.0.0_x64__8wekyb3d8bbwe
std::wstring GetMinecraftPackageId();

// Launches an application given the ApplicationId and returns the process ID via pdwProcessId
HRESULT LaunchApplication(LPCWSTR ApplicationId, PDWORD pdwProcessId);

// Call GetPackageExecutionState from IPackageDebugSettings
PACKAGE_EXECUTION_STATE GetPackageExecutionState(const std::wstring& PackageFullName);

} // namespace AppUtils