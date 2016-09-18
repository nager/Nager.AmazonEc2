namespace Nager.AmazonEc2.Model
{
    public class AmazonInstanceInfo
    {
        public string InstanceType { get; set; }
        public int CpuCount { get; set; }
        public double Memory { get; set; }
        public bool LocalStorage { get; set; }
    }
}