﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel.References;

/// <summary>
/// Implementation of <see cref="IDurableRef{T}"/> based on <see cref="SerializableDeclarationId"/>.
/// </summary>
internal class DeclarationIdRef<T> : StringRef<T>
    where T : class, ICompilationElement
{
    private DeclarationIdRef( string id ) : base( id ) { }

    public DeclarationIdRef( SerializableDeclarationId id ) : base( id.Id ) { }

    public override SerializableDeclarationId ToSerializableId() => new( this.Id );

    public override SerializableDeclarationId ToSerializableId( CompilationContext compilationContext ) => new( this.Id );

    protected override ISymbol GetSymbol( CompilationContext compilationContext, bool ignoreAssemblyKey = false )
        => new SerializableDeclarationId( this.Id ).ResolveToSymbol( compilationContext );

    protected override T? Resolve( CompilationModel compilation, bool throwIfMissing, IGenericContext? genericContext )
    {
        var declaration = new SerializableDeclarationId( this.Id ).ResolveToDeclaration( compilation );

        if ( declaration == null )
        {
            return ReturnNullOrThrow( this.Id, throwIfMissing, compilation );
        }

        return ConvertDeclarationOrThrow( declaration, compilation );
    }

    public override IRefImpl<TOut> As<TOut>() => this as IRefImpl<TOut> ?? new DeclarationIdRef<TOut>( this.Id );
}