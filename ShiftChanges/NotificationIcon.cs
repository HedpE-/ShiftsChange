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
using System.Collections;
using System.Collections.Generic;
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
			notifyIcon.Icon = Resources.AppIcon;
			notifyIcon.ContextMenu = notificationMenu;
		}
		
		MenuItem[] InitializeMenu()
		{
			MenuItem[] menu = new MenuItem[] {
				new MenuItem("Start Service", startServiceClick),
				new MenuItem("Start for Existing Items", RunOnExistingItems),
				new MenuItem("Settings", menuSettingsClick),
//				new MenuItem("About", menuAboutClick),
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
					
					Settings.SettingsFile.LoadSettingsFile();
//					Settings.CurrentUser.OtherUser = "SOROKIND2";
					DialogResult enteredValidKey = DialogResult.Abort;
					if(string.IsNullOrEmpty(Settings.SettingsFile.MasterKey)) {
						if(Settings.CurrentUser.UserName == "PANCHOPJ") {
							MessageBox.Show("Master Key not found, please define a new key.", "Master Key not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
							AuthForm auth = new AuthForm(AuthForm.UiModes.Define);
							enteredValidKey = auth.ShowDialog();
						}
						else {
							if(Settings.CurrentUser.UserName == "GONCARJ3") {
								AuthForm auth = new AuthForm();
								enteredValidKey = auth.ShowDialog();
							}
							else {
								MessageBox.Show("Master Key not found, for security reasons,\nonly Pedro Pancho is allowed to define a new key.\n\nTerminating application.", "Master Key not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
								if (Application.MessageLoop)
									Application.Exit();
								else
									Environment.Exit(1);
							}
						}
						
					}
					else {
						AuthForm auth = new AuthForm();
						enteredValidKey = auth.ShowDialog();
					}
					if(enteredValidKey == DialogResult.OK) {
						Settings.ApplicationSettings.InitializeSettings();
						
						if(Settings.ApplicationSettings.DevMode && (Settings.ApplicationSettings.DevMode_ShiftsDefaultLocation == null || Settings.ApplicationSettings.DevMode_OldShiftsDefaultLocation == null)) {
							MessageBox.Show("You're running the application on Developer Mode, this mode needs the shifts folders to be local since it's meant for testing.\n\nPlease define local folders.", "Dev Mode", MessageBoxButtons.OK, MessageBoxIcon.Information);
							Settings.UI.SettingsForm settings = new Settings.UI.SettingsForm("Folders");
							DialogResult ans = settings.ShowDialog();
							if(ans != DialogResult.OK) {
								MessageBox.Show("Mandatory settings are missing and Settings window was cancelled, terminating application.", "Quitting", MessageBoxButtons.OK, MessageBoxIcon.Error);
								if (Application.MessageLoop)
									Application.Exit();
								else
									Environment.Exit(1);
							}
						}
						
						// Create the binding.
						service = new ExchangeService(ExchangeVersion.Exchange2010_SP2);
						service.UseDefaultCredentials = true;
						// Set the URL.
						service.Url = new Uri("https://outlook-north.vodafone.com/ews/exchange.asmx");
						
						CheckIfFoldersExist();
						CheckIfRuleExists();
						
						ShiftsFile.Initiate();
						
						Application.Run();
						notificationIcon.notifyIcon.Dispose();
					}
					else {
						MessageBox.Show("Master Key not entered, terminating application.", "Quitting", MessageBoxButtons.OK, MessageBoxIcon.Error);
						if (Application.MessageLoop)
							Application.Exit();
						else
							Environment.Exit(1);
					}
				} else {
					// The application is already running
					Application.Exit();
				}
			} // releases the Mutex
		}
		
		static void CheckIfFoldersExist() {
			bool IncomingFolderCreated = false;
			bool ApprovedFolderCreated = false;
			bool PendingFolderCreated = false;
			Folder Inbox = Folder.Bind(service, WellKnownFolderName.Inbox);
			
			IncomingRequestsFolder = getExchangeFolderID(Settings.ApplicationSettings.IncomingRequestsFolder);
			if(IncomingRequestsFolder == null) {
				IncomingRequestsFolder = new Folder(service);
				IncomingRequestsFolder.DisplayName = Settings.ApplicationSettings.IncomingRequestsFolder;
				IncomingRequestsFolder.Save(Inbox.Id);
				IncomingFolderCreated = true;
			}
			ApprovedRequestsFolder = getExchangeFolderID(Settings.ApplicationSettings.ApprovedRequestsFolder);
			if(ApprovedRequestsFolder == null) {
				ApprovedRequestsFolder = new Folder(service);
				ApprovedRequestsFolder.DisplayName = Settings.ApplicationSettings.ApprovedRequestsFolder;
				ApprovedRequestsFolder.Save(Inbox.Id);
				ApprovedFolderCreated = true;
			}
			PendingRequestApprovalFolder = getExchangeFolderID(Settings.ApplicationSettings.PendingRequestApprovalFolder);
			if(PendingRequestApprovalFolder == null) {
				PendingRequestApprovalFolder = new Folder(service);
				PendingRequestApprovalFolder.DisplayName = Settings.ApplicationSettings.PendingRequestApprovalFolder;
				PendingRequestApprovalFolder.Save(Inbox.Id);
				PendingFolderCreated = true;
			}
			
			if(IncomingFolderCreated || ApprovedFolderCreated || PendingFolderCreated) {
				string message = IncomingFolderCreated ? '"' + Settings.ApplicationSettings.IncomingRequestsFolder + '"' : string.Empty;
				message += ApprovedFolderCreated ? (!string.IsNullOrEmpty(message) ? " and " + '"' + Settings.ApplicationSettings.ApprovedRequestsFolder + '"' : '"' + Settings.ApplicationSettings.ApprovedRequestsFolder + '"') : string.Empty;
				message += PendingFolderCreated ? (!string.IsNullOrEmpty(message) ? " and "  + '"' + Settings.ApplicationSettings.PendingRequestApprovalFolder + '"' : '"' + Settings.ApplicationSettings.PendingRequestApprovalFolder + '"') : string.Empty;
				MessageBox.Show(message + " folders were created in your Inbox.", "Folders created", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}
		
		static void CheckIfRuleExists() {
			bool ruleCreated = false;
			Folder Inbox = Folder.Bind(service, WellKnownFolderName.Inbox);
			
			AlternateId aiAlternateid = new AlternateId(IdFormat.EwsId, Inbox.Id.UniqueId, "mailbox@domain.com");
			AlternateIdBase aiResponse = service.ConvertId(aiAlternateid, IdFormat.EwsId);
			
			RuleCollection inboxRules = null;

			// Get the rules from the user's Inbox.
			try
			{
				inboxRules = service.GetInboxRules(((AlternateId)aiResponse).Mailbox);
			}
			catch (ServiceResponseException ex)
			{
				return;
			}
			
			if(inboxRules.Count(r => r.DisplayName == "Trocas de turno") == 0) {
				Rule newRule = new Rule();

				newRule.DisplayName = "Trocas de turno";
				newRule.Priority = 1;
				newRule.IsEnabled = true;
				newRule.Conditions.ContainsSubjectStrings.Add("Troca de turno");
				newRule.Actions.MoveToFolder = IncomingRequestsFolder.Id;
				newRule.Actions.StopProcessingRules = true;

				CreateRuleOperation createOperation = new CreateRuleOperation(newRule);

				service.UpdateInboxRules(new RuleOperation[] { createOperation }, true);
				ruleCreated = true;
			}
			
			if(ruleCreated)
				MessageBox.Show("A new rule was created and configured on your mailbox.\n" +
				                "If you had any previous rules for emails containing\n" +
				                '"' + "Troca de turno" + '"' + " on the subject, please delete or disable them.\n\n" +
				                "Due to limitation on the Microsoft Exchange API, it isn't possible" +
				                "to run the rule automatically so, please run the rule manually or" +
				                "move all the emmails affected by the rule to the " + '"' + "Troca de turno" + '"' + "folder.",
				                "Rule created", MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}
		
		void startServiceClick(object sender, EventArgs e) {
			if(IncomingRequestsFolder != null) {
				RunOnExistingItems(null, null);
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
				if(item is EmailMessage) {
					if(item.Subject == "Troca de turno") {
						PropertySet itempropertyset = new PropertySet(BasePropertySet.FirstClassProperties);
						itempropertyset.RequestedBodyType = BodyType.Text;
						item.Load(itempropertyset);
						
						ShiftsSwapRequest swapRequest = new ShiftsSwapRequest(item);
						ShiftsFile.ResolveShiftsSwapRequestAdresses(ref swapRequest);
						
						if(swapRequest.Validate()) {
							commitRequest(swapRequest);
							item = item.Move(ApprovedRequestsFolder.Id);
						}
						else
							MessageBox.Show(swapRequest.ValidationMessage);
					}
				}
			}
		}
		
		void OnDisconnect(object sender, SubscriptionErrorEventArgs e) {
			if(!connection.IsOpen)
				connection.Open();
		}
		
		void commitRequest(ShiftsSwapRequest swapRequest) {
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
			
			using (ExcelPackage package = new ExcelPackage(ShiftsFile.existingFile))
			{
				// get the first worksheet in the workbook
				ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
				
				var reqCellRangeToCopy = worksheet.Cells[swapRequest.Requester.StartDateColumn + swapRequest.Requester.PersonRow + ":" +
				                                         swapRequest.Requester.EndDateColumn + swapRequest.Requester.PersonRow].ToList();
				var reqCellRangeToPaste = worksheet.Cells[swapRequest.SwapWith.StartDateColumn + swapRequest.Requester.PersonRow + ":" +
				                                          swapRequest.SwapWith.EndDateColumn + swapRequest.Requester.PersonRow].ToList();
				var swapCellRangeToCopy = worksheet.Cells[swapRequest.SwapWith.StartDateColumn + swapRequest.SwapWith.PersonRow + ":" +
				                                          swapRequest.SwapWith.EndDateColumn + swapRequest.SwapWith.PersonRow].ToList();
				var swapCellRangeToPaste = worksheet.Cells[swapRequest.Requester.StartDateColumn + swapRequest.SwapWith.PersonRow + ":" +
				                                           swapRequest.Requester.EndDateColumn + swapRequest.SwapWith.PersonRow].ToList();
				var tempCellRange = worksheet.Cells[swapRequest.SwapWith.StartDateColumn + "200:" +
				                                    swapRequest.SwapWith.EndDateColumn + "200"];
				
				for(int c = 0;c < reqCellRangeToCopy.Count;c++) {
					worksheet.Cells[tempCellRange.Start.Address].Offset(0, c).Value = reqCellRangeToCopy[c].Value;
					reqCellRangeToCopy[c].Value = string.Empty;
				}
				for(int c = 0;c < swapCellRangeToCopy.Count;c++) {
					reqCellRangeToPaste[c].Value = swapCellRangeToCopy[c].Value;
					swapCellRangeToCopy[c].Value = string.Empty;
					reqCellRangeToPaste[c].AddComment("Troca com " + swapRequest.SwapWith.Name + " a pedido de " + swapRequest.Requester.Name, approverContact.Contact.DisplayName);
				}
				var tempCellRangeList = tempCellRange.ToList();
				for(int c = 0;c < tempCellRange.Count();c++) {
					swapCellRangeToPaste[c].Value = tempCellRangeList[c].Value;
					swapCellRangeToPaste[c].AddComment("Troca com " + swapRequest.SwapWith.Name + " a pedido de " + swapRequest.Requester.Name, approverContact.Contact.DisplayName);
				}
				
				tempCellRange.Clear();
				worksheet.Save();
				package.Save();
			} // the using statement automatically calls Dispose() which closes the package.
		}
		
		void RunOnExistingItems(object sender, EventArgs e) {
			// The search filter to get unread email.
			SearchFilter sf = new SearchFilter.SearchFilterCollection(LogicalOperator.And, new SearchFilter.ContainsSubstring(EmailMessageSchema.Subject, "Troca de turno"));

			ItemView itemview = new ItemView(1);

			
			bool moreItems = true;
			while(moreItems) {
				// Fire the query for the unread items.
				// This method call results in a FindItem call to EWS.
				FindItemsResults<Item> findResults = service.FindItems(IncomingRequestsFolder.Id, sf, itemview);
				moreItems = findResults.MoreAvailable;
				
				PropertySet itempropertyset = new PropertySet(BasePropertySet.FirstClassProperties);
				itempropertyset.RequestedBodyType = BodyType.Text;
				Item item = findResults.Items[0];
				item.Load(itempropertyset);
				
//				foreach (Item item in findResults)
				ShiftsSwapRequest swapRequest = new ShiftsSwapRequest(item);
				ShiftsFile.ResolveShiftsSwapRequestAdresses(ref swapRequest);
				
				if(swapRequest.Validate()) {
					commitRequest(swapRequest);
					item = item.Move(ApprovedRequestsFolder.Id);
				}
				else {
//					MessageBox.Show(swapRequest.ValidationMessage);
					CreateTask(swapRequest);
				}
				
				itemview.Offset += 1;
			}
		}
		
		void CreateTask(ShiftsSwapRequest request) // Main method
		{
			Task oTask = new Task(service);
//			oService.ImpersonatedUserId = new ImpersonatedUserId(ConnectingIdType.SmtpAddress, "userTowhomtasktocreate@domain.com");
			
			oTask.ActualWork = 10;

//			oTask.Body = new TextBody("Test");

			oTask.Importance = Importance.High;
			oTask.StartDate =  DateTime.Today;
			oTask.Status = TaskStatus.InProgress;
			oTask.Subject = "Shifts Change request could not be automatically approved";
			oTask.Body.Text = "Original request:\n\n" + request.OriginalRequest + "\n\nRequest received on: " + request.RequestReceivedDateTime + "\nRejection message: " + request.ValidationMessage;
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
			Settings.UI.SettingsForm settings = new Settings.UI.SettingsForm();
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
//			MessageBox.Show('"' + folder + '"' + " folder not found on Exchange Inbox Folder tree.\n\nPlease check the folders definitions on Settings.","Folder not found",MessageBoxButtons.OK,MessageBoxIcon.Error);
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
	
	public class ShiftsSwapRequest {
		public ShiftsSwapRequestData Requester { get; private set; }
		public ShiftsSwapRequestData SwapWith { get; private set; }
		
		public string OriginalRequest { get; private set; }
		public DateTime RequestReceivedDateTime { get; private set; }
		
		public bool SameDates {
			get { return Requester.StartDateColumn == SwapWith.StartDateColumn && Requester.EndDateColumn == SwapWith.EndDateColumn; }
		}
		public bool Valid { get; private set; }
		public string ValidationMessage { get; private set; }
		
		public ShiftsSwapRequest(Item item) {
			OriginalRequest = item.Body.Text;
			RequestReceivedDateTime = item.DateTimeReceived;
			string[] body = OriginalRequest.Split('\n').Select(s => s.Replace("\r", "").Replace("\n", "")).ToArray();
			
			Requester = new ShiftsSwapRequestData(
				body[0].Substring("Interessado: ".Length),
				Convert.ToDateTime(body[1].Substring("Data início: ".Length)),
				Convert.ToDateTime(body[2].Substring("Data fim: ".Length)));
			
			SwapWith = new ShiftsSwapRequestData(
				body[3].Substring("Troca com: ".Length),
				Convert.ToDateTime(body[4].Substring("Data início: ".Length)),
				Convert.ToDateTime(body[5].Substring("Data fim: ".Length)));
		}
		
		public bool Validate() {
			if(Requester.Role != SwapWith.Role) { // Só pode trocar dentro do grupo
				Valid = false;
				ValidationMessage = "Different role groups";
				return Valid;
			}
			
			// Não pode trocar com outra pessoa num range acima de 10 linhas
			int maxRowIndex = Math.Max(Requester.PersonRow, SwapWith.PersonRow);
			int minRowIndex = Math.Min(Requester.PersonRow, SwapWith.PersonRow);
			int rowsDifference = maxRowIndex - minRowIndex;
			if(rowsDifference > 10) {
				Valid = false;
				ValidationMessage = "More than 10 rows difference between the 2 persons (" + rowsDifference + ")";
				return Valid;
			}
			
			// Formandos não podem fazer trocas
			if(Requester.IsTrainee) {
				Valid = false;
				ValidationMessage = "Requester is a trainee";
				return Valid;
			}
			if(SwapWith.IsTrainee) {
				Valid = false;
				ValidationMessage = "Person to swap is a trainee";
				return Valid;
			}
			
			Requester.UpdateMonthRangeShifts();
			SwapWith.UpdateMonthRangeShifts();
			
			if(Requester.TotalWorkingDaysAfterSwap > 5) { // Não pode ficar a trabalhar mais de 5 dias seguidos
				Valid = false;
				ValidationMessage = "Requester works more than 5 days in a row";
				return Valid;
			}
			if(SwapWith.TotalWorkingDaysAfterSwap > 5) {
				Valid = false;
				ValidationMessage = "Person to swap with works more than 5 days in a row";
				return Valid;
			}
			
			if(Requester.MaxDaysOffAfterSwap > 4) { // Não pode ficar com mais de 4 folgas seguidas
				Valid = false;
				ValidationMessage = "Requester gets more than 4 days off in a row";
				return Valid;
			}
			if(SwapWith.MaxDaysOffAfterSwap > 4) {
				Valid = false;
				ValidationMessage = "Person to swap with gets more than 4 days off in a row";
				return Valid;
			}
			
			// Não pode ficar com menos de 24h entre mudança de turnos para mais cedo.
			List<string> temp = new List<string>();
			temp.Add(Requester.MonthRangeShifts[Requester.StartDateIndex - 1]);
			temp.Add(Requester.MonthRangeShifts[Requester.StartDateIndex]);
			TimeSpan startShiftChangeTime = Requester.CalculateTimeOff(temp);
			TimeSpan endShiftChangeTime = new TimeSpan(24, 0, 0);
			if(Requester.EndDateIndex < Requester.MonthRangeShifts.Length - 1) {
				temp.Clear();
				temp.Add(Requester.MonthRangeShifts[Requester.EndDateIndex]);
				temp.Add(Requester.MonthRangeShifts[Requester.StartDateIndex]);
				endShiftChangeTime = Requester.CalculateTimeOff(temp);
			}
			if(startShiftChangeTime.TotalHours < 16 || endShiftChangeTime.TotalHours < 16) {
				Valid = false;
				ValidationMessage = "Requester gets less than 24h between shifts";
				return Valid;
			}
			
			temp.Clear();
			temp.Add(SwapWith.MonthRangeShifts[Requester.StartDateIndex - 1]);
			temp.Add(SwapWith.MonthRangeShifts[Requester.StartDateIndex]);
			startShiftChangeTime = Requester.CalculateTimeOff(temp);
			endShiftChangeTime = new TimeSpan(24, 0, 0);
			if(Requester.EndDateIndex < Requester.MonthRangeShifts.Length - 1) {
				temp.Clear();
				temp.Add(Requester.MonthRangeShifts[Requester.EndDateIndex]);
				temp.Add(Requester.MonthRangeShifts[Requester.StartDateIndex]);
				endShiftChangeTime = Requester.CalculateTimeOff(temp);
			}
			if(startShiftChangeTime.TotalHours < 16 || endShiftChangeTime.TotalHours < 16) {
				Valid = false;
				ValidationMessage = "Person to swap with gets less than 24h between shifts";
				return Valid;
			}
			
			Valid = true;
//			if(string.IsNullOrEmpty(ValidationMessage))
			ValidationMessage = "Valid for approval";
			
			return Valid;
		}
		
		public class ShiftsSwapRequestData {
			public string Name { get; private set; }
			public int PersonRow { get; set; }
			public Roles Role { get; private set; }
			public bool IsTrainee { get; set; }
			public DateTime StartDate { get; private set; }
			public string StartDateColumn { get; set; }
			public int StartDateIndex { get; set; }
			public DateTime EndDate { get; private set; }
			public string EndDateColumn { get; set; }
			public int EndDateIndex { get; set; }
			public int AffectedMonths { get; set; }
			public string[] SwappedShifts { get; set; }
			public string[] ShiftsToSwap { get; set; }
			public string[] MonthRangeShifts { get; private set; }
			public int TotalWorkingDaysAfterSwap { get; private set; }
			public int MaxDaysOffAfterSwap { get; private set; }
			public TimeSpan ShiftBreakBeforeShiftsToSwap { get; private set; }
			public TimeSpan ShiftBreakAfterShiftsToSwap { get; private set; }
			public TimeSpan TimeOffBeforeShiftsToSwap { get; private set; }
			public TimeSpan TimeOffAfterShiftsToSwap { get; private set; }
			
			public ShiftsSwapRequestData(string name, DateTime startDate, DateTime endDate) {
				Name = name;
				StartDate = startDate;
				EndDate = endDate;
				AffectedMonths = StartDate.Month == EndDate.Month ? 1 : 2;
				Role = ShiftsFile.GetRole(Name);
			}
			
			public void UpdateMonthRangeShifts() {
				MonthRangeShifts = ShiftsFile.GetAllShiftsInMonth(this);
				
				StartDateIndex = StartDate.Day - 1;
				EndDateIndex = AffectedMonths == 1 ? EndDate.Day - 1 : DateTime.DaysInMonth(StartDate.Year, StartDate.Month) + EndDate.Day - 1;
				
				for(int c = StartDateIndex;c <= EndDateIndex;c++)
					MonthRangeShifts[c] = SwappedShifts[c - StartDateIndex];
				
				
				List<string> shiftsList = new List<string>();
				bool previousMonthRequested = false;
				int currentIndex = StartDateIndex;
				while(!string.IsNullOrEmpty(MonthRangeShifts[currentIndex])) {
					if(currentIndex > 0)
						currentIndex--;
					else {
						previousMonthRequested = true;
						List<string> prevMonth = ShiftsFile.RequestPreviousMonth(this);
						StartDateIndex += prevMonth.Count;
						EndDateIndex += prevMonth.Count;
						currentIndex += prevMonth.Count - 1;
						prevMonth.AddRange(MonthRangeShifts);
						MonthRangeShifts = prevMonth.ToArray();
					}
				}
				while(string.IsNullOrEmpty(MonthRangeShifts[currentIndex])) {
					if(currentIndex > 0)
						currentIndex--;
					else {
						previousMonthRequested = true;
						List<string> prevMonth = ShiftsFile.RequestPreviousMonth(this);
						StartDateIndex += prevMonth.Count;
						EndDateIndex += prevMonth.Count;
						currentIndex += prevMonth.Count - 1;
						prevMonth.AddRange(MonthRangeShifts);
						MonthRangeShifts = prevMonth.ToArray();
					}
				}
				
				List<string> list = new List<string>();
				list.Add(MonthRangeShifts[StartDateIndex - 1]);
				list.Add(MonthRangeShifts[StartDateIndex]);
				
				ShiftBreakBeforeShiftsToSwap = list.Count(s => string.IsNullOrEmpty(s)) > 0 ? new TimeSpan(24, 0, 0) : CalculateTimeOff(list);
				
				list.Clear();
				list.Add(MonthRangeShifts[StartDateIndex]);
				list.Add(MonthRangeShifts[StartDateIndex + 1]);
				
				ShiftBreakBeforeShiftsToSwap = list.Count(s => string.IsNullOrEmpty(s)) > 0 ? new TimeSpan(24, 0, 0) : CalculateTimeOff(list);
				
				int year = previousMonthRequested ? (StartDate.Month == 1 ? StartDate.Year - 1 : StartDate.Year) : StartDate.Year;
				int month = previousMonthRequested ? (StartDate.Month == 1 ? 12 : StartDate.Month) : StartDate.Month;
				
				DateTime listFirstDate = new DateTime(year, month, currentIndex + 1, 0, 0, 0);
				bool endOfMonth = false;
				while(currentIndex <= EndDateIndex)
					shiftsList.Add(MonthRangeShifts[currentIndex++]);
				
				while(!string.IsNullOrEmpty(MonthRangeShifts[currentIndex]) && !endOfMonth) {
					shiftsList.Add(MonthRangeShifts[currentIndex++]);
					if(currentIndex > MonthRangeShifts.Length - 1) {
						if(ShiftsFile.LastMonthAvailable > EndDate.Month) {
							var tempList = ShiftsFile.RequestNextMonth(this);
							tempList.InsertRange(0, MonthRangeShifts);
							MonthRangeShifts = tempList.ToArray();
						}
						else {
							endOfMonth = true;
							currentIndex--;
						}
					}
				}
				if(!endOfMonth) {
					while(string.IsNullOrEmpty(MonthRangeShifts[currentIndex]) && !endOfMonth) {
						shiftsList.Add(MonthRangeShifts[currentIndex++]);
						if(currentIndex > MonthRangeShifts.Length - 1) {
							if(ShiftsFile.LastMonthAvailable > EndDate.Month) {
								var tempList = ShiftsFile.RequestNextMonth(this);
								tempList.InsertRange(0, MonthRangeShifts);
								MonthRangeShifts = tempList.ToArray();
							}
							else {
								endOfMonth = true;
								currentIndex--;
							}
						}
					}
					if(!endOfMonth)
						shiftsList.Add(MonthRangeShifts[currentIndex]);
				}
				
				for(int c = 1;c < shiftsList.Count;c++) {
					if(string.IsNullOrEmpty(shiftsList[c]) && TotalWorkingDaysAfterSwap > 0)
						break;
					if(!string.IsNullOrEmpty(shiftsList[c]))
						TotalWorkingDaysAfterSwap++;
				}
				
//				TotalWorkingDaysAfterSwap = shiftsList.Count(s => !string.IsNullOrEmpty(s)) - 2;
				
				int daysOffCount = 0;
				list.Clear();
				list.Add(shiftsList[0]);
				currentIndex = 1;
				while(string.IsNullOrEmpty(shiftsList[currentIndex])) {
					list.Add(shiftsList[currentIndex++]);
					daysOffCount++;
				}
				list.Add(shiftsList[currentIndex]);
				MaxDaysOffAfterSwap = daysOffCount;
				
				TimeOffBeforeShiftsToSwap = CalculateTimeOff(list);
				
				daysOffCount = 0;
				list.Clear();
				list.Add(shiftsList[shiftsList.Count - 1]);
				currentIndex = shiftsList.Count - 2;
				while(string.IsNullOrEmpty(shiftsList[currentIndex])) {
					list.Insert(0, shiftsList[currentIndex--]);
				}
				list.Insert(0, shiftsList[currentIndex]);
				if(daysOffCount > MaxDaysOffAfterSwap)
					MaxDaysOffAfterSwap = daysOffCount;
				
				TimeOffAfterShiftsToSwap = CalculateTimeOff(list);
			}
			
			public TimeSpan CalculateTimeOff(List<string> shiftsList) {
				DateTime currentDate = new DateTime(1,1,1);
				
				int shiftEndHour = 8;
				switch(shiftsList[0]) {
//					case "N":
//						shiftEndHour = 8;
//						break;
					case "M": case "QM":
						shiftEndHour = 16;
						break;
					case "A": case "QA":
						shiftEndHour = 0;
						currentDate = currentDate.AddDays(1);
						break;
					case "I":
						shiftEndHour = 18;
						break;
				}
				
				DateTime timeOffStart = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, shiftEndHour, 0, 0);
				
				currentDate = currentDate.AddDays(shiftEndHour == 0 ? shiftsList.Count - 2 : shiftsList.Count - 1);
				
//				int currentIndex;
//				for(currentIndex = 1;currentIndex < shiftsList.Count;currentIndex++) {
//					if(!string.IsNullOrEmpty(shiftsList[currentIndex])) {
//						currentDate = currentDate.AddDays(shiftEndHour == 0 ? currentIndex - 1 : currentIndex);
//						break;
//					}
//				}
				
				int shiftStartHour = 0;
				switch(shiftsList[shiftsList.Count - 1]) {
//					case "N":
//						shiftStartHour = 0;
//						break;
					case "M": case "QM":
						shiftStartHour = 8;
						break;
					case "A": case "QA":
						shiftStartHour = 16;
						break;
					case "I":
						shiftStartHour = 10;
						break;
				}
				
				DateTime timeOffEnd = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, shiftStartHour, 0, 0);
				
				return timeOffEnd - timeOffStart;
			}
		}
	}
}

public enum Roles {
	ShiftLeader,
	TEF,
	External,
	RAN,
	None
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