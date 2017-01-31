﻿/*
 * Created by SharpDevelop.
 * User: goncarj3
 * Date: 31/12/2016
 * Time: 18:31
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using OfficeOpenXml;

[assembly:CLSCompliant(true)]
namespace ShiftChanges
{
	public sealed class NotificationIcon
	{
		NotifyIcon notifyIcon;
		ContextMenu notificationMenu;
		static ExchangeService service;
		static StreamingSubscriptionConnection connection;
		static StreamingSubscriptionConnection connection2;
		static Folder IncomingRequestsFolder;
		static Folder ApprovedRequestsFolder;
		static Folder PendingRequestApprovalFolder;
		
		#region Initialize icon and menu
		public NotificationIcon()
		{
			notifyIcon = new NotifyIcon();
			notificationMenu = new ContextMenu(InitializeMenu());
			
			notifyIcon.MouseUp += IconMouseUp;
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NotificationIcon));
			notifyIcon.Icon = (Icon)resources.GetObject("$this.Icon");
			notifyIcon.ContextMenu = notificationMenu;
		}
		
		MenuItem[] InitializeMenu()
		{
			MenuItem[] menu = new MenuItem[] {
				new MenuItem("Start Service", startServiceClick),
				new MenuItem("Import Shifts File", importShiftsFile),
				new MenuItem("Settings", menuSettingsClick),
				new MenuItem("About", menuAboutClick),
				new MenuItem("Exit", menuExitClick)
			};
			return menu;
		}
		#endregion
		
		#region Main - Program entry point
		/// <summary>Program entry point.</summary>
		/// <param name="args">Command Line Arguments</param>
		[STAThread]
		public static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
//			EmbeddedAssembly.EmbeddedAssembliesInit();
			
			bool isFirstInstance;
			// Please use a unique name for the mutex to prevent conflicts with other programs
			using (Mutex mtx = new Mutex(true, "ShiftChanges", out isFirstInstance)) {
				if (isFirstInstance) {
					NotificationIcon notificationIcon = new NotificationIcon();
					notificationIcon.notifyIcon.Visible = true;
					
					// Create the binding.
					service = new ExchangeService(ExchangeVersion.Exchange2010_SP2);
					service.UseDefaultCredentials = true;
					// Set the URL.
					service.Url = new Uri("https://outlook-north.vodafone.com/ews/exchange.asmx");
					
					Application.Run();
					notificationIcon.notifyIcon.Dispose();
				} else {
					// The application is already running
					Application.Exit();
				}
			} // releases the Mutex
		}
		#endregion
		
		#region Event Handlers
		void startServiceClick(object sender, EventArgs e) {
			IncomingRequestsFolder = getExchangeFolderID(Settings.IncomingRequestsFolder);
			ApprovedRequestsFolder = getExchangeFolderID(Settings.ApprovedRequestsFolder);
			
			if(IncomingRequestsFolder != null) {
				// Subscribe to streaming notifications in the Inbox.
				StreamingSubscription streamingSubscription = service.SubscribeToStreamingNotifications(
					new FolderId[] { IncomingRequestsFolder.Id }, EventType.NewMail);

				// Create a streaming connection to the service object, over which events are returned to the client.
				// Keep the streaming connection open for 30 minutes.
				connection = new StreamingSubscriptionConnection(service, 30);
				connection.AddSubscription(streamingSubscription);
				connection.OnNotificationEvent += OnNotificationEvent;
				connection.OnDisconnect += OnDisconnect;
				connection.Open();
			}
		}

		void OnNotificationEvent(object sender, NotificationEventArgs args)
		{
//			System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
//			st.Start();
			foreach(var notification in args.Events){
				if (notification.EventType != EventType.NewMail)
					continue;
				
				PropertySet pSet = new PropertySet(new[]{ ItemSchema.UniqueBody, ItemSchema.DisplayTo, ItemSchema.Subject, FolderSchema.ParentFolderId });
				pSet.RequestedBodyType = BodyType.Text;
				pSet.BasePropertySet = BasePropertySet.FirstClassProperties;
				
				ItemEvent itemEvent = (ItemEvent)notification;
				Item item = Item.Bind(service, itemEvent.ItemId.UniqueId, pSet);
				if(item is EmailMessage)
					commitRequest(item);
			}
		}
		
		void OnDisconnect(object sender, SubscriptionErrorEventArgs e) {
			if(!connection.IsOpen)
				connection.Open();
		}
		
		void commitRequest(Item item) {
			PropertySet itempropertyset = new PropertySet(BasePropertySet.FirstClassProperties);
			itempropertyset.RequestedBodyType = BodyType.Text;

			item.Load(itempropertyset);
			string[] body = item.Body.Text.Split('\n');
			
			string requester = body[0].Substring("Interessado: ".Length);
			string swapped = body[3].Substring("Troca com: ".Length);
			DateTime reqStartDate = Convert.ToDateTime(body[1].Substring("Data início: ".Length));
			DateTime reqEndDate = Convert.ToDateTime(body[2].Substring("Data fim: ".Length));
			DateTime swapStartDate = Convert.ToDateTime(body[4].Substring("Data início: ".Length));
			DateTime swapEndDate = Convert.ToDateTime(body[5].Substring("Data fim: ".Length));
			
			FileInfo existingFile = new FileInfo(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\New Folder\Shift 2017_FEV.xlsx");
			
			using (ExcelPackage package = new ExcelPackage(existingFile))
			{
				// get the first worksheet in the workbook
				ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
				
				int requesterRow = 0;
				int swappedRow = 0;
				foreach(var cell in worksheet.Cells["c:c"]) {
					if(cell.Value != null) {
						if(requesterRow == 0) {
							if(cell.Value.ToString() == requester)
								requesterRow = cell.Start.Row;
						}
						if(swappedRow == 0) {
							if(cell.Value.ToString() == swapped)
								swappedRow = cell.Start.Row;
						}
						if(requesterRow > 0 && swappedRow > 0)
							break;
					}
				}
				
				var allMergedCells = worksheet.MergedCells;
				List<string> monthRanges = new List<string>();
				foreach(string address in allMergedCells.List) {
					string[] temp = address.Split(':');
					if(temp[0].RemoveLetters() != "1")
						continue;
					if(temp[1].RemoveLetters() != "1")
						continue;
					monthRanges.Add(address);
				}
				
				ArrayList arrList = new ArrayList(monthRanges);
				SortAlphabetLength alphaLen = new SortAlphabetLength();
				arrList.Sort(alphaLen);
				monthRanges = arrList.Cast<string>().ToList();
				
				string reqStartDayColumn = string.Empty;
				string reqEndDayColumn = string.Empty;
				
				string swapStartDayColumn = string.Empty;
				string swapEndDayColumn = string.Empty;
				foreach(var cell in worksheet.Cells[monthRanges[reqStartDate.Month - 1].Replace('1','3')]) {
					if(string.IsNullOrEmpty(reqStartDayColumn)) {
						if(cell.Value.ToString() == reqStartDate.Day.ToString())
							reqStartDayColumn = cell.Address.RemoveDigits();
					}
					if(string.IsNullOrEmpty(reqEndDayColumn)) {
						if(cell.Value.ToString() == reqEndDate.Day.ToString())
							reqEndDayColumn = cell.Address.RemoveDigits();
					}
					if(string.IsNullOrEmpty(swapStartDayColumn)) {
						if(cell.Value.ToString() == swapStartDate.Day.ToString())
							swapStartDayColumn = cell.Address.RemoveDigits();
					}
					if(string.IsNullOrEmpty(swapEndDayColumn)) {
						if(cell.Value.ToString() == swapEndDate.Day.ToString())
							swapEndDayColumn = cell.Address.RemoveDigits();
					}
					if(!string.IsNullOrEmpty(reqStartDayColumn) &&
					   !string.IsNullOrEmpty(reqEndDayColumn) &&
					   !string.IsNullOrEmpty(swapStartDayColumn) &&
					   !string.IsNullOrEmpty(swapEndDayColumn))
						break;
				}
				
				var allContacts = service.ResolveName("Pedro Pancho", ResolveNameSearchLocation.DirectoryOnly, true);
				NameResolution approverContact = null;
				if(allContacts.Count > 0) {
					if(allContacts.Count == 1)
						approverContact = allContacts[0];
					else {
						foreach(NameResolution nr in allContacts) {
							if(nr.Contact.CompanyName == "Vodafone Portugal" && (nr.Contact.Department.StartsWith("First Line Operations UK") || nr.Contact.Department.EndsWith("UK - RAN"))) {
								approverContact = nr;
								break;
							}
						}
					}
				}
				
				var reqCellRangeToCopy = worksheet.Cells[reqStartDayColumn + requesterRow + ":" + reqEndDayColumn + requesterRow].ToList();
				var reqCellRangeToPaste = worksheet.Cells[swapStartDayColumn + requesterRow + ":" + swapEndDayColumn + requesterRow].ToList();
				var swapCellRangeToCopy = worksheet.Cells[swapStartDayColumn + swappedRow + ":" + swapEndDayColumn + swappedRow].ToList();
				var swapCellRangeToPaste = worksheet.Cells[reqStartDayColumn + swappedRow + ":" + reqEndDayColumn + swappedRow].ToList();
				var tempCellRange = worksheet.Cells[swapStartDayColumn + "200:" + swapEndDayColumn + "200"];
				for(int c = 0;c < reqCellRangeToCopy.Count;c++) {
					worksheet.Cells[tempCellRange.Start.Address].Offset(0, c).Value = reqCellRangeToCopy[c].Value;
					reqCellRangeToCopy[c].Value = string.Empty;
				}
				for(int c = 0;c < swapCellRangeToCopy.Count;c++) {
					reqCellRangeToPaste[c].Value = swapCellRangeToCopy[c].Value;
					swapCellRangeToCopy[c].Value = string.Empty;
					reqCellRangeToPaste[c].AddComment("Troca com o " + swapped + " a pedido do " + requester, approverContact.Contact.DisplayName);
				}
				var tempCellRangeList = tempCellRange.ToList();
				for(int c = 0;c < tempCellRange.Count();c++) {
					swapCellRangeToPaste[c].Value = tempCellRangeList[c].Value;
					swapCellRangeToPaste[c].AddComment("Troca com o " + requester + " a pedido do " + requester, approverContact.Contact.DisplayName);
				}
				
				tempCellRange.Clear();
				worksheet.Save();
				package.Save();
				
				item = item.Move(ApprovedRequestsFolder.Id);
			} // the using statement automatically calls Dispose() which closes the package.
		}
		
		void importShiftsFile(object sender, EventArgs e) {
			IncomingRequestsFolder = getExchangeFolderID(Settings.IncomingRequestsFolder);
			ApprovedRequestsFolder = getExchangeFolderID(Settings.ApprovedRequestsFolder);
			// The search filter to get unread email.
			SearchFilter sf = new SearchFilter.SearchFilterCollection(LogicalOperator.And, new SearchFilter.ContainsSubstring(EmailMessageSchema.Subject, "Troca de turno"));

			ItemView itemview = new ItemView(1);

			// Fire the query for the unread items.
			// This method call results in a FindItem call to EWS.
			FindItemsResults<Item> findResults = service.FindItems(IncomingRequestsFolder.Id, sf, itemview);
			
			if(findResults.Items.Count > 0) {
//				foreach (Item item in findResults)
					commitRequest(findResults.Items[0]);
			}
		}
		
		void CreateTask() ////Main method
		{
			Task oTask = new Task(service);
//			oService.ImpersonatedUserId = new ImpersonatedUserId(ConnectingIdType.SmtpAddress, "userTowhomtasktocreate@domain.com");
			
			oTask.ActualWork = 10;

			oTask.Body = new TextBody("Test");

			oTask.Importance = Importance.High;
			oTask.StartDate =  DateTime.Today;
			oTask.Status = TaskStatus.InProgress;
			oTask.Subject = "Test";
			Recurrence.DailyPattern dailyPattern = new Recurrence.DailyPattern(DateTime.Now, 1);
			oTask.Recurrence = dailyPattern;
			oTask.IsReminderSet = true;
			oTask.ReminderDueBy = DateTime.Now;
//			oTask.PercentComplete = 11.10;

//			StringList lstCategory = new StringList();
//			lstCategory.Add("RED Category");
//
//			lstCategory.Add("BLACK Category");
//			lstCategory.Add("BLUE Category");
//			oTask.Categories = lstCategory;

			oTask.Save();
		}
		
		void menuSettingsClick(object sender, EventArgs e) {
			SettingsForm settings = new SettingsForm();
			settings.Show();
		}
		
		void menuAboutClick(object sender, EventArgs e) {
			MessageBox.Show("About This Application");
		}
		
		void menuExitClick(object sender, EventArgs e) {
			Application.Exit();
		}
		
		void IconMouseUp(object sender, MouseEventArgs e) {
			if(e.Button == MouseButtons.Left) {
				System.Reflection.FieldInfo notifyIconNativeWindowInfo = typeof(NotifyIcon).GetField("window", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				NativeWindow notifyIconNativeWindow = (NativeWindow)notifyIconNativeWindowInfo.GetValue(notifyIcon);

				bool visible = notifyIconNativeWindow.Handle == GetForegroundWindow();
				if(!visible) {
					System.Reflection.MethodInfo mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
					mi.Invoke(notifyIcon, null);
				}
			}
		}
		#endregion
		
		Folder getExchangeFolderID(string folder) {
			//SetView
			FolderView view = new FolderView(100);
			view.PropertySet = new PropertySet(BasePropertySet.IdOnly);
			view.PropertySet.Add(FolderSchema.DisplayName);
			view.Traversal = FolderTraversal.Deep;
			FindFoldersResults findFolderResults = service.FindFolders(WellKnownFolderName.Inbox, view);
			//find specific folder
			foreach(Folder f in findFolderResults) {
				if(f.DisplayName == folder)
					return f;
			}
			MessageBox.Show('"' + folder + '"' + " folder not found on Exchange Inbox Folder tree.\n\nPlease check the folders definitions on Settings.","Folder not found",MessageBoxButtons.OK,MessageBoxIcon.Error);
			return null;
		}
		
		[System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr GetForegroundWindow();
		
		static bool RedirectionUrlValidationCallback(string redirectionUrl)
		{
			// The default for the validation callback is to reject the URL.
			bool result = false;

			Uri redirectionUri = new Uri(redirectionUrl);

			// Validate the contents of the redirection URL. In this simple validation
			// callback, the redirection URL is considered valid if it is using HTTPS
			// to encrypt the authentication credentials.
			if (redirectionUri.Scheme == "https")
				result = true;
			return result;
		}
	}
}

public static class ExtensionTools {
	public static String RemoveDigits(this string str) {
		return Regex.Replace(str, @"\d", "");
	}
	
	public static String RemoveLetters(this string str) {
		return Regex.Replace(str, "[^0-9.]", "");
	}
}

public class SortAlphabetLength : IComparer
{
	public int Compare(Object x, Object y)
	{
		if(x.ToString().Length == y.ToString().Length)
			return string.Compare(x.ToString(), y.ToString());
		if(x.ToString().Length > y.ToString().Length)
			return 1;
		return -1;
	}
}