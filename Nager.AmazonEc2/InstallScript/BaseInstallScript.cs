using System;
using System.Collections.Generic;

namespace Nager.AmazonEc2.InstallScript
{
    public abstract class BaseInstallScript : IInstallScript
    {
        public List<string> Commands = new List<string>();

        public bool Add(string command)
        {
            if (string.IsNullOrEmpty(command))
            {
                return false;
            }

            this.Commands.Add(command);
            return true;
        }

        public abstract string Create();
    }
}