// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Sdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.CompileTime
{
    /// <summary>
    /// Rewrites a run-time syntax tree so that the implementation of compile-time-only methods is replaced
    /// by a <c>throw new NotSupportedException()</c>.
    /// </summary>
    internal class RunTimeAssemblyRewriter : CompileTimeBaseRewriter
    {
        private const string _intrinsics = @"
using System;

namespace Caravela.Compiler
{
    internal static class Intrinsics
    {
        public static RuntimeMethodHandle GetRuntimeMethodHandle(string documentationId) => throw new InvalidOperationException(""Code calling this method has to be compiled by the Caravela compiler."");
        public static RuntimeFieldHandle GetRuntimeFieldHandle(string documentationId) => throw new InvalidOperationException(""Code calling this method has to be compiled by the Caravela compiler."");
        public static RuntimeTypeHandle GetRuntimeTypeHandle(string documentationId) => throw new InvalidOperationException(""Code calling this method has to be compiled by the Caravela compiler."");
    }
}
";

        // TODO: We can do more in cleaning the run-time assembly. 
        // Private compile-time code can be stripped, except when they are templates, because their metadata must be preserved.
        // In general, accessible compile-time metadata must remain.

        private static readonly Lazy<SyntaxTree> _intrinsicsSyntaxTree =
            new( () => CSharpSyntaxTree.ParseText( _intrinsics, CSharpParseOptions.Default ) );

        private readonly INamedTypeSymbol? _aspectDriverSymbol;

        private RunTimeAssemblyRewriter( Compilation runTimeCompilation, IServiceProvider serviceProvider )
            : base( runTimeCompilation, serviceProvider )
        {
            this._aspectDriverSymbol = runTimeCompilation.GetTypeByMetadataName( typeof(IAspectDriver).FullName );
        }

        public static IPartialCompilation Rewrite( IPartialCompilation compilation, IServiceProvider serviceProvider )
        {
            var rewriter = new RunTimeAssemblyRewriter( compilation.Compilation, serviceProvider );

            var transformedCompilation = compilation.RewriteSyntaxTrees( rewriter );

            if ( transformedCompilation.Compilation.GetTypeByMetadataName( "Caravela.Compiler.Intrinsics" ) == null )
            {
                transformedCompilation = transformedCompilation.WithSyntaxTreeModifications( null, new[] { _intrinsicsSyntaxTree.Value } );
            }

            return transformedCompilation;
        }

        public override SyntaxNode? VisitClassDeclaration( ClassDeclarationSyntax node )
        {
            var symbol = this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree ).GetDeclaredSymbol( node )!;

            // Special case: aspect weavers and other aspect drivers are preserved in the runtime assembly.
            // This only happens if regular Caravela.Framework is referenced from the weaver project, which generally shouldn't happen.
            // But it is a pattern used by Caravela.Samples for try.postsharp.net.
            if ( this._aspectDriverSymbol != null && symbol.AllInterfaces.Any( i => SymbolEqualityComparer.Default.Equals( i, this._aspectDriverSymbol ) ) )
            {
                return node;
            }

            // In classes that contain compile-time-only code, we should disable a few warnings:
            // - warning CS0067: X is never used (because method bodies have been replaced by 'throw')

            var leadingTrivia = node.GetLeadingTrivia();
            var trailingTrivia = node.GetTrailingTrivia();

            if ( symbol.GetMembers().Any( this.MustReplaceByThrow ) )
            {
                var errorCodes = SingletonSeparatedList<ExpressionSyntax>( IdentifierName( "CS0067" ) );

                leadingTrivia = leadingTrivia.Insert(
                    0,
                    Trivia(
                        PragmaWarningDirectiveTrivia(
                                Token( SyntaxKind.DisableKeyword ),
                                true )
                            .WithErrorCodes( errorCodes )
                            .NormalizeWhitespace() ) );

                trailingTrivia = trailingTrivia.Add(
                    Trivia(
                        PragmaWarningDirectiveTrivia(
                                Token( SyntaxKind.RestoreKeyword ),
                                true )
                            .WithErrorCodes( errorCodes )
                            .NormalizeWhitespace() ) );
            }

            return base.VisitClassDeclaration( node )!
                .WithLeadingTrivia( leadingTrivia )
                .WithTrailingTrivia( trailingTrivia );
        }

        public override SyntaxNode VisitMethodDeclaration( MethodDeclarationSyntax node )
        {
            if ( this.MustReplaceByThrow( node ) )
            {
                return WithThrowNotSupportedExceptionBody( node, "Compile-time only code cannot be called at run-time." );
            }

            return node;
        }

        private bool MustReplaceByThrow( SyntaxNode node )
        {
            var symbol = this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree ).GetDeclaredSymbol( node )!;

            return this.MustReplaceByThrow( symbol );
        }

        private bool MustReplaceByThrow( ISymbol symbol )
            => !symbol.IsAbstract && (this.SymbolClassifier.GetTemplatingScope( symbol ) == TemplatingScope.CompileTimeOnly ||
                                      !this.SymbolClassifier.GetTemplateInfo( symbol ).IsNone);

        public override SyntaxNode? VisitIndexerDeclaration( IndexerDeclarationSyntax node )
        {
            throw new AssertionFailedException( "Indexers are not supported." );

            /*
            if ( this.MustReplaceByThrow( node ) )
            {
                return WithThrowNotSupportedExceptionBody( node, "Compile-time only code cannot be called at run-time." );
            }

            return node;
            */
        }

        public override SyntaxNode? VisitPropertyDeclaration( PropertyDeclarationSyntax node )
        {
            // Properties can be in following forms:
            //  * Accessors with implicit bodies and backing field:         int Foo { get; set; }
            //  * Accessors with explicit bodies:                           int Foo { get { ... } set { ... } }
            //  * Accessors without bodies (abstract):                      abstract int Foo { get; set; }
            //  * Expression body:                                          int Foo => 42;
            //  * Accessors and initializer and backing field:              int Foo { get; } = 42;

            if ( this.MustReplaceByThrow( node ) )
            {
                if ( node.Modifiers.All( x => x.Kind() != SyntaxKind.AbstractKeyword )
                     && node.AccessorList?.Accessors.All( x => x.Body == null && x.ExpressionBody == null ) == true )
                {
                    // This is auto property - we keep it as it is (otherwise we lose the initial value and the fact that it is an auto property).
                    return node;
                }

                return WithThrowNotSupportedExceptionBody( node, "Compile-time only code cannot be called at run-time." );
            }

            return node;
        }

        public override SyntaxNode? VisitEventDeclaration( EventDeclarationSyntax node )
        {
            if ( this.MustReplaceByThrow( node ) )
            {
                return WithThrowNotSupportedExceptionBody( node, "Compile-time only code cannot be called at run-time." );
            }

            return node;
        }
    }
}