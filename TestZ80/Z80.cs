using System;
using System.Runtime.InteropServices;

namespace TestZ80 {
	public class Z80 {
		const byte BIT0 = 0x01;
		const byte BIT1 = 0x02;
		const byte BIT2 = 0x04;
		const byte BIT3 = 0x08;
		const byte BIT4 = 0x10;
		const byte BIT5 = 0x20;
		const byte BIT6 = 0x40;
		const byte BIT7 = 0x80;

		const byte FLAG_0 = 0x00;
		const byte FLAG_C = 0x01;
		const byte FLAG_N = 0x02;
		const byte FLAG_V = 0x04;
		const byte FLAG_3 = 0x08;
		const byte FLAG_H = 0x10;
		const byte FLAG_5 = 0x20;
		const byte FLAG_Z = 0x40;
		const byte FLAG_S = 0x80;
		const byte FLAGS_HC = 0x17;
		const byte FLAGS_53 = 0x28;
		const byte FLAGS_SZ = 0xC0;
		const byte FLAGS_ZV = 0x44;
		const byte FLAGS_S53 = 0xA8;
		const byte FLAGS_SZC = 0xC1;
		const byte FLAGS_SZN = 0xC2;
		const byte FLAGS_SZV = 0xC4;

		const byte N_FLAG_N = 0xFD;
		const byte N_FLAG_V = 0xFB;
		const byte N_FLAG_3 = 0xF7;
		const byte N_FLAG_H = 0xEF;
		const byte N_FLAG_5 = 0xDF;
		const byte N_FLAG_Z = 0xBF;
		const byte N_FLAG_53 = 0xD7;

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

		public static Reg AF, BC, DE, HL, _AF, _BC, _DE, _HL, IX, IY, SP, PC, IR;
		static ushort memPtr;
		static int LastInstr, Halt;
		public static byte InterruptMode;
		static int SupIrqWaitState;
		public static int IRQ, IFF1, IFF2;
		static int d;
		static byte[] TabSZYX = new byte[256];
		static byte[] TabSZYXN = new byte[256];
		static byte[] TabSZN = new byte[256];
		static byte[] TabSZYXP = new byte[256];
		static byte[] TabSZYXHP = new byte[256];
		static byte[] TabHVC = new byte[512];
		static byte[] TabInc = new byte[256];
		static byte[] TabDec = new byte[256];
		static byte[] TabSR = new byte[256];

		static pFct[] TabInstr = new pFct[256] {
	___00, ___01, ___02, ___03, ___04, ___05, ___06, ___07,	
	___08, ___09, ___0A, ___0B, ___0C, ___0D, ___0E, ___0F,	
	___10, ___11, ___12, ___13, ___14, ___15, ___16, ___17,	
	___18, ___19, ___1A, ___1B, ___1C, ___1D, ___1E, ___1F,	
	___20, ___21, ___22, ___23, ___24, ___25, ___26, ___27,	
	___28, ___29, ___2A, ___2B, ___2C, ___2D, ___2E, ___2F,	
	___30, ___31, ___32, ___33, ___34, ___35, ___36, ___37,	
	___38, ___39, ___3A, ___3B, ___3C, ___3D, ___3E, ___3F,	
	___00, ___41, ___42, ___43, ___44, ___45, ___46, ___47,	
	___48, ___00, ___4A, ___4B, ___4C, ___4D, ___4E, ___4F,	
	___50, ___51, ___00, ___53, ___54, ___55, ___56, ___57,	
	___58, ___59, ___5A, ___00, ___5C, ___5D, ___5E, ___5F,	
	___60, ___61, ___62, ___63, ___00, ___65, ___66, ___67,	
	___68, ___69, ___6A, ___6B, ___6C, ___00, ___6E, ___6F,	
	___70, ___71, ___72, ___73, ___74, ___75, ___76, ___77,	
	___78, ___79, ___7A, ___7B, ___7C, ___7D, ___7E, ___00,	
	___80, ___81, ___82, ___83, ___84, ___85, ___86, ___87,	
	___88, ___89, ___8A, ___8B, ___8C, ___8D, ___8E, ___8F,	
	___90, ___91, ___92, ___93, ___94, ___95, ___96, ___97,	
	___98, ___99, ___9A, ___9B, ___9C, ___9D, ___9E, ___9F,	
	___A0, ___A1, ___A2, ___A3, ___A4, ___A5, ___A6, ___A7,	
	___A8, ___A9, ___AA, ___AB, ___AC, ___AD, ___AE, ___AF,	
	___B0, ___B1, ___B2, ___B3, ___B4, ___B5, ___B6, ___B7,	
	___B8, ___B9, ___BA, ___BB, ___BC, ___BD, ___BE, ___BF,	
	___C0, ___C1, ___C2, ___C3, ___C4, ___C5, ___C6, ___C7,	
	___C8, ___C9, ___CA, ___CB, ___CC, ___CD, ___CE, ___C7,	
	___D0, ___D1, ___D2, ___D3, ___D4, ___D5, ___D6, ___C7,	
	___D8, ___D9, ___DA, ___DB, ___DC, ___DD, ___DE, ___C7,	
	___E0, ___E1, ___E2, ___E3, ___E4, ___E5, ___E6, ___C7,	
	___E8, ___E9, ___EA, ___EB, ___EC, ___ED, ___EE, ___C7,	
	___F0, ___F1, ___F2, ___F3, ___F4, ___F5, ___F6, ___C7,	
	___F8, ___F9, ___FA, ___FB, ___FC, ___FD, ___FE, ___C7	
};

		static pFct[] TabInstrCB = new pFct[256] {
	CB_00, CB_01, CB_02, CB_03, CB_04, CB_05, CB_06, CB_07,	
	CB_08, CB_09, CB_0A, CB_0B, CB_0C, CB_0D, CB_0E, CB_0F,	
	CB_10, CB_11, CB_12, CB_13, CB_14, CB_15, CB_16, CB_17,	
	CB_18, CB_19, CB_1A, CB_1B, CB_1C, CB_1D, CB_1E, CB_1F,	
	CB_20, CB_21, CB_22, CB_23, CB_24, CB_25, CB_26, CB_27,	
	CB_28, CB_29, CB_2A, CB_2B, CB_2C, CB_2D, CB_2E, CB_2F,	
	CB_30, CB_31, CB_32, CB_33, CB_34, CB_35, CB_36, CB_37,	
	CB_38, CB_39, CB_3A, CB_3B, CB_3C, CB_3D, CB_3E, CB_3F,	
	CB_40, CB_41, CB_42, CB_43, CB_44, CB_45, CB_46, CB_47,	
	CB_40, CB_41, CB_42, CB_43, CB_44, CB_45, CB_46, CB_47,	
	CB_40, CB_41, CB_42, CB_43, CB_44, CB_45, CB_46, CB_47,	
	CB_40, CB_41, CB_42, CB_43, CB_44, CB_45, CB_46, CB_47,	
	CB_40, CB_41, CB_42, CB_43, CB_44, CB_45, CB_46, CB_47,	
	CB_40, CB_41, CB_42, CB_43, CB_44, CB_45, CB_46, CB_47,	
	CB_40, CB_41, CB_42, CB_43, CB_44, CB_45, CB_46, CB_47,	
	CB_40, CB_41, CB_42, CB_43, CB_44, CB_45, CB_46, CB_47,	
	CB_80, CB_81, CB_82, CB_83, CB_84, CB_85, CB_86, CB_87,	
	CB_80, CB_81, CB_82, CB_83, CB_84, CB_85, CB_86, CB_87,	
	CB_80, CB_81, CB_82, CB_83, CB_84, CB_85, CB_86, CB_87,	
	CB_80, CB_81, CB_82, CB_83, CB_84, CB_85, CB_86, CB_87,	
	CB_80, CB_81, CB_82, CB_83, CB_84, CB_85, CB_86, CB_87,	
	CB_80, CB_81, CB_82, CB_83, CB_84, CB_85, CB_86, CB_87,	
	CB_80, CB_81, CB_82, CB_83, CB_84, CB_85, CB_86, CB_87,	
	CB_80, CB_81, CB_82, CB_83, CB_84, CB_85, CB_86, CB_87,	
	CB_C0, CB_C1, CB_C2, CB_C3, CB_C4, CB_C5, CB_C6, CB_C7,	
	CB_C0, CB_C1, CB_C2, CB_C3, CB_C4, CB_C5, CB_C6, CB_C7,	
	CB_C0, CB_C1, CB_C2, CB_C3, CB_C4, CB_C5, CB_C6, CB_C7,	
	CB_C0, CB_C1, CB_C2, CB_C3, CB_C4, CB_C5, CB_C6, CB_C7,	
	CB_C0, CB_C1, CB_C2, CB_C3, CB_C4, CB_C5, CB_C6, CB_C7,	
	CB_C0, CB_C1, CB_C2, CB_C3, CB_C4, CB_C5, CB_C6, CB_C7,	
	CB_C0, CB_C1, CB_C2, CB_C3, CB_C4, CB_C5, CB_C6, CB_C7,	
	CB_C0, CB_C1, CB_C2, CB_C3, CB_C4, CB_C5, CB_C6, CB_C7,	
};

		static pFct[] TabInstrED = new pFct[256] {
	___00, ___00, ___00, ___00, ___00, ___00, ___00, ___00,	
	___00, ___00, ___00, ___00, ___00, ___00, ___00, ___00,	
	___00, ___00, ___00, ___00, ___00, ___00, ___00, ___00,	
	___00, ___00, ___00, ___00, ___00, ___00, ___00, ___00,	
	___00, ___00, ___00, ___00, ___00, ___00, ___00, ___00,	
	___00, ___00, ___00, ___00, ___00, ___00, ___00, ___00,	
	___00, ___00, ___00, ___00, ___00, ___00, ___00, ___00,	
	___00, ___00, ___00, ___00, ___00, ___00, ___00, ___00,	
	ED_40, ED_41, ED_42, ED_43, ED_44, ED_45, ED_46, ED_47,	
	ED_48, ED_49, ED_4A, ED_4B, ED_44, ED_45, ED_46, ED_4F,	
	ED_50, ED_51, ED_52, ED_53, ED_44, ED_45, ED_56, ED_57,	
	ED_58, ED_59, ED_5A, ED_5B, ED_44, ED_45, ED_5E, ED_5F,	
	ED_60, ED_61, ED_62, ___22, ED_44, ED_45, ED_56, ED_67,	
	ED_68, ED_69, ED_6A, ___2A, ED_44, ED_45, ED_56, ED_6F,	
	ED_70, ED_71, ED_72, ED_73, ED_44, ED_45, ED_56, ___00,	
	ED_78, ED_79, ED_7A, ED_7B, ED_44, ED_45, ED_5E, ___00,	
	___00, ___00, ___00, ___00, ___00, ___00, ___00, ___00,	
	___00, ___00, ___00, ___00, ___00, ___00, ___00, ___00,	
	___00, ___00, ___00, ___00, ___00, ___00, ___00, ___00,	
	___00, ___00, ___00, ___00, ___00, ___00, ___00, ___00,	
	ED_A0, ED_A1, ED_A2, ED_A3, ___00, ___00, ___00, ___00,	
	ED_A8, ED_A9, ED_AA, ED_AB, ___00, ___00, ___00, ___00,	
	ED_B0, ED_B1, ED_B2, ED_B3, ___00, ___00, ___00, ___00,	
	ED_B8, ED_B9, ED_BA, ED_BB, ___00, ___00, ___00, ___00,	
	___00, ___00, ___00, ___00, ___00, ___00, ___00, ___00,	
	___00, ___00, ___00, ___00, ___00, ___00, ___00, ___00,	
	___00, ___00, ___00, ___00, ___00, ___00, ___00, ___00,	
	___00, ___00, ___00, ___00, ___00, ___00, ___00, ___00,	
	___00, ___00, ___00, ___00, ___00, ___00, ___00, ___00,	
	___00, ___00, ___00, ___00, ___00, ___00, ___00, ___00,	
	___00, ___00, ___00, ___00, ___00, ___00, ___00, ___00,	
	___00, ___00, ___00, ___00, ___00, ___00, ___00, ___00	
};

		static pFct[] TabInstrDD = new pFct[256] {
	___00, ___01, ___02, ___03, ___04, ___05, ___06, ___07,	
	___08, DD_09, ___0A, ___0B, ___0C, ___0D, ___0E, ___0F,	
	___10, ___11, ___12, ___13, ___14, ___15, ___16, ___17,	
	___18, DD_19, ___1A, ___1B, ___1C, ___1D, ___1E, ___1F,	
	___20, DD_21, DD_22, DD_23, DD_24, DD_25, DD_26, ___27,	
	___28, DD_29, DD_2A, DD_2B, DD_2C, DD_2D, DD_2E, ___2F,	
	___30, ___31, ___32, ___33, DD_34, DD_35, DD_36, ___37,	
	___38, DD_39, ___3A, ___3B, ___3C, ___3D, ___3E, ___3F,	
	___00, ___41, ___42, ___43, DD_44, DD_45, DD_46, ___47,	
	___48, ___00, ___4A, ___4B, DD_4C, DD_4D, DD_4E, ___4F,	
	___50, ___51, ___00, ___53, DD_54, DD_55, DD_56, ___57,	
	___58, ___59, ___5A, ___00, DD_5C, DD_5D, DD_5E, ___5F,	
	DD_60, DD_61, DD_62, DD_63, ___00, DD_65, DD_66, DD_67,	
	DD_68, DD_69, DD_6A, DD_6B, DD_6C, ___00, DD_6E, DD_6F,	
	DD_70, DD_71, DD_72, DD_73, DD_74, DD_75, ___76, DD_77,	
	___78, ___79, ___7A, ___7B, DD_7C, DD_7D, DD_7E, ___00,	
	___80, ___81, ___82, ___83, DD_84, DD_85, DD_86, ___87,	
	___88, ___89, ___8A, ___8B, DD_8C, DD_8D, DD_8E, ___8F,	
	___90, ___91, ___92, ___93, DD_94, DD_95, DD_96, ___97,	
	___98, ___99, ___9A, ___9B, DD_9C, DD_9D, DD_9E, ___9F,	
	___A0, ___A1, ___A2, ___A3, DD_A4, DD_A5, DD_A6, ___A7,	
	___A8, ___A9, ___AA, ___AB, DD_AC, DD_AD, DD_AE, ___AF,	
	___B0, ___B1, ___B2, ___B3, DD_B4, DD_B5, DD_B6, ___B7,	
	___B8, ___B9, ___BA, ___BB, DD_BC, DD_BD, DD_BE, ___BF,	
	___C0, ___C1, ___C2, ___C3, ___C4, ___C5, ___C6, ___C7,	
	___C8, ___C9, ___CA, DD_CB, ___CC, ___CD, ___CE, ___C7,	
	___D0, ___D1, ___D2, ___D3, ___D4, ___D5, ___D6, ___C7,	
	___D8, ___D9, ___DA, ___DB, ___DC, ___DD, ___DE, ___C7,	
	___E0, DD_E1, ___E2, DD_E3, ___E4, DD_E5, ___E6, ___C7,	
	___E8, DD_E9, ___EA, ___EB, ___EC, ___ED, ___EE, ___C7,	
	___F0, ___F1, ___F2, ___F3, ___F4, ___F5, ___F6, ___C7,	
	___F8, DD_F9, ___FA, ___FB, ___FC, ___FD, ___FE, ___C7	
};

		static pFct[] TabInstrFD = new pFct[256] {
	___00, ___01, ___02, ___03, ___04, ___05, ___06, ___07,	
	___08, FD_09, ___0A, ___0B, ___0C, ___0D, ___0E, ___0F,	
	___10, ___11, ___12, ___13, ___14, ___15, ___16, ___17,	
	___18, FD_19, ___1A, ___1B, ___1C, ___1D, ___1E, ___1F,	
	___20, FD_21, FD_22, FD_23, FD_24, FD_25, FD_26, ___27,	
	___28, FD_29, FD_2A, FD_2B, FD_2C, FD_2D, FD_2E, ___2F,	
	___30, ___31, ___32, ___33, FD_34, FD_35, FD_36, ___37,	
	___38, FD_39, ___3A, ___3B, ___3C, ___3D, ___3E, ___3F,	
	___00, ___41, ___42, ___43, FD_44, FD_45, FD_46, ___47,	
	___48, ___00, ___4A, ___4B, FD_4C, FD_4D, FD_4E, ___4F,	
	___50, ___51, ___00, ___53, FD_54, FD_55, FD_56, ___57,	
	___58, ___59, ___5A, ___00, FD_5C, FD_5D, FD_5E, ___5F,	
	FD_60, FD_61, FD_62, FD_63, ___00, FD_65, FD_66, FD_67,	
	FD_68, FD_69, FD_6A, FD_6B, FD_6C, ___00, FD_6E, FD_6F,	
	FD_70, FD_71, FD_72, FD_73, FD_74, FD_75, ___76, FD_77,	
	___78, ___79, ___7A, ___7B, FD_7C, FD_7D, FD_7E, ___00,	
	___80, ___81, ___82, ___83, FD_84, FD_85, FD_86, ___87,	
	___88, ___89, ___8A, ___8B, FD_8C, FD_8D, FD_8E, ___8F,	
	___90, ___91, ___92, ___93, FD_94, FD_95, FD_96, ___97,	
	___98, ___99, ___9A, ___9B, FD_9C, FD_9D, FD_9E, ___9F,	
	___A0, ___A1, ___A2, ___A3, FD_A4, FD_A5, FD_A6, ___A7,	
	___A8, ___A9, ___AA, ___AB, FD_AC, FD_AD, FD_AE, ___AF,	
	___B0, ___B1, ___B2, ___B3, FD_B4, FD_B5, FD_B6, ___B7,	
	___B8, ___B9, ___BA, ___BB, FD_BC, FD_BD, FD_BE, ___BF,	
	___C0, ___C1, ___C2, ___C3, ___C4, ___C5, ___C6, ___C7,	
	___C8, ___C9, ___CA, FD_CB, ___CC, ___CD, ___CE, ___C7,	
	___D0, ___D1, ___D2, ___D3, ___D4, ___D5, ___D6, ___C7,	
	___D8, ___D9, ___DA, ___DB, ___DC, ___DD, ___DE, ___C7,	
	___E0, FD_E1, ___E2, FD_E3, ___E4, FD_E5, ___E6, ___C7,	
	___E8, FD_E9, ___EA, ___EB, ___EC, ___ED, ___EE, ___C7,	
	___F0, ___F1, ___F2, ___F3, ___F4, ___F5, ___F6, ___C7,	
	___F8, FD_F9, ___FA, ___FB, ___FC, ___FD, ___FE, ___C7	
};

