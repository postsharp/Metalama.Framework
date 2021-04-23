// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Caravela.Framework.Impl.CompileTime
{
    internal partial class CompileTimeAssemblyBuilder
    {
        private class PrepareRunTimeAssemblyRewriter : Rewriter
        {
            private readonly INamedTypeSymbol? _aspectDriverSymbol;

            public PrepareRunTimeAssemblyRewriter( Compilation runTimeCompilation )
                : base( runTimeCompilation )
            {
                this._aspectDriverSymbol = runTimeCompilation.GetTypeByMetadataName( typeof(IAspectDriver).FullName );
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

                return base.VisitClassDeclaration( node );
            }

            public override SyntaxNode VisitMethodDeclaration( MethodDeclarationSyntax node )
            {
                if ( this.GetSymbolDeclarationScope( node ) is SymbolDeclarationScope.CompileTimeOnly )
                {
                    return WithThrowNotSupportedExceptionBody( node, "Compile-time only code cannot be called at run-time." );
                }

                return node;
            }
        }
    }
}