// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.CodeModel.Substituted;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Linq;
using MethodKind = Metalama.Framework.Code.MethodKind;

namespace Metalama.Framework.Engine.CodeModel.References
{
    /// <summary>
    /// Contains factory methods for the generic <see cref="Ref{T}"/>.
    /// </summary>
    internal static class Ref
    {
        /// <summary>
        /// Asserts that a given symbol is compatible with a given <see cref="IDeclaration"/> interface.
        /// </summary>
        public static ISymbol AssertValidType<T>( this ISymbol symbol )
            where T : ICompilationElement
        {
            Invariant.Implies(
                typeof(T) == typeof(IConstructor),
                symbol.GetDeclarationKind() == DeclarationKind.Constructor );

            Invariant.Implies(
                typeof(T) == typeof(IMethod),
                symbol.GetDeclarationKind() is DeclarationKind.Method or DeclarationKind.Finalizer );

            return symbol;
        }

        /// <summary>
        /// Creates a <see cref="Ref{T}"/> from an <see cref="IDeclarationBuilder"/>.
        /// </summary>
        public static Ref<TCodeElement> FromBuilder<TCodeElement>( IDeclarationBuilder builder )
            where TCodeElement : class, IDeclaration
            => new( builder );

        /// <summary>
        /// Creates a <see cref="Ref{T}"/> from an <see cref="IDeclarationBuilder"/>.
        /// </summary>
        public static Ref<IDeclaration> FromBuilder( IDeclarationBuilder builder ) => new( builder );

        /// <summary>
        /// Creates a <see cref="Ref{T}"/> from an <see cref="ISubstitutedDeclaration"/>.
        /// </summary>
        public static Ref<IDeclaration> FromSubstitutedDeclaration( ISubstitutedDeclaration declaration ) => new( declaration );

        /// <summary>
        /// Creates a <see cref="Ref{T}"/> from a Roslyn symbol.
        /// </summary>
        public static Ref<IDeclaration> FromSymbol( ISymbol symbol, CompilationContext compilationContext ) => new( symbol, compilationContext );

        public static Ref<IDeclaration> PseudoAccessor( IMethod accessor )
        {
            Invariant.Assert( accessor.IsImplicitlyDeclared );

            if ( accessor.ContainingDeclaration is not IHasAccessors declaringMember )
            {
                throw new AssertionFailedException( $"Unexpected containing declaration: '{accessor.ContainingDeclaration}'." );
            }

            return new Ref<IDeclaration>(
                declaringMember.GetSymbol().AssertNotNull(),
                declaringMember.GetCompilationModel().CompilationContext,
                accessor.MethodKind.ToDeclarationRefTargetKind() );
        }

        public static Ref<IDeclaration> PseudoParameter( IParameter pseudoParameter )
        {
            var accessor = (IMethod) pseudoParameter.DeclaringMember;

            Invariant.Assert( accessor.IsImplicitlyDeclared );

            if ( accessor.ContainingDeclaration is not IHasAccessors declaringMember )
            {
                throw new AssertionFailedException( $"Unexpected containing declaration: '{accessor.ContainingDeclaration}'." );
            }

            return new Ref<IDeclaration>(
                declaringMember.GetSymbol().AssertNotNull(),
                declaringMember.GetCompilationModel().CompilationContext,
                accessor.MethodKind switch
                {
                    MethodKind.PropertySet when pseudoParameter.IsReturnParameter => DeclarationRefTargetKind.PropertySetReturnParameter,
                    MethodKind.PropertySet => DeclarationRefTargetKind.PropertySetParameter,
                    MethodKind.PropertyGet => DeclarationRefTargetKind.PropertyGetReturnParameter,
                    MethodKind.EventRaise when pseudoParameter.IsReturnParameter => DeclarationRefTargetKind.EventRaiseReturnParameter,
                    MethodKind.EventRaise => throw new NotImplementedException(
                        $"Getting the reference of a pseudo event raiser parameter is not implemented." ),
                    _ => throw new AssertionFailedException( $"Unexpected MethodKind: {accessor.MethodKind}." )
                } );
        }

        public static Ref<T> FromSymbolId<T>( SymbolId symbolKey )
            where T : class, ICompilationElement
            => new( symbolKey );

        public static Ref<T> FromDeclarationId<T>( SerializableDeclarationId id )
            where T : class, ICompilationElement
            => new( id.Id );

        public static Ref<T> FromTypeId<T>( SerializableTypeId id )
            where T : class, IType
            => new( id.Id );

