using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace TestZ80 {
	public class DirectBitmap : IDisposable {
		public Bitmap Bitmap { get; private set; }
		private uint[] Bits;
		private bool Disposed;
		private int Width;
		private GCHandle BitsHandle;

		public DirectBitmap(int width, int height) {
			Width = width;
			Bits = new uint[width * height];
			BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
			Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppRgb, BitsHandle.AddrOfPinnedObject());
		}

		public void SetPixelDoubleHeight(int pixelX, int pixelY, int c) {
			uint color = (uint)c | 0xFF000000;
			int index = pixelX + (pixelY * Width);
			Bits[index] = Bits[index + Width] = color;
		}

		public void Dispose() {
			if (!Disposed) {
				Disposed = true;
				Bitmap.Dispose();
				BitsHandle.Free();
			}
		}
	}
}
