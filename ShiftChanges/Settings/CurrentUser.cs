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
			get { return GetUserDetails("Username"); }
			private set { }
		}
		public static string[] FullName
		{
			get {
				string[] fullName = GetUserDetails("Name").Split(' ');
				for (int c = 0; c < fullName.Length; c++)
					fullName[c] = fullName[c].Replace(",", string.Empty);
				return fullName;
			}
			private set { }
		}
		public static string Department
		{
			get { return GetUserDetails("Department"); }
			private set { }
		}
		public static string NetworkDomain
		{
			get { return GetUserDetails("NetworkDomain"); }
			private set { }
		}
		
		public static string OtherUser;
		static UserPrincipal ActiveDirectoryUser;

//		public static void InitializeUserProperties()
//		{
//			UserName = GetUserDetails("Username");
//			FullName = GetUserDetails("Name").Split(' ');
//			for (int c = 0; c < FullName.Length; c++)
//				FullName[c] = FullName[c].Replace(",", string.Empty);
//			Department = GetUserDetails("Department").Contains("2nd Line RAN") ? "2nd Line RAN Support" : "1st Line RAN Support";
//		}

		/// <summary>
		/// Valid queries: "Name", "Username", "Department" or "NetworkDomain"
		/// </summary>
		public static string GetUserDetails(string detail)
		{
			if(ActiveDirectoryUser == null) {
				if(string.IsNullOrEmpty(OtherUser))
					ActiveDirectoryUser = UserPrincipal.Current;
				else {
					var ctx = new PrincipalContext(ContextType.Domain);
					try {
						ActiveDirectoryUser = UserPrincipal.FindByIdentity(ctx,
						                                                   IdentityType.SamAccountName,
						                                                   OtherUser);
					}
					catch (Exception e) {
						var m = e.Message;
						ActiveDirectoryUser = UserPrincipal.Current;
					}
				}
			}
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
