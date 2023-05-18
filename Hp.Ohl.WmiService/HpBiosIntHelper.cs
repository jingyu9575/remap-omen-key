using System;
using System.ComponentModel;
using System.Linq;
using System.Management;
using Hp.Ohl.WmiService.Models;

namespace Hp.Ohl.WmiService
{
    public static class HpBiosIntHelper
    {
        private static readonly ManagementObject ClassInstance;
        private static readonly byte[] Sign;

        static HpBiosIntHelper()
        {
            var searcher = new ManagementObjectSearcher(@"root\wmi",
                "SELECT * FROM hpqBIntM");
            ClassInstance = searcher.Get().OfType<ManagementObject>().First();
            Sign = new byte[] {83, 69, 67, 85};
        }

        public static void Initialize() { /* call static ctor */ }

        public static HpBiosDataOut InvokeBiosCommand(uint command, uint commandType, uint size, byte[] biosData = null)
        {
            if (biosData == null)
                biosData = Array.Empty<byte>();

            ManagementObject dataIn = new ManagementClass(@"root\wmi:hpqBDataIn");
            var inParams = ClassInstance.GetMethodParameters(@"hpqBIOSInt128");

            dataIn["Command"] = command;
            dataIn["CommandType"] = commandType;
            dataIn["hpqBData"] = biosData;
            dataIn["Size"] = biosData.Length;
            dataIn["Sign"] = Sign;
            inParams["InData"] = dataIn;

            var outParams = ClassInstance.InvokeMethod($"hpqBIOSInt{size}", inParams, null);
            var dataOut = (ManagementBaseObject) outParams?["OutData"];
            if (dataOut == null)
            {
                throw new Exception("BIOS not responding to WMI command");
            }

            switch ((uint) dataOut.Properties["rwReturnCode"].Value)
            {
                case 0x03:
                    throw new Exception("Command not available");
                case 0x05:
                    throw new Exception("Size is too small");
            }

            if (dataOut.ClassPath.ClassName == "hpqBDataOut0")
            {
                return new HpBiosDataOut(dataOut.ClassPath.ClassName, (bool?) dataOut.Properties["Active"].Value,
                    null, (string) dataOut.Properties["InstanceName"].Value,
                    (uint) dataOut.Properties["rwReturnCode"].Value, (byte[]) dataOut.Properties["Sign"].Value);
            }

            if (((byte[]) dataOut.Properties["Data"].Value).Length != size)
            {
                // OMENEventSource.Log.CommandWarn("InvokeBiosCommand", "BIOS return data length is not expected length");
            }

            return new HpBiosDataOut(dataOut.ClassPath.ClassName, (bool?) dataOut.Properties["Active"].Value,
                (byte[]) dataOut.Properties["Data"].Value, (string) dataOut.Properties["InstanceName"].Value,
                (uint) dataOut.Properties["rwReturnCode"].Value, (byte[]) dataOut.Properties["Sign"].Value);
        }
    }
}