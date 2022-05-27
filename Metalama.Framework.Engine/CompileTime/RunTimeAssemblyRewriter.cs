// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.AspectWeavers;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.CompileTime
{
    /// <summary>
    /// Rewrites a run-time syntax tree so that the implementation of compile-time-only methods is replaced
    /// by a <c>throw new NotSupportedException()</c>.
    /// </summary>
    internal class RunTimeAssemblyRewriter : CompileTimeBaseRewriter
    {
        private const string _intrinsics = @"
using System;

namespace Metalama.Compiler
{
    internal static class Intrinsics
    {
        public static RuntimeMethodHandle GetRuntimeMethodHandle(string documentationId) => throw new InvalidOperationException(""Code calling this method has to be compiled by the Metalama compiler."");
        public static RuntimeFieldHandle GetRuntimeFieldHandle(string documentationId) => throw new InvalidOperationException(""Code calling this method has to be compiled by the Metalama compiler."");
        public static RuntimeTypeHandle GetRuntimeTypeHandle(string documentationId) => throw new InvalidOperationException(""Code calling this method has to be compiled by the Metalama compiler."");
    }
}
";

        // TODO: We can do more in cleaning the run-time assembly. 
        // Private compile-time code can be stripped, except when they are templates, because their metadata must be preserved.
        // In general, accessible compile-time metadata must remain.

        private static readonly Lazy<SyntaxTree> _intrinsicsSyntaxTree =
            new( () => CSharpSyntaxTree.ParseText( _intrinsics, CSharpParseOptions.Default, path: "@@Intrinsics.cs", Encoding.UTF8 ) );

        private readonly INamedTypeSymbol? _aspectDriverSymbol;
        private readonly bool _removeCompileTimeOnlyCode;

        private RunTimeAssemblyRewriter( Compilation runTimeCompilation, IServiceProvider serviceProvider )
            : base( runTimeCompilation, serviceProvider )
        {
            this._aspectDriverSymbol = runTimeCompilation.GetTypeByMetadataName( typeof(IAspectDriver).FullName );
            this._removeCompileTimeOnlyCode = serviceProvider.GetRequiredService<IProjectOptions>().RemoveCompileTimeOnlyCode;
        }

        public static IPartialCompilation Rewrite( IPartialCompilation compilation, IServiceProvider serviceProvider )
        {
            var rewriter = new RunTimeAssemblyRewriter( compilation.Compilation, serviceProvider );

            var transformedCompilation = compilation.RewriteSyntaxTrees( rewriter );

            if ( transformedCompilation.Compilation.GetTypeByMetadataName( "Metalama.Compiler.Intrinsics" ) == null )
            {
                var instrinsicsSyntaxTree = _intrinsicsSyntaxTree.Value;

                // We need to copy syntax tree options, otherwise we may have language version mismatch.
                if ( compilation.Compilation.SyntaxTrees.Any() )
                {
                    var options = compilation.Compilation.SyntaxTrees.First().Options;
                    instrinsicsSyntaxTree = instrinsicsSyntaxTree.WithRootAndOptions( instrinsicsSyntaxTree.GetRoot(), options );
                }

                transformedCompilation = transformedCompilation.WithSyntaxTreeModifications( null, new[] { instrinsicsSyntaxTree } );
            }

            return transformedCompilation;
        }

        public override SyntaxNode? VisitClassDeclaration( ClassDeclarationSyntax node )
        {
            var symbol = this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree ).GetDeclaredSymbol( node )!;

            // Special case: aspect weavers and other aspect drivers are preserved in the runtime assembly.
            // This only happens if regular Metalama.Framework is referenced from the weaver project, which generally shouldn't happen.
            // But it is a pattern used by Metalama.Samples for try.postsharp.net.
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

                trailingTrivia = trailingTrivia.Add( ElasticLineFeed )
                    .Add(
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

        public override SyntaxNode? VisitFieldDeclaration( FieldDeclarationSyntax node )
        {
            var anyChange = false;
            var variables = new List<VariableDeclaratorSyntax>();

            foreach ( var variable in node.Declaration.Variables )
            {
                if ( variable.Initializer != null && this.MustRemoveInitializer( variable ) )
                {
                    anyChange = true;
                    variables.Add( variable.WithInitializer( null ) );
                }
                else
                {
                    variables.Add( variable );
                }
            }

            if ( anyChange )
            {
                return node.WithDeclaration( node.Declaration.WithVariables( SeparatedList( variables ) ) );
            }
            else
            {
                return node;
            }
        }

        public override SyntaxNode VisitMethodDeclaration( MethodDeclarationSyntax node )
        {
            if ( this.MustReplaceByThrow( node ) )
            {
                return this.WithThrowNotSupportedExceptionBody( node, "Compile-time-only code cannot be called at run-time." );
            }

            return node;
        }

        private bool MustRemoveInitializer( SyntaxNode node )
        {
            var symbol = this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree ).GetDeclaredSymbol( node )!;

            return this.MustRemoveInitializer( symbol );
        }

        private bool MustRemoveInitializer( ISymbol symbol ) => !this.SymbolClassifier.GetTemplateInfo( symbol ).IsNone;

        private bool MustReplaceByThrow( SyntaxNode node )
        {
            var symbol = this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree ).GetDeclaredSymbol( node )!;

            return this.MustReplaceByThrow( symbol );
        }

        private bool MustReplaceByThrow( ISymbol symbol )
            => this._removeCompileTimeOnlyCode && !symbol.IsAbstract
                                               && (this.SymbolClassifier.GetTemplatingScope( symbol ) == TemplatingScope.CompileTimeOnly ||
                                                   !this.SymbolClassifier.GetTemplateInfo( symbol ).IsNone);

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
                if ( node.Modifiers.All( x => !x.IsKind( SyntaxKind.AbstractKeyword ) )
                     && node.AccessorList?.Accessors.All( x => x.Body == null && x.ExpressionBody == null ) == true )
                {
                    // This is auto property - we keep it as it is (otherwise we lose the initial value and the fact that it is an auto property).
                    return node;
                }

                return this.WithThrowNotSupportedExceptionBody( node, "Compile-time-only code cannot be called at run-time." );
            }

            if ( node.Initializer != null && this.MustRemoveInitializer( node ) )
            {
                return node.WithInitializer( null );
            }

            return node;
        }

        public override SyntaxNode? VisitEventDeclaration( EventDeclarationSyntax node )
        {
            if ( this.MustReplaceByThrow( node ) )
            {
                return this.WithThrowNotSupportedExceptionBody( node, "Compile-time-only code cannot be called at run-time." );
            }

            return node;
        }
    }
}