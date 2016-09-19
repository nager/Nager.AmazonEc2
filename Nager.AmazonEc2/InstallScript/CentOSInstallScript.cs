using System;
using System.Text;

namespace Nager.AmazonEc2.InstallScript
{
    public class CentOSInstallScript : BaseInstallScript
    {
        public override string Create()
        {
            var sb = new StringBuilder();
            //Shebang
            sb.Append("#!/bin/bash\n");
            foreach (var command in base.Commands)
            {
                sb.AppendFormat("{0}\n", command);
            }
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(sb.ToString()));
        }

        public bool PrepareDataDisk()
        {
            //Create Mount Folder
            base.Add("mkdir /data");
            //Create ext4 File System
            base.Add("mkfs.ext4 /dev/xvdb");
            //Mount Data Disk
            base.Add("mount /dev/xvdb /data");
            //Automatic mount on boot
            base.Add("echo \"/dev/xvdb    /data    ext4    defaults,nofail    0    2\" >> /etc/fstab");

            return true;
        }
    }
}