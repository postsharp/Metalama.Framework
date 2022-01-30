// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace Metalama.Framework.Engine.Formatting;

public static partial class OutputCodeFormatter
{
    private class MarkUsingsRewriter : CSharpSyntaxRewriter
    {
        private MarkUsingsRewriter() { }

        public static MarkUsingsRewriter Instance { get; } = new();

        public override SyntaxNode? VisitUsingDirective( UsingDirectiveSyntax node )
            => node.HasAnnotation( Formatter.Annotation )
                ? node.NormalizeWhitespace( elasticTrivia: true ).WithAdditionalAnnotations( FormattingAnnotations.GeneratedCode )
                : node;

        public override SyntaxNode? VisitAccessorDeclaration( AccessorDeclarationSyntax node ) => node;

        public override SyntaxNode? VisitClassDeclaration( ClassDeclarationSyntax node ) => node;

        public override SyntaxNode? VisitStructDeclaration( StructDeclarationSyntax node ) => node;

        public override SyntaxNode? VisitRecordDeclaration( RecordDeclarationSyntax node ) => node;

        public override SyntaxNode? VisitConstructorDeclaration( ConstructorDeclarationSyntax node ) => node;

        public override SyntaxNode? VisitDelegateDeclaration( DelegateDeclarationSyntax node ) => node;

        public override SyntaxNode? VisitDestructorDeclaration( DestructorDeclarationSyntax node ) => node;

        public override SyntaxNode? VisitEnumDeclaration( EnumDeclarationSyntax node ) => node;

        public override SyntaxNode? VisitEventDeclaration( EventDeclarationSyntax node ) => node;

        public override SyntaxNode? VisitFieldDeclaration( FieldDeclarationSyntax node ) => node;

        public override SyntaxNode? VisitEventFieldDeclaration( EventFieldDeclarationSyntax node ) => node;

        public override SyntaxNode? VisitIndexerDeclaration( IndexerDeclarationSyntax node ) => node;

        public override SyntaxNode? VisitInterfaceDeclaration( InterfaceDeclarationSyntax node ) => node;

        public override SyntaxNode? VisitOperatorDeclaration( OperatorDeclarationSyntax node ) => node;

        public override SyntaxNode? VisitMethodDeclaration( MethodDeclarationSyntax node ) => node;

        public override SyntaxNode? VisitConversionOperatorDeclaration( ConversionOperatorDeclarationSyntax node ) => node;

        public override SyntaxNode? VisitPropertyDeclaration( PropertyDeclarationSyntax node ) => node;
    }
}