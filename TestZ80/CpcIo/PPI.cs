namespace TestZ80 {
	static class PPI {
		const int REFRESH_HZ = 0x1E;// Screen refresh = 50Hz
		static int[] RegsPSG = new int[16];
		static int modePSG;
		static int RegPSGSel;
		static public int[] clav = new int[16] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
		static public byte[] RegsPPI = new byte[4];
		static int MaskPortC;

		static void UpdatePSG() {
			switch (modePSG) {
				case 2:
					//Write8912( RegPSGSel, RegsPPI[ 0 ] );
					break;

				case 3:
					RegPSGSel = RegsPPI[0] & 0x0F;
					break;
			}
		}

		static public void Write(int adr, int val) {
			switch ((adr >> 8) & 3) {
				case 0: // 0xF4xx
					RegsPPI[0] = (byte)val;
					UpdatePSG();
					break;

				case 1: // 0xF5xx
					break;

				case 2: // 0xF6xx
					RegsPPI[2] = (byte)val;
					RegsPSG[14] = clav[val & 0x0F];
					modePSG = val >> 6;
					UpdatePSG();
					break;

				case 3: // 0xF7xx
					RegsPPI[3] = (byte)val;
					if ((val & 0x80) == 0x80) {
						RegsPPI[0] = RegsPPI[2] = 0;
						MaskPortC = 0xFF;
						if ((val & 0x01) == 0x01)
							MaskPortC &= 0xF0;

						if ((val & 0x08) == 0x08)
							MaskPortC &= 0x0F;
					}
					else {
						int BitMask = 1 << ((val >> 1) & 0x07);
						if ((val & 1) == 1)
							RegsPPI[2] |= (byte)BitMask;
						else
							RegsPPI[2] &= (byte)~BitMask;
					}
					break;
			}
		}

		static public int Read(int adr) {
			switch ((adr >> 8) & 3) {
				case 0: // 0xF4xx
					return modePSG == 1 ? RegsPSG[RegPSGSel] : 0xFF;

				case 1: // 0xF5xx
					return (REFRESH_HZ | CRTC.VSync); // Port B toujours en lecture

				case 2: // 0xF6xx
					return (RegsPPI[2] & MaskPortC);
			}
			return (0xFF);
		}

		static public void Init() {
			Reset();
		}

		static public void Reset() {
		modePSG = 0;
		RegPSGSel = 0;
		MaskPortC = 0;
		}
	}
}
