using System;

namespace Hp.Ohl.WmiService.Models
{
    public abstract class HpBiosEventPayload
    {
    }

    public class NotHandledPayload : HpBiosEventPayload
    {
        public byte[] OriginalBytes;
    }

    public class OmenKeyPressedPayload : HpBiosEventPayload
    {
    }

    public class TouchPadTogglePayload : HpBiosEventPayload
    {
        public bool IsTouchPadEnabled;
    }

    public class WinKeyLockPayload : HpBiosEventPayload
    {
        public bool IsWinKeyEnabled;
    }

    public class PowerAdapterPayload : HpBiosEventPayload
    {
    }

    public class LowPowerAdapterPayload : HpBiosEventPayload
    {
    }

    public class IncompatiblePowerAdapterPayload : HpBiosEventPayload
    {
    }

    public class HpBiosEventArgs : EventArgs
    {
        public uint eventData;
        public uint eventId;
        public HpBiosEventPayload eventPayload;
        public ulong timeCreated;
    }

    public delegate void HpBiosEventHandler(object sender, HpBiosEventArgs e);
}