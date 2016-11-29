using Amazon;
using Amazon.EC2;
using Amazon.EC2.Model;
using log4net;
using Nager.AmazonEc2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Nager.AmazonEc2.Project
{
    public class ProjectBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProjectBase));

        public AmazonEC2Client Client;
        public RegionEndpoint RegionEndpoint;

        public ProjectBase(AmazonAccessKey accessKey, RegionEndpoint regionEnpoint)
        {
            this.RegionEndpoint = regionEnpoint;
            this.Client = new AmazonEC2Client(accessKey.AccessKeyId, accessKey.SecretKey, regionEnpoint);
        }

        public List<string> GetAvailabilityZones()
        {
            var filter = new Filter("region-name", new List<string> { this.RegionEndpoint.SystemName });

            var response = this.Client.DescribeAvailabilityZones(new DescribeAvailabilityZonesRequest() { Filters = new List<Filter> { filter } });
            return response?.AvailabilityZones.Select(o => o.ZoneName).ToList();
        }

        public string GetImageId(string ownerId, string name)
        {
            var filterOwner = new Filter("owner-id", new List<string> { ownerId }); //amazon
            var filterName = new Filter("name", new List<string> { name });

            var describeImagesRequest = new DescribeImagesRequest() { Filters = new List<Filter>() { filterOwner, filterName } };
            var response = this.Client.DescribeImages(describeImagesRequest);
            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                Log.Error("GetImageId - Cannot get the ami id");
            }

            return response.Images?.OrderByDescending(o => o.CreationDate).Select(o => o.ImageId).FirstOrDefault();
        }
    }
}
