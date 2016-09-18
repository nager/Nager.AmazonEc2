using Amazon.IdentityManagement;
using Amazon.IdentityManagement.Model;
using log4net;
using Nager.AmazonEc2.Model;
using System;
using System.Net;

namespace Nager.AmazonEc2.Helper
{
    public static class AccessKeyHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(AccessKeyHelper));

        public static AmazonAccessKey CreateDiscoveryAccessKey(AmazonAccessKey accessKey, string prefix = "nager")
        {
            var userName = $"{prefix}.ElasticsearchDiscovery";

            using (var identityClient = new AmazonIdentityManagementServiceClient(accessKey.AccessKeyId, accessKey.SecretKey))
            {

                try
                {
                    var getUserResponse = identityClient.GetUser(new GetUserRequest() { UserName = userName });
                    if (getUserResponse != null)
                    {
                        Log.DebugFormat("CreateDiscoveryAccessKey - User already exist {0}", userName);
                        return CreateAccessKey(identityClient, userName);
                    }
                }
                catch (NoSuchEntityException exception)
                {
                    if (exception.StatusCode != HttpStatusCode.NotFound)
                    {
                        return null;
                    }
                }
                catch (Exception exception)
                {
                    Log.Error("CreateDiscoveryAccessKey", exception);
                    return null;
                }

                #region Create user

                var createUserResponse = identityClient.CreateUser(new CreateUserRequest(userName));
                if (createUserResponse.HttpStatusCode != HttpStatusCode.OK)
                {
                    return null;
                }

                #endregion

                #region Create policy

                var policyName = $"{prefix}.ElasticsearchDiscovery";
                var policyArn = string.Empty;


                var listPolicyResponse = identityClient.ListPolicies(new ListPoliciesRequest() { PathPrefix = "/nager/" });
                if (listPolicyResponse.Policies.Count > 0)
                {
                    foreach (var policy in listPolicyResponse.Policies)
                    {
                        if (policy.PolicyName == policyName)
                        {
                            policyArn = policy.Arn;
                            break;
                        }
                    }
                }

                if (String.IsNullOrEmpty(policyArn))
                {
                    var policyRequest = new CreatePolicyRequest();
                    policyRequest.PolicyName = $"{prefix}.ElasticsearchDiscovery";
                    policyRequest.Description = "Allow discovery for Elasticsearch created by Nager.AmazonEc2";
                    policyRequest.Path = "/nager/";
                    policyRequest.PolicyDocument = @"{
                ""Version"": ""2012-10-17"",
                ""Statement"": [
                    {
                        ""Effect"": ""Allow"",
                        ""Action"": [ ""ec2:DescribeInstances"" ],
                        ""Resource"": [ ""*"" ]
                    }
                ]}";

                    try
                    {
                        var createPolicyResponse = identityClient.CreatePolicy(policyRequest);
                        if (createPolicyResponse.HttpStatusCode != HttpStatusCode.OK)
                        {
                            return null;
                        }

                        policyArn = createPolicyResponse.Policy.Arn;
                    }
                    catch (Exception exception)
                    {
                        Log.Error("CreateDiscoveryAccessKey", exception);
                        return null;
                    }
                }


                #endregion

                #region Attach policy to user

                var attachUserPolicyResponse = identityClient.AttachUserPolicy(new AttachUserPolicyRequest() { UserName = userName, PolicyArn = policyArn });
                if (attachUserPolicyResponse.HttpStatusCode != HttpStatusCode.OK)
                {
                    return null;
                }

                #endregion

                return CreateAccessKey(identityClient, userName);
            }
        }

        private static AmazonAccessKey CreateAccessKey(AmazonIdentityManagementServiceClient client, string userName)
        {
            var createAccessKeyResponse = client.CreateAccessKey(new CreateAccessKeyRequest() { UserName = userName });
            if (createAccessKeyResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            return new AmazonAccessKey { AccessKeyId = createAccessKeyResponse.AccessKey.AccessKeyId, SecretKey = createAccessKeyResponse.AccessKey.SecretAccessKey };
        }
    }
}