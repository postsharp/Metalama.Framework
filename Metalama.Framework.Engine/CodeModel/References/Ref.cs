// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
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
        /// <param name="symbol"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
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
        /// Creates a <see cref="Ref{T}"/> from a <see cref="DeclarationBuilder"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <typeparam name="TCodeElement"></typeparam>
        /// <typeparam name="TBuilder"></typeparam>
        /// <returns></returns>
        public static Ref<TCodeElement> FromBuilder<TCodeElement, TBuilder>( TBuilder builder )
            where TCodeElement : class, IDeclaration
            where TBuilder : IDeclarationBuilder
            => new( builder );

        /// <summary>
        /// Creates a <see cref="Ref{T}"/> from a <see cref="DeclarationBuilder"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static Ref<IDeclaration> FromBuilder( IDeclarationBuilder builder ) => new( builder );

        /// <summary>
        /// Creates a <see cref="Ref{T}"/> from a Roslyn symbol.
        /// </summary>
        public static Ref<IDeclaration> FromSymbol( ISymbol symbol, Compilation compilation ) => new( symbol, compilation );

        public static Ref<IDeclaration> PseudoAccessor( IMethod accessor )
        {
            Invariant.Assert( accessor.IsImplicitlyDeclared );

            if ( accessor.ContainingDeclaration is not IMemberWithAccessors declaringMember )
            {
                throw new AssertionFailedException( $"Unexpected containing declaration: '{accessor.ContainingDeclaration}'." );
            }

            return new Ref<IDeclaration>(
                declaringMember.GetSymbol().AssertNotNull(),
                declaringMember.GetCompilationModel().RoslynCompilation,
                accessor.MethodKind.ToDeclarationRefTargetKind() );
        }

        public static Ref<IDeclaration> PseudoParameter( IParameter pseudoParameter )
        {
            var accessor = (IMethod) pseudoParameter.DeclaringMember;

            Invariant.Assert( accessor.IsImplicitlyDeclared );

            if ( accessor.ContainingDeclaration is not IMemberWithAccessors declaringMember )
            {
                throw new AssertionFailedException( $"Unexpected containing declaration: '{accessor.ContainingDeclaration}'." );
            }

            return new Ref<IDeclaration>(
                declaringMember.GetSymbol().AssertNotNull(),
                declaringMember.GetCompilationModel().RoslynCompilation,
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

        public static Ref<T> FromSerializedId<T>( SerializableDeclarationId id )
            where T : class, ICompilationElement
            => new( id.Id );

        /// <summary>
        /// Creates a <see cref="Ref{T}"/> from a Roslyn symbol.
        /// </summary>
        public static Ref<T> FromSymbol<T>( ISymbol symbol, Compilation compilation, DeclarationRefTargetKind targetKind = DeclarationRefTargetKind.Default )
            where T : class, ICompilationElement
            => new( symbol, compilation, targetKind );

        public static Ref<IDeclaration> ReturnParameter( IMethodSymbol methodSymbol, Compilation compilation )
            => new( methodSymbol, compilation, DeclarationRefTargetKind.Return );

        internal static Ref<ICompilation> Compilation( Compilation compilation ) => FromSymbol( compilation.Assembly, compilation ).As<ICompilation>();
    }

    /// <summary>
    /// The base implementation of <see cref="ISdkRef{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Obfuscation( Exclude = true /* Serialized */ )]
    internal readonly struct Ref<T> : IRefImpl<T>, IEquatable<Ref<T>>
        where T : class, ICompilationElement
    {
        // The compilation for which the symbol (stored in Target) is valid.
        private readonly Compilation? _compilation;

        internal Ref( ISymbol symbol, Compilation compilation, DeclarationRefTargetKind targetKind = DeclarationRefTargetKind.Default )
        {
            symbol.AssertValidType<T>();

            this.TargetKind = targetKind;
            this._compilation = compilation;
            this.Target = symbol;
        }

        internal Ref( IDeclarationBuilder builder )
        {
            this.Target = builder;
            this.TargetKind = DeclarationRefTargetKind.Default;
            this._compilation = builder.GetCompilationModel().RoslynCompilation;
        }

        private Ref( object? target, Compilation? compilation, DeclarationRefTargetKind targetKind )
        {
            this.Target = target;
            this.TargetKind = targetKind;
            this._compilation = compilation;
        }

        internal Ref( SymbolId symbolKey )
        {
            this.Target = symbolKey.ToString();
            this.TargetKind = DeclarationRefTargetKind.Default;
            this._compilation = null;
        }

        internal Ref( string id )
        {
            this.Target = id;
            this.TargetKind = DeclarationRefTargetKind.Default;
            this._compilation = null;
        }

        // ReSharper disable once UnusedParameter.Local
        public Ref( SyntaxNode? declaration, DeclarationRefTargetKind targetKind, Compilation compilation )
        {
            this.Target = declaration;
            this.TargetKind = targetKind;
            this._compilation = compilation;
        }

        public object? Target { get; }

        public DeclarationRefTargetKind TargetKind { get; }

        public SerializableDeclarationId ToSerializableId()
        {
            if ( this._compilation == null )
            {
                throw new InvalidOperationException( "This reference cannot be serialized because it has no compilation." );
            }

            var symbol = this.GetSymbolIgnoringKind( this._compilation, true );

            return symbol.GetSerializableId( this.TargetKind );
        }

        private static bool IsSerializableId( string id ) => char.IsLetter( id[0] ) && id[1] == ':';

        public T GetTarget( ICompilation compilation, ReferenceResolutionOptions options = default )
        {
            var compilationModel = (CompilationModel) compilation;

            if ( options.FollowRedirections() && compilationModel.TryGetRedirectedDeclaration(
                    new Ref<IDeclaration>( this.Target, this._compilation, this.TargetKind ),
                    out var redirected ) )
            {
                // Referencing redirected declaration.
                return this.Resolve( redirected.Target, compilationModel, options, this.TargetKind );
            }
            else
            {
                return this.Resolve( this.Target, compilationModel, options, this.TargetKind );
            }
        }

        /// <summary>
        /// Gets all <see cref="AttributeData"/> on the target of the reference without resolving the reference to
        /// the code model.
        /// </summary>
        public (ImmutableArray<AttributeData> Attributes, ISymbol Symbol) GetAttributeData( Compilation compilation )
        {
            if ( this.TargetKind == DeclarationRefTargetKind.Return )
            {
                var method = (IMethodSymbol) this.GetSymbolIgnoringKind( compilation );

                return (method.GetReturnTypeAttributes(), method);
            }
            else if ( this.TargetKind == DeclarationRefTargetKind.Field )
            {
                var target = this.GetSymbolIgnoringKind( compilation );

                if ( target is IEventSymbol )
                {
                    // Roslyn does not expose the backing field of an event, so we don't have access to its attributes.
                    return (ImmutableArray<AttributeData>.Empty, target);
                }
            }

            var symbol = this.GetSymbol( compilation, true );

            return (symbol.GetAttributes(), symbol);
        }

        public bool IsDefault => this.Target == null && this.TargetKind == DeclarationRefTargetKind.Default;

        public ISymbol GetSymbol( Compilation compilation, bool ignoreAssemblyKey = false )
            => this.GetSymbolWithKind( this.GetSymbolIgnoringKind( compilation, ignoreAssemblyKey ) );

        private ISymbol GetSymbolIgnoringKind( Compilation compilation, bool ignoreAssemblyKey = false )
        {
            switch ( this.Target )
            {
                case null:
                    throw new AssertionFailedException( "The reference target is null." );

                case ISymbol symbol:
                    return symbol.Translate( this._compilation, compilation ).AssertNotNull();

                case string id:
                    {
                        ISymbol? symbol;

                        if ( IsSerializableId( id ) )
                        {
                            symbol = DocumentationCommentId.GetFirstSymbolForDeclarationId( id, compilation );
                        }
                        else
                        {
                            var symbolKey = new SymbolId( id );

                            symbol = symbolKey.Resolve( compilation, ignoreAssemblyKey );
                        }

                        if ( symbol == null )
                        {
                            throw new SymbolNotFoundException( id, compilation );
                        }

                        return symbol;
                    }

                case SyntaxNode node:
                    {
                        return GetSymbolOfNode( compilation, node );
                    }

                default:
                    throw new InvalidOperationException();
            }
        }

        private ISymbol GetSymbolWithKind( ISymbol symbol )
        {
            return this.TargetKind switch
            {
                DeclarationRefTargetKind.Assembly when symbol is IAssemblySymbol => symbol,
                DeclarationRefTargetKind.Module when symbol is IModuleSymbol => symbol,
                DeclarationRefTargetKind.Default => symbol,
                DeclarationRefTargetKind.Return => throw new InvalidOperationException( "Cannot get a symbol for the method return parameter." ),
                DeclarationRefTargetKind.Field when symbol is IPropertySymbol property => property.GetBackingField().AssertNotNull(),
                DeclarationRefTargetKind.Field when symbol is IEventSymbol => throw new InvalidOperationException(
                    "Cannot get the underlying field of an event." ),
                DeclarationRefTargetKind.Parameter when symbol is IPropertySymbol property => property.SetMethod.AssertNotNull().Parameters[0],
                DeclarationRefTargetKind.Parameter when symbol is IMethodSymbol method => method.Parameters[0],
                DeclarationRefTargetKind.Property when symbol is IParameterSymbol parameter => parameter.ContainingType.GetMembers( symbol.Name )
                    .OfType<IPropertySymbol>()
                    .Single(),
                _ => throw new AssertionFailedException( $"Don't know how to get the symbol kind {this.TargetKind} for a {symbol.Kind}." )
            };
        }

        private static ISymbol GetSymbolOfNode( Compilation compilation, SyntaxNode node )
        {
            var semanticModel = compilation.GetCachedSemanticModel( node.SyntaxTree );

            if ( semanticModel == null )
            {
                throw new AssertionFailedException( $"Cannot get a semantic model for '{node.SyntaxTree.FilePath}'." );
            }

            var symbol = semanticModel.GetDeclaredSymbol( node );

            if ( symbol == null )
            {
                throw new AssertionFailedException( $"Cannot get a symbol for {node.GetType().Name}." );
            }

            return symbol;
        }

        private T Resolve(
            object? reference,
            CompilationModel compilation,
            ReferenceResolutionOptions options = default,
            DeclarationRefTargetKind kind = DeclarationRefTargetKind.Default )
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
                    return kind is DeclarationRefTargetKind.Assembly or DeclarationRefTargetKind.Module
                        ? (T) (object) compilation
                        : throw new AssertionFailedException( "The reference target is null but the kind is not assembly or module." );

                case ISymbol symbol:
                    return Convert(
                        compilation.Factory.GetCompilationElement(
                                symbol.AssertValidType<T>().Translate( this._compilation, compilation.RoslynCompilation ).AssertNotNull(),
                                kind )
                            .AssertNotNull() );

                case SyntaxNode node:
                    return Convert(
                        compilation.Factory.GetCompilationElement(
                                GetSymbolOfNode( compilation.PartialCompilation.Compilation, node ).AssertValidType<T>(),
                                kind )
                            .AssertNotNull() );

                case IDeclarationBuilder builder:
                    return Convert( compilation.Factory.GetDeclaration( builder, options ) );

                case string id:
                    {
                        ISymbol? symbol;

                        if ( IsSerializableId( id ) )
                        {
                            symbol = DocumentationCommentId.GetFirstSymbolForDeclarationId( id, compilation.RoslynCompilation );
                        }
                        else
                        {
                            symbol = new SymbolId( id ).Resolve( compilation.RoslynCompilation );
                        }

                        if ( symbol == null )
                        {
                            throw new SymbolNotFoundException( id, compilation.RoslynCompilation );
                        }

                        return Convert( compilation.Factory.GetCompilationElement( symbol ).AssertNotNull() );
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
                ISymbol symbol => symbol.ToDisplayString( SymbolDisplayFormat.CSharpShortErrorMessageFormat ),
                _ => this.Target.ToString() ?? "null"
            };

            if ( this.TargetKind != DeclarationRefTargetKind.Default )
            {
                value += $" ({this.TargetKind})";
            }

            return value;
        }

        public Ref<TOut> As<TOut>()
            where TOut : class, ICompilationElement
            => new( this.Target, this._compilation, this.TargetKind );

        public override int GetHashCode() => RefEqualityComparer<T>.Default.GetHashCode( this );

        public bool Equals( Ref<T> other ) => RefEqualityComparer<T>.Default.Equals( this, other );
    }
}