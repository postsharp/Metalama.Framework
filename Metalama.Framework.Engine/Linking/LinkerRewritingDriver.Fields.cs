// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Templating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Xml.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Linking
{
    internal sealed partial class LinkerRewritingDriver
    {
        private IReadOnlyList<MemberDeclarationSyntax> RewriteField(
            FieldDeclarationSyntax fieldDeclaration,
            IFieldSymbol symbol,
            SyntaxGenerationContext generationContext )
        {
            Invariant.Assert( !this.InjectionRegistry.IsOverrideTarget( symbol ) );

            var members = new List<MemberDeclarationSyntax>();

            if ( this.AnalysisRegistry.IsReachable( symbol.ToSemantic( IntermediateSymbolSemanticKind.Base ) )
                 && !this.AnalysisRegistry.IsInlined( symbol.ToSemantic( IntermediateSymbolSemanticKind.Base ) )
                 && this.ShouldGenerateEmptyMember( symbol ) )
            {
                members.Add(
                    this.GetEmptyImplField(
                        symbol,
                        List<AttributeListSyntax>(),
                        fieldDeclaration.Declaration.Type ) );
            }

            members.Add( fieldDeclaration );

            return members;
        }

        private MemberDeclarationSyntax GetEmptyImplField(
            IFieldSymbol symbol,
            SyntaxList<AttributeListSyntax> attributes,
            TypeSyntax type )
        {
            var setAccessorKind =
                symbol switch
                {
                    { IsReadOnly: false } => SyntaxKind.SetAccessorDeclaration,
                    { IsReadOnly: true } => SyntaxKind.InitAccessorDeclaration,
                };

            var accessorList =
                AccessorList(
                        List(
                            new[] 
                            {
                                AccessorDeclaration(
                                    SyntaxKind.GetAccessorDeclaration,
                                    List<AttributeListSyntax>(),
                                    TokenList(),
                                    Token( SyntaxKind.GetKeyword ),
                                    null,
                                    ArrowExpressionClause( DefaultExpression( type ) ),
                                    Token( SyntaxKind.SemicolonToken ) ),
                                AccessorDeclaration(
                                    setAccessorKind,
                                    SyntaxFactoryEx.FormattedBlock() ) 
                            } ) )
                    .NormalizeWhitespace();
            return
                PropertyDeclaration(
                    attributes,
                    symbol.IsStatic
                        ? TokenList(
                            Token( SyntaxKind.PrivateKeyword ).WithTrailingTrivia( Space ),
                            Token( SyntaxKind.StaticKeyword ).WithTrailingTrivia( Space ) )
                        : TokenList( Token( SyntaxKind.PrivateKeyword ).WithTrailingTrivia( Space ) ),
                    type,
                    null,
                    Identifier( GetEmptyImplMemberName( symbol ) ),
                    null,
                    null,
                    null )
                .NormalizeWhitespace()
                .WithLeadingTrivia( ElasticLineFeed )
                .WithAccessorList( accessorList.WithTrailingTrivia( ElasticLineFeed ) )
                .WithGeneratedCodeAnnotation( FormattingAnnotations.SystemGeneratedCodeAnnotation );
        }
    }
}