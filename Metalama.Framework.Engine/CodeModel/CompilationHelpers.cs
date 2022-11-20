// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.CodeModel
{
    internal class CompilationHelpers : ICompilationHelpers
    {
        public IteratorInfo GetIteratorInfo( IMethod method ) => method.GetIteratorInfoImpl();

        public AsyncInfo GetAsyncInfo( IMethod method ) => method.GetAsyncInfoImpl();

        public AsyncInfo GetAsyncInfo( IType type ) => type.GetAsyncInfoImpl();

        public string GetFullMetadataName( INamedType type ) => ((INamedTypeSymbol) ((INamedTypeInternal) type).TypeSymbol).GetFullMetadataName();

        public SerializableTypeId GetSerializableId( IType type ) => SerializableTypeIdProvider.GetId( type.GetSymbol() );

        public SerializableDeclarationId GetSerializableId( IDeclaration declaration )
        {
            var symbol = declaration.GetSymbol();

            if ( symbol == null )
            {
                throw new NotImplementedException( $"Getting a {nameof(SerializableDeclarationId)} for an introduced declaration is not implemented." );
            }

            return symbol.GetSerializableId();
        }

        public bool TryGetSerializableId( IDeclaration declaration, out SerializableDeclarationId id )
        {
            var symbol = declaration.GetSymbol();

            if ( symbol == null )
            {
                id = default;

                return false;
            }

            return symbol.TryGetSerializableId( out id );
        }
    }
}