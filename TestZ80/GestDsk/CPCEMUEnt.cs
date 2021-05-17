using System.Runtime.InteropServices;

namespace TestZ80 {
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct CPCEMUEnt {
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x30)]
		public string id; // "MV - CPCEMU Disk-File\r\nDisk-Info\r\n"
		public byte NbTracks;
		public byte NbHeads;
		public short TrackSize; // 0x1300 = 256 + ( 512 * nbsecteurs )
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 0xCC)]
		public byte[] TrackSizeTable; // Si "EXTENDED CPC DSK File"
	}
}

