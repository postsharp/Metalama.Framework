// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Reflection;

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
                symbol.GetDeclarationKind() == DeclarationKind.Method );

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

        public static Ref<T> FromSymbolId<T>( SymbolId symbolKey )
            where T : class, ICompilationElement
            => new( symbolKey );

        public static Ref<T> FromSerializedId<T>( string id )
            where T : class, ICompilationElement
            => new( id );

        /// <summary>
        /// Creates a <see cref="Ref{T}"/> from a Roslyn symbol.
        /// </summary>
        public static Ref<T> FromSymbol<T>( ISymbol symbol, Compilation compilation )
            where T : class, ICompilationElement
            => new( symbol, compilation );

        public static Ref<IDeclaration> ReturnParameter( IMethodSymbol methodSymbol, Compilation compilation )
            => new( methodSymbol, compilation, DeclarationRefTargetKind.Return );

        internal static Ref<ICompilation> Compilation( Compilation compilation ) => new( DeclarationRefTargetKind.Assembly, compilation );
    }

    /// <summary>
    /// The base implementation of <see cref="ISdkRef{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Obfuscation( Exclude = true /* Serialized */ )]
    internal readonly struct Ref<T> : IRefImpl<T>
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

        internal Ref( DeclarationRefTargetKind targetKind, Compilation compilation )
        {
            this._compilation = compilation;
            this.TargetKind = targetKind;
            this.Target = null;
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

        public string? ToSerializableId()
        {
            if ( this._compilation == null )
            {
                throw new InvalidOperationException( "This reference cannot be serialized because it has no compilation." );
            }

            var symbol = this.GetSymbol( this._compilation );

            return DocumentationCommentId.CreateDeclarationId( symbol );
        }

        private static bool IsSerializableId( string id ) => char.IsLetter( id[0] ) && id[1] == ':';

        public static ISymbol? Deserialize( Compilation compilation, string serializedId )
            => DocumentationCommentId.GetFirstSymbolForDeclarationId( serializedId, compilation );

        public T GetTarget( ICompilation compilation ) => Resolve( this.Target, (CompilationModel) compilation, this.TargetKind );

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

            var symbol = this.GetSymbol( compilation );

            return (symbol.GetAttributes(), symbol);
        }

        public bool IsDefault => this.Target == null && this.TargetKind == DeclarationRefTargetKind.Default;

        public ISymbol GetSymbol( Compilation compilation ) => this.GetSymbolWithKind( this.GetSymbolIgnoringKind( compilation ) );

        private ISymbol GetSymbolIgnoringKind( Compilation compilation, bool ignoreAssemblyKey = false )
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

        internal static T Resolve( object? reference, ICompilation compilation, DeclarationRefTargetKind kind = DeclarationRefTargetKind.Default )
            => Resolve( reference, (CompilationModel) compilation, kind );

        private static T Resolve( object? reference, CompilationModel compilation, DeclarationRefTargetKind kind = DeclarationRefTargetKind.Default )
        {
            switch ( reference )
            {
                case null:
                    return kind is DeclarationRefTargetKind.Assembly or DeclarationRefTargetKind.Module
                        ? (T) (object) compilation
                        : throw new AssertionFailedException();

                case ISymbol symbol:
                    return (T) compilation.Factory.GetCompilationElement( symbol.AssertValidType<T>(), kind ).AssertNotNull();

                case SyntaxNode node:
                    return (T) compilation.Factory.GetCompilationElement(
                            GetSymbolOfNode( compilation.PartialCompilation.Compilation, node ).AssertValidType<T>(),
                            kind )
                        .AssertNotNull();

                case IDeclarationBuilder builder:
                    return (T) compilation.Factory.GetDeclaration( builder );

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

                        return (T) compilation.Factory.GetCompilationElement( symbol ).AssertNotNull();
                    }

                default:
                    throw new AssertionFailedException();
            }
        }

        public override string ToString() => this.Target?.ToString() ?? "null";

        public Ref<TOut> As<TOut>()
            where TOut : class, ICompilationElement
            => new( this.Target, this._compilation, this.TargetKind );

        public override int GetHashCode() => this.Target?.GetHashCode() ?? 0;
    }
}