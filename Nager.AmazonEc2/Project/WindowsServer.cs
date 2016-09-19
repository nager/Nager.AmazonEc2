using Amazon.EC2;
using Amazon.EC2.Model;
using log4net;
using Nager.AmazonEc2.Helper;
using Nager.AmazonEc2.InstallScript;
using Nager.AmazonEc2.Model;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Nager.AmazonEc2.Project
{
    public class WindowsServer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WindowsServer));
        private AmazonEC2Client _client;

        public WindowsServer(AmazonAccessKey accessKey)
        {
            this._client = new AmazonEC2Client(accessKey.AccessKeyId, accessKey.SecretKey, Amazon.RegionEndpoint.EUWest1);
        }

        public InstallResult Install(AmazonInstance amazonInstance, string name, string securityGroupId, string keyName, IInstallScript installScript)
        {
            var instanceInfo = InstanceInfoHelper.GetInstanceInfo(amazonInstance);

            var instanceRequest = new RunInstancesRequest();
            instanceRequest.ImageId = "ami-a8592cdb"; //windows server 2012 r2 base
            instanceRequest.InstanceType = instanceInfo.InstanceType;
            instanceRequest.MinCount = 1;
            instanceRequest.MaxCount = 1;
            instanceRequest.KeyName = keyName;
            instanceRequest.SecurityGroupIds = new List<string>() { securityGroupId };

            if (!instanceInfo.LocalStorage)
            {
                var blockDeviceMappingSystem = new BlockDeviceMapping
                {
                    DeviceName = "/dev/sda1",
                    Ebs = new EbsBlockDevice
                    {
                        DeleteOnTermination = true,
                        VolumeType = VolumeType.Gp2,
                        VolumeSize = 30
                    }
                };

                instanceRequest.BlockDeviceMappings.Add(blockDeviceMappingSystem);
            }

            //Install Process can check in this log file
            //<C:\Program Files\Amazon\Ec2ConfigService\Logs\Ec2ConfigLog.txt>
            instanceRequest.UserData = installScript.Create();

            var response = this._client.RunInstances(instanceRequest);
            var instance = response.Reservation.Instances.First();

            var installResult = new InstallResult();
            installResult.Name = name;
            installResult.InstanceId = instance.InstanceId;
            installResult.PrivateIpAddress = instance.PrivateIpAddress;

            var tags = new List<Tag> { new Tag("Name", name) };
            this._client.CreateTags(new CreateTagsRequest(new List<string>() { instance.InstanceId }, tags));

            if (response.HttpStatusCode == HttpStatusCode.OK)
            {
                installResult.Successful = true;
            }

            return installResult;
        }
    }
}