		static pFct[] TabInstrCBDD = new pFct[256] {
	CB_00, CB_01, CB_02, CB_03, DC_04, DC_05, DC_06, CB_07,	
	CB_08, CB_09, CB_0A, CB_0B, DC_0C, DC_0D, DC_0E, CB_0F,	
	CB_10, CB_11, CB_12, CB_13, DC_14, DC_15, DC_16, CB_17,	
	CB_18, CB_19, CB_1A, CB_1B, DC_1C, DC_1D, DC_1E, CB_1F,	
	CB_20, CB_21, CB_22, CB_23, DC_24, DC_25, DC_26, CB_27,	
	CB_28, CB_29, CB_2A, CB_2B, DC_2C, DC_2D, DC_2E, CB_2F,	
	CB_30, CB_31, CB_32, CB_33, DC_34, DC_35, DC_36, CB_37,	
	CB_38, CB_39, CB_3A, CB_3B, DC_3C, DC_3D, DC_3E, CB_3F,	
	CB_40, CB_41, CB_42, CB_43, DC_44, DC_45, DC_46, CB_47,	
	CB_40, CB_41, CB_42, CB_43, DC_44, DC_45, DC_46, CB_47,	
	CB_40, CB_41, CB_42, CB_43, DC_44, DC_45, DC_46, CB_47,	
	CB_40, CB_41, CB_42, CB_43, DC_44, DC_45, DC_46, CB_47,	
	CB_40, CB_41, CB_42, CB_43, DC_44, DC_45, DC_46, CB_47,	
	CB_40, CB_41, CB_42, CB_43, DC_44, DC_45, DC_46, CB_47,	
	CB_40, CB_41, CB_42, CB_43, DC_44, DC_45, DC_46, CB_47,	
	CB_40, CB_41, CB_42, CB_43, DC_44, DC_45, DC_46, CB_47,	
	CB_80, CB_81, CB_82, CB_83, DC_84, DC_85, DC_86, CB_87,	
	CB_80, CB_81, CB_82, CB_83, DC_84, DC_85, DC_86, CB_87,	
	CB_80, CB_81, CB_82, CB_83, DC_84, DC_85, DC_86, CB_87,	
	CB_80, CB_81, CB_82, CB_83, DC_84, DC_85, DC_86, CB_87,	
	CB_80, CB_81, CB_82, CB_83, DC_84, DC_85, DC_86, CB_87,	
	CB_80, CB_81, CB_82, CB_83, DC_84, DC_85, DC_86, CB_87,	
	CB_80, CB_81, CB_82, CB_83, DC_84, DC_85, DC_86, CB_87,	
	CB_80, CB_81, CB_82, CB_83, DC_84, DC_85, DC_86, CB_87,	
	CB_C0, CB_C1, CB_C2, CB_C3, DC_C4, DC_C5, DC_C6, CB_C7,	
	CB_C0, CB_C1, CB_C2, CB_C3, DC_C4, DC_C5, DC_C6, CB_C7,	
	CB_C0, CB_C1, CB_C2, CB_C3, DC_C4, DC_C5, DC_C6, CB_C7,	
	CB_C0, CB_C1, CB_C2, CB_C3, DC_C4, DC_C5, DC_C6, CB_C7,	
	CB_C0, CB_C1, CB_C2, CB_C3, DC_C4, DC_C5, DC_C6, CB_C7,	
	CB_C0, CB_C1, CB_C2, CB_C3, DC_C4, DC_C5, DC_C6, CB_C7,	
	CB_C0, CB_C1, CB_C2, CB_C3, DC_C4, DC_C5, DC_C6, CB_C7,	
	CB_C0, CB_C1, CB_C2, CB_C3, DC_C4, DC_C5, DC_C6, CB_C7,	
};

		static pFct[] TabInstrCBFD = new pFct[256] {
	CB_00, CB_01, CB_02, CB_03, FC_04, FC_05, DC_06, CB_07,	
	CB_08, CB_09, CB_0A, CB_0B, FC_0C, FC_0D, DC_0E, CB_0F,	
	CB_10, CB_11, CB_12, CB_13, FC_14, FC_15, DC_16, CB_17,	
	CB_18, CB_19, CB_1A, CB_1B, FC_1C, FC_1D, DC_1E, CB_1F,	
	CB_20, CB_21, CB_22, CB_23, FC_24, FC_25, DC_26, CB_27,	
	CB_28, CB_29, CB_2A, CB_2B, FC_2C, FC_2D, DC_2E, CB_2F,	
	CB_30, CB_31, CB_32, CB_33, FC_34, FC_35, DC_36, CB_37,	
	CB_38, CB_39, CB_3A, CB_3B, FC_3C, FC_3D, DC_3E, CB_3F,	
	CB_40, CB_41, CB_42, CB_43, FC_44, FC_45, DC_46, CB_47,	
	CB_40, CB_41, CB_42, CB_43, FC_44, FC_45, DC_46, CB_47,	
	CB_40, CB_41, CB_42, CB_43, FC_44, FC_45, DC_46, CB_47,	
	CB_40, CB_41, CB_42, CB_43, FC_44, FC_45, DC_46, CB_47,	
	CB_40, CB_41, CB_42, CB_43, FC_44, FC_45, DC_46, CB_47,	
	CB_40, CB_41, CB_42, CB_43, FC_44, FC_45, DC_46, CB_47,	
	CB_40, CB_41, CB_42, CB_43, FC_44, FC_45, DC_46, CB_47,	
	CB_40, CB_41, CB_42, CB_43, FC_44, FC_45, DC_46, CB_47,	
	CB_80, CB_81, CB_82, CB_83, FC_84, FC_85, DC_86, CB_87,	
	CB_80, CB_81, CB_82, CB_83, FC_84, FC_85, DC_86, CB_87,	
	CB_80, CB_81, CB_82, CB_83, FC_84, FC_85, DC_86, CB_87,	
	CB_80, CB_81, CB_82, CB_83, FC_84, FC_85, DC_86, CB_87,	
	CB_80, CB_81, CB_82, CB_83, FC_84, FC_85, DC_86, CB_87,	
	CB_80, CB_81, CB_82, CB_83, FC_84, FC_85, DC_86, CB_87,	
	CB_80, CB_81, CB_82, CB_83, FC_84, FC_85, DC_86, CB_87,	
	CB_80, CB_81, CB_82, CB_83, FC_84, FC_85, DC_86, CB_87,	
	CB_C0, CB_C1, CB_C2, CB_C3, FC_C4, FC_C5, DC_C6, CB_C7,	
	CB_C0, CB_C1, CB_C2, CB_C3, FC_C4, FC_C5, DC_C6, CB_C7,	
	CB_C0, CB_C1, CB_C2, CB_C3, FC_C4, FC_C5, DC_C6, CB_C7,	
	CB_C0, CB_C1, CB_C2, CB_C3, FC_C4, FC_C5, DC_C6, CB_C7,	
	CB_C0, CB_C1, CB_C2, CB_C3, FC_C4, FC_C5, DC_C6, CB_C7,	
	CB_C0, CB_C1, CB_C2, CB_C3, FC_C4, FC_C5, DC_C6, CB_C7,	
	CB_C0, CB_C1, CB_C2, CB_C3, FC_C4, FC_C5, DC_C6, CB_C7,	
	CB_C0, CB_C1, CB_C2, CB_C3, FC_C4, FC_C5, DC_C6, CB_C7,	
};

		static int ___00() {
			return 1;
		}

		static int ___01() {
			BC.Word = VGA.PEEK16(PC.Word);
			PC.Word += 2;
			return 3;
		}

		static int ___02() {
			VGA.POKE8(memPtr = BC.Word, AF.High);
			memPtr = (ushort)((++memPtr & 0xFF) + (AF.High << 8));
			return 2;
		}

		static int ___03() {
			BC.Word++;
			return SupIrqWaitState = 2;
		}

		static int ___04() {
			AF.Low = (byte)((AF.Low & FLAG_C) | TabInc[++BC.High]);
			return 1;
		}

		static int ___05() {
			AF.Low = (byte)((AF.Low & FLAG_C) | TabDec[--BC.High]);
			return 1;
		}

		static int ___06() {
			BC.High = VGA.PEEK8(PC.Word++);
			return 2;
		}

		static int ___07() {
			AF.Low = (byte)((AF.Low & FLAGS_SZV) | ((AF.High = (byte)((AF.High << 1) | (AF.High >> 7))) & (FLAGS_53 | FLAG_C)));
			return 1;
		}

		static int ___08() {
			ushort t = AF.Word;
			AF.Word = _AF.Word;
			_AF.Word = t;
			return 1;
		}

		static int ___09() {
			int z = (memPtr = HL.Word) + BC.Word, c = (memPtr++ ^ BC.Word ^ z) >> 8;
			AF.Low = (byte)((AF.Low & FLAGS_SZV) | (c & FLAG_H) | (c >> 8) | (((HL.Word = (ushort)z) >> 8) & FLAGS_53));
			return 3;
		}

		static int ___0A() {
			AF.High = VGA.PEEK8(memPtr = BC.Word);
			memPtr++;
			return 2;
		}

		static int ___0B() {
			BC.Word--;
			return SupIrqWaitState = 2;
		}

		static int ___0C() {
			AF.Low = (byte)((AF.Low & FLAG_C) | TabInc[++BC.Low]);
			return 1;
		}

		static int ___0D() {
			AF.Low = (byte)((AF.Low & FLAG_C) | TabDec[--BC.Low]);
			return 1;
		}

		static int ___0E() {
			BC.Low = VGA.PEEK8(PC.Word++);
			return 2;
		}

		static int ___0F() {
			AF.Low = (byte)((AF.Low & FLAGS_SZV) | (AF.High & FLAG_C));
			AF.Low |= (byte)((AF.High = (byte)((AF.High >> 1) | (AF.Low << 7))) & FLAGS_53);
			return 1;
		}

		static int ___10() {
			if (--BC.High != 0) {
				PC.Word = (ushort)(PC.Word + (sbyte)VGA.PEEK8(PC.Word));
				memPtr = ++PC.Word;
				return 4;
			}
			PC.Word++;
			return 3;
		}

		static int ___11() {
			DE.Word = VGA.PEEK16(PC.Word);
			PC.Word += 2;
			return 3;
		}

		static int ___12() {
			VGA.POKE8(memPtr = DE.Word, AF.High);
			memPtr = (ushort)((++memPtr & 0xFF) + (AF.High << 8));
			return 2;
		}

		static int ___13() {
			DE.Word++;
			return SupIrqWaitState = 2;
		}

		static int ___14() {
			AF.Low = (byte)((AF.Low & FLAG_C) | TabInc[++DE.High]);
			return 1;
		}

		static int ___15() {
			AF.Low = (byte)((AF.Low & FLAG_C) | TabDec[--DE.High]);
			return 1;
		}

		static int ___16() {
			DE.High = VGA.PEEK8(PC.Word++);
			return 2;
		}

		static int ___17() {
			int a = AF.High << 1;
			AF.Low = (byte)(((AF.High = (byte)(a | (AF.Low & FLAG_C))) & FLAGS_53) | (a >> 8) | (AF.Low & FLAGS_SZV));
			return 1;
		}

		static int ___18() {
			PC.Word = (ushort)(PC.Word + (sbyte)VGA.PEEK8(PC.Word));
			memPtr = ++PC.Word;
			return 3;
		}

		static int ___19() {
			int z = (memPtr = HL.Word) + DE.Word, c = (memPtr++ ^ DE.Word ^ z) >> 8;
			AF.Low = (byte)((AF.Low & FLAGS_SZV) | (c & FLAG_H) | (c >> 8) | (((HL.Word = (ushort)z) >> 8) & FLAGS_53));
			return 3;
		}

		static int ___1A() {
			AF.High = VGA.PEEK8(memPtr = DE.Word);
			memPtr++;
			return 2;
		}

		static int ___1B() {
			DE.Word--;
			return SupIrqWaitState = 2;
		}

		static int ___1C() {
			AF.Low = (byte)((AF.Low & FLAG_C) | TabInc[++DE.Low]);
			return 1;
		}

		static int ___1D() {
			AF.Low = (byte)((AF.Low & FLAG_C) | TabDec[--DE.Low]);
			return 1;
		}

		static int ___1E() {
			DE.Low = VGA.PEEK8(PC.Word++);
			return 2;
		}

		static int ___1F() {
			int c = AF.High & FLAG_C;
			AF.Low = (byte)((AF.Low & FLAGS_SZV) | ((AF.High = (byte)((AF.High >> 1) | (AF.Low << 7))) & FLAGS_53) | c);
			return 1;
		}

		static int ___20() {
			if ((AF.Low & FLAG_Z) != 0) {
				PC.Word++;
				return 2;
			}
			PC.Word = (ushort)(PC.Word + (sbyte)VGA.PEEK8(PC.Word));
			memPtr = ++PC.Word;
			return 3;
		}

		static int ___21() {
			HL.Word = VGA.PEEK16(PC.Word);
			PC.Word += 2;
			return 3;
		}

		static int ___22() {
			memPtr = VGA.PEEK16(PC.Word);
			PC.Word += 2;
			VGA.POKE16(memPtr++, HL.Word);
			return 5;
		}

		static int ___23() {
			HL.Word++;
			return SupIrqWaitState = 2;
		}

		static int ___24() {
			AF.Low = (byte)((AF.Low & FLAG_C) | TabInc[++HL.High]);
			return 1;
		}

		static int ___25() {
			AF.Low = (byte)((AF.Low & FLAG_C) | TabDec[--HL.High]);
			return 1;
		}

		static int ___26() {
			HL.High = VGA.PEEK8(PC.Word++);
			return 2;
		}

		static int ___27() {
			int a = AF.High, t = (AF.High & 0x0F) > 0x09 || (AF.Low & FLAG_H) != 0 ? 6 : 0;
			if (a > 0x99 || (AF.Low & FLAG_C) != 0)
				t |= 0x60;

			AF.High += (byte)((AF.Low & FLAG_N) != 0 ? -t : t);
			AF.Low = (byte)((AF.Low & FLAG_N) | TabSZYXP[AF.High] | (t >> 6) | ((AF.High ^ a) & FLAG_H));
			return 1;
		}

		static int ___28() {
			if ((AF.Low & FLAG_Z) != 0) {
				PC.Word = (ushort)(PC.Word + (sbyte)VGA.PEEK8(PC.Word));
				memPtr = ++PC.Word;
				return 3;
			}
			PC.Word++;
			return 2;
		}

		static int ___29() {
			AF.Low = (byte)((AF.Low & FLAGS_SZV) | ((HL.Word >> 7) & 0x38) | (HL.Word >> 15));
			memPtr = (ushort)(HL.Word + 1);
			HL.Word <<= 1;
			return 3;
		}

		static int ___2A() {
			memPtr = VGA.PEEK16(PC.Word);
			PC.Word += 2;
			HL.Word = VGA.PEEK16(memPtr++);
			return 5;
		}

		static int ___2B() {
			HL.Word--;
			return SupIrqWaitState = 2;
		}

		static int ___2C() {
			AF.Low = (byte)((AF.Low & FLAG_C) | TabInc[++HL.Low]);
			return 1;
		}

		static int ___2D() {
			AF.Low = (byte)((AF.Low & FLAG_C) | TabDec[--HL.Low]);
			return 1;
		}

		static int ___2E() {
			HL.Low = VGA.PEEK8(PC.Word++);
			return 2;
		}

		static int ___2F() {
			AF.Low = (byte)((AF.Low & (FLAGS_SZV | FLAG_C)) | ((AF.High ^= 0xFF) & FLAGS_53) | FLAG_H | FLAG_N);
			return 1;
		}

		static int ___30() {
			if ((AF.Low & FLAG_C) != 0) {
				PC.Word++;
				return 2;
			}
			PC.Word = (ushort)(PC.Word + (sbyte)VGA.PEEK8(PC.Word));
			memPtr = ++PC.Word;
			return 3;
		}

		static int ___31() {
			SP.Word = VGA.PEEK16(PC.Word);
			PC.Word += 2;
			return 3;
		}

		static int ___32() {
			memPtr = VGA.PEEK16(PC.Word);
			PC.Word += 2;
			VGA.POKE8(memPtr++, AF.High);
			memPtr = (ushort)((memPtr & 0xFF) | (AF.High << 8));
			return 4;
		}

		static int ___33() {
			SP.Word++;
			return SupIrqWaitState = 2;
		}

		static int ___34() {
			int x = VGA.PEEK8(HL.Word) + 1;
			VGA.POKE8(HL.Word, (byte)x);
			AF.Low = (byte)((AF.Low & FLAG_C) | TabInc[(byte)x]);
			return 3;
		}

		static int ___35() {
			int x = VGA.PEEK8(HL.Word) - 1;
			VGA.POKE8(HL.Word, (byte)x);
			AF.Low = (byte)((AF.Low & FLAG_C) | TabDec[(byte)x]);
			return 3;
		}

		static int ___36() {
			VGA.POKE8(HL.Word, VGA.PEEK8(PC.Word++));
			return 3;
		}

		static int ___37() {
			AF.Low = (byte)(((AF.Low & FLAGS_SZV) | (AF.High & FLAGS_53) | FLAG_C));
			return 1;
		}

		static int ___38() {
			if ((AF.Low & FLAG_C) != 0) {
				PC.Word = (ushort)(PC.Word + (sbyte)VGA.PEEK8(PC.Word));
				memPtr = ++PC.Word;
				return 3;
			}
			PC.Word++;
			return 2;
		}

