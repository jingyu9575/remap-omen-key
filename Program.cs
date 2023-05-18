using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using Hp.Ohl.WmiService;
using Hp.Ohl.WmiService.Models;

namespace RemapOMENKey {
	static class Program {
		[StructLayout(LayoutKind.Sequential)]
		struct INPUT {
			public uint type;
			public KEYBDINPUT ki;

			public static int Size {
				get { return Marshal.SizeOf(typeof(INPUT)); }
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		struct KEYBDINPUT {
			public ushort wVk;
			public ushort wScan;
			public uint dwFlags;
			public int time;
			public UIntPtr dwExtraInfo;
			public uint unused1;
			public uint unused2;
		}

		[DllImport("user32.dll", SetLastError = true)]
		static extern uint SendInput(uint cInputs, INPUT[] inputs, int cbSize);
		[DllImport("user32.dll")]
		static extern uint MapVirtualKey(uint uCode, uint uMapType);
		[DllImport("kernel32.dll")]
		static extern uint WTSGetActiveConsoleSessionId();

		const ushort VK_HOME = 0x24;

		static readonly INPUT[] HomeInputs = new INPUT[] {
			new INPUT {
				type = 1, // INPUT_KEYBOARD
				ki = new KEYBDINPUT {
					wVk = VK_HOME,
					dwFlags = 9, // KEYEVENTF_EXTENDEDKEY | KEYEVENTF_SCANCODE
					wScan = 0, time = 0, dwExtraInfo = (UIntPtr) 0,
					unused1 = 0, unused2 = 0,
				}
			},
			new INPUT(),
		};

		static readonly uint ThisSessionId = (uint) Process.GetCurrentProcess().SessionId;

		static Program() {
			HomeInputs[0].ki.wScan = (ushort) MapVirtualKey(VK_HOME, 0 /*MAPVK_VK_TO_VSC*/);
			HomeInputs[1] = HomeInputs[0];
			HomeInputs[1].ki.dwFlags |= 2; // KEYEVENTF_KEYUP
		}

		static void SendHome() {
			_ = SendInput((uint) HomeInputs.Length, HomeInputs, INPUT.Size);
		}

		const string KEY_PREFIX = "Global\\RemapOMENKey_db4af290-2c83-4bdd-b9b9-4261e3840ac0_";

		static void Main() {
			using (var identity = WindowsIdentity.GetCurrent()) {
				if (new WindowsPrincipal(identity)
						.IsInRole(WindowsBuiltInRole.Administrator)) {
					new Thread(WatcherThread).Start();
				}
			}

			var sec = new SemaphoreSecurity();
			sec.AddAccessRule(new SemaphoreAccessRule(
				new SecurityIdentifier(WellKnownSidType.WorldSid, null),
				SemaphoreRights.Modify, AccessControlType.Allow));
			using (var semaphore = new Semaphore(0, 999999,
					KEY_PREFIX + "trigger-" + ThisSessionId, out var _, sec)) {
				for (; ; ) {
					semaphore.WaitOne();
					SendHome();
				}
			}
		}

		static void WatcherThread() {
			var sec = new MutexSecurity();
			sec.AddAccessRule(new MutexAccessRule(
				new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null),
				MutexRights.Synchronize | MutexRights.Modify, AccessControlType.Allow));
			using (var mutex = new Mutex(false,
					KEY_PREFIX + "watcher", out var _, sec)) {
				try {
					mutex.WaitOne();
					WatcherThreadInMutex();
				} catch (AbandonedMutexException) {
					WatcherThreadInMutex();
				} finally {
					mutex.ReleaseMutex(); // unreachable
				}
			}
		}

		static void WatcherThreadInMutex() {
			HpBiosIntHelper.Initialize();
			WmiEventWatcher.HpBiosEventArrived += (sender, eventArgs) => {
				if (!(eventArgs.eventPayload is OmenKeyPressedPayload))
					return;
				var sessionId = WTSGetActiveConsoleSessionId();
				if (sessionId == 0xFFFFFFFF)
					return;
				if (sessionId == ThisSessionId) {
					SendHome();
				} else {
					if (Semaphore.TryOpenExisting(KEY_PREFIX + "trigger-" + sessionId,
							SemaphoreRights.Modify, out var semaphore))
						using (var _ = semaphore)
							semaphore.Release();
				}
			};
			WmiEventWatcher.StartHpBiosEventWatcher();
			Thread.Sleep(Timeout.Infinite);
		}
	}
}
