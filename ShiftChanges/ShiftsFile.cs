﻿/*
 * Created by SharpDevelop.
 * User: goncarj3
 * Date: 18-08-2016
 * Time: 06:32
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using OfficeOpenXml;

namespace ShiftChanges
{
	/// <summary>
	/// Description of ShiftsFile.
	/// </summary>
	public static class ShiftsFile
	{
		public static FileInfo existingFile;
		
		static ExcelPackage package;
		
		static ArrayList monthRanges;

		public static int LastMonthAvailable {
			get {
				int lastMonth = DateTime.Now.Month;
				int personRow = 0;
				foreach(var cell in package.Workbook.Worksheets[1].Cells["c:c"]) {
					if(cell.Value != null) {
						string[] nameArr = RAN[0].ToUpper().Split(' ');
						if(cell.Text.ToUpper().RemoveDiacritics().Contains(nameArr[0].ToUpper().RemoveDiacritics()) &&
						   cell.Text.ToUpper().RemoveDiacritics().Contains(nameArr[1].ToUpper().RemoveDiacritics()))
							personRow = cell.Start.Row;
					}
				}
				string[] nextMonthShifts = null;
				do {
					nextMonthShifts = GetAllShiftsInMonth(personRow, ++lastMonth);
					for(int c = 0;c < nextMonthShifts.Length;c++)
						if(nextMonthShifts[c].Contains("H") || nextMonthShifts[c] == "B" || nextMonthShifts[c] == "L")
							nextMonthShifts[c] = string.Empty;
				}
				while(nextMonthShifts.Count(s => !string.IsNullOrEmpty(s)) > 0);
				
				return --lastMonth;
			}
			private set { }
		}

		static int FindPersonRow(string name) {
			foreach(var cell in package.Workbook.Worksheets[1].Cells["c:c"]) {
				if(cell.Value != null) {
					string[] nameArr = name.ToUpper().Split(' ');
					if(cell.Text.ToUpper().RemoveDiacritics().Contains(nameArr[0].ToUpper().RemoveDiacritics()) &&
					   cell.Text.ToUpper().RemoveDiacritics().Contains(nameArr[1].ToUpper().RemoveDiacritics()))
						return cell.Start.Row;
				}
			}
			return 0;
		}
		
		public static List<string> ShiftLeaders {
			get;
			private set;
		}
		
		public static List<string> RAN {
			get;
			private set;
		}
		
		public static List<string> TEF {
			get;
			private set;
		}
		
		public static List<string> External {
			get;
			private set;
		}
		
		static int LastRow { get; set; }
		
		public static void Initiate() {
			existingFile = GetShiftsFile(DateTime.Now.Year);
			
		retry:
			try {
				package = new ExcelPackage(existingFile);
			}
			catch(IOException e) {
				int errorCode = (int)(e.HResult & 0x0000FFFF);
				if(errorCode == 32) {
					DialogResult ans = MessageBox.Show("File '" + existingFile.FullName + "' is currently in use, please close it and retry.", "File in use", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
					if(ans == DialogResult.Retry)
						goto retry;
					
					if(Application.MessageLoop)
						Application.Exit();
					else
						Environment.Exit(1);
				}
			}
			
			if (package.File != null) {
				var allMergedCells = package.Workbook.Worksheets[1].MergedCells;
				monthRanges = new ArrayList();
				foreach(string address in allMergedCells.List) {
					string[] temp = address.Split(':');
					if(temp[0].RemoveLetters() != "1")
						continue;
					if(temp[1].RemoveLetters() != "1")
						continue;
					monthRanges.Add(address);
				}
				
				SortAlphabetLength alphaLen = new SortAlphabetLength();
				monthRanges.Sort(alphaLen);
				
				var currentCell = package.Workbook.Worksheets[1].Cells["a:a"].FirstOrDefault(c => c.Text == "Closure Code").Offset(2, 0);
				while(!string.IsNullOrEmpty(currentCell.Text)) {
					if(ShiftLeaders == null)
						ShiftLeaders = new List<string>();
					ShiftLeaders.Add(currentCell.Offset(0, 2).Text);
					currentCell = package.Workbook.Worksheets[1].Cells[currentCell.Start.Row + 1, 1];
				}
				while(string.IsNullOrEmpty(currentCell.Text)) {
					currentCell = package.Workbook.Worksheets[1].Cells[currentCell.Start.Row + 1, 1];
				}
				while(!string.IsNullOrEmpty(currentCell.Text)) {
					if(TEF == null)
						TEF = new List<string>();
					TEF.Add(currentCell.Offset(0, 2).Text);
					currentCell = package.Workbook.Worksheets[1].Cells[currentCell.Start.Row + 1, 1];
				}
				currentCell = package.Workbook.Worksheets[1].Cells[currentCell.Start.Row + 1, 1];
				while(!string.IsNullOrEmpty(currentCell.Text)) {
					if(External == null)
						External = new List<string>();
					External.Add(currentCell.Offset(0, 2).Text);
					currentCell = package.Workbook.Worksheets[1].Cells[currentCell.Start.Row + 1, 1];
				}
				currentCell = package.Workbook.Worksheets[1].Cells[currentCell.Start.Row + 1, 1];
				while(!string.IsNullOrEmpty(currentCell.Text)) {
					if(RAN == null)
						RAN = new List<string>();
					RAN.Add(currentCell.Offset(0, 2).Text);
					currentCell = package.Workbook.Worksheets[1].Cells[currentCell.Start.Row + 1, 1];
				}
			}
			
			if(package.File.FullName.Contains(Environment.SpecialFolder.ApplicationData.ToString()))
				File.Delete(package.File.FullName);
		}
		
		static FileInfo GetShiftsFile(int year) {
			DirectoryInfo folderToSearch = null;
			if(Settings.ApplicationSettings.DevMode) {
				folderToSearch = year == DateTime.Now.Year ?
					Settings.ApplicationSettings.DevMode_ShiftsDefaultLocation :
					Settings.ApplicationSettings.DevMode_OldShiftsDefaultLocation;
			}
			else {
				folderToSearch = year == DateTime.Now.Year ?
					Settings.ApplicationSettings.ShiftsDefaultLocation :
					Settings.ApplicationSettings.OldShiftsDefaultLocation;
			}
			
			FileInfo[] foundFiles = folderToSearch.GetFiles("*shift*" + year + "*.xlsx", SearchOption.TopDirectoryOnly).Where(f => !f.Name.StartsWith("~$")).ToArray();
			
			FileInfo foundFile = null;
			if(foundFiles.Length > 1)
				foundFile = foundFiles.Aggregate((f1, f2) => f1.Length > f2.Length ? f1 : f2);
			else {
				if(foundFiles.Length == 1)
					foundFile = foundFiles[0];
			}
			
			return foundFile;
		}
		
		public static string[] GetAllShiftsInMonth(ShiftsSwapRequest.ShiftsSwapRequestData request) {
			List<string> list = null;
			if(request.PersonRow > 0) {
				list = new List<string>();
				var personRange = package.Workbook.Worksheets[1].Cells[monthRanges[request.StartDate.Month - 1].ToString().Replace("1",request.PersonRow.ToString())];
				for(int c = personRange.Start.Column;c <= personRange.End.Column;c++) {
					var cell = package.Workbook.Worksheets[1].Cells[request.PersonRow, c];
					if(cell.Value == null)
						list.Add(string.Empty);
					else
						list.Add(cell.Text);
				}
				
				if(request.AffectedMonths > 1) {
					personRange = package.Workbook.Worksheets[1].Cells[monthRanges[request.EndDate.Month - 1].ToString().Replace("1",request.PersonRow.ToString())];
					for(int c = personRange.Start.Column;c <= personRange.End.Column;c++) {
						var cell = package.Workbook.Worksheets[1].Cells[request.PersonRow, c];
						if(cell.Value == null)
							list.Add(string.Empty);
						else
							list.Add(cell.Text);
					}
				}
			}
			return list == null ? null : list.ToArray();
		}
		
		public static string[] GetAllShiftsInMonth(int personRow, int month) {
			List<string> list = null;
			if(personRow > 0) {
				list = new List<string>();
				var personRange = package.Workbook.Worksheets[1].Cells[monthRanges[month - 1].ToString().Replace("1",personRow.ToString())];
				for(int c = personRange.Start.Column;c <= personRange.End.Column;c++) {
					var cell = package.Workbook.Worksheets[1].Cells[personRow, c];
					if(cell.Value == null)
						list.Add(string.Empty);
					else
						list.Add(cell.Text);
				}
			}
			return list == null ? null : list.ToArray();
		}
		
		public static List<string> RequestPreviousMonth(ShiftsSwapRequest.ShiftsSwapRequestData request) {
			List<string> list = new List<string>();
			if(request.StartDate.Month > 1)
				list = GetAllShiftsInMonth(request.PersonRow, request.StartDate.Month - 1).ToList();
			else {
				FileInfo foundFile = GetShiftsFile(request.StartDate.Year - 1);
				
				ExcelPackage pack = null;
				try {
					pack = new ExcelPackage(foundFile);
				}
				catch {
					if(existingFile != null) {
						FileInfo tempShiftsFile = existingFile.CopyTo(Environment.SpecialFolder.ApplicationData + "\\" + existingFile.Name, true);
						pack = new ExcelPackage(tempShiftsFile);
					}
					
				}
				
				if (pack.File != null) {
					var allMergedCells = pack.Workbook.Worksheets[1].MergedCells;
					ArrayList monthRangesArr = new ArrayList();
					foreach(string address in allMergedCells.List) {
						string[] temp = address.Split(':');
						if(temp[0].RemoveLetters() != "1")
							continue;
						if(temp[1].RemoveLetters() != "1")
							continue;
						monthRangesArr.Add(address);
					}
					
					SortAlphabetLength alphaLen = new SortAlphabetLength();
					monthRanges.Sort(alphaLen);
					
					int personRow = 0;
					foreach(var cell in pack.Workbook.Worksheets[1].Cells["c:c"]) {
						if(cell.Value != null) {
							string[] nameArr = request.Name.ToUpper().RemoveDiacritics().Split(' ');
							if(cell.Text.ToUpper().RemoveDiacritics().Contains(nameArr[0]) &&
							   cell.Text.ToUpper().RemoveDiacritics().Contains(nameArr[1]))
								personRow = cell.Start.Row;
						}
					}
					
					var personRange = pack.Workbook.Worksheets[1].Cells[monthRangesArr[11].ToString().Replace("1", request.PersonRow.ToString())];
					for(int c = personRange.Start.Column;c <= personRange.End.Column;c++) {
						if(list.Count > 0 && pack.Workbook.Worksheets[1].Cells[3, c].Text == "1")
							continue;
						var cell = pack.Workbook.Worksheets[1].Cells[personRow, c];
						if(cell.Value == null)
							list.Add(string.Empty);
						else
							list.Add(cell.Text);
					}
				}
			}
			return list;
		}
		
		public static List<string> RequestNextMonth(ShiftsSwapRequest.ShiftsSwapRequestData request) {
			List<string> list = new List<string>();
			if(request.StartDate.Month < 12)
				list = GetAllShiftsInMonth(request.PersonRow, request.StartDate.Month + 1).ToList();
			else {
				var foundFiles = Settings.ApplicationSettings.ShiftsDefaultLocation.GetFiles("*shift*" + (request.StartDate.Year + 1) + "*.xlsx", SearchOption.TopDirectoryOnly);
				
				FileInfo foundFile = null;
				if(foundFiles.Length > 1) {
					foundFile = foundFiles.Aggregate((f1, f2) => f1.Length > f2.Length ? f1 : f2);
//					var biggestFile = foundFiles.Max(f => f.Length);
//					foundFile = foundFiles.First(f => f.Length == biggestFile);
				}
				else {
					if(foundFiles.Length == 1)
						foundFile = foundFiles[0];
					else
						return list;
				}
				
				ExcelPackage pack = null;
				try {
					pack = new ExcelPackage(foundFile);
				}
				catch {
					if(existingFile != null) {
						FileInfo tempShiftsFile = existingFile.CopyTo(Environment.SpecialFolder.ApplicationData + "\\" + existingFile.Name, true);
						pack = new ExcelPackage(tempShiftsFile);
					}
					
				}
				
				if (pack.File != null) {
					var allMergedCells = pack.Workbook.Worksheets[1].MergedCells;
					ArrayList monthRangesArr = new ArrayList();
					foreach(string address in allMergedCells.List) {
						string[] temp = address.Split(':');
						if(temp[0].RemoveLetters() != "1")
							continue;
						if(temp[1].RemoveLetters() != "1")
							continue;
						monthRangesArr.Add(address);
					}
					
					SortAlphabetLength alphaLen = new SortAlphabetLength();
					monthRanges.Sort(alphaLen);
					
					int personRow = 0;
					foreach(var cell in pack.Workbook.Worksheets[1].Cells["c:c"]) {
						if(cell.Value != null) {
							string[] nameArr = request.Name.ToUpper().RemoveDiacritics().Split(' ');
							if(cell.Text.ToUpper().RemoveDiacritics().Contains(nameArr[0]) &&
							   cell.Text.ToUpper().RemoveDiacritics().Contains(nameArr[1]))
								personRow = cell.Start.Row;
						}
					}
					
					var personRange = pack.Workbook.Worksheets[1].Cells[monthRangesArr[0].ToString().Replace("1", request.PersonRow.ToString())];
					for(int c = personRange.Start.Column;c <= personRange.End.Column;c++) {
						if(list.Count > 0 && pack.Workbook.Worksheets[1].Cells[3, c].Text == "1")
							continue;
						var cell = pack.Workbook.Worksheets[1].Cells[personRow, c];
						if(cell.Value == null)
							list.Add(string.Empty);
						else
							list.Add(cell.Text);
					}
				}
			}
			return list;
		}
		
		public static void ResolveShiftsSwapRequestAdresses(ref ShiftsSwapRequest req) {
			FindRequestPeopleRows(ref req);
			ResolveTraineeStatus(ref req);
			FindRequestDatesColumns(ref req);
			
			ResolveShiftsToSwap(ref req);
		}
		
		public static void ResolveShiftsToSwap(ref ShiftsSwapRequest request) {
			var shiftsRange = package.Workbook.Worksheets[1].Cells[request.Requester.StartDateColumn + request.Requester.PersonRow + ":" +
			                                                       request.Requester.EndDateColumn + request.Requester.PersonRow];
			
			List<string> shifts = new List<string>();
			
			foreach(var cell in shiftsRange) {
				if(cell.Value != null)
					shifts.Add(cell.Text);
			}
			
			request.Requester.ShiftsToSwap = shifts.ToArray();
			request.SwapWith.SwappedShifts = shifts.ToArray();
			
			shiftsRange = package.Workbook.Worksheets[1].Cells[request.SwapWith.StartDateColumn + request.SwapWith.PersonRow + ":" +
			                                                   request.SwapWith.EndDateColumn + request.SwapWith.PersonRow];
			
			shifts = new List<string>();
			
			foreach(var cell in shiftsRange) {
				if(cell.Value != null)
					shifts.Add(cell.Text);
			}
			
			request.SwapWith.ShiftsToSwap = shifts.ToArray();
			request.Requester.SwappedShifts = shifts.ToArray();
		}
		
		static void FindRequestPeopleRows(ref ShiftsSwapRequest req) {
			foreach(var cell in package.Workbook.Worksheets[1].Cells["c:c"]) {
				if(cell.Value != null) {
					if(req.Requester.PersonRow == 0) {
						if(cell.Value.ToString() == req.Requester.Name)
							req.Requester.PersonRow = cell.Start.Row;
					}
					if(req.SwapWith.PersonRow == 0) {
						if(cell.Value.ToString() == req.SwapWith.Name)
							req.SwapWith.PersonRow = cell.Start.Row;
					}
					if(req.Requester.PersonRow > 0 && req.SwapWith.PersonRow > 0)
						break;
				}
			}
		}
		
		static void FindRequestDatesColumns(ref ShiftsSwapRequest req) {
			foreach(var cell in package.Workbook.Worksheets[1].Cells[monthRanges[req.Requester.StartDate.Month - 1].ToString().Replace('1','3')]) {
				if(string.IsNullOrEmpty(req.Requester.StartDateColumn)) {
					if(cell.Value.ToString() == req.Requester.StartDate.Day.ToString())
						req.Requester.StartDateColumn = cell.Address.RemoveDigits();
				}
				if(string.IsNullOrEmpty(req.Requester.EndDateColumn)) {
					if(cell.Value.ToString() == req.Requester.EndDate.Day.ToString())
						req.Requester.EndDateColumn = cell.Address.RemoveDigits();
				}
				if(string.IsNullOrEmpty(req.SwapWith.StartDateColumn)) {
					if(cell.Value.ToString() == req.SwapWith.StartDate.Day.ToString())
						req.SwapWith.StartDateColumn = cell.Address.RemoveDigits();
				}
				if(string.IsNullOrEmpty(req.SwapWith.EndDateColumn)) {
					if(cell.Value.ToString() == req.SwapWith.EndDate.Day.ToString())
						req.SwapWith.EndDateColumn = cell.Address.RemoveDigits();
				}
				if(!string.IsNullOrEmpty(req.Requester.StartDateColumn) &&
				   !string.IsNullOrEmpty(req.Requester.EndDateColumn) &&
				   !string.IsNullOrEmpty(req.SwapWith.StartDateColumn) &&
				   !string.IsNullOrEmpty(req.SwapWith.EndDateColumn))
					break;
			}
		}
		
		public static Roles GetRole(string name) {
			if(ShiftLeaders.FindIndex(s => s.ToUpper() == name.ToUpper()) > -1)
				return Roles.ShiftLeader;
			
			if(TEF.FindIndex(s => s.ToUpper() == name.ToUpper()) > -1)
				return Roles.TEF;
			
			if(External.FindIndex(s => s.ToUpper() == name.ToUpper()) > -1)
				return Roles.External;
			
			if(RAN.FindIndex(s => s.ToUpper() == name.ToUpper()) > -1)
				return Roles.RAN;
			
			return Roles.None;
		}
		
		public static void ResolveTraineeStatus(ref ShiftsSwapRequest req) {
			req.Requester.IsTrainee = package.Workbook.Worksheets[1].Cells[req.Requester.PersonRow, 3].Comment != null;
			req.SwapWith.IsTrainee = package.Workbook.Worksheets[1].Cells[req.SwapWith.PersonRow, 3].Comment != null;
		}
		
//		static string FindDayColumn(DateTime date) {
//			foreach(var cell in package.Workbook.Worksheets[1].Cells[monthRanges[date.Month - 1].ToString().Replace("1","3")]) {
//				if(cell.Value != null) {
//					if(cell.Text == date.Day.ToString()) {
//						return cell.Address.RemoveDigits();
//					}
//				}
//			}
//			return string.Empty;
//		}
	}
}

public enum Months : byte {
	January,
	February,
	March,
	April,
	May,
	June,
	July,
	August,
	September,
	October,
	November,
	December
};
