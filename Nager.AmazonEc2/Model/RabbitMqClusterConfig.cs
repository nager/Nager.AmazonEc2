namespace Nager.AmazonEc2.Model
{
    public class RabbitMqClusterConfig
    {
        public string ClusterName { get; set; }
        public string Prefix { get; set; }
        public string KeyName { get; set; }
        public AmazonInstance NodeInstance { get; set; }
        public int NodeCount { get; set; }
        public string AdminUsername { get; set; }
        public string AdminPassword { get; set; }
    }
}