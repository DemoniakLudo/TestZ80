using System.Runtime.InteropServices;

namespace TestZ80 {
	static public class Z80 {
		const byte BIT0 = 0x01;
		const byte BIT1 = 0x02;
		const byte BIT2 = 0x04;
		const byte BIT3 = 0x08;
		const byte BIT4 = 0x10;
		const byte BIT5 = 0x20;
		const byte BIT6 = 0x40;
		const byte BIT7 = 0x80;

		//
		// Flags Z80
		//
		const byte FLAG_0 = 0x00;
		const byte FLAG_C = 0x01;
		const byte FLAG_N = 0x02;
		const byte FLAG_V = 0x04;
		const byte FLAG_3 = 0x08;
		const byte FLAG_H = 0x10;
		const byte FLAG_5 = 0x20;
		const byte FLAG_Z = 0x40;
		const byte FLAG_S = 0x80;

		const byte N_FLAG_N = 0xFD;
		const byte N_FLAG_V = 0xFB;
		const byte N_FLAG_3 = 0xF7;
		const byte N_FLAG_H = 0xEF;
		const byte N_FLAG_5 = 0xDF;
		const byte N_FLAG_Z = 0xBF;

		private delegate int pFct();

		[StructLayout(LayoutKind.Explicit)]
		public struct Reg {
			[FieldOffset(0)]
			public ushort Word;
			[FieldOffset(0)]
			public byte Low;
			[FieldOffset(1)]
			public byte High;
		}

		static public Reg AF, BC, DE, HL;
		static public Reg _AF, _BC, _DE, _HL;
		static public Reg IX, IY, SP, PC, IR;
		static public byte IFF1;
		static public byte IFF2;
		static public byte InterruptMode;
		static public int IRQ;
		static private Reg MemPtr;
		static private byte[] Parite = new byte[256];
		static private bool CBIndex = false;
		static private int AdrCB;
		static private int SupIrqWaitState;
		static private int Halt;

		static pFct[] tabinstr = new pFct[256]  {
				NO_OP, ___01, ___02, ___03, ___04, ___05, ___06, ___07, // 00
				___08, ___09, ___0A, ___0B, ___0C, ___0D, ___0E, ___0F, // 08
				___10, ___11, ___12, ___13, ___14, ___15, ___16, ___17, // 10
				___18, ___19, ___1A, ___1B, ___1C, ___1D, ___1E, ___1F, // 18
				___20, ___21, ___22, ___23, ___24, ___25, ___26, ___27, // 20
				___28, ___29, ___2A, ___2B, ___2C, ___2D, ___2E, ___2F, // 28
				___30, ___31, ___32, ___33, ___34, ___35, ___36, ___37, // 30
				___38, ___39, ___3A, ___3B, ___3C, ___3D, ___3E, ___3F, // 38
				NO_OP, ___41, ___42, ___43, ___44, ___45, ___46, ___47, // 40
				___48, NO_OP, ___4A, ___4B, ___4C, ___4D, ___4E, ___4F, // 48
				___50, ___51, NO_OP, ___53, ___54, ___55, ___56, ___57, // 50
				___58, ___59, ___5A, NO_OP, ___5C, ___5D, ___5E, ___5F, // 58
				___60, ___61, ___62, ___63, NO_OP, ___65, ___66, ___67, // 60
				___68, ___69, ___6A, ___6B, ___6C, NO_OP, ___6E, ___6F, // 68
				___70, ___71, ___72, ___73, ___74, ___75, ___76, ___77, // 70
				___78, ___79, ___7A, ___7B, ___7C, ___7D, ___7E, NO_OP, // 78
				___80, ___81, ___82, ___83, ___84, ___85, ___86, ___87, // 80
				___88, ___89, ___8A, ___8B, ___8C, ___8D, ___8E, ___8F, // 88
				___90, ___91, ___92, ___93, ___94, ___95, ___96, ___97, // 90
				___98, ___99, ___9A, ___9B, ___9C, ___9D, ___9E, ___9F, // 98
				___A0, ___A1, ___A2, ___A3, ___A4, ___A5, ___A6, ___A7, // A0
				___A8, ___A9, ___AA, ___AB, ___AC, ___AD, ___AE, ___AF, // A8
				___B0, ___B1, ___B2, ___B3, ___B4, ___B5, ___B6, ___B7, // B0
				___B8, ___B9, ___BA, ___BB, ___BC, ___BD, ___BE, ___BF, // B8
				___C0, ___C1, ___C2, ___C3, ___C4, ___C5, ___C6, ___C7, // C0
				___C8, ___C9, ___CA, ___CB, ___CC, ___CD, ___CE, ___CF, // C8
				___D0, ___D1, ___D2, ___D3, ___D4, ___D5, ___D6, ___D7, // D0
				___D8, ___D9, ___DA, ___DB, ___DC, ___DD, ___DE, ___DF, // D8
				___E0, ___E1, ___E2, ___E3, ___E4, ___E5, ___E6, ___E7, // E0
				___E8, ___E9, ___EA, ___EB, ___EC, ___ED, ___EE, ___EF, // E8
				___F0, ___F1, ___F2, ___F3, ___F4, ___F5, ___F6, ___F7, // F0
				___F8, ___F9, ___FA, ___FB, ___FC, ___FD, ___FE, ___FF  // F8
			};

		static pFct[] tabCB = new pFct[256]  {
				CB_00, CB_01, CB_02, CB_03, CB_04, CB_05, CB_06, CB_07, // 00
				CB_08, CB_09, CB_0A, CB_0B, CB_0C, CB_0D, CB_0E, CB_0F, // 08
				CB_10, CB_11, CB_12, CB_13, CB_14, CB_15, CB_16, CB_17, // 10
				CB_18, CB_19, CB_1A, CB_1B, CB_1C, CB_1D, CB_1E, CB_1F, // 18
				CB_20, CB_21, CB_22, CB_23, CB_24, CB_25, CB_26, CB_27, // 20
				CB_28, CB_29, CB_2A, CB_2B, CB_2C, CB_2D, CB_2E, CB_2F, // 28
				CB_30, CB_31, CB_32, CB_33, CB_34, CB_35, CB_36, CB_37, // 30
				CB_38, CB_39, CB_3A, CB_3B, CB_3C, CB_3D, CB_3E, CB_3F, // 38
				CB_40, CB_41, CB_42, CB_43, CB_44, CB_45, CB_46, CB_47, // 40
				CB_48, CB_49, CB_4A, CB_4B, CB_4C, CB_4D, CB_4E, CB_4F, // 48
				CB_50, CB_51, CB_52, CB_53, CB_54, CB_55, CB_56, CB_57, // 50
				CB_58, CB_59, CB_5A, CB_5B, CB_5C, CB_5D, CB_5E, CB_5F, // 58
				CB_60, CB_61, CB_62, CB_63, CB_64, CB_65, CB_66, CB_67, // 60
				CB_68, CB_69, CB_6A, CB_6B, CB_6C, CB_6D, CB_6E, CB_6F, // 68
				CB_70, CB_71, CB_72, CB_73, CB_74, CB_75, CB_76, CB_77, // 70
				CB_78, CB_79, CB_7A, CB_7B, CB_7C, CB_7D, CB_7E, CB_7F, // 78
				CB_80, CB_81, CB_82, CB_83, CB_84, CB_85, CB_86, CB_87, // 80
				CB_88, CB_89, CB_8A, CB_8B, CB_8C, CB_8D, CB_8E, CB_8F, // 88
				CB_90, CB_91, CB_92, CB_93, CB_94, CB_95, CB_96, CB_97, // 90
				CB_98, CB_99, CB_9A, CB_9B, CB_9C, CB_9D, CB_9E, CB_9F, // 98
				CB_A0, CB_A1, CB_A2, CB_A3, CB_A4, CB_A5, CB_A6, CB_A7, // A0
				CB_A8, CB_A9, CB_AA, CB_AB, CB_AC, CB_AD, CB_AE, CB_AF, // A8
				CB_B0, CB_B1, CB_B2, CB_B3, CB_B4, CB_B5, CB_B6, CB_B7, // B0
				CB_B8, CB_B9, CB_BA, CB_BB, CB_BC, CB_BD, CB_BE, CB_BF, // B8
				CB_C0, CB_C1, CB_C2, CB_C3, CB_C4, CB_C5, CB_C6, CB_C7, // C0
				CB_C8, CB_C9, CB_CA, CB_CB, CB_CC, CB_CD, CB_CE, CB_CF, // C8
				CB_D0, CB_D1, CB_D2, CB_D3, CB_D4, CB_D5, CB_D6, CB_D7, // D0
				CB_D8, CB_D9, CB_DA, CB_DB, CB_DC, CB_DD, CB_DE, CB_DF, // D8
				CB_E0, CB_E1, CB_E2, CB_E3, CB_E4, CB_E5, CB_E6, CB_E7, // E0
				CB_E8, CB_E9, CB_EA, CB_EB, CB_EC, CB_ED, CB_EE, CB_EF, // E8
				CB_F0, CB_F1, CB_F2, CB_F3, CB_F4, CB_F5, CB_F6, CB_F7, // F0
				CB_F8, CB_F9, CB_FA, CB_FB, CB_FC, CB_FD, CB_FE, CB_FF, // F8
			};

		static pFct[] tabED = new pFct[256]  {
				ed___, ed___, ed___, ed___, ed___, ed___, ed___, ed___, // 00
				ed___, ed___, ed___, ed___, ed___, ed___, ed___, ed___, // 08
				ed___, ed___, ed___, ed___, ed___, ed___, ed___, ed___, // 10
				ed___, ed___, ed___, ed___, ed___, ed___, ed___, ed___, // 18
				ed___, ed___, ed___, ed___, ed___, ed___, ed___, ed___, // 20
				ed___, ed___, ed___, ed___, ed___, ed___, ed___, ed___, // 28
				ed___, ed___, ed___, ed___, ed___, ed___, ed___, ed___, // 30
				ed___, ed___, ed___, ed___, ed___, ed___, ed___, ed___, // 38
				ED_40, ED_41, ED_42, ED_43, ED_44, ED_45, ED_46, ED_47, // 40
				ED_48, ED_49, ED_4A, ED_4B, ED_44, ED_4D, ED_46, ED_4F, // 48
				ED_50, ED_51, ED_52, ED_53, ED_44, ED_45, ED_56, ED_57, // 50
				ED_58, ED_59, ED_5A, ED_5B, ED_44, ED_45, ED_5E, ED_5F, // 58
				ED_60, ED_61, ED_62, ED_63, ED_44, ED_45, ED_46, ED_67, // 60
				ED_68, ED_69, ED_6A, ED_6B, ED_44, ED_45, ED_46, ED_6F, // 68
				ED_70, ED_71, ED_72, ED_73, ED_44, ED_45, ED_56, ed___, // 70
				ED_78, ED_79, ED_7A, ED_7B, ED_44, ED_45, ED_5E, ed___, // 78
				ed___, ed___, ed___, ed___, ed___, ed___, ed___, ed___, // 80
				ed___, ed___, ed___, ed___, ed___, ed___, ed___, ed___, // 88
				ed___, ed___, ed___, ed___, ed___, ed___, ed___, ed___, // 90
				ed___, ed___, ed___, ed___, ed___, ed___, ed___, ed___, // 98
				ED_A0, ED_A1, ED_A2, ED_A3, ed___, ed___, ed___, ed___, // A0
				ED_A8, ED_A9, ED_AA, ED_AB, ed___, ed___, ed___, ed___, // A8
				ED_B0, ED_B1, ED_B2, ED_B3, ed___, ed___, ed___, ed___, // B0
				ED_B8, ED_B9, ED_BA, ED_BB, ed___, ed___, ed___, ed___, // B8
				ed___, ed___, ed___, ed___, ed___, ed___, ed___, ed___, // C0
				ed___, ed___, ed___, ed___, ed___, ed___, ed___, ed___, // C8
				ed___, ed___, ed___, ed___, ed___, ed___, ed___, ed___, // D0
				ed___, ed___, ed___, ed___, ed___, ed___, ed___, ed___, // D8
				ed___, ed___, ed___, ed___, ed___, ed___, ed___, ed___, // E0
				ed___, ed___, ed___, ed___, ed___, ed___, ed___, ed___, // E8
				ed___, ed___, ed___, ed___, ed___, ed___, ed___, ed___, // F0
				ed___, ed___, ed___, ed___, ed___, ed___, ed___, ed___  // F8
			};

		static pFct[] tabIX = new pFct[256]  {
				dd___, ___01, ___02, ___03, ___04, ___05, ___06, ___07, // 00
				___08, DD_09, ___0A, ___0B, ___0C, ___0D, ___0E, ___0F, // 08
				___10, ___11, ___12, ___13, ___14, ___15, ___16, ___17, // 10
				___18, DD_19, ___1A, ___1B, ___1C, ___1D, ___1E, ___1F, // 18
				___20, DD_21, DD_22, DD_23, DD_24, DD_25, DD_26, ___27, // 20
				___28, DD_29, DD_2A, DD_2B, DD_2C, DD_2D, DD_2E, ___2F, // 28
				___30, ___31, ___32, ___33, DD_34, DD_35, DD_36, ___37, // 30
				___38, DD_39, ___3A, ___3B, ___3C, ___3D, ___3E, ___3F, // 38
				dd___, ___41, ___42, ___43, DD_44, DD_45, DD_46, ___47, // 40
				___48, dd___, ___4A, ___4B, DD_4C, DD_4D, DD_4E, ___4F, // 48
				___50, ___51, dd___, ___53, DD_54, DD_55, DD_56, ___57, // 50
				___58, ___59, ___5A, dd___, DD_5C, DD_5D, DD_5E, ___5F, // 58
				DD_60, DD_61, DD_62, DD_63, dd___, DD_65, DD_66, DD_67, // 60
				DD_68, DD_69, DD_6A, DD_6B, DD_6C, dd___, DD_6E, DD_6F, // 68
				DD_70, DD_71, DD_72, DD_73, DD_74, DD_75, ___76, DD_77, // 70
				___78, ___79, ___7A, ___7B, DD_7C, DD_7D, DD_7E, dd___, // 78
				___80, ___81, ___82, ___83, DD_84, DD_85, DD_86, ___87, // 80
				___88, ___89, ___8A, ___8B, DD_8C, DD_8D, DD_8E, ___8F, // 88
				___90, ___91, ___92, ___93, DD_94, DD_95, DD_96, ___97, // 90
				___98, ___99, ___9A, ___9B, DD_9C, DD_9D, DD_9E, ___9F, // 98
				___A0, ___A1, ___A2, ___A3, DD_A4, DD_A5, DD_A6, ___A7, // A0
				___A8, ___A9, ___AA, ___AB, DD_AC, DD_AD, DD_AE, ___AF, // A8
				___B0, ___B1, ___B2, ___B3, DD_B4, DD_B5, DD_B6, ___B7, // B0
				___B8, ___B9, ___BA, ___BB, DD_BC, DD_BD, DD_BE, ___BF, // B8
				___C0, ___C1, ___C2, ___C3, ___C4, ___C5, ___C6, ___C7, // C0
				___C8, ___C9, ___CA, DD_CB, ___CC, ___CD, ___CE, ___CF, // C8
				___D0, ___D1, ___D2, ___D3, ___D4, ___D5, ___D6, ___D7, // D0
				___D8, ___D9, ___DA, ___DB, ___DC, dd___, ___DE, ___DF, // D8
				___E0, DD_E1, ___E2, DD_E3, ___E4, DD_E5, ___E6, ___E7, // E0
				___E8, DD_E9, ___EA, ___EB, ___EC, ___ED, ___EE, ___EF, // E8
				___F0, ___F1, ___F2, ___F3, ___F4, ___F5, ___F6, ___F7, // F0
				___F8, DD_F9, ___FA, ___FB, ___FC, DD_FD, ___FE, ___FF  // F8
			};

		static pFct[] tabIY = new pFct[256]  {
				fd___, ___01, ___02, ___03, ___04, ___05, ___06, ___07, // 00
				___08, FD_09, ___0A, ___0B, ___0C, ___0D, ___0E, ___0F, // 08
				___10, ___11, ___12, ___13, ___14, ___15, ___16, ___17, // 10
				___18, FD_19, ___1A, ___1B, ___1C, ___1D, ___1E, ___1F, // 18
				___20, FD_21, FD_22, FD_23, FD_24, FD_25, FD_26, ___27, // 20
				___28, FD_29, FD_2A, FD_2B, FD_2C, FD_2D, FD_2E, ___2F, // 28
				___30, ___31, ___32, ___33, FD_34, FD_35, FD_36, ___37, // 30
				___38, FD_39, ___3A, ___3B, ___3C, ___3D, ___3E, ___3F, // 38
				fd___, ___41, ___42, ___43, FD_44, FD_45, FD_46, ___47, // 40
				___48, fd___, ___4A, ___4B, FD_4C, FD_4D, FD_4E, ___4F, // 48
				___50, ___51, fd___, ___53, FD_54, FD_55, FD_56, ___57, // 50
				___58, ___59, ___5A, fd___, FD_5C, FD_5D, FD_5E, ___5F, // 58
				FD_60, FD_61, FD_62, FD_63, fd___, FD_65, FD_66, FD_67, // 60
				FD_68, FD_69, FD_6A, FD_6B, FD_6C, fd___, FD_6E, FD_6F, // 68
				FD_70, FD_71, FD_72, FD_73, FD_74, FD_75, ___76, FD_77, // 70
				___78, ___79, ___7A, ___7B, FD_7C, FD_7D, FD_7E, fd___, // 78
				___80, ___81, ___82, ___83, FD_84, FD_85, FD_86, ___87, // 80
				___88, ___89, ___8A, ___8B, FD_8C, FD_8D, FD_8E, ___8F, // 88
				___90, ___91, ___92, ___93, FD_94, FD_95, FD_96, ___97, // 90
				___98, ___99, ___9A, ___9B, FD_9C, FD_9D, FD_9E, ___9F, // 98
				___A0, ___A1, ___A2, ___A3, FD_A4, FD_A5, FD_A6, ___A7, // A0
				___A8, ___A9, ___AA, ___AB, FD_AC, FD_AD, FD_AE, ___AF, // A8
				___B0, ___B1, ___B2, ___B3, FD_B4, FD_B5, FD_B6, ___B7, // B0
				___B8, ___B9, ___BA, ___BB, FD_BC, FD_BD, FD_BE, ___BF, // B8
				___C0, ___C1, ___C2, ___C3, ___C4, ___C5, ___C6, ___C7, // C0
				___C8, ___C9, ___CA, FD_CB, ___CC, ___CD, ___CE, ___CF, // C8
				___D0, ___D1, ___D2, ___D3, ___D4, ___D5, ___D6, ___D7, // D0
				___D8, ___D9, ___DA, ___DB, ___DC, FD_DD, ___DE, ___DF, // D8
				___E0, FD_E1, ___E2, FD_E3, ___E4, FD_E5, ___E6, ___E7, // E0
				___E8, FD_E9, ___EA, ___EB, ___EC, ___ED, ___EE, ___EF, // E8
				___F0, ___F1, ___F2, ___F3, ___F4, ___F5, ___F6, ___F7, // F0
				___F8, FD_F9, ___FA, ___FB, ___FC, fd___, ___FE, ___FF  // F8
			};

