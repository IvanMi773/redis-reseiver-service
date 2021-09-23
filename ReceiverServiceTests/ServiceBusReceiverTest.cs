using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Xunit;
using Xunit.Abstractions;

namespace ReceiverServiceTests
{
    public class ServiceBusReceiverTest
    {
        private readonly ITestOutputHelper _output;

        public ServiceBusReceiverTest(ITestOutputHelper output)
        {
            this._output = output;
        }

        [Fact]
        public void GetToken()
        {
            
            var r =
                "Endpoint=sb://test-bus-32.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=TEjsFC9BypXZT5bgVLTK4W1dd5tQBgwWEviXIYE4rE8=";
            var eventHubHostName = "test-bus-32.servicebus.windows.net";
            var eventHubName = "test-topic";
            var keyName = "RootManageSharedAccessKey";
            var keyValue = "TEjsFC9BypXZT5bgVLTK4W1dd5tQBgwWEviXIYE4rE8=";
            // var publisher = "publisher1";
            var sasToken = CreateToken($"https://{eventHubHostName}/{eventHubName}/", keyName,
                keyValue);
            _output.WriteLine(sasToken);
        }

        private string CreateToken(string resourceUri, string keyName, string key)
        {
            TimeSpan sinceEpoch = DateTime.UtcNow - new DateTime(1970, 1, 1);
            var week = 60 * 60 * 24 * 7;
            var expiry = Convert.ToString((int)sinceEpoch.TotalSeconds + week);
            string stringToSign = HttpUtility.UrlEncode(resourceUri) + "\n" + expiry;
            HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));
            var sasToken = String.Format(CultureInfo.InvariantCulture, "SharedAccessSignature sr={0}&sig={1}&se={2}&skn={3}", HttpUtility.UrlEncode(resourceUri), HttpUtility.UrlEncode(signature), expiry, keyName);
            return sasToken;
        }
    }
}