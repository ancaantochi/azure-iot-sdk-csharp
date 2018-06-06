﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
namespace Microsoft.Azure.Devices.Client.Edge
{
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;

    internal interface ITrustBundleProvider
    {
        Task<IList<X509Certificate2>> GetTrustBundleAsync(Uri providerUri, string defaultApiVersion);
    }
}
