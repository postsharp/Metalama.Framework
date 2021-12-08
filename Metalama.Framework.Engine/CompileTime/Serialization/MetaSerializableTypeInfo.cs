// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Metalama.Framework.Impl.CompileTime.Serialization
{
    /// <summary>
    /// Describes serializable type, serializer of which needs to be emitted in this compilation.
    /// </summary>
    internal class MetaSerializableTypeInfo
    {
        /// <summary>
        /// Gets the serializable type.
        /// </summary>
        public INamedTypeSymbol Type { get; }

        /// <summary>
        /// Gets a list of serializable fields an properties.
        /// </summary>
        public IReadOnlyList<ISymbol> SerializedMembers { get; }

        public MetaSerializableTypeInfo( INamedTypeSymbol type, IReadOnlyList<ISymbol> serializedMembers )
        {
            this.Type = type;
            this.SerializedMembers = serializedMembers;
        }
    }
}