		static int ___39() {
			int z = (memPtr = HL.Word) + SP.Word, c = (memPtr++ ^ SP.Word ^ z) >> 8;
			AF.Low = (byte)((AF.Low & FLAGS_SZV) | (c & FLAG_H) | (c >> 8) | (((HL.Word = (ushort)z) >> 8) & FLAGS_53));
			return 3;
		}
		static int ___3A() {
			memPtr = VGA.PEEK16(PC.Word);
			PC.Word += 2;
			AF.High = VGA.PEEK8(memPtr++);
			return 4;
		}

		static int ___3B() {
			SP.Word--;
			return SupIrqWaitState = 2;
		}

		static int ___3C() {
			AF.Low = (byte)((AF.Low & FLAG_C) | TabInc[++AF.High]);
			return 1;
		}

		static int ___3D() {
			AF.Low = (byte)((AF.Low & FLAG_C) | TabDec[--AF.High]);
			return 1;
		}

		static int ___3E() {
			AF.High = VGA.PEEK8(PC.Word++);
			return 2;
		}

		static int ___3F() {
			int c = AF.Low & FLAG_C;
			AF.Low = (byte)((AF.Low & FLAGS_SZV) | (AF.High & FLAGS_53) | (c << 4) | (c ^ FLAG_C));
			return 1;
		}

		static int ___41() {
			BC.High = BC.Low;
			return 1;
		}

		static int ___42() {
			BC.High = DE.High;
			return 1;
		}

		static int ___43() {
			BC.High = DE.Low;
			return 1;
		}

		static int ___44() {
			BC.High = HL.High;
			return 1;
		}

		static int ___45() {
			BC.High = HL.Low;
			return 1;
		}

		static int ___46() {
			BC.High = VGA.PEEK8(HL.Word);
			return 2;
		}

		static int ___47() {
			BC.High = AF.High;
			return 1;
		}

		static int ___48() {
			BC.Low = BC.High;
			return 1;
		}

		static int ___4A() {
			BC.Low = DE.High;
			return 1;
		}

		static int ___4B() {
			BC.Low = DE.Low;
			return 1;
		}

		static int ___4C() {
			BC.Low = HL.High;
			return 1;
		}

		static int ___4D() {
			BC.Low = HL.Low;
			return 1;
		}

		static int ___4E() {
			BC.Low = VGA.PEEK8(HL.Word);
			return 2;
		}

		static int ___4F() {
			BC.Low = AF.High;
			return 1;
		}

		static int ___50() {
			DE.High = BC.High;
			return 1;
		}

		static int ___51() {
			DE.High = BC.Low;
			return 1;
		}

		static int ___53() {
			DE.High = DE.Low;
			return 1;
		}

		static int ___54() {
			DE.High = HL.High;
			return 1;
		}

		static int ___55() {
			DE.High = HL.Low;
			return 1;
		}

		static int ___56() {
			DE.High = VGA.PEEK8(HL.Word);
			return 2;
		}

		static int ___57() {
			DE.High = AF.High;
			return 1;
		}

		static int ___58() {
			DE.Low = BC.High;
			return 1;
		}

		static int ___59() {
			DE.Low = BC.Low;
			return 1;
		}

		static int ___5A() {
			DE.Low = DE.High;
			return 1;
		}

		static int ___5C() {
			DE.Low = HL.High;
			return 1;
		}

		static int ___5D() {
			DE.Low = HL.Low;
			return 1;
		}

		static int ___5E() {
			DE.Low = VGA.PEEK8(HL.Word);
			return 2;
		}

		static int ___5F() {
			DE.Low = AF.High;
			return 1;
		}

		static int ___60() {
			HL.High = BC.High;
			return 1;
		}

		static int ___61() {
			HL.High = BC.Low;
			return 1;
		}

		static int ___62() {
			HL.High = DE.High;
			return 1;
		}

		static int ___63() {
			HL.High = DE.Low;
			return 1;
		}

		static int ___65() {
			HL.High = HL.Low;
			return 1;
		}

		static int ___66() {
			HL.High = VGA.PEEK8(HL.Word);
			return 2;
		}

		static int ___67() {
			HL.High = AF.High;
			return 1;
		}

		static int ___68() {
			HL.Low = BC.High;
			return 1;
		}

		static int ___69() {
			HL.Low = BC.Low;
			return 1;
		}

		static int ___6A() {
			HL.Low = DE.High;
			return 1;
		}

		static int ___6B() {
			HL.Low = DE.Low;
			return 1;
		}

		static int ___6C() {
			HL.Low = HL.High;
			return 1;
		}

		static int ___6E() {
			HL.Low = VGA.PEEK8(HL.Word);
			return 2;
		}

		static int ___6F() {
			HL.Low = AF.High;
			return 1;
		}

		static int ___70() {
			VGA.POKE8(HL.Word, BC.High);
			return 2;
		}

		static int ___71() {
			VGA.POKE8(HL.Word, BC.Low);
			return 2;
		}

		static int ___72() {
			VGA.POKE8(HL.Word, DE.High);
			return 2;
		}

		static int ___73() {
			VGA.POKE8(HL.Word, DE.Low);
			return 2;
		}

		static int ___74() {
			VGA.POKE8(HL.Word, HL.High);
			return 2;
		}

		static int ___75() {
			VGA.POKE8(HL.Word, HL.Low);
			return 2;
		}

		static int ___76() {
			PC.Word--;
			return Halt = 1;
		}

		static int ___77() {
			VGA.POKE8(HL.Word, AF.High);
			return 2;
		}

		static int ___78() {
			AF.High = BC.High;
			return 1;
		}

		static int ___79() {
			AF.High = BC.Low;
			return 1;
		}

		static int ___7A() {
			AF.High = DE.High;
			return 1;
		}

		static int ___7B() {
			AF.High = DE.Low;
			return 1;
		}

		static int ___7C() {
			AF.High = HL.High;
			return 1;
		}

		static int ___7D() {
			AF.High = HL.Low;
			return 1;
		}

		static int ___7E() {
			AF.High = VGA.PEEK8(HL.Word);
			return 2;
		}

		static int ___80() {
			int z = AF.High + BC.High, c = AF.High ^ BC.High ^ z;
			AF.Low = (byte)(TabSZYX[AF.High = (byte)z] | TabHVC[c]);
			return 1;
		}

		static int ___81() {
			int z = AF.High + BC.Low, c = AF.High ^ BC.Low ^ z;
			AF.Low = (byte)(TabSZYX[AF.High = (byte)z] | TabHVC[c]);
			return 1;
		}

		static int ___82() {
			int z = AF.High + DE.High, c = AF.High ^ DE.High ^ z;
			AF.Low = (byte)(TabSZYX[AF.High = (byte)z] | TabHVC[c]);
			return 1;
		}

		static int ___83() {
			int z = AF.High + DE.Low, c = AF.High ^ DE.Low ^ z;
			AF.Low = (byte)(TabSZYX[AF.High = (byte)z] | TabHVC[c]);
			return 1;
		}

		static int ___84() {
			int z = AF.High + HL.High, c = AF.High ^ HL.High ^ z;
			AF.Low = (byte)(TabSZYX[AF.High = (byte)z] | TabHVC[c]);
			return 1;
		}

		static int ___85() {
			int z = AF.High + HL.Low, c = AF.High ^ HL.Low ^ z;
			AF.Low = (byte)(TabSZYX[AF.High = (byte)z] | TabHVC[c]);
			return 1;
		}

		static int ___86() {
			int x = VGA.PEEK8(HL.Word), z = AF.High + x, c = AF.High ^ x ^ z;
			AF.Low = (byte)(TabSZYX[AF.High = (byte)z] | TabHVC[c]);
			return 2;
		}

		static int ___87() {
			int c = AF.High << 1;
			AF.Low = (byte)(TabSZYX[AF.High = (byte)c] | TabHVC[c]);
			return 1;
		}

		static int ___88() {
			int z = AF.High + BC.High + (AF.Low & FLAG_C), c = AF.High ^ BC.High ^ z;
			AF.Low = (byte)(TabSZYX[AF.High = (byte)z] | TabHVC[c]);
			return 1;
		}

		static int ___89() {
			int z = AF.High + BC.Low + (AF.Low & FLAG_C), c = AF.High ^ BC.Low ^ z;
			AF.Low = (byte)(TabSZYX[AF.High = (byte)z] | TabHVC[c]);
			return 1;
		}

		static int ___8A() {
			int z = AF.High + DE.High + (AF.Low & FLAG_C), c = AF.High ^ DE.High ^ z;
			AF.Low = (byte)(TabSZYX[AF.High = (byte)z] | TabHVC[c]);
			return 1;
		}

		static int ___8B() {
			int z = AF.High + DE.Low + (AF.Low & FLAG_C), c = AF.High ^ DE.Low ^ z;
			AF.Low = (byte)(TabSZYX[AF.High = (byte)z] | TabHVC[c]);
			return 1;
		}

		static int ___8C() {
			int z = AF.High + HL.High + (AF.Low & FLAG_C), c = AF.High ^ HL.High ^ z;
			AF.Low = (byte)(TabSZYX[AF.High = (byte)z] | TabHVC[c]);
			return 1;
		}

		static int ___8D() {
			int z = AF.High + HL.Low + (AF.Low & FLAG_C), c = AF.High ^ HL.Low ^ z;
			AF.Low = (byte)(TabSZYX[AF.High = (byte)z] | TabHVC[c]);
			return 1;
		}

		static int ___8E() {
			int x = VGA.PEEK8(HL.Word), z = AF.High + x + (AF.Low & FLAG_C), c = AF.High ^ x ^ z;
			AF.Low = (byte)(TabSZYX[AF.High = (byte)z] | TabHVC[c]);
			return 2;
		}

		static int ___8F() {
			int c = (AF.High << 1) + (AF.Low & FLAG_C);
			AF.Low = (byte)(TabSZYX[AF.High = (byte)c] | TabHVC[c]);
			return 1;
		}

		static int ___90() {
			int z = AF.High - BC.High, c = AF.High ^ BC.High ^ z;
			AF.Low = (byte)(TabSZYXN[AF.High = (byte)z] | TabHVC[c & 0x190]);
			return 1;
		}

		static int ___91() {
			int z = AF.High - BC.Low, c = AF.High ^ BC.Low ^ z;
			AF.Low = (byte)(TabSZYXN[AF.High = (byte)z] | TabHVC[c & 0x190]);
			return 1;
		}

		static int ___92() {
			int z = AF.High - DE.High, c = AF.High ^ DE.High ^ z;
			AF.Low = (byte)(TabSZYXN[AF.High = (byte)z] | TabHVC[c & 0x190]);
			return 1;
		}

		static int ___93() {
			int z = AF.High - DE.Low, c = AF.High ^ DE.Low ^ z;
			AF.Low = (byte)(TabSZYXN[AF.High = (byte)z] | TabHVC[c & 0x190]);
			return 1;
		}

		static int ___94() {
			int z = AF.High - HL.High, c = AF.High ^ HL.High ^ z;
			AF.Low = (byte)(TabSZYXN[AF.High = (byte)z] | TabHVC[c & 0x190]);
			return 1;
		}

		static int ___95() {
			int z = AF.High - HL.Low, c = AF.High ^ HL.Low ^ z;
			AF.Low = (byte)(TabSZYXN[AF.High = (byte)z] | TabHVC[c & 0x190]);
			return 1;
		}

		static int ___96() {
			int x = VGA.PEEK8(HL.Word), z = AF.High - x, c = AF.High ^ x ^ z;
			AF.Low = (byte)(TabSZYXN[AF.High = (byte)z] | TabHVC[c & 0x190]);
			return 2;
		}

		static int ___97() {
			AF.Word = 0x42;
			return 1;
		}

		static int ___98() {
			int z = AF.High - BC.High - (AF.Low & FLAG_C), c = AF.High ^ BC.High ^ z;
			AF.Low = (byte)(TabSZYXN[AF.High = (byte)z] | TabHVC[c & 0x190]);
			return 1;
		}

		static int ___99() {
			int z = AF.High - BC.Low - (AF.Low & FLAG_C), c = AF.High ^ BC.Low ^ z;
			AF.Low = (byte)(TabSZYXN[AF.High = (byte)z] | TabHVC[c & 0x190]);
			return 1;
		}

		static int ___9A() {
			int z = AF.High - DE.High - (AF.Low & FLAG_C), c = AF.High ^ DE.High ^ z;
			AF.Low = (byte)(TabSZYXN[AF.High = (byte)z] | TabHVC[c & 0x190]);
			return 1;
		}

		static int ___9B() {
			int z = AF.High - DE.Low - (AF.Low & FLAG_C), c = AF.High ^ DE.Low ^ z;
			AF.Low = (byte)(TabSZYXN[AF.High = (byte)z] | TabHVC[c & 0x190]);
			return 1;
		}

		static int ___9C() {
			int z = AF.High - HL.High - (AF.Low & FLAG_C), c = AF.High ^ HL.High ^ z;
			AF.Low = (byte)(TabSZYXN[AF.High = (byte)z] | TabHVC[c & 0x190]);
			return 1;
		}

		static int ___9D() {
			int z = AF.High - HL.Low - (AF.Low & FLAG_C), c = AF.High ^ HL.Low ^ z;
			AF.Low = (byte)(TabSZYXN[AF.High = (byte)z] | TabHVC[c & 0x190]);
			return 1;
		}

		static int ___9E() {
			int x = VGA.PEEK8(HL.Word), z = AF.High - x - (AF.Low & FLAG_C), c = AF.High ^ x ^ z;
			AF.Low = (byte)(TabSZYXN[AF.High = (byte)z] | TabHVC[c & 0x190]);
			return 2;
		}

		static int ___9F() {
			int z = -(AF.Low & FLAG_C);
			AF.Low = (byte)(TabSZYXN[AF.High = (byte)z] | TabHVC[z & 0x190]);
			return 1;
		}

		static int ___A0() {
			AF.Low = TabSZYXHP[AF.High &= BC.High];
			return 1;
		}

		static int ___A1() {
			AF.Low = TabSZYXHP[AF.High &= BC.Low];
			return 1;
		}

		static int ___A2() {
			AF.Low = TabSZYXHP[AF.High &= DE.High];
			return 1;
		}

		static int ___A3() {
			AF.Low = TabSZYXHP[AF.High &= DE.Low];
			return 1;
		}

		static int ___A4() {
			AF.Low = TabSZYXHP[AF.High &= HL.High];
			return 1;
		}

		static int ___A5() {
			AF.Low = TabSZYXHP[AF.High &= HL.Low];
			return 1;
		}

		static int ___A6() {
			AF.Low = TabSZYXHP[AF.High &= VGA.PEEK8(HL.Word)];
			return 2;
		}

		static int ___A7() {
			AF.Low = TabSZYXHP[AF.High];
			return 1;
		}

		static int ___A8() {
			AF.Low = TabSZYXP[AF.High ^= BC.High];
			return 1;
		}

		static int ___A9() {
			AF.Low = TabSZYXP[AF.High ^= BC.Low];
			return 1;
		}

		static int ___AA() {
			AF.Low = TabSZYXP[AF.High ^= DE.High];
			return 1;
		}

		static int ___AB() {
			AF.Low = TabSZYXP[AF.High ^= DE.Low];
			return 1;
		}

		static int ___AC() {
			AF.Low = TabSZYXP[AF.High ^= HL.High];
			return 1;
		}

		static int ___AD() {
			AF.Low = TabSZYXP[AF.High ^= HL.Low];
			return 1;
		}

		static int ___AE() {
			AF.Low = TabSZYXP[AF.High ^= VGA.PEEK8(HL.Word)];
			return 2;
		}

		static int ___AF() {
			AF.Word = 0x44;
			return 1;
		}

		static int ___B0() {
			AF.Low = TabSZYXP[AF.High |= BC.High];
			return 1;
		}

		static int ___B1() {
			AF.Low = TabSZYXP[AF.High |= BC.Low];
			return 1;
		}

		static int ___B2() {
			AF.Low = TabSZYXP[AF.High |= DE.High];
			return 1;
		}

		static int ___B3() {
			AF.Low = TabSZYXP[AF.High |= DE.Low];
			return 1;
		}

		static int ___B4() {
			AF.Low = TabSZYXP[AF.High |= HL.High];
			return 1;
		}

		static int ___B5() {
			AF.Low = TabSZYXP[AF.High |= HL.Low];
			return 1;
		}

		static int ___B6() {
			AF.Low = TabSZYXP[AF.High |= VGA.PEEK8(HL.Word)];
			return 2;
		}

		static int ___B7() {
			AF.Low = TabSZYXP[AF.High];
			return 1;
		}

		static int ___B8() {
			int z = AF.High - BC.High;
			AF.Low = (byte)(TabSZN[z & 0xFF] | (BC.High & FLAGS_53) | TabHVC[(AF.High ^ BC.High ^ z) & 0x190]);
			return 1;
		}

		static int ___B9() {
			int z = AF.High - BC.Low;
			AF.Low = (byte)(TabSZN[z & 0xFF] | (BC.Low & FLAGS_53) | TabHVC[(AF.High ^ BC.Low ^ z) & 0x190]);
			return 1;
		}

		static int ___BA() {
			int z = AF.High - DE.High;
			AF.Low = (byte)(TabSZN[z & 0xFF] | (DE.High & FLAGS_53) | TabHVC[(AF.High ^ DE.High ^ z) & 0x190]);
			return 1;
		}

		static int ___BB() {
			int z = AF.High - DE.Low;
			AF.Low = (byte)(TabSZN[z & 0xFF] | (DE.Low & FLAGS_53) | TabHVC[(AF.High ^ DE.Low ^ z) & 0x190]);
			return 1;
		}

		static int ___BC() {
			int z = AF.High - HL.High;
			AF.Low = (byte)(TabSZN[z & 0xFF] | (HL.High & FLAGS_53) | TabHVC[(AF.High ^ HL.High ^ z) & 0x190]);
			return 1;
		}

