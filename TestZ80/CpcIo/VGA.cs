namespace TestZ80 {
	static class VGA {
		public const int ROMINF_OFF = 0x04;
		public const int ROMSUP_OFF = 0x08;
		public const int BANK_SIZE = 0x4000;
		public const int MAX_ROMS = 256;
		public const int LOWER_ROM_INDEX = 255;
		public const int BASIC_ROM_INDEX = 0;
		private const int LOWER_ROM_OFFSET = 0x1FF * BANK_SIZE;
		private const int BASIC_ROM_OFFSET = 0x100 * BANK_SIZE;
		private const int DISK_ROM_OFFSET = 0x107 * BANK_SIZE;
		static public byte[] ram = new byte[512 * BANK_SIZE];
		static public byte NumRomExt = 0;
		static private readonly int[][] TabPeek = new int[8][];
		static private readonly int[][] TabPoke = new int[8][];
		static public int MemoMode = 0;
		static public byte bank = 0;
		static private byte PenSelect;
		static private int MemoColor = 0xFF;
		static public int DecodeurAdresse = 0;
		static public int DelayGa = 0;
		static public int CntHSync = 0;
		static public int[] tabCoul = new int[32];
		static public int[] tabInk = new int[32];
		static private int[] RgbCPCColor =
				{
				//RRVVBB
				0x6E7D6B,                   // Blanc            (13) -> #40
				0x6E7B6D,                   // Blanc            (13) -> #41
				0x00F36B,                   // Vert Marin       (19) -> #42
				0xF3F36D,                   // Jaune Pastel     (25) -> #43
				0x00026B,                   // Bleu              (1) -> #44
				0xF00268,                   // Pourpre           (7) -> #45
				0x007868,                   // Turquoise        (10) -> #46
				0xF37D6B,                   // Rose             (16) -> #47
				0xF30268,                   // Pourpre           (7) -> #48
				0xF3F36B,                   // Jaune pastel     (25) -> #49
				0xF3F30D,                   // Jaune vif        (24) -> #4A
				0xFFF3F9,                   // Blanc Brillant   (26) -> #4B
				0xF30506,                   // Rouge vif         (6) -> #4C
				0xF302F4,                   // Magenta vif       (8) -> #4D
				0xF37D0D,                   // Orange           (15) -> #4E
				0xFA80F9,                   // Magenta pastel   (17) -> #4F
				0x000268,                   // Bleu              (1) -> #50
				0x02F36B,                   // Vert Marin       (19) -> #51
				0x02F001,                   // Vert vif         (18) -> #52
				0x0FF3F2,                   // Turquoise vif    (20) -> #53
				0x000201,                   // Noir              (0) -> #54
				0x0C02F4,                   // Bleu vif          (2) -> #55
				0x027801,                   // Vert              (9) -> #56
				0x0C7BF4,                   // Bleu ciel        (11) -> #57
				0x690268,                   // Magenta           (4) -> #58
				0x71F36B,                   // Vert pastel      (22) -> #59
				0x71F504,                   // Vert citron      (21) -> #5A
				0x71F3F4,                   // Turquoise pastel (23) -> #5B
				0x6C0201,                   // Rouge             (3) -> #5C
				0x6C02F2,                   // Mauve             (5) -> #5D
				0x6E7B01,                   // Jaune            (12) -> #5E
				0x6E7BF6                    // Bleu pastel      (14) -> #5F
				};

		static void SetPeekMode() {
			for (int b = 0; b < 8; b++) {
				TabPeek[b][0] = (DecodeurAdresse & ROMINF_OFF) == 0 ? LOWER_ROM_OFFSET : TabPoke[b][0];
				TabPeek[b][3] = (DecodeurAdresse & ROMSUP_OFF) == 0 ? (0x100 + NumRomExt) * BANK_SIZE : TabPoke[b][3];
			}
		}

		static public void POKE8(int adr, byte value) {
			ram[TabPoke[bank][adr >> 14] + (adr & 0x3FFF)] = value;
		}

		static public byte PEEK8(int adr) {
			return ram[TabPeek[bank][adr >> 14] + (adr & 0x3FFF)];
		}

		static public ushort PEEK16(int adr) {
			ushort r = ram[TabPeek[bank][adr >> 14] + (adr & 0x3FFF)];
			adr = adr+1 & 0xFFFF;
			r += (ushort)(ram[TabPeek[bank][adr >> 14] + (adr & 0x3FFF)] << 8);
			return r;
		}

		static public void POKE16(ushort adr, ushort r) {
			ram[TabPoke[bank][adr >> 14] + (adr & 0x3FFF)] = (byte)r;
			adr++;
			ram[TabPoke[bank][adr >> 14] + (adr & 0x3FFF)] = (byte)(r >> 8);
		}

		static public void SyncColor() {
			if (MemoColor != 0xFF) {
				if (DelayGa == 0) {
					tabCoul[PenSelect] = RgbCPCColor[MemoColor];
					tabInk[PenSelect] = MemoColor;
					MemoColor = 0xFF;
				}
				else
					DelayGa--;
			}
		}

		static public void Write(int val) {
			int newVal = val & 0x1F;
			switch (val >> 6) {
				case 0:
					PenSelect = (byte)(newVal < 16 ? newVal : 16);
					break;

				case 1:
					MemoColor = newVal;
					break;

				case 2:
					MemoMode = val & 3;
					DecodeurAdresse = val;
					SetPeekMode();
					if ((val & 16) == 16) {
						CntHSync = 0;
						Z80.IRQ = 0;
					}
					break;

				case 3:
					bank = (byte)(val & 7);
					break;
			}
		}

		static public void WriteROM(int val) {
			NumRomExt = (byte)val;
			SetPeekMode();
		}

		static public void Init() {
			TabPeek[0] = new int[4] { 0x00000, 0x04000, 0x08000, 0x0C000 };
			TabPeek[1] = new int[4] { 0x00000, 0x04000, 0x08000, 0x1C000 };
			TabPeek[2] = new int[4] { 0x10000, 0x14000, 0x18000, 0x1C000 };
			TabPeek[3] = new int[4] { 0x00000, 0x0C000, 0x08000, 0x1C000 };
			TabPeek[4] = new int[4] { 0x00000, 0x10000, 0x08000, 0x0C000 };
			TabPeek[5] = new int[4] { 0x00000, 0x14000, 0x08000, 0x0C000 };
			TabPeek[6] = new int[4] { 0x00000, 0x18000, 0x08000, 0x0C000 };
			TabPeek[7] = new int[4] { 0x00000, 0x1C000, 0x08000, 0x0C000 };
			TabPoke[0] = new int[4] { 0x00000, 0x04000, 0x08000, 0x0C000 };
			TabPoke[1] = new int[4] { 0x00000, 0x04000, 0x08000, 0x1C000 };
			TabPoke[2] = new int[4] { 0x10000, 0x14000, 0x18000, 0x1C000 };
			TabPoke[3] = new int[4] { 0x00000, 0x0C000, 0x08000, 0x1C000 };
			TabPoke[4] = new int[4] { 0x00000, 0x10000, 0x08000, 0x0C000 };
			TabPoke[5] = new int[4] { 0x00000, 0x14000, 0x08000, 0x0C000 };
			TabPoke[6] = new int[4] { 0x00000, 0x18000, 0x08000, 0x0C000 };
			TabPoke[7] = new int[4] { 0x00000, 0x1C000, 0x08000, 0x0C000 };
			System.Buffer.BlockCopy(ROMS.ROMINF, 0, ram, LOWER_ROM_OFFSET, 0x4000);
			System.Buffer.BlockCopy(ROMS.ROMSUP, 0, ram, BASIC_ROM_OFFSET, 0x4000);
			System.Buffer.BlockCopy(ROMS.ROMDISC, 0, ram, DISK_ROM_OFFSET, 0x4000);
			Reset();
		}

		static public void Reset() {
			DecodeurAdresse = 0;
			SetPeekMode();
		}
	}
}
