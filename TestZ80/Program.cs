using System;
using System.Windows.Forms;

namespace TestZ80 {
	static class Program {
		/// <summary>
		/// Point d'entrée principal de l'application.
		/// </summary>
		[STAThread]
		static void Main() {
			try {
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new Form1());
			}
			catch (Exception ex) {
				MessageBox.Show(ex.StackTrace, ex.Message);
			}
		}
	}
}