		static int ADD_R8(int v, int c) {
			int t = AF.High + v + (c & FLAG_C);
			AF.Low = (byte)(((~(AF.High ^ v) & (v ^ t) & 0x80) != 0 ? FLAG_V : 0) | (t >> 8) | (t & (FLAG_S | FLAG_3 | FLAG_5)) | (((t & 0xFF) != 0) ? 0 : FLAG_Z) | ((AF.High ^ v ^ t) & FLAG_H));
			AF.High = (byte)t;
			return 1;
		}

		static int SUB_R8(int v, int c) {
			int t = AF.High - v - (c & FLAG_C);
			AF.Low = (byte)((((AF.High ^ v) & (AF.High ^ t) & 0x80) != 0 ? FLAG_V : 0) | FLAG_N | -(t >> 8) | (t & (FLAG_S | FLAG_3 | FLAG_5)) | (((t & 0xFF) != 0) ? 0 : FLAG_Z) | ((AF.High ^ v ^ t) & FLAG_H));
			AF.High = (byte)t;
			return 1;
		}

		static int CP_R8(int v) {
			int t = AF.High - v;
			AF.Low = (byte)((((AF.High ^ v) & (AF.High ^ t) & 0x80) != 0 ? FLAG_V : 0) | FLAG_N | -(t >> 8) | (t & FLAG_S) | (v & (FLAG_5 | FLAG_3)) | (((t & 0xFF) != 0) ? 0 : FLAG_Z) | ((AF.High ^ v ^ t) & FLAG_H));
			return 1;
		}

		static int AND_R8(int v) {
			AF.High &= (byte)v;
			AF.Low = (byte)(FLAG_H | Parite[AF.High]);
			return 1;
		}

		static int OR_R8(int v) {
			AF.High |= (byte)v;
			AF.Low = Parite[AF.High];
			return 1;
		}

		static int XOR_R8(int v) {
			AF.High ^= (byte)v;
			AF.Low = Parite[AF.High];
			return 1;
		}

		static int FLAG_INC(int reg) {
			AF.Low = (byte)((AF.Low & FLAG_C) | (reg & (FLAG_S | FLAG_3 | FLAG_5)) | (reg == 0x80 ? FLAG_V : 0) | ((reg & 0x0F) != 0 ? 0 : FLAG_H) | ((reg != 0) ? 0 : FLAG_Z));
			return 1;
		}

		static int FLAG_DEC(int reg) {
			AF.Low = (byte)(FLAG_N | (AF.Low & FLAG_C) | (reg == 0x7F ? FLAG_V : 0) | ((reg & 0x0F) == 0x0F ? FLAG_H : 0) | (reg & (FLAG_S | FLAG_3 | FLAG_5)) | ((reg != 0) ? 0 : FLAG_Z));
			return 1;
		}

		static int ADD_R16(ref ushort Reg, int v) {
			ushort tmp = Reg;
			MemPtr.Word = (ushort)(Reg + 1);
			Reg = (ushort)(Reg + v);
			AF.Low = (byte)(AF.Low & (FLAG_S | FLAG_Z | FLAG_V) | ((Reg >> 8) & (FLAG_5 | FLAG_3)));
			if (tmp > Reg)
				AF.Low |= FLAG_C;

			if (((tmp ^ v ^ Reg) & 0x1000) != 0)
				AF.Low |= FLAG_H;

			return 3;
		}

		static int ADC_R16(int v) {
			int t = HL.Word + v + (AF.Low & FLAG_C);
			MemPtr.Word = (ushort)(HL.Word + 1);
			AF.Low = (byte)(((t & 0x10000) != 0 ? FLAG_C : 0) | ((~(HL.Word ^ v) & (v ^ t) & 0x8000) != 0 ? FLAG_V : 0) | (((HL.Word ^ v ^ t) & 0x1000) != 0 ? FLAG_H : 0) | ((t & 0xFFFF) != 0 ? 0 : FLAG_Z) | ((t >> 8) & (FLAG_S | FLAG_5 | FLAG_3)));
			HL.Word = (ushort)t;
			return 4;
		}

		static int SBC_R16(int v) {
			int t = HL.Word - v - (AF.Low & FLAG_C);
			MemPtr.Word = (ushort)(HL.Word + 1);
			AF.Low = (byte)(FLAG_N | ((t & 0x10000) != 0 ? FLAG_C : 0) | (((HL.Word ^ v) & (HL.Word ^ t) & 0x8000) != 0 ? FLAG_V : 0) | (((HL.Word ^ v ^ t) & 0x1000) != 0 ? FLAG_H : 0) | ((t & 0xFFFF) != 0 ? 0 : FLAG_Z) | ((t >> 8) & (FLAG_S | FLAG_5 | FLAG_3)));
			HL.Word = (ushort)t;
			return 4;
		}

		static int Bit(byte r, int b) {
			AF.Low = (byte)((AF.Low & FLAG_C) | FLAG_H | Parite[r & b]);
			return 2;
		}

		static int Res(ref byte r, int b) {
			r = (byte)((CBIndex ? VGA.PEEK8(AdrCB) : r) & ~b);
			if (CBIndex)
				VGA.POKE8(AdrCB, r);

			return (2);
		}

		static int Set(ref byte r, int b) {
			r = (byte)((CBIndex ? VGA.PEEK8(AdrCB) : r) | b);
			if (CBIndex)
				VGA.POKE8(AdrCB, r);

			return (2);
		}

		static byte RLC(byte reg) {
			AF.Low = (byte)(reg >> 7);
			reg = (byte)((reg << 1) | AF.Low);
			AF.Low |= Parite[reg];
			return (reg);
		}

		static byte RRC(byte reg) {
			AF.Low = (byte)(reg & FLAG_C);
			reg = (byte)((reg >> 1) | (AF.Low << 7));
			AF.Low |= Parite[reg];
			return (reg);
		}

		static byte RL(byte reg) {
			int i = reg << 1;
			reg = (byte)(i | (AF.Low & FLAG_C));
			AF.Low = (byte)((i >> 8) | Parite[reg]);
			return (reg);
		}

		static byte RR(byte reg) {
			byte i = (byte)((reg >> 1) | (AF.Low << 7));
			AF.Low = (byte)((reg & FLAG_C) | Parite[i]);
			reg = i;
			return (reg);
		}

		static byte SLA(byte reg) {
			AF.Low = (byte)(reg >> 7);
			reg = (byte)(reg << 1);
			AF.Low |= Parite[reg];
			return (reg);
		}

		static byte SRA(byte reg) {
			AF.Low = (byte)(reg & FLAG_C);
			reg = (byte)((reg >> 1) | (reg & FLAG_S));
			AF.Low |= Parite[reg];
			return (reg);
		}

		static byte SLL(byte reg) {
			AF.Low = (byte)(reg >> 7);
			reg = (byte)((reg << 1) | 1);
			AF.Low |= Parite[reg];
			return (reg);
		}

		static byte SRL(byte reg) {
			AF.Low = (byte)(reg & FLAG_C);
			reg = (byte)(reg >> 1);
			AF.Low |= Parite[reg];
			return (reg);
		}

		static int PUSH(ushort Reg) {
			SP.Word -= 2;
			VGA.POKE16(SP.Word, Reg);
			return 4;
		}

		static ushort POP() {
			ushort ret = VGA.PEEK16(SP.Word);
			SP.Word += 2;
			return ret;
		}

		static int RST(ushort adr) {
			PUSH(PC.Word);
			MemPtr.Word = PC.Word = adr;
			return (4);
		}

		/************
	   * OPCODE CB *
	   ************/

		static int CB_00() {/* RLC B */
			BC.High = RLC(CBIndex ? VGA.PEEK8(AdrCB) : BC.High);
			if (CBIndex)
				VGA.POKE8(AdrCB, BC.High);

			return (2);
		}

		static int CB_01() {/* RLC C */
			BC.Low = RLC(CBIndex ? VGA.PEEK8(AdrCB) : BC.Low);
			if (CBIndex)
				VGA.POKE8(AdrCB, BC.Low);

			return (2);
		}

		static int CB_02() {/* RLC D */
			DE.High = RLC(CBIndex ? VGA.PEEK8(AdrCB) : DE.High);
			if (CBIndex)
				VGA.POKE8(AdrCB, DE.High);

			return (2);
		}

		static int CB_03() {/* RLC E */
			DE.Low = RLC(CBIndex ? VGA.PEEK8(AdrCB) : DE.Low);
			if (CBIndex)
				VGA.POKE8(AdrCB, DE.Low);

			return (2);
		}


		static int CB_04() {/* RLC H */
			HL.High = RLC(CBIndex ? VGA.PEEK8(AdrCB) : HL.High);
			if (CBIndex)
				VGA.POKE8(AdrCB, HL.High);

			return (2);
		}


		static int CB_05() {/* RLC L */
			HL.Low = RLC(CBIndex ? VGA.PEEK8(AdrCB) : HL.Low);
			if (CBIndex)
				VGA.POKE8(AdrCB, HL.Low);

			return (2);
		}


		static int CB_06() {/* RLC (HL) */
			VGA.POKE8(AdrCB, RLC(VGA.PEEK8(AdrCB)));
			return (4);
		}


		static int CB_07() {/* RLC A */
			AF.High = RLC(CBIndex ? VGA.PEEK8(AdrCB) : AF.High);
			if (CBIndex)
				VGA.POKE8(AdrCB, AF.High);

			return (2);
		}


		static int CB_08() {/* RRC B */
			BC.High = RRC(CBIndex ? VGA.PEEK8(AdrCB) : BC.High);
			if (CBIndex)
				VGA.POKE8(AdrCB, BC.High);

			return (2);
		}


		static int CB_09() {/* RRC C */
			BC.Low = RRC(CBIndex ? VGA.PEEK8(AdrCB) : BC.Low);
			if (CBIndex)
				VGA.POKE8(AdrCB, BC.Low);

			return (2);
		}


		static int CB_0A() {/* RRC D */
			DE.High = RRC(CBIndex ? VGA.PEEK8(AdrCB) : DE.High);
			if (CBIndex)
				VGA.POKE8(AdrCB, DE.High);

			return (2);
		}


		static int CB_0B() {/* RRC E */
			DE.Low = RRC(CBIndex ? VGA.PEEK8(AdrCB) : DE.Low);
			if (CBIndex)
				VGA.POKE8(AdrCB, DE.Low);

			return (2);
		}


		static int CB_0C() {/* RRC H */
			HL.High = RRC(CBIndex ? VGA.PEEK8(AdrCB) : HL.High);
			if (CBIndex)
				VGA.POKE8(AdrCB, HL.High);

			return (2);
		}


		static int CB_0D() {/* RRC L */
			HL.Low = RRC(CBIndex ? VGA.PEEK8(AdrCB) : HL.Low);
			if (CBIndex)
				VGA.POKE8(AdrCB, HL.Low);

			return (2);
		}


		static int CB_0E() {/* RRC (HL) */
			VGA.POKE8(AdrCB, RRC(VGA.PEEK8(AdrCB)));
			return (4);
		}


		static int CB_0F() {/* RRC A */
			AF.High = RRC(CBIndex ? VGA.PEEK8(AdrCB) : AF.High);
			if (CBIndex)
				VGA.POKE8(AdrCB, AF.High);

			return (2);
		}


		static int CB_10() {/* RL B */
			BC.High = RL(CBIndex ? VGA.PEEK8(AdrCB) : BC.High);
			if (CBIndex)
				VGA.POKE8(AdrCB, BC.High);

			return (2);
		}


		static int CB_11() {/* RL C */
			BC.Low = RL(CBIndex ? VGA.PEEK8(AdrCB) : BC.Low);
			if (CBIndex)
				VGA.POKE8(AdrCB, BC.Low);

			return (2);
		}


		static int CB_12() {/* RL D */
			DE.High = RL(CBIndex ? VGA.PEEK8(AdrCB) : DE.High);
			if (CBIndex)
				VGA.POKE8(AdrCB, DE.High);

			return (2);
		}


		static int CB_13() {/* RL E */
			DE.Low = RL(CBIndex ? VGA.PEEK8(AdrCB) : DE.Low);
			if (CBIndex)
				VGA.POKE8(AdrCB, DE.Low);

			return (2);
		}


		static int CB_14() {/* RL H */
			HL.High = RL(CBIndex ? VGA.PEEK8(AdrCB) : HL.High);
			if (CBIndex)
				VGA.POKE8(AdrCB, HL.High);

			return (2);
		}


		static int CB_15() {/* RL L */
			HL.Low = RL(CBIndex ? VGA.PEEK8(AdrCB) : HL.Low);
			if (CBIndex)
				VGA.POKE8(AdrCB, HL.Low);

			return (2);
		}


		static int CB_16() {/* RL (HL) */
			VGA.POKE8(AdrCB, RL(VGA.PEEK8(AdrCB)));
			return (4);
		}


		static int CB_17() {/* RL A */
			AF.High = RL(CBIndex ? VGA.PEEK8(AdrCB) : AF.High);
			if (CBIndex)
				VGA.POKE8(AdrCB, AF.High);

			return (2);
		}


		static int CB_18() {/* RR B */
			BC.High = RR(CBIndex ? VGA.PEEK8(AdrCB) : BC.High);
			if (CBIndex)
				VGA.POKE8(AdrCB, BC.High);

			return (2);
		}


		static int CB_19() {/* RR C */
			BC.Low = RR(CBIndex ? VGA.PEEK8(AdrCB) : BC.Low);
			if (CBIndex)
				VGA.POKE8(AdrCB, BC.Low);

			return (2);
		}


		static int CB_1A() {/* RR D */
			DE.High = RR(CBIndex ? VGA.PEEK8(AdrCB) : DE.High);
			if (CBIndex)
				VGA.POKE8(AdrCB, DE.High);

			return (2);
		}


		static int CB_1B() {/* RR E */
			DE.Low = RR(CBIndex ? VGA.PEEK8(AdrCB) : DE.Low);
			if (CBIndex)
				VGA.POKE8(AdrCB, DE.Low);

			return (2);
		}


		static int CB_1C() {/* RR H */
			HL.High = RR(CBIndex ? VGA.PEEK8(AdrCB) : HL.High);
			if (CBIndex)
				VGA.POKE8(AdrCB, HL.High);

			return (2);
		}


		static int CB_1D() {/* RR L */
			HL.Low = RR(CBIndex ? VGA.PEEK8(AdrCB) : HL.Low);
			if (CBIndex)
				VGA.POKE8(AdrCB, HL.Low);

			return (2);
		}


		static int CB_1E() {/* RR (HL) */
			VGA.POKE8(AdrCB, RR(VGA.PEEK8(AdrCB)));
			return (4);
		}


		static int CB_1F() {/* RR A */
			AF.High = RR(CBIndex ? VGA.PEEK8(AdrCB) : AF.High);
			if (CBIndex)
				VGA.POKE8(AdrCB, AF.High);

			return (2);
		}


		static int CB_20() {/* SLA B */
			BC.High = SLA(CBIndex ? VGA.PEEK8(AdrCB) : BC.High);
			if (CBIndex)
				VGA.POKE8(AdrCB, BC.High);

			return (2);
		}


		static int CB_21() {/* SLA C */
			BC.Low = SLA(CBIndex ? VGA.PEEK8(AdrCB) : BC.Low);
			if (CBIndex)
				VGA.POKE8(AdrCB, BC.Low);

			return (2);
		}


		static int CB_22() {/* SLA D */
			DE.High = SLA(CBIndex ? VGA.PEEK8(AdrCB) : DE.High);
			if (CBIndex)
				VGA.POKE8(AdrCB, DE.High);

			return (2);
		}


		static int CB_23() {/* SLA E */
			DE.Low = SLA(CBIndex ? VGA.PEEK8(AdrCB) : DE.Low);
			if (CBIndex)
				VGA.POKE8(AdrCB, DE.Low);

			return (2);
		}


		static int CB_24() {/* SLA H */
			HL.High = SLA(CBIndex ? VGA.PEEK8(AdrCB) : HL.High);
			if (CBIndex)
				VGA.POKE8(AdrCB, HL.High);

			return (2);
		}


		static int CB_25() {/* SLA L */
			HL.Low = SLA(CBIndex ? VGA.PEEK8(AdrCB) : HL.Low);
			if (CBIndex)
				VGA.POKE8(AdrCB, HL.Low);

			return (2);
		}


