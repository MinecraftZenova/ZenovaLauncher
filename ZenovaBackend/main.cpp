#include "pch.h"
#include "combaseapi.h"
#include <thread>

using namespace winrt;
using namespace Windows::Foundation;
using namespace Windows::Security::Authentication::Web::Core;
using namespace Windows::Internal::Security::Authentication::Web;
using namespace Windows::Security::Credentials;
using namespace Windows::Security::Cryptography;

#define ZL_NO_ACCOUNT MAKE_HRESULT(SEVERITY_ERROR, FACILITY_ITF, 0x200)

extern "C" __declspec(dllexport) int  __stdcall GetMSAccounts(wchar_t*** accountNames, wchar_t*** accountIds, int* accountCount) {
	auto tokenBrokerStatics = get_activation_factory<TokenBrokerInternal, Windows::Foundation::IUnknown>();
	auto statics = tokenBrokerStatics.as<ITokenBrokerInternalStatics>();
	auto accounts = statics.FindAllAccountsAsync().get();
	wprintf(L"Account count = %i\n", accounts.Size());
	if (accounts.Size() == 0)
		return ZL_NO_ACCOUNT;

	*accountCount = accounts.Size();
	size_t arrayLen = sizeof(wchar_t*) * accounts.Size();
	*accountNames = (wchar_t**)::CoTaskMemAlloc(arrayLen);
	memset(*accountNames, 0, arrayLen);
	*accountIds = (wchar_t**)::CoTaskMemAlloc(arrayLen);
	memset(*accountIds, 0, arrayLen);
	for (int index = 0; index < accounts.Size(); index++) {
		auto accountInfo = accounts.GetAt(index);
		(*accountNames)[index] = (wchar_t*)::CoTaskMemAlloc((accountInfo.UserName().size() + 1) * sizeof(wchar_t));
		memcpy((*accountNames)[index], accountInfo.UserName().data(), (accountInfo.UserName().size() + 1) * sizeof(wchar_t));
		(*accountIds)[index] = (wchar_t*)::CoTaskMemAlloc((accountInfo.Id().size() + 1) * sizeof(wchar_t));
		memcpy((*accountIds)[index], accountInfo.Id().data(), (accountInfo.Id().size() + 1) * sizeof(wchar_t));
	}
	
	return S_OK;
}