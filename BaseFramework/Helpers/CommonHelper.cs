using RW.Common.Helpers;
using System.Diagnostics;
using System.Windows;

namespace BaseFramework.Helpers;

public static class CommonHelper {
    public static string NumberToK(this int number) {
        if (number > 1000) {
            int a = number / 1000;
            int length = $"{number}".Length;
            int pow = (int)Math.Pow(10, length - 1);
            int head = int.Parse($"{number}".First().ToString());
            int b = (number - (pow * head)) / (pow / 10);
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
        return value is byte or sbyte or
            short or ushort or
            int or uint or
            long or ulong or
            float or double or
            decimal;
    }

    //public static double Abs(object value) {
    //    if (IsNumber(value)) {
    //        return Math.Abs(Convert.ToDouble(value));
    //    } else {
    //        throw new ArgumentException("Value must be a number");
    //    }
    //}

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

}