		static int CB_26() {/* SLA (HL) */
			VGA.POKE8(AdrCB, SLA(VGA.PEEK8(AdrCB)));
			return (4);
		}


		static int CB_27() {/* SLA A */
			AF.High = SLA(CBIndex ? VGA.PEEK8(AdrCB) : AF.High);
			if (CBIndex)
				VGA.POKE8(AdrCB, AF.High);

			return (2);
		}


		static int CB_28() {/* SRA B */
			BC.High = SRA(CBIndex ? VGA.PEEK8(AdrCB) : BC.High);
			if (CBIndex)
				VGA.POKE8(AdrCB, BC.High);

			return (2);
		}


		static int CB_29() {/* SRA C */
			BC.Low = SRA(CBIndex ? VGA.PEEK8(AdrCB) : BC.Low);
			if (CBIndex)
				VGA.POKE8(AdrCB, BC.Low);

			return (2);
		}


		static int CB_2A() {/* SRA D */
			DE.High = SRA(CBIndex ? VGA.PEEK8(AdrCB) : DE.High);
			if (CBIndex)
				VGA.POKE8(AdrCB, DE.High);

			return (2);
		}


		static int CB_2B() {/* SRA E */
			DE.Low = SRA(CBIndex ? VGA.PEEK8(AdrCB) : DE.Low);
			if (CBIndex)
				VGA.POKE8(AdrCB, DE.Low);

			return (2);
		}


		static int CB_2C() {/* SRA H */
			HL.High = SRA(CBIndex ? VGA.PEEK8(AdrCB) : HL.High);
			if (CBIndex)
				VGA.POKE8(AdrCB, HL.High);

			return (2);
		}


		static int CB_2D() {/* SRA L */
			HL.Low = SRA(CBIndex ? VGA.PEEK8(AdrCB) : HL.Low);
			if (CBIndex)
				VGA.POKE8(AdrCB, HL.Low);

			return (2);
		}


		static int CB_2E() {/* SRA (HL) */
			VGA.POKE8(AdrCB, SRA(VGA.PEEK8(AdrCB)));
			return (4);
		}


		static int CB_2F() {/* SRA A */
			AF.High = SRA(CBIndex ? VGA.PEEK8(AdrCB) : AF.High);
			if (CBIndex)
				VGA.POKE8(AdrCB, AF.High);

			return (2);
		}


		static int CB_30() {/* SLL B */
			BC.High = SLL(CBIndex ? VGA.PEEK8(AdrCB) : BC.High);
			if (CBIndex)
				VGA.POKE8(AdrCB, BC.High);

			return (2);
		}


		static int CB_31() {/* SLL C */
			BC.Low = SLL(CBIndex ? VGA.PEEK8(AdrCB) : BC.Low);
			if (CBIndex)
				VGA.POKE8(AdrCB, BC.Low);

			return (2);
		}


		static int CB_32() {/* SLL D */
			DE.High = SLL(CBIndex ? VGA.PEEK8(AdrCB) : DE.High);
			if (CBIndex)
				VGA.POKE8(AdrCB, DE.High);

			return (2);
		}


		static int CB_33() {/* SLL E */
			DE.Low = SLL(CBIndex ? VGA.PEEK8(AdrCB) : DE.Low);
			if (CBIndex)
				VGA.POKE8(AdrCB, DE.Low);

			return (2);
		}


		static int CB_34() {/* SLL H */
			HL.High = SLL(CBIndex ? VGA.PEEK8(AdrCB) : HL.High);
			if (CBIndex)
				VGA.POKE8(AdrCB, HL.High);

			return (2);
		}


		static int CB_35() {/* SLL L */
			HL.Low = SLL(CBIndex ? VGA.PEEK8(AdrCB) : HL.Low);
			if (CBIndex)
				VGA.POKE8(AdrCB, HL.Low);

			return (2);
		}


		static int CB_36() {/* SLL (HL) */
			VGA.POKE8(AdrCB, SLL(VGA.PEEK8(AdrCB)));
			return (4);
		}


		static int CB_37() {/* SLL A */
			AF.High = SLL(CBIndex ? VGA.PEEK8(AdrCB) : AF.High);
			if (CBIndex)
				VGA.POKE8(AdrCB, AF.High);

			return (2);
		}


		static int CB_38() {/* SRL B */
			BC.High = SRL(CBIndex ? VGA.PEEK8(AdrCB) : BC.High);
			if (CBIndex)
				VGA.POKE8(AdrCB, BC.High);

			return (2);
		}


		static int CB_39() {/* SRL C */
			BC.Low = SRL(CBIndex ? VGA.PEEK8(AdrCB) : BC.Low);
			if (CBIndex)
				VGA.POKE8(AdrCB, BC.Low);

			return (2);
		}


		static int CB_3A() {/* SRL D */
			DE.High = SRL(CBIndex ? VGA.PEEK8(AdrCB) : DE.High);
			if (CBIndex)
				VGA.POKE8(AdrCB, DE.High);

			return (2);
		}


		static int CB_3B() {/* SRL E */
			DE.Low = SRL(CBIndex ? VGA.PEEK8(AdrCB) : DE.Low);
			if (CBIndex)
				VGA.POKE8(AdrCB, DE.Low);

			return (2);
		}


		static int CB_3C() {/* SRL H */
			HL.High = SRL(CBIndex ? VGA.PEEK8(AdrCB) : HL.High);
			if (CBIndex)
				VGA.POKE8(AdrCB, HL.High);

			return (2);
		}


		static int CB_3D() {/* SRL L */
			if (CBIndex)
				VGA.POKE8(AdrCB, HL.Low);

			return (2);
		}


		static int CB_3E() {/* SRL (HL) */
			VGA.POKE8(AdrCB, SRL(VGA.PEEK8(AdrCB)));
			return (4);
		}


		static int CB_3F() {/* SRL A */
			AF.High = SRL(CBIndex ? VGA.PEEK8(AdrCB) : AF.High);
			if (CBIndex)
				VGA.POKE8(AdrCB, AF.High);

			return (2);
		}


		static int CB_40() {/* BIT 0, B */
			return Bit(BC.High, BIT0);
		}


		static int CB_41() {/* BIT 0, C */
			return Bit(BC.Low, BIT0);
		}


		static int CB_42() {/* BIT 0, D */
			return Bit(DE.High, BIT0);
		}


		static int CB_43() {/* BIT 0, E */
			return Bit(DE.Low, BIT0);
		}


		static int CB_44() {/* BIT 0, H */
			return Bit(HL.High, BIT0);
		}


		static int CB_45() {/* BIT 0, L */
			return Bit(HL.Low, BIT0);
		}


		static int CB_46() {/* BIT 0, (HL) */
			Bit(VGA.PEEK8(AdrCB), BIT0);
			AF.Low &= N_FLAG_5 & N_FLAG_3;
			AF.Low |= (byte)(MemPtr.High & (FLAG_5 | FLAG_3));
			return (3);
		}


		static int CB_47() {/* BIT 0, A */
			return Bit(AF.High, BIT0);
		}


		static int CB_48() {/* BIT 1, B */
			return Bit(BC.High, BIT1);
		}


		static int CB_49() {/* BIT 1, C */
			return Bit(BC.Low, BIT1);
		}


		static int CB_4A() {/* BIT 1, D */
			return Bit(DE.High, BIT1);
		}


		static int CB_4B() {/* BIT 1, E */
			return Bit(DE.Low, BIT1);
		}


		static int CB_4C() {/* BIT 1, H */
			return Bit(HL.High, BIT1);
		}


		static int CB_4D() {/* BIT 1, L */
			return Bit(HL.Low, BIT1);
		}


		static int CB_4E() {/* BIT 1, (HL) */
			Bit(VGA.PEEK8(AdrCB), BIT1);
			AF.Low &= N_FLAG_5 & N_FLAG_3;
			AF.Low |= (byte)(MemPtr.High & (FLAG_5 | FLAG_3));
			return (3);
		}


		static int CB_4F() {/* BIT 1, A */
			return Bit(AF.High, BIT1);
		}


		static int CB_50() {/* BIT 2, B */
			return Bit(BC.High, BIT2);
		}


		static int CB_51() {/* BIT 2, C */
			return Bit(BC.Low, BIT2);
		}


		static int CB_52() {/* BIT 2, D */
			return Bit(DE.High, BIT2);
		}


		static int CB_53() {/* BIT 2, E */
			return Bit(DE.Low, BIT2);
		}


		static int CB_54() {/* BIT 2, H */
			return Bit(HL.High, BIT2);
		}


		static int CB_55() {/* BIT 2, L */
			return Bit(HL.Low, BIT2);
		}


		static int CB_56() {/* BIT 2, (HL) */
			Bit(VGA.PEEK8(AdrCB), BIT2);
			AF.Low &= N_FLAG_5 & N_FLAG_3;
			AF.Low |= (byte)(MemPtr.High & (FLAG_5 | FLAG_3));
			return (3);
		}


		static int CB_57() {/* BIT 2, A */
			return Bit(AF.High, BIT2);
		}


		static int CB_58() {/* BIT 3, B */
			return Bit(BC.High, BIT3);
		}


		static int CB_59() {/* BIT 3, C */
			return Bit(BC.Low, BIT3);
		}


		static int CB_5A() {/* BIT 3, D */
			return Bit(DE.High, BIT3);
		}


		static int CB_5B() {/* BIT 3, E */
			return Bit(DE.Low, BIT3);
		}


		static int CB_5C() {/* BIT 3, H */
			return Bit(HL.High, BIT3);
		}


		static int CB_5D() {/* BIT 3, L */
			return Bit(HL.Low, BIT3);
		}


		static int CB_5E() {/* BIT 3, (HL) */
			Bit(VGA.PEEK8(AdrCB), BIT3);
			AF.Low &= N_FLAG_5 & N_FLAG_3; ;
			AF.Low |= (byte)(MemPtr.High & (FLAG_5 | FLAG_3));
			return (3);
		}


		static int CB_5F() {/* BIT 3, A */
			return Bit(AF.High, BIT3);
		}


		static int CB_60() {/* BIT 4, B */
			return Bit(BC.High, BIT4);
		}


		static int CB_61() {/* BIT 4, C */
			return Bit(BC.Low, BIT4);
		}


		static int CB_62() {/* BIT 4, D */
			return Bit(DE.High, BIT4);
		}


		static int CB_63() {/* BIT 4, E */
			return Bit(DE.Low, BIT4);
		}


		static int CB_64() {/* BIT 4, H */
			return Bit(HL.High, BIT4);
		}


		static int CB_65() {/* BIT 4, L */
			return Bit(HL.Low, BIT4);
		}


		static int CB_66() {/* BIT 4, (HL) */
			Bit(VGA.PEEK8(AdrCB), BIT4);
			AF.Low &= N_FLAG_5 & N_FLAG_3;
			AF.Low |= (byte)(MemPtr.High & (FLAG_5 | FLAG_3));
			return (3);
		}


		static int CB_67() {/* BIT 4, A */
			return Bit(AF.High, BIT4);
		}


		static int CB_68() {/* BIT 5, B */
			return Bit(BC.High, BIT5);
		}


		static int CB_69() {/* BIT 5, C */
			return Bit(BC.Low, BIT5);
		}


		static int CB_6A() {/* BIT 5, D */
			return Bit(DE.High, BIT5);
		}


		static int CB_6B() {/* BIT 5, E */
			return Bit(DE.Low, BIT5);
		}


		static int CB_6C() {/* BIT 5, H */
			return Bit(HL.High, BIT5);
		}


		static int CB_6D() {/* BIT 5, L */
			return Bit(HL.Low, BIT5);
		}


		static int CB_6E() {/* BIT 5, (HL) */
			Bit(VGA.PEEK8(AdrCB), BIT5);
			AF.Low &= N_FLAG_5 & N_FLAG_3;
			AF.Low |= (byte)(MemPtr.High & (FLAG_5 | FLAG_3));
			return (3);
		}


		static int CB_6F() {/* BIT 5, A */
			return Bit(AF.High, BIT5);
		}


		static int CB_70() {/* BIT 6, B */
			return Bit(BC.High, BIT6);
		}


		static int CB_71() {/* BIT 6, C */
			return Bit(BC.Low, BIT6);
		}


		static int CB_72() {/* BIT 6, D */
			return Bit(DE.High, BIT6);
		}


		static int CB_73() {/* BIT 6, E */
			return Bit(DE.Low, BIT6);
		}


		static int CB_74() {/* BIT 6, H */
			return Bit(HL.High, BIT6);
		}


		static int CB_75() {/* BIT 6, L */
			return Bit(HL.Low, BIT6);
		}


		static int CB_76() {/* BIT 6, (HL) */
			Bit(VGA.PEEK8(AdrCB), BIT6);
			AF.Low &= N_FLAG_5 & N_FLAG_3;
			AF.Low |= (byte)(MemPtr.High & (FLAG_5 | FLAG_3));
			return (3);
		}


		static int CB_77() {/* BIT 6, A */
			return Bit(AF.High, BIT6);
		}


		static int CB_78() {/* BIT 7, B */
			return Bit(BC.High, BIT7);
		}


		static int CB_79() {/* BIT 7, C */
			return Bit(BC.Low, BIT7);
		}


		static int CB_7A() {/* BIT 7, D */
			return Bit(DE.High, BIT7);
		}


		static int CB_7B() {/* BIT 7, E */
			return Bit(DE.Low, BIT7);
		}


		static int CB_7C() {/* BIT 7, H */
			return Bit(HL.High, BIT7);
		}


		static int CB_7D() {/* BIT 7, L */
			return Bit(HL.Low, BIT7);
		}


		static int CB_7E() {/* BIT 7, (HL) */
			Bit(VGA.PEEK8(AdrCB), BIT7);
			AF.Low &= N_FLAG_5 & N_FLAG_3;
			AF.Low |= (byte)(MemPtr.High & (FLAG_5 | FLAG_3));
			return (3);
		}


		static int CB_7F() {/* BIT 7, A */
			return Bit(AF.High, BIT7);
		}

		static int CB_80() {/* RES 0, B */
			return (Res(ref BC.High, BIT0));
		}


		static int CB_81() {/* RES 0, C */
			return (Res(ref BC.Low, BIT0));
		}


		static int CB_82() {/* RES 0, D */
			return (Res(ref DE.High, BIT0));
		}


		static int CB_83() {/* RES 0, E */
			return (Res(ref DE.Low, BIT0));
		}


		static int CB_84() {/* RES 0, H */
			return (Res(ref HL.High, BIT0));
		}


		static int CB_85() {/* RES 0, L */
			return (Res(ref HL.Low, BIT0));
		}


		static int CB_86() {/* RES 0, ( HL ) */
			VGA.POKE8(AdrCB, (byte)(VGA.PEEK8(AdrCB) & ~BIT0));
			return (4);
		}


		static int CB_87() {/* RES 0, A */
			return (Res(ref AF.High, BIT0));
		}


		static int CB_88() {/* RES 1, B */
			return (Res(ref BC.High, BIT1));
		}


		static int CB_89() {/* RES 1, C */
			return (Res(ref BC.Low, BIT1));
		}


		static int CB_8A() {/* RES 1, D */
			return (Res(ref DE.High, BIT1));
		}


		static int CB_8B() {/* RES 1, E */
			return (Res(ref DE.Low, BIT1));
		}


		static int CB_8C() {/* RES 1, H */
			return (Res(ref HL.High, BIT1));
		}


		static int CB_8D() {/* RES 1, L */
			return (Res(ref HL.Low, BIT1));
		}


		static int CB_8E() {/* RES 1, ( HL ) */
			VGA.POKE8(AdrCB, (byte)(VGA.PEEK8(AdrCB) & ~BIT1));
			return (4);
		}


		static int CB_8F() {/* RES 1, A */
			return (Res(ref AF.High, BIT1));
		}


		static int CB_90() {/* RES 2, B */
			return (Res(ref BC.High, BIT2));
		}


		static int CB_91() {/* RES 2, C */
			return (Res(ref BC.Low, BIT2));
		}


		static int CB_92() {/* RES 2, D */
			return (Res(ref DE.High, BIT2));
		}


		static int CB_93() {/* RES 2, E */
			return (Res(ref DE.Low, BIT2));
		}


		static int CB_94() {/* RES 2, H */
			return (Res(ref HL.High, BIT2));
		}


		static int CB_95() {/* RES 2, L */
			return (Res(ref HL.Low, BIT2));
		}


		static int CB_96() {/* RES 2, ( HL ) */
			VGA.POKE8(AdrCB, (byte)(VGA.PEEK8(AdrCB) & ~BIT2));
			return (4);
		}


		static int CB_97() {/* RES 2, A */
			return (Res(ref AF.High, BIT2));
		}


		static int CB_98() {/* RES 3, B */
			return (Res(ref BC.High, BIT3));
		}


		static int CB_99() {/* RES 3, C */
			return (Res(ref BC.Low, BIT3));
		}


		static int CB_9A() {/* RES 3, D */
			return (Res(ref DE.High, BIT3));
		}


		static int CB_9B() {/* RES 3, E */
			return (Res(ref DE.Low, BIT3));
		}


		static int CB_9C() {/* RES 3, H */
			return (Res(ref HL.High, BIT3));
		}


		static int CB_9D() {/* RES 3, L */
			return (Res(ref HL.Low, BIT3));
		}


		static int CB_9E() {/* RES 3, ( HL ) */
			VGA.POKE8(AdrCB, (byte)(VGA.PEEK8(AdrCB) & ~BIT3));
			return (4);
		}


		static int CB_9F() {/* RES 3, A */
			return (Res(ref AF.High, BIT3));
		}


		static int CB_A0() {/* RES 4, B */
			return (Res(ref BC.High, BIT4));
		}


		static int CB_A1() {/* RES 4, C */
			return (Res(ref BC.Low, BIT4));
		}


		static int CB_A2() {/* RES 4, D */
			return (Res(ref DE.High, BIT4));
		}


