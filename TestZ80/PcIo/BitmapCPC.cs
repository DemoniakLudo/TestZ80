﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace TestZ80 {
	static class BitmapCpc {
		static private Bitmap source = null;
		static private Rectangle rect;
		static private byte[] pixels;
		static private int width;
		static private int[][][] TabPoints = new int[4][][];

		static public void Init(Bitmap s) {
			source = s;
			width = source.Width;
			int height = source.Height;
			pixels = new byte[width * height * (Bitmap.GetPixelFormatSize(source.PixelFormat) >> 3)];
			rect = new Rectangle(0, 0, width, height);
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
		}

		static public void RefreshBitmap() {
			BitmapData bitmapData = source.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
			Marshal.Copy(pixels, 0, bitmapData.Scan0, pixels.Length);
			source.UnlockBits(bitmapData);
		}

		static public void TraceMot(int x, int y, int adrMemCpc) {
			int adrPixel = ((y * width) + x) << 3;
			int posPixel = adrPixel;
			if (adrMemCpc < 0) {
				byte r = (byte)VGA.tabCoul[16];
				byte v = (byte)(VGA.tabCoul[16] >> 8);
				byte b = (byte)(VGA.tabCoul[16] >> 16);
				for (int i = 0; i < 16; i++) {
					pixels[posPixel++] = r;
					pixels[posPixel++] = v;
					pixels[posPixel++] = b;
					pixels[posPixel++] = 0xFF;
					if (i == 7)
						VGA.SyncColor();
				}
			}
			else {
				int oct = VGA.ram[adrMemCpc++];
				for (int i = 0; i < 16; i++) {
					int color = VGA.tabCoul[TabPoints[CRTC.LastMode][oct][i & 7]];
					pixels[posPixel++] = (byte)(color);
					pixels[posPixel++] = (byte)(color >> 8);
					pixels[posPixel++] = (byte)(color >> 16);
					pixels[posPixel++] = 0xFF;
					if (i == 7) {
						VGA.SyncColor();
						oct = VGA.ram[adrMemCpc];
					}
				}
			}
			Buffer.BlockCopy(pixels, adrPixel, pixels, adrPixel + (width << 2), 64);
		}
	}
}