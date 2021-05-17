using System.Windows.Forms;

namespace TestZ80 {
	class Keyboard {
		private const int WM_KEYDOWN = 0x0100;
		private const int WM_KEYUP = 0x0101;
		private const int WM_SYSKEYDOWN = 0x0104;
		private const int WM_SYSKEYUP = 0x0105;

		static private int Shift = 0;
		static private bool ClavFree = true;

		private struct stToucheClav {
			public byte Port;
			public byte Ligne;
			public stToucheClav(byte p, byte l) {
				Port = p;
				Ligne = l;
			}
		}

		static private stToucheClav[] TrKey = {
				new stToucheClav( 0xFF, 0x0F ),           // 0x00
				new stToucheClav( 0xFF, 0x0F ),           // 0x01
				new stToucheClav( 0xFF, 0x0F ),           // 0x02
				new stToucheClav( 0xFF, 0x0F ),           // 0x03
				new stToucheClav( 0xFF, 0x0F ),           // 0x04
				new stToucheClav( 0xFF, 0x0F ),           // 0x05
				new stToucheClav( 0xFF, 0x0F ),           // 0x06
				new stToucheClav( 0xFF, 0x0F ),           // 0x07
				new stToucheClav( 0x7F, 0x09 ),           // DEL
				new stToucheClav( 0xEF, 0x08 ),           // TAB
				new stToucheClav( 0xFF, 0x0F ),           // 0x0A
				new stToucheClav( 0xFF, 0x0F ),           // 0x0B
				new stToucheClav( 0xFF, 0x0F ),           // 0x0C
				new stToucheClav( 0xFB, 0x02 ),           // ENTER
				new stToucheClav( 0xFF, 0x0F ),           // 0x0E
				new stToucheClav( 0xFF, 0x0F ),           // 0x0F
				new stToucheClav( 0xDF, 0x02 ),           // SHIFT
				new stToucheClav( 0x7F, 0x02 ),           // CTRL
				new stToucheClav( 0xFD, 0x01 ),           // ALTGR -> COPY
				new stToucheClav( 0xFF, 0x0F ),           // 0x13
				new stToucheClav( 0xBF, 0x08 ),           // CAPS LOCK
				new stToucheClav( 0xFF, 0x0F ),           // 0x15
				new stToucheClav( 0xFF, 0x0F ),           // 0x16
				new stToucheClav( 0xFF, 0x0F ),           // 0x17
				new stToucheClav( 0xFF, 0x0F ),           // 0x18
				new stToucheClav( 0xFF, 0x0F ),           // 0x19
				new stToucheClav( 0xFF, 0x0F ),           // 0x1A
				new stToucheClav( 0xFB, 0x08 ),           // ESC
				new stToucheClav( 0xFF, 0x0F ),           // 0x1C
				new stToucheClav( 0xFF, 0x0F ),           // 0x1D
				new stToucheClav( 0xFF, 0x0F ),           // 0x1E
				new stToucheClav( 0xFF, 0x0F ),           // 0x1F
				new stToucheClav( 0x7F, 0x05 ),           // ' '
				new stToucheClav( 0xFF, 0x0F ),           // 0x21
				new stToucheClav( 0xFF, 0x0F ),           // 0x22
				new stToucheClav( 0xFF, 0x0F ),           // 0x23
				new stToucheClav( 0xFF, 0x0F ),           // 0x24
				new stToucheClav( 0xFE, 0x01 ),           // Fleche Gauche
				new stToucheClav( 0xFE, 0x00 ),           // Fleche Haut
				new stToucheClav( 0xFD, 0x00 ),           // Fleche Droite
				new stToucheClav( 0xFB, 0x00 ),           // Fleche Bas
				new stToucheClav( 0xFF, 0x0F ),           // 0x29
				new stToucheClav( 0xFF, 0x0F ),           // 0x2A
				new stToucheClav( 0xFF, 0x0F ),           // 0x2B
				new stToucheClav( 0xFF, 0x0F ),           // 0x2C
				new stToucheClav( 0xFF, 0x0F ),           // 0x2D
				new stToucheClav( 0xFE, 0x02 ),           // SUPPR
				new stToucheClav( 0xFF, 0x0F ),           // 0x2F
				new stToucheClav( 0xFE, 0x04 ),           // '0'
				new stToucheClav( 0xFE, 0x08 ),           // '1'
				new stToucheClav( 0xFD, 0x08 ),           // '2'
				new stToucheClav( 0xFD, 0x07 ),           // '3'
				new stToucheClav( 0xFE, 0x07 ),           // '4'
				new stToucheClav( 0xFD, 0x06 ),           // '5'
				new stToucheClav( 0xFE, 0x06 ),           // '6'
				new stToucheClav( 0xFD, 0x05 ),           // '7'
				new stToucheClav( 0xFE, 0x05 ),           // '8'
				new stToucheClav( 0xFD, 0x04 ),           // '9'
				new stToucheClav( 0xFF, 0x0F ),           // 0x3A
				new stToucheClav( 0xFF, 0x0F ),           // 0x3B
				new stToucheClav( 0xFF, 0x0F ),           // 0x3C
				new stToucheClav( 0xFF, 0x0F ),           // 0x3D
				new stToucheClav( 0xFF, 0x0F ),           // 0x3E
				new stToucheClav( 0xFF, 0x0F ),           // 0x3F
				new stToucheClav( 0xFF, 0x0F ),           // 0x40
				new stToucheClav( 0xF7, 0x08 ),           // 'A'
				new stToucheClav( 0xBF, 0x06 ),           // 'B'
				new stToucheClav( 0xBF, 0x07 ),           // 'C'
				new stToucheClav( 0xDF, 0x07 ),           // 'D'
				new stToucheClav( 0xFB, 0x07 ),           // 'E'
				new stToucheClav( 0xDF, 0x06 ),           // 'F'
				new stToucheClav( 0xEF, 0x06 ),           // 'G'
				new stToucheClav( 0xEF, 0x05 ),           // 'H'
				new stToucheClav( 0xF7, 0x04 ),           // 'I'
				new stToucheClav( 0xDF, 0x05 ),           // 'J'
				new stToucheClav( 0xDF, 0x04 ),           // 'K'
				new stToucheClav( 0xEF, 0x04 ),           // 'L'
				new stToucheClav( 0xDF, 0x03 ),           // 'M'
				new stToucheClav( 0xBF, 0x05 ),           // 'N'
				new stToucheClav( 0xFB, 0x04 ),           // 'O'
				new stToucheClav( 0xF7, 0x03 ),           // 'P'
				new stToucheClav( 0xDF, 0x08 ),           // 'Q'
				new stToucheClav( 0xFB, 0x06 ),           // 'R'
				new stToucheClav( 0xEF, 0x07 ),           // 'S'
				new stToucheClav( 0xF7, 0x06 ),           // 'T'
				new stToucheClav( 0xFB, 0x05 ),           // 'U'
				new stToucheClav( 0x7F, 0x06 ),           // 'V'
				new stToucheClav( 0x7F, 0x08 ),           // 'W'
				new stToucheClav( 0x7F, 0x07 ),           // 'X'
				new stToucheClav( 0xF7, 0x05 ),           // 'Y'
				new stToucheClav( 0xF7, 0x07 ),           // 'Z'
				new stToucheClav( 0xFF, 0x0F ),           // 0x5B
				new stToucheClav( 0xFF, 0x0F ),           // 0x5C
				new stToucheClav( 0xFF, 0x0F ),           // menu contextuel
				new stToucheClav( 0xFF, 0x0F ),           // 0x5E
				new stToucheClav( 0xFF, 0x0F ),           // 0x5F
				new stToucheClav( 0x7F, 0x01 ),           // P'0'
				new stToucheClav( 0xDF, 0x01 ),           // P'1'
				new stToucheClav( 0xBF, 0x01 ),           // P'2'
				new stToucheClav( 0xDF, 0x00 ),           // P'3'
				new stToucheClav( 0xEF, 0x02 ),           // P'4'
				new stToucheClav( 0xEF, 0x01 ),           // P'5'
				new stToucheClav( 0xEF, 0x00 ),           // P'6'
				new stToucheClav( 0xFB, 0x01 ),           // P'7'
				new stToucheClav( 0xF7, 0x01 ),           // P'8'
				new stToucheClav( 0xF7, 0x00 ),           // P'9'
				new stToucheClav( 0xFF, 0x0F ),           // 0x6A
				new stToucheClav( 0xFF, 0x0F ),           // 0x6B
				new stToucheClav( 0xFF, 0x0F ),           // 0x6C
				new stToucheClav( 0xFF, 0x0F ),           // 0x6D
				new stToucheClav( 0x7F, 0x00 ),           // P'.'
				new stToucheClav( 0xFF, 0x0F ),           // 0x6F
				new stToucheClav( 0xFF, 0x0F ),           // 0x70
				new stToucheClav( 0xFF, 0x0F ),           // 0x71
				new stToucheClav( 0xFF, 0x0F ),           // 0x72
				new stToucheClav( 0xFF, 0x0F ),           // 0x73
				new stToucheClav( 0xFF, 0x0F ),           // 0x74
				new stToucheClav( 0xFF, 0x0F ),           // 0x75
				new stToucheClav( 0xFF, 0x0F ),           // 0x76
				new stToucheClav( 0xFF, 0x0F ),           // 0x77
				new stToucheClav( 0xFF, 0x0F ),           // 0x78
				new stToucheClav( 0xFF, 0x0F ),           // 0x79
				new stToucheClav( 0xFF, 0x0F ),           // 0x7A
				new stToucheClav( 0xFF, 0x0F ),           // 0x7B
				new stToucheClav( 0xFF, 0x0F ),           // 0x7C
				new stToucheClav( 0xFF, 0x0F ),           // 0x7D
				new stToucheClav( 0xFF, 0x0F ),           // 0x7E
				new stToucheClav( 0xFF, 0x0F ),           // 0x7F
				new stToucheClav( 0xFF, 0x0F ),           // 0x80
				new stToucheClav( 0xFF, 0x0F ),           // 0x81
				new stToucheClav( 0xFF, 0x0F ),           // 0x82
				new stToucheClav( 0xFF, 0x0F ),           // 0x83
				new stToucheClav( 0xFF, 0x0F ),           // 0x84
				new stToucheClav( 0xFF, 0x0F ),           // 0x85
				new stToucheClav( 0xFF, 0x0F ),           // 0x86
				new stToucheClav( 0xFF, 0x0F ),           // 0x87
				new stToucheClav( 0xFF, 0x0F ),           // 0x88
				new stToucheClav( 0xFF, 0x0F ),           // 0x89
				new stToucheClav( 0xFF, 0x0F ),           // 0x8A
				new stToucheClav( 0xFF, 0x0F ),           // 0x8B
				new stToucheClav( 0xFF, 0x0F ),           // 0x8C
				new stToucheClav( 0xBF, 0x00 ),           // 0x8D   -> Emulation touche "return"
				new stToucheClav( 0xFF, 0x0F ),           // 0x8E
				new stToucheClav( 0xFF, 0x0F ),           // 0x8F
				new stToucheClav( 0xFF, 0x0F ),           // 0x90
				new stToucheClav( 0xFF, 0x0F ),           // 0x91
				new stToucheClav( 0xFF, 0x0F ),           // 0x92
				new stToucheClav( 0xFF, 0x0F ),           // 0x93
				new stToucheClav( 0xFF, 0x0F ),           // 0x94
				new stToucheClav( 0xFF, 0x0F ),           // 0x95
				new stToucheClav( 0xFF, 0x0F ),           // 0x96
				new stToucheClav( 0xFF, 0x0F ),           // 0x97
				new stToucheClav( 0xFF, 0x0F ),           // 0x98
				new stToucheClav( 0xFF, 0x0F ),           // 0x99
				new stToucheClav( 0xFF, 0x0F ),           // 0x9A
				new stToucheClav( 0xFF, 0x0F ),           // 0x9B
				new stToucheClav( 0xFF, 0x0F ),           // 0x9C
				new stToucheClav( 0xFF, 0x0F ),           // 0x9D
				new stToucheClav( 0xFF, 0x0F ),           // 0x9E
				new stToucheClav( 0xFF, 0x0F ),           // 0x9F
				new stToucheClav( 0xFF, 0x0F ),           // 0xA0
				new stToucheClav( 0xFF, 0x0F ),           // 0xA1
				new stToucheClav( 0xFF, 0x0F ),           // 0xA2
				new stToucheClav( 0xFF, 0x0F ),           // 0xA3
				new stToucheClav( 0xFF, 0x0F ),           // 0xA4
				new stToucheClav( 0xFF, 0x0F ),           // 0xA5
				new stToucheClav( 0xFF, 0x0F ),           // 0xA6
				new stToucheClav( 0xFF, 0x0F ),           // 0xA7
				new stToucheClav( 0xFF, 0x0F ),           // 0xA8
				new stToucheClav( 0xFF, 0x0F ),           // 0xA9
				new stToucheClav( 0xFF, 0x0F ),           // 0xAA
				new stToucheClav( 0xFF, 0x0F ),           // 0xAB
				new stToucheClav( 0xFF, 0x0F ),           // 0xAC
				new stToucheClav( 0xFF, 0x0F ),           // 0xAD
				new stToucheClav( 0xFF, 0x0F ),           // 0xAE
				new stToucheClav( 0xFF, 0x0F ),           // 0xAF
				new stToucheClav( 0xFF, 0x0F ),           // 0xB0
				new stToucheClav( 0xFF, 0x0F ),           // 0xB1
				new stToucheClav( 0xFF, 0x0F ),           // 0xB2
				new stToucheClav( 0xFF, 0x0F ),           // 0xB3
				new stToucheClav( 0xFF, 0x0F ),           // 0xB4
				new stToucheClav( 0xFF, 0x0F ),           // 0xB5
				new stToucheClav( 0xFF, 0x0F ),           // 0xB6
				new stToucheClav( 0xFF, 0x0F ),           // 0xB7
				new stToucheClav( 0xFF, 0x0F ),           // 0xB8
				new stToucheClav( 0xFF, 0x0F ),           // 0xB9
				new stToucheClav( 0xBF, 0x02 ),           // '$'        En fct du type de clavier
				new stToucheClav( 0xFE, 0x03 ),           // '='
				new stToucheClav( 0xBF, 0x04 ),           // ','
				new stToucheClav( 0xFF, 0x0F ),           // 0xBD
				new stToucheClav( 0x7F, 0x04 ),           // ';'
				new stToucheClav( 0x7F, 0x03 ),           // ':'
				new stToucheClav( 0xEF, 0x03 ),           // 'œ'
				new stToucheClav( 0xFF, 0x0F ),           // 0xC1
				new stToucheClav( 0xFF, 0x0F ),           // 0xC2
				new stToucheClav( 0xFF, 0x0F ),           // 0xC3
				new stToucheClav( 0xFF, 0x0F ),           // 0xC4
				new stToucheClav( 0xFF, 0x0F ),           // 0xC5
				new stToucheClav( 0xFF, 0x0F ),           // 0xC6
				new stToucheClav( 0xFF, 0x0F ),           // 0xC7
				new stToucheClav( 0xFF, 0x0F ),           // 0xC8
				new stToucheClav( 0xFF, 0x0F ),           // 0xC9
				new stToucheClav( 0xFF, 0x0F ),           // 0xCA
				new stToucheClav( 0xFF, 0x0F ),           // 0xCB
				new stToucheClav( 0xFF, 0x0F ),           // 0xCC
				new stToucheClav( 0xFF, 0x0F ),           // 0xCD
				new stToucheClav( 0xFF, 0x0F ),           // 0xCE
				new stToucheClav( 0xFF, 0x0F ),           // 0xCF
				new stToucheClav( 0xFF, 0x0F ),           // 0xD0
				new stToucheClav( 0xFF, 0x0F ),           // 0xD1
				new stToucheClav( 0xFF, 0x0F ),           // 0xD2
				new stToucheClav( 0xFF, 0x0F ),           // 0xD3
				new stToucheClav( 0xFF, 0x0F ),           // 0xD4
				new stToucheClav( 0xFF, 0x0F ),           // 0xD5
				new stToucheClav( 0xFF, 0x0F ),           // 0xD6
				new stToucheClav( 0xFF, 0x0F ),           // 0xD7
				new stToucheClav( 0xFF, 0x0F ),           // 0xD8
				new stToucheClav( 0xFF, 0x0F ),           // 0xD9
				new stToucheClav( 0xFF, 0x0F ),           // 0xDA
				new stToucheClav( 0xFD, 0x03 ),           // ')'
				new stToucheClav( 0xFD, 0x02 ),           // '#'        En fct du type de clavier
				new stToucheClav( 0xFB, 0x03 ),           // '^'        En fct du type de clavier
				new stToucheClav( 0xF7, 0x02 ),           // '*'        En fct du type de clavier
				new stToucheClav( 0xBF, 0x03 ),           // '!'
				new stToucheClav( 0xFF, 0x0F ),           // 0xE0
				new stToucheClav( 0xFF, 0x0F ),           // 0xE1
				new stToucheClav( 0xF7, 0x02 ),           // '*'        En fct du type de clavier
				new stToucheClav( 0xFF, 0x0F ),           // 0xE3
				new stToucheClav( 0xFF, 0x0F ),           // 0xE4
				new stToucheClav( 0xFF, 0x0F ),           // 0xE5
				new stToucheClav( 0xFF, 0x0F ),           // 0xE6
				new stToucheClav( 0xFF, 0x0F ),           // 0xE7
				new stToucheClav( 0xFF, 0x0F ),           // 0xE8
				new stToucheClav( 0xFF, 0x0F ),           // 0xE9
				new stToucheClav( 0xFF, 0x0F ),           // 0xEA
				new stToucheClav( 0xFF, 0x0F ),           // 0xEB
				new stToucheClav( 0xFF, 0x0F ),           // 0xEC
				new stToucheClav( 0xFF, 0x0F ),           // 0xED
				new stToucheClav( 0xFF, 0x0F ),           // 0xEE
				new stToucheClav( 0xFF, 0x0F ),           // 0xEF
				new stToucheClav( 0xFF, 0x0F ),           // 0xF0
				new stToucheClav( 0xFF, 0x0F ),           // 0xF1
				new stToucheClav( 0xFF, 0x0F ),           // 0xF2
				new stToucheClav( 0xFF, 0x0F ),           // 0xF3
				new stToucheClav( 0xFF, 0x0F ),           // 0xF4
				new stToucheClav( 0xFF, 0x0F ),           // 0xF5
				new stToucheClav( 0xFF, 0x0F ),           // 0xF6
				new stToucheClav( 0xFF, 0x0F ),           // 0xF7
				new stToucheClav( 0xFF, 0x0F ),           // 0xF8
				new stToucheClav( 0xFF, 0x0F ),           // 0xF9
				new stToucheClav( 0xFF, 0x0F ),           // 0xFA
				new stToucheClav( 0xFF, 0x0F ),           // 0xFB
				new stToucheClav( 0xFF, 0x0F ),           // 0xFC
				new stToucheClav( 0xFF, 0x0F ),           // 0xFD
				new stToucheClav( 0xFF, 0x0F ),           // 0xFE
				new stToucheClav( 0xFF, 0x0F )            // 0xFF
			};


