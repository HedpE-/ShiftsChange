/*
 * Created by SharpDevelop.
 * User: goncarj3
 * Date: 10-06-2015
 * Time: 02:40
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace ShiftChanges
{
	/// <summary>
	/// Description of AuthForm.
	/// </summary>
	public sealed partial class AuthForm : Form
	{
		public string Password = string.Empty;
		
		string mode;
		string Mode {
			get { return mode; }
			set {
				mode = value;
				Label secondNewPassLabel = null;
				TextBox secondNewPassTB = null;
				switch(mode) {
					case "Define":
						label2.Text = "New Password";
						secondNewPassLabel = label2;
						secondNewPassLabel.Top += secondNewPassLabel.Height + 3;
						secondNewPassTB = textBox1;
						secondNewPassTB.Name = "secondNewPassTB";
						secondNewPassTB.Top += secondNewPassTB.Height + 3;
						Height += secondNewPassTB.Height + 3;
						break;
					case "Redefine":
						label2.Text = "Old Password";
						Label firstNewPassLabel = label2;
						firstNewPassLabel.Text = "New Password";
						firstNewPassLabel = label2;
						firstNewPassLabel.Top += firstNewPassLabel.Height + 3;
						TextBox firstNewPassTB = textBox1;
						firstNewPassTB.Name = "firstNewPassTB";
						firstNewPassTB.Top += textBox1.Top + 3;
						secondNewPassLabel = firstNewPassLabel;
						secondNewPassLabel.Top += secondNewPassLabel.Height + 3;
						secondNewPassTB = firstNewPassTB;
						secondNewPassTB.Name = "secondNewPassTB";
						secondNewPassTB.Top += secondNewPassTB.Top + 3;
						Height += firstNewPassTB.Height + 3 + secondNewPassTB.Height + 3;
						break;
				}
			}
		}
		
		public AuthForm()
		{
			InitializeComponent();
		}
		
		public AuthForm(string mode)
		{
			InitializeComponent();
			Mode = mode;
		}
		
		void Button1Click(object sender, EventArgs e)
		{
			label3.Text = "Login failed";
			label3.Visible = false;
			if(!string.IsNullOrEmpty(textBox1.Text)) {
				Password = textBox1.Text.EncryptText();
				
				this.Close();
			}
			else {
				label3.Text = "No credentials entered";
				label3.Visible = true;
			}
		}
		
		void Button2Click(object sender, EventArgs e)
		{
			textBox1.Text = string.Empty;
			this.Close();
		}
	}
}
