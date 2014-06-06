namespace TestZ80 {
	static public class GestPort {
		static public int ReadPort(int port) {
			if ((port & 0x0480) == 0)
				return (UPD.Read(port));

			if ((port & 0x0800) == 0)
				return (PPI.Read(port));

			if ((port & 0x4000) == 0)
				return (CRTC.Read(port));

			return (0xFF);
		}

		static public void WritePort(int port, int val) {
			if ((port & 0xC000) == 0x04000)
				VGA.Write(val);

			if ((port & 0x4000) == 0)
				CRTC.Write(port, val);

			if ((port & 0x2000) == 0)
				VGA.WriteROM(val);

			if ((port & 0x0800) == 0)
				PPI.Write(port, val);

			if ((port & 0x0480) == 0)
				UPD.Write(port, val);
		}
	}
}
