// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.CodeModel.References;

/// <summary>
/// The base implementation of <see cref="IRef{T}"/> except for attributes.
/// </summary>
internal abstract class BaseRef<T> : IRefImpl<T>
    where T : class, ICompilationElement
{
    // The compilation for which the symbol (stored in Target) is valid.

    static BaseRef()
    {
        if ( !typeof(T).IsInterface )
        {
            throw new ArgumentException( "The type argument must be an interface." );
        }
    }

    public virtual RefTargetKind TargetKind => RefTargetKind.Default;

    public abstract string Name { get; }

    public virtual IRefStrategy Strategy => throw new NotSupportedException();

    public abstract SerializableDeclarationId ToSerializableId();

    ICompilationElement IRef.GetTarget( ICompilation compilation, ReferenceResolutionOptions options, IGenericContext? genericContext )
        => this.GetTarget( compilation, options, genericContext );

    ICompilationElement? IRef.GetTargetOrNull( ICompilation compilation, ReferenceResolutionOptions options, IGenericContext? genericContext )
        => this.GetTargetOrNull( compilation, options, genericContext );

    public abstract IDurableRef<T> ToDurable();

    public abstract ISymbol GetClosestContainingSymbol( CompilationContext compilationContext );

    public abstract bool IsPortable { get; }

    IRef IRefImpl.ToDurable() => this.ToDurable();

    IRef<TOut> IRef.As<TOut>() => this.As<TOut>();

    public T GetTarget( ICompilation compilation, ReferenceResolutionOptions options = default, IGenericContext? genericContext = default )
        => this.GetTargetImpl( compilation, options, true, genericContext )!;

    public T? GetTargetOrNull( ICompilation compilation, ReferenceResolutionOptions options = default, IGenericContext? genericContext = default )
        => this.GetTargetImpl( compilation, options, false, genericContext );

    private T? GetTargetImpl( ICompilation compilation, ReferenceResolutionOptions options, bool throwIfMissing, IGenericContext? genericContext )
    {
        var compilationModel = (CompilationModel) compilation;

        if ( options.FollowRedirections() && compilationModel.TryGetRedirectedDeclaration( this, out var redirected ) )
        {
            // Referencing redirected declaration.
            if ( throwIfMissing )
            {
                return (T) redirected.GetTarget( compilation, options, genericContext );
            }
            else
            {
                return (T?) redirected.GetTargetOrNull( compilation, options, genericContext );
            }
        }
        else
        {
            return this.Resolve( compilationModel, options, throwIfMissing, genericContext );
        }
    }

    ISymbol ISdkRef.GetSymbol( Compilation compilation, bool ignoreAssemblyKey ) => this.GetSymbol( compilation.GetCompilationContext(), ignoreAssemblyKey );

    protected abstract ISymbol GetSymbol( CompilationContext compilationContext, bool ignoreAssemblyKey = false );

    protected abstract T? Resolve(
        CompilationModel compilation,
        ReferenceResolutionOptions options,
        bool throwIfMissing,
        IGenericContext? genericContext );

    protected static T? ReturnNullOrThrow( string id, bool throwIfMissing, CompilationModel compilation, Exception? ex = null )
    {
        if ( throwIfMissing )
        {
            throw new SymbolNotFoundException( id, compilation.RoslynCompilation, ex );
        }
        else
        {
            return null;
        }
    }

    protected static T? ConvertOrThrow( ICompilationElement? compilationElement, CompilationModel compilation )
    {
        if ( compilationElement == null )
        {
            return null;
        }

        if ( compilationElement is not T safeCast )
        {
            // Throw an exception with a better exception message for better troubleshooting.
            throw new InvalidOperationException(
                $"Cannot convert '{compilationElement}' into a {typeof(T).Name} within the compilation '{compilation.Identity}'." );
        }

        return safeCast;
    }

    public abstract IRefImpl<TOut> As<TOut>()
        where TOut : class, ICompilationElement;

    public override int GetHashCode() => this.GetHashCode( RefComparison.Default );

    public bool Equals( IRef? other ) => this.Equals( other, RefComparison.Default );

    public abstract bool Equals( IRef? other, RefComparison comparison );

    public abstract int GetHashCode( RefComparison comparison );
}