		static int CB_A3() {/* RES 4, E */
			return (Res(ref DE.Low, BIT4));
		}


		static int CB_A4() {/* RES 4, H */
			return (Res(ref HL.High, BIT4));
		}


		static int CB_A5() {/* RES 4, L */
			return (Res(ref HL.Low, BIT4));
		}


		static int CB_A6() {/* RES 4, ( HL ) */
			VGA.POKE8(AdrCB, (byte)(VGA.PEEK8(AdrCB) & ~BIT4));
			return (4);
		}


		static int CB_A7() {/* RES 4, A */
			return (Res(ref AF.High, BIT4));
		}


		static int CB_A8() {/* RES 5, B */
			return (Res(ref BC.High, BIT5));
		}


		static int CB_A9() {/* RES 5, C */
			return (Res(ref BC.Low, BIT5));
		}


		static int CB_AA() {/* RES 5, D */
			return (Res(ref DE.High, BIT5));
		}


		static int CB_AB() {/* RES 5, E */
			return (Res(ref DE.Low, BIT5));
		}


		static int CB_AC() {/* RES 5, H */
			return (Res(ref HL.High, BIT5));
		}


		static int CB_AD() {/* RES 5, L */
			return (Res(ref HL.Low, BIT5));
		}


		static int CB_AE() {/* RES 5, ( HL ) */
			VGA.POKE8(AdrCB, (byte)(VGA.PEEK8(AdrCB) & ~BIT5));
			return (4);
		}


		static int CB_AF() {/* RES 5, A */
			return (Res(ref AF.High, BIT5));
		}


		static int CB_B0() {/* RES 6, B */
			return (Res(ref BC.High, BIT6));
		}


		static int CB_B1() {/* RES 6, C */
			return (Res(ref BC.Low, BIT6));
		}


		static int CB_B2() {/* RES 6, D */
			return (Res(ref DE.High, BIT6));
		}


		static int CB_B3() {/* RES 6, E */
			return (Res(ref DE.Low, BIT6));
		}


		static int CB_B4() {/* RES 6, H */
			return (Res(ref HL.High, BIT6));
		}


		static int CB_B5() {/* RES 6, L */
			return (Res(ref HL.Low, BIT6));
		}


		static int CB_B6() {/* RES 6, ( HL ) */
			VGA.POKE8(AdrCB, (byte)(VGA.PEEK8(AdrCB) & ~BIT6));
			return (4);
		}


		static int CB_B7() {/* RES 6, A */
			return (Res(ref AF.High, BIT6));
		}


		static int CB_B8() {/* RES 7, B */
			return (Res(ref BC.High, BIT7));
		}


		static int CB_B9() {/* RES 7, C */
			return (Res(ref BC.Low, BIT7));
		}


		static int CB_BA() {/* RES 7, D */
			return (Res(ref DE.High, BIT7));
		}


		static int CB_BB() {/* RES 7, E */
			return (Res(ref DE.Low, BIT7));
		}


		static int CB_BC() {/* RES 7, H */
			return (Res(ref HL.High, BIT7));
		}


		static int CB_BD() {/* RES 7, L */
			return (Res(ref HL.Low, BIT7));
		}


		static int CB_BE() {/* RES 7, ( HL ) */
			VGA.POKE8(AdrCB, (byte)(VGA.PEEK8(AdrCB) & ~BIT7));
			return (4);
		}


		static int CB_BF() {/* RES 7, A */
			return (Res(ref AF.High, BIT7));
		}


		static int CB_C0() {/* SET 0, B */
			return (Set(ref BC.High, BIT0));
		}


		static int CB_C1() {/* SET 0, C */
			return (Set(ref BC.Low, BIT0));
		}


		static int CB_C2() {/* SET 0, D */
			return (Set(ref DE.High, BIT0));
		}


		static int CB_C3() {/* SET 0, E */
			return (Set(ref DE.Low, BIT0));
		}


		static int CB_C4() {/* SET 0, H */
			return (Set(ref HL.High, BIT0));
		}


		static int CB_C5() {/* SET 0, L */
			return (Set(ref HL.Low, BIT0));
		}


		static int CB_C6() {/* SET 0, ( HL ) */
			VGA.POKE8(AdrCB, (byte)(VGA.PEEK8(AdrCB) | BIT0));
			return (4);
		}


		static int CB_C7() {/* SET 0, A */
			return (Set(ref AF.High, BIT0));
		}


		static int CB_C8() {/* SET 1, B */
			return (Set(ref BC.High, BIT1));
		}


		static int CB_C9() {/* SET 1, C */
			return (Set(ref BC.Low, BIT1));
		}


		static int CB_CA() {/* SET 1, D */
			return (Set(ref DE.High, BIT1));
		}


		static int CB_CB() {/* SET 1, E */
			return (Set(ref DE.Low, BIT1));
		}


		static int CB_CC() {/* SET 1, H */
			return (Set(ref HL.High, BIT1));
		}


		static int CB_CD() {/* SET 1, L */
			return (Set(ref HL.Low, BIT1));
		}


		static int CB_CE() {/* SET 1, ( HL ) */
			VGA.POKE8(AdrCB, (byte)(VGA.PEEK8(AdrCB) | BIT1));
			return (4);
		}


		static int CB_CF() {/* SET 1, A */
			return (Set(ref AF.High, BIT1));
		}


		static int CB_D0() {/* SET 2, B */
			return (Set(ref BC.High, BIT2));
		}


		static int CB_D1() {/* SET 2, C */
			return (Set(ref BC.Low, BIT2));
		}


		static int CB_D2() {/* SET 2, D */
			return (Set(ref DE.High, BIT2));
		}


		static int CB_D3() {/* SET 2, E */
			return (Set(ref DE.Low, BIT2));
		}


		static int CB_D4() {/* SET 2, H */
			return (Set(ref HL.High, BIT2));
		}


		static int CB_D5() {/* SET 2, L */
			return (Set(ref HL.Low, BIT2));
		}


		static int CB_D6() {/* SET 2, ( HL ) */
			VGA.POKE8(AdrCB, (byte)(VGA.PEEK8(AdrCB) | BIT2));
			return (4);
		}


		static int CB_D7() {/* SET 2, A */
			return (Set(ref AF.High, BIT2));
		}


		static int CB_D8() {/* SET 3, B */
			return (Set(ref BC.High, BIT3));
		}


		static int CB_D9() {/* SET 3, C */
			return (Set(ref BC.Low, BIT3));
		}


		static int CB_DA() {/* SET 3, D */
			return (Set(ref DE.High, BIT3));
		}


		static int CB_DB() {/* SET 3, E */
			return (Set(ref DE.Low, BIT3));
		}


		static int CB_DC() {/* SET 3, H */
			return (Set(ref HL.High, BIT3));
		}


		static int CB_DD() {/* SET 3, L */
			return (Set(ref HL.Low, BIT3));
		}


		static int CB_DE() {/* SET 3, ( HL ) */
			VGA.POKE8(AdrCB, (byte)(VGA.PEEK8(AdrCB) | BIT3));
			return (4);
		}


		static int CB_DF() {/* SET 3, A */
			return (Set(ref AF.High, BIT3));
		}


		static int CB_E0() {/* SET 4, B */
			return (Set(ref BC.High, BIT4));
		}


		static int CB_E1() {/* SET 4, C */
			return (Set(ref BC.Low, BIT4));
		}


		static int CB_E2() {/* SET 4, D */
			return (Set(ref DE.High, BIT4));
		}


		static int CB_E3() {/* SET 4, E */
			return (Set(ref DE.Low, BIT4));
		}


		static int CB_E4() {/* SET 4, H */
			return (Set(ref HL.High, BIT4));
		}


		static int CB_E5() {/* SET 4, L */
			return (Set(ref HL.Low, BIT4));
		}


		static int CB_E6() {/* SET 4, ( HL ) */
			VGA.POKE8(AdrCB, (byte)(VGA.PEEK8(AdrCB) | BIT4));
			return (4);
		}


		static int CB_E7() {/* SET 4, A */
			return (Set(ref AF.High, BIT4));
		}


		static int CB_E8() {/* SET 5, B */
			return (Set(ref BC.High, BIT5));
		}


		static int CB_E9() {/* SET 5, C */
			return (Set(ref BC.Low, BIT5));
		}


		static int CB_EA() {/* SET 5, D */
			return (Set(ref DE.High, BIT5));
		}


		static int CB_EB() {/* SET 5, E */
			return (Set(ref DE.Low, BIT5));
		}


		static int CB_EC() {/* SET 5, H */
			return (Set(ref HL.High, BIT5));
		}


		static int CB_ED() {/* SET 5, L */
			return (Set(ref HL.Low, BIT5));
		}


		static int CB_EE() {/* SET 5, ( HL ) */
			VGA.POKE8(AdrCB, (byte)(VGA.PEEK8(AdrCB) | BIT5));
			return (4);
		}


		static int CB_EF() {/* SET 5, A */
			return (Set(ref AF.High, BIT5));
		}


		static int CB_F0() {/* SET 6, B */
			return (Set(ref BC.High, BIT6));
		}


		static int CB_F1() {/* SET 6, C */
			return (Set(ref BC.Low, BIT6));
		}


		static int CB_F2() {/* SET 6, D */
			return (Set(ref DE.High, BIT6));
		}


		static int CB_F3() {/* SET 6, E */
			return (Set(ref DE.Low, BIT6));
		}


		static int CB_F4() {/* SET 6, H */
			return (Set(ref HL.High, BIT6));
		}


		static int CB_F5() {/* SET 6, L */
			return (Set(ref HL.Low, BIT6));
		}


		static int CB_F6() {/* SET 6, ( HL ) */
			VGA.POKE8(AdrCB, (byte)(VGA.PEEK8(AdrCB) | BIT6));
			return (4);
		}


		static int CB_F7() {/* SET 6, A */
			return (Set(ref AF.High, BIT6));
		}


		static int CB_F8() {/* SET 7, B */
			return (Set(ref BC.High, BIT7));
		}


		static int CB_F9() {/* SET 7, C */
			return (Set(ref BC.Low, BIT7));
		}


		static int CB_FA() {/* SET 7, D */
			return (Set(ref DE.High, BIT7));
		}


		static int CB_FB() {/* SET 7, E */
			return (Set(ref DE.Low, BIT7));
		}


		static int CB_FC() {/* SET 7, H */
			return (Set(ref HL.High, BIT7));
		}


		static int CB_FD() {/* SET 7, L */
			return (Set(ref HL.Low, BIT7));
		}


		static int CB_FE() {/* SET 7, ( HL ) */
			VGA.POKE8(AdrCB, (byte)(VGA.PEEK8(AdrCB) | BIT7));
			return (4);
		}


		static int CB_FF() {/* SET 7, A */
			return (Set(ref AF.High, BIT7));
		}


		/************
	   * OPCODE ED *
	   ************/

		static int ed___() {
			return (2);
		}


		static int ED_40() {/* IN B, ( C ) */
			BC.High = (byte)GestPort.ReadPort(BC.Word);
			AF.Low = (byte)((AF.Low & FLAG_C) | Parite[BC.High]);
			return (4);
		}


		static int ED_41() {/* OUT ( C ), B */
			GestPort.WritePort(BC.Word, BC.High);
			return (4);
		}


		static int ED_42() {/* SBC HL, BC */
			return SBC_R16(BC.Word);
		}


		static int ED_43() {/* LD ( nnnn ), BC */
			MemPtr.Word = VGA.PEEK16(PC.Word);
			VGA.POKE16(MemPtr.Word, BC.Word);
			MemPtr.Word++;
			PC.Word += 2;
			return (6);
		}


		static int ED_44() {/* NEG */
			int a = AF.High;
			AF.High = 0;
			SUB_R8(a, 0);
			return (2);
		}


		static int ED_45() {/* RETN */
			IFF1 = IFF2;
			MemPtr.Word = PC.Word = POP();
			return (4);
		}


		static int ED_46() {/* IM 0 */
			InterruptMode = 0;
			ed___();
			return (2);
		}


		static int ED_47() {/* LD I, A */
			IR.High = AF.High;
			SupIrqWaitState = 1;
			return (3);
		}


		static int ED_48() {/* IN C, ( C ) */
			BC.Low = (byte)GestPort.ReadPort(BC.Word);
			AF.Low = (byte)((AF.Low & FLAG_C) | Parite[BC.Low]);
			return (4);
		}


		static int ED_49() {/* OUT ( C ), C */
			GestPort.WritePort(BC.Word, BC.Low);
			return (4);
		}


		static int ED_4A() {/* ADC HL, BC */
			return ADC_R16(BC.Word);
		}


		static int ED_4B() {/* LD BC, ( nnnn ) */
			MemPtr.Word = VGA.PEEK16(PC.Word);
			BC.Word = VGA.PEEK16(MemPtr.Word);
			MemPtr.Word++;
			PC.Word += 2;
			return (6);
		}


		static int ED_4D() {/* RETI */
			IFF1 = IFF2;
			MemPtr.Word = PC.Word = POP();
			return (4);
		}


		static int ED_4F() {/* LD R, A */
			IR.Low = AF.High;
			SupIrqWaitState = 1;
			return (3);
		}


		static int ED_50() {/* IN D, ( C ) */
			DE.High = (byte)GestPort.ReadPort(BC.Word);
			AF.Low = (byte)((AF.Low & FLAG_C) | Parite[DE.High]);
			return (4);
		}


		static int ED_51() {/* OUT ( C ), D */
			GestPort.WritePort(BC.Word, DE.High);
			return (4);
		}


		static int ED_52() {/* SBC HL, DE */
			return SBC_R16(DE.Word);
		}


		static int ED_53() {/* LD ( nnnn ), DE */
			MemPtr.Word = VGA.PEEK16(PC.Word);
			VGA.POKE16(MemPtr.Word, DE.Word);
			MemPtr.Word++;
			PC.Word += 2;
			return (6);
		}


		static int ED_56() {/* IM 1 */
			InterruptMode = 1;
			return (2);
		}


		static int ED_57() {/* LD A, I */
			AF.High = IR.High;
			SupIrqWaitState = 1;
			AF.Low = (byte)((AF.Low & FLAG_C) | (AF.High != 0 ? (AF.High & FLAG_S) : FLAG_Z) | (AF.High & (FLAG_5 | FLAG_3)) | (IFF2 != 0 ? FLAG_V : 0));
			return (3);
		}


		static int ED_58() {/* IN E, ( C ) */
			DE.Low = (byte)GestPort.ReadPort(BC.Word);
			AF.Low = (byte)((AF.Low & FLAG_C) | Parite[DE.Low]);
			return (4);
		}


		static int ED_59() {/* OUT ( C ), E */
			GestPort.WritePort(BC.Word, DE.Low);
			return (4);
		}


		static int ED_5A() {/* ADC HL, DE */
			return ADC_R16(DE.Word);
		}


		static int ED_5B() {/* LD DE, ( nnnn ) */
			MemPtr.Word = VGA.PEEK16(PC.Word);
			DE.Word = VGA.PEEK16(MemPtr.Word);
			MemPtr.Word++;
			PC.Word += 2;
			return (6);
		}


		static int ED_5E() {/* IM 2 */
			InterruptMode = 2;
			return (2);
		}


		static int ED_5F() {/* LD A, R */
			AF.High = IR.Low;
			SupIrqWaitState = 1;
			AF.Low = (byte)((AF.Low & FLAG_C) | (AF.High != 0 ? (AF.High & FLAG_S) : FLAG_Z) | (AF.High & (FLAG_5 | FLAG_3)) | (IFF2 != 0 ? FLAG_V : 0));
			return (3);
		}


		static int ED_60() {/* IN H, ( C ) */
			HL.High = (byte)GestPort.ReadPort(BC.Word);
			AF.Low = (byte)((AF.Low & FLAG_C) | Parite[HL.High]);
			return (4);
		}


		static int ED_61() {/* OUT ( C ), H */
			GestPort.WritePort(BC.Word, HL.High);
			return (4);
		}


		static int ED_62() {/* SBC HL, HL */
			return SBC_R16(HL.Word);
		}

		static int ED_63() {/* LD ( nnnn ), HL */
			return (1 + ___22());  // Identique à l'instruction #22
		}


		static int ED_67() {/* RRD */
			int a = AF.High;
			int hl = VGA.PEEK8(HL.Word);
			MemPtr.Word = (ushort)(HL.Word + 1);
			AF.High = (byte)((a & 0xF0) | (hl & 0xF));
			VGA.POKE8(HL.Word, (byte)((hl >> 4) | (a << 4)));
			AF.Low = (byte)((AF.Low & FLAG_C) | Parite[AF.High]);
			return (5);
		}


		static int ED_68() {/* IN L, ( C ) */
			HL.Low = (byte)GestPort.ReadPort(BC.Word);
			AF.Low = (byte)((AF.Low & FLAG_C) | Parite[HL.Low]);
			return (4);
		}


		static int ED_69() {/* OUT ( C ), L */
			GestPort.WritePort(BC.Word, HL.Low);
			return (4);
		}


		static int ED_6A() {/* ADC HL, HL */
			return ADC_R16(HL.Word);
		}


		static int ED_6B() {/* LD HL, ( nnnn ) */
			return (1 + ___2A()); // Identique à l'instruction #2A
		}


		static int ED_6F() {/* RLD */
			int a = AF.High;
			int hl = VGA.PEEK8(HL.Word);
			MemPtr.Word = (ushort)(HL.Word + 1);
			AF.High = (byte)((a & 0xF0) | (hl >> 4));
			VGA.POKE8(HL.Word, (byte)((hl << 4) | (a & 0xF)));
			AF.Low = (byte)((AF.Low & FLAG_C) | Parite[AF.High]);
			return (5);
		}


		static int ED_70() {/* IN F, ( C ) */
			int Tmp = GestPort.ReadPort(BC.Word);
			AF.Low = (byte)((AF.Low & FLAG_C) | Parite[Tmp]);
			return (4);
		}


