using System.Drawing;

namespace TestZ80 {
	static class BitmapCpc {
		static private DirectBitmap source = null;
		static private int[][][] TabPoints = new int[4][][];

		static public Bitmap Init(int width, int height) {
			source = new DirectBitmap(width, height);
			for (int i = 0; i < 4; i++)
				TabPoints[i] = new int[256][];

			for (int i = 0; i < 256; i++) {
				int b0 = i & 1;
				int b1 = i & 2;
				int b2 = (i & 4) >> 1;
				int b3 = (i & 8) >> 2;
				int b4 = (i & 0x10) >> 4;
				int b5 = (i & 0x20) >> 5;
				int b6 = (i & 0x40) >> 6;
				int b7 = i >> 7;

				// Mode 0
				TabPoints[0][i] = new int[8];
				TabPoints[0][i][0] = TabPoints[0][i][1] = TabPoints[0][i][2] = TabPoints[0][i][3] = b7 + (b5 << 2) + b3 + (b1 << 2);
				TabPoints[0][i][4] = TabPoints[0][i][5] = TabPoints[0][i][6] = TabPoints[0][i][7] = b6 + (b4 << 2) + b2 + (b0 << 3);

				// Mode 1
				TabPoints[1][i] = new int[8];
				TabPoints[1][i][0] = TabPoints[1][i][1] = b7 + b3;
				TabPoints[1][i][2] = TabPoints[1][i][3] = b6 + b2;
				TabPoints[1][i][4] = TabPoints[1][i][5] = b5 + b1;
				TabPoints[1][i][6] = TabPoints[1][i][7] = b4 + (b0 << 1);

				// Mode 2
				TabPoints[2][i] = new int[8];
				TabPoints[2][i][0] = b7;
				TabPoints[2][i][1] = b6;
				TabPoints[2][i][2] = b5;
				TabPoints[2][i][3] = b4;
				TabPoints[2][i][4] = (b3 >> 1);
				TabPoints[2][i][5] = (b2 >> 1);
				TabPoints[2][i][6] = (b1 >> 1);
				TabPoints[2][i][7] = b0;

				// Mode 3
				TabPoints[3][i] = new int[8];
				TabPoints[3][i][0] = TabPoints[3][i][1] = TabPoints[3][i][2] = TabPoints[3][i][3] = b7 + b3;
				TabPoints[3][i][4] = TabPoints[3][i][5] = TabPoints[3][i][6] = TabPoints[3][i][7] = b6 + b2;
			}
			return source.Bitmap;
		}

		static public void TraceMot(int x, int y, int adrMemCpc) {
			x <<= 1;
			y <<= 1;
			if (adrMemCpc < 0) {
				for (int i = 0; i < 16; i++) {
					source.SetPixelDoubleHeight(x + i, y, VGA.tabCoul[16]);
					if (i == 7)
						VGA.SyncColor();
				}
			}
			else {
				int oct = VGA.ram[adrMemCpc++];
				for (int i = 0; i < 16; i++) {
					source.SetPixelDoubleHeight(x + i, y, VGA.tabCoul[TabPoints[CRTC.LastMode][oct][i & 7]]);
					if (i == 7) {
						VGA.SyncColor();
						oct = VGA.ram[adrMemCpc];
					}
				}
			}
		}
	}
}
