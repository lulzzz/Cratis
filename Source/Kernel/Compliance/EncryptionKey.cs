// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Compliance
{
    /// <summary>
    /// Represents a key used in encryption.
    /// </summary>
    /// <param name="Value">The actual content of the key.</param>
    public record EncryptionKey(byte[] Value);
}