		static int ___BD() {
			int z = AF.High - HL.Low;
			AF.Low = (byte)(TabSZN[z & 0xFF] | (HL.Low & FLAGS_53) | TabHVC[(AF.High ^ HL.Low ^ z) & 0x190]);
			return 1;
		}

		static int ___BE() {
			int x = VGA.PEEK8(HL.Word), z = AF.High - x;
			AF.Low = (byte)(TabSZN[z & 0xFF] | (x & FLAGS_53) | TabHVC[(AF.High ^ x ^ z) & 0x190]);
			return 2;
		}

		static int ___BF() {
			AF.Low = (byte)(0x42 | (AF.High & FLAGS_53));
			return 1;
		}

		static int ___C0() {
			if ((AF.Low & FLAG_Z) != 0)
				return 2;
			else {
				memPtr = PC.Word = VGA.PEEK16(SP.Word);
				SP.Word += 2;
				return 4;
			}
		}

		static int ___C1() {
			BC.Word = VGA.PEEK16(SP.Word);
			SP.Word += 2;
			return 3;
		}

		static int ___C2() {
			memPtr = VGA.PEEK16(PC.Word);
			PC.Word = (AF.Low & FLAG_Z) != 0 ? PC.Word += 2 : memPtr;
			return 3;
		}

		static int ___C3() {
			memPtr = VGA.PEEK16(PC.Word);
			PC.Word = memPtr;
			return 3;
		}

		static int ___C4() {
			memPtr = VGA.PEEK16(PC.Word);
			PC.Word += 2;
			if ((AF.Low & FLAG_Z) != 0)
				return 3;
			else {
				SP.Word -= 2;
				VGA.POKE16(SP.Word, PC.Word);
				PC.Word = memPtr;
				return 5;
			}
		}

		static int ___C5() {
			SP.Word -= 2;
			VGA.POKE16(SP.Word, BC.Word);
			return 4;
		}

		static int ___C6() {
			int x = VGA.PEEK8(PC.Word++), z = AF.High + x, c = AF.High ^ x ^ z;
			AF.Low = (byte)(TabSZYX[AF.High = (byte)z] | TabHVC[c]);
			return 2;
		}

		static int ___C7() {
			SP.Word -= 2;
			VGA.POKE16(SP.Word, PC.Word);
			memPtr = PC.Word = (ushort)(LastInstr & 0x38);
			return 4;
		}

		static int ___C8() {
			if ((AF.Low & FLAG_Z) != 0) {
				memPtr = PC.Word = VGA.PEEK16(SP.Word);
				SP.Word += 2;
				return 4;
			}
			return 2;
		}

		static int ___C9() {
			memPtr = PC.Word = VGA.PEEK16(SP.Word);
			SP.Word += 2;
			return 3;
		}

		static int ___CA() {
			memPtr = VGA.PEEK16(PC.Word);
			PC.Word = (ushort)((AF.Low & FLAG_Z) != 0 ? memPtr : PC.Word + 2);
			return 3;
		}

		static int ___CB() {
			IR.Low ^= (byte)(((IR.Low + 1) ^ IR.Low) & 0x7F);
			return TabInstrCB[LastInstr = VGA.PEEK8(PC.Word++)]();
		}

		static int CB_00() {
			int x = BC.High << 1, c = x >> 8;
			AF.Low = (byte)(TabSZYXP[BC.High = (byte)(x | c)] | c);
			return 2;
		}

		static int CB_01() {
			int x = BC.Low << 1, c = x >> 8;
			AF.Low = (byte)(TabSZYXP[BC.Low = (byte)(x | c)] | c);
			return 2;
		}

		static int CB_02() {
			int x = DE.High << 1, c = x >> 8;
			AF.Low = (byte)(TabSZYXP[DE.High = (byte)(x | c)] | c);
			return 2;
		}

		static int CB_03() {
			int x = DE.Low << 1, c = x >> 8;
			AF.Low = (byte)(TabSZYXP[DE.Low = (byte)(x | c)] | c);
			return 2;
		}

		static int CB_04() {
			int x = HL.High << 1, c = x >> 8;
			AF.Low = (byte)(TabSZYXP[HL.High = (byte)(x | c)] | c);
			return 2;
		}

		static int CB_05() {
			int x = HL.Low << 1, c = x >> 8;
			AF.Low = (byte)(TabSZYXP[HL.Low = (byte)(x | c)] | c);
			return 2;
		}

		static int CB_06() {
			int x = VGA.PEEK8(HL.Word) << 1, c = x >> 8;
			x |= c;
			VGA.POKE8(HL.Word, (byte)x);
			AF.Low = (byte)(TabSZYXP[x & 0xFF] | c);
			return 4;
		}

		static int CB_07() {
			int x = AF.High << 1, c = x >> 8;
			AF.Low = (byte)(TabSZYXP[AF.High = (byte)(x | c)] | c);
			return 2;
		}

		static int CB_08() {
			int c = BC.High & 0x01;
			AF.Low = (byte)(TabSZYXP[BC.High = (byte)((BC.High >>= 1) | (c << 7))] | c);
			return 2;
		}

		static int CB_09() {
			int c = BC.Low & 0x01;
			AF.Low = (byte)(TabSZYXP[BC.Low = (byte)((BC.Low >>= 1) | (c << 7))] | c);
			return 2;
		}

		static int CB_0A() {
			int c = DE.High & 0x01;
			AF.Low = (byte)(TabSZYXP[DE.High = (byte)((DE.High >>= 1) | (c << 7))] | c);
			return 2;
		}

		static int CB_0B() {
			int c = DE.Low & 0x01;
			AF.Low = (byte)(TabSZYXP[DE.Low = (byte)((DE.Low >>= 1) | (c << 7))] | c);
			return 2;
		}

		static int CB_0C() {
			int c = HL.High & 0x01;
			AF.Low = (byte)(TabSZYXP[HL.High = (byte)((HL.High >>= 1) | (c << 7))] | c);
			return 2;
		}

		static int CB_0D() {
			int c = HL.Low & 0x01;
			AF.Low = (byte)(TabSZYXP[HL.Low = (byte)((HL.Low >>= 1) | (c << 7))] | c);
			return 2;
		}

		static int CB_0E() {
			int x = VGA.PEEK8(HL.Word), c = x & 1;
			x = (x >>= 1) | (c << 7);
			VGA.POKE8(HL.Word, (byte)x);
			AF.Low = (byte)(TabSZYXP[(byte)x] | c);
			return 4;
		}

		static int CB_0F() {
			int c = AF.High & 0x01;
			AF.Low = (byte)(TabSZYXP[AF.High = (byte)((AF.High >>= 1) | (c << 7))] | c);
			return 2;
		}

		static int CB_10() {
			int x = (AF.Low & FLAG_C) | BC.High << 1;
			AF.Low = (byte)(TabSZYXP[BC.High = (byte)x] | (x >> 8));
			return 2;
		}

		static int CB_11() {
			int x = (AF.Low & FLAG_C) | BC.Low << 1;
			AF.Low = (byte)(TabSZYXP[BC.Low = (byte)x] | (x >> 8));
			return 2;
		}

		static int CB_12() {
			int x = (AF.Low & FLAG_C) | DE.High << 1;
			AF.Low = (byte)(TabSZYXP[DE.High = (byte)x] | (x >> 8));
			return 2;
		}

		static int CB_13() {
			int x = (AF.Low & FLAG_C) | DE.Low << 1;
			AF.Low = (byte)(TabSZYXP[DE.Low = (byte)x] | (x >> 8));
			return 2;
		}

		static int CB_14() {
			int x = (AF.Low & FLAG_C) | HL.High << 1;
			AF.Low = (byte)(TabSZYXP[HL.High = (byte)x] | (x >> 8));
			return 2;
		}

		static int CB_15() {
			int x = (AF.Low & FLAG_C) | HL.Low << 1;
			AF.Low = (byte)(TabSZYXP[HL.Low = (byte)x] | (x >> 8));
			return 2;
		}

		static int CB_16() {
			int x = (AF.Low & FLAG_C) | VGA.PEEK8(HL.Word) << 1;
			VGA.POKE8(HL.Word, (byte)x);
			AF.Low = (byte)(TabSZYXP[x & 0xFF] | (x >> 8));
			return 4;
		}

		static int CB_17() {
			int x = (AF.Low & FLAG_C) | AF.High << 1;
			AF.Low = (byte)(TabSZYXP[AF.High = (byte)x] | (x >> 8));
			return 2;
		}

		static int CB_18() {
			int c = BC.High & 0x01;
			AF.Low = (byte)(TabSZYXP[BC.High = (byte)((BC.High >>= 1) | (AF.Low << 7))] | c);
			return 2;
		}

		static int CB_19() {
			int c = BC.Low & 0x01;
			AF.Low = (byte)(TabSZYXP[BC.Low = (byte)((BC.Low >>= 1) | (AF.Low << 7))] | c);
			return 2;
		}

		static int CB_1A() {
			int c = DE.High & 0x01;
			AF.Low = (byte)(TabSZYXP[DE.High = (byte)((DE.High >>= 1) | (AF.Low << 7))] | c);
			return 2;
		}

		static int CB_1B() {
			int c = DE.Low & 0x01;
			AF.Low = (byte)(TabSZYXP[DE.Low = (byte)((DE.Low >>= 1) | (AF.Low << 7))] | c);
			return 2;
		}

		static int CB_1C() {
			int c = HL.High & 0x01;
			AF.Low = (byte)(TabSZYXP[HL.High = (byte)((HL.High >>= 1) | (AF.Low << 7))] | c);
			return 2;
		}

		static int CB_1D() {
			int c = HL.Low & 0x01;
			AF.Low = (byte)(TabSZYXP[HL.Low = (byte)((HL.Low >>= 1) | (AF.Low << 7))] | c);
			return 2;
		}

		static int CB_1E() {
			int x = VGA.PEEK8(HL.Word), c = x & 0x01;
			x = (x >>= 1) | (AF.Low << 7);
			VGA.POKE8(HL.Word, (byte)x);
			AF.Low = (byte)(TabSZYXP[(byte)x] | c);
			return 4;
		}

		static int CB_1F() {
			int c = AF.High & 0x01;
			AF.Low = (byte)(TabSZYXP[AF.High = (byte)((AF.High >>= 1) | (AF.Low << 7))] | c);
			return 2;
		}

		static int CB_20() {
			int x = BC.High << 1;
			AF.Low = (byte)(TabSZYXP[BC.High = (byte)x] | (x >> 8));
			return 2;
		}

		static int CB_21() {
			int x = BC.Low << 1;
			AF.Low = (byte)(TabSZYXP[BC.Low = (byte)x] | (x >> 8));
			return 2;
		}

		static int CB_22() {
			int x = DE.High << 1;
			AF.Low = (byte)(TabSZYXP[DE.High = (byte)x] | (x >> 8));
			return 2;
		}

		static int CB_23() {
			int x = DE.Low << 1;
			AF.Low = (byte)(TabSZYXP[DE.Low = (byte)x] | (x >> 8));
			return 2;
		}

		static int CB_24() {
			int x = HL.High << 1;
			AF.Low = (byte)(TabSZYXP[HL.High = (byte)x] | (x >> 8));
			return 2;
		}

		static int CB_25() {
			int x = HL.Low << 1;
			AF.Low = (byte)(TabSZYXP[HL.Low = (byte)x] | (x >> 8));
			return 2;
		}

		static int CB_26() {
			int x = VGA.PEEK8(HL.Word) << 1;
			VGA.POKE8(HL.Word, (byte)x);
			AF.Low = (byte)(TabSZYXP[x & 0xFF] | (x >> 8));
			return 4;
		}

		static int CB_27() {
			int x = AF.High << 1;
			AF.Low = (byte)(TabSZYXP[AF.High = (byte)x] | (x >> 8));
			return 2;
		}

		static int CB_28() {
			int c = BC.High & 0x01;
			AF.Low = (byte)(c | TabSZYXP[BC.High = (byte)(((sbyte)BC.High) >> 1)]);
			return 2;
		}

		static int CB_29() {
			int c = BC.Low & 0x01;
			AF.Low = (byte)(c | TabSZYXP[BC.Low = (byte)(((sbyte)BC.Low) >> 1)]);
			return 2;
		}

		static int CB_2A() {
			int c = DE.High & 0x01;
			AF.Low = (byte)(c | TabSZYXP[DE.High = (byte)(((sbyte)DE.High) >> 1)]);
			return 2;
		}

		static int CB_2B() {
			int c = DE.Low & 0x01;
			AF.Low = (byte)(c | TabSZYXP[DE.Low = (byte)(((sbyte)DE.Low) >> 1)]);
			return 2;
		}

		static int CB_2C() {
			int c = HL.High & 0x01;
			AF.Low = (byte)(c | TabSZYXP[HL.High = (byte)(((sbyte)HL.High) >> 1)]);
			return 2;
		}

		static int CB_2D() {
			int c = HL.Low & 0x01;
			AF.Low = (byte)(c | TabSZYXP[HL.Low = (byte)(((sbyte)HL.Low) >> 1)]);
			return 2;
		}

		static int CB_2E() {
			int x = VGA.PEEK8(HL.Word), c = x & 0x01;
			x = ((sbyte)x) >> 1;
			VGA.POKE8(HL.Word, (byte)x);
			AF.Low = (byte)(c | TabSZYXP[x & 0xFF]);
			return 4;
		}

		static int CB_2F() {
			int c = AF.High & 0x01;
			AF.Low = (byte)(c | TabSZYXP[AF.High = (byte)(((sbyte)AF.High) >> 1)]);
			return 2;
		}

		static int CB_30() {
			int x = BC.High << 1;
			AF.Low = (byte)(TabSZYXP[BC.High = (byte)(x | 0x01)] | (x >> 8));
			return 2;
		}

		static int CB_31() {
			int x = BC.Low << 1;
			AF.Low = (byte)(TabSZYXP[BC.Low = (byte)(x | 0x01)] | (x >> 8));
			return 2;
		}

		static int CB_32() {
			int x = DE.High << 1;
			AF.Low = (byte)(TabSZYXP[DE.High = (byte)(x | 0x01)] | (x >> 8));
			return 2;
		}

		static int CB_33() {
			int x = DE.Low << 1;
			AF.Low = (byte)(TabSZYXP[DE.Low = (byte)(x | 0x01)] | (x >> 8));
			return 2;
		}

		static int CB_34() {
			int x = HL.High << 1;
			AF.Low = (byte)(TabSZYXP[HL.High = (byte)(x | 0x01)] | (x >> 8));
			return 2;
		}

		static int CB_35() {
			int x = HL.Low << 1;
			AF.Low = (byte)(TabSZYXP[HL.Low = (byte)(x | 0x01)] | (x >> 8));
			return 2;
		}

		static int CB_36() {
			int x = VGA.PEEK8(HL.Word) << 1, c = x >> 8;
			x |= 1;
			VGA.POKE8(HL.Word, (byte)x);
			AF.Low = (byte)(TabSZYXP[x & 0xFF] | c);
			return 4;
		}

		static int CB_37() {
			int x = AF.High << 1;
			AF.Low = (byte)(TabSZYXP[AF.High = (byte)(x | 0x01)] | (x >> 8));
			return 2;
		}

		static int CB_38() {
			int c = BC.High & 0x01;
			AF.Low = (byte)(TabSZYXP[BC.High = (byte)(BC.High >> 1)] | c);
			return 2;
		}

		static int CB_39() {
			int c = BC.Low & 0x01;
			AF.Low = (byte)(TabSZYXP[BC.Low = (byte)(BC.Low >> 1)] | c);
			return 2;
		}

		static int CB_3A() {
			int c = DE.High & 0x01;
			AF.Low = (byte)(TabSZYXP[DE.High = (byte)(DE.High >> 1)] | c);
			return 2;
		}

		static int CB_3B() {
			int c = DE.Low & 0x01;
			AF.Low = (byte)(TabSZYXP[DE.Low = (byte)(DE.Low >> 1)] | c);
			return 2;
		}

		static int CB_3C() {
			int c = HL.High & 0x01;
			AF.Low = (byte)(TabSZYXP[HL.High = (byte)(HL.High >> 1)] | c);
			return 2;
		}

		static int CB_3D() {
			int c = HL.Low & 0x01;
			AF.Low = (byte)(TabSZYXP[HL.Low = (byte)(HL.Low >> 1)] | c);
			return 2;
		}

		static int CB_3E() {
			int x = VGA.PEEK8(HL.Word), c = x & 0x01;
			x >>= 1;
			VGA.POKE8(HL.Word, (byte)x);
			AF.Low = (byte)(TabSZYXP[x] | c);
			return 4;
		}

		static int CB_3F() {
			int c = AF.High & 0x01;
			AF.Low = (byte)(TabSZYXP[AF.High = (byte)(AF.High >> 1)] | c);
			return 2;
		}

		static int CB_40() {
			AF.Low = (byte)((AF.Low & FLAG_C) | TabSR[BC.High & (1 << ((LastInstr >> 3) & 0x07))] | (BC.High & FLAGS_53));
			return 2;
		}

		static int CB_41() {
			AF.Low = (byte)((AF.Low & FLAG_C) | TabSR[BC.Low & (1 << ((LastInstr >> 3) & 0x07))] | (BC.Low & FLAGS_53));
			return 2;
		}

		static int CB_42() {
			AF.Low = (byte)((AF.Low & FLAG_C) | TabSR[DE.High & (1 << ((LastInstr >> 3) & 0x07))] | (DE.High & FLAGS_53));
			return 2;
		}

		static int CB_43() {
			AF.Low = (byte)((AF.Low & FLAG_C) | TabSR[DE.Low & (1 << ((LastInstr >> 3) & 0x07))] | (DE.Low & FLAGS_53));
			return 2;
		}

		static int CB_44() {
			AF.Low = (byte)((AF.Low & FLAG_C) | TabSR[HL.High & (1 << ((LastInstr >> 3) & 0x07))] | (HL.High & FLAGS_53));
			return 2;
		}

		static int CB_45() {
			AF.Low = (byte)((AF.Low & FLAG_C) | TabSR[HL.Low & 0x01] | (HL.Low & FLAGS_53));
			return 2;
		}

