// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Framework.Serialization
{
    /// <summary>
    /// An attribute that marks a field of a type that implements <see cref="ILamaSerializable"/> as non-serialized.
    /// </summary>
    [AttributeUsage( AttributeTargets.Field )]
    internal class LamaNonSerializedAttribute : Attribute { }
}