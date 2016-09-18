namespace Nager.AmazonEc2.Model
{
    public class ElasticsearchClusterConfig
    {
        public string ClusterName { get; set; }
        public string Prefix { get; set; }
        public string KeyName { get; set; }
        public AmazonAccessKey DiscoveryAccessKey { get; set; }
        public AmazonInstance MasterNodeInstance { get; set; }
        public int MasterNodeCount { get; set; }
        public AmazonInstance DataNodeInstance { get; set; }
        public int DataNodeCount { get; set; }
        public AmazonInstance ClientNodeInstance { get; set; }
        public int ClientNodeCount { get; set; }
    }
}