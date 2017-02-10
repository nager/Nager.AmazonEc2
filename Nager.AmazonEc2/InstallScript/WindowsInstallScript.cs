using Nager.AmazonEc2.Model;
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

        public bool SetHostname(string hostname, bool restart = true)
        {
            if (restart)
            {
                base.Add($"Rename-Computer -NewName \"{hostname}\" -Restart");
            }
            else
            {
                base.Add($"Rename-Computer -NewName \"{hostname}\"");
            }

            return true;
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

        public bool ConfigureWindowsUpdate(WindowsUpdateNotificationLevel notificationLevel)
        {
            base.Add("$Updates = (New-Object -ComObject \"Microsoft.Update.AutoUpdate\").Settings");
            base.Add($"$Updates.NotificationLevel = {(int)notificationLevel}");
            base.Add("$Updates.Save()");
            base.Add("$Updates.Refresh()");

            return true;
        }

        public bool InstallMsi(string url, string parameter = "/qn")
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }

            var uri = new Uri(url);
            var filename = Path.GetFileName(uri.LocalPath);

            base.Add($"$url = \"{url}\"");
            base.Add($"$output = \"$PSScriptRoot\\{filename}\"");
            base.Add("(New-Object System.Net.WebClient).DownloadFile($url, $output)");
            base.Add($"Start-Process -Wait -FilePath msiexec -ArgumentList \"/i $PSScriptRoot\\{filename} {parameter}\"");

            return true;
        }

        public bool RunExecutable(string url, string parameter)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }

            var uri = new Uri(url);
            var filename = Path.GetFileName(uri.LocalPath);

            base.Add($"$url = \"{url}\"");
            base.Add($"$output = \"$PSScriptRoot\\{filename}\"");
            base.Add("(New-Object System.Net.WebClient).DownloadFile($url, $output)");
            base.Add($"Start-Process \"$PSScriptRoot\\{filename}\" {parameter} -Wait");

            return true;
        }

        public bool SendSlackMessage(string webhookUrl, SlackMessage message)
        {
            if (message == null)
            {
                return false;
            }

            base.Add("Set-StrictMode -Version Latest");
            base.Add("$payload = @{");
            base.Add($"\"channel\" = \"{message.Channel}\";");
            base.Add($"\"icon_emoji\" = \"{message.IconEmoji}\";");
            base.Add($"\"username\" = \"{message.Username}\";");
            base.Add($"\"text\" = \"{message.Text}\";");
            base.Add("}");

            base.Add($"Invoke-WebRequest -Uri \"{webhookUrl}\" -Method \"POST\" -Body (ConvertTo-Json -Compress -InputObject $payload)");

            return true;
        }

        public bool GetPublicIpAddress(string variableName = "publicip")
        {
            base.Add($"${variableName} = Invoke-WebRequest -URI http://169.254.169.254/latest/meta-data/public-ipv4");
            return true;
        }

        public bool CreateScheduledTask(string taskName, string filePath)
        {
            base.Add($"Unregister-ScheduledTask {taskName} -Confirm:$false");
            base.Add($"$actionscript = \"-ExecutionPolicy Bypass -File {filePath}\"");
            base.Add("$pstart = \"C:\\Windows\\System32\\WindowsPowerShell\\v1.0\\powershell.exe\"");
            base.Add("$action = New-ScheduledTaskAction -Execute $pstart -Argument $actionscript");
            base.Add("$trigger = New-ScheduledTaskTrigger -AtStartup");
            base.Add("$taskUserName = New-ScheduledTaskPrincipal -UserID \"NT AUTHORITY\\SYSTEM\" -LogonType ServiceAccount -RunLevel Highest");
            base.Add($"Register-ScheduledTask -TaskName {taskName} -Principal $taskUserName -Action $action -Trigger $trigger");

            return true;
        }

        public bool DisableFirewall()
        {
            base.Add("Set-NetFirewallProfile -Profile Domain,Public,Private -Enabled False");

            return true;
        }

        public bool ReplaceContentPart(string filePath, string search, string replace)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            if (string.IsNullOrEmpty(search))
            {
                return false;
            }

            base.Add($"(Get-Content '{filePath}').replace('{search}', '{replace}') | Set-Content '{filePath}'");

            return true;
        }
    }
}