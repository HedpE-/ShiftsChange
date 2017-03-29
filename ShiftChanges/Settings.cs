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

namespace ShiftChanges
{
	/// <summary>
	/// Description of Settings.
	/// </summary>
	public static class Settings
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
		
		public static FileInfo existingFile = new FileInfo(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Shift 2017_MAR.xlsx");
	}
}
