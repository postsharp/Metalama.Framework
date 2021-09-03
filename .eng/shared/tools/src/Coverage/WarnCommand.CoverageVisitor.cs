// Copyright (c) SharpCrafters s.r.o. All rights reserved.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;

namespace PostSharp.Engineering.BuildTools.Coverage
{
    public partial class WarnCommand
    {
        private class CoverageVisitor : CSharpSyntaxWalker
        {
            private readonly HashSet<SyntaxNode> _nonCoveredNodes;
            private readonly IConsole _contextConsole;
            private readonly HashSet<SyntaxNode> _nonCoveredDeclarations = new();

            public int InvalidDeclarationCounts { get; private set; }

            public CoverageVisitor( HashSet<SyntaxNode> nonCoveredNodes, IConsole contextConsole )
            {
                this._nonCoveredNodes = nonCoveredNodes;
                this._contextConsole = contextConsole;
            }

            public override void DefaultVisit( SyntaxNode node )
            {
                if ( this._nonCoveredNodes.Contains( node ) && !IsIgnoredSyntaxNode( node ) )
                {
                    var declarations = node.AncestorsAndSelf().OfType<MemberDeclarationSyntax>().Reverse().ToList();
                    var member = declarations.Last();

                    if ( IsIgnoredMember( member ) )
                    {
                        return;
                    }


                    if ( this._nonCoveredDeclarations.Add( member ) )
                    {
                        var lineSpan = node.SyntaxTree.GetLineSpan( node.Span );
                        var memberName = string.Join( '.', declarations.Select( GetNodeName ) );
                        this._contextConsole.Error.WriteLine(
                            $"{node.SyntaxTree.FilePath}({lineSpan.Span.Start.Line + 1}): warning COVER01: '{memberName}' has gaps in test coverage." );
                        this.InvalidDeclarationCounts++;
                    }
                }
                else
                {
                    base.DefaultVisit( node );
                }

                static bool IsIgnoredSyntaxNode( SyntaxNode node )
                    => node switch
                    {
                        ThrowStatementSyntax => true,
                        MemberDeclarationSyntax member => IsIgnoredMember( member ),
                        _ => node.ToFullString().Contains( "// Coverage: Ignore" )
                    };

                static bool IsIgnoredMember( MemberDeclarationSyntax member ) => member switch
                {
                    MethodDeclarationSyntax { Identifier: { Text: "ToString" } } => true,
                    PropertyDeclarationSyntax { AccessorList: { } accessorList } when accessorList.Accessors.All(
                        a => a.Body == null && a.ExpressionBody == null ) => true,
                    _ => false
                };


                static string GetNodeName( SyntaxNode node )
                    => node switch
                    {
                        MethodDeclarationSyntax m => m.Identifier.Text,
                        ClassDeclarationSyntax c => c.Identifier.Text,
                        StructDeclarationSyntax s => s.Identifier.Text,
                        RecordDeclarationSyntax r => r.Identifier.Text,
                        FieldDeclarationSyntax f => f.Declaration.Variables.ToString(),
                        EventDeclarationSyntax e => e.Identifier.Text,
                        EventFieldDeclarationSyntax e => e.Declaration.Variables.ToString(),
                        PropertyDeclarationSyntax p => p.Identifier.Text,
                        NamespaceDeclarationSyntax n => n.Name.ToString(),
                        ConstructorDeclarationSyntax => "constructor",
                        _ => node.Kind().ToString()
                    };
            }
        }
    }
}