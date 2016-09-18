﻿using Amazon.EC2;
using Amazon.EC2.Model;
using log4net;
using Nager.AmazonEc2.Helper;
using Nager.AmazonEc2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Nager.AmazonEc2.Project
{
    public class Elasticsearch
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Elasticsearch));
        private AmazonEC2Client _client;

        public Elasticsearch(AmazonAccessKey accessKey)
        {
            this._client = new AmazonEC2Client(accessKey.AccessKeyId, accessKey.SecretKey, Amazon.RegionEndpoint.EUWest1);
        }

        private string CreateSecurityGroup(string prefix)
        {
            var name = $"{prefix}.Elasticsearch";
            var description = "This security group was generated by Nager.AmazonEc2 System";

            try
            {
                var result = this._client.DescribeSecurityGroups(new DescribeSecurityGroupsRequest() { GroupNames = new List<string> { name } });
                if (result.HttpStatusCode == HttpStatusCode.OK)
                {
                    return result.SecurityGroups.Select(o => o.GroupId).FirstOrDefault();
                }
            }
            catch (AmazonEC2Exception exception)
            {
                if (exception.ErrorCode != "InvalidGroup.NotFound")
                {
                    return null;
                }
            }

            var createSecurityGroupRequest = new CreateSecurityGroupRequest(name, description);
            var createSecurityGroupResponse = this._client.CreateSecurityGroup(createSecurityGroupRequest);

            if (createSecurityGroupResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var ipPermissionWebinterface = new IpPermission()
            {
                IpProtocol = "tcp",
                FromPort = 9200,
                ToPort = 9200,
                IpRanges = new List<string>() { "0.0.0.0/0" }
            };

            var ipPermissionTransport = new IpPermission()
            {
                IpProtocol = "tcp",
                FromPort = 9300,
                ToPort = 9300,
                UserIdGroupPairs = new List<UserIdGroupPair>() { new UserIdGroupPair() { GroupId = createSecurityGroupResponse.GroupId } }
            };

            var ipPermissionSsh = new IpPermission()
            {
                IpProtocol = "tcp",
                FromPort = 22,
                ToPort = 22,
                IpRanges = new List<string>() { "0.0.0.0/0" },
            };

            var ingressRequest = new AuthorizeSecurityGroupIngressRequest();
            ingressRequest.GroupId = createSecurityGroupResponse.GroupId;
            ingressRequest.IpPermissions = new List<IpPermission>();
            ingressRequest.IpPermissions.Add(ipPermissionWebinterface);
            ingressRequest.IpPermissions.Add(ipPermissionTransport);
            ingressRequest.IpPermissions.Add(ipPermissionSsh);

            var ingressResponse = this._client.AuthorizeSecurityGroupIngress(ingressRequest);
            if (ingressResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            return createSecurityGroupResponse.GroupId;
        }

        public string GetManagementUrl(List<InstallResult> installResults)
        {
            var results = this._client.DescribeInstances(new DescribeInstancesRequest() { InstanceIds = new List<string> { installResults.First().InstanceId } });
            var publicUrl = results.Reservations[0]?.Instances[0]?.PublicDnsName;

            return $"http://{publicUrl}:9200/_plugin/hq/";
        }

        public List<InstallResult> InstallCluster(ElasticsearchClusterConfig clusterConfig)
        {
            Log.Debug("InstallCluster");

            if (String.IsNullOrEmpty(clusterConfig.DiscoveryAccessKey.AccessKeyId))
            {
                return null;
            }

            if (String.IsNullOrEmpty(clusterConfig.DiscoveryAccessKey.SecretKey))
            {
                return null;
            }

            var securityGroupId = this.CreateSecurityGroup(clusterConfig.Prefix);
            var installResults = new List<InstallResult>();

            InstallResult installResult;

            for (var i = 0; i < clusterConfig.MasterNodeCount; i++)
            {
                var nodeName = $"{clusterConfig.ClusterName}.master{i}";

                installResult = InstallNode(clusterConfig.MasterNodeInstance, clusterConfig.ClusterName, nodeName, true, false, securityGroupId, clusterConfig.DiscoveryAccessKey, clusterConfig.MasterNodeCount, clusterConfig.KeyName);
                installResults.Add(installResult);
            }

            for (var i = 0; i < clusterConfig.DataNodeCount; i++)
            {
                var nodeName = $"{clusterConfig.ClusterName}.data{i}";

                installResult = InstallNode(clusterConfig.DataNodeInstance, clusterConfig.ClusterName, nodeName, false, true, securityGroupId, clusterConfig.DiscoveryAccessKey, clusterConfig.MasterNodeCount, clusterConfig.KeyName);
                installResults.Add(installResult);
            }

            for (var i = 0; i < clusterConfig.ClientNodeCount; i++)
            {
                var nodeName = $"{clusterConfig.ClusterName}.client{i}";

                installResult = InstallNode(clusterConfig.ClientNodeInstance, clusterConfig.ClusterName, nodeName, false, false, securityGroupId, clusterConfig.DiscoveryAccessKey, clusterConfig.MasterNodeCount, clusterConfig.KeyName);
                installResults.Add(installResult);
            }

            return installResults;
        }

        public InstallResult InstallNode(AmazonInstance amazonInstance, string clusterName, string name, bool masterNode, bool dataNode, string securityGroupId, AmazonAccessKey discoveryAccessKey, int minimumMasterNodes, string keyName)
        {
            var instanceInfo = InstanceInfoHelper.GetInstanceInfo(amazonInstance);

            var instanceRequest = new RunInstancesRequest();
            instanceRequest.ImageId = "ami-7abd0209"; //centos
            instanceRequest.InstanceType = instanceInfo.InstanceType;
            instanceRequest.MinCount = 1;
            instanceRequest.MaxCount = 1;
            instanceRequest.KeyName = keyName;
            instanceRequest.SecurityGroupIds = new List<string>() { securityGroupId };

            var blockDeviceMappingSystem = new BlockDeviceMapping
            {
                DeviceName = "/dev/sda1",
                Ebs = new EbsBlockDevice
                {
                    DeleteOnTermination = true,
                    VolumeType = VolumeType.Gp2,
                    VolumeSize = 12
                }
            };

            var blockDeviceMappingData = new BlockDeviceMapping
            {
                DeviceName = "/dev/sda2",
                Ebs = new EbsBlockDevice
                {
                    DeleteOnTermination = true,
                    VolumeType = VolumeType.Io1,
                    Iops = 100,
                    VolumeSize = (int)Math.Ceiling(instanceInfo.Memory * 2)
                }
            };

            instanceRequest.BlockDeviceMappings.Add(blockDeviceMappingSystem);
            instanceRequest.BlockDeviceMappings.Add(blockDeviceMappingData);

            //Install Process can check in this log file
            //</var/log/cloud-init-output.log>
            instanceRequest.UserData = InstallScriptHelper.CreateLinuxScript(GetInstallScript(clusterName, name, masterNode, dataNode, instanceInfo, discoveryAccessKey, minimumMasterNodes));

            var response = this._client.RunInstances(instanceRequest);
            var instance = response.Reservation.Instances.First();

            var installResult = new InstallResult();
            installResult.Name = name;
            installResult.InstanceId = instance.InstanceId;
            installResult.PrivateIpAddress = instance.PrivateIpAddress;

            var tags = new List<Tag> { new Tag("Name", name), new Tag("cluster", clusterName) };
            this._client.CreateTags(new CreateTagsRequest(new List<string>() { instance.InstanceId }, tags));

            if (response.HttpStatusCode == HttpStatusCode.OK)
            {
                installResult.Successful = true;
            }

            return installResult;
        }

        private static List<string> GetInstallScript(string clusterName, string nodeName, bool masterNode, bool dataNode, AmazonInstanceInfo instanceInfo, AmazonAccessKey accessKey, int minimumMasterNodes)
        {
            var items = new List<string>();

            //Prepare Data Disk
            items.AddRange(InstallScriptHelper.PrepareDataDisk());

            //Disable Swap
            //(A swappiness of 1 is better than 0, since on some kernel versions a swappiness of 0 can invoke the OOM-killer)
            items.Add("sysctl vm.swappiness=1");
            items.Add("echo \"vm.swappiness = 1\" >> /etc/sysctl.conf");

            //Import Public Key
            items.Add("rpm --import http://packages.elasticsearch.org/GPG-KEY-elasticsearch");

            #region V 2.X

            items.Add("cat > /etc/yum.repos.d/elasticsearch.repo <<EOL");
            items.Add("[elasticsearch-2.x]");
            items.Add("name=Elasticsearch repository for 2.x packages");
            items.Add("baseurl=http://packages.elastic.co/elasticsearch/2.x/centos");
            items.Add("gpgcheck=1");
            items.Add("gpgkey=http://packages.elasticsearch.org/GPG-KEY-elasticsearch");
            items.Add("enabled=1");
            items.Add("EOL");

            #endregion

            items.Add("yum install java elasticsearch -y");

            //Autostart Service
            items.Add("chkconfig elasticsearch on");


            items.Add($"mkdir /data/{clusterName}");
            items.Add($"chown elasticsearch:elasticsearch /data/{clusterName}");

            #region Config file

            items.Add("cat > /etc/elasticsearch/elasticsearch.yml <<EOL");
            items.Add($"cluster.name: {clusterName}");
            items.Add($"node.name: {nodeName}");

            items.Add($"node.master: {masterNode.ToString().ToLower()}");
            items.Add($"node.data: {dataNode.ToString().ToLower()}");

            items.Add("path.data: /data");

            items.Add("network.host: _ec2_"); //cloud-aws needed _ec2_

            items.Add("http.compression: true");

            items.Add("http.cors.enabled: true");
            items.Add("http.cors.allow-origin: \"*\"");

            items.Add("index.max_result_window: \"2147483647\"");

            //AWS

            items.Add("plugin.mandatory: cloud-aws");
            items.Add($"cloud.aws.access_key: {accessKey.AccessKeyId}");
            items.Add($"cloud.aws.secret_key: {accessKey.SecretKey}");
            items.Add("cloud.aws.region: eu-west-1");

            items.Add("discovery.type: ec2");
            //items.Add("discovery.ec2.groups: sg-9a0901fd");
            items.Add($"discovery.ec2.tag.cluster: {clusterName}");
            items.Add("discovery.ec2.host_type: private_ip");
            items.Add("discovery.ec2.ping_timeout: 30s");

            //AWS EC2 not support multicast
            items.Add("discovery.zen.ping.multicast.enabled: false");

            //Minimum Master Nodes
            items.Add($"discovery.zen.minimum_master_nodes: {minimumMasterNodes}");

            //This allows the JVM to lock its memory and prevent it from being swapped by the OS
            items.Add("bootstrap.mlockall: true");

            //disable allowing to delete indices via wildcards or _all
            items.Add("action.destructive_requires_name: true");

            items.Add("EOL");

            #endregion

            var heapSize = (int)Math.Ceiling(instanceInfo.Memory / 2);
            items.Add($"sed -i -e 's|#ES_HEAP_SIZE=2g|ES_HEAP_SIZE={heapSize}g|' /etc/sysconfig/elasticsearch");

            //Insall Management Interface
            items.Add("/usr/share/elasticsearch/bin/plugin install royrusso/elasticsearch-HQ");
            items.Add("/usr/share/elasticsearch/bin/plugin install cloud-aws -b");

            //Start Service
            items.Add("service elasticsearch start");

            return items;
        }
    }
}