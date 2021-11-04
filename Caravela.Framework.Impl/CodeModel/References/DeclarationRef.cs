// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.DeclarationBuilders;
using Caravela.Framework.Impl.CodeModel.Builders;
using Caravela.Framework.Impl.Linking;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.CodeModel.References
{
    /// <summary>
    /// Contains factory methods for the generic <see cref="DeclarationRef{T}"/>.
    /// </summary>
    internal static class DeclarationRef
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
                symbol.GetDeclarationKind() == DeclarationKind.Method );

            return symbol;
        }

        /// <summary>
        /// Creates a <see cref="DeclarationRef{T}"/> from a <see cref="DeclarationBuilder"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <typeparam name="TCodeElement"></typeparam>
        /// <typeparam name="TBuilder"></typeparam>
        /// <returns></returns>
        public static DeclarationRef<TCodeElement> FromBuilder<TCodeElement, TBuilder>( TBuilder builder )
            where TCodeElement : class, IDeclaration
            where TBuilder : IDeclarationBuilder
            => new( builder );

        /// <summary>
        /// Creates a <see cref="DeclarationRef{T}"/> from a <see cref="DeclarationBuilder"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static DeclarationRef<IDeclaration> FromBuilder( IDeclarationBuilder builder ) => new( builder );

        /// <summary>
        /// Creates a <see cref="DeclarationRef{T}"/> from a Roslyn symbol.
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public static DeclarationRef<IDeclaration> FromSymbol( ISymbol symbol ) => new( symbol );

        public static DeclarationRef<T> FromDocumentationId<T>( string documentationId )
            where T : class, ICompilationElement
            => new( documentationId );

        /// <summary>
        /// Creates a <see cref="DeclarationRef{T}"/> from a Roslyn symbol.
        /// </summary>
        /// <param name="symbol"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static DeclarationRef<T> FromSymbol<T>( ISymbol symbol )
            where T : class, ICompilationElement
            => new( symbol );

        public static DeclarationRef<IDeclaration> ReturnParameter( IMethodSymbol methodSymbol ) => new( methodSymbol, DeclarationRefTargetKind.Return );

        internal static DeclarationRef<IDeclaration> Assembly() => new( null, DeclarationRefTargetKind.Assembly );
    }

    /// <summary>
    /// The base implementation of <see cref="IDeclarationRef{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal readonly struct DeclarationRef<T> : IDeclarationRef<T>
        where T : class, ICompilationElement
    {
        internal DeclarationRef( ISymbol? symbol, DeclarationRefTargetKind targetKind = DeclarationRefTargetKind.Default )
        {
            this.TargetKind = targetKind;

            if ( symbol != null )
            {
                symbol.AssertValidType<T>();
            }

            this.Target = symbol;
        }

        internal DeclarationRef( IDeclarationBuilder builder )
        {
            this.Target = builder;
            this.TargetKind = DeclarationRefTargetKind.Default;
        }

        private DeclarationRef( object? target, DeclarationRefTargetKind targetKind )
        {
            this.Target = target;
            this.TargetKind = targetKind;
        }

        internal DeclarationRef( string documentationId )
        {
            this.Target = documentationId;
            this.TargetKind = DeclarationRefTargetKind.Default;
        }

        // ReSharper disable once UnusedParameter.Local
        public DeclarationRef( SyntaxNode? declaration, DeclarationRefTargetKind targetKind, Compilation compilation )
        {
#if DEBUG
            if ( declaration != null )
            {
                // Check that we have received a node that we can resolve to a symbol
                var semanticModel = compilation.GetSemanticModel( declaration.SyntaxTree );
                semanticModel.GetDeclaredSymbol( declaration ).AssertNotNull();
            }
#endif

            this.Target = declaration;
            this.TargetKind = targetKind;
        }

        public object? Target { get; }

        public DeclarationRefTargetKind TargetKind { get; }

        public T Resolve( CompilationModel compilation ) => Resolve( this.Target, compilation, this.TargetKind );

        public ( ImmutableArray<AttributeData> Attributes, ISymbol Symbol ) GetAttributeData( Compilation compilation )
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

            var symbol = this.GetSymbol( compilation );

            return (symbol.GetAttributes(), symbol);
        }

        public ISymbol GetSymbol( Compilation compilation ) => this.GetSymbolWithKind( this.GetSymbolIgnoringKind( compilation ) );

        private ISymbol GetSymbolIgnoringKind( Compilation compilation )
        {
            switch ( this.Target )
            {
                case null:
                    switch ( this.TargetKind )
                    {
                        case DeclarationRefTargetKind.Assembly:
                            return compilation.Assembly;

                        case DeclarationRefTargetKind.Module:
                            return compilation.SourceModule;

                        default:
                            throw new AssertionFailedException();
                    }

                case ISymbol symbol:
                    return symbol;

                case string documentationId:
                    {
                        var symbol = DocumentationCommentId.GetFirstSymbolForReferenceId( documentationId, compilation );

                        if ( symbol == null )
                        {
                            throw new AssertionFailedException( $"Cannot resolve {documentationId} into a symbol." );
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
            switch ( this.TargetKind )
            {
                case DeclarationRefTargetKind.Assembly when symbol is IAssemblySymbol:
                case DeclarationRefTargetKind.Module when symbol is IModuleSymbol:
                case DeclarationRefTargetKind.Default:
                    return symbol;

                case DeclarationRefTargetKind.Return:
                    throw new InvalidOperationException( "Cannot get a symbol for the method return parameter." );

                case DeclarationRefTargetKind.Field when symbol is IPropertySymbol property:
                    return property.GetBackingField().AssertNotNull();

                case DeclarationRefTargetKind.Field when symbol is IEventSymbol:
                    throw new InvalidOperationException( "Cannot get the underlying field of an event." );

                case DeclarationRefTargetKind.Parameter when symbol is IPropertySymbol property:
                    return property.SetMethod.AssertNotNull().Parameters[0];

                case DeclarationRefTargetKind.Parameter when symbol is IMethodSymbol method:
                    return method.Parameters[0];

                default:
                    throw new AssertionFailedException( $"Don't know how to get the symbol kind {this.TargetKind} for a {symbol.Kind}." );
            }
        }

        private static ISymbol GetSymbolOfNode( Compilation compilation, SyntaxNode node )
        {
            var semanticModel = compilation.GetSemanticModel( node.SyntaxTree );

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

        internal static T Resolve( object? reference, CompilationModel compilation, DeclarationRefTargetKind kind = DeclarationRefTargetKind.Default )
        {
            switch ( reference )
            {
                case null:
                    return kind is DeclarationRefTargetKind.Assembly or DeclarationRefTargetKind.Module
                        ? (T) (object) compilation
                        : throw new AssertionFailedException();

                case ISymbol symbol:
                    return (T) compilation.Factory.GetDeclaration( symbol.AssertValidType<T>(), kind );

                case SyntaxNode node:
                    return (T) compilation.Factory.GetDeclaration(
                        GetSymbolOfNode( compilation.PartialCompilation.Compilation, node ).AssertValidType<T>(),
                        kind );

                case IDeclarationBuilder builder:
                    return (T) compilation.Factory.GetDeclaration( builder );

                case string documentationId:
                    {
                        var symbol = DocumentationCommentId.GetFirstSymbolForDeclarationId( documentationId, compilation.RoslynCompilation );

                        if ( symbol == null )
                        {
                            throw new AssertionFailedException( $"Cannot resolve {documentationId} into a symbol." );
                        }

                        return (T) compilation.Factory.GetDeclaration( symbol );
                    }

                default:
                    throw new AssertionFailedException();
            }
        }

        public override string ToString() => this.Target?.ToString() ?? "null";

        public DeclarationRef<TOut> Cast<TOut>()
            where TOut : class, ICompilationElement
            => new( this.Target, this.TargetKind );

        public override int GetHashCode() => this.Target?.GetHashCode() ?? 0;
    }
}