// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel.References;

/// <summary>
/// A reference-typed version of the <see cref="Ref{T}"/> struct aimed at making boxing operations explicit
/// during type conversions.
/// </summary>
/// <typeparam name="T"></typeparam>
internal class BoxedRef<T> : IRefImpl<T>
    where T : class, ICompilationElement
{
    private readonly Ref<IDeclaration> _underlying;

    public BoxedRef( in Ref<IDeclaration> underlying )
    {
        this._underlying = underlying;
    }

    public SerializableDeclarationId ToSerializableId() => this._underlying.ToSerializableId();

    public T GetTarget( ICompilation compilation, ReferenceResolutionOptions options = default ) => (T) this._underlying.GetTarget( compilation, options );

    public T? GetTargetOrNull( ICompilation compilation, ReferenceResolutionOptions options = default )
        => (T?) this._underlying.GetTargetOrNull( compilation, options );

    public IRef<TOut> As<TOut>()
        where TOut : class, ICompilationElement
        => this as IRef<TOut> ?? new BoxedRef<TOut>( this._underlying );

    public ISymbol? GetSymbol( Compilation compilation, bool ignoreAssemblyKey = false ) => this._underlying.GetSymbol( compilation, ignoreAssemblyKey );

    public object? Target => this._underlying.Target;

    public bool IsDefault => this._underlying.IsDefault;

    public DeclarationRefTargetKind TargetKind => this._underlying.TargetKind;

    public ISymbol GetClosestSymbol( CompilationContext compilationContext ) => this._underlying.GetClosestSymbol( compilationContext );

    public bool Equals( IRef<ICompilationElement>? other ) => RefEqualityComparer.Default.Equals( this, other );

    public bool Equals( IRef<ICompilationElement>? other, bool includeNullability )
        => RefEqualityComparer.GetInstance( includeNullability ).Equals( this, other );

    public override int GetHashCode() => RefEqualityComparer.Default.GetHashCode( this );
}