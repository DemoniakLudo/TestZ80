using System;

namespace TestZ80 {
	static class CRTC {
		const int TAILLE_X_ECR = 384;
		const int TAILLE_Y_ECR = 272;
		const int TAILLE_Y_CRTC = 312;     // Le crtc génère 312 lignes verticales
		const int TAILLE_VBL = 26; // VBL = 26 lignes
		static int x, y, LigneCRTC, MaCRTC, MaHi;
		static int HDelay, VDelay, VswCount;
		static bool HDT, VDT, HS, MR, VT;
		static bool OldVSync;
		static byte VCharCount, VHCount;
		static public byte HCC;
		static public byte VCC;
		static public byte VLC;
		static public byte VtAdj;
		static public byte TailleVBL;
		static public byte TailleHBL;
		static public byte SyncCount;
		static int[] RegsCRTC = new int[32];
		static int RegCRTCSel;
		static public byte CntHSync = 0;
		static public int VSync;
		static public int LastMode;

		static public int Read(int adr) {
			adr &= 0x300;
			if (adr == 0x300)
				return ((RegCRTCSel >= -10) ? RegsCRTC[RegCRTCSel] : 0);

			if (adr == 0x200)
				return (VSync << 5);       // Reg. Status sur Crtc Type 1 seulement ???

			return (0xFF);
		}

		static public void Write(int adr, int val) {
			adr &= 0x300;
			if (adr == 0) {
				RegCRTCSel = val & 0x1F;
			}
			else
				if (adr == 0x100) {
					switch (RegCRTCSel) {
						case 4:
						case 6:
						case 7:
						case 10:
							val &= 0x7F;
							break;

						case 5:
						case 9:
						case 11:
							val &= 0x1F;
							break;

						case 12:
							val &= 0x3F;
							MaHi = ((val << 8) & 0x0F00) | ((val << 9) & 0x6000);
							break;

						case 14:
							val &= 0x3F;
							break;
					}
					RegsCRTC[RegCRTCSel] = val;
				}
		}

		// Exécution de "cycles" crtc (1 cycle = 1µs)
		// Retourne un indicateur disant si il faut rafraichir l'écran (une "frame" terminée)
		static public bool CycleCRTC(int NbCycles) {
			bool CalcMa = false, DoResync = false;
			for (; NbCycles-- > 0; ) {
				VCharCount++;
				if (HCC++ >= RegsCRTC[0]) {
					HCC = 0;
					VLC = (byte)((VLC + 1) & 0x1F);
					if (VSync != 0 && ++TailleVBL >= ((RegsCRTC[3] & 0xF0) != 0 ? RegsCRTC[3] >> 4 : 16)) {
						OldVSync = true;
						VSync = 0;
					}
					if (MR) {
						MR = false;
						VLC = 0;
						if (!HDT)
							MaCRTC += RegsCRTC[1];

						VCC = (byte)((VCC + 1) & 0x7F);
					}
					if (VtAdj != 0)
						CalcMa = --VtAdj == 0;

					if (VT) {
						VT = false;
						if ((RegsCRTC[5] & 0x1F) != 0)
							VtAdj = (byte)(RegsCRTC[5] & 0x1F); // ####
						else
							CalcMa = true;
						//VtAdj = RegsCRTC[ 5 ] & 0x1F;
						//CalcMa = ! VtAdj;
					}
					if (CalcMa) {
						CalcMa = false;
						OldVSync = false;
						VLC = VCC = 0;
						VDT = true;
						MaCRTC = RegsCRTC[13] + MaHi;
					}
					if (VLC == RegsCRTC[9]) {
						MR = true;
						if (VtAdj == 0 && VCC == RegsCRTC[4])
							VT = true;
					}
					if (VCC == RegsCRTC[6])
						VDT = false;

					if (VCC == RegsCRTC[7] && VSync == 0 && !OldVSync) {
						TailleVBL = 0;
						SyncCount = 2;
						VSync = 1;
						VDelay = 2;
						VswCount = 4;
					}
					HDT = true;
				}
				if (HCC == RegsCRTC[1])
					HDT = false;

				if (HS) {
					if (HDelay == 2) {
						if (--VHCount == 0) {
							LigneCRTC++;
							VCharCount = 0;
							HDelay++;
						}
					}
					else
						HDelay++;
				}
				if (TailleHBL != 0 && --TailleHBL == 0) {
					HS = false;
					if (VDelay != 0)
						VDelay--;

					if (VDelay == 0 && VswCount != 0 && --VswCount == 0) {
						LigneCRTC = 0;
						DoResync = true; // ####
					}
					CntHSync++;
					if ((SyncCount != 0 && --SyncCount == 0) || CntHSync == 52) {
						Z80.IRQ = CntHSync & 32;    // Si 52 lignes comptée -> Génération d'une interruption
						CntHSync = 0;
					}
					LastMode = VGA.MemoMode; // Prise en compte nouveau mode écran
				}
				if (HCC == RegsCRTC[2]) {
					HDelay = 0;
					HS = true;
					TailleHBL = (byte)((RegsCRTC[3] & 0x0F) != 0 ? RegsCRTC[3] & 0x0F : 16);
					VHCount = (byte)Math.Max(0, Math.Min((TailleHBL - 2), 4));
				}
				if (x < TAILLE_X_ECR && LigneCRTC >= TAILLE_VBL) {
					BitmapCpc.TraceMot(x, y - 1, HDT && VDT ? ((((HCC + MaCRTC) << 1) + (((HCC + MaCRTC) & 0x1000) << 2)) & 0xC7FF) | (VLC << 11) : -1);
					x += 8;
				}
				else {
					VGA.SyncColor();
					if (VCharCount == 7) {
						if (y < TAILLE_Y_ECR) {
							x = 0;
							y++;
						}
						else
							if (LigneCRTC == TAILLE_VBL)
								y = 0;
					}
					if (LigneCRTC == TAILLE_Y_CRTC) {
						LigneCRTC = 0;
						DoResync = true;
					}
				}
			}
			return (DoResync);
		}

		static public void Init() {
			Reset();
		}

		static public void Reset() {
			x = y = LigneCRTC = 0;
			MaCRTC = MaHi = 0;
			SyncCount = VCC = VLC = 0;
			TailleVBL = TailleHBL = 16;
			HDelay = VDelay = VswCount = 0;
			HDT = VDT = HS = MR = VT = false;
			VtAdj = 0;
			OldVSync = false;
			VCharCount = VHCount = 0;
			HCC = 0;
		}
	}
}