		static public void ProcessKey(Message m) {
			int wParam = (int)m.WParam;
			switch (m.Msg) {
				case WM_KEYUP:
				case WM_SYSKEYUP:
					if (ClavFree) {
						if (wParam == 0x10)
							Shift = 0;

						if (wParam == 0x0D && ((long)m.LParam & 0x01000000) != 0)
							wParam = 0x8D;

						PPI.clav[TrKey[wParam].Ligne] |= ~TrKey[wParam].Port;
						if (TouchesFonctions(Shift, wParam) != 0) {
							PPI.clav[TrKey[0x10].Ligne] |= ~TrKey[0x10].Port;
							Shift = 0;
						}
					}
					break;

				case WM_KEYDOWN:
				case WM_SYSKEYDOWN:
					if (ClavFree) {
						if (wParam == 0x10)
							Shift = 1;

						if (wParam == 0x0D && ((long)m.LParam & 0x01000000) != 0)
							wParam = 0x8D;

						PPI.clav[TrKey[wParam].Ligne] &= TrKey[wParam].Port;
					}
					break;
			}
		}

		static int TouchesFonctions(int Shift, int wParam) {
			switch (wParam) {
				/* 
					Touche "Menu contextuel"
				*/
				//case 0x5D:
				//	ShowMenu ^= 1;
				//	SetMenu(hWnd, ShowMenu ? menu : NULL);
				//	SetScreenSize();
				//	break;

				//case 0x70:
				//	if (Shift)
				//		BDDPhenix();        // SHIFT+F1
				//	else {
				//		DWORD ThreadID;
				//		CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Aide, 0, 0, &ThreadID);
				//	}
				//	break;

				//case 0x71:
				//	if (Shift)
				//		ReadWritePC();          // SHIFT+F2
				//	else
				//		SetTurbo();             // F2
				//	break;

				//case 0x72:
				//	if (Shift)
				//		SetWinSize();           // SHIFT+F3
				//	else
				//		SetMultiface();         // F3
				//	break;

				case 0x73:
					if (Shift == 0)
						SnaRead();              // F4
					//	else
					//		SetMonoColor();         // SHIFT+F4
					break;

				//case 0x74:
				//	if (Shift)
				//		SaveYM();               // SHIFT+F5
				//	else
				//		SnaWrite();             // F5
				//	break;

				//case 0x75:
				//	if (Shift) {
				//		ShowWindow(hWnd, SW_MINIMIZE);
				//		return (1);
				//	}
				//	else
				//		Debugger();             // F6
				//	break;

				case 0x76:
					SetNewDsk(Shift);         // F7 ou SHIFT+F7
					break;

				case 0x77:
					if (Shift == 0)
						OptReset();             // F8
					//else
					//	SetRomPack();           // SHIFT+F8
					break;

				//case 0x78:
				//	if (Shift)
				//		TapeBrowse();           // SHIFT+F9
				//	else
				//		TapeRead();             // F9
				//	break;

				//case 0x79:
				//	finMain = 1;
				//	break;

				//case 0x7A:
				//	if (Shift)
				//		SaveAVI();              // SHIFT+F11
				//	else
				//		TapeWrite();            // F11
				//	break;

				case 0x7B:
					SaveDsk(Shift);

				//	if (Shift)
				//		Assemble();             // SHIFT + F12
				//	else
				//		SaveBMP();              // F12
				//	return (1);
					break;
			}
			return (0);
		}

		static private void SnaRead() {
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Filter = "Fichiers SNA (*.sna)|*.sna";
			DialogResult result = dlg.ShowDialog();
			if (result == DialogResult.OK)
				Snapshot.Load(dlg.FileName);
		}

		static public void OptReset() {
			Z80.Reset();
			CRTC.Reset();
			PPI.Reset();
			UPD.Reset();
			VGA.Reset();
		}

		static private void SetNewDsk(int drive) {
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Filter = "Fichiers DSK (*.dsk)|*.dsk";
			DialogResult result = dlg.ShowDialog();
			if (result == DialogResult.OK)
				UPD.Dsk[drive].Load(dlg.FileName);
		}

		static private void SaveDsk(int drive) {
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.Filter = "Fichiers DSK (*.dsk)|*.dsk";
			DialogResult result = dlg.ShowDialog();
			if (result == DialogResult.OK)
				UPD.Dsk[drive].Save(dlg.FileName);
		}
	}
}
