namespace Nager.AmazonEc2.Model
{
    public class AmazonAccessKey
    {
        public string AccessKeyId { get; set; }
        public string SecretKey { get; set; }

        public AmazonAccessKey()
        {
        }

        public AmazonAccessKey(string accessKeyId, string secretKey)
        {
            this.AccessKeyId = accessKeyId;
            this.SecretKey = secretKey;

        }
    }
}