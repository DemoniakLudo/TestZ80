﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace TestZ80 {
	public class DirectBitmap : IDisposable {
		public Bitmap Bitmap { get; private set; }
		public uint[] Bits { get; private set; }
		public bool Disposed { get; private set; }
		public int Height { get; private set; }
		public int Width { get; private set; }
		public int Length { get { return Width * Height; } }

		protected GCHandle BitsHandle { get; private set; }

		public DirectBitmap(int width, int height) {
			CreateBitmap(width, height);
		}

		public DirectBitmap(DirectBitmap source) {
			CreateBitmap(source.Width, source.Height);
			CopyBits(source);
		}

		public void CopyBits(DirectBitmap source) {
			Array.Copy(source.Bits, Bits, Bits.Length);
		}

		private void CreateBitmap(int width, int height) {
			Width = width;
			Height = height;
			Bits = new uint[width * height];
			BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
			Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppRgb, BitsHandle.AddrOfPinnedObject());
		}

		public void SetPixel(int x, int y, int c) {
			Bits[x + (y * Width)] = (uint)c | 0xFF000000;
		}

		public int GetPixel(int x, int y) {
			return (int)(Bits[x + (y * Width)] & 0xFFFFFF);
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