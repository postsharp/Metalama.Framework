// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Comparers;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Threading;

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

    public abstract SerializableDeclarationId ToSerializableId();

    public abstract SerializableDeclarationId ToSerializableId( CompilationContext compilationContext );

    ICompilationElement IRef.GetTarget( ICompilation compilation, IGenericContext? genericContext ) => this.GetTarget( compilation, genericContext );

    ICompilationElement? IRef.GetTargetOrNull( ICompilation compilation, IGenericContext? genericContext )
        => this.GetTargetOrNull( compilation, genericContext );

    public abstract IDurableRef<T> ToDurable();

    public abstract ISymbol GetClosestContainingSymbol( CompilationContext compilationContext );

    public abstract bool IsDurable { get; }

    IRef IRefImpl.ToDurable() => this.ToDurable();

    IRef<TOut> IRef.As<TOut>() => this.As<TOut>();

    public T GetTarget( ICompilation compilation, IGenericContext? genericContext = null ) => this.GetTargetImpl( compilation, true, genericContext )!;

    public T? GetTargetOrNull( ICompilation compilation, IGenericContext? genericContext = null ) => this.GetTargetImpl( compilation, false, genericContext );

    private T? GetTargetImpl( ICompilation compilation, bool throwIfMissing, IGenericContext? genericContext = null )
    {
        using ( StackOverflowHelper.Detect() )
        {
            var compilationModel = (CompilationModel) compilation;

            return this.Resolve( compilationModel, throwIfMissing, genericContext );
        }
    }

    ISymbol ISdkRef.GetSymbol( Compilation compilation, bool ignoreAssemblyKey ) => this.GetSymbol( compilation.GetCompilationContext(), ignoreAssemblyKey );

    protected abstract ISymbol GetSymbol( CompilationContext compilationContext, bool ignoreAssemblyKey = false );

    protected abstract T? Resolve(
        CompilationModel compilation,
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

    protected static T? ConvertDeclarationOrThrow( ICompilationElement? compilationElement, CompilationModel compilation, bool throwOnError = true )
    {
        var result = compilationElement switch
        {
            null => null,
            T desired => desired,
            IProperty property when typeof(T) == typeof(IField) && property.OriginalField != null => (T) property.OriginalField,
            IField field when typeof(T) == typeof(IProperty) && field.OverridingProperty != null => (T) field.OverridingProperty,
            _ when throwOnError => throw new InvalidCastException(
                $"Cannot convert the '{compilationElement}' {compilationElement.DeclarationKind.ToDisplayString()} into a {typeof(T).Name} within the compilation '{compilation.Identity}'." ),
            _ => null
        };

        return result;
    }

    public abstract IRefImpl<TOut> As<TOut>()
        where TOut : class, ICompilationElement;

    // We throw an exception when the vanilla GetHashCode is used to make sure that the caller specifies a RefEqualityComparer 
    // when using IRef as a key of dictionaries.
    public override int GetHashCode() => throw new InvalidOperationException( $"Specify a {nameof(RefEqualityComparer)} or a RefComparison." );

    public bool Equals( IRef? other ) => this.Equals( other, RefComparison.Default );

    public abstract bool Equals( IRef? other, RefComparison comparison );

    public abstract int GetHashCode( RefComparison comparison );
}