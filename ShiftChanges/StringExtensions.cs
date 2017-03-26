/*
 * Created by SharpDevelop.
 * User: GONCARJ3
 * Date: 06/01/2017
 * Time: 20:43
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

public static class StringExtensions
{
	public static String RemoveDigits(this string str) {
		return Regex.Replace(str, @"\d", "");
	}
	
	public static String RemoveLetters(this string str) {
		return Regex.Replace(str, "[^0-9.]", "");
	}

	public static int CountStringOccurrences(this string str, string pattern) {
		int num = 0;
		int startIndex = 0;
		while ((startIndex = str.IndexOf(pattern, startIndex, StringComparison.Ordinal)) != -1) {
			startIndex += pattern.Length;
			num++;
		}
		return num;
	}
	
	public static bool IsAllDigits(this string str) {
		foreach (char ch in str)
			if (!char.IsDigit(ch))
				return false;
		
		return true;
	}
	
	public static String RemoveDiacritics(this string str) {
		var normalizedString = str.Normalize(NormalizationForm.FormD);
		var stringBuilder = new StringBuilder();

		foreach (var c in normalizedString)
		{
			var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
			if(unicodeCategory != UnicodeCategory.NonSpacingMark) {
				stringBuilder.Append(c);
			}
		}

		return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
	}
}