/*
 * Created by SharpDevelop.
 * User: goncarj3
 * Date: 02/04/2017
 * Time: 10:42
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ShiftChanges.Settings.UI
{
	/// <summary>
	/// Description of AuthenticationSettingsPanel.
	/// </summary>
	public class AuthenticationSettingsPanel : Panel {
		Label label1 = new Label();
		Button button1 = new Button();
		
		Color borderColor = SystemColors.InactiveBorder;
		public Color BorderColor {
			get {
				return borderColor;
			}
			set {
				borderColor = value;
			}
		}
		
		public AuthenticationSettingsPanel() {
			InitializeComponent();
		}
		
		void InitializeComponent() {
			SuspendLayout();
			Name = "AuthenticationSettingsPanel";
			// 
			// label1
			// 
			label1.Dock = DockStyle.Top;
			label1.Name = "label1";
			label1.Height = 45;
			label1.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
			label1.ForeColor = Color.Red;
			label1.Text = "Only Pedro Pancho is allowed to change the Master Key";
			label1.TextAlign = ContentAlignment.MiddleCenter;
			// 
			// button1
			// 
			button1.Name = "button1";
			button1.Size = new Size(130, 21);
			button1.Location = new Point((379 - button1.Width) / 2, label1.Bottom + 5);
			button1.Text = "Change Master Key...";
			button1.UseVisualStyleBackColor = true;
			button1.Enabled = CurrentUser.UserName == "PANCHOPJ";
			button1.Click += (sender, e) => {
				AuthForm auth = new AuthForm(AuthForm.UiModes.Redefine);
				auth.ShowDialog();
			};
			
			Controls.AddRange(new Control[] {
			                  	label1,
			                  	button1
			                  });
			ResumeLayout(false);
		}
		
		protected override void OnPaint(PaintEventArgs e) {
			using (SolidBrush brush = new SolidBrush(BackColor))
				e.Graphics.FillRectangle(brush, ClientRectangle);
			if(BorderStyle != BorderStyle.None)
				e.Graphics.DrawRectangle(new Pen(BorderColor), 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
		}
	}
}
