using Nager.AmazonEc2.Model;
using Nager.AmazonEc2.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nager.AmazonEc2.TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var accesskey = new AmazonAccessKey("accessKeyId", "secretKey");

            var couchbase = new Couchbase(accesskey);

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
