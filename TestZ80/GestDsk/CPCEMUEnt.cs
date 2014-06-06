namespace TestZ80 {
	public class CPCEMUEnt {
		public string id; // "MV - CPCEMU Disk-File\r\nDisk-Info\r\n"
		public byte NbTracks;
		public byte NbHeads;
		public short TrackSize; // 0x1300 = 256 + ( 512 * nbsecteurs )
		public byte[] TrackSizeTable = new byte[204]; // Si "EXTENDED CPC DSK File"
	};
}

