

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Zeroconf;

namespace Microsoft.Azure.Devices.Client.Edge
{
    static class EdgeHubServiceDiscovery
    {
        public static async Task<ServiceProfile> DiscoverEdgeHub(string serviceName)
        {
            var result = new List<IPEndPoint>();
            var hosts = await ZeroconfResolver.ResolveAsync("_edgehub._tcp.local.").ConfigureAwait(false);
            foreach (var zeroconfHost in hosts)
            {
                var ips = new List<IPEndPoint>();
                if (zeroconfHost.DisplayName.Equals(serviceName, StringComparison.InvariantCultureIgnoreCase))
                {
                    foreach (var ipAddress in zeroconfHost.IPAddresses)
                    {
                        if (IPAddress.TryParse(ipAddress, out IPAddress address))
                        {
                            ips.Add(new IPEndPoint(address, zeroconfHost.Services["_edgehub._tcp.local."].Port));
                        }
                    }
                }
                return new ServiceProfile(zeroconfHost.CanonicalHostname.TrimEnd('.'), ips);
            }

            return new ServiceProfile(serviceName, result);

        }
    }
}
