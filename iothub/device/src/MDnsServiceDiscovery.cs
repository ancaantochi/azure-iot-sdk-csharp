using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client.Edge;

namespace Microsoft.Azure.Devices.Client
{
    public class MDnsServiceDiscovery : IServiceDiscovery
    {
        private string serviceName;
        public MDnsServiceDiscovery(string serviceName)
        {
            this.serviceName = serviceName;
        }

        public Task<ServiceProfile> GetServiceProfileAsync()
        {
            return EdgeHubServiceDiscovery.DiscoverEdgeHub(serviceName);
        }
    }
}