        /// <summary>
        /// Creates a <see cref="Ref{T}"/> from a Roslyn symbol.
        /// </summary>
        public static Ref<T> FromSymbol<T>(
            ISymbol symbol,
            CompilationContext compilationContext,
            DeclarationRefTargetKind targetKind = DeclarationRefTargetKind.Default )
            where T : class, ICompilationElement
            => new( symbol, compilationContext, targetKind );

        public static Ref<IDeclaration> ReturnParameter( IMethodSymbol methodSymbol, CompilationContext compilationContext )
            => new( methodSymbol, compilationContext, DeclarationRefTargetKind.Return );

        internal static Ref<ICompilation> Compilation( CompilationContext compilationContext )
            => FromSymbol( compilationContext.Compilation.Assembly, compilationContext ).As<ICompilation>();
    }

    /// <summary>
    /// The base implementation of <see cref="ISdkRef{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct Ref<T> : IRefImpl<T>, IEquatable<Ref<T>>
        where T : class, ICompilationElement
    {
        // The compilation for which the symbol (stored in Target) is valid.
        private readonly CompilationContext? _compilationContext;

        internal Ref( ISymbol symbol, CompilationContext compilationContext, DeclarationRefTargetKind targetKind = DeclarationRefTargetKind.Default )
        {
            symbol.AssertValidType<T>();

            this.TargetKind = targetKind;
            this._compilationContext = compilationContext;
            this.Target = symbol;
        }

        internal Ref( IDeclarationBuilder builder )
        {
            this.Target = builder;
            this.TargetKind = DeclarationRefTargetKind.Default;
            this._compilationContext = builder.GetCompilationModel().CompilationContext;
        }

        internal Ref( ISubstitutedDeclaration declaration )
        {
            this.Target = declaration;
            this.TargetKind = DeclarationRefTargetKind.Default;
            this._compilationContext = declaration.GetCompilationModel().CompilationContext;
        }

        private Ref( object? target, CompilationContext? compilationContext, DeclarationRefTargetKind targetKind )
        {
            this.Target = target;
            this.TargetKind = targetKind;
            this._compilationContext = compilationContext;
        }

        internal Ref( SymbolId symbolKey )
        {
            this.Target = symbolKey.ToString();
            this.TargetKind = DeclarationRefTargetKind.Default;
            this._compilationContext = null;
        }

        internal Ref( string id )
        {
            this.Target = id;
            this.TargetKind = DeclarationRefTargetKind.Default;
            this._compilationContext = null;
        }

        internal Ref( SyntaxNode? declaration, DeclarationRefTargetKind targetKind, CompilationContext compilationContext )
        {
            this.Target = declaration;
            this.TargetKind = targetKind;
            this._compilationContext = compilationContext;
        }

        public object? Target { get; }

        internal DeclarationRefTargetKind TargetKind { get; }

        public SerializableDeclarationId ToSerializableId()
        {
            if ( this.Target is IDeclaration declaration )
            {
                return declaration.GetSerializableId( this.TargetKind );
            }

            if ( this.Target is string id && IsDeclarationId( id ) && this.TargetKind == DeclarationRefTargetKind.Default )
            {
                return new SerializableDeclarationId( id );
            }

            if ( this._compilationContext == null )
            {
                throw new InvalidOperationException( "This reference cannot be serialized because it has no compilation." );
            }

            var symbol = this.GetSymbolIgnoringKind( this._compilationContext, true );

            return symbol.GetSerializableId( this.TargetKind );
        }

        private static bool IsDeclarationId( string id ) => char.IsLetter( id[0] ) && id[1] == ':' && id[0] != SerializableTypeIdResolver.Prefix[0];

        private static bool IsTypeId( string id )
            => id.StartsWith( SerializableTypeIdResolver.LegacyPrefix, StringComparison.Ordinal )
               || id.StartsWith( SerializableTypeIdResolver.Prefix, StringComparison.Ordinal );

        public T GetTarget( ICompilation compilation, ReferenceResolutionOptions options = default ) => this.GetTargetImpl( compilation, options, true )!;

        public T? GetTargetOrNull( ICompilation compilation, ReferenceResolutionOptions options = default )
            => this.GetTargetImpl( compilation, options, false );

        private T? GetTargetImpl( ICompilation compilation, ReferenceResolutionOptions options, bool throwIfMissing )
        {
            var compilationModel = (CompilationModel) compilation;

            if ( options.FollowRedirections() && compilationModel.TryGetRedirectedDeclaration(
                    new Ref<IDeclaration>( this.Target, this._compilationContext, this.TargetKind ),
                    out var redirected ) )
            {
                // Referencing redirected declaration.
                return this.Resolve( redirected.Target, compilationModel, options, throwIfMissing );
            }
            else
            {
                return this.Resolve( this.Target, compilationModel, options, throwIfMissing );
            }
        }

        /// <summary>
        /// Gets all <see cref="AttributeData"/> on the target of the reference without resolving the reference to
        /// the code model.
        /// </summary>
        internal (ImmutableArray<AttributeData> Attributes, ISymbol Symbol) GetAttributeData( CompilationContext compilationContext )
        {
            if ( this.TargetKind == DeclarationRefTargetKind.Return )
            {
                var method = (IMethodSymbol) this.GetSymbolIgnoringKind( compilationContext );

                return (method.GetReturnTypeAttributes(), method);
            }
            else if ( this.TargetKind == DeclarationRefTargetKind.Field )
            {
                var target = this.GetSymbolIgnoringKind( compilationContext );

                if ( target is IEventSymbol )
                {
                    // Roslyn does not expose the backing field of an event, so we don't have access to its attributes.
                    return (ImmutableArray<AttributeData>.Empty, target);
                }
            }

            var symbol = this.GetSymbol( compilationContext, true );

            return (symbol.GetAttributes(), symbol);
        }

        public bool IsDefault => this.Target == null && this.TargetKind == DeclarationRefTargetKind.Default;

        public ISymbol GetClosestSymbol( CompilationContext compilationContext )
        {
            if ( this.Target is IDeclarationBuilder builder )
            {
                var containingDeclaration = builder.ContainingDeclaration;

                // This can happen for accessor method of a builder member.
                if ( containingDeclaration is IDeclarationBuilder containingBuilder )
                {
                    containingDeclaration = containingBuilder.ContainingDeclaration;
                }

                return containingDeclaration.AssertNotNull().GetSymbol( compilationContext ).AssertNotNull();
            }

            return this.GetSymbolIgnoringKind( compilationContext );
        }

        ISymbol ISdkRef<T>.GetSymbol( Compilation compilation, bool ignoreAssemblyKey )
            => this.GetSymbol( CompilationContextFactory.GetInstance( compilation ), ignoreAssemblyKey );

        private ISymbol GetSymbol( CompilationContext compilationContext, bool ignoreAssemblyKey = false )
            => this.GetSymbolWithKind( this.GetSymbolIgnoringKind( compilationContext, ignoreAssemblyKey ) );

        private ISymbol GetSymbolIgnoringKind( CompilationContext compilationContext, bool ignoreAssemblyKey = false )
        {
            switch ( this.Target )
            {
                case null:
                    throw new AssertionFailedException( "The reference target is null." );

                case ISymbol symbol:
                    return compilationContext.SymbolTranslator.Translate( symbol ).AssertNotNull();

                case string id:
                    {
                        ISymbol? symbol;

                        if ( IsDeclarationId( id ) )
                        {
                            symbol = new SerializableDeclarationId( id ).ResolveToSymbolOrNull( compilationContext.Compilation );
                        }
                        else if ( IsTypeId( id ) )
                        {
                            symbol = compilationContext.SerializableTypeIdResolver.ResolveId( new SerializableTypeId( id ) );
                        }
                        else
                        {
                            var symbolKey = new SymbolId( id );

                            symbol = symbolKey.Resolve( compilationContext.Compilation, ignoreAssemblyKey );
                        }

                        if ( symbol == null )
                        {
                            throw new SymbolNotFoundException( id, compilationContext.Compilation );
                        }

                        return symbol;
                    }

                case SyntaxNode node:
                    {
                        return GetSymbolOfNode( compilationContext, node );
                    }

                default:
                    throw new InvalidOperationException( $"Can't get symbol for ref with target {this.Target}." );
            }
        }

        private ISymbol GetSymbolWithKind( ISymbol symbol )
        {
            return this.TargetKind switch
            {
                DeclarationRefTargetKind.Assembly when symbol is IAssemblySymbol => symbol,
                DeclarationRefTargetKind.Module when symbol is IModuleSymbol => symbol,
                DeclarationRefTargetKind.NamedType when symbol is INamedTypeSymbol => symbol,
                DeclarationRefTargetKind.Default => symbol,
                DeclarationRefTargetKind.Return => throw new InvalidOperationException( "Cannot get a symbol for the method return parameter." ),
                DeclarationRefTargetKind.Field when symbol is IPropertySymbol property => property.GetBackingField().AssertSymbolNotNull( false ),
                DeclarationRefTargetKind.Field when symbol is IEventSymbol => throw new InvalidOperationException(
                    "Cannot get the underlying field of an event." ),
                DeclarationRefTargetKind.Parameter when symbol is IPropertySymbol property => property.SetMethod.AssertSymbolNotNull( false ).Parameters[0],
                DeclarationRefTargetKind.Parameter when symbol is IMethodSymbol method => method.Parameters[0],
                DeclarationRefTargetKind.Property when symbol is IParameterSymbol parameter => parameter.ContainingType.GetMembers( symbol.Name )
                    .OfType<IPropertySymbol>()
                    .Single(),
                _ => throw new AssertionFailedException( $"Don't know how to get the symbol kind {this.TargetKind} for a {symbol.Kind}." )
            };
        }

        private static ISymbol GetSymbolOfNode( CompilationContext compilationContext, SyntaxNode node )
        {
            var semanticModel =
                compilationContext.SemanticModelProvider.GetSemanticModel( node.SyntaxTree )
                ?? throw new AssertionFailedException( $"Cannot get a semantic model for '{node.SyntaxTree.FilePath}'." );

            var symbol =
                (node is LambdaExpressionSyntax ? semanticModel.GetSymbolInfo( node ).Symbol : semanticModel.GetDeclaredSymbol( node ))
                ?? throw new AssertionFailedException( $"Cannot get a symbol for {node.GetType().Name}." );

            return symbol;
        }

        private T? Resolve(
            object? reference,
            CompilationModel compilation,
            ReferenceResolutionOptions options,
            bool throwIfMissing )
        {
            T Convert( ICompilationElement compilationElement )
            {
                if ( compilationElement is not T safeCast )
                {
                    // Throw an exception with a better exception message for better troubleshooting.
                    throw new InvalidOperationException(
                        $"Cannot convert '{compilationElement}' into a {typeof(T).Name} within the compilation '{compilation.Identity}'." );
                }

                return safeCast;
            }

            switch ( reference )
            {
                case null:
                    return this.TargetKind is DeclarationRefTargetKind.Assembly or DeclarationRefTargetKind.Module
                        ? (T) (object) compilation
                        : throw new AssertionFailedException( "The reference target is null but the kind is not assembly or module." );

                case ISymbol symbol:
                    return Convert(
                        compilation.Factory.GetCompilationElement(
                                compilation.CompilationContext.SymbolTranslator.Translate( symbol, this._compilationContext?.Compilation ).AssertNotNull(),
                                this.TargetKind )
                            .AssertNotNull() );

                case SyntaxNode node:
                    return Convert(
                        compilation.Factory.GetCompilationElement(
                                GetSymbolOfNode( compilation.PartialCompilation.CompilationContext, node ).AssertValidType<T>(),
                                this.TargetKind )
                            .AssertNotNull() );

                case IDeclarationBuilder builder:
                    return Convert( compilation.Factory.GetDeclaration( builder, options ) );

                case ISubstitutedDeclaration substitutedDeclaration:
                    return Convert( compilation.Factory.GetDeclaration( substitutedDeclaration ) );

                case string id:
                    {
                        if ( IsDeclarationId( id ) )
                        {
                            var declaration = new SerializableDeclarationId( id ).ResolveToDeclaration( compilation )
                                              ?? (throwIfMissing ? throw new SymbolNotFoundException( id, compilation.RoslynCompilation ) : null);

                            if ( declaration == null )
                            {
                                return null;
                            }

                            return Convert( declaration );
                        }
                        else if ( IsTypeId( id ) )
                        {
                            try
                            {
                                var type = new SerializableTypeId( id ).Resolve( compilation );

                                return Convert( type );
                            }
                            catch ( InvalidOperationException ex )
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
                        }
                        else
                        {
                            var symbol = new SymbolId( id ).Resolve( compilation.RoslynCompilation )
                                         ?? (throwIfMissing ? throw new SymbolNotFoundException( id, compilation.RoslynCompilation ) : null);

                            if ( symbol == null )
                            {
                                return null;
                            }

                            return Convert( compilation.Factory.GetCompilationElement( symbol ).AssertNotNull() );
                        }
                    }

                default:
                    throw new AssertionFailedException( $"Unexpected type: {reference.GetType()}." );
            }
        }

        public override string ToString()
        {
            var value = this.Target switch
            {
                null => "null",
                ISymbol symbol => MetalamaStringFormatter.Instance.Format( null, symbol, null ),
                _ => this.Target.ToString() ?? "null"
            };

            if ( this.TargetKind != DeclarationRefTargetKind.Default )
            {
                value += $" ({this.TargetKind})";
            }

            return value;
        }

        internal Ref<TOut> As<TOut>()
            where TOut : class, ICompilationElement
            => new( this.Target, this._compilationContext, this.TargetKind );

        public override int GetHashCode() => RefEqualityComparer<T>.Default.GetHashCode( this );

        public bool Equals( Ref<T> other ) => RefEqualityComparer<T>.Default.Equals( this, other );
    }
}