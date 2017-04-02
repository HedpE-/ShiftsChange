/*
 * Created by SharpDevelop.
 * User: goncarj3
 * Date: 02/04/2017
 * Time: 10:41
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ShiftChanges.Settings.UI
{
	/// <summary>
	/// Description of FoldersSettingsPanel.
	/// </summary>
	public class FoldersSettingsPanel : Panel {
		Label ShiftsFolderLabel = new Label();
		Label OldShiftsFolderLabel = new Label();
		TextBox ShiftsFolderTextBox = new TextBox();
		TextBox OldShiftsFolderTextBox = new TextBox();
		Button ShiftsFolderButton = new Button();
		Button OldShiftsFolderButton = new Button();
		
		public string ShiftsFolder { get { return ShiftsFolderTextBox.Text; } set { ShiftsFolderTextBox.Text = value; } }
		public string OldShiftsFolder { get { return OldShiftsFolderTextBox.Text; } set { OldShiftsFolderTextBox.Text = value; } }
		
		Color borderColor = SystemColors.InactiveBorder;
		public Color BorderColor {
			get {
				return borderColor;
			}
			set {
				borderColor = value;
			}
		}
		
		public FoldersSettingsPanel() {
			InitializeComponent();
		}
		
		void BrowseButtonsClick(object sender, EventArgs e) {
			Button btn = sender as Button;
			
			FolderBrowserDialog browse = new FolderBrowserDialog();
			browse.SelectedPath = SettingsFile.ShareRootFolderPath;
			browse.ShowNewFolderButton = false;
			DialogResult ans = browse.ShowDialog();
			
			if(ans != DialogResult.Cancel && !string.IsNullOrEmpty(browse.SelectedPath)) {
				switch(btn.Name) {
					case "ShiftsFolderButton":
						ShiftsFolderTextBox.Text = browse.SelectedPath;
						break;
					case "OldShiftsFolderButton":
						OldShiftsFolderTextBox.Text = browse.SelectedPath;
						break;
				}
			}
		}
		
		void InitializeComponent() {
			SuspendLayout();
			Name = "FolderSettingsPanel";
			// 
			// ShiftsFolderLabel
			// 
//			ShiftsFolderLabel.Location = new Point(4, ShareRootFolderLabel.Bottom + 5);
			ShiftsFolderLabel.Location = new Point(4, 15);
			ShiftsFolderLabel.Name = "ShiftsFolderLabel";
			ShiftsFolderLabel.Size = new Size(90, 20);
			ShiftsFolderLabel.Text = "Shifts Folder";
			ShiftsFolderLabel.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// OldShiftsFolderLabel
			// 
			OldShiftsFolderLabel.Location = new Point(4, ShiftsFolderLabel.Bottom + 5);
			OldShiftsFolderLabel.Name = "OldShiftsFolderLabel";
			OldShiftsFolderLabel.Size = new Size(90, 20);
			OldShiftsFolderLabel.Text = "Old Shifts Folder";
			OldShiftsFolderLabel.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// ShiftsFolderTextBox
			// 
			ShiftsFolderTextBox.Location = new Point(ShiftsFolderLabel.Right + 6, ShiftsFolderLabel.Top);
			ShiftsFolderTextBox.Name = "ShiftsFolderTextBox";
			ShiftsFolderTextBox.ReadOnly = true;
			ShiftsFolderTextBox.Size = new Size(208, 20);
			// 
			// OldShiftsFolderTextBox
			// 
			OldShiftsFolderTextBox.Location = new Point(OldShiftsFolderLabel.Right + 6, OldShiftsFolderLabel.Top);
			OldShiftsFolderTextBox.Name = "OldShiftsFolderTextBox";
			OldShiftsFolderTextBox.ReadOnly = true;
			OldShiftsFolderTextBox.Size = new Size(208, 20);
			// 
			// ShiftsFolderButton
			// 
			ShiftsFolderButton.Location = new Point(ShiftsFolderTextBox.Right + 6, ShiftsFolderLabel.Top - 1);
			ShiftsFolderButton.Name = "ShiftsFolderButton";
			ShiftsFolderButton.Size = new Size(59, 21);
			ShiftsFolderButton.Text = "Browse...";
			ShiftsFolderButton.UseVisualStyleBackColor = true;
			ShiftsFolderButton.Click += BrowseButtonsClick;
			// 
			// OldShiftsFolderButton
			// 
			OldShiftsFolderButton.Location = new Point(OldShiftsFolderTextBox.Right + 6, OldShiftsFolderLabel.Top - 1);
			OldShiftsFolderButton.Name = "OldShiftsFolderButton";
			OldShiftsFolderButton.Size = new Size(59, 21);
			OldShiftsFolderButton.Text = "Browse...";
			OldShiftsFolderButton.UseVisualStyleBackColor = true;
			OldShiftsFolderButton.Click += BrowseButtonsClick;
			
			Controls.AddRange(new Control[] {
//			                  	ShareRootFolderLabel,
			                  	ShiftsFolderLabel,
			                  	OldShiftsFolderLabel,
//			                  	ShareRootFolderTextBox,
			                  	ShiftsFolderTextBox,
			                  	OldShiftsFolderTextBox,
//			                  	ShareRootFolderButton,
			                  	ShiftsFolderButton,
			                  	OldShiftsFolderButton
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
