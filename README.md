Nager.AmazonEC2
==========

#####Features
* Couchbase Cluster Installation
* Elasticsearch Cluster Installation
* RabbitMq Cluster Installation
* Windows Installation


#####Example Couchbase Installation
```cs
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
```