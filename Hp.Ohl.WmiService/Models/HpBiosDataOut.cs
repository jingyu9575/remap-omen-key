namespace Hp.Ohl.WmiService.Models {
	public struct HpBiosDataOut {
		public string OriginalDataType;
		public bool? Active;
		public byte[] Data;
		public string InstanceName;
		public uint RwReturnCode;
		public byte[] Sign;

		public HpBiosDataOut(string originalDataType, bool? active, byte[] data,
				string instanceName, uint rwReturnCode, byte[] sign) {
			OriginalDataType = originalDataType;
			Active = active;
			Data = data;
			InstanceName = instanceName;
			RwReturnCode = rwReturnCode;
			Sign = sign;
		}
	}
}