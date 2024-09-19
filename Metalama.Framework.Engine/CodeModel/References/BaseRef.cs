// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.References;

/// <summary>
/// The base implementation of <see cref="ISdkRef{T}"/>.
/// </summary>
/// <typeparam name="T"></typeparam>
internal abstract class BaseRef<T> : IRefImpl<T>
    where T : class, ICompilationElement
{
    // The compilation for which the symbol (stored in Target) is valid.

    public abstract CompilationContext CompilationContext { get; }

    static BaseRef()
    {
        if ( !typeof(T).IsInterface )
        {
            throw new ArgumentException( "The type argument must be an interface." );
        }
    }

    public virtual DeclarationRefTargetKind TargetKind => DeclarationRefTargetKind.Default;

    public abstract string Name { get; }

    public virtual IRefStrategy Strategy => throw new NotSupportedException();

    public IRefImpl Unwrap() => this;

    public virtual SerializableDeclarationId ToSerializableId()
    {
        if ( this.CompilationContext == null )
        {
            throw new InvalidOperationException( "This reference cannot be serialized because it has no compilation." );
        }

        var symbol = this.GetSymbolIgnoringKind( true );

        return symbol.GetSerializableId( this.TargetKind );
    }

    ICompilationElement IRef.GetTarget( ICompilation compilation, ReferenceResolutionOptions options, IGenericContext? genericContext )
        => this.GetTarget( compilation, options, genericContext );

    ICompilationElement? IRef.GetTargetOrNull( ICompilation compilation, ReferenceResolutionOptions options, IGenericContext? genericContext )
        => this.GetTargetOrNull( compilation, options, genericContext );

    IRef<TOut> IRef.As<TOut>() => this.As<TOut>();

    public T GetTarget( ICompilation compilation, ReferenceResolutionOptions options = default, IGenericContext? genericContext = default )
        => this.GetTargetImpl( compilation, options, true, genericContext )!;

    public T? GetTargetOrNull( ICompilation compilation, ReferenceResolutionOptions options = default, IGenericContext? genericContext = default )
        => this.GetTargetImpl( compilation, options, false, genericContext );

    private T? GetTargetImpl( ICompilation compilation, ReferenceResolutionOptions options, bool throwIfMissing, IGenericContext? genericContext )
    {
        Invariant.Assert( compilation.GetCompilationContext() == this.CompilationContext, "CompilationContext mismatch." );

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

    /// <summary>
    /// Gets all <see cref="AttributeData"/> on the target of the reference without resolving the reference to
    /// the code model.
    /// </summary>
    public (ImmutableArray<AttributeData> Attributes, ISymbol Symbol) GetAttributeData()
    {
        if ( this.TargetKind != DeclarationRefTargetKind.Default )
        {
            var baseSymbol = this.GetSymbolIgnoringKind();

            switch ( this.TargetKind )
            {
                case DeclarationRefTargetKind.Return when baseSymbol is IMethodSymbol method:
                    return (method.GetReturnTypeAttributes(), method);

                case DeclarationRefTargetKind.Field when baseSymbol is IEventSymbol @event:
                    // Roslyn does not expose the backing field of an event, so we don't have access to its attributes.
                    return (ImmutableArray<AttributeData>.Empty, @event);
            }

            // Fallback to the default GetSymbol implementation.
        }

        var symbol = this.GetSymbol( true );

        return (symbol.GetAttributes(), symbol);
    }

    public virtual ISymbol GetClosestSymbol()
    {
        return this.GetSymbolIgnoringKind();
    }

    ISymbol ISdkRef.GetSymbol( Compilation compilation, bool ignoreAssemblyKey ) => this.GetSymbol( ignoreAssemblyKey );

    private ISymbol GetSymbol( bool ignoreAssemblyKey = false ) => this.GetSymbolWithKind( this.GetSymbolIgnoringKind( ignoreAssemblyKey ) );

    protected abstract ISymbol GetSymbolIgnoringKind( bool ignoreAssemblyKey = false );

    private ISymbol GetSymbolWithKind( ISymbol symbol )
        => this.TargetKind switch
        {
            DeclarationRefTargetKind.Assembly when symbol is IAssemblySymbol => symbol,
            DeclarationRefTargetKind.Module when symbol is IModuleSymbol => symbol,
            DeclarationRefTargetKind.NamedType when symbol is INamedTypeSymbol => symbol,
            DeclarationRefTargetKind.Default => symbol,
            DeclarationRefTargetKind.Return => throw new InvalidOperationException( "Cannot get a symbol for the method return parameter." ),
            DeclarationRefTargetKind.Field when symbol is IPropertySymbol property => property.GetBackingField().AssertSymbolNotNull(),
            DeclarationRefTargetKind.Field when symbol is IEventSymbol => throw new InvalidOperationException( "Cannot get the underlying field of an event." ),
            DeclarationRefTargetKind.Parameter when symbol is IPropertySymbol property => property.SetMethod.AssertSymbolNotNull().Parameters[0],
            DeclarationRefTargetKind.Parameter when symbol is IMethodSymbol method => method.Parameters[0],
            DeclarationRefTargetKind.Property when symbol is IParameterSymbol parameter => parameter.ContainingType.GetMembers( symbol.Name )
                .OfType<IPropertySymbol>()
                .Single(),
            _ => throw new AssertionFailedException( $"Don't know how to get the symbol kind {this.TargetKind} for a {symbol.Kind}." )
        };

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

    public IRefImpl<TOut> As<TOut>()
        where TOut : class, ICompilationElement
        => this as IRefImpl<TOut> ?? new CastRef<TOut>( this );

    public override int GetHashCode() => this.GetHashCodeCore();

    public abstract bool Equals( IRef? other );

    protected abstract int GetHashCodeCore();
}