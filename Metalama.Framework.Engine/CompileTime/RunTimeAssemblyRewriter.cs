// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.AspectWeavers;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Accessibility = Metalama.Framework.Code.Accessibility;

namespace Metalama.Framework.Engine.CompileTime
{
    /// <summary>
    /// Rewrites a run-time syntax tree so that the implementation of compile-time-only methods is replaced
    /// by a <c>throw new NotSupportedException()</c>.
    /// </summary>
    internal class RunTimeAssemblyRewriter : SafeSyntaxRewriter
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

        /// <summary>
        /// List of warnings that are suppressed from the run-time code of aspects.
        /// </summary>
        private static readonly SeparatedSyntaxList<ExpressionSyntax> _suppressedWarnings = SeparatedList<ExpressionSyntax>(
            new[]
            {
                // An event was declared but never used in the class in which it was declared.
                IdentifierName( "CS0067" ),

                // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
                IdentifierName( "CS8618" ),

                // The compiler detected code that will never be executed.
                IdentifierName( "CS0162" ),

                // The private field is never used.
                IdentifierName( "CS0169" ),

                // The private field 'field' is assigned but its value is never used.
                IdentifierName( "CS0414" ),

                // Can be made static.
                IdentifierName( "CA1822" ),

                // Avoid unused private fields
                IdentifierName( "CA1823 " )
            } );

        // TODO: We can do more in cleaning the run-time assembly. 
        // Private compile-time code can be stripped, except when they are templates, because their metadata must be preserved.
        // In general, accessible compile-time metadata must remain.

        private static readonly Lazy<SyntaxTree> _intrinsicsSyntaxTree =
            new( () => CSharpSyntaxTree.ParseText( _intrinsics, CSharpParseOptions.Default, path: "@@Intrinsics.cs", Encoding.UTF8 ) );

        private readonly INamedTypeSymbol? _aspectDriverSymbol;
        private readonly bool _removeCompileTimeOnlyCode;
        private readonly SyntaxGenerationContextFactory _syntaxGenerationContextFactory;
        private readonly RewriterHelper _rewriterHelper;

        private RunTimeAssemblyRewriter( Compilation runTimeCompilation, IServiceProvider serviceProvider )
        {
            this._rewriterHelper = new RewriterHelper( runTimeCompilation, serviceProvider );
            this._aspectDriverSymbol = runTimeCompilation.GetTypeByMetadataName( typeof(IAspectDriver).FullName );
            this._removeCompileTimeOnlyCode = serviceProvider.GetRequiredService<IProjectOptions>().RemoveCompileTimeOnlyCode;
            this._syntaxGenerationContextFactory = new SyntaxGenerationContextFactory( this.RunTimeCompilation, serviceProvider );
        }

        private Compilation RunTimeCompilation => this._rewriterHelper.RunTimeCompilation;

        private ISymbolClassifier SymbolClassifier => this._rewriterHelper.SymbolClassifier;

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

                transformedCompilation =
                    transformedCompilation.WithSyntaxTreeTransformations( new[] { SyntaxTreeTransformation.AddTree( instrinsicsSyntaxTree ) } );
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
                leadingTrivia = leadingTrivia.InsertAfterFirstNonWhitespaceTrivia(
                    Trivia(
                        PragmaWarningDirectiveTrivia(
                                Token( SyntaxKind.DisableKeyword ),
                                true )
                            .WithErrorCodes( _suppressedWarnings )
                            .NormalizeWhitespace()
                            .WithLeadingTrivia( ElasticLineFeed )
                            .WithTrailingTrivia( ElasticLineFeed ) ) );

