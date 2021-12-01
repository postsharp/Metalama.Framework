// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Serialization
{
    /// <summary>
    /// An attribute that marks a field of a type that implements <see cref="IMetaSerializable"/> as non-serialized.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    internal class MetaNonSerializedAttribute : Attribute
    {
    }
}