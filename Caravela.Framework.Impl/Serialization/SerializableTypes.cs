// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Caravela.Framework.Impl.Serialization
{
    /// <summary>
    /// Contains the set of types that can be serialized by <see cref="SyntaxSerializationService"/>. This class can be used to
    /// determine the possibility to serialize a type when the template is compiled. It may return false positives.
    /// </summary>
    internal class SerializableTypes
    {
        private readonly ImmutableHashSet<string> _serializableTypes;

        public SerializableTypes( IEnumerable<ITypeSymbol> serializableTypes )
        {
            this._serializableTypes = serializableTypes.Select( t => t.GetDocumentationCommentId().AssertNotNull() ).ToImmutableHashSet();
        }

        public bool IsSerializable( ITypeSymbol type, Location? diagnosticLocation = null, IDiagnosticAdder? diagnosticAdder = null )
        {
            var id = type.GetDocumentationCommentId().AssertNotNull();
            
            if ( this._serializableTypes.Contains( id ) )
            {
                return true;
            }
            else if ( type is INamedTypeSymbol namedType )
            {
                if ( namedType.IsGenericType && !SymbolEqualityComparer.Default.Equals( namedType, namedType.ConstructedFrom ) )
                {
                    if ( namedType.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T )
                    {
                        return this.IsSerializable( namedType.TypeArguments[0], diagnosticLocation, diagnosticAdder );
                    }
                    else if ( this.IsSerializable( namedType.ConstructedFrom, diagnosticLocation, diagnosticAdder ) )
                    {
                        // The child call will report the diagnostic.
                        return namedType.TypeArguments.All( arg => this.IsSerializable( arg, diagnosticLocation, diagnosticAdder ) );
                    }
                }
            }

            if ( diagnosticAdder != null )
            {
                diagnosticAdder.ReportDiagnostic( SerializationDiagnosticDescriptors.UnsupportedSerialization.CreateDiagnostic( diagnosticLocation, type ) );
            }

            return false;
        }
    }
}