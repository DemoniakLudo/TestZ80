using System.IO;

namespace TestZ80 {
	class ImgDSK {
		public const int MAX_DSK = 2;       // Nbre de fichiers DSK gérés
		const int MAX_TRACKS = 99;      // Nbre maxi de pistes/DSK
		public string NomFic;
		public bool ImageOk;
		public CPCEMUEnt Infos = new CPCEMUEnt();
		public CPCEMUTrack[][] Tracks;
		public byte FlagWrite;
		public byte[][][] Data;

		public ImgDSK() {
			Tracks = new CPCEMUTrack[MAX_TRACKS][];
			Data = new byte[MAX_TRACKS][][];
			for (int t = 0; t < MAX_TRACKS; t++) {
				Tracks[t] = new CPCEMUTrack[2];
				Tracks[t][0] = new CPCEMUTrack();
				Tracks[t][1] = new CPCEMUTrack();
				Data[t] = new byte[2][];
				Data[t][0] = new byte[0x1800];
				Data[t][1] = new byte[0x1800];
			}
		}

		public void Load(string fileName) {
			NomFic = fileName;
			BinaryReader br = new BinaryReader(new FileStream(fileName, FileMode.Open));
			Infos.id = System.Text.Encoding.UTF8.GetString(br.ReadBytes(0x30));
			Infos.NbTracks = br.ReadByte();
			Infos.NbHeads = br.ReadByte();
			Infos.TrackSize = br.ReadInt16();
			Infos.TrackSizeTable = br.ReadBytes(204);
			for (int t = 0; t < Infos.NbTracks; t++)
				for (int h = 0; h < Infos.NbHeads; h++) {
					CPCEMUTrack tr = Tracks[t][h];
					tr.ID = System.Text.Encoding.UTF8.GetString(br.ReadBytes(0x10));
					if (tr.ID.Length >= 10 && tr.ID.Substring(0, 10) == "Track-Info") {
						tr.Track = br.ReadByte();
						tr.Head = br.ReadByte();
						tr.Unused = br.ReadInt16();
						tr.SectSize = br.ReadByte();
						tr.NbSect = br.ReadByte();
						tr.Gap3 = br.ReadByte();
						tr.OctRemp = br.ReadByte();
						// Si une seule face, alors faire face2=face1 ?
						if (tr.Head == 0 && Infos.NbHeads == 1) {
							Tracks[t][1] = Tracks[t][0];
							Data[t][1] = Data[t][0];
						}
						int tailleData = 0;
						for (int s = 0; s < CPCEMUTrack.MAX_SECTS; s++) {
							CPCEMUSect sect = tr.Sect[s];
							sect.C = br.ReadByte();
							sect.H = br.ReadByte();
							sect.R = br.ReadByte();
							sect.N = br.ReadByte();
							sect.ST1 = br.ReadByte();
							sect.ST2 = br.ReadByte();
							sect.SectSize = br.ReadInt16();
							if (s < tr.NbSect) {
								int n = sect.SectSize;
								if ((n & 0xFF) > 0)
									n = (n + 0xFF) & 0x1F00;
								else
									if (n == 0) {
										n = sect.N;
										if (n < 6)
											n = 128 << n;
										else
											n = 0x1800;
									}
								tailleData += n > 0x100 ? n : 0x100;
							}
						}
						Data[t][h] = br.ReadBytes(tailleData);
					}
					else
						break;
				}
			br.Close();
			ImageOk = true;
		}
	}
}