		static int ED_71() {/* OUT ( C ), 0 */
			GestPort.WritePort(BC.Word, 0);
			return (4);
		}


		static int ED_72() {/* SBC HL, SP */
			return SBC_R16(SP.Word);
		}


		static int ED_73() {/* LD ( nnnn ), SP */
			MemPtr.Word = VGA.PEEK16(PC.Word);
			VGA.POKE16(MemPtr.Word, SP.Word);
			MemPtr.Word++;
			PC.Word += 2;
			return (6);
		}


		static int ED_78() {/* IN A, ( C ) */
			AF.High = (byte)GestPort.ReadPort(BC.Word);
			MemPtr.Word = (ushort)(BC.Word + 1);
			AF.Low = (byte)((AF.Low & FLAG_C) | Parite[AF.High]);
			return (4);
		}


		static int ED_79() {/* OUT ( C ), A */
			GestPort.WritePort(BC.Word, AF.High);
			MemPtr.Word = (ushort)(BC.Word + 1);
			return (4);
		}


		static int ED_7A() {/* ADC HL, SP */
			return ADC_R16(SP.Word);
		}


		static int ED_7B() {/* LD SP, ( nnnn ) */
			MemPtr.Word = VGA.PEEK16(PC.Word);
			SP.Word = VGA.PEEK16(MemPtr.Word);
			MemPtr.Word++;
			PC.Word += 2;
			return (6);
		}


		static int ED_A0() {/* LDI */
			int v = VGA.PEEK8(HL.Word++);
			VGA.POKE8(DE.Word++, (byte)v);
			AF.Low &= N_FLAG_H & N_FLAG_V & N_FLAG_N & N_FLAG_5 & N_FLAG_3;
			v += AF.High;
			AF.Low |= (byte)(((v << 4) & FLAG_5) | (v & FLAG_3));
			if (--BC.Word != 0)
				AF.Low |= FLAG_V;

			SupIrqWaitState = 1;
			return (5);
		}


		static int ED_A1() {/* CPI */
			int v = VGA.PEEK8(HL.Word++);
			MemPtr.Word++;
			int t = AF.High - v;
			AF.Low = (byte)((AF.Low & FLAG_C) | FLAG_N | (t & FLAG_S) | ((t & 0xFF) != 0 ? 0 : FLAG_Z) | ((AF.High ^ v ^ t) & FLAG_H) | (--BC.Word != 0 ? FLAG_V : 0));
			if ((AF.Low & FLAG_H) != 0)
				t--;

			AF.Low |= (byte)(((t << 4) & FLAG_5) | (t & FLAG_3));
			return (4);
		}


		static int ED_A2() {/* INI */
			VGA.POKE8(HL.Word++, (byte)GestPort.ReadPort(BC.Word));
			MemPtr.Word = (ushort)(BC.Word + 1);
			AF.Low = FLAG_S | FLAG_V | FLAG_N;
			if (--BC.High == 0)
				AF.Low |= FLAG_Z;

			return (5);
		}


		static int ED_A3() {/* OUTI */
			AF.Low &= N_FLAG_H & N_FLAG_Z;
			if (--BC.High == 0)
				AF.Low |= FLAG_Z;

			MemPtr.Word = (ushort)(BC.Word + 1);
			VGA.DelayGa = 1;
			GestPort.WritePort(BC.Word, VGA.PEEK8(HL.Word++));
			return (5);
		}


		static int ED_A8() {/* LDD */
			int v = VGA.PEEK8(HL.Word--);
			VGA.POKE8(DE.Word--, (byte)v);
			AF.Low &= N_FLAG_H & N_FLAG_V & N_FLAG_N & N_FLAG_5 & N_FLAG_3;
			AF.Low |= (byte)((((AF.High + v) << 4) & FLAG_5) | ((AF.High + v) & FLAG_3));
			if (--BC.Word != 0)
				AF.Low |= FLAG_V;

			SupIrqWaitState = 1;
			return (5);
		}


		static int ED_A9() {/* CPD */
			int v = VGA.PEEK8(HL.Word--);
			MemPtr.Word--;
			int t = AF.High - v;
			AF.Low = (byte)((AF.Low & FLAG_C) | FLAG_N | (t & FLAG_S) | ((t & 0xFF) != 0 ? 0 : FLAG_Z) | ((AF.High ^ v ^ t) & FLAG_H) | (--BC.Word != 0 ? FLAG_V : 0));
			if ((AF.Low & FLAG_H) != 0)
				t--;

			AF.Low |= (byte)(((t << 4) & FLAG_5) | (t & FLAG_3));
			return (4);
		}


		static int ED_AA() {/* IND */
			AF.Low = FLAG_N | FLAG_Z;
			VGA.POKE8(HL.Word--, (byte)GestPort.ReadPort(BC.Word));
			MemPtr.Word = (ushort)(BC.Word - 1);
			if (--BC.High != 0)
				AF.Low &= N_FLAG_Z;

			return (5);
		}


		static int ED_AB() {/* OUTD */
			AF.Low = FLAG_N | FLAG_Z;
			if (--BC.High != 0)
				AF.Low &= N_FLAG_Z;

			MemPtr.Word = (ushort)(BC.Word - 1);
			VGA.DelayGa = 1;
			GestPort.WritePort(BC.Word, VGA.PEEK8(HL.Word--));
			return (5);
		}


		static int ED_B0() {/* LDIR */
			int r = 5;
			int v = VGA.PEEK8(HL.Word++);
			VGA.POKE8(DE.Word++, (byte)v);
			AF.Low &= N_FLAG_H & N_FLAG_V & N_FLAG_N & N_FLAG_5 & N_FLAG_3;
			AF.Low |= (byte)((((AF.High + v) << 4) & FLAG_5) | ((AF.High + v) & FLAG_3));
			if (--BC.Word != 0) {
				AF.Low |= FLAG_V;
				r++;
				PC.Word -= 2;
				MemPtr.Word = (ushort)(PC.Word + 1);
			}
			SupIrqWaitState = 1;
			return (r);
		}


		static int ED_B1() {/* CPIR */
			int r = 4;
			byte i = VGA.PEEK8(HL.Word++);
			byte tmp = (byte)(AF.High - i);
			MemPtr.Word++;
			BC.Word--;
			AF.Low = (byte)(FLAG_N | (AF.Low & FLAG_C) | (tmp & FLAG_S) | (tmp != 0 ? 0 : FLAG_Z) | ((AF.High ^ i ^ tmp) & FLAG_H) | (BC.Word != 0 ? FLAG_V : 0));
			if (BC.Word != 0 && tmp != 0) {
				SupIrqWaitState = 1;
				r += 2;
				PC.Word -= 2;
				MemPtr.Word = (ushort)(PC.Word + 1);
			}
			return (r);
		}


		static int ED_B2() {/* INIR */
			int r = 5;
			VGA.POKE8(HL.Word++, (byte)GestPort.ReadPort(BC.Word));
			MemPtr.Word = (ushort)(BC.Word + 1);
			AF.Low = FLAG_S | FLAG_V | FLAG_N | FLAG_Z;
			if (--BC.High != 0) {
				AF.Low &= N_FLAG_Z;
				PC.Word -= 2;
				r++;
			}
			return (r);
		}


		static int ED_B3() {/* OTIR */
			int r = 5;
			AF.Low = (byte)((AF.Low | FLAG_Z) & N_FLAG_H);
			MemPtr.Word = (ushort)(BC.Word + 1);
			VGA.DelayGa = 1;
			if (--BC.High != 0) {
				AF.Low &= N_FLAG_Z;
				PC.Word -= 2;
				r++;
				VGA.DelayGa++;
			}
			GestPort.WritePort(BC.Word, VGA.PEEK8(HL.Word++));
			return (r);
		}


		static int ED_B8() {/* LDDR */
			int r = 5;
			int v = VGA.PEEK8(HL.Word--);
			VGA.POKE8(DE.Word--, (byte)v);
			AF.Low &= N_FLAG_H & N_FLAG_V & N_FLAG_N & N_FLAG_5 & N_FLAG_3;
			AF.Low |= (byte)((((AF.High + v) << 4) & FLAG_5) | ((AF.High + v) & FLAG_3));
			if ((--BC.Word != 0)) {
				AF.Low |= FLAG_V;
				r++;
				PC.Word -= 2;
				MemPtr.Word = (ushort)(PC.Word + 1);
			}
			SupIrqWaitState = 1;
			return (r);
		}


		static int ED_B9() {/* CPDR */
			int r = 4;
			byte i = VGA.PEEK8(HL.Word--);
			byte tmp = (byte)(AF.High - i);
			MemPtr.Word--;
			BC.Word--;
			AF.Low = (byte)(FLAG_N | (AF.Low & FLAG_C) | (tmp & FLAG_S) | (tmp != 0 ? 0 : FLAG_Z) | ((AF.High ^ i ^ tmp) & FLAG_H) | (BC.Word != 0 ? FLAG_V : 0));
			if (BC.Word != 0 && tmp != 0) {
				SupIrqWaitState = 1;
				r += 2;
				PC.Word -= 2;
				MemPtr.Word = (ushort)(PC.Word + 1);
			}
			return (r);
		}


		static int ED_BA() {/* INDR */
			int r = 5;
			AF.Low = FLAG_N | FLAG_Z;
			VGA.POKE8(HL.Word--, (byte)GestPort.ReadPort(BC.Word));
			MemPtr.Word = (ushort)(BC.Word - 1);
			if (--BC.High != 0) {
				AF.Low &= N_FLAG_Z;
				PC.Word -= 2;
				r++;
			}
			return (r);
		}


		static int ED_BB() {/* OTDR */
			int r = 5;
			MemPtr.Word = (ushort)(PC.Word - 1);
			AF.Low = FLAG_N | FLAG_Z;
			VGA.DelayGa = 1;
			if (--BC.High != 0) {
				AF.Low &= N_FLAG_Z;
				PC.Word -= 2;
				r++;
				VGA.DelayGa++;
			}
			GestPort.WritePort(BC.Word, VGA.PEEK8(HL.Word--));
			return (r);
		}


		/************
	   * OPCODE DD *
	   ************/

		static int dd___() {
			return (1);
		}


		static ushort GetIXdd() {
			int ofs = VGA.PEEK8(PC.Word++);
			MemPtr.Word = (ushort)(IX.Word + (ofs > 127 ? ofs - 256 : ofs));
			return MemPtr.Word;
		}


		static int DD_09() {/* ADD IX, BC */
			return ADD_R16(ref IX.Word, BC.Word);
		}


		static int DD_19() {/* ADD IX, DE */
			return ADD_R16(ref IX.Word, DE.Word);
		}


		static int DD_21() {/* LD IX, nnnn */
			IX.Word = VGA.PEEK16(PC.Word);
			PC.Word += 2;
			return (3);
		}


		static int DD_22() {/* LD ( nnnn ), IX */
			MemPtr.Word = VGA.PEEK16(PC.Word);
			VGA.POKE16(MemPtr.Word, IX.Word);
			MemPtr.Word++;
			PC.Word += 2;
			return (5);
		}


		static int DD_23() {/* INC IX */
			++IX.Word;
			SupIrqWaitState = 1;
			return (2);
		}


		static int DD_24() {/* INC IXh */
			return FLAG_INC(++IX.High);
		}


		static int DD_25() {/* DEC IXh */
			return FLAG_DEC(--IX.High);
		}


		static int DD_26() {/* LD IXh, n */
			IX.High = VGA.PEEK8(PC.Word++);
			return (2);
		}


		static int DD_29() {/* ADD IX, IX */
			return ADD_R16(ref IX.Word, IX.Word);
		}


		static int DD_2A() {/* LD IX, ( nnnn ) */
			MemPtr.Word = VGA.PEEK16(PC.Word);
			IX.Word = VGA.PEEK16(MemPtr.Word);
			MemPtr.Word++;
			PC.Word += 2;
			return (5);
		}


		static int DD_2B() {/* DEC IX */
			--IX.Word;
			SupIrqWaitState = 1;
			return (2);
		}


		static int DD_2C() {/* INC IXl */
			return FLAG_INC(++IX.Low);
		}


		static int DD_2D() {/* DEC IXl */
			return FLAG_DEC(--IX.Low);
		}


		static int DD_2E() {/* LD IXl, n */
			IX.Low = VGA.PEEK8(PC.Word++);
			return (2);
		}


		static int DD_34() {/* INC (IX+n) */
			ushort ofs = GetIXdd();
			byte r = VGA.PEEK8(ofs);
			FLAG_INC(++r);
			VGA.POKE8(ofs, r);
			return (5);
		}


		static int DD_35() {/* DEC (IX+n) */
			ushort ofs = GetIXdd();
			byte r = VGA.PEEK8(ofs);
			FLAG_DEC(--r);
			VGA.POKE8(ofs, r);
			return (5);
		}


		static int DD_36() {/* LD (IX+d), n */
			ushort ofs = GetIXdd();
			VGA.POKE8(ofs, VGA.PEEK8(PC.Word++));
			return (5);
		}


		static int DD_39() {/* ADD IX, SP */
			return ADD_R16(ref IX.Word, SP.Word);
		}


		static int DD_44() {/* LD B, IXh */
			BC.High = IX.High;
			return (1);
		}


		static int DD_45() {/* LD B, IXl */
			BC.High = IX.Low;
			return (1);
		}


		static int DD_46() {/* LD B, (IX+d) */
			BC.High = VGA.PEEK8(GetIXdd());
			return (4);
		}


		static int DD_4C() {/* LD C, IXh */
			BC.Low = IX.High;
			return (1);
		}


		static int DD_4D() {/* LD C, IXl */
			BC.Low = IX.Low;
			return (1);
		}


		static int DD_4E() {/* LD C, (IX+d) */
			BC.Low = VGA.PEEK8(GetIXdd());
			return (4);
		}


		static int DD_54() {/* LD D, IXh */
			DE.High = IX.High;
			return (1);
		}


		static int DD_55() {/* LD D, IXl */
			DE.High = IX.Low;
			return (1);
		}


		static int DD_56() {/* LD D, (IX+d) */
			DE.High = VGA.PEEK8(GetIXdd());
			return (4);
		}


		static int DD_5C() {/* LD E, IXh */
			DE.Low = IX.High;
			return (1);
		}


		static int DD_5D() {/* LD E, IXl */
			DE.Low = IX.Low;
			return (1);
		}


		static int DD_5E() {/* LD E, (IX+d) */
			DE.Low = VGA.PEEK8(GetIXdd());
			return (4);
		}


		static int DD_60() {/* LD IXh, B */
			IX.High = BC.High;
			return (1);
		}


		static int DD_61() {/* LD IXh, C */
			IX.High = BC.Low;
			return (1);
		}


		static int DD_62() {/* LD IXh, D */
			IX.High = DE.High;
			return (1);
		}


		static int DD_63() {/* LD IXh, E */
			IX.High = DE.Low;
			return (1);
		}


		static int DD_65() {/* LD IXh, IXl */
			IX.High = IX.Low;
			return (1);
		}


		static int DD_66() {/* LD H, (IX+d) */
			HL.High = VGA.PEEK8(GetIXdd());
			return (4);
		}


		static int DD_67() {/* LD IXh, A */
			IX.High = AF.High;
			return (1);
		}


		static int DD_68() {/* LD IXl, B */
			IX.Low = BC.High;
			return (1);
		}


		static int DD_69() {/* LD IXl, C */
			IX.Low = BC.Low;
			return (1);
		}


		static int DD_6A() {/* LD IXl, D */
			IX.Low = DE.High;
			return (1);
		}


		static int DD_6B() {/* LD IXl, E */
			IX.Low = DE.Low;
			return (1);
		}


		static int DD_6C() {/* LD IXl, IXH */
			IX.Low = IX.High;
			return (1);
		}


		static int DD_6E() {/* LD L, (IX+d) */
			HL.Low = VGA.PEEK8(GetIXdd());
			return (4);
		}


		static int DD_6F() {/* LD IXl, A */
			IX.Low = AF.High;
			return (1);
		}


		static int DD_70() {/* LD (IX+d), B */
			VGA.POKE8(GetIXdd(), BC.High);
			return (4);
		}


		static int DD_71() {/* LD (IX+d), C */
			VGA.POKE8(GetIXdd(), BC.Low);
			return (4);
		}


		static int DD_72() {/* LD (IX+d), D */
			VGA.POKE8(GetIXdd(), DE.High);
			return (4);
		}


		static int DD_73() {/* LD (IX+d), E */
			VGA.POKE8(GetIXdd(), DE.Low);
			return (4);
		}


		static int DD_74() {/* LD (IX+d), H */
			VGA.POKE8(GetIXdd(), HL.High);
			return (4);
		}


		static int DD_75() {/* LD (IX+d), L */
			VGA.POKE8(GetIXdd(), HL.Low);
			return (4);
		}


		static int DD_77() {/* LD (IX+d), A */
			VGA.POKE8(GetIXdd(), AF.High);
			return (4);
		}


		static int DD_7C() {/* LD A, IXh */
			AF.High = IX.High;
			return (1);
		}


		static int DD_7D() {/* LD A, IXl */
			AF.High = IX.Low;
			return (1);
		}


		static int DD_7E() {/* LD A, (IX+d) */
			AF.High = VGA.PEEK8(GetIXdd());
			return (4);
		}


		static int DD_84() {/* ADD A, IXh */
			return ADD_R8(IX.High, 0);
		}


		static int DD_85() {/* ADD A, IXl */
			return ADD_R8(IX.Low, 0);
		}


		static int DD_86() {/* ADD A, (IX+n) */
			ADD_R8(VGA.PEEK8(GetIXdd()), 0);
			return (4);
		}


		static int DD_8C() {/* ADC A, IXh */
			return ADD_R8(IX.High, AF.Low);
		}


