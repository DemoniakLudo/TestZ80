using System;
using System.Windows.Forms;

namespace TestZ80 {
	static class Program {
		[STAThread]
		
		static void Main() {
			try {
				Application.Run(new Form1());
			}
			catch (Exception ex) {
				MessageBox.Show(ex.StackTrace, ex.Message);
			}
		}
	}
}
