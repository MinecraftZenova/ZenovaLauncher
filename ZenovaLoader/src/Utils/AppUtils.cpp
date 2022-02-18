#include "AppUtils.h"

#include <atlbase.h> // CComPtr

namespace AppUtils {

// String found in HKEY_CLASSES_ROOT\AppX[MinecraftPackage]
// Need to find some way to not be dependent on this string
#define MINECRAFT_DEFAULT_NAME L"Minecraft"

std::wstring GetMinecraftPackageId() {
	std::wstring packageId;

	ATL::CRegKey key, subKey;
	LPWSTR szBuffer = new WCHAR[1024], szSubBuffer = new WCHAR[1024];
	DWORD dwBufferLen = 1024, dwSubBufferLen = 1024;
	DWORD index = 0;

	LRESULT result = ERROR_SUCCESS;

	result = key.Open(HKEY_CLASSES_ROOT, L"", KEY_ENUMERATE_SUB_KEYS | KEY_QUERY_VALUE | KEY_READ);
	if(result != ERROR_SUCCESS) return packageId;

	for(; key.EnumKey(index, szBuffer, &dwBufferLen, NULL) == ERROR_SUCCESS; dwBufferLen = 1024, index++) {
		std::wstring keyName(szBuffer);
		if(keyName.substr(0, 4) != L"AppX") continue;

		result = subKey.Open(HKEY_CLASSES_ROOT, keyName.c_str(), KEY_QUERY_VALUE | KEY_READ);
		if(result != ERROR_SUCCESS) continue;

		szSubBuffer = new TCHAR[1024];
		dwSubBufferLen = 1024;
		result = subKey.QueryStringValue(L"", szSubBuffer, &dwSubBufferLen);
		if(result != ERROR_SUCCESS) continue;

		std::wstring keyValue(szSubBuffer);
		if(keyValue.find(MINECRAFT_DEFAULT_NAME) == keyValue.npos) continue;

		szSubBuffer = new TCHAR[1024];
		dwSubBufferLen = 1024;
		result = subKey.Open(HKEY_CLASSES_ROOT, (keyName + L"\\Shell\\Open").c_str(), KEY_QUERY_VALUE | KEY_READ);
		if(result != ERROR_SUCCESS) continue;

		result = subKey.QueryStringValue(L"PackageId", szSubBuffer, &dwSubBufferLen);
		if(result != ERROR_SUCCESS) continue;

		packageId = szSubBuffer;

		break;
	}

	subKey.Close();
	key.Close();

	return packageId;
}

HRESULT LaunchApplication(LPCWSTR packageFullName, PDWORD pdwProcessId) {
	CComPtr<IApplicationActivationManager> spAppActivationManager;
	HRESULT result = E_INVALIDARG;

	/* Initialize IApplicationActivationManager */
	result = CoCreateInstance(CLSID_ApplicationActivationManager,
		NULL,
		CLSCTX_LOCAL_SERVER,
		IID_IApplicationActivationManager,
		(LPVOID*)&spAppActivationManager);

	if(!SUCCEEDED(result)) return result;
	
	/* This call ensures that the app is launched as the foreground window */
	result = CoAllowSetForegroundWindow(spAppActivationManager, NULL);

	/* Launch the app */
	if(!SUCCEEDED(result)) return result;
	
	result = spAppActivationManager->ActivateApplication(packageFullName, NULL, AO_NONE, pdwProcessId);

	return result;
}

PACKAGE_EXECUTION_STATE GetPackageExecutionState(const std::wstring& PackageFullName) {
	CComPtr<IPackageDebugSettings> debugSettings;
	HRESULT result = debugSettings.CoCreateInstance(CLSID_PackageDebugSettings, NULL, CLSCTX_ALL);
	if(result != S_OK || !debugSettings) {
		//std::cout << "Debug Settings is null\n";
		return PES_UNKNOWN;
	}

	PACKAGE_EXECUTION_STATE packageState = PES_UNKNOWN;
	debugSettings->GetPackageExecutionState(PackageFullName.c_str(), &packageState);

	return packageState;
}

} // namespace AppUtils