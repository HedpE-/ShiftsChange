/*
 * Created by SharpDevelop.
 * User: goncarj3
 * Date: 03-08-2016
 * Time: 13:43
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;

namespace ShiftChanges.Settings
{
	/// <summary>
	/// Description of CurrentUser.
	/// </summary>
	public static class CurrentUser
	{
		public static string UserName
		{
			get;
			private set;
		}
		public static string[] FullName
		{
			get;
			private set;
		}
		public static string Department
		{
			get;
			private set;
		}
		public static string NetworkDomain
		{
			get { return GetUserDetails("NetworkDomain"); }
			private set { }
		}

		public static void InitializeUserProperties()
		{
			UserName = GetUserDetails("Username");
			FullName = GetUserDetails("Name").Split(' ');
			for (int c = 0; c < FullName.Length; c++)
				FullName[c] = FullName[c].Replace(",", string.Empty);
			Department = GetUserDetails("Department").Contains("2nd Line RAN") ? "2nd Line RAN Support" : "1st Line RAN Support";
		}

		/// <summary>
		/// Valid queries: "Name", "Username", "Department" or "NetworkDomain"
		/// </summary>
		public static string GetUserDetails(string detail)
		{
			UserPrincipal ActiveDirectoryUser = UserPrincipal.Current;
			
			if(!string.IsNullOrEmpty(detail))
			{
				switch (detail)
				{
					case "Name":
						if (ActiveDirectoryUser.SamAccountName.Contains("Caramelos"))
							return "Gonçalves, Rui";
						if (ActiveDirectoryUser.SamAccountName.Contains("Hugo Gonçalves"))
							return "Gonçalves, Hugo";
						return ActiveDirectoryUser.DisplayName;
					case "Username":
						return ActiveDirectoryUser.SamAccountName.ToUpper();
					case "Department":
						if (ActiveDirectoryUser.SamAccountName.Contains("CARAMELOS"))
							return "1st Line RAN";
						else
						{
							DirectoryEntry underlyingObject = ActiveDirectoryUser.GetUnderlyingObject() as DirectoryEntry;
							if (underlyingObject.Properties.Contains("department"))
								return underlyingObject.Properties["department"].Value.ToString();
						}
						break;
					case "NetworkDomain":
						return System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;
				}
			}
			return string.Empty;
		}
	}
}