                trailingTrivia = trailingTrivia.InsertBeforeLastNonWhitespaceTrivia(
                    Trivia(
                        PragmaWarningDirectiveTrivia(
                                Token( SyntaxKind.RestoreKeyword ),
                                true )
                            .WithErrorCodes( _suppressedWarnings )
                            .NormalizeWhitespace()
                            .WithLeadingTrivia( ElasticLineFeed )
                            .WithTrailingTrivia( ElasticLineFeed ) ) );
            }

            return base.VisitClassDeclaration( node )!
                .WithLeadingTrivia( leadingTrivia )
                .WithTrailingTrivia( trailingTrivia );
        }

        public override SyntaxNode? VisitFieldDeclaration( FieldDeclarationSyntax node )
        {
            var anyChange = false;
            var variables = new List<VariableDeclaratorSyntax>();
            ISymbol? firstTemplateSymbol = null;
            var transformedNode = node;

            foreach ( var variable in node.Declaration.Variables )
            {
                var symbol = this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree ).GetDeclaredSymbol( variable )!;

                var transformedVariable = variable;

                if ( this.IsTemplate( symbol ) )
                {
                    firstTemplateSymbol = null;

                    if ( variable.Initializer != null )
                    {
                        anyChange = true;
                        transformedVariable = variable.WithInitializer( null );
                    }
                }

                variables.Add( transformedVariable );
            }

            if ( anyChange )
            {
                transformedNode = node.WithDeclaration( node.Declaration.WithVariables( SeparatedList( variables ) ) );
            }

            if ( firstTemplateSymbol != null )
            {
                transformedNode = this.MakePublicMember( transformedNode, node, firstTemplateSymbol );
            }

            return transformedNode;
        }

        public override SyntaxNode VisitMethodDeclaration( MethodDeclarationSyntax node )
        {
            var symbol = this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree ).GetDeclaredSymbol( node )!;
            var transformedNode = node;

            if ( this.MustReplaceByThrow( symbol ) )
            {
                transformedNode = this._rewriterHelper.WithThrowNotSupportedExceptionBody( node, "Compile-time-only code cannot be called at run-time." );
            }

            if ( this.IsTemplate( symbol ) )
            {
                transformedNode = this.MakePublicMember( transformedNode, node, symbol );
            }

            return transformedNode;
        }

        private bool MustReplaceByThrow( ISymbol symbol )
            => this._removeCompileTimeOnlyCode && !symbol.IsAbstract
                                               && (this.SymbolClassifier.GetTemplatingScope( symbol ) == TemplatingScope.CompileTimeOnly ||
                                                   !this.SymbolClassifier.GetTemplateInfo( symbol ).IsNone);

        private bool IsTemplate( ISymbol symbol ) => !this.SymbolClassifier.GetTemplateInfo( symbol ).IsNone;

        public override SyntaxNode? VisitPropertyDeclaration( PropertyDeclarationSyntax node )
        {
            // Properties can be in following forms:
            //  * Accessors with implicit bodies and backing field:         int Foo { get; set; }
            //  * Accessors with explicit bodies:                           int Foo { get { ... } set { ... } }
            //  * Accessors without bodies (abstract):                      abstract int Foo { get; set; }
            //  * Expression body:                                          int Foo => 42;
            //  * Accessors and initializer and backing field:              int Foo { get; } = 42;

            var symbol = this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree ).GetDeclaredSymbol( node )!;
            var transformedNode = node;

            if ( this.MustReplaceByThrow( symbol ) )
            {
                if ( node.Modifiers.All( x => !x.IsKind( SyntaxKind.AbstractKeyword ) )
                     && node.AccessorList?.Accessors.All( x => x.Body == null && x.ExpressionBody == null ) == true )
                {
                    // This is auto property - we keep it as it is (otherwise we lose the initial value and the fact that it is an auto property).
                }
                else
                {
                    transformedNode = (PropertyDeclarationSyntax) this._rewriterHelper.WithThrowNotSupportedExceptionBody(
                        node,
                        "Compile-time-only code cannot be called at run-time." );
                }
            }

            if ( this.IsTemplate( symbol ) )
            {
                if ( node.Initializer != null )
                {
                    transformedNode =
                        transformedNode
                            .WithInitializer( null )
                            .WithSemicolonToken( default );
                }

                transformedNode = this.MakePublicMember( transformedNode, node, symbol );

                void ReplaceAccessor( SyntaxKind accessorKind, ISymbol? accessorSymbol )
                {
                    var accessor = node.AccessorList?.Accessors.FirstOrDefault( a => a.IsKind( accessorKind ) );

                    if ( accessor != null )
                    {
                        var transformedAccessor = transformedNode.AccessorList!.Accessors.First( a => a.IsKind( accessorKind ) );
                        var accessorMadePublic = this.MakePublicAccessor( transformedAccessor, accessor, accessorSymbol.AssertNotNull() );
                        transformedNode = transformedNode.ReplaceNode( transformedAccessor, accessorMadePublic );
                    }
                }

                ReplaceAccessor( SyntaxKind.GetAccessorDeclaration, symbol.GetMethod );
                ReplaceAccessor( SyntaxKind.InitAccessorDeclaration, symbol.SetMethod );
                ReplaceAccessor( SyntaxKind.SetAccessorDeclaration, symbol.SetMethod );
            }

            return transformedNode;
        }

        public override SyntaxNode? VisitEventDeclaration( EventDeclarationSyntax node )
        {
            var symbol = this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree ).GetDeclaredSymbol( node )!;
            var transformedNode = node;

            if ( this.MustReplaceByThrow( symbol ) )
            {
                transformedNode = (EventDeclarationSyntax) this._rewriterHelper.WithThrowNotSupportedExceptionBody(
                    node,
                    "Compile-time-only code cannot be called at run-time." );
            }

            if ( this.IsTemplate( symbol ) )
            {
                transformedNode = this.MakePublicMember( transformedNode, node, symbol );
            }

            return transformedNode;
        }

        private T MakePublicMember<T>( T transformedNode, T originalNode, ISymbol symbol )
            where T : MemberDeclarationSyntax
        {
            var accessibility = symbol.DeclaredAccessibility.ToOurVisibility();

            if ( accessibility is Accessibility.Public or Accessibility.Protected )
            {
                // No change is needed.
                return transformedNode;
            }

            var attributeList = this.CreateCompiledTemplateAttribute( originalNode, accessibility ).WithTrailingTrivia( ElasticLineFeed );

            var newModifiers = transformedNode.Modifiers.Where( n => !n.IsAccessModifierKeyword() ).ToList();

            newModifiers.Add( Token( SyntaxKind.PublicKeyword ).WithTrailingTrivia( ElasticSpace ) );

            return (T) transformedNode.WithModifiers( TokenList( newModifiers ) )
                .WithAttributeLists( transformedNode.AttributeLists.Add( attributeList ) )
                .WithLeadingTrivia( transformedNode.GetLeadingTrivia() );
        }

        private AccessorDeclarationSyntax MakePublicAccessor(
            AccessorDeclarationSyntax transformedNode,
            AccessorDeclarationSyntax originalNode,
            ISymbol symbol )
        {
            var accessibility = symbol.DeclaredAccessibility.ToOurVisibility();

            if ( accessibility is Accessibility.Public or Accessibility.Protected )
            {
                // No change is needed.
                return transformedNode;
            }

            var attributeList = this.CreateCompiledTemplateAttribute( originalNode, accessibility ).WithTrailingTrivia( ElasticSpace );

            return transformedNode.WithModifiers( default )
                .WithAttributeLists( transformedNode.AttributeLists.Add( attributeList ) )
                .WithLeadingTrivia( transformedNode.GetLeadingTrivia() );
        }

        private AttributeListSyntax CreateCompiledTemplateAttribute( SyntaxNode node, Accessibility accessibility )
        {
            var syntaxFactory = this._syntaxGenerationContextFactory.GetSyntaxGenerationContext( node );
            var compiledTemplateAttributeType = (INamedTypeSymbol) syntaxFactory.ReflectionMapper.GetTypeSymbol( typeof(CompiledTemplateAttribute) );
            var accessibilityType = (INamedTypeSymbol) syntaxFactory.ReflectionMapper.GetTypeSymbol( typeof(Accessibility) );

            var attribute = Attribute( (NameSyntax) syntaxFactory.SyntaxGenerator.Type( compiledTemplateAttributeType ) )
                .WithArgumentList(
                    AttributeArgumentList(
                        SingletonSeparatedList(
                            AttributeArgument( syntaxFactory.SyntaxGenerator.EnumValueExpression( accessibilityType, (int) accessibility ) )
                                .WithNameEquals( NameEquals( nameof(CompiledTemplateAttribute.Accessibility) ) ) ) ) );

            var attributeList = AttributeList( SingletonSeparatedList( attribute ) )
                .WithGeneratedCodeAnnotation( FormattingAnnotations.SystemGeneratedCodeAnnotation );

            return attributeList;
        }
    }
}