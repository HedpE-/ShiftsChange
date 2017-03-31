/*
 * Created by SharpDevelop.
 * User: goncarj3
 * Date: 18-08-2016
 * Time: 06:32
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace ShiftChanges.Settings
{
	/// <summary>
	/// Description of SettingsFile.
	/// </summary>
	public static class SettingsFile
	{
		static FileInfo settingsFile { get; set; }
		
		public static string FullName {
			get {
				return settingsFile.FullName;
			}
			private set {}
		}
		
		public static bool Exists {
			get {
				return settingsFile.Exists;
			}
			private set {}
		}
		
		public static string ShareRootFolderPath {
			get {
				return SettingsFileHandler("ShareRootFolderPath");
			}
			set {
				SettingsFileHandler("ShareRootFolderPath", value);
			}
		}
		
		public static string ShiftsFolderPath {
			get {
				return SettingsFileHandler("ShiftsFolderPath");
			}
			set {
				SettingsFileHandler("ShiftsFolderPath", value);
			}
		}
		
		public static string OldShiftsFolderPath {
			get {
				return SettingsFileHandler("OldShiftsFolderPath");
			}
			set {
				SettingsFileHandler("OldShiftsFolderPath", value);
			}
		}
		
		public static string MasterKey {
			get {
				return SettingsFileHandler("MasterKey");
			}
			set {
				SettingsFileHandler("MasterKey", value);
			}
		}
		
		public static void LoadSettingsFile() {
			settingsFile = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\settings.xml");
			if(!settingsFile.Exists)
				CreateSettingsFile();
			CheckXMLIntegrity();
			ApplicationSettings.InitializeSettings();
		}
		
		static void CheckXMLIntegrity()
		{
			XmlNode documentElement;
			XmlElement element;
			XmlDocument document = new XmlDocument();
			
			document.Load(settingsFile.FullName);
			
			if (document.GetElementsByTagName("ShareRootFolderPath").Count == 0) {
				documentElement = document.DocumentElement;
				element = document.CreateElement("ShareRootFolderPath");
//				element.InnerText = UserFolder.FullName;
				documentElement.AppendChild(element);
			}
			if (document.GetElementsByTagName("ShiftsFolderPath").Count == 0) {
				documentElement = document.DocumentElement;
				element = document.CreateElement("ShiftsFolderPath");
//				element.InnerText = "Default";
				documentElement.AppendChild(element);
			}
			if (document.GetElementsByTagName("OldShiftsFolderPath").Count == 0) {
				documentElement = document.DocumentElement;
				element = document.CreateElement("OldShiftsFolderPath");
//				string fileName = Process.GetCurrentProcess().MainModule.FileName;
//				element.InnerText = FileVersionInfo.GetVersionInfo(fileName).FileVersion;
				documentElement.AppendChild(element);
			}
			if (document.GetElementsByTagName("MasterKey").Count == 0) {
				documentElement = document.DocumentElement;
				element = document.CreateElement("MasterKey");
				documentElement.AppendChild(element);
			}
//			if (document.GetElementsByTagName("OIPassword").Count == 0) {
//				documentElement = document.DocumentElement;
//				element = document.CreateElement("OIPassword");
//				documentElement.AppendChild(element);
//			}
//			XmlNodeList elementsByTagName = document.GetElementsByTagName("StartCount");
//			if (elementsByTagName.Count != 0)
//				elementsByTagName[0].ParentNode.RemoveChild(elementsByTagName[0]);
			
			document.Save(settingsFile.FullName);
		}

		static void CreateSettingsFile()
		{
			new XDocument(
				new object[] {
					new XElement("ApplicationSettings", new object[] {
					             	new XElement("ShareRootFolderPath"),
					             	new XElement("ShiftsFolderPath"),
					             	new XElement("OldShiftsFolderPath"),
					             	new XElement("MasterKey")
					             })
				}).Save(settingsFile.FullName);
			settingsFile = new FileInfo(settingsFile.FullName);
		}
		
		public static string SettingsFileHandler(string property)
		{
			XmlDocument document = new XmlDocument();
			document.Load(settingsFile.FullName);
			XmlNodeList elementsByTagName = document.GetElementsByTagName(property);
			
			return elementsByTagName[0].InnerXml;
		}
		
		public static void SettingsFileHandler(string property, string newvalue)
		{
			XmlDocument document = new XmlDocument();
			document.Load(settingsFile.FullName);
			XmlNodeList elementsByTagName = document.GetElementsByTagName(property);
			
			elementsByTagName[0].InnerXml = newvalue;
			document.Save(settingsFile.FullName);
		}
	}
}
