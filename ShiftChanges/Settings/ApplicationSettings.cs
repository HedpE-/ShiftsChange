/*
 * Created by SharpDevelop.
 * User: goncarj3
 * Date: 04/01/2017
 * Time: 00:48
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace ShiftChanges.Settings
{
	/// <summary>
	/// Description of Settings.
	/// </summary>
	public static class ApplicationSettings
	{
		static string incomingRequestsFolder = "Trocas de turno";
		public static string IncomingRequestsFolder {
			get { return incomingRequestsFolder; }
			set { incomingRequestsFolder = value; }
		}
		
		static string approvedRequestsFolder = "Trocas Aprovadas";
		public static string ApprovedRequestsFolder {
			get { return approvedRequestsFolder; }
			set { approvedRequestsFolder = value; }
		}
		
		static string pendingRequestApprovalFolder = "Trocas Pendentes";
		public static string PendingRequestApprovalFolder {
			get { return pendingRequestApprovalFolder; }
			set { pendingRequestApprovalFolder = value; }
		}
		
		public static DirectoryInfo ShareRootDir;
		
		public static DirectoryInfo ShiftsDefaultLocation;
		public static DirectoryInfo OldShiftsDefaultLocation;
		public static DirectoryInfo DevMode_ShiftsDefaultLocation;
		public static DirectoryInfo DevMode_OldShiftsDefaultLocation;
		
		public static bool DevMode = false;

		public static bool IsDirectoryWritable(string dirPath, bool throwIfFails = false)
		{
			bool networkAccess = false;
			var networkAccessCheck = new Thread(() => {
			                                    	try {
			                                    		using(File.Create(Path.Combine(dirPath, Path.GetRandomFileName()), 1, FileOptions.DeleteOnClose)) { }
			                                    		networkAccess = true;
			                                    	}
			                                    	catch {
			                                    		if(throwIfFails)
			                                    			throw;
			                                    	}
			                                    });
			networkAccessCheck.Name = "networkAccessCheck";
			networkAccessCheck.Start();
			if (!networkAccessCheck.Join(TimeSpan.FromSeconds(20))) {
				try {
					networkAccessCheck.Abort();
				}
				catch(ThreadAbortException) { }
				return false;
			}
			return networkAccess;
		}
		
		public static void InitializeSettings() {
			DialogResult ans = DialogResult.OK;
			if(string.IsNullOrEmpty(SettingsFile.ShiftsFolderPath) ||  string.IsNullOrEmpty(SettingsFile.OldShiftsFolderPath)) {
				MessageBox.Show("Default Folders settings not found, please update the application settings.", "Default folders not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Settings.UI.SettingsForm settings = new Settings.UI.SettingsForm("Folders");
				ans = settings.ShowDialog();
			}
			if(ans == DialogResult.OK) {
				ShareRootDir = new DirectoryInfo(SettingsFile.ShareRootFolderPath);
				if(!DevMode) {
					ShiftsDefaultLocation = new DirectoryInfo(SettingsFile.ShiftsFolderPath);
					OldShiftsDefaultLocation = new DirectoryInfo(SettingsFile.OldShiftsFolderPath);
				}
			}
			else {
				MessageBox.Show("Mandatory settings are missing and Settings window was cancelled, terminating application.", "Quitting", MessageBoxButtons.OK, MessageBoxIcon.Error);
				if (Application.MessageLoop)
					Application.Exit();
				else
					Environment.Exit(1);
			}
		}
	}
}
