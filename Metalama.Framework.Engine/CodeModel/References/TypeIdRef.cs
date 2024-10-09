// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.CodeModel.References;

internal class TypeIdRef<T> : DurableRef<T>
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

    protected override ICompilationElement? Resolve(
        CompilationModel compilation,
        bool throwIfMissing,
        IGenericContext? genericContext,
        Type interfaceType )
    {
        if ( !compilation.SerializableTypeIdResolver.TryResolveId( new SerializableTypeId( this.Id ), out var symbol ) )
        {
            return ReturnNullOrThrow( this.Id, throwIfMissing, compilation );
        }

        return ConvertDeclarationOrThrow( symbol, compilation, interfaceType );
    }

    protected override IRef<TOut> CastAsRef<TOut>() => this as IRef<TOut> ?? new TypeIdRef<TOut>( this.Id );

    public override IFullRef ToFullRef( CompilationContext compilationContext )
    {
        if ( !compilationContext.SerializableTypeIdResolver.TryResolveId( new SerializableTypeId( this.Id ), out var symbol ) )
        {
            throw new InvalidOperationException( $"Unable to resolve type id: {this.Id}." );
        }

        return compilationContext.RefFactory.FromAnySymbol( symbol );
    }
}