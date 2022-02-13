#pragma once

#include <Windows.h>

namespace ProcessUtils {

void SuspendProcess(DWORD processId);
void ResumeProcess(DWORD processId);
void TerminateProcess(DWORD processId);

} // namespace ProcessUtils