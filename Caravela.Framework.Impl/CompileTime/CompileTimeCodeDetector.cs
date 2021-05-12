// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Caravela.Framework.Impl.CompileTime
{
    internal static class CompileTimeCodeDetector
    {
        public static bool HasCompileTimeCode( SyntaxNode node ) => DetectCompileTimeVisitor.Instance.Visit( node );

        private class DetectCompileTimeVisitor : CSharpSyntaxVisitor<bool>
        {
            public static readonly DetectCompileTimeVisitor Instance = new();

            public override bool VisitUsingDirective( UsingDirectiveSyntax node )
                => node.Name.ToString() is "Caravela.Framework.Aspects" or "Caravela.Framework.Project";

            public override bool VisitNamespaceDeclaration( NamespaceDeclarationSyntax node ) => node.ChildNodes().Any( this.Visit );

            public override bool VisitCompilationUnit( CompilationUnitSyntax node ) => node.ChildNodes().Any( this.Visit );

            public override bool DefaultVisit( SyntaxNode node ) => false;
        }
    }
}