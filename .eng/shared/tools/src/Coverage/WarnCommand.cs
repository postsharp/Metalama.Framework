// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PostSharp.Engineering.BuildTools.Coverage
{
    public partial class WarnCommand : Command
    {
        public WarnCommand() : base( "warn", "Emit warnings based on a test coverage report" )
        {
            this.AddArgument( new Argument<string>( "path", "Path to the OpenCover xml file" ) );

            this.Handler = CommandHandler.Create<InvocationContext, string>( this.Execute );
        }

        private void Execute( InvocationContext context, string path )
        {
            var totalInvalidDeclarations = 0;

            var document = JsonDocument.Parse( File.ReadAllText( path ) );

            foreach ( var packageNode in document.RootElement.EnumerateObject() )
            {
                this.ProcessPackage( context, packageNode, ref totalInvalidDeclarations );
            }

            context.Console.Out.WriteLine(
                $"The whole solution has {totalInvalidDeclarations} declaration(s) with insufficient test coverage." );
            context.ResultCode = totalInvalidDeclarations == 0 ? 0 : 1;
        }

        private void ProcessPackage( InvocationContext context, JsonProperty packageNode, ref int totalInvalidDeclarations )
        {
            var invalidDeclarations = 0;
            var packageName = packageNode.Name;

            if ( packageName.Contains( ".Tests" ) )
            {
                return;
            }

            HashSet<SyntaxNode> nonCoveredNodes = new();

            foreach ( var fileNode in packageNode.Value.EnumerateObject().OrderBy( n=> n.Name ) )
            {
                SyntaxTree? syntaxTree = null;

                foreach ( var classNode in fileNode.Value.EnumerateObject().OrderBy( n=> n.Name ) )
                {
                    var className = classNode.Name;

                    foreach ( var methodNode in classNode.Value.EnumerateObject().OrderBy( n=> n.Name ) )
                    {
                        // Getting the method name is convenient to set a conditional breakpoint.
                        var methodName = methodNode.Name;

                        // Enumerate lines.
                        foreach ( var lineNode in methodNode.Value.GetProperty( "Lines" ).EnumerateObject() )
                        {
                            var hits = lineNode.Value.GetInt32();
                            if ( hits > 0 )
                            {
                                continue;
                            }

                            var lineNumber = int.Parse( lineNode.Name ) - 1;

                            // The sequence point is not covered.
                            syntaxTree ??=  CSharpSyntaxTree.ParseText(
                                File.ReadAllText( fileNode.Name ),
                                CSharpParseOptions.Default.WithPreprocessorSymbols( "NET5_0" ),
                                fileNode.Name );

                            var sourceText = syntaxTree.GetText();
                            var line = sourceText.Lines[lineNumber];
                            var span = TextSpan.FromBounds( line.Start, line.End );

                            int spanStart, spanEnd;
                            for ( spanStart = span.Start;
                                char.IsWhiteSpace( sourceText[spanStart] ) && spanStart < span.End;
                                spanStart++ )
                            {
                                // Intentionally empty.
                            }

                            for ( spanEnd = span.End;
                                char.IsWhiteSpace( sourceText[spanEnd] ) && spanEnd > spanStart;
                                spanEnd-- )
                            {
                                // Intentionally empty.
                            }

                            var trimmedSpan = TextSpan.FromBounds( spanStart, spanEnd + 1 );
                            var spanText = sourceText.GetSubText( trimmedSpan ).ToString();

                            if ( spanText.Length == 1 )
                            {
                                // Ignore single-character lines. Typically it's a line containing { or }.
                                continue;
                            }


                            var node = syntaxTree.GetRoot()
                                .FindNode( TextSpan.FromBounds( spanStart, spanEnd ), true, true );

                            if ( node is CompilationUnitSyntax or ClassDeclarationSyntax )
                            {
                                // THis typically happens because the block is disabled by an inactive #if.
                                continue;
                            }


                            if ( !nonCoveredNodes.Add( node ) )
                            {
                                continue;
                            }

                          

                            var declarations = node.AncestorsAndSelf().Where( a => a is MemberDeclarationSyntax or AccessorDeclarationSyntax).Reverse()
                                .ToList();
                            
                            if ( declarations.Count == 0 )
                            {
                                return;
                            }

                            var member = declarations.OfType<MemberDeclarationSyntax>().Last();
                            
                            if ( ShouldIgnore( member ) )
                            {
                                break;
                            }
                            
                            if ( ShouldIgnore( member, node ) )
                            {
                                continue;
                            }




                            var memberName = string.Join( '.', declarations.Select( GetNodeSignature ) );
                            context.Console.Error.WriteLine(
                                $"{node.SyntaxTree.FilePath}({lineNumber + 1}): warning COVER01: '{memberName}' has gaps in test coverage." );
                            totalInvalidDeclarations++;
                            invalidDeclarations++;

                            break;
                        }
                    }
                }
            }

            context.Console.Out.WriteLine(
                $"Module {packageName} has {invalidDeclarations} declaration(s) with insufficient test coverage." );
        }


        private static readonly Regex _ignoreCommentRegex = new(@"//\s*Coverage:\s*Ignore", RegexOptions.IgnoreCase);

        private static bool ContainsIgnoreComment( string text ) => _ignoreCommentRegex.IsMatch( text );

        private static bool IsSameMemberName( MemberDeclarationSyntax declaringMember, ExpressionSyntax expression )
            => expression switch
            {
                MemberAccessExpressionSyntax name => GetNodeName( declaringMember ) == name.Name.Identifier.Text,
                _ => false
            };

        private static bool ShouldIgnore( MemberDeclarationSyntax declaringMember, SyntaxNode? node )
            => node switch
            {
                null => true,
                BlockSyntax block => block.Statements.All( b => ShouldIgnore( declaringMember, b ) ),
                ThrowExpressionSyntax => true,
                ThrowStatementSyntax => true,
                MemberDeclarationSyntax member => ShouldIgnore( member ),
                SwitchExpressionArmSyntax arm => ShouldIgnore( declaringMember,arm.Expression ),
                ArrowExpressionClauseSyntax arrow => ShouldIgnore( declaringMember, arrow.Expression ),
                ExpressionStatementSyntax statement => ShouldIgnore(declaringMember, statement.Expression ),
                InvocationExpressionSyntax invocation => IsSameMemberName( declaringMember, invocation.Expression ) || invocation.ArgumentList.Arguments.Count == 0,
                MemberAccessExpressionSyntax memberAccess => ShouldIgnore( declaringMember, memberAccess.Expression ),
                QualifiedNameSyntax => true,
                IdentifierNameSyntax => true,
                LiteralExpressionSyntax => true,
                DefaultExpressionSyntax => true,
                ReturnStatementSyntax r => ShouldIgnore( declaringMember, r.Expression ),
                AccessorDeclarationSyntax accessor => ShouldIgnore( declaringMember, accessor.Body ) && ShouldIgnore( declaringMember, accessor.ExpressionBody ),
                _ => ContainsIgnoreComment( node.ToFullString() )
            };

        private static bool ShouldIgnore( MemberDeclarationSyntax member ) => member switch
        {
            // Ignore ToString()
            MethodDeclarationSyntax { Identifier: { Text: "ToString" } } => true,

            // Process properties with accessors.
            PropertyDeclarationSyntax { ExpressionBody: null, AccessorList: { } accessorList }
                when accessorList.Accessors.All( a => ShouldIgnore( member, a.Body ) && ShouldIgnore(member, a.ExpressionBody ) )
                => true,

            // Process properties without accessors.
            PropertyDeclarationSyntax { ExpressionBody: { Expression: { } expression } } when ShouldIgnore( member, expression )
                => true,

            // Process methods.
            MethodDeclarationSyntax method when ShouldIgnore( member, method.ExpressionBody ) && ShouldIgnore( member, method.Body )
                => true,

            _ => member.GetLeadingTrivia().Any( t => ContainsIgnoreComment( t.ToFullString() ) )
        };


        private static string GetNodeSignature( SyntaxNode node )
            => node switch
            {
                MethodDeclarationSyntax m => m.Identifier.Text + "(" +
                                             string.Join( ",",
                                                 m.ParameterList.Parameters.Select( p => p.Type?.ToString() ) ) + ")",
                ConstructorDeclarationSyntax c => "ctor" + "(" +
                                                  string.Join( ",",
                                                      c.ParameterList.Parameters.Select( p => p.Type?.ToString() ) ) +
                                                  ")",
                _ => GetNodeName( node )
            };
        private static string GetNodeName( SyntaxNode node )
            => node switch
            {
                AccessorDeclarationSyntax a => a.Keyword.Text,
                MethodDeclarationSyntax m => m.Identifier.Text,
                ClassDeclarationSyntax c => c.Identifier.Text,
                StructDeclarationSyntax s => s.Identifier.Text,
                RecordDeclarationSyntax r => r.Identifier.Text,
                FieldDeclarationSyntax f => string.Join( ",", f.Declaration.Variables.Select( v => v.Identifier.Text ) ),
                EventDeclarationSyntax e => e.Identifier.Text, EventFieldDeclarationSyntax e => string.Join( ",",
                    e.Declaration.Variables.Select( v => v.Identifier.Text ) ),
                PropertyDeclarationSyntax p => p.Identifier.Text,
                NamespaceDeclarationSyntax n => n.Name.ToString(),
                ConstructorDeclarationSyntax c => "ctor",
                _ => node.Kind().ToString()
            };
    }
}