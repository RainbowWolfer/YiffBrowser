﻿using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace BaseFramework.Helpers {
	public static class CommonHelper {
		public static bool SearchFor(this string source, string target) {
			return source.ToLower().Trim().Contains(target.ToLower().Trim());
		}

		public static void RemoveAll(this IList list) {
			while (list.Count > 0) {
				list.RemoveAt(list.Count - 1);
			}
		}

		public static int Count(this IEnumerable ie) {
			if (ie == null) {
				return 0;
			}

			if (ie is ICollection collection) {
				return collection.Count;
			}

			if (ie is IList list) {
				return list.Count;
			}

			int count = 0;
			IEnumerator enumerator = ie.GetEnumerator();
			while (enumerator.MoveNext()) {
				count = checked(count + 1);
			}

			return count;
		}

		public static bool IsEmpty(this IEnumerable? ie) {
			if (ie == null) {
				return true;
			}

			if (ie is ICollection collection) {
				return collection.Count == 0;
			}

			if (ie is IList list) {
				return list.Count == 0;
			}

			IEnumerator enumerator = ie.GetEnumerator();
			return !enumerator.MoveNext();
		}

		public static bool IsNotEmpty(this IEnumerable? ie) => !ie.IsEmpty();


		public static bool IsEmpty<T>(this IEnumerable<T>? ie) => ie == null || !ie.Any();
		public static bool IsNotEmpty<T>(this IEnumerable<T>? ie) => !ie.IsEmpty();

		public static bool IsBlank([NotNullWhen(false)] this string? str) => string.IsNullOrWhiteSpace(str);
		public static bool IsNotBlank([NotNullWhen(true)] this string? str) => !str.IsBlank();

		public static string? NotBlankCheck(this string? text) => text.IsBlank() ? null : text;

		public static string NumberToK(this int number) {
			if (number > 1000) {
				int a = number / 1000;
				int length = $"{number}".Length;
				int pow = (int)Math.Pow(10, length - 1);
				int head = int.Parse($"{number}".First().ToString());
				int b = (number - pow * head) / (pow / 10);
				if (b == 0) {
					return $"{a}K";
				} else {
					return $"{a}.{b}K";
				}
			} else {
				return $"{number}";
			}
		}

		public static bool OnlyContainDigits(this string text) {
			foreach (char item in text) {
				if (!char.IsDigit(item)) {
					return false;
				}
			}
			return true;
		}

		public static string ToFullString(this IEnumerable<string> tags) {
			return string.Join(" ", tags);
		}

		public static bool IsNumber(this object value) {
			return value is byte || value is sbyte ||
				value is short || value is ushort ||
				value is int || value is uint ||
				value is long || value is ulong ||
				value is float || value is double ||
				value is decimal;
		}

		public static double Abs(object value) {
			if (IsNumber(value)) {
				return Math.Abs(Convert.ToDouble(value));
			} else {
				throw new ArgumentException("Value must be a number");
			}
		}


		public static string FileSizeToKB(this int size, bool gap = false) {
			return FileSizeToKB((long)size, gap);
		}

		public static string FileSizeToKB(this long size, bool gap = false) {
			string output = (size / 1024.0).ToString("#,##0");
			//string kb = $"{size / 1000}";
			//string output = Regex.Replace(kb, ".{3}(?!.)", ",$&").Trim(',');
			if (gap) {
				return $"{output} KB";
			} else {
				return $"{output}KB";
			}
		}

		public static void CopyToClipboard(this string? text) {
			if (text.IsBlank()) {
				return;
			}
			try {
				Clipboard.SetText(text);
			} catch (Exception e1) {
				Debug.WriteLine(e1);
				try {
					Clipboard.SetDataObject(text, true);
				} catch (Exception e2) {
					Debug.WriteLine(e2);
				}
			}
		}

		public static void OpenInBrowser(this string? link) {
			if (link.IsBlank()) {
				return;
			}
			try {
				//Process.Start(link);
				ProcessStartInfo processStartInfo = new() {
					FileName = link,
					UseShellExecute = true // This ensures the URL opens in the default browser
				};
				Process.Start(processStartInfo);
			} catch (Exception ex) {
				Debug.WriteLine(ex);
				MessageBox.Show("Unable to open browser", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		public static double Distance(this Point a, Point b) {
			double x = Math.Pow(a.X - b.X, 2);
			double y = Math.Pow(a.Y - b.Y, 2);
			return Math.Sqrt(x + y);
		}

		//public static async Task ComposeEmail(string email, string name, string subject, string messageBody) {
		//	//EmailMessage emailMessage = new() {
		//	//	Subject = subject,
		//	//	Body = messageBody,
		//	//};
		//	//emailMessage.To.Add(new EmailRecipient(email, name));
		//	//await EmailManager.ShowComposeNewEmailAsync(emailMessage);
		//}

		public static double Remap(this double value, double fromMin, double fromMax, double toMin, double toMax) {
			return (value - fromMin) / (fromMax - fromMin) * (toMax - toMin) + toMin;
		}

	}
}
