// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.CompileTime
{
    /// <summary>
    /// Implements a fast way to detect whether a syntax tree contains compile-time code, just by looking at namespace imports.
    /// This way is imprecise, but it is enforced by an analyzer <see cref="TemplatingCodeValidator"/>. 
    /// </summary>
    public static class CompileTimeCodeFastDetector
    {
        public const string Namespace = "Metalama.Framework";

        public static ImmutableHashSet<string> SubNamespaces { get; } = ImmutableHashSet.Create<string>(
            StringComparer.Ordinal,
            "Aspects",
            "Code",
            "Diagnostics",
            "Fabrics",
            "Project" );

        // The 'Validation' namespace should NOT be included because referencing a constraint of this namespace is a normal use case in run-time code.

        public static bool HasCompileTimeCode( SyntaxNode node ) => DetectCompileTimeVisitor.Instance.Visit( node );

        private class DetectCompileTimeVisitor : SafeSyntaxVisitor<bool>
        {
            public static readonly DetectCompileTimeVisitor Instance = new();

            public override bool VisitUsingDirective( UsingDirectiveSyntax node )
            {
                if ( node.GlobalKeyword.IsKind( SyntaxKind.GlobalKeyword ) )
                {
                    // Any tree containing a global using must be included in the set of compile-time trees because they need to be scanned.
                    return true;
                }
                else
                {
                    // Any tree containing a using directive for Metalama.Framework needs to be included.
                    return node.Name switch
                    {
                        QualifiedNameSyntax q3 when SubNamespaces.Contains( q3.Right.Identifier.Text ) &&
                                                    q3.Left is QualifiedNameSyntax q2 && q2.Right.Identifier.Text == "Framework"
                                                    && q2.Left is IdentifierNameSyntax { Identifier: { Text: "Metalama" } } => true,
                        _ => false
                    };
                }
            }

            public override bool VisitNamespaceDeclaration( NamespaceDeclarationSyntax node ) => node.ChildNodes().Any( this.Visit );

            public override bool VisitFileScopedNamespaceDeclaration( FileScopedNamespaceDeclarationSyntax node ) => node.ChildNodes().Any( this.Visit );

            public override bool VisitCompilationUnit( CompilationUnitSyntax node ) => node.ChildNodes().Any( this.Visit );

            public override bool DefaultVisit( SyntaxNode node ) => false;
        }
    }
}