/*
 * Created by SharpDevelop.
 * User: goncarj3
 * Date: 04/01/2017
 * Time: 01:28
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ShiftChanges.Settings.UI
{
	/// <summary>
	/// Description of SettingsForm.
	/// </summary>
	public partial class SettingsForm : Form
	{
		FoldersSettingsPanel foldersSettingsPanel = new FoldersSettingsPanel();
		AuthenticationSettingsPanel authenticationSettingsPanel = new AuthenticationSettingsPanel();
		
		/// <summary>
		/// SettingsForm.
		/// </summary>
		/// <param name="selectItem">Optional: "Auth" or "Folders"</param>"
		public SettingsForm(string selectItem = "")
		{
			InitializeComponent();
			foldersSettingsPanel.Location =
				authenticationSettingsPanel.Location = new Point(treeView1.Right + 6, 5);
			foldersSettingsPanel.Size =
				authenticationSettingsPanel.Size = new Size(379, 105);
			foldersSettingsPanel.BorderStyle =
				authenticationSettingsPanel.BorderStyle = BorderStyle.FixedSingle;
			
			if(ApplicationSettings.DevMode) {
				foldersSettingsPanel.ShiftsFolder = ApplicationSettings.DevMode_ShiftsDefaultLocation == null ?
					string.Empty :
					ApplicationSettings.DevMode_ShiftsDefaultLocation.FullName;
				
				foldersSettingsPanel.OldShiftsFolder = ApplicationSettings.DevMode_OldShiftsDefaultLocation == null ?
					string.Empty :
					ApplicationSettings.DevMode_OldShiftsDefaultLocation.FullName;
			}
			else {
				foldersSettingsPanel.ShiftsFolder = SettingsFile.ShiftsFolderPath;
				foldersSettingsPanel.OldShiftsFolder = SettingsFile.OldShiftsFolderPath;
			}
			if(!string.IsNullOrEmpty(selectItem)) {
				if(selectItem == "Folders")
					treeView1.SelectedNode = treeView1.Nodes[0].Nodes[selectItem == "Folders" ? 1 : 0];
			}
		}
		
		void TreeView1AfterSelect(object sender, TreeViewEventArgs e)
		{
			switch(e.Node.Text) {
				case "Authentication":
					Controls.Remove(foldersSettingsPanel);
					Controls.Add(authenticationSettingsPanel);
					break;
				case "Folders":
					Controls.Remove(authenticationSettingsPanel);
					Controls.Add(foldersSettingsPanel);
					break;
				default:
					Controls.Remove(authenticationSettingsPanel);
					Controls.Remove(foldersSettingsPanel);
					break;
			}
		}
		
		void Button2Click(object sender, EventArgs e) {
			DialogResult = DialogResult.Cancel;
			this.Close();
		}
		
		void Button1Click(object sender, EventArgs e) {
			if(!string.IsNullOrEmpty(foldersSettingsPanel.ShiftsFolder) && !string.IsNullOrEmpty(foldersSettingsPanel.OldShiftsFolder)) {
				if(ApplicationSettings.DevMode) {
					try {
						DirectoryInfo temp = new DirectoryInfo(foldersSettingsPanel.ShiftsFolder);
						DirectoryInfo temp2 = new DirectoryInfo(foldersSettingsPanel.OldShiftsFolder);
						if(new DriveInfo(temp.Root.FullName).DriveType != DriveType.Network && new DriveInfo(temp2.Root.FullName).DriveType != DriveType.Network) {
							ApplicationSettings.DevMode_ShiftsDefaultLocation = temp;
							ApplicationSettings.DevMode_OldShiftsDefaultLocation = temp2;
						}
						else
							throw new Exception();
					}
					catch {
						MessageBox.Show("Invalid path chosen for the default folders on Dev Mode. Choose a local path.", "Invalid path", MessageBoxButtons.OK, MessageBoxIcon.Error);
						return;
					}
				}
				else {
					SettingsFile.ShiftsFolderPath = foldersSettingsPanel.ShiftsFolder;
					SettingsFile.OldShiftsFolderPath = foldersSettingsPanel.OldShiftsFolder;
				}
				DialogResult = DialogResult.OK;
				this.Close();
			}
			else
				MessageBox.Show("Please choose a valid path for the folders settings.", "Folders missing", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}
}
