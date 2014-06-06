namespace TestZ80 {
	class UPD {
		const int TPS_MOTOR_ON = 400000;	// temps ready après un motor on (en µs)
		const int TPS_DATA_READ = 34;		// temps entre chaque "read" de datas (en µs)
		const int TPS_OVER_RUN = 68;		// temps entre chaque accès secteur avant overrun (en µs)

		// Sens des commades
		const int SENS_NONE = 0;
		const int SENS_RD = 1;
		const int SENS_WR = 2;

		private delegate int pFctExec(int val, int sens, int tps);

		// Fonction EXECUTE
		static pFctExec FctExec;

		// Position dans la pile
		static int PosStack;

		// Définition des fonctions UPD : de type int fct( int OctetEcris, int sens, int TpsCycle )
		// Si sens == 0, alors exécution depuis Cycle, sinon exécution IO<->Z80
		static pFctExec[] TabCmdUPD = 
				{
				CmdInvalid,            // 0x00
				CmdInvalid,            // 0x01
				CmdReadTrack,          // 0x02 READ TRACK
				CmdSpecify,            // 0x03 SPECIFY
				CmdSenseDriveStatus,   // 0x04 SENSE DRIVE STATUS
				CmdWriteData,          // 0x05 WRITE DATA
				CmdReadData,           // 0x06 READ DATA
				CmdRecalibrate,        // 0x07 RECALIBRATE
				CmdSenseInterStatus,   // 0x08 SENSE INTERRUPT STATUS
				CmdWriteDelData,       // 0x09 WRITE DELETED DATA
				CmdReadID,             // 0x0A READ ID
				CmdInvalid,            // 0x0B
				CmdReadDelData,        // 0x0C READ DELETED DATA
				CmdFormatTrack,        // 0x0D FORMAT TRACK
				CmdInvalid,            // 0x0E
				CmdSeek,               // 0x0F SEEK
				CmdInvalid,            // 0x10
				CmdScanLow,            // 0x11 SCAN LOW
				CmdInvalid,            // 0x12
				CmdInvalid,            // 0x13
				CmdInvalid,            // 0x14
				CmdInvalid,            // 0x15
				CmdInvalid,            // 0x16
				CmdInvalid,            // 0x17
				CmdInvalid,            // 0x18
				CmdScanLowEq,          // 0x19 SCAN LOW OR EQUAL
				CmdInvalid,            // 0x1A
				CmdInvalid,            // 0x1B
				CmdInvalid,            // 0x1C
				CmdScanHighEq,         // 0x1D SCAN HIGH OR EQUAL
				CmdInvalid,            // 0x1E
				CmdInvalid             // 0x1F
				};

		// Status registrer
		static int Status;

		// Bits de Status
		const int STATUS_CB = 0x10;        // Si positionné = UPD Occupé (exécution commande)
		const int STATUS_EXE = 0x20;        // Si positionné = phase EXECUTION
		const int STATUS_DIO = 0x40;        // Si positionné = sens UPD->Z80, sinon Z80->UPD
		const int STATUS_RQM = 0x80;        // Si positionné = UPD Ready

		static int ST0, ST1, ST2, ST3;

		static int UnitSel;                 // Drive sélectionné
		static int HeadSel;                 // Tête sélectionnée

		// ST0
		const int ST0_NR = 0x08;        // Not ready
		const int ST0_EC = 0x10;        // Equipment Check (erreur de seek)
		const int ST0_SE = 0x20;        // Seek end
		const int ST0_IC1 = 0x40;        // Interrupt Code (1)
		const int ST0_IC2 = 0x80;        // Interrupt Code (2)

		// ST1
		const int ST1_MA = 0x01;        // Missing Adress mark
		const int ST1_ND = 0x04;        // No data
		const int ST1_OR = 0x10;        // Over Run
		const int ST1_DE = 0x20;        // Data Error
		const int ST1_EN = 0x80;        // End of cylinder

		// ST2
		const int ST2_MD = 0x01;        // No Data Address Mark
		const int ST2_BC = 0x02;        // Bad cylinder
		const int ST2_WC = 0x10;        // Wrong cylinder
		const int ST2_CRC = 0x20;        // CRC Error
		const int ST2_CM = 0x40;        // Deleted Address mark

		// ST3
		const int ST3_HD = 0x04;        // Head address
		const int ST3_TS = 0x08;        // Double-sided
		const int ST3_T0 = 0x10;        // Tête en Track 0
		const int ST3_RY = 0x20;        // Drive ready

		// Temps
		static int SRT, HUT, HLT;

		static int TpsReady;
		static int TpsNextByte;
		static int TpsOverRun;
		static int CptOverRun;
		static int[] CptRotation = new int[4]; // Compteur rotation pour chaque disquette
		static int TimeMoveTrack;

		// Etat des moteurs
		static public byte Moteur;

		// Position piste
		static public byte[] CurrTrack = new byte[4] { 0, 0, 0, 0 };

		// Position demandée
		static int NewCylinder;
		static int NbSteps;
		static int IndexSecteur = 0;
		static int PosData = 0;
		static int TailleSect = 0;
		static int Weak = 1;
		static CPCEMUTrack[][] CurrTrackDatasDSK = new CPCEMUTrack[ImgDSK.MAX_DSK][];
		static public ImgDSK[] Dsk = new ImgDSK[ImgDSK.MAX_DSK];
		static byte[] BufExtra = new byte[0x100];
		static int posBuf;
		static int Break = 0;
		static int sectC, sectH, sectR, sectN, sectEOT, sectGAP, sectSize;
		static int cntdata = 0, posSect = 0, posExtra = -1;
		static int PosWeak = 0;
		public static bool led = false;

		static void StartExePhase() {
			Status |= STATUS_EXE;
			led = true;
		}

		static void EndExePhase() {
			Status &= ~STATUS_EXE;
			led = false;
		}

		static int CalcST0() {
			Status &= 0xF0;
			ST0 = UnitSel; // drive A ou B ###
			if (Moteur == 0 || UnitSel > 1 || !Dsk[UnitSel].ImageOk)
				ST0 |= ST0_IC1 | ST0_NR;
			else
				ST0 &= ~ST0_IC1 & ~ST0_NR;
			//ST0 &= 0xFC;
			//ST0 |= UnitSel;
			//if ( ! Dsk[ UnitSel ].Image )
			//    ST0 |= ST0_NR;
			//else
			//    ST0 &= ~ST0_NR;

			return (ST0);
		}

		static void CalcTailleSect(CPCEMUSect Sect) {
			TailleSect = Sect.SectSize > 0 ? Sect.SectSize : 128 << Sect.N;
			Weak = (Sect.SectSize / TailleSect) - 1;
			if (TailleSect > 0x1800)
				TailleSect = 0x1800;
		}

		static int NextSecteur(ref int sectC, ref int sectH, ref int sectR, ref int sectN) {
			if (UnitSel < 2 && Dsk[UnitSel].ImageOk) {
				// Calcule la "position" du secteur actuel
				int i, tps = 146 * 32; // 146 octets avant le premier secteur
				for (i = 0; i <= IndexSecteur; i++) {
					tps += 0; // ###
				}
				ST1 &= ~ST1_ND;
				CPCEMUSect Sect = CurrTrackDatasDSK[UnitSel][HeadSel].Sect[IndexSecteur];
				CalcTailleSect(Sect);
				PosData += TailleSect + PosWeak;
				PosWeak += TailleSect;
				if (PosWeak > TailleSect * Weak)
					PosWeak = 0;

				if (++IndexSecteur >= CurrTrackDatasDSK[UnitSel][HeadSel].NbSect) {
					IndexSecteur = 0;
					PosData = 0;
				}
				Sect = CurrTrackDatasDSK[UnitSel][HeadSel].Sect[IndexSecteur];
				sectC = Sect.C;
				sectH = Sect.H;
				sectR = Sect.R;
				sectN = Sect.N;
			}
			else
				sectC = sectH = sectR = sectN = 0xFF;

			return (IndexSecteur);
		}

		static void SetSecteur(int index) {
			int HoleIndex = 0;
			int tmpC = 0, tmpH = 0, tmpR = 0, tmpN = 0;
			if (UnitSel < 2 && Dsk[UnitSel].ImageOk) {
				while (HoleIndex < 2) {
					CPCEMUSect Sect = CurrTrackDatasDSK[UnitSel][HeadSel].Sect[IndexSecteur];
					if (IndexSecteur == index) {
						ST0 &= ~ST0_IC1;
						ST1 = Sect.ST1 & (ST1_DE | ST1_ND | ST1_MA);
						ST2 = Sect.ST2 & (ST2_CM | ST2_CRC | ST2_MD);
						CalcTailleSect(Sect);
						return;
					}
					if (NextSecteur(ref tmpC, ref tmpH, ref tmpR, ref tmpN) == 0)
						HoleIndex++;
				}
			}
			ST0 |= ST0_IC1;
			ST1 |= ST1_ND;
		}

		static void RechercheSecteur(int findC, int findH, int findR, int findN, int Eot) {
			int HoleIndex = 0;
			int tmpC = 0, tmpH = 0, tmpR = 0, tmpN = 0;
			if (UnitSel < 2 && Dsk[UnitSel].ImageOk) {
				while (HoleIndex < 2) {
					CPCEMUSect Sect = CurrTrackDatasDSK[UnitSel][HeadSel].Sect[IndexSecteur];
					if (Sect.R == findR) {
						if (Sect.C == findC) {
							ST2 &= ~ST2_WC;
							ST2 &= ~ST2_BC;
							if (Sect.H == findH && Sect.N == findN) {
								ST0 &= ~ST0_IC1;
								ST1 = Sect.ST1 & (ST1_DE | ST1_ND | ST1_MA);
								ST2 = Sect.ST2 & (ST2_CM | ST2_CRC | ST2_MD);
								CalcTailleSect(Sect);
								if (findR == Eot)
									ST1 |= ST1_EN;

								return;
							}
						}
						else {
							ST2 |= ST2_WC;
							if (Sect.C == 0xFF)
								ST2 |= ST2_BC;

							break;
						}
					}
					if (NextSecteur(ref tmpC, ref tmpH, ref tmpR, ref tmpN) == 0)
						HoleIndex++;
				}
			}
			ST0 |= ST0_IC1;
			ST1 |= ST1_ND;
		}

		static void MoveTrack(int tps, int maxMove) {
			if (tps == 0) {
				TimeMoveTrack = 0;
				NbSteps = maxMove;
				ST0 &= ~ST0_SE & ~ST0_EC;
				ST3 &= ~ST3_RY;
				Status &= ~STATUS_RQM;          // UPD Occupé à déplacer la tête
				StartExePhase();
				TpsNextByte += SRT;
			}
			else
				if (CurrTrack[UnitSel] == NewCylinder || NbSteps == 0) {
					FctExec = null;
					ST0 &= ~ST0_IC1 & ~ST0_IC2;
					ST0 |= ST0_SE;
					if (UnitSel < 2 && Dsk[UnitSel].ImageOk) {
						CurrTrackDatasDSK[UnitSel][0] = Dsk[UnitSel].Tracks[CurrTrack[UnitSel]][0];
						CurrTrackDatasDSK[UnitSel][1] = Dsk[UnitSel].Tracks[CurrTrack[UnitSel]][1];
						ST3 |= ST3_RY;
					}

					if (CurrTrack[UnitSel] == 0)
						ST3 |= ST3_T0;
					else
						ST3 &= ~ST3_T0;

					TpsNextByte = 0;
					IndexSecteur = PosData = 0;
					if (CurrTrack[UnitSel] != NewCylinder)
						ST0 |= ST0_EC;

					Status &= ~STATUS_CB & ~STATUS_DIO; // ## pourquoi mettre le ~STATUS_CB en commentaire ???
					Status |= STATUS_RQM;
					EndExePhase();
				}
				else {
					TpsNextByte += tps;
					TimeMoveTrack += tps;
					if (TimeMoveTrack > SRT) {
						TimeMoveTrack -= SRT;
						if (CurrTrack[UnitSel] > NewCylinder)
							CurrTrack[UnitSel]--;
						else
							CurrTrack[UnitSel]++;

						NbSteps--;
					}
				}
		}

		static int CmdInvalid(int val, int sens, int tps) {
			if (sens == SENS_WR && (Status & STATUS_DIO) == 0)
				Status |= STATUS_DIO; // phase result

			if (sens == SENS_RD && (Status & STATUS_DIO) != 0) {
				FctExec = null;            // Prêt pour une nouvelle commande
				Status &= ~STATUS_DIO & ~STATUS_CB;  // Repositionné en Z80->UPD (phase cmd)
				EndExePhase();
				ST0 = ST0_IC2;  // ### ST0 forcé à 0x80
				return (ST0);
			}
			//Log( MODULENAME, "Commande Invalide appelée.", LOG_INFO );
			return (0xFF);
		}

		//
		// Données inter-secteur (pour CmdReadTrack)
		//
		static int GetExtraData(ref int posExtra, int gap) {
			if (posExtra == -1) { // Initialisation
				posBuf = 0;
				BufExtra[posBuf++] = 0x00; // ### crc secteur
				BufExtra[posBuf++] = 0x00; // ### crc secteur
				for (int i = 0; i < gap; i++)
					BufExtra[posBuf++] = 0x4E;

				for (int i = 0; i < 12; i++)
					BufExtra[posBuf++] = 0;

				BufExtra[posBuf++] = 0xA1;
				BufExtra[posBuf++] = 0xA1;
				BufExtra[posBuf++] = 0xA1;
				BufExtra[posBuf++] = 0xFE;
				BufExtra[posBuf++] = 0x00; // ### C;
				BufExtra[posBuf++] = 0x00; // ### H;
				BufExtra[posBuf++] = 0x00; // ### R;
				BufExtra[posBuf++] = 0x00; // ### N;
				BufExtra[posBuf++] = 0x00; // ### crc id
				BufExtra[posBuf++] = 0x00; // ### crc id
				for (int i = 0; i < 22; i++)
					BufExtra[posBuf++] = 0x4E;

				for (int i = 0; i < 12; i++)
					BufExtra[posBuf++] = 0;

				BufExtra[posBuf++] = 0xA1;
				BufExtra[posBuf++] = 0xA1;
				BufExtra[posBuf++] = 0xA1;
				BufExtra[posBuf++] = 0xFB;
				posExtra = posBuf;
				posBuf = 0;
			}
			posExtra--;
			return (BufExtra[posBuf++]);
		}

		static int CmdReadTrack(int val, int sens, int tps) {
			int ret;

			if (sens == SENS_WR && (Status & STATUS_DIO) == 0) {
				switch (PosStack++) {
					case 1:
						UnitSel = val & 3;
						HeadSel = (val >> 2) & 1;
						ST0 &= ~ST0_IC1 & ~ST0_IC2;
						break;

					case 2:
						sectC = val;
						break;

					case 3:
						sectH = val;
						break;

					case 4:
						sectR = val;
						break;

					case 5:
						sectN = val;
						break;

					case 6:
						sectEOT = val;
						break;

					case 7:
						sectGAP = val;
						break;

					case 8:
						sectSize = val;
						SetSecteur(posSect); // Se positionne sur le premier secteur après le trou d'index"
						cntdata = PosData;
						Status |= STATUS_DIO;
						if ((ST1 & ST1_ND) == 0) {
							StartExePhase();
							TpsOverRun = TPS_OVER_RUN * 8000; //### Valeur arbitraire, temps entre id et données secteurs
							CptOverRun = 1;
							if (sectN == 0)
								TailleSect = sectSize;
						}
						else {
							PosStack++;
							Break = 1; // Ne peut pas passer en EXECUTE
						}
						break;
				}
			}
			if (sens == SENS_RD && (Status & STATUS_DIO) != 0) {
				switch (PosStack++) {
					case 9:
						TpsOverRun = TPS_OVER_RUN;//* 6250; // ##### GRRR
						posExtra = -1;
						ret = Dsk[UnitSel].Data[CurrTrack[UnitSel]][HeadSel][cntdata++];
						if ((Status & STATUS_EXE) != 0) {
							if (--TailleSect > 0) // Tant que fin de secteur pas atteinte
								PosStack--;
						}
						else
							PosStack++;

						return (ret);

					case 10:
						TpsOverRun = TPS_OVER_RUN * 6250;
						ret = GetExtraData(ref posExtra, CurrTrackDatasDSK[UnitSel][HeadSel].Gap3);
						if ((Status & STATUS_EXE) != 0) {
							if (posExtra > 0)
								PosStack--;
							else {
								if (posSect++ < CurrTrackDatasDSK[UnitSel][HeadSel].NbSect) { // Encore des secteurs à transférer ?
									SetSecteur(posSect);
									cntdata = PosData;
									PosStack -= 2;
								}
								else {
									CptOverRun = 0;
									EndExePhase(); // Dernier octet à envoyer avant résultat
								}
							}
						}
						return (ret);

					case 11:
						ST0 &= ~ST0_SE;
						ST0 |= ST0_IC1; // ### à voir
						CptOverRun = 0;
						return (ST0);

					case 12:
						return (ST1);

					case 13:
						return (ST2);

					case 14:
						return (sectC);

					case 15:
						return (sectH);

					case 16:
						return (sectR);

					case 17:
						FctExec = null;            // Prêt pour une nouvelle commande
						Status &= ~STATUS_DIO & ~STATUS_CB;  // Repositionné en Z80->UPD
						return (sectN);
				}
			}
			return (0xFF);
		}

		static int CmdSpecify(int val, int sens, int tps) {
			if (sens == SENS_WR && (Status & STATUS_DIO) == 0) {
				switch (PosStack++) {
					case 1:
						SRT = (16 - (val >> 4)) * 2000; // Temps déplacement d'un pas de la tête en µs
						HUT = val & 0x0F;
						break;
					case 2:
						HLT = val & 0xFE;
						FctExec = null;
						Status &= ~STATUS_DIO & ~STATUS_CB;  // Repositionné en Z80->UPD (phase cmd)
						EndExePhase();
						break;
				}
			}
			return (0xFF);
		}

		static int CmdSenseDriveStatus(int val, int sens, int tps) {
			if (sens == SENS_WR && (Status & STATUS_DIO) == 0) {
				if (PosStack++ > 0) {
					UnitSel = val & 3;
					HeadSel = (val >> 2) & 1;
					ST3 &= 0xF8;
					ST3 |= UnitSel | (HeadSel << 2);
					Status |= STATUS_DIO; // Phase result
				}
			}
			if (sens == SENS_RD && (Status & STATUS_DIO) != 0) {
				FctExec = null;            // Prêt pour une nouvelle commande
				Status &= ~STATUS_DIO & ~STATUS_CB;  // Repositionné en Z80->UPD
				EndExePhase();
				return (ST3);
			}
			return (0xFF);
		}

		static int ReadDataWithCM(int val, int sens, int tps, int mask) {
			int ret, tmpC = 0, tmpH = 0, tmpR = 0, tmpN = 0;
			if (sens == SENS_WR && (Status & STATUS_DIO) == 0) {
				switch (PosStack++) {
					case 1:
						UnitSel = val & 3;
						HeadSel = (val >> 2) & 1;
						ST0 &= ~ST0_IC1 & ~ST0_IC2;
						break;

					case 2:
						sectC = val;
						break;

					case 3:
						sectH = val;
						break;

					case 4:
						sectR = val;
						break;

					case 5:
						sectN = val;
						break;

					case 6:
						sectEOT = val;
						break;

					case 7:
						sectGAP = val;
						break;

					case 8:
						sectSize = val;
						RechercheSecteur(sectC, sectH, sectR, sectN, sectEOT);
						ST2 ^= mask;
						cntdata = PosData;
						Status |= STATUS_DIO;
						if ((ST1 & ST1_ND) == 0) {
							StartExePhase();
							TpsOverRun = TPS_OVER_RUN * 2000; //### Valeur arbitraire, temps entre id et données secteurs
							CptOverRun = 1;
							if (sectN == 0)
								TailleSect = sectSize;
						}
						else {
							PosStack++;
							Break = 1; // Ne peut pas passer en EXECUTE
						}
						break;
				}
			}
			if (sens == SENS_RD && (Status & STATUS_DIO) != 0) {
				switch (PosStack++) {
					case 9:
						TpsOverRun = TPS_OVER_RUN;
						ret = Dsk[UnitSel].Data[CurrTrack[UnitSel]][HeadSel][cntdata++];
						if ((Status & STATUS_EXE) != 0) {
							if (--TailleSect > 0) // Tant que fin de secteur pas atteinte
								PosStack--;
							else {
								NextSecteur(ref tmpC, ref tmpH, ref tmpR, ref tmpN);
								if (sectR < sectEOT) {// Encore des secteurs à transférer ?
									RechercheSecteur(sectC, sectH, ++sectR, sectN, sectEOT);
									cntdata = PosData;
									PosStack--;
								}
								else {
									CptOverRun = 0;
									EndExePhase(); // Dernier octet à envoyer avant résultat
								}
							}
						}
						return (ret);

					case 10:
						ST0 &= ~ST0_SE;
						ST0 |= ST0_IC1; // ### à voir
						CptOverRun = 0;
						return (ST0);

					case 11:
						return (ST1);

					case 12:
						return (ST2);

					case 13:
						return (sectC);

					case 14:
						return (sectH);

					case 15:
						return (sectR);

					case 16:
						FctExec = null;            // Prêt pour une nouvelle commande
						Status &= ~STATUS_DIO & ~STATUS_CB;  // Repositionné en Z80->UPD
						return (sectN);
				}
			}
			return (0xFF);
		}

		static int CmdReadData(int val, int sens, int tps) {
			return (ReadDataWithCM(val, sens, tps, 0x00));
		}

		static int WriteDataWithCM(int val, int sens, int tps, int mask) {
			int tmpC = 0, tmpH = 0, tmpR = 0, tmpN = 0;

			if (sens == SENS_WR && (Status & STATUS_DIO) == 0) {
				switch (PosStack++) {
					case 1:
						UnitSel = val & 3;
						HeadSel = (val >> 2) & 1;
						ST0 &= ~ST0_IC1 & ~ST0_IC2;
						break;

					case 2:
						sectC = val;
						break;

					case 3:
						sectH = val;
						break;

					case 4:
						sectR = val;
						break;

					case 5:
						sectN = val;
						break;

					case 6:
						sectEOT = val;
						break;

					case 7:
						sectGAP = val;
						break;

					case 8:
						sectSize = val;
						RechercheSecteur(sectC, sectH, sectR, sectN, sectEOT);
						cntdata = PosData;
						if ((ST1 & ST1_ND) == 0) {
							StartExePhase();
							if (sectN == 0)
								TailleSect = sectSize;
						}
						else {
							Status |= STATUS_DIO;
							PosStack++;
						}
						break;

					case 9:
						Dsk[UnitSel].Data[CurrTrack[UnitSel]][HeadSel][cntdata++] = (byte)val;
						Dsk[UnitSel].FlagWrite = 1;
						if ((Status & STATUS_EXE) != 0) {
							if (--TailleSect > 0) // Tant que fin de secteur pas atteinte
								PosStack--;
							else {
								NextSecteur(ref tmpC, ref tmpH, ref tmpR, ref tmpN);
								if (sectR++ < sectEOT) { // Encore des secteurs à transférer ?
									RechercheSecteur(sectC, sectH, sectR, sectN, sectEOT);
									cntdata = PosData;
									PosStack--;
								}
								else {
									Status |= STATUS_DIO;
									EndExePhase(); // Dernier octet à envoyer avant résultat
								}
							}
						}
						break;
				}
			}
			if (sens == SENS_RD && (Status & STATUS_DIO) != 0) {
				switch (PosStack++) {
					case 10:
						ST0 &= ~ST0_SE;
						ST0 |= ST0_IC1; // ### à voir
						CptOverRun = 0;
						return (ST0);

					case 11:
						return (ST1);

					case 12:
						return (ST2);

					case 13:
						return (sectC);

					case 14:
						return (sectH);

					case 15:
						return (sectR);

					case 16:
						FctExec = null;            // Prêt pour une nouvelle commande
						Status &= ~STATUS_DIO & ~STATUS_CB;  // Repositionné en Z80->UPD
						EndExePhase();
						return (sectN);
				}
			}
			return (0xFF);
		}

		static int CmdWriteData(int val, int sens, int tps) {
			return (WriteDataWithCM(val, sens, tps, 0x00));
		}

		static int CmdRecalibrate(int val, int sens, int tps) {
			if (sens == SENS_WR && (Status & STATUS_DIO) == 0) {
				if (PosStack++ > 0) {
					UnitSel = val & 3;
					NewCylinder = 0;
					MoveTrack(0, 77);
				}
			}
			if (sens == SENS_NONE && (Status & STATUS_EXE) != 0)
				MoveTrack(tps, 0);

			return (0xFF);
		}

		static int CmdSenseInterStatus(int val, int sens, int tps) {
			if (sens == SENS_WR && (Status & STATUS_DIO) == 0)
				Status |= STATUS_DIO; // Phase result

			if (sens == SENS_RD && (Status & STATUS_DIO) != 0) {
				switch (PosStack++) {
					case 0:
						if (Break != 0) {
							ST0 &= ~ST0_IC1;
							ST0 |= ST0_IC2;
							Break = 0;
						}
						return (ST0);

					case 1:
						FctExec = null;            // Prêt pour une nouvelle commande
						Status &= ~STATUS_DIO & ~STATUS_CB;  // Repositionné en Z80->UPD
						if ((ST0 & (ST0_SE | ST0_IC1)) != 0)
							ST0 = ST0_IC2;

						EndExePhase();
						return (CurrTrack[UnitSel]);
				}
			}
			return (0xFF);
		}

		static int CmdReadID(int val, int sens, int tps) {
			if (sens == SENS_WR && (Status & STATUS_DIO) == 0) {
				if (PosStack++ > 0) {
					UnitSel = val & 3;
					HeadSel = (val >> 2) & 1;
					Status |= STATUS_DIO;
				}
			}
			if (sens == SENS_RD && (Status & STATUS_DIO) != 0) {
				//        TpsNextByte += 16000;
				switch (PosStack++) {
					case 2:
						return (CalcST0());

					case 3:
						return (ST1);

					case 4:
						return (ST2);

					case 5:
						NextSecteur(ref sectC, ref sectH, ref sectR, ref sectN);
						return (sectC);

					case 6:
						return (sectH);

					case 7:
						return (sectR);

					case 8:
						FctExec = null;            // Prêt pour une nouvelle commande
						Status &= ~STATUS_CB & ~STATUS_DIO;
						EndExePhase();
						return (sectN);
				}
			}
			return (0xFF);
		}

		static int CmdReadDelData(int val, int sens, int tps) {
			return (ReadDataWithCM(val, sens, tps, ST2_CM));
		}

		static int CmdFormatTrack(int val, int sens, int tps) {
			//Log( MODULENAME, "Commande CmdFormatTrack pas implémentée.", LOG_WARNING );
			return (0);
		}

		static int CmdSeek(int val, int sens, int tps) {
			if (sens == SENS_WR && (Status & STATUS_DIO) == 0) {
				switch (PosStack++) {
					case 1:
						UnitSel = val & 3;
						break;

					case 2:
						NewCylinder = val;
						MoveTrack(0, 255);
						break;
				}
			}
			if (sens == SENS_NONE && (Status & STATUS_EXE) != 0)
				MoveTrack(tps, 0);

			return (0xFF);
		}

		static int CmdWriteDelData(int val, int sens, int tps) {
			return (WriteDataWithCM(val, sens, tps, ST2_CM));
		}

		static int CmdScanLow(int val, int sens, int tps) {
			//Log( MODULENAME, "Commande CmdScanLow pas implémentée.", LOG_WARNING );
			return (0);
		}

		static int CmdScanLowEq(int val, int sens, int tps) {
			//Log( MODULENAME, "Commande CmdScanLowEq pas implémentée.", LOG_WARNING );
			return (0);
		}

		static int CmdScanHighEq(int val, int sens, int tps) {
			//Log( MODULENAME, "Commande CmdScanHighEq pas implémentée.", LOG_WARNING );
			return (0);
		}

		static public int Read(int port) {
			int ret = 0xFF;

			if ((port & 0x101) > 0) {
				if ((port & 1) > 0) {// Read Data Register
					Status &= ~STATUS_RQM;
					TpsNextByte += TPS_DATA_READ;
					if ((Status & STATUS_DIO) != 0)
						ret = FctExec(0, SENS_RD, 0);
				}
				else
					ret = Status;
			}
			return (ret);
		}

		static public void Write(int port, int val) {
			port &= 0x101;
			if (port == 0x101) {// Write Data Register
				if ((Status & STATUS_DIO) == 0) {
					if (FctExec == null) {
						FctExec = TabCmdUPD[val & 0x1F];
						Status |= STATUS_CB;
						PosStack = 0;
					}
					FctExec(val, SENS_WR, 0);
				}
			}
			else
				if (port <= 1) {
					if (Moteur == 0)
						TpsReady = TPS_MOTOR_ON;

					Moteur = (byte)(val & 1);
					if (Moteur == 0) {
						ST3 &= ~ST3_RY;
						Break = 1;
					}
				}
		}

		static public void Cycle(int us) {
			if (CptOverRun > 0) {
				TpsOverRun -= us;
				if (TpsOverRun < 0) {
					ST1 |= ST1_OR;
					CptOverRun = 0;
					EndExePhase();
				}
			}
			if ((Status & (STATUS_EXE | STATUS_DIO)) != 0 && FctExec != null)
				FctExec(0, SENS_NONE, us);

			if (TpsNextByte > 0 && (Status & STATUS_RQM) == 0)
				TpsNextByte -= us;
			else
				Status |= STATUS_RQM;

			if (Moteur > 0) {
				if (Dsk[UnitSel].ImageOk) {
					CptRotation[UnitSel] += us;
					if (CptRotation[UnitSel] > 200000)
						CptRotation[UnitSel] -= 200000;
				}
				if ((ST3 & ST3_RY) == 0) {
					if (TpsReady > 0)
						TpsReady -= us;
					else
						if (Dsk[UnitSel].ImageOk)
							ST3 |= ST3_RY;
				}
			}
		}

		static public void Init() {
			Dsk[0] = new ImgDSK();
			Dsk[1] = new ImgDSK();
			CurrTrackDatasDSK[0] = new CPCEMUTrack[2];
			CurrTrackDatasDSK[1] = new CPCEMUTrack[2];
			Reset();
		}

		static public void Reset() {
			Status = STATUS_RQM;
			PosStack = 0;
			FctExec = null;
			Moteur = 0;
			IndexSecteur = PosData = 0;
			ST0 = ST1 = ST2 = ST3 = 0;
			SRT = 12000;
			Break = 0;
			HeadSel = 0;
			NbSteps = 0;
			for (UnitSel = 4; --UnitSel > 0; ) {
				NewCylinder = CurrTrack[UnitSel];
				MoveTrack(1, 0);
			}
		}
	}
}
