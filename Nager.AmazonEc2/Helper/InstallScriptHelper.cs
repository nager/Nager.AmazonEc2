using log4net;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nager.AmazonEc2.Helper
{
    public static class InstallScriptHelper
    {
        public static string CreateLinuxScript(List<string> items)
        {
            var sb = new StringBuilder();
            sb.Append("#! /bin/bash\n");

            foreach (var item in items)
            {
                sb.AppendFormat("{0}\n", item);
            }

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(sb.ToString()));
        }

        public static string CreateWindowsScript(List<string> items)
        {
            var sb = new StringBuilder();

            foreach (var item in items)
            {
                sb.AppendFormat("{0}\r\n", item);
            }

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(sb.ToString()));
        }

        public static List<string> PrepareDataDisk()
        {
            var items = new List<string>();

            //Create Mount Folder
            items.Add("mkdir /data");
            //Create ext4 File System
            items.Add("mkfs.ext4 /dev/xvdb");
            //Mount Data Disk
            items.Add("mount /dev/xvdb /data");
            //Automatic mount on boot
            items.Add("echo \"/dev/xvdb    /data    ext4    defaults,nofail    0    2\" >> /etc/fstab");

            return items;
        }
    }
}