		static int DD_8D() {/* ADC A, IXl */
			return ADD_R8(IX.Low, AF.Low);
		}


		static int DD_8E() {/* ADC A, (IX+n) */
			ADD_R8(VGA.PEEK8(GetIXdd()), AF.Low);
			return (4);
		}


		static int DD_94() {/* SUB IXh */
			return SUB_R8(IX.High, 0);
		}


		static int DD_95() {/* SUB IXl */
			return SUB_R8(IX.Low, 0);
		}


		static int DD_96() {/* SUB (IX+n) */
			SUB_R8(VGA.PEEK8(GetIXdd()), 0);
			return (4);
		}


		static int DD_9C() {/* SBC A, IXh */
			return SUB_R8(IX.High, AF.Low);
		}


		static int DD_9D() {/* SBC A, IXl */
			return SUB_R8(IX.Low, AF.Low);
		}


		static int DD_9E() {/* SBC A, (IX+n) */
			SUB_R8(VGA.PEEK8(GetIXdd()), AF.Low);
			return (4);
		}


		static int DD_A4() {/* AND IXh */
			return AND_R8(IX.High);
		}


		static int DD_A5() {/* AND IXl */
			return AND_R8(IX.Low);
		}


		static int DD_A6() {/* AND (IX+n) */
			AND_R8(VGA.PEEK8(GetIXdd()));
			return (4);
		}


		static int DD_AC() {/* XOR IXh */
			return XOR_R8(IX.High);
		}


		static int DD_AD() {/* XOR IXl */
			return XOR_R8(IX.Low);
		}


		static int DD_AE() {/* XOR (IX+n) */
			XOR_R8(VGA.PEEK8(GetIXdd()));
			return (4);
		}


		static int DD_B4() {/* OR IXh */
			return OR_R8(AF.High);
		}


		static int DD_B5() {/* OR IXl */
			return OR_R8(IX.Low);
		}


		static int DD_B6() {/* OR (IX+n) */
			OR_R8(VGA.PEEK8(GetIXdd()));
			return (4);
		}


		static int DD_BC() {/* CP IXh */
			return CP_R8(IX.High);
		}


		static int DD_BD() {/* CP IXl */
			return CP_R8(IX.Low);
		}


		static int DD_BE() {/* CP (IX+n) */
			CP_R8(VGA.PEEK8(GetIXdd()));
			return (4);
		}


		static int DD_CB() {/* special code CB */
			int k;
			AdrCB = (ushort)GetIXdd();
			k = VGA.PEEK8(PC.Word++);
			//Log(MODULENAME, "Instruction #DD,#CB,#%02X,#%02X (PC=#%04X)", VGA.PEEK8(PC.Word - 2), k, PC.Word - 4, LOG_DEBUG);
			if (k > 0x3F && k < 0x80)
				k = (k & 0xF8) | 0x06;
			CBIndex = true;
			IR.Low = (byte)(((IR.Low + 1) & 0x7F) | (IR.Low & 0x80));
			tabCB[k]();
			CBIndex = false;
			return (k > 0x3F && k < 0x80 ? 5 : 6); // ### a vérifier...
		}


		static int DD_E1() {/* POP IX */
			IX.Word = POP();
			return (3);
		}


		static int DD_E3() {/* EX (SP), IX */
			MemPtr.Word = VGA.PEEK16(SP.Word);
			VGA.POKE16(SP.Word, IX.Word);
			IX.Word = MemPtr.Word;
			SupIrqWaitState = 1;
			return (6);
		}


		static int DD_E5() {/* PUSH IX */
			return PUSH(IX.Word);
		}


		static int DD_E9() {/* JP (IX) */
			PC = IX;
			return (1);
		}


		static int DD_F9() {/* LD SP, IX */
			SP.Word = IX.Word;
			SupIrqWaitState = 1;
			return (2);
		}


		static int DD_FD() {/* special DD_FD */
			// Se comporte commme un simple FD
			IR.Low = (byte)(((IR.Low + 1) & 0x7F) | (IR.Low & 0x80));
			//Log(MODULENAME, "Instruction #DD,#FD (PC=#%04X)", PC, LOG_DEBUG);
			return (1 + tabIY[VGA.PEEK8(PC.Word++)]()); // ### A vérifier
		}



		/************
	   * OPCODE FD *
	   ************/


		static int fd___() {
			return (1);
		}


		static ushort GetIYdd() {
			int ofs = VGA.PEEK8(PC.Word++);
			MemPtr.Word = (ushort)(IY.Word + (ofs > 127 ? ofs - 256 : ofs));
			return MemPtr.Word;
		}


		static int FD_09() {/* ADD IY, BC */
			return ADD_R16(ref IY.Word, BC.Word);
		}


		static int FD_19() {/* ADD IY, DE */
			return ADD_R16(ref IY.Word, DE.Word);
		}


		static int FD_21() {/* LD IY, nnnn */
			IY.Word = VGA.PEEK16(PC.Word);
			PC.Word += 2;
			return (3);
		}


		static int FD_22() {/* LD ( nnnn ), IY */
			MemPtr.Word = VGA.PEEK16(PC.Word);
			VGA.POKE16(MemPtr.Word, IY.Word);
			MemPtr.Word++;
			PC.Word += 2;
			return (5);
		}


		static int FD_23() {/* INC IY */
			++IY.Word;
			SupIrqWaitState = 1;
			return (2);
		}


		static int FD_24() {/* INC IYh */
			return FLAG_INC(++IY.High);
		}


		static int FD_25() {/* DEC IYh */
			return FLAG_DEC(--IY.High);
		}


		static int FD_26() {/* LD IYh, n */
			IY.High = VGA.PEEK8(PC.Word++);
			return (2);
		}


		static int FD_29() {/* ADD IY, IY */
			return ADD_R16(ref IY.Word, IY.Word);
		}


		static int FD_2A() {/* LD IY, ( nnnn ) */
			MemPtr.Word = VGA.PEEK16(PC.Word);
			IY.Word = VGA.PEEK16(MemPtr.Word);
			MemPtr.Word++;
			PC.Word += 2;
			return (5);
		}


		static int FD_2B() {/* DEC IY */
			--IY.Word;
			SupIrqWaitState = 1;
			return (2);
		}


		static int FD_2C() {/* INC IYl */
			return FLAG_INC(++IY.Low);
		}


		static int FD_2D() {/* DEC IYl */
			return FLAG_DEC(--IY.Low);
		}


		static int FD_2E() {/* LD IYl, n */
			IY.Low = VGA.PEEK8(PC.Word++);
			return (2);
		}


		static int FD_34() {/* INC (IY+n) */
			ushort ofs = GetIYdd();
			byte r = VGA.PEEK8(ofs);
			FLAG_INC(++r);
			VGA.POKE8(ofs, r);
			return (5);
		}


		static int FD_35() {/* DEC (IY+n) */
			ushort ofs = GetIYdd();
			byte r = VGA.PEEK8(ofs);
			FLAG_DEC(--r);
			VGA.POKE8(ofs, r);
			return (5);
		}


		static int FD_36() {/* LD (IY+d), n */
			ushort ofs = GetIYdd();
			VGA.POKE8(ofs, VGA.PEEK8(PC.Word++));
			return (5);
		}


		static int FD_39() {/* ADD IY, SP */
			return ADD_R16(ref IY.Word, SP.Word);
		}


		static int FD_44() {/* LD B, IYh */
			BC.High = IY.High;
			return (1);
		}


		static int FD_45() {/* LD B, IYl */
			BC.High = IY.Low;
			return (1);
		}


		static int FD_46() {/* LD B, (IY+d) */
			BC.High = VGA.PEEK8(GetIYdd());
			return (4);
		}


		static int FD_4C() {/* LD C, IYh */
			BC.Low = IY.High;
			return (1);
		}


		static int FD_4D() {/* LD C, IYl */
			BC.Low = IY.Low;
			return (1);
		}


		static int FD_4E() {/* LD C, (IY+d) */
			BC.Low = VGA.PEEK8(GetIYdd());
			return (4);
		}


		static int FD_54() {/* LD D, IYh */
			DE.High = IY.High;
			return (1);
		}


		static int FD_55() {/* LD D, IYl */
			DE.High = IY.Low;
			return (1);
		}


		static int FD_56() {/* LD D, (IY+d) */
			DE.High = VGA.PEEK8(GetIYdd());
			return (4);
		}


		static int FD_5C() {/* LD E, IYh */
			DE.Low = IY.High;
			return (1);
		}


		static int FD_5D() {/* LD E, IYl */
			DE.Low = IY.Low;
			return (1);
		}


		static int FD_5E() {/* LD E, (IY+d) */
			DE.Low = VGA.PEEK8(GetIYdd());
			return (4);
		}


		static int FD_60() {/* LD IYh, B */
			IY.High = BC.High;
			return (1);
		}


		static int FD_61() {/* LD IYh, C */
			IY.High = BC.Low;
			return (1);
		}


		static int FD_62() {/* LD IYh, D */
			IY.High = DE.High;
			return (1);
		}


		static int FD_63() {/* LD IYh, E */
			IY.High = DE.Low;
			return (1);
		}


		static int FD_65() {/* LD IYh, IYl */
			IY.High = IY.Low;
			return (1);
		}


		static int FD_66() {/* LD H, (IY+d) */
			HL.High = VGA.PEEK8(GetIYdd());
			return (4);
		}


		static int FD_67() {/* LD IYh, A */
			IY.High = AF.High;
			return (1);
		}


		static int FD_68() {/* LD IYl, B */
			IY.Low = BC.High;
			return (1);
		}


		static int FD_69() {/* LD IYl, C */
			IY.Low = BC.Low;
			return (1);
		}


		static int FD_6A() {/* LD IYl, D */
			IY.Low = DE.High;
			return (1);
		}


		static int FD_6B() {/* LD IYl, E */
			IY.Low = DE.Low;
			return (1);
		}


		static int FD_6C() {/* LD IYl, IYH */
			IY.Low = IY.High;
			return (1);
		}


		static int FD_6E() {/* LD L, (IY+d) */
			HL.Low = VGA.PEEK8(GetIYdd());
			return (4);
		}


		static int FD_6F() {/* LD IYl, A */
			IY.Low = AF.High;
			return (1);
		}


		static int FD_70() {/* LD (IY+d), B */
			VGA.POKE8(GetIYdd(), BC.High);
			return (4);
		}


		static int FD_71() {/* LD (IY+d), C */
			VGA.POKE8(GetIYdd(), BC.Low);
			return (4);
		}


		static int FD_72() {/* LD (IY+d), D */
			VGA.POKE8(GetIYdd(), DE.High);
			return (4);
		}


		static int FD_73() {/* LD (IY+d), E */
			VGA.POKE8(GetIYdd(), DE.Low);
			return (4);
		}


		static int FD_74() {/* LD (IY+d), H */
			VGA.POKE8(GetIYdd(), HL.High);
			return (4);
		}


		static int FD_75() {/* LD (IY+d), L */
			VGA.POKE8(GetIYdd(), HL.Low);
			return (4);
		}


		static int FD_77() {/* LD (IY+d), A */
			VGA.POKE8(GetIYdd(), AF.High);
			return (4);
		}


		static int FD_7C() {/* LD A, IYh */
			AF.High = IY.High;
			return (1);
		}


		static int FD_7D() {/* LD A, IYl */
			AF.High = IY.Low;
			return (1);
		}


		static int FD_7E() {/* LD A, (IY+d) */
			AF.High = VGA.PEEK8(GetIYdd());
			return (4);
		}


		static int FD_84() {/* ADD A, IYh */
			return ADD_R8(IY.High, 0);
		}


		static int FD_85() {/* ADD A, IYl */
			return ADD_R8(IY.Low, 0);
		}


		static int FD_86() {/* ADD A, (IY+n) */
			ADD_R8(VGA.PEEK8(GetIYdd()), 0);
			return (4);
		}


		static int FD_8C() {/* ADC A, IYh */
			return ADD_R8(IY.High, AF.Low);
		}


		static int FD_8D() {/* ADC A, IYl */
			return ADD_R8(IY.Low, AF.Low);
		}


		static int FD_8E() {/* ADC A, (IY+n) */
			ADD_R8(VGA.PEEK8(GetIYdd()), AF.Low);
			return (4);
		}


		static int FD_94() {/* SUB IYh */
			return SUB_R8(IY.High, 0);
		}


		static int FD_95() {/* SUB IYl */
			return SUB_R8(IY.Low, 0);
		}


		static int FD_96() {/* SUB (IY+n) */
			SUB_R8(VGA.PEEK8(GetIYdd()), 0);
			return (4);
		}


		static int FD_9C() {/* SBC A, IYh */
			return SUB_R8(IY.High, AF.Low);
		}


		static int FD_9D() {/* SBC A, IYl */
			return SUB_R8(IY.Low, AF.Low);
		}


		static int FD_9E() {/* SBC A, (IY+n) */
			SUB_R8(VGA.PEEK8(GetIYdd()), AF.Low);
			return (4);
		}


		static int FD_A4() {/* AND IYh */
			return AND_R8(IY.High);
		}


		static int FD_A5() {/* AND IYl */
			return AND_R8(IY.Low);
		}


		static int FD_A6() {/* AND (IY+n) */
			AND_R8(VGA.PEEK8(GetIYdd()));
			return (4);
		}


		static int FD_AC() {/* XOR IYh */
			return XOR_R8(IY.High);
		}


		static int FD_AD() {/* XOR IYl */
			return XOR_R8(IY.Low);
		}


		static int FD_AE() {/* XOR (IY+n) */
			XOR_R8(VGA.PEEK8(GetIYdd()));
			return (4);
		}


		static int FD_B4() {/* OR IYh */
			return OR_R8(IY.High);
		}


		static int FD_B5() {/* OR IYl */
			return OR_R8(IY.Low);
		}


		static int FD_B6() {/* OR (IY+n) */
			OR_R8(VGA.PEEK8(GetIYdd()));
			return (4);
		}


		static int FD_BC() {/* CP IYh */
			return CP_R8(IY.High);
		}


		static int FD_BD() {/* CP IYl */
			return CP_R8(IY.Low);
		}


		static int FD_BE() {/* CP (IY+n) */
			CP_R8(VGA.PEEK8(GetIYdd()));
			return (4);
		}


		static int FD_CB() {/* special code CB */
			int k;
			AdrCB = (ushort)GetIYdd();
			k = VGA.PEEK8(PC.Word++);
			//Log(MODULENAME, "Instruction #FD,#CB,#%02X,#%02X (PC=#%04X)", VGA.PEEK8(PC.Word - 2), k, PC.Word - 4, LOG_DEBUG);
			if (k > 0x3F && k < 0x80)
				k = (k & 0xF8) | 0x06;
			CBIndex = true;
			IR.Low = (byte)(((IR.Low + 1) & 0x7F) | (IR.Low & 0x80));
			tabCB[k]();
			CBIndex = false;
			return (k > 0x3F && k < 0x80 ? 5 : 6); // ### a vérifier...
		}


		static int FD_DD() {/* special FD_DD */
			// Se comporte comme un simple DD
			IR.Low = (byte)(((IR.Low + 1) & 0x7F) | (IR.Low & 0x80));
			return (tabIX[VGA.PEEK8(PC.Word++)]()); // ### A vérifier
		}


		static int FD_E1() {/* POP IY */
			IY.Word = POP();
			return (3);
		}


		static int FD_E3() {/* EX (SP), IY */
			MemPtr.Word = VGA.PEEK16(SP.Word);
			VGA.POKE16(SP.Word, IY.Word);
			IY.Word = MemPtr.Word;
			SupIrqWaitState = 1;
			return (6);
		}


		static int FD_E5() {/* PUSH IY */
			return PUSH(IY.Word);
		}


		static int FD_E9() {/* JP (IY) */
			PC = IY;
			return (1);
		}


		static int FD_F9() {/* LD SP, IY */
			SP.Word = IY.Word;
			SupIrqWaitState = 1;
			return (2);
		}


		/*******************
	   * OPCODE Standards *
	   *******************/


		static int NO_OP() {
			return (1);
		}


		static int ___01() {/* LD BC, nnnn */
			BC.Word = (ushort)VGA.PEEK16(PC.Word);
			PC.Word += 2;
			return (3);
		}


		static int ___02() {/* LD ( BC ), A */
			VGA.POKE8(BC.Word, AF.High);
			MemPtr.Word = (ushort)(((BC.Word + 1) & 0xFF) + (AF.High << 8));
			return (2);
		}


		static int ___03() {/* INC BC */

			BC.Word++;
			SupIrqWaitState = 1;
			return (2);
		}


		static int ___04() {/* INC B */
			return FLAG_INC(++BC.High);
		}


		static int ___05() {/* DEC B */
			return FLAG_DEC(--BC.High);
		}


		static int ___06() {/* LD B, n */
			BC.High = VGA.PEEK8(PC.Word++);
			return (2);
		}


		static int ___07() {/* RLCA */
			AF.Low = (byte)((AF.Low & (FLAG_S | FLAG_Z | FLAG_V)) | (AF.High >> 7));
			AF.High = (byte)((AF.High << 1) | (AF.Low & FLAG_C));
			AF.Low |= (byte)(AF.High & (FLAG_5 | FLAG_3));
			return (1);
		}


		static int ___08() {/* EX AF, AF' */
			ushort tmp = AF.Word;
			AF = _AF;
			_AF.Word = tmp;
			return (1);
		}


		static int ___09() {/* ADD HL, BC */
			return ADD_R16(ref HL.Word, BC.Word);
		}


		static int ___0A() {/* LD A, ( BC ) */
			AF.High = VGA.PEEK8(BC.Word);
			MemPtr.Word = (ushort)(BC.Word + 1);
			return (2);
		}


		static int ___0B() {/* DEC BC */
			BC.Word--;
			SupIrqWaitState = 1;
			return (2);
		}


