/*
 * Created by SharpDevelop.
 * User: goncarj3
 * Date: 04/01/2017
 * Time: 00:48
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.ComponentModel;
using System.IO;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Globalization;
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
		
		public static DirectoryInfo ShareRootDir; // = new DirectoryInfo(@"\\vf-pt\fs\ANOC-UK\ANOC-UK 1st LINE\1. RAN 1st LINE\ANOC Master Tool");
		
		public static DirectoryInfo ShiftsDefaultLocation; // = new DirectoryInfo(@"\\vf-pt\fs\ANOC-UK\ANOC-UK 1st LINE\1. RAN 1st LINE\Shifts");
		public static DirectoryInfo OldShiftsDefaultLocation; // = new DirectoryInfo(@"\\vf-pt\fs\ANOC-UK\ANOC-UK 1st LINE\1. RAN 1st LINE\Shifts\Old");
		
		public static FileInfo existingFile = new FileInfo(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Shift 2017_MAR.xlsx");
		
//		public static CultureInfo culture = new CultureInfo("pt-PT");
//		public static DateTime dt = DateTime.Parse(DateTime.Now.ToString(), culture);
		
//		static bool _shareAccess = true;
//		public static bool shareAccess {
//			get {
//				return _shareAccess;
//			}
//			set {
//				_shareAccess = value;
////				if(UserFolder.FullName != null)
////					MainForm.trayIcon.toggleShareAccess();
//			}
//		}
//		
//		public static void CheckShareAccess() {
//			if(ShareRootDir == null) {
//				MessageBox.Show("
//				InitializeSettings();
//			}
//			if(!ShareRootDir.Exists) {
//				InitializeSettings();
//			}
//				
//			if(!IsDirectoryWritable(ShareRootDir.FullName)) {
////				MainForm.trayIcon.showBalloon("Network share access denied","Access to the network share was denied! Your settings file will be created on the following path:" + Environment.NewLine + Environment.NewLine + Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\ANOC Master Tool\\UserSettings\\");
//				shareAccess = false;
//			}
//		}

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
		
		/// <summary>
		/// Valid queries: "Name", "Username", "Department" or "NetworkDomain"
		/// </summary>
		public static string GetUserDetails(string detail)
		{
			UserPrincipal current = UserPrincipal.Current;
			if (detail != null)
			{
				switch(detail) {
					case "Name":
						if(current.SamAccountName.Contains("Caramelos"))
							return "Gonçalves, Rui";
						return current.DisplayName;
					case "Username":
						return current.SamAccountName;
					case "Department":
						if(current.SamAccountName.Contains("Caramelos"))
							return "1st Line RAN";
						else {
							DirectoryEntry underlyingObject = current.GetUnderlyingObject() as DirectoryEntry;
							if(underlyingObject.Properties.Contains("department")) {
								return underlyingObject.Properties["department"].Value.ToString();
							}
						}
						break;
					case "NetworkDomain":
						return System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;
				}
			}
			return string.Empty;
		}
		
		public static void InitializeSettings() {
			if(string.IsNullOrEmpty(SettingsFile.ShareRootFolderPath) ||
			   string.IsNullOrEmpty(SettingsFile.ShiftsFolderPath) ||
			   string.IsNullOrEmpty(SettingsFile.OldShiftsFolderPath)) {
				
			}
			ShareRootDir = new DirectoryInfo(SettingsFile.ShareRootFolderPath);
			ShiftsDefaultLocation = new DirectoryInfo(SettingsFile.ShiftsFolderPath);
			OldShiftsDefaultLocation = new DirectoryInfo(SettingsFile.OldShiftsFolderPath);
		}
	}
}
