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
		
		public SettingsForm()
		{
			InitializeComponent();
			foldersSettingsPanel.Location =
				authenticationSettingsPanel.Location = new Point(treeView1.Right + 6, 5);
			foldersSettingsPanel.Size =
				authenticationSettingsPanel.Size = new Size(379, 105);
			foldersSettingsPanel.BorderStyle =
				authenticationSettingsPanel.BorderStyle = BorderStyle.FixedSingle;
			
			foldersSettingsPanel.ShiftsFolder = SettingsFile.ShiftsFolderPath;
			foldersSettingsPanel.OldShiftsFolder = SettingsFile.OldShiftsFolderPath;
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
//			if(_currentControl != null) {
//				Controls.Remove(_currentControl);
//				_currentControl = null;
//			}
//			
//			// if no type is bound to the node, just leave the panel empty
//			if (e.Node.Tag == null)
//				return;
//
//			_currentControl = (Control)Activator.CreateInstance(Type.GetType(e.Node.Tag.ToString()));
//			switch(e.Node.Tag.ToString()) {
//				case "ShiftChanges.Settings.UI.FoldersSettingsPanel":
//					((FoldersSettingsPanel)_currentControl).BorderStyle = BorderStyle.FixedSingle;
//					break;
//				case "ShiftChanges.Settings.UI.AuthenticationSettingsPanel":
//					((AuthenticationSettingsPanel)_currentControl).BorderStyle = BorderStyle.FixedSingle;
//					break;
//			}
//			Controls.Add(_currentControl);
		}
		
		void Button2Click(object sender, EventArgs e)
		{
			
		}
	}
}
