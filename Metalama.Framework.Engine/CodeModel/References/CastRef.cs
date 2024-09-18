// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CodeModel.References;

internal class CastRef<T> : IRefImpl<T>
    where T : class, ICompilationElement
{
    private readonly IRefImpl _underlying;

    public CastRef( IRefImpl underlying )
    {
        this._underlying = underlying.Unwrap();
    }

    public ISymbol GetClosestSymbol( CompilationContext compilationContext ) => this._underlying.GetClosestSymbol( compilationContext );

    public (ImmutableArray<AttributeData> Attributes, ISymbol Symbol) GetAttributeData( CompilationContext compilationContext )
        => this._underlying.GetAttributeData( compilationContext );

    public string Name => this._underlying.Name;

    public IRefStrategy Strategy => this._underlying.Strategy;

    public IRefImpl Unwrap() => this._underlying.Unwrap();

    public ISymbol? GetSymbol( Compilation compilation, bool ignoreAssemblyKey = false ) => this._underlying.GetSymbol( compilation, ignoreAssemblyKey );

    public bool Equals( IRef? other ) => this._underlying.Equals( other );

    public SerializableDeclarationId ToSerializableId() => this._underlying.ToSerializableId();

    IRefImpl<TOut> IRefImpl<T>.As<TOut>() => this as IRefImpl<TOut> ?? new CastRef<TOut>( this );

    T IRef<T>.GetTarget( ICompilation compilation, ReferenceResolutionOptions options, IGenericContext? genericContext )
        => (T) this._underlying.GetTarget( compilation, options, genericContext );

    T? IRef<T>.GetTargetOrNull( ICompilation compilation, ReferenceResolutionOptions options, IGenericContext? genericContext )
        => (T?) this._underlying.GetTargetOrNull( compilation, options, genericContext );

    public ICompilationElement GetTarget(
        ICompilation compilation,
        ReferenceResolutionOptions options = ReferenceResolutionOptions.Default,
        IGenericContext? genericContext = null )
        => this._underlying.GetTarget( compilation, options, genericContext );

    public ICompilationElement? GetTargetOrNull(
        ICompilation compilation,
        ReferenceResolutionOptions options = ReferenceResolutionOptions.Default,
        IGenericContext? genericContext = null )
        => this._underlying.GetTargetOrNull( compilation, options, genericContext );

    public IRef<TOut> As<TOut>()
        where TOut : class, ICompilationElement
        => this._underlying.As<TOut>();
}