// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.CodeModel.References;

internal class TypeIdRef<T> : StringRef<T>
    where T : class, ICompilationElement
{
    private TypeIdRef( string id ) : base( id )
    {
        Invariant.Assert( SerializableTypeId.IsTypeId( id ) );
    }

    public TypeIdRef( SerializableTypeId id ) : base( id.Id ) { }

    public override SerializableDeclarationId ToSerializableId() => throw new NotSupportedException();

    public override SerializableDeclarationId ToSerializableId( CompilationContext compilationContext ) => throw new NotSupportedException();

    protected override ISymbol GetSymbol( CompilationContext compilationContext, bool ignoreAssemblyKey = false )
    {
        if ( !compilationContext.SerializableTypeIdResolver.TryResolveId( new SerializableTypeId( this.Id ), out var symbol ) )
        {
            throw new InvalidOperationException( $"Unable to resolve type id: {this.Id}." );
        }

        return symbol;
    }

    protected override T? Resolve( CompilationModel compilation, ReferenceResolutionOptions options, bool throwIfMissing, IGenericContext? genericContext )
    {
        if ( !compilation.SerializableTypeIdResolver.TryResolveId( new SerializableTypeId( this.Id ), out var symbol ) )
        {
            return ReturnNullOrThrow( this.Id, throwIfMissing, compilation );
        }

        return ConvertOrThrow( symbol, compilation );
    }

    public override IRefImpl<TOut> As<TOut>() => this as IRefImpl<TOut> ?? new TypeIdRef<TOut>( this.Id );
}