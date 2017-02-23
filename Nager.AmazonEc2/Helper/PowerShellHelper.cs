using Nager.AmazonEc2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nager.AmazonEc2.Helper
{
    public static class PowerShellHelper
    {
        /// <summary>
        /// Create a PowerShell script
        /// </summary>
        /// <param name="filePath">path and directory</param>
        /// <param name="items">powershell commands</param>
        /// <returns></returns>
        public static string CreateScript(string filePath, List<string> items)
        {
            var sb = new StringBuilder();

            sb.Append($"New-Item {filePath} -type file -force -value \"");

            foreach (var item in items)
            {
                sb.Append(item.Replace("$", "`$").Replace("\"", "`\""));
                sb.Append("`r`n");
            }

            sb = sb.Remove(sb.Length - 4, 4);

            sb.Append("\"");

            return sb.ToString();
        }

        public static string SendSlackMessage(string webhookUrl, SlackMessage message)
        {
            if (message == null)
            {
                return null;
            }

            var sb = new StringBuilder();

            sb.AppendLine("Set-StrictMode -Version Latest");
            sb.AppendLine("$payload = @{");
            sb.AppendLine($"\"channel\" = \"{message.Channel}\";");
            sb.AppendLine($"\"icon_emoji\" = \"{message.IconEmoji}\";");
            sb.AppendLine($"\"username\" = \"{message.Username}\";");
            sb.AppendLine($"\"text\" = \"{message.Text}\";");
            sb.AppendLine("}");

            sb.AppendLine($"Invoke-WebRequest -Uri \"{webhookUrl}\" -Method \"POST\" -Body (ConvertTo-Json -Compress -InputObject $payload)");

            return sb.ToString();
        }

        public static string GetPublicAmazonIpAddress(string variableName = "publicip", bool fallback = false)
        {
            var sb = new StringBuilder();

            if (fallback)
            {
                sb.AppendLine("try");
                sb.AppendLine("{");
                sb.AppendLine("$request = [System.Net.HttpWebRequest]::CreateHttp(\"http://169.254.169.254/latest/meta-data/public-ipv4\")");
                sb.AppendLine("$response = $request.GetResponseAsync().Result");
                sb.AppendLine("$stream = $response.GetResponseStream()");
                sb.AppendLine("$streamReader = New-Object System.IO.StreamReader($stream)");
                sb.AppendLine($"${variableName} = $streamReader.ReadToEnd()");
                sb.AppendLine("}");
                sb.AppendLine("finally");
                sb.AppendLine("{");
                sb.AppendLine("if ($stream)");
                sb.AppendLine("{");
                sb.AppendLine("$stream.Dispose()");
                sb.AppendLine("}");
                sb.AppendLine("if ($streamReader)");
                sb.AppendLine("{");
                sb.AppendLine("$streamReader.Dispose()");
                sb.AppendLine("}");
                sb.AppendLine("}");
            }

            sb.AppendLine($"${variableName} = Invoke-WebRequest -URI http://169.254.169.254/latest/meta-data/public-ipv4");
            return sb.ToString();
        }
    }
}
