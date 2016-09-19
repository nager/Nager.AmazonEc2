using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Nager.AmazonEc2.InstallScript
{
    public class WindowsInstallScript : BaseInstallScript
    {
        public override string Create()
        {
            var sb = new StringBuilder();
            sb.Append("<powershell>\r\n");
            foreach (var command in base.Commands)
            {
                sb.AppendFormat("{0}\r\n", command);
            }
            sb.Append("</powershell>\r\n");
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(sb.ToString()));
        }

        public bool SetAdministratorPassword(string password)
        {
            if (!this.IsComplexPassword(password))
            {
                return false;
            }

            var passwordEscaped = password.Replace("$", "`$");
            base.Add("$user = [ADSI]\"WinNT://./Administrator\";");
            base.Add($"$user.SetPassword(\"{passwordEscaped}\");");

            return true;
        }

        public bool IsComplexPassword(string password)
        {
            if (password.Length < 7)
            {
                return false;
            }

            int score = 1;

            if (Regex.IsMatch(password, @"\d+"))
            {
                score++;
            }

            if (Regex.IsMatch(password, "[a-z]"))
            {
                score++;
            }

            if (Regex.IsMatch(password, "[A-Z]"))
            {
                score++;
            }

            if (Regex.IsMatch(password, @"\W"))
            {
                score++;
            }

            if (score >= 4)
            {
                return true;
            }

            return false;
        }

        public bool InstallMsi(string url)
        {
            if (String.IsNullOrEmpty(url))
            {
                return false;
            }

            var uri = new Uri(url);
            var filename = Path.GetFileName(uri.LocalPath);

            base.Add($"$url = \"{url}\"");
            base.Add($"$output = \"$PSScriptRoot\\{filename}\"");
            base.Add("(New-Object System.Net.WebClient).DownloadFile($url, $output)");
            base.Add($"Start-Process \"$PSScriptRoot\\{filename}\" /qn -Wait");

            return true;
        }

        public bool DisableFirewall()
        {
            base.Add("Set-NetFirewallProfile -Profile Domain,Public,Private -Enabled False");

            return true;
        }

        public bool ReplaceContentPart(string filePath, string search, string replace)
        {
            if (String.IsNullOrEmpty(filePath))
            {
                return false;
            }

            if (String.IsNullOrEmpty(search))
            {
                return false;
            }

            base.Add($"(Get-Content '{filePath}').replace('{search}', '{replace}') | Set-Content '{filePath}'");

            return true;
        }
    }
}