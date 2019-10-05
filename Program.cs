using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading;

namespace RemapOMENKey {
	class Program {
		static void Main() => new Program().Run();

		struct KEYBDINPUT {
			public uint type;
			public ushort wVk;
			public ushort wScan;
			public uint dwFlags;
			public uint time;
			public uint dwExtraInfo;
			public uint unused1;
			public uint unused2;
		}
		static readonly int SIZEOF_KEYBDINPUT = Marshal.SizeOf(typeof(KEYBDINPUT));

		[DllImport("user32.dll", SetLastError = true)]
		static extern uint SendInput(uint cInputs,
			[MarshalAs(UnmanagedType.LPArray), In] KEYBDINPUT[] inputs, int cbSize);

		readonly ManagementEventWatcher watcher = new ManagementEventWatcher(
			new ManagementScope(@"\\localhost\root\wmi", null),
			new EventQuery("select * from hpqbevnt"));
		readonly ServiceController wmiSvc = new ServiceController("HPWMISVC");
		readonly RegistryKey regKey = Registry.ClassesRoot.OpenSubKey(
			@"CLSID\{60EB195D-B64E-4209-8AB8-53E040061B9C}\SystemEvent");

		readonly KEYBDINPUT[] homeInputs = new KEYBDINPUT[] {
			new KEYBDINPUT {
				type = 1, // INPUT_KEYBOARD
				wVk = 0x24, // VK_HOME
				dwFlags = 1, // KEYEVENTF_EXTENDEDKEY
				wScan = 0, time = 0, dwExtraInfo = 0,
				unused1 = 0, unused2 = 0,
			},
			new KEYBDINPUT(),
		};

		Program() {
			homeInputs[1] = homeInputs[0];
			homeInputs[1].dwFlags |= 2; // KEYEVENTF_KEYUP
			watcher.EventArrived += EventArrived;
		}

		void Run() {
			watcher.Start();
			Thread.Sleep(Timeout.Infinite);
		}

		void EventArrived(object sender, EventArrivedEventArgs e) {
			try {
				if (!4u.Equals(e.NewEvent["eventId"])) return;
				wmiSvc.ExecuteCommand(0x83);
				var buttonID = regKey.GetValue("ButtonID");
				if (!8613.Equals(buttonID)) return;

				SendInput((uint) homeInputs.Length, homeInputs, SIZEOF_KEYBDINPUT);
			} catch (Exception exception) {
				Debug.WriteLine(exception);
			}
		}
	}
}