		static int ___0C() {/* INC C */
			return FLAG_INC(++BC.Low);
		}


		static int ___0D() {/* DEC C */
			return FLAG_DEC(--BC.Low);
		}


		static int ___0E() {/* LD C, n */
			BC.Low = VGA.PEEK8(PC.Word++);
			return (2);
		}


		static int ___0F() {/* RRCA */
			AF.Low = (byte)((AF.Low & (FLAG_S | FLAG_Z | FLAG_V)) | (AF.High & FLAG_C));
			AF.High = (byte)((AF.High >> 1) | (AF.Low << 7));
			AF.Low |= (byte)(AF.High & (FLAG_5 | FLAG_3));
			return (1);
		}


		static int ___10() {/* DJNZ e */
			int r = 3;
			if ((--BC.High) != 0) {
				int ofs = VGA.PEEK8(PC.Word);
				PC.Word = (ushort)(PC.Word + (ofs > 127 ? ofs - 256 : ofs));
				MemPtr.Word = ++PC.Word;
				r++;
			}
			else
				PC.Word++;

			return (r);
		}


		static int ___11() {/* LD DE, nnnn */
			DE.Word = VGA.PEEK16(PC.Word);
			PC.Word += 2;
			return (3);
		}


		static int ___12() {/* LD ( DE ), A */
			VGA.POKE8(DE.Word, AF.High);
			MemPtr.Word = (ushort)(((DE.Word + 1) & 0xFF) + (AF.High << 8));
			return (2);
		}


		static int ___13() {/* INC DE */
			DE.Word++;
			SupIrqWaitState = 1;
			return (2);
		}


		static int ___14() {/* INC D */
			return FLAG_INC(++DE.High);
		}


		static int ___15() {/* DEC D */
			return FLAG_DEC(--DE.High);
		}


		static int ___16() {/* LD D, n */
			DE.High = VGA.PEEK8(PC.Word++);
			return (2);
		}


		static int ___17() {/* RLA */
			int i = AF.High << 1;
			AF.High = (byte)(i | (AF.Low & FLAG_C));
			AF.Low = (byte)((AF.Low & (FLAG_S | FLAG_Z | FLAG_V)) | (i >> 8) | (AF.High & (FLAG_5 | FLAG_3)));
			return (1);
		}


		static int ___18() {/* JR e */
			int ofs = VGA.PEEK8(PC.Word);
			PC.Word = (ushort)(PC.Word + (ofs > 127 ? ofs - 256 : ofs));
			MemPtr.Word = (ushort)(++PC.Word);
			return (3);
		}


		static int ___19() {/* ADD HL, DE */
			return ADD_R16(ref HL.Word, DE.Word);
		}


		static int ___1A() {/* LD A, ( DE ) */
			AF.High = VGA.PEEK8(DE.Word);
			MemPtr.Word = (ushort)(DE.Word + 1);
			return (2);
		}


		static int ___1B() {/* DEC DE */
			DE.Word--;
			SupIrqWaitState = 1;
			return (2);
		}


		static int ___1C() {/* INC E */
			return FLAG_INC(++DE.Low);
		}


		static int ___1D() {/* DEC E */
			return FLAG_DEC(--DE.Low);
		}


		static int ___1E() {/* LD E, n */
			DE.Low = VGA.PEEK8(PC.Word++);
			return (2);
		}


		static int ___1F() {/* RRA */
			int i = (AF.High >> 1) | ((AF.Low << 7) & 128);
			AF.Low = (byte)((AF.Low & (FLAG_S | FLAG_Z | FLAG_V)) | (AF.High & FLAG_C) | (i & (FLAG_5 | FLAG_3)));
			AF.High = (byte)i;
			return (1);
		}


		static int ___20() {/* JR NZ, e */
			int r = 2;
			if ((AF.Low & FLAG_Z) == 0) {
				int ofs = VGA.PEEK8(PC.Word);
				PC.Word = (ushort)(PC.Word + (ofs > 127 ? ofs - 256 : ofs));
				MemPtr.Word = ++PC.Word;
				r++;
			}
			else
				PC.Word++;

			return (r);
		}


		static int ___21() {/* LD HL, nnnn */
			HL.Word = VGA.PEEK16(PC.Word);
			PC.Word += 2;
			return (3);
		}


		static int ___22() {/* LD ( nnnn ), HL */
			MemPtr.Word = VGA.PEEK16(PC.Word);
			VGA.POKE16(MemPtr.Word, HL.Word);
			MemPtr.Word++;
			PC.Word += 2;
			return (5);
		}


		static int ___23() {/* INC HL */
			HL.Word++;
			SupIrqWaitState = 1;
			return (2);
		}


		static int ___24() {/* INC H */
			return FLAG_INC(++HL.High);
		}


		static int ___25() {/* DEC H */
			return FLAG_DEC(--HL.High);
		}


		static int ___26() {/* LD H, n */
			HL.High = VGA.PEEK8(PC.Word++);
			return (2);
		}


		static int ___27() {/* DAA */
			int f = AF.Low;
			byte add = (byte)(((AF.High & 0x0F) > 9) || ((f & FLAG_H) != 0) ? 0x06 : 0);
			if (((f & FLAG_C) != 0) || AF.High > 0x99) {
				f |= FLAG_C;
				add |= 0x60;
			}
			if ((f & FLAG_N) != 0)
				SUB_R8(add, 0);
			else
				ADD_R8(add, 0);

			AF.Low = (byte)(AF.Low & (~(FLAG_C | FLAG_V)) | (f & FLAG_C) | Parite[AF.High]);
			return (1);
		}


		static int ___28() {/* JR Z, e */
			int r = 2;
			if ((AF.Low & FLAG_Z) != 0) {
				int ofs = VGA.PEEK8(PC.Word);
				PC.Word = (ushort)(PC.Word + (ofs > 127 ? ofs - 256 : ofs));
				MemPtr.Word = ++PC.Word;
				r++;
			}
			else
				PC.Word++;

			return (r);
		}


		static int ___29() {/* ADD HL, HL */
			return ADD_R16(ref HL.Word, HL.Word);
		}


		static int ___2A() {/* LD HL, ( nnnn ) */
			MemPtr.Word = VGA.PEEK16(PC.Word);
			HL.Word = VGA.PEEK16(MemPtr.Word++);
			PC.Word += 2;
			return (5);
		}


		static int ___2B() {/* DEC HL */
			HL.Word--;
			SupIrqWaitState = 1;
			return (2);
		}


		static int ___2C() {/* INC L */
			return FLAG_INC(++HL.Low);
		}


		static int ___2D() {/* DEC L */
			return FLAG_DEC(--HL.Low);
		}


		static int ___2E() {/* LD L, n */
			HL.Low = VGA.PEEK8(PC.Word++);
			return (2);
		}


		static int ___2F() {/* CPL */
			AF.High ^= 0xFF;
			AF.Low = (byte)(AF.Low & N_FLAG_5 & N_FLAG_3 | (AF.High & (FLAG_5 | FLAG_3)) | FLAG_H | FLAG_N);
			return (1);
		}


		static int ___30() {/* JR NC, e */
			int r = 2;
			if ((AF.Low & FLAG_C) == 0) {
				int ofs = VGA.PEEK8(PC.Word);
				PC.Word = (ushort)(PC.Word + (ofs > 127 ? ofs - 256 : ofs));
				MemPtr.Word = ++PC.Word;
				r++;
			}
			else
				PC.Word++;

			return (r);
		}


		static int ___31() {/* LD SP, nnnn */
			SP.Word = VGA.PEEK16(PC.Word);
			PC.Word += 2;
			return (3);
		}


		static int ___32() {/* LD ( nnnn ), A */
			MemPtr.Word = VGA.PEEK16(PC.Word);
			VGA.POKE8(MemPtr.Word++, AF.High);
			MemPtr.High = AF.High;
			PC.Word += 2;
			return (4);
		}


		static int ___33() {/* INC SP */
			SP.Word++;
			SupIrqWaitState = 1;
			return (2);
		}


		static int ___34() {/* INC ( HL ) */
			byte r = VGA.PEEK8(HL.Word);
			FLAG_INC(++r);
			VGA.POKE8(HL.Word, r);
			return (3);
		}


		static int ___35() {/* DEC ( HL ) */
			byte r = VGA.PEEK8(HL.Word);
			FLAG_DEC(--r);
			VGA.POKE8(HL.Word, r);
			return (3);
		}


		static int ___36() {/* LD ( HL ), n */
			VGA.POKE8(HL.Word, VGA.PEEK8(PC.Word++));
			return (3);
		}


		static int ___37() {/* SCF */
			AF.Low = (byte)(((AF.Low | FLAG_C) & (N_FLAG_N & N_FLAG_3 & N_FLAG_H & N_FLAG_5)) | (AF.High & (FLAG_5 | FLAG_3)));
			return (1);
		}


		static int ___38() {/* JR C, e */
			int r = 2;
			if ((AF.Low & FLAG_C) != 0) {
				int ofs = VGA.PEEK8(PC.Word);
				PC.Word = (ushort)(PC.Word + (ofs > 127 ? ofs - 256 : ofs));
				MemPtr.Word = ++PC.Word;
				r++;
			}
			else
				PC.Word++;

			return (r);
		}


		static int ___39() {/* ADD HL, SP */
			return ADD_R16(ref HL.Word, SP.Word);
		}


		static int ___3A() {/* LD A, ( nnnn ) */
			MemPtr.Word = VGA.PEEK16(PC.Word);
			AF.High = VGA.PEEK8(MemPtr.Word++);
			PC.Word += 2;
			return (4);
		}


		static int ___3B() {/* DEC SP */
			SP.Word--;
			SupIrqWaitState = 1;
			return (2);
		}


		static int ___3C() {/* INC A */
			return FLAG_INC(++AF.High);
		}


		static int ___3D() {/* DEC A */
			return FLAG_DEC(--AF.High);
		}


		static int ___3E() {/* LD A, ee */
			AF.High = VGA.PEEK8(PC.Word++);
			return (2);
		}


		static int ___3F() {/* CCF */
			int tmp = (AF.Low & FLAG_C) << 4;
			AF.Low = (byte)(((AF.Low & (N_FLAG_N & N_FLAG_3 & N_FLAG_H & N_FLAG_5)) ^ FLAG_C) | (AF.High & (FLAG_5 | FLAG_3)) | tmp);
			return (1);
		}


		static int ___41() {/* LD B, C */
			BC.High = BC.Low;
			return (1);
		}


		static int ___42() {/* LD B, D */
			BC.High = DE.High;
			return (1);
		}


		static int ___43() {/* LD B, E */
			BC.High = DE.Low;
			return (1);
		}


		static int ___44() {/* LD B, H */
			BC.High = HL.High;
			return (1);
		}


		static int ___45() {/* LD B, L */
			BC.High = HL.Low;
			return (1);
		}


		static int ___46() {/* LD B, ( HL ) */
			BC.High = VGA.PEEK8(HL.Word);
			return (2);
		}


		static int ___47() {/* LD B, A */
			BC.High = AF.High;
			return (1);
		}


		static int ___48() {/* LD C, B */
			BC.Low = BC.High;
			return (1);
		}


		static int ___4A() {/* LD C, D */
			BC.Low = DE.High;
			return (1);
		}


		static int ___4B() {/* LD C, E */
			BC.Low = DE.Low;
			return (1);
		}


		static int ___4C() {/* LD C, H */
			BC.Low = HL.High;
			return (1);
		}


		static int ___4D() {/* LD C, L */
			BC.Low = HL.Low;
			return (1);
		}


		static int ___4E() {/* LD C, ( HL ) */
			BC.Low = VGA.PEEK8(HL.Word);
			return (2);
		}


		static int ___4F() {/* LD C, A */
			BC.Low = AF.High;
			return (1);
		}


		static int ___50() {/* LD D, B */
			DE.High = BC.High;
			return (1);
		}


		static int ___51() {/* LD D, C */
			DE.High = BC.Low;
			return (1);
		}


		static int ___53() {/* LD D, E */
			DE.High = DE.Low;
			return (1);
		}


		static int ___54() {/* LD D, H */
			DE.High = HL.High;
			return (1);
		}


		static int ___55() {/* LD D, L */
			DE.High = HL.Low;
			return (1);
		}


		static int ___56() {/* LD D, ( HL ) */
			DE.High = VGA.PEEK8(HL.Word);
			return (2);
		}


		static int ___57() {/* LD D, A */
			DE.High = AF.High;
			return (1);
		}


		static int ___58() {/* LD E, B */
			DE.Low = BC.High;
			return (1);
		}


		static int ___59() {/* LD E, C */
			DE.Low = BC.Low;
			return (1);
		}


		static int ___5A() {/* LD E, D */
			DE.Low = DE.High;
			return (1);
		}


		static int ___5C() {/* LD E, H */
			DE.Low = HL.High;
			return (1);
		}


		static int ___5D() {/* LD E, L */
			DE.Low = HL.Low;
			return (1);
		}


		static int ___5E() {/* LD E, ( HL ) */
			DE.Low = VGA.PEEK8(HL.Word);
			return (2);
		}


		static int ___5F() {/* LD E, A */
			DE.Low = AF.High;
			return (1);
		}


		static int ___60() {/* LD H, B */
			HL.High = BC.High;
			return (1);
		}


		static int ___61() {/* LD H, C */
			HL.High = BC.Low;
			return (1);
		}


		static int ___62() {/* LD H, D */
			HL.High = DE.High;
			return (1);
		}


		static int ___63() {/* LD H, E */
			HL.High = DE.Low;
			return (1);
		}


		static int ___65() {/* LD H, L */
			HL.High = HL.Low;
			return (1);
		}


		static int ___66() {/* LD H, ( HL ) */
			HL.High = VGA.PEEK8(HL.Word);
			return (2);
		}


		static int ___67() {/* LD H, A */
			HL.High = AF.High;
			return (1);
		}


		static int ___68() {/* LD L, B */
			HL.Low = BC.High;
			return (1);
		}


		static int ___69() {/* LD L, C */
			HL.Low = BC.Low;
			return (1);
		}


		static int ___6A() {/* LD L, D */
			HL.Low = DE.High;
			return (1);
		}


		static int ___6B() {/* LD L, E */
			HL.Low = DE.Low;
			return (1);
		}


		static int ___6C() {/* LD L, H */
			HL.Low = HL.High;
			return (1);
		}


		static int ___6E() {/* LD L, ( HL ) */
			HL.Low = VGA.PEEK8(HL.Word);
			return (2);
		}


		static int ___6F() {/* LD L, A */
			HL.Low = AF.High;
			return (1);
		}


		static int ___70() {/* LD ( HL ), B */
			VGA.POKE8(HL.Word, BC.High);
			return (2);
		}


		static int ___71() {/* LD ( HL ), C */
			VGA.POKE8(HL.Word, BC.Low);
			return (2);
		}


		static int ___72() {/* LD ( HL ), D */
			VGA.POKE8(HL.Word, DE.High);
			return (2);
		}


		static int ___73() {/* LD ( HL ), E */
			VGA.POKE8(HL.Word, DE.Low);
			return (2);
		}


		static int ___74() {/* LD ( HL ), H */
			VGA.POKE8(HL.Word, HL.High);
			return (2);
		}


		static int ___75() {/* LD ( HL ), L */
			VGA.POKE8(HL.Word, HL.Low);
			return (2);
		}

		static int ___76() {/* HALT */
			PC.Word--;
			Halt = 1;
			return (1);
		}


		static int ___77() {/* LD ( HL ), A */
			VGA.POKE8(HL.Word, AF.High);
			return (2);
		}


		static int ___78() {/* LD A, B */
			AF.High = BC.High;
			return (1);
		}


		static int ___79() {/* LD A, C */
			AF.High = BC.Low;
			return (1);
		}


		static int ___7A() {/* LD A, D */
			AF.High = DE.High;
			return (1);
		}


		static int ___7B() {/* LD A, E */
			AF.High = DE.Low;
			return (1);
		}


		static int ___7C() {/* LD A, H */
			AF.High = HL.High;
			return (1);
		}


		static int ___7D() {/* LD A, L */
			AF.High = HL.Low;
			return (1);
		}


		static int ___7E() {/* LD A, ( HL ) */
			AF.High = VGA.PEEK8(HL.Word);
			return (2);
		}


		static int ___80() {/* ADD A, B */
			return ADD_R8(BC.High, 0);
		}


		static int ___81() {/* ADD A, C */
			return ADD_R8(BC.Low, 0);
		}


		static int ___82() {/* ADD A, D */
			return ADD_R8(DE.High, 0);
		}


		static int ___83() {/* ADD A, E */
			return ADD_R8(DE.Low, 0);
		}


		static int ___84() {/* ADD A, H */
			return ADD_R8(HL.High, 0);
		}


		static int ___85() {/* ADD A, L */
			return ADD_R8(HL.Low, 0);
		}


		static int ___86() {/* ADD A, ( HL ) */
			ADD_R8(VGA.PEEK8(HL.Word), 0);
			return (2);
		}


		static int ___87() {/* ADD A, A */
			return ADD_R8(AF.High, 0);
		}


		static int ___88() {/* ADC A, B */
			return ADD_R8(BC.High, AF.Low);
		}


		static int ___89() {/* ADC A, C */
			return ADD_R8(BC.Low, AF.Low);
		}


		static int ___8A() {/* ADC A, D */
			return ADD_R8(DE.High, AF.Low);
		}


		static int ___8B() {/* ADC A, E */
			return ADD_R8(DE.Low, AF.Low);
		}


		static int ___8C() {/* ADC A, H */
			return ADD_R8(HL.High, AF.Low);
		}


		static int ___8D() {/* ADC A, L */
			return ADD_R8(HL.Low, AF.Low);
		}


		static int ___8E() {/* ADC A, (HL) */
			ADD_R8(VGA.PEEK8(HL.Word), AF.Low);
			return (2);
		}