		static int CB_46() {
			AF.Low = (byte)((AF.Low & FLAG_C) | TabSR[VGA.PEEK8(HL.Word) & (1 << ((LastInstr >> 3) & 0x07))] | (memPtr >> 8 & FLAGS_53));
			return 4;
		}

		static int CB_47() {
			AF.Low = (byte)((AF.Low & FLAG_C) | TabSR[AF.High & (1 << ((LastInstr >> 3) & 0x07))] | (AF.High & FLAGS_53));
			return 2;
		}

		static int CB_80() {
			BC.High &= (byte)~(1 << ((LastInstr >> 3) & 0x07));
			return 2;
		}

		static int CB_81() {
			BC.Low &= (byte)~(1 << ((LastInstr >> 3) & 0x07));
			return 2;
		}

		static int CB_82() {
			DE.High &= (byte)~(1 << ((LastInstr >> 3) & 0x07));
			return 2;
		}

		static int CB_83() {
			DE.Low &= (byte)~(1 << ((LastInstr >> 3) & 0x07));
			return 2;
		}

		static int CB_84() {
			HL.High &= (byte)~(1 << ((LastInstr >> 3) & 0x07));
			return 2;
		}

		static int CB_85() {
			HL.Low &= (byte)~(1 << ((LastInstr >> 3) & 0x07));
			return 2;
		}

		static int CB_86() {
			VGA.POKE8(HL.Word, (byte)(VGA.PEEK8(HL.Word) & ~(1 << ((LastInstr >> 3) & 0x07))));
			return 4;
		}

		static int CB_87() {
			AF.High &= (byte)~(1 << ((LastInstr >> 3) & 0x07));
			return 2;
		}

		static int CB_C0() {
			BC.High = (byte)(BC.High | 1 << ((LastInstr >> 3) & 0x07));
			return 2;
		}

		static int CB_C1() {
			BC.Low = (byte)(BC.Low | 1 << ((LastInstr >> 3) & 0x07));
			return 2;
		}

		static int CB_C2() {
			DE.High = (byte)(DE.High | 1 << ((LastInstr >> 3) & 0x07));
			return 2;
		}

		static int CB_C3() {
			DE.Low = (byte)(DE.Low | 1 << ((LastInstr >> 3) & 0x07));
			return 2;
		}

		static int CB_C4() {
			HL.High = (byte)(HL.High | 1 << ((LastInstr >> 3) & 0x07));
			return 2;
		}

		static int CB_C5() {
			HL.Low = (byte)(HL.Low | 1 << ((LastInstr >> 3) & 0x07));
			return 2;
		}

		static int CB_C6() {
			VGA.POKE8(HL.Word, (byte)(VGA.PEEK8(HL.Word) | 1 << ((LastInstr >> 3) & 0x07)));
			return 4;
		}

		static int CB_C7() {
			AF.High = (byte)(AF.High | 1 << ((LastInstr >> 3) & 0x07));
			return 2;
		}

		static int ___CC() {
			memPtr = VGA.PEEK16(PC.Word);
			PC.Word += 2;
			if ((AF.Low & FLAG_Z) != 0) {
				SP.Word -= 2;
				VGA.POKE16(SP.Word, PC.Word);
				PC.Word = memPtr;
				return 5;
			}
			return 3;
		}

		static int ___CD() {
			memPtr = VGA.PEEK16(PC.Word);
			PC.Word += 2;
			SP.Word -= 2;
			VGA.POKE16(SP.Word, PC.Word);
			PC.Word = memPtr;
			return 5;
		}

		static int ___CE() {
			int x = VGA.PEEK8(PC.Word++), z = AF.High + x + (AF.Low & FLAG_C), c = AF.High ^ x ^ z;
			AF.Low = (byte)(TabSZYX[AF.High = (byte)z] | TabHVC[c]);
			return 2;
		}

		static int ___D0() {
			if ((AF.Low & FLAG_C) != 0)
				return 2;
			else {
				memPtr = PC.Word = VGA.PEEK16(SP.Word);
				SP.Word += 2;
				return 4;
			}
		}

		static int ___D1() {
			DE.Word = VGA.PEEK16(SP.Word);
			SP.Word += 2;
			return 3;
		}

		static int ___D2() {
			memPtr = VGA.PEEK16(PC.Word);
			PC.Word = (ushort)((AF.Low & FLAG_C) != 0 ? PC.Word + 2 : memPtr);
			return 3;
		}

		static int ___D3() {
			memPtr = (ushort)((AF.High << 8) | VGA.PEEK8(PC.Word++));
			GestPort.WritePort(memPtr++, AF.High);
			return 3;
		}

		static int ___D4() {
			memPtr = VGA.PEEK16(PC.Word);
			PC.Word += 2;
			if ((AF.Low & FLAG_C) != 0)
				return 3;
			else {
				SP.Word -= 2;
				VGA.POKE16(SP.Word, PC.Word);
				PC.Word = memPtr;
				return 5;
			}
		}

		static int ___D5() {
			SP.Word -= 2;
			VGA.POKE16(SP.Word, DE.Word);
			return 4;
		}

		static int ___D6() {
			int x = VGA.PEEK8(PC.Word++), z = AF.High - x, c = AF.High ^ x ^ z;
			AF.Low = (byte)(TabSZYXN[AF.High = (byte)z] | TabHVC[c & 0x190]);
			return 2;
		}

		static int ___D8() {
			if ((AF.Low & FLAG_C) != 0) {
				memPtr = PC.Word = VGA.PEEK16(SP.Word);
				SP.Word += 2;
				return 4;
			}
			return 2;
		}

		static int ___D9() {
			ushort t = BC.Word;
			BC = _BC;
			_BC.Word = t;
			t = DE.Word;
			DE = _DE;
			_DE.Word = t;
			t = HL.Word;
			HL = _HL;
			_HL.Word = t;
			return 1;
		}

		static int ___DA() {
			memPtr = VGA.PEEK16(PC.Word);
			PC.Word = (ushort)((AF.Low & FLAG_C) != 0 ? memPtr : PC.Word + 2);
			return 3;
		}

		static int ___DB() {
			memPtr = (ushort)((AF.High << 8) | VGA.PEEK8(PC.Word++));
			AF.High = (byte)GestPort.ReadPort(memPtr++);
			return 3;
		}

		static int ___DC() {
			memPtr = VGA.PEEK16(PC.Word);
			PC.Word += 2;
			if ((AF.Low & FLAG_C) != 0) {
				SP.Word -= 2;
				VGA.POKE16(SP.Word, PC.Word);
				PC.Word = memPtr;
				return 5;
			}
			return 3;
		}

		static int ___DD() {
			IR.Low ^= (byte)(((IR.Low + 1) ^ IR.Low) & 0x7F);
			return 1 + TabInstrDD[LastInstr = VGA.PEEK8(PC.Word++)]();
		}

		static int DD_09() {
			int z = (memPtr = IX.Word) + BC.Word, c = (memPtr++ ^ BC.Word ^ z) >> 8;
			AF.Low = (byte)((AF.Low & FLAGS_SZV) | (c & FLAG_H) | (c >> 8) | ((z >> 8) & FLAGS_53));
			IX.Word = (ushort)z;
			return 3;
		}

		static int DD_19() {
			int z = (memPtr = IX.Word) + DE.Word, c = (memPtr++ ^ DE.Word ^ z) >> 8;
			AF.Low = (byte)((AF.Low & FLAGS_SZV) | (c & FLAG_H) | (c >> 8) | ((z >> 8) & FLAGS_53));
			IX.Word = (ushort)z;
			return 3;
		}

		static int DD_21() {
			IX.Word = VGA.PEEK16(PC.Word);
			PC.Word += 2;
			return 3;
		}

		static int DD_22() {
			memPtr = VGA.PEEK16(PC.Word);
			PC.Word += 2;
			VGA.POKE16(memPtr++, IX.Word);
			return 5;
		}

		static int DD_23() {
			IX.Word++;
			return 2;
		}

		static int DD_24() {
			AF.Low = (byte)((AF.Low & FLAG_C) | TabInc[++IX.High]);
			return 1;
		}

		static int DD_25() {
			AF.Low = (byte)((AF.Low & FLAG_C) | TabDec[--IX.High]);
			return 1;
		}

		static int DD_26() {
			IX.High = VGA.PEEK8(PC.Word++);
			return 2;
		}

		static int DD_29() {
			AF.Low = (byte)((AF.Low & FLAGS_SZV) | ((IX.Word >> 7) & 0x38) | (IX.Word >> 15));
			memPtr = (ushort)(IX.Word + 1);
			IX.Word <<= 1;
			return 3;
		}

		static int DD_2A() {
			memPtr = VGA.PEEK16(PC.Word);
			PC.Word += 2;
			IX.Low = VGA.PEEK8(memPtr++);
			IX.High = VGA.PEEK8(memPtr);
			return 5;
		}

		static int DD_2B() {
			IX.Word--;
			return 2;
		}

		static int DD_2C() {
			AF.Low = (byte)((AF.Low & FLAG_C) | TabInc[++IX.Low]);
			return 1;
		}

		static int DD_2D() {
			AF.Low = (byte)((AF.Low & FLAG_C) | TabDec[--IX.Low]);
			return 1;
		}

		static int DD_2E() {
			IX.Low = VGA.PEEK8(PC.Word++);
			return 2;
		}

		static int DD_34() {
			ushort t = (ushort)(IX.Word + (sbyte)VGA.PEEK8(PC.Word++));
			int x = VGA.PEEK8(t) + 1;
			VGA.POKE8(t, (byte)x);
			AF.Low = (byte)((AF.Low & FLAG_C) | TabInc[(byte)x]);
			return 5;
		}

		static int DD_35() {
			ushort t = (ushort)(IX.Word + (sbyte)VGA.PEEK8(PC.Word++));
			int x = VGA.PEEK8(t) - 1;
			VGA.POKE8(t, (byte)x);
			AF.Low = (byte)((AF.Low & FLAG_C) | TabDec[(byte)x]);
			return 5;
		}

		static int DD_36() {
			VGA.POKE8((ushort)(IX.Word + (sbyte)VGA.PEEK8(PC.Word++)), VGA.PEEK8(PC.Word++));
			return 5;
		}

		static int DD_39() {
			int z = (memPtr = IX.Word) + SP.Word, c = (memPtr++ ^ SP.Word ^ z) >> 8;
			AF.Low = (byte)((AF.Low & FLAGS_SZV) | (c & FLAG_H) | (c >> 8) | ((z >> 8) & FLAGS_53));
			IX.Word = (ushort)z;
			return 3;
		}

		static int DD_44() {
			BC.High = IX.High;
			return 1;
		}

		static int DD_45() {
			BC.High = IX.Low;
			return 1;
		}

		static int DD_46() {
			BC.High = VGA.PEEK8((ushort)(IX.Word + (sbyte)VGA.PEEK8(PC.Word++)));
			return 4;
		}
		static int DD_4C() {
			BC.Low = IX.High;
			return 1;
		}

		static int DD_4D() {
			BC.Low = IX.Low;
			return 1;
		}

		static int DD_4E() {
			BC.Low = VGA.PEEK8((ushort)(IX.Word + (sbyte)VGA.PEEK8(PC.Word++)));
			return 4;
		}

		static int DD_54() {
			DE.High = IX.High;
			return 1;
		}

		static int DD_55() {
			DE.High = IX.Low;
			return 1;
		}

		static int DD_56() {
			DE.High = VGA.PEEK8((ushort)(IX.Word + (sbyte)VGA.PEEK8(PC.Word++)));
			return 4;
		}

		static int DD_5C() {
			DE.Low = IX.High;
			return 1;
		}

		static int DD_5D() {
			DE.Low = IX.Low;
			return 1;
		}

		static int DD_5E() {
			DE.Low = VGA.PEEK8((ushort)(IX.Word + (sbyte)VGA.PEEK8(PC.Word++)));
			return 4;
		}

		static int DD_60() {
			IX.High = BC.High;
			return 1;
		}

		static int DD_61() {
			IX.High = BC.Low;
			return 1;
		}

		static int DD_62() {
			IX.High = DE.High;
			return 1;
		}

		static int DD_63() {
			IX.High = DE.Low;
			return 1;
		}

		static int DD_65() {
			IX.High = IX.Low;
			return 1;
		}

		static int DD_66() {
			HL.High = VGA.PEEK8((ushort)(IX.Word + (sbyte)VGA.PEEK8(PC.Word++)));
			return 4;
		}

		static int DD_67() {
			IX.High = AF.High;
			return 1;
		}

		static int DD_68() {
			IX.Low = BC.High;
			return 1;
		}

		static int DD_69() {
			IX.Low = BC.Low;
			return 1;
		}

		static int DD_6A() {
			IX.Low = DE.High;
			return 1;
		}

		static int DD_6B() {
			IX.Low = DE.Low;
			return 1;
		}

		static int DD_6C() {
			IX.Low = IX.High;
			return 1;
		}

		static int DD_6E() {
			HL.Low = VGA.PEEK8((ushort)(IX.Word + (sbyte)VGA.PEEK8(PC.Word++)));
			return 4;
		}

		static int DD_6F() {
			IX.Low = AF.High;
			return 1;
		}

		static int DD_70() {
			VGA.POKE8((ushort)(IX.Word + (sbyte)VGA.PEEK8(PC.Word++)), BC.High);
			return 4;
		}

		static int DD_71() {
			VGA.POKE8((ushort)(IX.Word + (sbyte)VGA.PEEK8(PC.Word++)), BC.Low);
			return 4;
		}

		static int DD_72() {
			VGA.POKE8((ushort)(IX.Word + (sbyte)VGA.PEEK8(PC.Word++)), DE.High);
			return 4;
		}

		static int DD_73() {
			VGA.POKE8((ushort)(IX.Word + (sbyte)VGA.PEEK8(PC.Word++)), DE.Low);
			return 4;
		}

		static int DD_74() {
			VGA.POKE8((ushort)(IX.Word + (sbyte)VGA.PEEK8(PC.Word++)), HL.High);
			return 4;
		}

		static int DD_75() {
			VGA.POKE8((ushort)(IX.Word + (sbyte)VGA.PEEK8(PC.Word++)), HL.Low);
			return 4;
		}

		static int DD_77() {
			VGA.POKE8((ushort)(IX.Word + (sbyte)VGA.PEEK8(PC.Word++)), AF.High);
			return 4;
		}

		static int DD_7C() {
			AF.High = IX.High;
			return 1;
		}

		static int DD_7D() {
			AF.High = IX.Low;
			return 1;
		}

		static int DD_7E() {
			AF.High = VGA.PEEK8((ushort)(IX.Word + (sbyte)VGA.PEEK8(PC.Word++)));
			return 4;
		}
		static int DD_84() {
			int z = AF.High + IX.High, c = AF.High ^ IX.High ^ z;
			AF.Low = (byte)(TabSZYX[AF.High = (byte)z] | TabHVC[c]);
			return 1;
		}

		static int DD_85() {
			int z = AF.High + IX.Low, c = AF.High ^ IX.Low ^ z;
			AF.Low = (byte)(TabSZYX[AF.High = (byte)z] | TabHVC[c]);
			return 1;
		}

		static int DD_86() {
			int x = VGA.PEEK8((ushort)(IX.Word + (sbyte)VGA.PEEK8(PC.Word++))), z = AF.High + x, c = AF.High ^ x ^ z;
			AF.Low = (byte)(TabSZYX[AF.High = (byte)z] | TabHVC[c]);
			return 4;
		}

		static int DD_8C() {
			int z = AF.High + IX.High + (AF.Low & FLAG_C), c = AF.High ^ IX.High ^ z;
			AF.Low = (byte)(TabSZYX[AF.High = (byte)z] | TabHVC[c]);
			return 1;
		}

		static int DD_8D() {
			int z = AF.High + IX.Low + (AF.Low & FLAG_C), c = AF.High ^ IX.Low ^ z;
			AF.Low = (byte)(TabSZYX[AF.High = (byte)z] | TabHVC[c]);
			return 1;
		}

		static int DD_8E() {
			int x = VGA.PEEK8((ushort)(IX.Word + (sbyte)VGA.PEEK8(PC.Word++))), z = AF.High + x + (AF.Low & FLAG_C), c = AF.High ^ x ^ z;
			AF.Low = (byte)(TabSZYX[AF.High = (byte)z] | TabHVC[c]);
			return 4;
		}

		static int DD_94() {
			int z = AF.High - IX.High, c = AF.High ^ IX.High ^ z;
			AF.Low = (byte)(TabSZYXN[AF.High = (byte)z] | TabHVC[c & 0x190]);
			return 1;
		}

		static int DD_95() {
			int z = AF.High - IX.Low, c = AF.High ^ IX.Low ^ z;
			AF.Low = (byte)(TabSZYXN[AF.High = (byte)z] | TabHVC[c & 0x190]);
			return 1;
		}

		static int DD_96() {
			int x = VGA.PEEK8((ushort)(IX.Word + (sbyte)VGA.PEEK8(PC.Word++))), z = AF.High - x, c = AF.High ^ x ^ z;
			AF.Low = (byte)(TabSZYXN[AF.High = (byte)z] | TabHVC[c & 0x190]);
			return 4;
		}

		static int DD_9C() {
			int z = AF.High - IX.High - (AF.Low & FLAG_C), c = AF.High ^ IX.High ^ z;
			AF.Low = (byte)(TabSZYXN[AF.High = (byte)z] | TabHVC[c & 0x190]);
			return 1;
		}

		static int DD_9D() {
			int z = AF.High - IX.Low - (AF.Low & FLAG_C), c = AF.High ^ IX.Low ^ z;
			AF.Low = (byte)(TabSZYXN[AF.High = (byte)z] | TabHVC[c & 0x190]);
			return 1;
		}

		static int DD_9E() {
			int x = VGA.PEEK8((ushort)(IX.Word + (sbyte)VGA.PEEK8(PC.Word++))), z = AF.High - x - (AF.Low & FLAG_C), c = AF.High ^ x ^ z;
			AF.Low = (byte)(TabSZYXN[AF.High = (byte)z] | TabHVC[c & 0x190]);
			return 4;
		}

