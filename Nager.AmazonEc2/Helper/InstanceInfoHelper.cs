using Nager.AmazonEc2.Model;

namespace Nager.AmazonEc2.Helper
{
    public static class InstanceInfoHelper
    {
        public static AmazonInstanceInfo GetInstanceInfo(AmazonInstance amazonInstance)
        {
            switch (amazonInstance)
            {
                case AmazonInstance.t2_Small:
                    return new AmazonInstanceInfo { InstanceType = "t2.small", CpuCount = 1, Memory = 2 };
                case AmazonInstance.t2_Medium:
                    return new AmazonInstanceInfo { InstanceType = "t2.medium", CpuCount = 2, Memory = 4 };
                case AmazonInstance.t2_Large:
                    return new AmazonInstanceInfo { InstanceType = "t2.large", CpuCount = 2, Memory = 8 };
                case AmazonInstance.r3_Large:
                    return new AmazonInstanceInfo { InstanceType = "r3.large", CpuCount = 2, Memory = 15.25 };
                case AmazonInstance.c4_xLarge:
                    return new AmazonInstanceInfo { InstanceType = "c4.xlarge", CpuCount = 4, Memory = 7.5 };
                case AmazonInstance.c4_2xLarge:
                    return new AmazonInstanceInfo { InstanceType = "c4.2xlarge", CpuCount = 8, Memory = 15 };
                case AmazonInstance.m4_xLarge:
                    return new AmazonInstanceInfo { InstanceType = "m4.xlarge", CpuCount = 4, Memory = 16 };
                case AmazonInstance.m4_2xLarge:
                    return new AmazonInstanceInfo { InstanceType = "m4.2xlarge", CpuCount = 8, Memory = 32 };
                case AmazonInstance.m4_4xLarge:
                    return new AmazonInstanceInfo { InstanceType = "m4.4xlarge", CpuCount = 16, Memory = 64 };
            }

            return null;
        }
    }
}