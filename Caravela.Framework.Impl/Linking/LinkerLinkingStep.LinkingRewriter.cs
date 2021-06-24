// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Linking
{
    internal partial class LinkerLinkingStep
    {
        /// <summary>
        /// Rewriter which rewrites classes and methods producing the linked and inlined syntax tree.
        /// </summary>
        private class LinkingRewriter : CSharpSyntaxRewriter
        {
            private readonly Compilation _intermediateCompilation;
            private readonly LinkerAnalysisRegistry _analysisRegistry;
            private readonly LinkerRewritingDriver _rewritingDriver;

            public LinkingRewriter(
                Compilation intermediateCompilation,
                LinkerAnalysisRegistry analysisRegistry, 
                LinkerRewritingDriver rewritingDriver )
            {
                this._intermediateCompilation = intermediateCompilation;
                this._analysisRegistry = analysisRegistry;
                this._rewritingDriver = rewritingDriver;
            }

            public override SyntaxNode? VisitClassDeclaration( ClassDeclarationSyntax node )
            {
                // TODO: Other transformations than method overrides.
                var newMembers = new List<MemberDeclarationSyntax>();

                foreach ( var member in node.Members )
                {
                    // Go through all members of the type.
                    // For members that represent overrides:
                    //  * If the member can be inlined, skip it.
                    //  * If the member cannot be inlined (or is the root of inlining), add the transformed member with all possible inlining instances.
                    // For members that represent override targets (i.e. overridden members):
                    //  * If the last (transformation order) override is inlineable, replace the member with it's transformed body.
                    //  * Otherwise create a stub that calls the last override.

                    var semanticModel = this._intermediateCompilation.GetSemanticModel( node.SyntaxTree );
                    var symbols =
                        member switch
                        {
                            MethodDeclarationSyntax methodDecl => new[] { semanticModel.GetDeclaredSymbol( methodDecl ) },
                            BasePropertyDeclarationSyntax basePropertyDecl => new[] { semanticModel.GetDeclaredSymbol( basePropertyDecl ) },
                            FieldDeclarationSyntax fieldDecl =>
                                fieldDecl.Declaration.Variables.Select( v => semanticModel.GetDeclaredSymbol(v)).ToArray(),
                            EventFieldDeclarationSyntax eventFieldDecl =>
                                eventFieldDecl.Declaration.Variables.Select( v => semanticModel.GetDeclaredSymbol( v ) ).ToArray(),
                        };

                    if ( symbols.Length == 0 || (symbols.Length == 1 && symbols[0] == null ) )
                    {
                        // TODO: Comment when this happens.
                        newMembers.Add( member );
                        continue;
                    }

                    if (symbols.Length == 1)
                    {
                        // Simple case where the declaration declares a single symbol.
                        if ( this._rewritingDriver.IsRewriteTarget( symbols[0].AssertNotNull() ) )
                        {
                            // Add rewritten member and it's induced members (or nothing if the member is discarded).
                            newMembers.AddRange( this._rewritingDriver.RewriteMember( member, symbols[0].AssertNotNull() ) );
                        }
                        else
                        {
                            // Normal member without any transformations.
                            newMembers.Add( member );
                        }
                    }
                    else
                    {
                        var remainingSymbols = new HashSet<ISymbol>(SymbolEqualityComparer.Default);

                        foreach (var symbol in symbols)
                        {
                            if (this._rewritingDriver.IsRewriteTarget(symbol.AssertNotNull()))
                            {
                                newMembers.AddRange( this._rewritingDriver.RewriteMember( member, symbol.AssertNotNull() ) );
                            }
                            else
                            {
                                remainingSymbols.Add( symbol.AssertNotNull() );
                            }
                        }

                        if (remainingSymbols.Count == symbols.Length)
                        {
                            // No change.
                            newMembers.Add( member );
                        }
                        else if ( remainingSymbols.Count > 0)
                        {
                            // Remove declarators that were rewritten.
                            switch ( member )
                            {
                                case FieldDeclarationSyntax fieldDecl:
                                    newMembers.Add(
                                        fieldDecl.WithDeclaration(
                                            fieldDecl.Declaration.WithVariables(
                                                SeparatedList(
                                                    fieldDecl.Declaration.Variables
                                                    .Where( v => 
                                                        remainingSymbols.Contains( semanticModel.GetDeclaredSymbol( v ).AssertNotNull() ) ) ) ) ) );
                                    break;

                                case EventFieldDeclarationSyntax eventFieldDecl:
                                    newMembers.Add(
                                        eventFieldDecl.WithDeclaration(
                                            eventFieldDecl.Declaration.WithVariables(
                                                SeparatedList(
                                                    eventFieldDecl.Declaration.Variables
                                                    .Where( v =>
                                                        remainingSymbols.Contains( semanticModel.GetDeclaredSymbol( v ).AssertNotNull() ) ) ) ) ) );
                                    break;

                                default:
                                    throw new AssertionFailedException();
                            }
                        }
                    }
                }

                return node.WithMembers( List( newMembers ) );
            }
        }
    }
}