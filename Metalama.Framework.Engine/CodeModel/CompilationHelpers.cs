// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel
{
    internal sealed class CompilationHelpers : ICompilationHelpers
    {
        public IteratorInfo GetIteratorInfo( IMethod method ) => method.GetIteratorInfoImpl();

        public AsyncInfo GetAsyncInfo( IMethod method ) => method.GetAsyncInfoImpl();

        public AsyncInfo GetAsyncInfo( IType type ) => type.GetAsyncInfoImpl();

        public string GetMetadataName( INamedType type ) => ((INamedTypeSymbol) ((INamedTypeInternal) type).TypeSymbol).GetMetadataName();

        public string GetFullMetadataName( INamedType type ) => ((INamedTypeSymbol) ((INamedTypeInternal) type).TypeSymbol).GetFullMetadataName();

        public SerializableTypeId GetSerializableId( IType type ) => type.GetSymbol().GetSerializableTypeId();
    }
}