		static int DD_A4() {
			AF.Low = TabSZYXHP[AF.High &= IX.High];
			return 1;
		}

		static int DD_A5() {
			AF.Low = TabSZYXHP[AF.High &= IX.Low];
			return 1;
		}

		static int DD_A6() {
			AF.Low = TabSZYXHP[AF.High &= VGA.PEEK8((ushort)(IX.Word + (sbyte)VGA.PEEK8(PC.Word++)))];
			return 4;
		}

		static int DD_AC() {
			AF.Low = TabSZYXP[AF.High ^= IX.High];
			return 1;
		}

		static int DD_AD() {
			AF.Low = TabSZYXP[AF.High ^= IX.Low];
			return 1;
		}

		static int DD_AE() {
			AF.Low = TabSZYXP[AF.High ^= VGA.PEEK8((ushort)(IX.Word + (sbyte)VGA.PEEK8(PC.Word++)))];
			return 4;
		}

		static int DD_B4() {
			AF.Low = TabSZYXP[AF.High |= IX.High];
			return 1;
		}

		static int DD_B5() {
			AF.Low = TabSZYXP[AF.High |= IX.Low];
			return 1;
		}

		static int DD_B6() {
			AF.Low = TabSZYXP[AF.High |= VGA.PEEK8((ushort)(IX.Word + (sbyte)VGA.PEEK8(PC.Word++)))];
			return 4;
		}

		static int DD_BC() {
			int z = AF.High - IX.High;
			AF.Low = (byte)(TabSZN[z & 0xFF] | (IX.High & FLAGS_53) | TabHVC[(AF.High ^ IX.High ^ z) & 0x190]);
			return 1;
		}

		static int DD_BD() {
			int z = AF.High - IX.Low;
			AF.Low = (byte)(TabSZN[z & 0xFF] | (IX.Low & FLAGS_53) | TabHVC[(AF.High ^ IX.Low ^ z) & 0x190]);
			return 1;
		}

		static int DD_BE() {
			int x = VGA.PEEK8((ushort)(IX.Word + (sbyte)VGA.PEEK8(PC.Word++))), z = AF.High - x;
			AF.Low = (byte)(TabSZN[z & 0xFF] | (x & FLAGS_53) | TabHVC[(AF.High ^ x ^ z) & 0x190]);
			return 4;
		}

		static int DD_CB() {
			IR.Low ^= (byte)(((IR.Low + 1) ^ IR.Low) & 0x7F);
			d = (ushort)(IX.Word + (sbyte)VGA.PEEK8(PC.Word++));
			return TabInstrCBDD[LastInstr = VGA.PEEK8(PC.Word++)]();
		}

		static int DC_04() {
			int x = IX.High << 1, c = x >> 8;
			AF.Low = (byte)(TabSZYXP[IX.High = (byte)(x | c)] | c);
			return 2;
		}

		static int DC_05() {
			int x = IX.Low << 1, c = x >> 8;
			AF.Low = (byte)(TabSZYXP[IX.Low = (byte)(x | c)] | c);
			return 2;
		}

		static int DC_06() {
			int x = VGA.PEEK8(d) << 1, c = x >> 8;
			x |= c;
			VGA.POKE8(d, (byte)x);
			AF.Low = (byte)(TabSZYXP[x & 0xFF] | c);
			return 6;
		}

		static int DC_0C() {
			int c = IX.High & 0x01;
			AF.Low = (byte)(TabSZYXP[IX.High = (byte)((IX.High >>= 1) | (c << 7))] | c);
			return 2;
		}

		static int DC_0D() {
			int c = IX.Low & 0x01;
			AF.Low = (byte)(TabSZYXP[IX.Low = (byte)((IX.Low >>= 1) | (c << 7))] | c);
			return 2;
		}

		static int DC_0E() {
			int x = VGA.PEEK8(d), c = x & 0x01;
			x = (x >> 1) | (c << 7);
			VGA.POKE8(d, (byte)x);
			AF.Low = (byte)(TabSZYXP[(byte)x] | c);
			return 6;
		}

		static int DC_14() {
			int x = (AF.Low & FLAG_C) | IX.High << 1;
			AF.Low = (byte)(TabSZYXP[IX.High = (byte)x] | (x >> 8));
			return 2;
		}

		static int DC_15() {
			int x = (AF.Low & FLAG_C) | IX.Low << 1;
			AF.Low = (byte)(TabSZYXP[IX.Low = (byte)x] | (x >> 8));
			return 2;
		}

		static int DC_16() {
			int x = (AF.Low & FLAG_C) | VGA.PEEK8(d) << 1;
			VGA.POKE8(d, (byte)x);
			AF.Low = (byte)(TabSZYXP[x & 0xFF] | (x >> 8));
			return 6;
		}

		static int DC_1C() {
			int c = IX.High & 0x01;
			AF.Low = (byte)(TabSZYXP[IX.High = (byte)((IX.High >>= 1) | (AF.Low << 7))] | c);
			return 2;
		}

		static int DC_1D() {
			int c = IX.Low & 0x01;
			AF.Low = (byte)(TabSZYXP[IX.Low = (byte)((IX.Low >>= 1) | (AF.Low << 7))] | c);
			return 2;
		}

		static int DC_1E() {
			int x = VGA.PEEK8(d), c = x & 0x01;
			x = (x >> 1) | (AF.Low << 7);
			VGA.POKE8(d, (byte)x);
			AF.Low = (byte)(TabSZYXP[(byte)x] | c);
			return 6;
		}

		static int DC_24() {
			int x = IX.High << 1;
			AF.Low = (byte)(TabSZYXP[IX.High = (byte)x] | (x >> 8));
			return 2;
		}

		static int DC_25() {
			int x = IX.Low << 1;
			AF.Low = (byte)(TabSZYXP[IX.Low = (byte)x] | (x >> 8));
			return 2;
		}

		static int DC_26() {
			int x = VGA.PEEK8(d) << 1;
			VGA.POKE8(d, (byte)x);
			AF.Low = (byte)(TabSZYXP[x & 0xFF] | (x >> 8));
			return 6;
		}

		static int DC_2C() {
			int c = IX.High & 0x01;
			AF.Low = (byte)(c | TabSZYXP[IX.High = (byte)(((sbyte)IX.High) >> 1)]);
			return 2;
		}

		static int DC_2D() {
			int c = IX.Low & 0x01;
			AF.Low = (byte)(c | TabSZYXP[IX.Low = (byte)(((sbyte)IX.Low) >> 1)]);
			return 2;
		}

		static int DC_2E() {
			int x = VGA.PEEK8(d), c = x & 0x01;
			x = ((sbyte)x) >> 1;
			VGA.POKE8(d, (byte)x);
			AF.Low = (byte)(c | TabSZYXP[x & 0xFF]);
			return 6;
		}

		static int DC_34() {
			int x = IX.High << 1;
			AF.Low = (byte)(TabSZYXP[IX.High = (byte)(x | 0x01)] | (x >> 8));
			return 2;
		}

		static int DC_35() {
			int x = IX.Low << 1;
			AF.Low = (byte)(TabSZYXP[IX.Low = (byte)(x | 0x01)] | (x >> 8));
			return 2;
		}

		static int DC_36() {
			int x = VGA.PEEK8(d) << 1;
			x |= 1;
			VGA.POKE8(d, (byte)x);
			AF.Low = (byte)(TabSZYXP[x & 0xFF] | (x >> 8));
			return 6;
		}

		static int DC_3C() {
			int c = IX.High & 0x01;
			AF.Low = (byte)(TabSZYXP[IX.High = (byte)(IX.High >> 1)] | c);
			return 2;
		}

		static int DC_3D() {
			int c = IX.Low & 0x01;
			AF.Low = (byte)(TabSZYXP[IX.Low = (byte)(IX.Low >> 1)] | c);
			return 2;
		}

		static int DC_3E() {
			int x = VGA.PEEK8(d), c = x & 0x01;
			x >>= 1;
			VGA.POKE8(d, (byte)x);
			AF.Low = (byte)(TabSZYXP[x] | c);
			return 6;
		}

		static int DC_44() {
			AF.Low = (byte)((AF.Low & FLAG_C) | TabSR[IX.High & 1 << ((LastInstr >> 3) & 0x07)] | (IX.High & FLAGS_53));
			return 2;
		}

		static int DC_45() {
			AF.Low = (byte)((AF.Low & FLAG_C) | TabSR[IX.Low & 1 << ((LastInstr >> 3) & 0x07)] | (IX.Low & FLAGS_53));
			return 2;
		}

		static int DC_46() {
			AF.Low = (byte)((AF.Low & FLAG_C) | TabSR[VGA.PEEK8(d) & 1 << ((LastInstr >> 3) & 0x07)] | (d & FLAGS_53));
			return 5;
		}

		static int DC_84() {
			IX.High &= (byte)~(1 << ((LastInstr >> 3) & 0x07));
			return 2;
		}

		static int DC_85() {
			IX.Low &= (byte)~(1 << ((LastInstr >> 3) & 0x07));
			return 2;
		}

		static int DC_86() {
			VGA.POKE8(d, (byte)(VGA.PEEK8(d) & ~(1 << ((LastInstr >> 3) & 0x07))));
			return 6;
		}

		static int DC_C4() {
			IX.High = (byte)(IX.High | 1 << ((LastInstr >> 3) & 0x07));
			return 2;
		}

		static int DC_C5() {
			IX.Low = (byte)(IX.Low | 1 << ((LastInstr >> 3) & 0x07));
			return 2;
		}

		static int DC_C6() {
			VGA.POKE8(d, (byte)(VGA.PEEK8(d) | 1 << ((LastInstr >> 3) & 0x07)));
			return 6;
		}

		static int DD_E1() {
			IX.Word = VGA.PEEK16(SP.Word);
			SP.Word += 2;
			return 3;
		}

		static int DD_E3() {
			memPtr = VGA.PEEK16(SP.Word);
			VGA.POKE16(SP.Word, IX.Word);
			IX.Word = memPtr;
			return 6;
		}

		static int DD_E5() {
			SP.Word -= 2;
			VGA.POKE16(SP.Word, IX.Word);
			return 4;
		}

		static int DD_E9() {
			PC.Word = IX.Word;
			return 1;
		}

		static int DD_F9() {
			SP.Word = IX.Word;
			return 2;
		}

		static int ___DE() {
			int x = VGA.PEEK8(PC.Word++), z = AF.High - x - (AF.Low & FLAG_C), c = AF.High ^ x ^ z;
			AF.Low = (byte)(TabSZYXN[AF.High = (byte)z] | TabHVC[c & 0x190]);
			return 2;
		}

		static int ___E0() {
			if ((AF.Low & FLAG_V) != 0)
				return 2;
			else {
				memPtr = PC.Word = VGA.PEEK16(SP.Word);
				SP.Word += 2;
				return 4;
			}
		}

		static int ___E1() {
			HL.Word = VGA.PEEK16(SP.Word);
			SP.Word += 2;
			return 3;
		}

		static int ___E2() {
			memPtr = VGA.PEEK16(PC.Word);
			PC.Word = (ushort)((AF.Low & FLAG_V) != 0 ? PC.Word + 2 : memPtr);
			return 3;
		}

		static int ___E3() {
			memPtr = VGA.PEEK16(SP.Word);
			VGA.POKE16(SP.Word, HL.Word);
			HL.Word = memPtr;
			return 6;
		}

		static int ___E4() {
			memPtr = VGA.PEEK16(PC.Word);
			PC.Word += 2;
			if ((AF.Low & FLAG_V) != 0)
				return 3;
			else {
				SP.Word -= 2;
				VGA.POKE16(SP.Word, PC.Word);
				PC.Word = memPtr;
				return 5;
			}
		}

		static int ___E5() {
			SP.Word -= 2;
			VGA.POKE16(SP.Word, HL.Word);
			return 4;
		}

		static int ___E6() {
			AF.Low = TabSZYXHP[AF.High &= VGA.PEEK8(PC.Word++)];
			return 2;
		}

		static int ___E8() {
			if ((AF.Low & FLAG_V) != 0)
				return 2;
			else {
				memPtr = PC.Word = VGA.PEEK16(SP.Word);
				SP.Word += 2;
				return 4;
			}
		}

		static int ___E9() {
			PC.Word = HL.Word;
			return 1;
		}

		static int ___EA() {
			memPtr = VGA.PEEK16(PC.Word);
			PC.Word = (ushort)((AF.Low & FLAG_V) != 0 ? memPtr : PC.Word + 2);
			return 3;
		}

		static int ___EB() {
			ushort t = DE.Word;
			DE.Word = HL.Word;
			HL.Word = t;
			return 1;
		}

		static int ___EC() {
			memPtr = VGA.PEEK16(PC.Word);
			PC.Word += 2;
			if ((AF.Low & FLAG_V) != 0) {
				SP.Word -= 2;
				VGA.POKE16(SP.Word, PC.Word);
				PC.Word = memPtr;
				return 5;
			}
			return 3;
		}

		static int ___ED() {
			IR.Low ^= (byte)(((IR.Low + 1) ^ IR.Low) & 0x7F);
			return 1 + TabInstrED[LastInstr = VGA.PEEK8(PC.Word++)]();
		}

		static int ED_40() {
			AF.Low = (byte)(TabSZYXP[BC.High = (byte)GestPort.ReadPort(memPtr = BC.Word)] | (AF.Low & FLAG_C));
			memPtr++;
			return 3;
		}

		static int ED_41() {
			memPtr = BC.Word;
			GestPort.WritePort(memPtr++, BC.High);
			return 3;
		}

		static int ED_42() {
			int z = (memPtr = HL.Word) - BC.Word - (AF.Low & FLAG_C);
			AF.Low = (byte)(FLAG_N | ((HL.Word = (ushort)z) != 0 ? (z >> 8) & FLAGS_S53 : FLAG_Z) | TabHVC[((memPtr++ ^ BC.Word ^ z) >> 8) & 0x190]);
			return 3;
		}

		static int ED_43() {
			memPtr = VGA.PEEK16(PC.Word);
			PC.Word += 2;
			VGA.POKE16(memPtr++, BC.Word);
			return 5;
		}

		static int ED_44() {
			int c = (AF.High ^ -AF.High) & 0x190;
			AF.Low = (byte)(TabSZYXN[AF.High = (byte)-AF.High] | TabHVC[c]);
			return 1;
		}

		static int ED_45() {
			PC.Word = VGA.PEEK16(SP.Word);
			SP.Word += 2;
			memPtr = PC.Word;
			IFF1 = IFF2;
			return 3;
		}

		static int ED_46() {
			InterruptMode = 0;
			return 1;
		}

		static int ED_47() {
			IR.High = AF.High;
			return 2;
		}

		static int ED_48() {
			AF.Low = (byte)(TabSZYXP[BC.Low = (byte)GestPort.ReadPort(memPtr = BC.Word)] | (AF.Low & FLAG_C));
			memPtr++;
			return 3;
		}

		static int ED_49() {
			memPtr = BC.Word;
			GestPort.WritePort(memPtr++, BC.Low);
			return 3;
		}

		static int ED_4A() {
			int z = (memPtr = HL.Word) + BC.Word + (AF.Low & FLAG_C);
			AF.Low = (byte)(((HL.Word = (ushort)z) != 0 ? (z >> 8) & FLAGS_S53 : FLAG_Z) | TabHVC[(memPtr++ ^ BC.Word ^ z) >> 8]);
			return 3;
		}

		static int ED_4B() {
			memPtr = VGA.PEEK16(PC.Word);
			PC.Word += 2;
			BC.Low = VGA.PEEK8(memPtr++);
			BC.High = VGA.PEEK8(memPtr);
			return 5;
		}

		static int ED_4F() {
			IR.Low = AF.High;
			return 2;
		}

		static int ED_50() {
			AF.Low = (byte)(TabSZYXP[DE.High = (byte)GestPort.ReadPort(memPtr = BC.Word)] | (AF.Low & FLAG_C));
			memPtr++;
			return 3;
		}

		static int ED_51() {
			memPtr = BC.Word;
			GestPort.WritePort(memPtr++, DE.High);
			return 3;
		}

		static int ED_52() {
			int z = (memPtr = HL.Word) - DE.Word - (AF.Low & FLAG_C);
			AF.Low = (byte)(FLAG_N | ((HL.Word = (ushort)z) != 0 ? (z >> 8) & FLAGS_S53 : FLAG_Z) | TabHVC[((memPtr++ ^ DE.Word ^ z) >> 8) & 0x190]);
			return 3;
		}

		static int ED_53() {
			memPtr = VGA.PEEK16(PC.Word);
			PC.Word += 2;
			VGA.POKE16(memPtr++, DE.Word);
			return 5;
		}

		static int ED_56() {
			InterruptMode = 1;
			return 1;
		}

		static int ED_57() {
			int a = IR.High;
			AF.Word = (ushort)((a << 8) | TabSZYX[a] | IFF2  | (AF.Low & FLAG_C));
			return 2;
		}

		static int ED_58() {
			AF.Low = (byte)(TabSZYXP[DE.Low = (byte)GestPort.ReadPort(memPtr = BC.Word)] | (AF.Low & FLAG_C));
			memPtr++;
			return 3;
		}

		static int ED_59() {
			memPtr = BC.Word;
			GestPort.WritePort(memPtr++, DE.Low);
			return 3;
		}

		static int ED_5A() {
			int z = (memPtr = HL.Word) + DE.Word + (AF.Low & FLAG_C);
			AF.Low = (byte)(((HL.Word = (ushort)z) != 0 ? (z >> 8) & FLAGS_S53 : FLAG_Z) | TabHVC[(memPtr++ ^ DE.Word ^ z) >> 8]);
			return 3;
		}

		static int ED_5B() {
			memPtr = VGA.PEEK16(PC.Word);
			PC.Word += 2;
			DE.Low = VGA.PEEK8(memPtr++);
			DE.High = VGA.PEEK8(memPtr);
			return 5;
		}

		static int ED_5E() {
			InterruptMode = 2;
			return 1;
		}

