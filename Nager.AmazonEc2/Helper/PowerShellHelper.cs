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
    }
}
