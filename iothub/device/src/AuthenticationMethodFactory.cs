// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Devices.Client
{
    using System;
    using Microsoft.Azure.Devices.Client.Extensions;
    using System.Collections;
    using Microsoft.Azure.Devices.Client.HsmAuthentication;

    /// <summary>
    /// Creates an instance of an implementation of <see cref="IAuthenticationMethod"/> based on known authentication parameters.
    /// </summary>
    public sealed class AuthenticationMethodFactory
    {
        internal static IAuthenticationMethod GetAuthenticationMethod(IotHubConnectionStringBuilder iotHubConnectionStringBuilder)
        {
            if (iotHubConnectionStringBuilder.SharedAccessKeyName != null)
            {
                return new DeviceAuthenticationWithSharedAccessPolicyKey(
                    iotHubConnectionStringBuilder.DeviceId, iotHubConnectionStringBuilder.SharedAccessKeyName, iotHubConnectionStringBuilder.SharedAccessKey);
            }
            else if (iotHubConnectionStringBuilder.SharedAccessKey != null)
            {

#if ENABLE_MODULES_SDK
                if(iotHubConnectionStringBuilder.ModuleId != null)
                {
                    return new ModuleAuthenticationWithRegistrySymmetricKey(
                        iotHubConnectionStringBuilder.DeviceId, iotHubConnectionStringBuilder.ModuleId, iotHubConnectionStringBuilder.SharedAccessKey);
                }
                else
                {
                    return new DeviceAuthenticationWithRegistrySymmetricKey(
                        iotHubConnectionStringBuilder.DeviceId, iotHubConnectionStringBuilder.SharedAccessKey);
                }
#else
                return new DeviceAuthenticationWithRegistrySymmetricKey(
                    iotHubConnectionStringBuilder.DeviceId, iotHubConnectionStringBuilder.SharedAccessKey);
#endif
            }
            else if (iotHubConnectionStringBuilder.SharedAccessSignature != null)
            {
#if ENABLE_MODULES_SDK
                if(iotHubConnectionStringBuilder.ModuleId != null)
                {
                    return new ModuleAuthenticationWithToken(
                        iotHubConnectionStringBuilder.DeviceId, iotHubConnectionStringBuilder.ModuleId, iotHubConnectionStringBuilder.SharedAccessSignature);
                }
                else
                {
                    return new DeviceAuthenticationWithToken(
                        iotHubConnectionStringBuilder.DeviceId, iotHubConnectionStringBuilder.SharedAccessSignature);
                }
#else
                return new DeviceAuthenticationWithToken(iotHubConnectionStringBuilder.DeviceId, iotHubConnectionStringBuilder.SharedAccessSignature);
#endif
            }
#if !NETMF
            else if (iotHubConnectionStringBuilder.UsingX509Cert)
            {
                return new DeviceAuthenticationWithX509Certificate(iotHubConnectionStringBuilder.DeviceId, iotHubConnectionStringBuilder.Certificate);
            }
#endif

#if NETMF
            throw new InvalidOperationException("Unsupported Authentication Method " + iotHubConnectionStringBuilder.ToString());
#else
            throw new InvalidOperationException("Unsupported Authentication Method {0}".FormatInvariant(iotHubConnectionStringBuilder));
#endif
        }

        /// <summary>
        /// Creates a <see cref="DeviceAuthenticationWithSharedAccessPolicyKey"/> instance based on the parameters.
        /// </summary>
        /// <param name="deviceId">Device Identifier.</param>
        /// <param name="policyName">Name of the shared access policy to use.</param>
        /// <param name="key">Key associated with the shared access policy.</param>
        /// <returns>A new instance of the <see cref="DeviceAuthenticationWithSharedAccessPolicyKey"/> class.</returns>
        public static IAuthenticationMethod CreateAuthenticationWithSharedAccessPolicyKey(string deviceId, string policyName, string key)
        {
            return new DeviceAuthenticationWithSharedAccessPolicyKey(deviceId, policyName, key);
        }

        /// <summary>
        /// Creates a <see cref="DeviceAuthenticationWithToken"/> instance based on the parameters.
        /// </summary>
        /// <param name="deviceId">Device Identifier.</param>
        /// <param name="token">Security token associated with the device.</param>
        /// <returns>A new instance of the <see cref="DeviceAuthenticationWithToken"/> class.</returns>
        public static IAuthenticationMethod CreateAuthenticationWithToken(string deviceId, string token)
        {
            return new DeviceAuthenticationWithToken(deviceId, token);
        }

#if ENABLE_MODULES_SDK
        /// <summary>
        /// Creates a <see cref="ModuleAuthenticationWithToken"/> instance based on the parameters.
        /// </summary>
        /// <param name="deviceId">Device Identifier.</param>
        /// <param name="moduleId">Module Identifier.</param>
        /// <param name="token">Security token associated with the device.</param>
        /// <returns>A new instance of the <see cref="ModuleAuthenticationWithToken"/> class.</returns>
        public static IAuthenticationMethod CreateAuthenticationWithToken(string deviceId, string moduleId, string token)
        {
            return new ModuleAuthenticationWithToken(deviceId, moduleId, token);
        }
#endif

        /// <summary>
        /// Creates a <see cref="DeviceAuthenticationWithRegistrySymmetricKey"/> instance based on the parameters.
        /// </summary>
        /// <param name="deviceId">Device Identifier.</param>
        /// <param name="key">Key associated with the device in the device registry.</param>
        /// <returns>A new instance of the <see cref="DeviceAuthenticationWithRegistrySymmetricKey"/> class.</returns>
        public static IAuthenticationMethod CreateAuthenticationWithRegistrySymmetricKey(string deviceId, string key)
        {
            return new DeviceAuthenticationWithRegistrySymmetricKey(deviceId, key);
        }

#if ENABLE_MODULES_SDK
        /// <summary>
        /// Creates a <see cref="ModuleAuthenticationWithRegistrySymmetricKey"/> instance based on the parameters.
        /// </summary>
        /// <param name="deviceId">Device Identifier.</param>
        /// <param name="moduleId">Module Identifier.</param>
        /// <param name="key">Key associated with the module in the device registry.</param>
        /// <returns>A new instance of the <see cref="ModuleAuthenticationWithRegistrySymmetricKey"/> class.</returns>
        public static IAuthenticationMethod CreateAuthenticationWithRegistrySymmetricKey(string deviceId, string moduleId, string key)
        {
            return new ModuleAuthenticationWithRegistrySymmetricKey(deviceId, moduleId, key);
        }

        public static IAuthenticationMethod CreateAuthenticationWithEdge()
        {
            return new EdgeAuthenticationMethodFactory().Create();
            
        }

        class EdgeAuthenticationMethodFactory 
        {
            const string IotEdgedUriVariableName = "IOTEDGE_IOTEDGEDURI";
            const string IotEdgedApiVersionVariableName = "IOTEDGE_IOTEDGEDVERSION";
            const string IotHubHostnameVariableName = "IOTEDGE_IOTHUBHOSTNAME";
            const string GatewayHostnameVariableName = "IOTEDGE_GATEWAYHOSTNAME";
            const string DeviceIdVariableName = "IOTEDGE_DEVICEID";
            const string ModuleIdVariableName = "IOTEDGE_MODULEID";
            const string AuthSchemeVariableName = "IOTEDGE_AUTHSCHEME";
            const string SasTokenAuthScheme = "SasToken";
            const string EdgehubConnectionstringVariableName = "EdgeHubConnectionString";
            const string IothubConnectionstringVariableName = "IotHubConnectionString";

            public IAuthenticationMethod Create()
            {
                IDictionary envVariables = Environment.GetEnvironmentVariables();

                string connectionString = this.GetValueFromEnvironment(envVariables, EdgehubConnectionstringVariableName) ?? this.GetValueFromEnvironment(envVariables, IothubConnectionstringVariableName);

                // First try to create from connection string and if env variable for connection string is not found try to create from edgedUri
                if (!string.IsNullOrWhiteSpace(connectionString))
                {
                     var iotHubConnectionStringBuilder = IotHubConnectionStringBuilder.Create(connectionString);
                     return GetAuthenticationMethod(iotHubConnectionStringBuilder);
                }
                else
                {
                    string edgedUri = this.GetValueFromEnvironment(envVariables, IotEdgedUriVariableName) ?? throw new InvalidOperationException($"Environement variable {IotEdgedUriVariableName} is required.");
                    string deviceId = this.GetValueFromEnvironment(envVariables, DeviceIdVariableName) ?? throw new InvalidOperationException($"Environement variable {DeviceIdVariableName} is required.");
                    string moduleId = this.GetValueFromEnvironment(envVariables, ModuleIdVariableName) ?? throw new InvalidOperationException($"Environement variable {ModuleIdVariableName} is required.");
                    string hostname = this.GetValueFromEnvironment(envVariables, IotHubHostnameVariableName) ?? throw new InvalidOperationException($"Environement variable {IotHubHostnameVariableName} is required.");
                    string authScheme = this.GetValueFromEnvironment(envVariables, AuthSchemeVariableName) ?? throw new InvalidOperationException($"Environement variable {AuthSchemeVariableName} is required.");
                    string gateway = this.GetValueFromEnvironment(envVariables, GatewayHostnameVariableName);
                    string apiVersion = this.GetValueFromEnvironment(envVariables, IotEdgedApiVersionVariableName);

                    if (!string.Equals(authScheme, SasTokenAuthScheme, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException($"Unsupported authentication scheme. Supported scheme is {SasTokenAuthScheme}.");
                    }

                    ISignatureProvider signatureProvider = string.IsNullOrWhiteSpace(apiVersion)
                        ? new HttpHsmSignatureProvider(edgedUri)
                        : new HttpHsmSignatureProvider(edgedUri, apiVersion);
                    var authMethod = new ModuleAuthenticationWithHsm(signatureProvider, deviceId, moduleId);

                    return authMethod;
                }
            }

            string GetValueFromEnvironment(IDictionary envVariables, string variableName)
            {
                if (envVariables.Contains(variableName))
                {
                    return envVariables[variableName].ToString();
                }

                return null;
            }

        }
#endif
    }
}
