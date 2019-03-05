// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Client
{
    public interface IServiceDiscovery
    {
        Task<ServiceProfile> GetServiceProfileAsync();
    }

    public class ServiceProfile
    {
        public ServiceProfile() { }
        public ServiceProfile(string canonicalHostname, List<IPEndPoint> ips)
        {
            this.Hostname = canonicalHostname;
            this.Addresses = ips;
        }

        public string Hostname { get; set; }
        public IList<IPEndPoint> Addresses { get; set; }
    }
}