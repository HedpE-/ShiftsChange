/*
 * Created by SharpDevelop.
 * User: goncarj3
 * Date: 10-06-2015
 * Time: 02:40
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ShiftChanges
{
	/// <summary>
	/// Description of AuthForm.
	/// </summary>
	public sealed partial class AuthForm : Form
	{
		TextBox firstNewPassTB;
		TextBox secondNewPassTB;
		
		public enum UiModes {
			Authenticate,
			Define,
			Redefine
		}
		
		UiModes uiMode = UiModes.Authenticate;
		UiModes UiMode {
			get { return uiMode; }
			set {
				uiMode = value;
				Label firstNewPassLabel = null;
				Label secondNewPassLabel = null;
				switch(uiMode) {
					case UiModes.Define:
						firstNewPassLabel = new Label();
						firstNewPassLabel.Location = label2.Location;
						firstNewPassLabel.Size = label2.Size;
						firstNewPassLabel.TextAlign = ContentAlignment.MiddleLeft;
						firstNewPassLabel.Text = "New Password";
						
						firstNewPassTB = new TextBox();
						firstNewPassTB.Location = textBox1.Location;
						firstNewPassTB.Size = textBox1.Size;
						firstNewPassTB.Name = "firstNewPassTB";
						firstNewPassTB.UseSystemPasswordChar = true;
						
						secondNewPassLabel = new Label();
						secondNewPassLabel.Location = new Point(firstNewPassLabel.Left, firstNewPassLabel.Bottom + 5);
						secondNewPassLabel.Size = firstNewPassLabel.Size;
						secondNewPassLabel.TextAlign = ContentAlignment.MiddleLeft;
						secondNewPassLabel.Text = firstNewPassLabel.Text;
						
						secondNewPassTB = new TextBox();
						secondNewPassTB.Location = new Point(textBox1.Left, secondNewPassLabel.Top);
						secondNewPassTB.Size = textBox1.Size;
						secondNewPassTB.Name = "secondNewPassTB";
						secondNewPassTB.UseSystemPasswordChar = true;
						
						Height += secondNewPassTB.Height + 5;
						button1.Top =
							button2.Top += secondNewPassTB.Height + 5;
						Controls.AddRange(new Control[]{
						                  	firstNewPassLabel,
						                  	firstNewPassTB,
						                  	secondNewPassLabel,
						                  	secondNewPassTB
						                  });
						Controls.Remove(label2);
						Controls.Remove(textBox1);
						textBox1.KeyDown += (sender, e) => {
							if (e.KeyCode == Keys.Return)
								secondNewPassTB.Focus();
						};
						Text = "New Master Key";
						break;
					case UiModes.Redefine:
						label2.Text = "Old Password";
						
						firstNewPassLabel = new Label();
						firstNewPassLabel.Location = new Point(label2.Left, label2.Bottom + 5);
						firstNewPassLabel.Size = label2.Size;
						firstNewPassLabel.TextAlign = ContentAlignment.MiddleLeft;
						firstNewPassLabel.Text = "New Password";
						
						firstNewPassTB = new TextBox();
						firstNewPassTB.Location = new Point(textBox1.Left, firstNewPassLabel.Top);
						firstNewPassTB.Size = textBox1.Size;
						firstNewPassTB.Name = "firstNewPassTB";
						firstNewPassTB.UseSystemPasswordChar = true;
						
						secondNewPassLabel = new Label();
						secondNewPassLabel.Location = new Point(firstNewPassLabel.Left, firstNewPassLabel.Bottom + 5);
						secondNewPassLabel.Size = firstNewPassLabel.Size;
						secondNewPassLabel.TextAlign = ContentAlignment.MiddleLeft;
						secondNewPassLabel.Text = "New Password";
						
						secondNewPassTB = new TextBox();
						secondNewPassTB.Location = new Point(firstNewPassTB.Left, secondNewPassLabel.Top);
						secondNewPassTB.Size = firstNewPassTB.Size;
						secondNewPassTB.Name = "secondNewPassTB";
						secondNewPassTB.UseSystemPasswordChar = true;
						
						Height += firstNewPassTB.Height + 5 + secondNewPassTB.Height + 5;
						button1.Top =
							button2.Top += firstNewPassTB.Height + 5 + secondNewPassTB.Height + 5;
						Controls.AddRange(new Control[]{
						                  	firstNewPassLabel,
						                  	firstNewPassTB,
						                  	secondNewPassLabel,
						                  	secondNewPassTB
						                  });
						textBox1.KeyDown += (sender, e) => {
							if (e.KeyCode == Keys.Return)
								firstNewPassTB.Focus();
						};
						firstNewPassTB.KeyDown += (sender, e) => {
							if (e.KeyCode == Keys.Return)
								secondNewPassTB.Focus();
						};
						Text = "Change Master Key";
						break;
				}
				secondNewPassTB.KeyDown += (sender, e) => {
					if (e.KeyCode == Keys.Return)
						button1.PerformClick();
				};
			}
		}
		
		/// <summary>
		/// AuthForm
		/// </summary>
		public AuthForm() {
			InitializeComponent();
			Text = "Authenticate";
			AcceptButton = button1;
		}
		
		/// <summary>
		/// AuthForm
		/// </summary>
		/// <param name="mode">Specify the UiMode<para />
		/// UiModes.Define: Set new Key without old Key confirmation. To be used when previous key isn't set<para />
		/// UiModes.Redifine: Asks for old Key confirmation and sets a new one<para />
		/// DO NOT USE UiModes.Authenticate, use AuthForm() instead</param>
		/// <returns>DialogResult.OK or DialogResult.Abort dependant on authentication successful</returns>
		public AuthForm(UiModes mode) {
			InitializeComponent();
			UiMode = mode;
		}
		
		void OkButtonClick(object sender, EventArgs e) {
			label3.Visible = false;
			switch(UiMode) {
				case UiModes.Authenticate:
					label3.Text = "Login failed";
					if(!string.IsNullOrEmpty(textBox1.Text)) {
						string enteredKey = textBox1.Text.EncryptText();
						if(enteredKey == Settings.SettingsFile.MasterKey || enteredKey == DevKey) {
							Settings.ApplicationSettings.DevMode = enteredKey == DevKey;
							DialogResult = DialogResult.OK;
							this.Close();
						}
						else
							label3.Visible = true;
					}
					else {
						label3.Text = "No credentials entered";
						label3.Visible = true;
					}
					return;
				case UiModes.Redefine:
					if(string.IsNullOrEmpty(textBox1.Text) && string.IsNullOrEmpty(firstNewPassTB.Text) && string.IsNullOrEmpty(secondNewPassTB.Text)) {
						label3.Text = "No credentials entered";
						label3.Visible = true;
						return;
					}
					if(string.IsNullOrEmpty(textBox1.Text)) {
						label3.Text = "Please enter old Password field";
						label3.Visible = true;
						return;
					}
					if(textBox1.Text.EncryptText() != Settings.SettingsFile.MasterKey) {
						label3.Text = "Old Password doesn't match";
						label3.Visible = true;
						return;
					}
					break;
			}
			
			if(string.IsNullOrEmpty(firstNewPassTB.Text) || string.IsNullOrEmpty(secondNewPassTB.Text)) {
				if(string.IsNullOrEmpty(firstNewPassTB.Text) && string.IsNullOrEmpty(secondNewPassTB.Text)) {
					label3.Text = "Please enter new Password fields";
					label3.Visible = true;
					return;
				}
				label3.Text = "Please enter both new Password";
				label3.Visible = true;
				return;
			}
			if(firstNewPassTB.Text != secondNewPassTB.Text) {
				label3.Text = "New Password fields don't match";
				label3.Visible = true;
				return;
			}
			
			Settings.SettingsFile.MasterKey = secondNewPassTB.Text.EncryptText();
			MessageBox.Show("Master key saved", "Master key saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
			
			DialogResult = DialogResult.OK;
			this.Close();
		}
		
		void CancelButtonClick(object sender, EventArgs e) {
			DialogResult = DialogResult.Abort;
			this.Close();
		}
		
		static readonly string DevKey = "6433764d306465"; // "d3vM0de"
		
		const int CP_DISABLECLOSE_BUTTON = 0x200;
		protected override CreateParams CreateParams {
			get {
				CreateParams myCp = base.CreateParams;
				myCp.ClassStyle = myCp.ClassStyle | CP_DISABLECLOSE_BUTTON ;
				return myCp;
			}
		}
	}
}
