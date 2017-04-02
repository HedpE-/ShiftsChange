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
		Label label2 = new Label();
		Label label3 = new Label();
		TextBox textBox1 = new TextBox();
		TextBox textBox2 = new TextBox();
		TextBox textBox3 = new TextBox();
		Button button3 = new Button();
		Button button4 = new Button();
		Button button5 = new Button();
		
		Color borderColor = SystemColors.ActiveBorder;
		public Color BorderColor {
			get {
				return borderColor;
			}
			set {
				borderColor = value;
			}
		}
		
		public AuthenticationSettingsPanel()
		{
		}
		
		void InitializeComponent() {
			SuspendLayout();
			Name = "FolderSettingsPanel";
			// 
			// button5
			// 
			button5.Location = new Point(314, 70);
			button5.Name = "button5";
			button5.Size = new Size(59, 21);
			button5.TabIndex = 5;
			button5.Text = "Browse...";
			button5.UseVisualStyleBackColor = true;
			button5.Visible = false;
			// 
			// label3
			// 
			label3.Location = new Point(6, 70);
			label3.Name = "label3";
			label3.Size = new Size(97, 20);
			label3.TabIndex = 7;
			label3.Text = "Old Shifts Folder";
			label3.TextAlign = ContentAlignment.MiddleLeft;
			label3.Visible = false;
			// 
			// label2
			// 
			label2.Location = new Point(6, 44);
			label2.Name = "label2";
			label2.Size = new Size(97, 20);
			label2.TabIndex = 6;
			label2.Text = "Shifts Folder";
			label2.TextAlign = ContentAlignment.MiddleLeft;
			label2.Visible = false;
			// 
			// label1
			// 
			label1.Location = new Point(6, 19);
			label1.Name = "label1";
			label1.Size = new Size(97, 20);
			label1.TabIndex = 5;
			label1.Text = "Share Root Folder";
			label1.TextAlign = ContentAlignment.MiddleLeft;
			label1.Visible = false;
			// 
			// button4
			// 
			button4.Location = new Point(314, 44);
			button4.Name = "button4";
			button4.Size = new Size(59, 21);
			button4.TabIndex = 4;
			button4.Text = "Browse...";
			button4.UseVisualStyleBackColor = true;
			button4.Visible = false;
			// 
			// textBox3
			// 
			textBox3.Location = new Point(109, 71);
			textBox3.Name = "textBox3";
			textBox3.ReadOnly = true;
			textBox3.Size = new Size(199, 20);
			textBox3.TabIndex = 3;
			textBox3.Visible = false;
			// 
			// button3
			// 
			button3.Location = new Point(314, 18);
			button3.Name = "button3";
			button3.Size = new Size(59, 21);
			button3.TabIndex = 2;
			button3.Text = "Browse...";
			button3.UseVisualStyleBackColor = true;
			button3.Visible = false;
			// 
			// textBox2
			// 
			textBox2.Location = new Point(109, 45);
			textBox2.Name = "textBox2";
			textBox2.ReadOnly = true;
			textBox2.Size = new Size(199, 20);
			textBox2.TabIndex = 1;
			textBox2.Visible = false;
			// 
			// textBox1
			// 
			textBox1.Location = new Point(109, 19);
			textBox1.Name = "textBox1";
			textBox1.ReadOnly = true;
			textBox1.Size = new Size(199, 20);
			textBox1.TabIndex = 0;
			textBox1.Visible = false;
			// 
			// panel1
			// 
			Size = new Size(379, 105);
			ClientSize = new Size(379, 105);
			Controls.AddRange(new Control[] {
			                  	label1,
			                  	label2,
			                  	label3,
			                  	textBox1,
			                  	textBox2,
			                  	textBox3,
			                  	button3,
			                  	button4,
			                  	button5
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
