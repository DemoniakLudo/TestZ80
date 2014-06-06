using System.IO;
using System.Runtime.InteropServices;

namespace TestZ80 {
	class Snapshot {
		static private byte[] tmpRam;

		static public void Load(string fileName) {
			BinaryReader br = new BinaryReader(new FileStream(fileName, FileMode.Open));
			string id = System.Text.Encoding.UTF8.GetString(br.ReadBytes(0x10));
			byte Version = br.ReadByte();
			if (id.Substring(0, 8) == "MV - SNA") {
				Z80.AF.Word = br.ReadUInt16();
				Z80.BC.Word = br.ReadUInt16();
				Z80.DE.Word = br.ReadUInt16();
				Z80.HL.Word = br.ReadUInt16();
				Z80.IR.Word = br.ReadUInt16();
				Z80.IFF1 = br.ReadByte();
				Z80.IFF2 = br.ReadByte();
				Z80.IX.Word = br.ReadUInt16();
				Z80.IY.Word = br.ReadUInt16();
				Z80.SP.Word = br.ReadUInt16();
				Z80.PC.Word = br.ReadUInt16();
				Z80.InterruptMode = br.ReadByte();
				Z80._AF.Word = br.ReadUInt16();
				Z80._BC.Word = br.ReadUInt16();
				Z80._DE.Word = br.ReadUInt16();
				Z80._HL.Word = br.ReadUInt16();
				byte InkReg = br.ReadByte();
				for (int i = 0; i < 17; i++) {
					byte val = br.ReadByte();
					VGA.Write(i);
					VGA.Write(0x40 | (val & 0x1F));
					VGA.SyncColor();
				}
				VGA.Write(InkReg);
				VGA.Write(0x80 | br.ReadByte());
				VGA.Write(0xC0 | (br.ReadByte() & 7));
				byte CrtcIndex = br.ReadByte();
				for (int i = 0; i < 18; i++) {
					byte val = br.ReadByte();
					CRTC.Write(0, i);
					CRTC.Write(0x100, val);
				}
				CRTC.Write(0, CrtcIndex);
				VGA.NumRomExt = br.ReadByte();
				PPI.RegsPPI = br.ReadBytes(4);
				byte PsgIndex = br.ReadByte();
				byte[] PsgData = br.ReadBytes(16);
				ushort MemSize = br.ReadUInt16();
				byte CpcType = br.ReadByte();
				byte LastInt = br.ReadByte();
				byte[] ScrMode = br.ReadBytes(6);
				byte[] DrvAName = br.ReadBytes(13);
				byte[] DrvBName = br.ReadBytes(13);
				byte[] CartName = br.ReadBytes(13);
				UPD.Moteur = br.ReadByte();
				UPD.CurrTrack = br.ReadBytes(4);
				byte PrinterData = br.ReadByte();
				byte EnvStep = br.ReadByte();
				byte EnvDir = br.ReadByte();
				byte CrtcType = br.ReadByte();
				int Unused3 = br.ReadInt32();
				byte HCC = br.ReadByte();
				byte Unused4 = br.ReadByte();
				CRTC.VCC = br.ReadByte();
				CRTC.VLC = br.ReadByte();
				CRTC.VtAdj = br.ReadByte();
				CRTC.TailleHBL = br.ReadByte();
				CRTC.TailleVBL = br.ReadByte();
				ushort CrtcFlag = br.ReadUInt16();
				CRTC.SyncCount = br.ReadByte();
				CRTC.CntHSync = br.ReadByte();
				Z80.IRQ = br.ReadByte();
				byte[] Unused5 = br.ReadBytes(0x4B);
				tmpRam = br.ReadBytes(1024 * MemSize);
				System.Buffer.BlockCopy(tmpRam, 0, VGA.ram, 0, tmpRam.Length);
			}
			br.Close();
		}

		static public void Save() {
		}
	}
}
