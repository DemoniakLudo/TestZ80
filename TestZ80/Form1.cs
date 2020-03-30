using System;
using System.Drawing;
using System.Windows.Forms;

namespace TestZ80 {
	public partial class Form1: Form {
		private const int WM_ACTIVATEAPP = 0x001C;
		private const int WM_COMMAND = 0x0111;
		private const int WM_DESTROY = 0x0002;
		private const int WM_CLOSE = 0x0010;

		private bool finMain = false;
		private Desasm desasm = new Desasm();
		private bool appActive = true;
		private bool doExec = false;

		private void DoSync() {
			led.Checked = UPD.led;
			track.Text = "Piste " + UPD.CurrTrack[0].ToString("00");
			ShowValues();
			BitmapCpc.RefreshBitmap();
			pictureBox1.Refresh();
			Application.DoEvents();
		}

		public Form1() {
			//try {
				InitializeComponent();
				//debugMode.Checked = true;
				Show();
				desasm.Init();
				pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
				BitmapCpc.Init((Bitmap)pictureBox1.Image);
				VGA.DecodeurAdresse = 0;// VGA.ROMSUP_OFF;
				Z80.Init();
				VGA.Init();
				PPI.Init();
				UPD.Init();
				CRTC.Init();
				Keyboard.OptReset();
				int nbCycles = 0;
				while (!finMain) {
					int cycle = Z80.ExecInstr();
					bool DoResync = CRTC.CycleCRTC(cycle);
					UPD.Cycle(cycle);
					nbCycles += cycle;
					if (debugMode.Checked) {
						while (!doExec && debugMode.Checked) {
							DoSync();
						}
						doExec = false;
					}
					else {
						if (DoResync && nbCycles > 1000 || nbCycles > 100000) {
							DoSync();
							nbCycles = 0;
						}
					}
				}
			//}
			//catch (Exception ex) {
			//	MessageBox.Show(ex.StackTrace, ex.Message);
			//}
		}

		private void ShowValues() {
			string str = "";
			desasm.SetLigne(Z80.PC.Word, ref str);
			Instr.Text = str;
			AF.Text = Z80.AF.Word.ToString("X4");
			BC.Text = Z80.BC.Word.ToString("X4");
			DE.Text = Z80.DE.Word.ToString("X4");
			HL.Text = Z80.HL.Word.ToString("X4");
			AF_.Text = Z80._AF.Word.ToString("X4");
			BC_.Text = Z80._BC.Word.ToString("X4");
			DE_.Text = Z80._DE.Word.ToString("X4");
			HL_.Text = Z80._HL.Word.ToString("X4");
			PC.Text = Z80.PC.Word.ToString("X4");
			SP.Text = Z80.SP.Word.ToString("X4");
			IX.Text = Z80.IX.Word.ToString("X4");
			IY.Text = Z80.IY.Word.ToString("X4");
			I.Text = Z80.IR.High.ToString("X2");
			R.Text = Z80.IR.Low.ToString("X2");
			IM.Text = Z80.InterruptMode.ToString();
		}

		//[System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
		//protected override void WndProc(ref Message m) {
		//	switch (m.Msg) {
		//		case WM_ACTIVATEAPP:
		//			appActive = (((int)m.WParam != 0));
		//			this.Invalidate();
		//			break;

		//		case WM_DESTROY:
		//		case WM_CLOSE:
		//			finMain = true;
		//			System.Threading.Thread.Sleep(1000);
		//			break;
		//	}
		//	base.WndProc(ref m);
		//}

		protected override bool ProcessKeyPreview(ref Message m) {
			Keyboard.ProcessKey(m);
			return base.ProcessKeyPreview(ref m);
		}

		private void Form1_FormClosed(object sender, FormClosedEventArgs e) {
			//System.Threading.Thread.Sleep(1000);
			finMain = true;
		}

		private void button1_Click(object sender, EventArgs e) {
			doExec = true;
		}
	}
}
