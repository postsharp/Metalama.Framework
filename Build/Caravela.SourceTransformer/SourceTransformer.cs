// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.SourceTransformer
{
    [Transformer]
    internal class SourceTransformer : ISourceTransformer
    {
        public Compilation Execute( TransformerContext context )
        {
            var changeDynamicToObject = context.GlobalOptions.TryGetValue( "build_property.ChangeDynamicToObject", out var changeDynamicToObjectStr )
                                        && bool.Parse( changeDynamicToObjectStr );

            var rewriter = new Rewriter( changeDynamicToObject );
            var compilation = context.Compilation;

            foreach ( var tree in compilation.SyntaxTrees )
            {
                var newRoot = rewriter.Visit( tree.GetRoot() );

                compilation = compilation.ReplaceSyntaxTree( tree, tree.WithRootAndOptions( newRoot, tree.Options ) );
            }

            foreach ( var diagnostic in rewriter.Diagnostics )
            {
                context.ReportDiagnostic( diagnostic );
            }

            return compilation;
        }

        private class Rewriter : CSharpSyntaxRewriter
        {
            private readonly bool _changeDynamicToObject;
            private List<FieldDeclarationSyntax>? _fieldsToAdd;

            public Rewriter( bool changeDynamicToObject )
            {
                this._changeDynamicToObject = changeDynamicToObject;
            }

            public List<Diagnostic> Diagnostics { get; } = new();

            public override SyntaxNode VisitPropertyDeclaration( PropertyDeclarationSyntax node )
            {
                // Make sure we have substituted the dynamic keyword.
                node = (PropertyDeclarationSyntax) base.VisitPropertyDeclaration( node )!;

                if ( !node.AttributeLists.SelectMany( al => al.Attributes )
                    .Any(
                        a => string.Equals( a.Name.ToString(), "Memo", StringComparison.Ordinal ) ||
                             string.Equals( a.Name.ToString(), "MemoAttribute", StringComparison.Ordinal ) ) )
                {
                    return node;
                }

                if ( !IsSupportedPropertyDeclaration( node ) )
                {
                    this.Diagnostics.Add( Diagnostic.Create( _nonExpressionBodyError, node.GetLocation() ) );

                    return node;
                }

                if ( node.Modifiers.Any( SyntaxKind.StaticKeyword ) )
                {
                    this.Diagnostics.Add( Diagnostic.Create( _staticPropertyError, node.GetLocation() ) );

                    return node;
                }

                var fieldName = "@" + char.ToLowerInvariant( node.Identifier.ValueText[0] ) + node.Identifier.ValueText.Substring( 1 );

                this._fieldsToAdd!.Add( FieldDeclaration( VariableDeclaration( NullableType( node.Type ) ).AddVariables( VariableDeclarator( fieldName ) ) ) );

                if ( node.ExpressionBody != null )
                {
                    var block = TransformExpression( fieldName, node.ExpressionBody.Expression );

                    var newNode = node.WithExpressionBody( null )
                        .WithSemicolonToken( default )
                        .AddAccessorListAccessors( AccessorDeclaration( SyntaxKind.GetAccessorDeclaration, block ) );

                    // PropertyType? field;

                    return newNode;
                }
                else
                {
                    var getAccessor = node.AccessorList!.Accessors.SingleOrDefault( a => a.Kind() == SyntaxKind.GetAccessorDeclaration )!;
                    var setAccessor = node.AccessorList!.Accessors.SingleOrDefault( a => a.Kind() == SyntaxKind.SetAccessorDeclaration );

                    var block = TransformExpression( fieldName, getAccessor.ExpressionBody!.Expression );

                    var newGetAccessor =
                        getAccessor.WithExpressionBody( null )
                            .WithSemicolonToken( default )
                            .WithBody( block );

                    var newNode = node.WithAccessorList(
                            AccessorList(
                                setAccessor != null
                                    ? List( new[] { newGetAccessor, setAccessor } )
                                    : SingletonList( newGetAccessor ) ) )
                        .WithSemicolonToken( default );

                    return newNode;
                }
            }

            private static bool IsSupportedPropertyDeclaration( PropertyDeclarationSyntax node )
            {
                if ( node.ExpressionBody != null )
                {
                    return true;
                }

                if ( node.AccessorList == null )
                {
                    return false;
                }

                var getAccessor = node.AccessorList!.Accessors.SingleOrDefault( a => a.Kind() == SyntaxKind.GetAccessorDeclaration );
                var setAccessor = node.AccessorList!.Accessors.SingleOrDefault( a => a.Kind() == SyntaxKind.SetAccessorDeclaration );

                if ( getAccessor?.ExpressionBody == null )
                {
                    return false;
                }

                if ( setAccessor?.ExpressionBody?.Expression is ThrowExpressionSyntax )
                {
                    return true;
                }

                return false;
            }

            private static BlockSyntax TransformExpression( string fieldName, ExpressionSyntax expression )
                =>

                    // PERF: read the field into a local, to avoid unnecessary double read in the fast case
                    // if (field == null)
                    //     Interlocked.CompareExchange(ref field, expression, null);
                    // return field;
                    Block(
                        IfStatement(
                            BinaryExpression( SyntaxKind.EqualsExpression, IdentifierName( fieldName ), LiteralExpression( SyntaxKind.NullLiteralExpression ) ),
                            ExpressionStatement(
                                InvocationExpression( ParseExpression( "Interlocked.CompareExchange" ) )
                                    .AddArgumentListArguments(
                                        Argument( IdentifierName( fieldName ) ).WithRefKindKeyword( Token( SyntaxKind.RefKeyword ) ),
                                        Argument( expression ),
                                        Argument( LiteralExpression( SyntaxKind.NullLiteralExpression ) ) ) ) ),
                        ReturnStatement( IdentifierName( fieldName ) ) );

            public override SyntaxNode VisitClassDeclaration( ClassDeclarationSyntax node )
            {
                var parentFieldsToAdd = this._fieldsToAdd;
                this._fieldsToAdd = new List<FieldDeclarationSyntax>();

                var result = (ClassDeclarationSyntax) base.VisitClassDeclaration( node )!;

                result = result.AddMembers( this._fieldsToAdd.ToArray<MemberDeclarationSyntax>() );

                this._fieldsToAdd = parentFieldsToAdd;

                return result;
            }

            public override SyntaxNode? VisitCompilationUnit( CompilationUnitSyntax node )
            {
                const string usingSystemThreading = "using System.Threading;";

                node = (CompilationUnitSyntax) base.VisitCompilationUnit( node )!;

                if ( node.Usings.All( u => u.ToString() != usingSystemThreading ) )
                {
                    node = node.AddUsings( ParseCompilationUnit( usingSystemThreading ).Usings.Single() );
                }

                return node;
            }

            public override SyntaxNode? VisitIdentifierName( IdentifierNameSyntax node )
            {
                if ( node.Identifier.Text == "dynamic" )
                {
                    if ( this._changeDynamicToObject )
                    {
                        return PredefinedType( Token( SyntaxKind.ObjectKeyword ) ).WithTriviaFrom( node );
                    }
                    else
                    {
                        this.Diagnostics.Add( Diagnostic.Create( _dynamicKeywordError, node.GetLocation() ) );
                    }
                }

                return base.VisitIdentifierName( node );
            }
        }

        private static readonly DiagnosticDescriptor _nonExpressionBodyError =
            new(
                "CMT001",
                "Only expression-bodied properties are supported.",
                "Only expression-bodied properties are supported.",
                "Caravela.SourceTransformer",
                DiagnosticSeverity.Error,
                true );

        private static readonly DiagnosticDescriptor _staticPropertyError =
            new(
                "CMT002",
                "Static properties are not supported.",
                "Static properties are not supported.",
                "Caravela.SourceTransformer",
                DiagnosticSeverity.Error,
                true );

        private static readonly DiagnosticDescriptor _dynamicKeywordError =
            new(
                "CMT004",
                "Dynamic keyword forbidden.",
                "The 'dynamic' keyword is forbidden in this project.",
                "Caravela.SourceTransformer",
                DiagnosticSeverity.Error,
                true );
    }
}