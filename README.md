Nager.AmazonEC2
==========
Automatic cluster installation for multiple services in Amazon AWS Cloud. Unattended installation for Windows Server over powershell.

##### Features
* Elasticsearch Cluster Installation (Elasticsearch 2.x - CentOS 7)
* RabbitMq Cluster Installation (3.6.5-1 - CentOS 7)
* Couchbase Cluster Installation (Community 4.1.0 - CentOS 7)
* Windows Server Installation (2012 R2)

##### Example RabbitMq 3.6.5-1 Installation
```cs
	var accesskey = new AmazonAccessKey("accessKeyId", "secretKey");

	var rabbitmq = new RabbitMq(accesskey);

	var clusterConfig = new RabbitMqClusterConfig();
	clusterConfig.Prefix = "nager";
	clusterConfig.KeyName = "my-secret-key";
	clusterConfig.ClusterName = "Cluster1";
	clusterConfig.NodeInstance = AmazonInstance.t2_Small;
	clusterConfig.NodeCount = 2;
	clusterConfig.AdminUsername = "Administrator";
	clusterConfig.AdminUsername = "$ecurePassword";

	var installResults = rabbitmq.InstallCluster(clusterConfig);

	var managementUrl = rabbitmq.GetManagementUrl(installResults);
```

##### Example Elasticsearch 2.x Installation
```cs
	var accesskey = new AmazonAccessKey("accessKeyId", "secretKey");
	var discoveryAccessKey = AccessKeyHelper.CreateDiscoveryAccessKey(accesskey);

	var rabbitmq = new Elasticsearch(accesskey);

	var clusterConfig = new ElasticsearchClusterConfig();
	clusterConfig.Prefix = "nager";
	clusterConfig.KeyName = "my-secret-key";
	clusterConfig.ClusterName = "Cluster1";
	clusterConfig.DiscoveryAccessKey = discoveryAccessKey;
	clusterConfig.MasterNodeCount = 2;
	clusterConfig.MasterNodeInstance = AmazonInstance.t2_Small;
	clusterConfig.DataNodeCount = 2;
	clusterConfig.DataNodeInstance = AmazonInstance.t2_Medium;
	clusterConfig.ClientNodeCount = 2;
	clusterConfig.ClientNodeInstance = AmazonInstance.t2_Small;            

	var installResults = rabbitmq.InstallCluster(clusterConfig);

	var managementUrl = rabbitmq.GetManagementUrl(installResults);
```

##### Example Couchbase Community 4.1.0 Installation
```cs
	var accesskey = new AmazonAccessKey("accessKeyId", "secretKey");

	var couchbase = new Couchbase(accesskey);

	var clusterConfig = new CouchbaseClusterConfig();
	clusterConfig.Prefix = "nager";
	clusterConfig.KeyName = "my-secret-key";
	clusterConfig.ClusterName = "Cluster1";
	clusterConfig.NodeInstance = AmazonInstance.t2_Small;
	clusterConfig.NodeCount = 2;
	clusterConfig.AdminUsername = "Administrator";
	clusterConfig.AdminUsername = "$ecurePassword";

	var installResults = couchbase.InstallCluster(clusterConfig);

	var managementUrl = couchbase.GetManagementUrl(installResults);
```

##### Example Windows Server 2012 R2 Installation
```cs
	var installScript = new WindowsInstallScript();
	//Disable Windows Firewall
	installScript.DisableFirewall();
	//Download and install msi package
	installScript.InstallMsi("http://7-zip.org/a/7z1602-x64.msi");
	//Set a new windows Administrator password
	installScript.SetAdministratorPassword("ComplexPa$$w0rd");

	var windowsServer = new WindowsServer(accesskey);
	windowsServer.Install(AmazonInstance.t2_Small, "WindowsServer1", "sg-edcd9784", "my-secret-key", installScript);
```