		static int ___8F() {/* ADC A, A */
			return ADD_R8(AF.High, AF.Low);
		}


		static int ___90() {/* SUB B */
			return SUB_R8(BC.High, 0);
		}


		static int ___91() {/* SUB C */
			return SUB_R8(BC.Low, 0);
		}


		static int ___92() {/* SUB D */
			return SUB_R8(DE.High, 0);
		}


		static int ___93() {/* SUB E */
			return SUB_R8(DE.Low, 0);
		}


		static int ___94() {/* SUB H */
			return SUB_R8(HL.High, 0);
		}


		static int ___95() {/* SUB L */
			return SUB_R8(HL.Low, 0);
		}


		static int ___96() {/* SUB (HL) */
			SUB_R8(VGA.PEEK8(HL.Word), 0);
			return (2);
		}


		static int ___97() {/* SUB A */
			return SUB_R8(AF.High, 0);
		}


		static int ___98() {/* SBC A, B */
			return SUB_R8(BC.High, AF.Low);
		}


		static int ___99() {/* SBC A, C */
			return SUB_R8(BC.Low, AF.Low);
		}


		static int ___9A() {/* SBC A, D */
			return SUB_R8(DE.High, AF.Low);
		}


		static int ___9B() {/* SBC A, E */
			return SUB_R8(DE.Low, AF.Low);
		}


		static int ___9C() {/* SBC A, H */
			return SUB_R8(HL.High, AF.Low);
		}


		static int ___9D() {/* SBC A, L */
			return SUB_R8(HL.Low, AF.Low);
		}


		static int ___9E() {/* SBC A, (HL) */
			SUB_R8(VGA.PEEK8(HL.Word), AF.Low);
			return (2);
		}


		static int ___9F() {/* SBC A, A */
			return SUB_R8(AF.High, AF.Low);
		}


		static int ___A0() {/* AND B */
			return AND_R8(BC.High);
		}


		static int ___A1() {/* AND C */
			return AND_R8(BC.Low);
		}


		static int ___A2() {/* AND D */
			return AND_R8(DE.High);
		}


		static int ___A3() {/* AND E */
			return AND_R8(DE.Low);
		}


		static int ___A4() {/* AND H */
			return AND_R8(HL.High);
		}


		static int ___A5() {/* AND L */
			return AND_R8(HL.Low);
		}


		static int ___A6() {/* AND (HL) */
			AND_R8(VGA.PEEK8(HL.Word));
			return (2);
		}


		static int ___A7() {/* AND A */
			return AND_R8(AF.High);
		}


		static int ___A8() {/* XOR B */
			return XOR_R8(BC.High);
		}


		static int ___A9() {/* XOR C */
			return XOR_R8(BC.Low);
		}


		static int ___AA() {/* XOR D */
			return XOR_R8(DE.High);
		}


		static int ___AB() {/* XOR E */
			return XOR_R8(DE.Low);
		}


		static int ___AC() {/* XOR H */
			return XOR_R8(HL.High);
		}


		static int ___AD() {/* XOR L */
			return XOR_R8(HL.Low);
		}


		static int ___AE() {/* XOR (HL) */
			XOR_R8(VGA.PEEK8(HL.Word));
			return (2);
		}


		static int ___AF() {/* XOR A */
			return XOR_R8(AF.High);
		}


		static int ___B0() {/* OR B */
			return OR_R8(BC.High);
		}


		static int ___B1() {/* OR C */
			return OR_R8(BC.Low);
		}


		static int ___B2() {/* OR D */
			return OR_R8(DE.High);
		}


		static int ___B3() {/* OR E */
			return OR_R8(DE.Low);
		}


		static int ___B4() {/* OR H */
			return OR_R8(HL.High);
		}


		static int ___B5() {/* OR L */
			return OR_R8(HL.Low);
		}


		static int ___B6() {/* OR (HL) */
			OR_R8(VGA.PEEK8(HL.Word));
			return (2);
		}


		static int ___B7() {/* OR A */
			return OR_R8(AF.High);
		}


		static int ___B8() {/* CP B */
			return CP_R8(BC.High);
		}


		static int ___B9() {/* CP C */
			return CP_R8(BC.Low);
		}


		static int ___BA() {/* CP D */
			return CP_R8(DE.High);
		}


		static int ___BB() {/* CP E */
			return CP_R8(DE.Low);
		}


		static int ___BC() {/* CP H */
			return CP_R8(HL.High);
		}


		static int ___BD() {/* CP L */
			return CP_R8(HL.Low);
		}


		static int ___BE() {/* CP (HL) */
			CP_R8(VGA.PEEK8(HL.Word));
			return (2);
		}


		static int ___BF() {/* CP A */
			AF.Low = FLAG_N | FLAG_Z;
			return (1);
		}


		static int ___C0() {/* RET NZ */
			if ((AF.Low & FLAG_Z) == 0) {
				MemPtr.Word = PC.Word = POP();
				return (4);
			}
			SupIrqWaitState = 1;
			return (2);
		}


		static int ___C1() {/* POP BC */
			BC.Word = POP();
			return (3);
		}


		static int ___C2() {/* JP NZ, nnnn */
			MemPtr.Word = VGA.PEEK16(PC.Word);
			PC.Word = (ushort)(((AF.Low & FLAG_Z) == 0) ? MemPtr.Word : PC.Word + 2);
			return (3);
		}


		static int ___C3() {/* JP nnnn */
			MemPtr.Word = PC.Word = VGA.PEEK16(PC.Word);
			return (3);
		}


		static int ___C4() {/* CALL NZ, nnnn */
			if ((AF.Low & FLAG_Z) == 0)
				return ___CD();

			PC.Word += 2;
			return (3);
		}


		static int ___C5() {/* PUSH BC */
			return PUSH(BC.Word);
		}


		static int ___C6() {/* ADD A, ee */
			ADD_R8(VGA.PEEK8(PC.Word++), 0);
			return (2);
		}


		static int ___C7() {/* RST 00 */
			return RST(0x00);
		}


		static int ___C8() {/* RET Z */
			if ((AF.Low & FLAG_Z) != 0) {
				MemPtr.Word = PC.Word = POP();
				return (4);
			}
			SupIrqWaitState = 1;
			return (2);
		}


		static int ___C9() {/* RET */
			MemPtr.Word = PC.Word = POP();
			return (3);
		}


		static int ___CA() {/* JP Z, nnnn */
			MemPtr.Word = VGA.PEEK16(PC.Word);
			PC.Word = (ushort)((AF.Low & FLAG_Z) != 0 ? MemPtr.Word : PC.Word + 2);
			return (3);
		}


		static int ___CB() {/* Special code CB */
			IR.Low = (byte)(((IR.Low + 1) & 0x7F) | (IR.Low & 0x80));
			AdrCB = HL.Word;
			return (tabCB[VGA.PEEK8(PC.Word++)]());
		}


		static int ___CC() {/* CALL Z, nnnn */
			if ((AF.Low & FLAG_Z) != 0)
				return ___CD();

			PC.Word += 2;
			return (3);
		}


		static int ___CD() {/* CALL nnnn */
			MemPtr.Word = VGA.PEEK16(PC.Word);
			PUSH((ushort)(PC.Word + 2));
			PC.Word = MemPtr.Word;
			return (5);
		}


		static int ___CE() {/* ADC A, ee */
			ADD_R8(VGA.PEEK8(PC.Word++), AF.Low);
			return (2);
		}


		static int ___CF() {/* RST 08 */
			return RST(0x08);
		}


		static int ___D0() {/* RET NC */
			if ((AF.Low & FLAG_C) == 0) {
				MemPtr.Word = PC.Word = POP();
				return (4);
			}
			SupIrqWaitState = 1;
			return (2);
		}


		static int ___D1() {/* POP DE */
			DE.Word = POP();
			return (3);
		}


		static int ___D2() {/* JP NC, nnnn */
			MemPtr.Word = VGA.PEEK16(PC.Word);
			PC.Word = (ushort)(((AF.Low & FLAG_C) == 0) ? MemPtr.Word : PC.Word + 2);
			return (3);
		}


		static int ___D3() {/* OUT ( n ), A */
			MemPtr.Low = VGA.PEEK8(PC.Word++);
			MemPtr.High = AF.High;
			GestPort.WritePort(MemPtr.Word++, AF.High);
			return (3);
		}


		static int ___D4() {/* CALL NC, nnnn */
			if ((AF.Low & FLAG_C) == 0)
				return ___CD();

			PC.Word += 2;
			return (3);
		}


		static int ___D5() {/* PUSH DE */
			return PUSH(DE.Word);
		}


		static int ___D6() {/* SUB ee */
			SUB_R8(VGA.PEEK8(PC.Word++), 0);
			return (2);
		}


		static int ___D7() {/* RST 10 */
			return RST(0x10);
		}


		static int ___D8() {/* RET C */
			if ((AF.Low & FLAG_C) != 0) {
				MemPtr.Word = PC.Word = POP();
				return (4);
			}
			SupIrqWaitState = 1;
			return (2);
		}


		static int ___D9() {/* EXX */
			ushort tmp = BC.Word;
			BC = _BC;
			_BC.Word = tmp;

			tmp = DE.Word;
			DE = _DE;
			_DE.Word = tmp;

			tmp = HL.Word;
			HL = _HL;
			_HL.Word = tmp;

			return (1);
		}


		static int ___DA() {/* JP C, nnnn */
			MemPtr.Word = VGA.PEEK16(PC.Word);
			PC.Word = (ushort)((AF.Low & FLAG_C) != 0 ? MemPtr.Word : PC.Word + 2);
			return (3);
		}


		static int ___DB() {/* IN A, ( n ) */
			MemPtr.Word = (ushort)((AF.High << 8) + VGA.PEEK8(PC.Word++));
			AF.High = (byte)GestPort.ReadPort(MemPtr.Word++);
			return (3);
		}


		static int ___DC() {/* CALL C, nnnn */
			if ((AF.Low & FLAG_C) != 0)
				return ___CD();

			PC.Word += 2;
			return (3);
		}


		static int ___DD() {/* Special code DD : IX */
			IR.Low = (byte)(((IR.Low + 1) & 0x7F) | (IR.Low & 0x80));
			return (1 + tabIX[VGA.PEEK8(PC.Word++)]());
		}


		static int ___DE() {/* SBC A, ee */
			SUB_R8(VGA.PEEK8(PC.Word++), AF.Low);
			return (2);
		}


		static int ___DF() {/* RST 18 */
			return RST(0x18);
		}


		static int ___E0() {/* RET PO */
			if ((AF.Low & FLAG_V) == 0) {
				MemPtr.Word = PC.Word = POP();
				return (4);
			}
			SupIrqWaitState = 1;
			return (2);
		}


		static int ___E1() {/* POP HL */
			HL.Word = POP();
			return (3);
		}


		static int ___E2() {/* JP PO, nnnn */
			MemPtr.Word = VGA.PEEK16(PC.Word);
			PC.Word = (ushort)(((AF.Low & FLAG_V) == 0) ? MemPtr.Word : PC.Word + 2);
			return (3);
		}


		static int ___E3() {/* EX (SP), HL */
			MemPtr.Word = VGA.PEEK16(SP.Word);
			VGA.POKE16(SP.Word, HL.Word);
			HL.Word = MemPtr.Word;
			SupIrqWaitState = 1;
			return (6);
		}


		static int ___E4() {/* CALL PO, nnnn */
			if ((AF.Low & FLAG_V) == 0)
				return ___CD();

			PC.Word += 2;
			return (3);
		}


		static int ___E5() {/* PUSH HL */
			return PUSH(HL.Word);
		}


		static int ___E6() {/* AND ee */
			AND_R8(VGA.PEEK8(PC.Word++));
			return (2);
		}


		static int ___E7() {/* RST 20 */
			return RST(0x20);
		}


		static int ___E8() {/* RET PE */
			if ((AF.Low & FLAG_V) != 0) {
				MemPtr.Word = PC.Word = POP();
				return (4);
			}
			SupIrqWaitState = 1;
			return (2);
		}


		static int ___E9() {/* JP ( HL ) */
			PC.Word = HL.Word;
			return (1);
		}


		static int ___EA() {/* JP PE, nnnn */

			MemPtr.Word = VGA.PEEK16(PC.Word);
			PC.Word = (ushort)((AF.Low & FLAG_V) != 0 ? MemPtr.Word : PC.Word + 2);
			return (3);
		}


		static int ___EB() {/* EX DE, HL */
			ushort tmp = DE.Word;
			DE.Word = HL.Word;
			HL.Word = tmp;
			return (1);
		}


		static int ___EC() {/* CALL PE, nnnn */
			if ((AF.Low & FLAG_V) != 0)
				return ___CD();

			PC.Word += 2;
			return (3);
		}


		static int ___ED() {/* Special code ED */
			IR.Low = (byte)(((IR.Low + 1) & 0x7F) | (IR.Low & 0x80));
			return (tabED[VGA.PEEK8(PC.Word++)]());
		}


		static int ___EE() {/* XOR ee */
			XOR_R8(VGA.PEEK8(PC.Word++));
			return (2);
		}


		static int ___EF() {/* RST 28 */
			return RST(0x28);
		}


		static int ___F0() {/* RET P */
			if ((AF.Low & FLAG_S) == 0) {
				MemPtr.Word = PC.Word = POP();
				return (4);
			}
			SupIrqWaitState = 1;
			return (2);
		}


		static int ___F1() {/* POP AF */
			AF.Word = POP();
			return (3);
		}


		static int ___F2() {/* JP P, nnnn */
			MemPtr.Word = VGA.PEEK16(PC.Word);
			PC.Word = (ushort)(((AF.Low & FLAG_S) == 0) ? MemPtr.Word : PC.Word + 2);
			return (3);
		}


		static int ___F3() {/* DI */
			IFF1 = IFF2 = 0;
			return (1);
		}


		static int ___F4() {/* CALL P, nnnn */
			if ((AF.Low & FLAG_S) == 0)
				return ___CD();

			PC.Word += 2;
			return (3);
		}


		static int ___F5() {/* PUSH AF */
			return PUSH(AF.Word);
		}


		static int ___F6() {/* OR ee */
			OR_R8(VGA.PEEK8(PC.Word++));
			return (2);
		}


		static int ___F7() {/* RST 30 */
			return RST(0x30);
		}


		static int ___F8() {/* RET M */
			if ((AF.Low & FLAG_S) != 0) {
				MemPtr.Word = PC.Word = POP();
				return (4);
			}
			SupIrqWaitState = 1;
			return (2);
		}


		static int ___F9() {/* LD SP, HL */
			SP.Word = HL.Word;
			SupIrqWaitState = 1;
			return (2);
		}


		static int ___FA() {/* JP M, nnnn */
			MemPtr.Word = VGA.PEEK16(PC.Word);
			PC.Word = (ushort)((AF.Low & FLAG_S) != 0 ? MemPtr.Word : PC.Word + 2);
			return (3);
		}


		static int ___FB() {/* EI */
			IFF1 = IFF2 = 1;
			return (1);
		}


		static int ___FC() {/* CALL M, nnnn */
			if ((AF.Low & FLAG_S) != 0)
				return ___CD();

			PC.Word += 2;
			return (3);
		}


		static int ___FD() {/* Special code FD : IY */
			IR.Low = (byte)(((IR.Low + 1) & 0x7F) | (IR.Low & 0x80));
			return (1 + tabIY[VGA.PEEK8(PC.Word++)]());
		}


		static int ___FE() {/* CP ee */
			CP_R8(VGA.PEEK8(PC.Word++));
			return (2);
		}


		static int ___FF() {/* RST 38 */
			return RST(0x38);
		}


		public static int ExecInstr() {
			IR.Low = (byte)(((IR.Low + 1) & 0x7F) | (IR.Low & 0x80));
			int LastInstr = VGA.PEEK8(PC.Word++);
			int r = tabinstr[LastInstr]();
			if (IRQ > 0 && IFF1 > 0 && LastInstr != 0xFB) { // Pas d'irq juste après un EI
				if (Halt != 0) {
					Halt = 0;
					PC.Word++;
				}
				IRQ = 0;    // Acquittement interruption
				IFF1 = IFF2 = 0;
				IR.Low = (byte)(((IR.Low + 1) & 0x7F) | (IR.Low & 0x80));
				VGA.CntHSync &= 0x1F;       // Raz du bit 5 du compteur HSYNC du G.A.
				PUSH(PC.Word);
				r -= SupIrqWaitState;
				SupIrqWaitState = 0;
				if (InterruptMode < 2) { // IM 0 et IM 1 -> RST 38H
					PC.Word = 0x38;
					r += 6;
				}
				else { // IM 2 -> CALL ( adr( IR ) )
					PC.Word = (ushort)VGA.PEEK16((ushort)(IR.Word | 0xFF));
					r += 7;
				}
			}
			return (r);
		}

		public static void Init() {
			for (int i = 0; i < 256; i++) {
				int p = (i & 1) + ((i & 0x02) >> 1) + ((i & 0x04) >> 2) + ((i & 0x08) >> 3) + ((i & 0x10) >> 4) + ((i & 0x20) >> 5) + ((i & 0x40) >> 6) + ((i & 0x80) >> 7);
				Parite[i] = (byte)((i > 0 ? i & FLAG_S : FLAG_Z) | (i & (FLAG_5 | FLAG_3)) | ((p & 1) > 0 ? 0 : FLAG_V));
			}
			Reset();
		}

		public static void Reset() {
			CBIndex = false;
			AdrCB = 0;
			SupIrqWaitState = 0;
			IRQ = 0;
			Halt = 0;
			AF.Word = BC.Word = DE.Word = HL.Word = IR.Word = IX.Word = IY.Word = SP.Word = PC.Word = _AF.Word = _BC.Word = _DE.Word = _HL.Word = 0;
			IFF1 = IFF2 = InterruptMode = 0;
		}
	}
}
