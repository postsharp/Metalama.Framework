// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;


namespace Metalama.Framework.Engine.CompileTime
{
    internal partial class CompileTimeCompilationBuilder
    {
        /// <summary>
        /// Removes invalid <c>using</c> statements from a compile-time syntax tree. Such using statements
        /// are typically run-time-only.
        /// </summary>
        internal class RemoveInvalidUsingRewriter : SafeSyntaxRewriter
        {
            private readonly Compilation _compileTimeCompilation;

            public RemoveInvalidUsingRewriter( Compilation compileTimeCompilation )
            {
                this._compileTimeCompilation = compileTimeCompilation;
            }

            public override SyntaxNode? VisitCompilationUnit( CompilationUnitSyntax node )
            {
                var compilationMemberBuilder = new IncompleteSyntaxListBuilder();
                
                // Add usings.
                compilationMemberBuilder.AddFilteredNodes( node.Usings, u => this._compileTimeCompilation.GetSemanticModel( u.SyntaxTree ).GetSymbolInfo( u.Name ).Symbol == null ? null : u );
                
                // Add assembly-level attributes.
                compilationMemberBuilder.AddRange( node.AttributeLists );
                
                // Add members.
                compilationMemberBuilder.AddRange( node.Members );

                // The order of dequeuing nodes and trivia must be the same as the enqueuing order.
                return node
                    .WithUsings( List( compilationMemberBuilder.DequeueNodesOfType<UsingDirectiveSyntax>() ) )
                    .WithAttributeLists( List( compilationMemberBuilder.DequeueNodesOfType<AttributeListSyntax>() ) )
                    .WithMembers( List( compilationMemberBuilder.DequeueNodesOfType<MemberDeclarationSyntax>() ) )
                    .WithTrailingTrivia( node.GetTrailingTrivia().InsertRange( 0, compilationMemberBuilder.DequeueTriviaList() ) );
            }

        }
    }
}