		static int ED_5F() {
			int a = IR.Low;
			AF.Word = (ushort)((a << 8) | TabSZYX[a] | IFF2 | (AF.Low & FLAG_C));
			return 2;
		}

		static int ED_60() {
			AF.Low = (byte)(TabSZYXP[HL.High = (byte)GestPort.ReadPort(memPtr = BC.Word)] | (AF.Low & FLAG_C));
			memPtr++;
			return 3;
		}

		static int ED_61() {
			memPtr = BC.Word;
			GestPort.WritePort(memPtr++, HL.High);
			return 3;
		}

		static int ED_62() {
			AF.Low = (byte)(((AF.Low & FLAG_C) != 0 ? 0xB9 : FLAG_Z) | FLAG_N);
			memPtr = ++HL.Word;
			HL.Word = (ushort)-(AF.Low & FLAG_C);
			return 3;
		}

		static int ED_67() {
			int x = VGA.PEEK8(memPtr = HL.Word), y;
			VGA.POKE8(memPtr++, (byte)(y = ((AF.High & 0xF0) << 8) | (((x & 0x0F) << 8) | ((AF.High & 0x0F) << 4) | (x >> 4))));
			AF.Low = (byte)(TabSZYXP[AF.High = (byte)(y >> 8)] | (AF.Low & FLAG_C));
			return 4;
		}

		static int ED_68() {
			AF.Low = (byte)(TabSZYXP[HL.Low = (byte)GestPort.ReadPort(memPtr = BC.Word)] | (AF.Low & FLAG_C));
			memPtr++;
			return 3;
		}

		static int ED_69() {
			memPtr = BC.Word;
			GestPort.WritePort(memPtr++, HL.Low);
			return 3;
		}

		static int ED_6A() {
			int z = ((memPtr = HL.Word) << 1) + (AF.Low & FLAG_C);
			AF.Low = (byte)(((HL.Word = (ushort)z) != 0 ? (z >> 8) & (FLAGS_S53 | FLAG_H) : FLAG_Z) | TabHVC[z >> 8]);
			memPtr++;
			return 3;
		}

		static int ED_6F() {
			int x = VGA.PEEK8(memPtr = HL.Word), y;
			VGA.POKE8(memPtr++, (byte)(y = ((AF.High & 0xF0) << 8) | ((x << 4) | (AF.High & 0x0F))));
			AF.Low = (byte)(TabSZYXP[AF.High = (byte)(y >> 8)] | (AF.Low & FLAG_C));
			return 4;
		}

		static int ED_70() {
			AF.Low = (byte)(TabSZYXP[GestPort.ReadPort(memPtr = BC.Word)] | (AF.Low & FLAG_C));
			memPtr++;
			return 3;
		}

		static int ED_71() {
			memPtr = BC.Word;
			GestPort.WritePort(memPtr++, 0);
			return 3;
		}

		static int ED_72() {
			int z = (memPtr = HL.Word) - SP.Word - (AF.Low & FLAG_C);
			AF.Low = (byte)(FLAG_N | ((HL.Word = (ushort)z) != 0 ? (z >> 8) & FLAGS_S53 : FLAG_Z) | TabHVC[((memPtr++ ^ SP.Word ^ z) >> 8) & 0x190]);
			return 3;
		}

		static int ED_73() {
			memPtr = VGA.PEEK16(PC.Word);
			PC.Word += 2;
			VGA.POKE16(memPtr++, SP.Word);
			return 5;
		}

		static int ED_78() {
			AF.Low = (byte)(TabSZYXP[AF.High = (byte)GestPort.ReadPort(memPtr = BC.Word)] | (AF.Low & FLAG_C));
			memPtr++;
			return 3;
		}

		static int ED_79() {
			memPtr = BC.Word;
			GestPort.WritePort(memPtr++, AF.High);
			return 3;
		}

		static int ED_7A() {
			int z = (memPtr = HL.Word) + SP.Word + (AF.Low & FLAG_C);
			AF.Low = (byte)(((HL.Word = (ushort)z) != 0 ? (z >> 8) & FLAGS_S53 : FLAG_Z) | TabHVC[(memPtr++ ^ SP.Word ^ z) >> 8]);
			return 3;
		}

		static int ED_7B() {
			memPtr = VGA.PEEK16(PC.Word);
			PC.Word += 2;
			SP.Low = VGA.PEEK8(memPtr++);
			SP.High = VGA.PEEK8(memPtr);
			return 5;
		}

		static int ED_A0() {
			int n = VGA.PEEK8(HL.Word++);
			VGA.POKE8(DE.Word++, (byte)n);
			n += AF.High;
			AF.Low = (byte)((AF.Low & FLAGS_SZC) | (--BC.Word != 0 ? 0x04 : 0) | (n & FLAG_3) | ((n << 4) & FLAG_5));
			return 4;
		}

		static int ED_A1() {
			int n = VGA.PEEK8(HL.Word++), z = AF.High - n, f = (AF.High ^ n ^ z) & FLAG_H;
			n = z - (f >> 4);
			AF.Low = (byte)(f | ((n << 4) & FLAG_5) | (n & FLAG_3) | TabSZN[z & 0xFF] | (--BC.Word != 0 ? FLAG_V : 0) | (AF.Low & FLAG_C));
			memPtr++;
			return 3;
		}

		static int ED_A2() {
			int x = GestPort.ReadPort(memPtr = BC.Word);
			VGA.POKE8(HL.Word++, (byte)x);
			AF.Low = (byte)(TabSZYX[--BC.High & 0xFF] | (x >> 6));
			x += ++memPtr & 0xFF;
			AF.Low |= (byte)(((x & 0x0100) != 0 ? 0x11 : 0) | (TabSZYXP[(x & 0x07) ^ BC.High] & 0x04));
			return 4;
		}

		static int ED_A3() {
			AF.Low = (byte)(AF.Low & ~0x90 | (--BC.High != 0 ? 0 : FLAG_Z));
			GestPort.WritePort(memPtr = BC.Word, VGA.PEEK8(HL.Word++));
			memPtr++;
			return 4;
		}

		static int ED_A8() {
			int n = VGA.PEEK8(HL.Word--);
			VGA.POKE8(DE.Word--, (byte)n);
			n += AF.High;
			AF.Low = (byte)((AF.Low & FLAGS_SZC) | (--BC.Word != 0 ? 0x04 : 0) | (n & FLAG_3) | ((n << 4) & FLAG_5));
			return 4;
		}

		static int ED_A9() {
			int n = VGA.PEEK8(HL.Word--), z = AF.High - n, f = (AF.High ^ n ^ z) & FLAG_H;
			n = z - (f >> 4);
			AF.Low = (byte)(f | ((n << 4) & FLAG_5) | (n & FLAG_3) | TabSZN[z & 0xFF] | (--BC.Word != 0 ? 0x04 : 0) | (AF.Low & FLAG_C));
			memPtr--;
			return 3;
		}

		static int ED_AA() {
			int x = GestPort.ReadPort(memPtr = BC.Word);
			VGA.POKE8(HL.Word--, (byte)x);
			AF.Low = (byte)(TabSZYX[--BC.High & 0xFF] | (x >> 6));
			x += --memPtr & 0xFF;
			AF.Low |= (byte)(((x & 0x0100) != 0 ? 0x11 : 0) | (TabSZYXP[(x & 0x07) ^ BC.High] & 0x04));
			return 4;
		}

		static int ED_AB() {
			AF.Low = (byte)(AF.Low & ~0x90 | (--BC.High != 0 ? 0 : FLAG_Z));
			GestPort.WritePort(memPtr = BC.Word, VGA.PEEK8(HL.Word--));
			memPtr--;
			return 4;
		}

		static int ED_B0() {
			int n = VGA.PEEK8(HL.Word++);
			VGA.POKE8(DE.Word++, (byte)n);
			n += AF.High;
			AF.Low = (byte)((AF.Low & FLAGS_SZC) | (--BC.Word != 0 ? 0x04 : 0) | (n & FLAG_3) | ((n << 4) & FLAG_5));
			if (BC.Word != 0) {
				PC.Word--;
				memPtr = PC.Word--;
				return 5;
			}
			return 4;
		}

		static int ED_B1() {
			int n = VGA.PEEK8(HL.Word++), z = AF.High - n, f = (AF.High ^ n ^ z) & FLAG_H;
			n = z - (f >> 4);
			AF.Low = (byte)(f | ((n << 4) & FLAG_5) | (n & FLAG_3) | TabSZN[z & 0xFF] | (--BC.Word != 0 ? 0x04 : 0) | (AF.Low & FLAG_C));
			memPtr++;
			if (z != 0 && BC.Word != 0) {
				PC.Word--;
				memPtr = PC.Word--;
				return 5;
			}
			return 3;
		}

		static int ED_B2() {
			int x = GestPort.ReadPort(memPtr = BC.Word);
			VGA.POKE8(HL.Word++, (byte)x);
			AF.Low = (byte)(TabSZYX[--BC.High & 0xFF] | (x >> 6));
			x += ++memPtr & 0xFF;
			AF.Low |= (byte)(((x & 0x0100) != 0 ? 0x11 : 0) | (TabSZYXP[(x & 0x07) ^ BC.High] & 0x04));
			if (BC.High != 0) {
				PC.Word -= 2;
				return 5;
			}
			return 4;
		}

		static int ED_B3() {
			AF.Low = (byte)(AF.Low & ~0x90 | (--BC.High != 0 ? 0 : FLAG_Z));
			GestPort.WritePort(memPtr = BC.Word, VGA.PEEK8(HL.Word++));
			memPtr++;
			if (BC.High != 0) {
				PC.Word -= 2;
				return 5;
			}
			return 4;
		}

		static int ED_B8() {
			int n = VGA.PEEK8(HL.Word--);
			VGA.POKE8(DE.Word--, (byte)n);
			n += AF.High;
			AF.Low = (byte)((AF.Low & FLAGS_SZC) | (--BC.Word != 0 ? 0x04 : 0) | (n & FLAG_3) | ((n << 4) & FLAG_5));
			if (BC.Word != 0) {
				PC.Word--;
				memPtr = PC.Word--;
				return 5;
			}
			return 4;
		}

		static int ED_B9() {
			int n = VGA.PEEK8(HL.Word--), z = AF.High - n, f = (AF.High ^ n ^ z) & FLAG_H;
			n = z - (f >> 4);
			AF.Low = (byte)(f | ((n << 4) & FLAG_5) | (n & FLAG_3) | TabSZN[z & 0xFF] | (--BC.Word != 0 ? 0x04 : 0) | (AF.Low & FLAG_C));
			memPtr--;
			if (z != 0 && BC.Word != 0) {
				PC.Word--;
				memPtr = PC.Word--;
				return 5;
			}
			return 3;
		}

		static int ED_BA() {
			int x = GestPort.ReadPort(memPtr = BC.Word);
			VGA.POKE8(HL.Word--, (byte)x);
			AF.Low = (byte)(TabSZYX[--BC.High & 0xFF] | (x >> 6));
			x += --memPtr & 0xFF;
			AF.Low |= (byte)(((x & 0x0100) != 0 ? 0x11 : 0) | (TabSZYXP[(x & 0x07) ^ BC.High] & 0x04));
			if (BC.High != 0) {
				PC.Word -= 2;
				return 5;
			}
			return 4;
		}

		static int ED_BB() {
			AF.Low = (byte)(AF.Low & ~0x90 | (--BC.High != 0 ? 0 : FLAG_Z));
			GestPort.WritePort(memPtr = BC.Word, VGA.PEEK8(HL.Word--));
			memPtr--;
			if (BC.High != 0) {
				PC.Word -= 2;
				return 5;
			}
			return 4;
		}

		static int ___EE() {
			AF.Low = TabSZYXP[AF.High ^= VGA.PEEK8(PC.Word++)];
			return 2;
		}

		static int ___F0() {
			if ((AF.Low & FLAG_S) != 0)
				return 2;
			else {
				memPtr = PC.Word = VGA.PEEK16(SP.Word);
				SP.Word += 2;
				return 4;
			}
		}

		static int ___F1() {
			AF.Word = VGA.PEEK16(SP.Word);
			SP.Word += 2;
			return 3;
		}

		static int ___F2() {
			memPtr = VGA.PEEK16(PC.Word);
			PC.Word = (ushort)((AF.Low & FLAG_S) != 0 ? PC.Word + 2 : memPtr);
			return 3;
		}

		static int ___F3() {
			IFF1 = IFF2 = 0;
			return 1;
		}

		static int ___F4() {
			memPtr = VGA.PEEK16(PC.Word);
			PC.Word += 2;
			if ((AF.Low & FLAG_S) != 0)
				return 3;
			else {
				SP.Word -= 2;
				VGA.POKE16(SP.Word, PC.Word);
				PC.Word = memPtr;
				return 5;
			}
		}

		static int ___F5() {
			SP.Word -= 2;
			VGA.POKE16(SP.Word, AF.Word);
			return 4;
		}

		static int ___F6() {
			AF.Low = TabSZYXP[AF.High |= VGA.PEEK8(PC.Word++)];
			return 2;
		}

		static int ___F8() {
			if ((AF.Low & FLAG_S) != 0) {
				memPtr = PC.Word = VGA.PEEK16(SP.Word);
				SP.Word += 2;
				return 4;
			}
			return 2;
		}

		static int ___F9() {
			SP.Word = HL.Word;
			return 2;
		}

		static int ___FA() {
			memPtr = VGA.PEEK16(PC.Word);
			PC.Word = (ushort)((AF.Low & FLAG_S) != 0 ? memPtr : PC.Word + 2);
			return 3;
		}

		static int ___FB() {
			IFF1 = IFF2 = FLAG_V;
			return 1;
		}

		static int ___FC() {
			memPtr = VGA.PEEK16(PC.Word);
			PC.Word += 2;
			if ((AF.Low & FLAG_S) != 0) {
				SP.Word -= 2;
				VGA.POKE16(SP.Word, PC.Word);
				PC.Word = memPtr;
				return 5;
			}
			return 3;
		}

		static int ___FD() {
			IR.Low ^= (byte)(((IR.Low + 1) ^ IR.Low) & 0x7F);
			return 1 + TabInstrFD[LastInstr = VGA.PEEK8(PC.Word++)]();
		}

		static int FD_09() {
			int z = (memPtr = IY.Word) + BC.Word, c = (memPtr++ ^ BC.Word ^ z) >> 8;
			AF.Low = (byte)((AF.Low & FLAGS_SZV) | (c & FLAG_H) | (c >> 8) | ((z >> 8) & FLAGS_53));
			IY.Word = (ushort)z;
			return 3;
		}

		static int FD_19() {
			int z = (memPtr = IY.Word) + DE.Word, c = (memPtr++ ^ DE.Word ^ z) >> 8;
			AF.Low = (byte)((AF.Low & FLAGS_SZV) | (c & FLAG_H) | (c >> 8) | ((z >> 8) & FLAGS_53));
			IY.Word = (ushort)z;
			return 3;
		}

		static int FD_21() {
			IY.Word = VGA.PEEK16(PC.Word);
			PC.Word += 2;
			return 3;
		}

		static int FD_22() {
			memPtr = VGA.PEEK16(PC.Word);
			PC.Word += 2;
			VGA.POKE16(memPtr++, IY.Word);
			return 5;
		}

		static int FD_23() {
			IY.Word++;
			return 2;
		}

		static int FD_24() {
			AF.Low = (byte)((AF.Low & FLAG_C) | TabInc[++IY.High]);
			return 1;
		}

		static int FD_25() {
			AF.Low = (byte)((AF.Low & FLAG_C) | TabDec[--IY.High]);
			return 1;
		}

		static int FD_26() {
			IY.High = VGA.PEEK8(PC.Word++);
			return 2;
		}

		static int FD_29() {
			AF.Low = (byte)((AF.Low & FLAGS_SZV) | ((IY.Word >> 7) & 0x38) | (IY.Word >> 15));
			memPtr = (ushort)(IY.Word + 1);
			IY.Word <<= 1;
			return 3;
		}

		static int FD_2A() {
			memPtr = VGA.PEEK16(PC.Word);
			PC.Word += 2;
			IY.Low = VGA.PEEK8(memPtr++);
			IY.High = VGA.PEEK8(memPtr);
			return 5;
		}

		static int FD_2B() {
			IY.Word--;
			return 2;
		}

		static int FD_2C() {
			AF.Low = (byte)((AF.Low & FLAG_C) | TabInc[++IY.Low]);
			return 1;
		}

		static int FD_2D() {
			AF.Low = (byte)((AF.Low & FLAG_C) | TabDec[--IY.Low]);
			return 1;
		}

		static int FD_2E() {
			IY.Low = VGA.PEEK8(PC.Word++);
			return 2;
		}

		static int FD_34() {
			ushort t = (ushort)(IY.Word + (sbyte)VGA.PEEK8(PC.Word++));
			int x = VGA.PEEK8(t) + 1;
			VGA.POKE8(t, (byte)x);
			AF.Low = (byte)((AF.Low & FLAG_C) | TabInc[(byte)x]);
			return 5;
		}

		static int FD_35() {
			ushort t = (ushort)(IY.Word + (sbyte)VGA.PEEK8(PC.Word++));
			int x = VGA.PEEK8(t) - 1;
			VGA.POKE8(t, (byte)x);
			AF.Low = (byte)((AF.Low & FLAG_C) | TabDec[(byte)x]);
			return 5;
		}

		static int FD_36() {
			VGA.POKE8((ushort)(IY.Word + (sbyte)VGA.PEEK8(PC.Word++)), VGA.PEEK8(PC.Word++));
			return 5;
		}

		static int FD_39() {
			int z = (memPtr = IY.Word) + SP.Word, c = (memPtr++ ^ SP.Word ^ z) >> 8;
			AF.Low = (byte)((AF.Low & FLAGS_SZV) | (c & FLAG_H) | (c >> 8) | ((z >> 8) & FLAGS_53));
			IY.Word = (ushort)z;
			return 3;
		}

		static int FD_44() {
			BC.High = IY.High;
			return 1;
		}

		static int FD_45() {
			BC.High = IY.Low;
			return 1;
		}

