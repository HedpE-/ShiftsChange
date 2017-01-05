/*
 * Created by SharpDevelop.
 * User: goncarj3
 * Date: 31/12/2016
 * Time: 18:31
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
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
		
		static void InitializeService() {
			IncomingRequestsFolder = getExchangeFolderID(Settings.IncomingRequestsFolder);
			
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
		
		#region Event Handlers
		void startServiceClick(object sender, EventArgs e)
		{
			InitializeService();
		}

		static void OnNotificationEvent(object sender, NotificationEventArgs args)
		{
//			System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
//			st.Start();
			foreach(var notification in args.Events){
				if (notification.EventType != EventType.NewMail)
					continue;
				
				PropertySet pSet = new PropertySet(new[]{ ItemSchema.UniqueBody, ItemSchema.DisplayTo, ItemSchema.Subject });
				pSet.RequestedBodyType = BodyType.Text;
				pSet.BasePropertySet = BasePropertySet.FirstClassProperties;
				
				ItemEvent item = (ItemEvent)notification;
				EmailMessage message = EmailMessage.Bind(service, item.ItemId.UniqueId, pSet);
				
				string[] body = message.Body.Text.Split('\n');
				
				string requester = body[0].Substring("Interessado: ".Length);
				string swapped = body[1].Substring("Troca com: ".Length);
				DateTime startDate = Convert.ToDateTime(body[2].Substring("Data início: ".Length));
				DateTime endDate = Convert.ToDateTime(body[3].Substring("Data fim: ".Length));
				
				FileInfo existingFile = new FileInfo(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\New Folder\Shift 2017_JAN - Copy.xlsx");
				
				using (ExcelPackage package = new ExcelPackage(existingFile))
				{
					// get the first worksheet in the workbook
					ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
//					var cell = worksheet.Cell(4,3).Value;
					var objs = worksheet.Cells["c:c"].Where(cell => cell.Value.ToString().Equals(requester)).First();
//						select worksheet.Cells[cell.Start.Row, 3]; // 2 is column b, Email Address
					
					var val = worksheet.Cells["E11"].Style.Border.Bottom.Color.Rgb;
					var val2 = worksheet.Cells["E11"].Value;
				}
			}
			
			
			// The search filter to get unread email.
//			SearchFilter sf = new SearchFilter.SearchFilterCollection(LogicalOperator.And, new SearchFilter[] { new SearchFilter.IsEqualTo(EmailMessageSchema.IsRead, false), new SearchFilter.ContainsSubstring(EmailMessageSchema.Subject, "Troca de turno") });
//
//			ItemView itemview = new ItemView(1000);

			// Fire the query for the unread items.
			// This method call results in a FindItem call to EWS.
//			FindItemsResults<Item> findResults = service.FindItems(IncomingRequestsFolder.Id, sf, itemview);
//
//			if(findResults.Items.Count > 0) {
//				PropertySet itempropertyset = new PropertySet(BasePropertySet.FirstClassProperties);
//				itempropertyset.RequestedBodyType = BodyType.Text;
//
//				foreach (Item item in findResults.Items) {
//					item.Load(itempropertyset);
//					string[] arr = item.Body.Text.Split(new []{ "\r\n" }, StringSplitOptions.None);
//				}
//			}
			
//			st.Stop();
//			var t = st.Elapsed;
		}
		
		static void OnDisconnect(object sender, SubscriptionErrorEventArgs e) {
			if(!connection.IsOpen)
				connection.Open();
		}
		
		void importShiftsFile(object sender, EventArgs e) {
			FileInfo existingFile = new FileInfo(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\New Folder\Shift 2017_JAN - Copy.xlsx");
			
			using (ExcelPackage package = new ExcelPackage(existingFile))
			{
				// get the first worksheet in the workbook
				ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
				var val = worksheet.Cells["E11"].Style.Border.Bottom.Color.Rgb;
				var val2 = worksheet.Cells["E11"].Value;
				int col = 2; //The item description
				// output the data in column 2
				for (int row = 2; row < 5; row++)
					Console.WriteLine("\tCell({0},{1}).Value={2}", row, col, worksheet.Cells[row, col].Value);

				// output the formula in row 5
				Console.WriteLine("\tCell({0},{1}).Formula={2}", 3, 5, worksheet.Cells[3, 5].Formula);
				Console.WriteLine("\tCell({0},{1}).FormulaR1C1={2}", 3, 5, worksheet.Cells[3, 5].FormulaR1C1);

				// output the formula in row 5
				Console.WriteLine("\tCell({0},{1}).Formula={2}", 5, 3, worksheet.Cells[5, 3].Formula);
				Console.WriteLine("\tCell({0},{1}).FormulaR1C1={2}", 5, 3, worksheet.Cells[5, 3].FormulaR1C1);

			} // the using statement automatically calls Dispose() which closes the package.
		}
		
		void menuSettingsClick(object sender, EventArgs e) {
			
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
		
		static Folder getExchangeFolderID(string folder) {
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
