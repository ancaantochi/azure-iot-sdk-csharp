﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Devices
{
    using Newtonsoft.Json;

    /// <summary>
    /// Encapsulates the result of a bulk registry operation.
    /// </summary>
    public sealed class BulkRegistryOperationResult
    {
        /// <summary>
        /// Whether or not the operation was successful.
        /// </summary>
        [JsonProperty(PropertyName = "isSuccessful", Required = Required.Always)]
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// If the operation was not successful, this contains an array of DeviceRegistryOperationError objects.
        /// </summary>
        [JsonProperty(PropertyName = "errors", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DeviceRegistryOperationError[] Errors { get; set; }
    }
}
