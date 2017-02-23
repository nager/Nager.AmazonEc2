using Amazon;
using Nager.AmazonEc2.InstallScript;
using Nager.AmazonEc2.Model;
using Nager.AmazonEc2.Project;
using System.Collections.Generic;

namespace Nager.AmazonEc2.TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var accesskey = new AmazonAccessKey("accessKeyId", "secretKey");

            InstallWindowsWebserver(accesskey);
        }

        public static void InstallWindowsWebserver(AmazonAccessKey accesskey)
        {
            var windowsServer = new WindowsServer(accesskey, RegionEndpoint.EUCentral1);

            var installScript = new WindowsInstallScript();
            installScript.SetAdministratorPassword("Super$ecurePassword");
            installScript.DisableFirewall();
            installScript.AddWindowsFeature("Web-Server", "Web-WebServer", "Web-Security", "Web-Filtering", "Web-Dyn-Compression", "Web-Asp-Net45", "Web-Mgmt-Tools", "Web-Mgmt-Service", "NET-HTTP-Activation");

            var securityGroupId = windowsServer.CreateSecurityGroup("nager.Webserver", new List<int> { 80, 443 });
            windowsServer.Install(AmazonInstance.t2_Small, $"Webserver", securityGroupId, "adrom.webserver", installScript, WindowsVersion.V2016);
        }

        public static void InstallCouchbase(AmazonAccessKey accesskey)
        {
            var couchbase = new Couchbase(accesskey, RegionEndpoint.EUCentral1);

            var clusterConfig = new CouchbaseClusterConfig();
            clusterConfig.Prefix = "nager";
            clusterConfig.NodeInstance = AmazonInstance.t2_Small;
            clusterConfig.NodeCount = 2;
            clusterConfig.KeyName = "my-secret-key";
            clusterConfig.ClusterName = "Cluster1";
            clusterConfig.AdminUsername = "Administrator";
            clusterConfig.AdminUsername = "$ecurePassword";

            var installResults = couchbase.InstallCluster(clusterConfig);

            var managementUrl = couchbase.GetManagementUrl(installResults);
        }
    }
}