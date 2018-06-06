// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
namespace Microsoft.Azure.Devices.Client.Edge
{
    using System;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;

    internal interface ICertificateValidator
    {
        void SetupCertificateValidation();

        Func<object, X509Certificate, X509Chain, SslPolicyErrors, bool> GetCustomCertificateValidation();
    }
}