		static int FD_46() {
			BC.High = VGA.PEEK8((ushort)(IY.Word + (sbyte)VGA.PEEK8(PC.Word++)));
			return 4;
		}

		static int FD_4C() {
			BC.Low = IY.High;
			return 1;
		}

		static int FD_4D() {
			BC.Low = IY.Low;
			return 1;
		}

		static int FD_4E() {
			BC.Low = VGA.PEEK8((ushort)(IY.Word + (sbyte)VGA.PEEK8(PC.Word++)));
			return 4;
		}

		static int FD_54() {
			DE.High = IY.High;
			return 1;
		}

		static int FD_55() {
			DE.High = IY.Low;
			return 1;
		}

		static int FD_56() {
			DE.High = VGA.PEEK8((ushort)(IY.Word + (sbyte)VGA.PEEK8(PC.Word++)));
			return 4;
		}

		static int FD_5C() {
			DE.Low = IY.High;
			return 1;
		}

		static int FD_5D() {
			DE.Low = IY.Low;
			return 1;
		}

		static int FD_5E() {
			DE.Low = VGA.PEEK8((ushort)(IY.Word + (sbyte)VGA.PEEK8(PC.Word++)));
			return 4;
		}

		static int FD_60() {
			IY.High = BC.High;
			return 1;
		}

		static int FD_61() {
			IY.High = BC.Low;
			return 1;
		}

		static int FD_62() {
			IY.High = DE.High;
			return 1;
		}

		static int FD_63() {
			IY.High = DE.Low;
			return 1;
		}

		static int FD_65() {
			IY.High = IY.Low;
			return 1;
		}

		static int FD_66() {
			HL.High = VGA.PEEK8((ushort)(IY.Word + (sbyte)VGA.PEEK8(PC.Word++)));
			return 4;
		}

		static int FD_67() {
			IY.High = AF.High;
			return 1;
		}

		static int FD_68() {
			IY.Low = BC.High;
			return 1;
		}

		static int FD_69() {
			IY.Low = BC.Low;
			return 1;
		}

		static int FD_6A() {
			IY.Low = DE.High;
			return 1;
		}

		static int FD_6B() {
			IY.Low = DE.Low;
			return 1;
		}

		static int FD_6C() {
			IY.Low = IY.High;
			return 1;
		}

		static int FD_6E() {
			HL.Low = VGA.PEEK8((ushort)(IY.Word + (sbyte)VGA.PEEK8(PC.Word++)));
			return 4;
		}

		static int FD_6F() {
			IY.Low = AF.High;
			return 1;
		}

		static int FD_70() {
			VGA.POKE8((ushort)(IY.Word + (sbyte)VGA.PEEK8(PC.Word++)), BC.High);
			return 4;
		}

		static int FD_71() {
			VGA.POKE8((ushort)(IY.Word + (sbyte)VGA.PEEK8(PC.Word++)), BC.Low);
			return 4;
		}

		static int FD_72() {
			VGA.POKE8((ushort)(IY.Word + (sbyte)VGA.PEEK8(PC.Word++)), DE.High);
			return 4;
		}

		static int FD_73() {
			VGA.POKE8((ushort)(IY.Word + (sbyte)VGA.PEEK8(PC.Word++)), DE.Low);
			return 4;
		}

		static int FD_74() {
			VGA.POKE8((ushort)(IY.Word + (sbyte)VGA.PEEK8(PC.Word++)), HL.High);
			return 4;
		}

		static int FD_75() {
			VGA.POKE8((ushort)(IY.Word + (sbyte)VGA.PEEK8(PC.Word++)), HL.Low);
			return 4;
		}

		static int FD_77() {
			VGA.POKE8((ushort)(IY.Word + (sbyte)VGA.PEEK8(PC.Word++)), AF.High);
			return 4;
		}

		static int FD_7E() {
			AF.High = VGA.PEEK8((ushort)(IY.Word + (sbyte)VGA.PEEK8(PC.Word++)));
			return 4;
		}

		static int FD_7C() {
			AF.High = IY.High;
			return 1;
		}

		static int FD_7D() {
			AF.High = IY.Low;
			return 1;
		}

		static int FD_84() {
			int z = AF.High + IY.High, c = AF.High ^ IY.High ^ z;
			AF.Low = (byte)(TabSZYX[AF.High = (byte)z] | TabHVC[c]);
			return 1;
		}

		static int FD_85() {
			int z = AF.High + IY.Low, c = AF.High ^ IY.Low ^ z;
			AF.Low = (byte)(TabSZYX[AF.High = (byte)z] | TabHVC[c]);
			return 1;
		}

		static int FD_86() {
			int x = VGA.PEEK8((ushort)(IY.Word + (sbyte)VGA.PEEK8(PC.Word++))), z = AF.High + x, c = AF.High ^ x ^ z;
			AF.Low = (byte)(TabSZYX[AF.High = (byte)z] | TabHVC[c]);
			return 4;
		}

		static int FD_8C() {
			int z = AF.High + IY.High + (AF.Low & FLAG_C), c = AF.High ^ IY.High ^ z;
			AF.Low = (byte)(TabSZYX[AF.High = (byte)z] | TabHVC[c]);
			return 1;
		}

		static int FD_8D() {
			int z = AF.High + IY.Low + (AF.Low & FLAG_C), c = AF.High ^ IY.Low ^ z;
			AF.Low = (byte)(TabSZYX[AF.High = (byte)z] | TabHVC[c]);
			return 1;
		}

		static int FD_8E() {
			int x = VGA.PEEK8((ushort)(IY.Word + (sbyte)VGA.PEEK8(PC.Word++))), z = AF.High + x + (AF.Low & FLAG_C), c = AF.High ^ x ^ z;
			AF.Low = (byte)(TabSZYX[AF.High = (byte)z] | TabHVC[c]);
			return 4;
		}

		static int FD_94() {
			int z = AF.High - IY.High, c = AF.High ^ IY.High ^ z;
			AF.Low = (byte)(TabSZYXN[AF.High = (byte)z] | TabHVC[c & 0x190]);
			return 1;
		}

		static int FD_95() {
			int z = AF.High - IY.Low, c = AF.High ^ IY.Low ^ z;
			AF.Low = (byte)(TabSZYXN[AF.High = (byte)z] | TabHVC[c & 0x190]);
			return 1;
		}

		static int FD_96() {
			int x = VGA.PEEK8((ushort)(IY.Word + (sbyte)VGA.PEEK8(PC.Word++))), z = AF.High - x, c = AF.High ^ x ^ z;
			AF.Low = (byte)(TabSZYXN[AF.High = (byte)z] | TabHVC[c & 0x190]);
			return 4;
		}

		static int FD_9C() {
			int z = AF.High - IY.High - (AF.Low & FLAG_C), c = AF.High ^ IY.High ^ z;
			AF.Low = (byte)(TabSZYXN[AF.High = (byte)z] | TabHVC[c & 0x190]);
			return 1;
		}

		static int FD_9D() {
			int z = AF.High - IY.Low - (AF.Low & FLAG_C), c = AF.High ^ IY.Low ^ z;
			AF.Low = (byte)(TabSZYXN[AF.High = (byte)z] | TabHVC[c & 0x190]);
			return 1;
		}

		static int FD_9E() {
			int x = VGA.PEEK8((ushort)(IY.Word + (sbyte)VGA.PEEK8(PC.Word++))), z = AF.High - x - (AF.Low & FLAG_C), c = AF.High ^ x ^ z;
			AF.Low = (byte)(TabSZYXN[AF.High = (byte)z] | TabHVC[c & 0x190]);
			return 4;
		}

		static int FD_A4() {
			AF.Low = TabSZYXHP[AF.High &= IY.High];
			return 1;
		}

		static int FD_A5() {
			AF.Low = TabSZYXHP[AF.High &= IY.Low];
			return 1;
		}

		static int FD_A6() {
			AF.Low = TabSZYXHP[AF.High &= VGA.PEEK8((ushort)(IY.Word + (sbyte)VGA.PEEK8(PC.Word++)))];
			return 4;
		}

		static int FD_AC() {
			AF.Low = TabSZYXP[AF.High ^= IY.High];
			return 1;
		}

		static int FD_AD() {
			AF.Low = TabSZYXP[AF.High ^= IY.Low];
			return 1;
		}

		static int FD_AE() {
			AF.Low = TabSZYXP[AF.High ^= VGA.PEEK8((ushort)(IY.Word + (sbyte)VGA.PEEK8(PC.Word++)))];
			return 4;
		}

		static int FD_B4() {
			AF.Low = TabSZYXP[AF.High |= IY.High];
			return 1;
		}

		static int FD_B5() {
			AF.Low = TabSZYXP[AF.High |= IY.Low];
			return 1;
		}

		static int FD_B6() {
			AF.Low = TabSZYXP[AF.High |= VGA.PEEK8((ushort)(IY.Word + (sbyte)VGA.PEEK8(PC.Word++)))];
			return 4;
		}

		static int FD_BC() {
			int z = AF.High - IY.High;
			AF.Low = (byte)(TabSZN[z & 0xFF] | (IY.High & FLAGS_53) | TabHVC[(AF.High ^ IY.High ^ z) & 0x190]);
			return 1;
		}

		static int FD_BD() {
			int z = AF.High - IY.Low;
			AF.Low = (byte)(TabSZN[z & 0xFF] | (IY.Low & FLAGS_53) | TabHVC[(AF.High ^ IY.Low ^ z) & 0x190]);
			return 1;
		}

		static int FD_BE() {
			int x = VGA.PEEK8((ushort)(IY.Word + (sbyte)VGA.PEEK8(PC.Word++))), z = AF.High - x;
			AF.Low = (byte)(TabSZN[z & 0xFF] | (x & FLAGS_53) | TabHVC[(AF.High ^ x ^ z) & 0x190]);
			return 4;
		}

		static int FD_CB() {
			IR.Low ^= (byte)(((IR.Low + 1) ^ IR.Low) & 0x7F);
			d = (ushort)(IY.Word + (sbyte)VGA.PEEK8(PC.Word++));
			return TabInstrCBFD[LastInstr = VGA.PEEK8(PC.Word++)]();
		}

		static int FC_04() {
			int x = IY.High << 1, c = x >> 8;
			AF.Low = (byte)(TabSZYXP[IY.High = (byte)(x | c)] | c);
			return 2;
		}

		static int FC_05() {
			int x = IY.Low << 1, c = x >> 8;
			AF.Low = (byte)(TabSZYXP[IY.Low = (byte)(x | c)] | c);
			return 2;
		}

		static int FC_0C() {
			int c = IY.High & 0x01;
			AF.Low = (byte)(TabSZYXP[IY.High = (byte)((IY.High >>= 1) | (c << 7))] | c);
			return 2;
		}

		static int FC_0D() {
			int c = IY.Low & 0x01;
			AF.Low = (byte)(TabSZYXP[IY.Low = (byte)((IY.Low >>= 1) | (c << 7))] | c);
			return 2;
		}

		static int FC_14() {
			int x = (AF.Low & FLAG_C) | IY.High << 1;
			AF.Low = (byte)(TabSZYXP[IY.High = (byte)x] | (x >> 8));
			return 2;
		}

		static int FC_15() {
			int x = (AF.Low & FLAG_C) | IY.Low << 1;
			AF.Low = (byte)(TabSZYXP[IY.Low = (byte)x] | (x >> 8));
			return 2;
		}

		static int FC_1C() {
			int c = IY.High & 0x01;
			AF.Low = (byte)(TabSZYXP[IY.High = (byte)((IY.High >>= 1) | (AF.Low << 7))] | c);
			return 2;
		}

		static int FC_1D() {
			int c = IY.Low & 0x01;
			AF.Low = (byte)(TabSZYXP[IY.Low = (byte)((IY.Low >>= 1) | (AF.Low << 7))] | c);
			return 2;
		}

		static int FC_24() {
			int x = IY.High << 1;
			AF.Low = (byte)(TabSZYXP[IY.High = (byte)x] | (x >> 8));
			return 2;
		}

		static int FC_25() {
			int x = IY.Low << 1;
			AF.Low = (byte)(TabSZYXP[IY.Low = (byte)x] | (x >> 8));
			return 2;
		}

		static int FC_2C() {
			int c = IY.High & 0x01;
			AF.Low = (byte)(c | TabSZYXP[IY.High = (byte)(((sbyte)IY.High) >> 1)]);
			return 2;
		}

		static int FC_2D() {
			int c = IY.Low & 0x01;
			AF.Low = (byte)(c | TabSZYXP[IY.Low = (byte)(((sbyte)IY.Low) >> 1)]);
			return 2;
		}

		static int FC_34() {
			int x = IY.High << 1;
			AF.Low = (byte)(TabSZYXP[IY.High = (byte)(x | 0x01)] | (x >> 8));
			return 2;
		}

		static int FC_35() {
			int x = IY.Low << 1;
			AF.Low = (byte)(TabSZYXP[IY.Low = (byte)(x | 0x01)] | (x >> 8));
			return 2;
		}

		static int FC_3C() {
			int c = IY.High & 0x01;
			AF.Low = (byte)(TabSZYXP[IY.High = (byte)(IY.High >> 1)] | c);
			return 2;
		}

		static int FC_3D() {
			int c = IY.Low & 0x01;
			AF.Low = (byte)(TabSZYXP[IY.Low = (byte)(IY.Low >> 1)] | c);
			return 2;
		}

		static int FC_44() {
			AF.Low = (byte)((AF.Low & FLAG_C) | TabSR[IY.High & 1 << ((LastInstr >> 3) & 0x07)] | (IY.High & FLAGS_53));
			return 2;
		}

		static int FC_45() {
			AF.Low = (byte)((AF.Low & FLAG_C) | TabSR[IY.Low & 1 << ((LastInstr >> 3) & 0x07)] | (IY.Low & FLAGS_53));
			return 2;
		}

		static int FC_84() {
			IY.High &= (byte)~(1 << ((LastInstr >> 3) & 0x07));
			return 2;
		}

		static int FC_85() {
			IY.Low &= (byte)~(1 << ((LastInstr >> 3) & 0x07));
			return 2;
		}

		static int FC_C4() {
			IY.High |= (byte)(1 << ((LastInstr >> 3) & 0x07));
			return 2;
		}

		static int FC_C5() {
			IY.Low |= (byte)(1 << ((LastInstr >> 3) & 0x07));
			return 2;
		}

		static int FD_E1() {
			IY.Word = VGA.PEEK16(SP.Word);
			SP.Word += 2;
			SupIrqWaitState = 1;
			return 3;
		}

		static int FD_E3() {
			memPtr = VGA.PEEK16(SP.Word);
			VGA.POKE16(SP.Word, IY.Word);
			IY.Word = memPtr;
			SupIrqWaitState = 2;
			return 6;
		}

		static int FD_E5() {
			SP.Word -= 2;
			VGA.POKE16(SP.Word, IY.Word);
			return 4;
		}

		static int FD_E9() {
			PC.Word = IY.Word;
			return 1;
		}

		static int FD_F9() {
			SP.Word = IY.Word;
			return 2;
		}

		static int ___FE() {
			int x = VGA.PEEK8(PC.Word++), z = AF.High - x;
			AF.Low = (byte)(TabSZN[z & 0xFF] | (x & FLAGS_53) | TabHVC[(AF.High ^ x ^ z) & 0x190]);
			return 2;
		}

		public static int ExecInstr() {
			IR.Low ^= (byte)(((IR.Low + 1) ^ IR.Low) & 0x7F);
			int t = TabInstr[LastInstr = VGA.PEEK8(PC.Word++)]();
			if (IRQ != 0 && IFF1 != 0 && LastInstr != 0xFB) {
				PC.Word += (ushort)Halt;
				SP.Word -= 2;
				VGA.POKE16(SP.Word, PC.Word);
				IR.Low ^= (byte)(((IR.Low + 1) ^ IR.Low) & 0x7F);
				memPtr = PC.Word = (ushort)(InterruptMode < 2 ? 0x38 : VGA.PEEK16(IR.Word | 0xFF));
				t += 6 + (InterruptMode >> 1) - SupIrqWaitState;
				Halt = SupIrqWaitState = IRQ = IFF1 = IFF2 = 0;
				VGA.CntHSync &= 0x1F;
			}
			return t;
		}

		public static void Init() {
			for (int i = 0; i < 256; i++) {
				TabSZYX[i] = (byte)(i == 0 ? FLAG_Z : i & 0xA8);
				TabSZYXN[i] = (byte)(TabSZYX[i] | FLAG_N);
				TabSZN[i] = (byte)(TabSZYXN[i] & (FLAG_S | FLAG_Z | FLAG_N));
				TabSZYXP[i] = (byte)(TabSZYX[i] + ((((i >> 7) ^ (i >> 6) ^ (i >> 5) ^ (i >> 4) ^ (i >> 3) ^ (i >> 2) ^ (i >> 1) ^ i) & 1) > 0 ? 0 : FLAG_V));
				TabSZYXHP[i] = (byte)(TabSZYXP[i] | FLAG_H);
				TabHVC[i << 1] = TabHVC[1 + (i << 1)] = (byte)((i >> 7) | ((i << 1) & FLAG_H) | (i < 64 || i > 191 ? 0 : FLAG_V));
				TabInc[i] = (byte)(((i & 0x0F) > 0 ? 0 : FLAG_H) | TabSZYX[i] | (i == 0x80 ? FLAG_V : 0));
				TabDec[i] = (byte)(FLAG_N | ((i & 0x0F) == 0x0F ? FLAG_H : 0) | TabSZYX[i] | (i == 0x7F ? FLAG_V : 0));
				TabSR[i] = (byte)(FLAG_H | (i > 0 ? i & FLAG_S : FLAG_Z | FLAG_V));
			}
		}

		public static void Reset() {
			IRQ = IFF1 = IFF2 = 0;
			SupIrqWaitState = 0;
			AF.Word = BC.Word = DE.Word = HL.Word = IR.Word = IX.Word = IY.Word = SP.Word = PC.Word = _AF.Word = _BC.Word = _DE.Word = _HL.Word = 0;
			InterruptMode = 0;
		}
	}
}
