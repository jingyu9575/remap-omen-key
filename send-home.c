#include <windows.h>

int WINAPI WinMainCRTStartup(void) {
	INPUT ip;

	ip.type = INPUT_KEYBOARD;
	ip.ki.time = 0;
	ip.ki.wVk = 0;
	ip.ki.dwExtraInfo = 0;
	ip.ki.dwFlags = KEYEVENTF_EXTENDEDKEY | KEYEVENTF_SCANCODE;
	ip.ki.wScan = MapVirtualKey(VK_HOME, MAPVK_VK_TO_VSC);

	SendInput(1, &ip, sizeof(INPUT));

	ip.ki.dwFlags = KEYEVENTF_EXTENDEDKEY | KEYEVENTF_SCANCODE | KEYEVENTF_KEYUP;
	SendInput(1, &ip, sizeof(INPUT));

	ExitProcess(0);
	return 0;
}
