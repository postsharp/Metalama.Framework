// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Templating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;

namespace Caravela.Framework.Impl.CompileTime
{
    /// <summary>
    /// Implements a fast way to detect whether a syntax tree contains compile-time code, just by looking at namespace imports.
    /// This way is imprecise, but it is enforced by an analyzer <see cref="TemplatingCodeValidator"/>. 
    /// </summary>
    internal static class CompileTimeCodeDetector
    {
        public const string Namespace = "Caravela.Framework";
        private const string _excludedNamespace = "Caravela.Framework.RunTime";

        public static bool HasCompileTimeCode( SyntaxNode node ) => DetectCompileTimeVisitor.Instance.Visit( node );

        private class DetectCompileTimeVisitor : CSharpSyntaxVisitor<bool>
        {
            public static readonly DetectCompileTimeVisitor Instance = new();

            public override bool VisitUsingDirective( UsingDirectiveSyntax node )
                => node.Name.ToString().StartsWith( Namespace, StringComparison.OrdinalIgnoreCase ) &&
                   !node.Name.ToString().Equals( _excludedNamespace, StringComparison.Ordinal );

            public override bool VisitNamespaceDeclaration( NamespaceDeclarationSyntax node ) => node.ChildNodes().Any( this.Visit );

            public override bool VisitCompilationUnit( CompilationUnitSyntax node ) => node.ChildNodes().Any( this.Visit );

            public override bool DefaultVisit( SyntaxNode node ) => false;
        }
    }
}