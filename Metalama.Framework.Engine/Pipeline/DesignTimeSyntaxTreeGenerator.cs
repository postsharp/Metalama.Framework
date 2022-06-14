// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Linking;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using TypeKind = Metalama.Framework.Code.TypeKind;

namespace Metalama.Framework.Engine.Pipeline
{
    internal static class DesignTimeSyntaxTreeGenerator
    {
        public static void GenerateDesignTimeSyntaxTrees(
            PartialCompilation partialCompilation,
            CompilationModel compilationModel,
            IEnumerable<ITransformation> transformations,
            IServiceProvider serviceProvider,
            UserDiagnosticSink diagnostics,
            CancellationToken cancellationToken,
            out IReadOnlyList<IntroducedSyntaxTree> additionalSyntaxTrees )
        {
            var additionalSyntaxTreeList = new List<IntroducedSyntaxTree>();
            additionalSyntaxTrees = additionalSyntaxTreeList;

            LexicalScopeFactory lexicalScopeFactory = new( compilationModel );
            var introductionNameProvider = new LinkerIntroductionNameProvider();

            // Get all observable transformations except replacements, because replacements are not visible at design time.
            var observableTransformations = transformations.Where( t => t is IObservableTransformation and not IReplaceMemberTransformation );

            foreach ( var transformationGroup in
                     observableTransformations.GroupBy( t => ((IObservableTransformation) t).ContainingDeclaration ) )
            {
                cancellationToken.ThrowIfCancellationRequested();

                if ( transformationGroup.Key is not INamedType declaringType )
                {
                    // We only support introductions to types.
                    continue;
                }

                if ( !declaringType.IsPartial )
                {
                    // If the type is not marked as partial, we can emit a diagnostic and a code fix, but not a partial class itself.
                    diagnostics.Report(
                        GeneralDiagnosticDescriptors.TypeNotPartial.CreateRoslynDiagnostic( declaringType.GetDiagnosticLocation(), declaringType ) );

                    continue;
                }

                // Process members.
                BaseListSyntax? baseList = null;

                var members = List<MemberDeclarationSyntax>();
                var syntaxGenerationContext = SyntaxGenerationContext.Create( serviceProvider, partialCompilation.Compilation, true );

                foreach ( var transformation in transformationGroup )
                {
                    if ( transformation is IIntroduceMemberTransformation memberIntroduction )
                    {
                        // TODO: Provide other implementations or allow nulls (because this pipeline should not execute anything).
                        // TODO: Implement support for initializable transformations.
                        var introductionContext = new MemberIntroductionContext(
                            diagnostics,
                            introductionNameProvider,
                            lexicalScopeFactory,
                            syntaxGenerationContext,
                            serviceProvider,
                            compilationModel );

                        var introducedMembers = memberIntroduction.GetIntroducedMembers( introductionContext )
                            .Select( m => m.Syntax.NormalizeWhitespace() );

                        members = members.AddRange( introducedMembers );
                    }

                    if ( transformation is IIntroduceInterfaceTransformation interfaceImplementation )
                    {
                        baseList ??= BaseList();
                        baseList = baseList.AddTypes( interfaceImplementation.GetSyntax() );
                    }
                }

                // Create a class.
                var classDeclaration = CreatePartialType( declaringType, baseList, members );

                // Add the class to its nesting type.
                var topDeclaration = (MemberDeclarationSyntax) classDeclaration;

                for ( var containingType = declaringType.DeclaringType; containingType != null; containingType = containingType.DeclaringType )
                {
                    topDeclaration = CreatePartialType(
                        containingType,
                        default,
                        SingletonList( topDeclaration ) );
                }

                // Add the class to a namespace.
                if ( !declaringType.Namespace.IsGlobalNamespace )
                {
                    topDeclaration = NamespaceDeclaration(
                        ParseName( declaringType.Namespace.FullName ),
                        default,
                        default,
                        SingletonList( topDeclaration ) );
                }

                // Choose the best syntax tree
                var originalSyntaxTree = ((IDeclarationImpl) declaringType).DeclaringSyntaxReferences.Select( r => r.SyntaxTree )
                    .OrderBy( s => s.FilePath.Length )
                    .First();

                var compilationUnit = CompilationUnit()
                    .WithMembers( SingletonList( AddHeader( topDeclaration ) ) );

                var generatedSyntaxTree = SyntaxTree( compilationUnit.NormalizeWhitespace(), encoding: Encoding.UTF8 );
                var syntaxTreeName = declaringType.FullName + ".cs";

                additionalSyntaxTreeList.Add( new IntroducedSyntaxTree( syntaxTreeName, originalSyntaxTree, generatedSyntaxTree ) );
            }
        }

        private static TypeDeclarationSyntax CreatePartialType( INamedType type, BaseListSyntax? baseList, SyntaxList<MemberDeclarationSyntax> members )
            => type.TypeKind switch
            {
                TypeKind.Class => ClassDeclaration(
                    default,
                    SyntaxTokenList.Create( Token( SyntaxKind.PartialKeyword ) ),
                    Identifier( type.Name ),
                    null,
                    baseList,
                    default,
                    members ),
                TypeKind.RecordClass => RecordDeclaration(
                        default,
                        SyntaxTokenList.Create( Token( SyntaxKind.PartialKeyword ) ),
                        Token( SyntaxKind.RecordKeyword ),
                        Identifier( type.Name ),
                        null!,
                        null!,
                        baseList!,
                        default,
                        members )
                    .WithOpenBraceToken( Token( SyntaxKind.OpenBraceToken ) )
                    .WithCloseBraceToken( Token( SyntaxKind.CloseBraceToken ) )
                    .WithClassOrStructKeyword( Token( SyntaxKind.ClassKeyword ) ),
                TypeKind.Struct => StructDeclaration(
                    default,
                    SyntaxTokenList.Create( Token( SyntaxKind.PartialKeyword ) ),
                    Identifier( type.Name ),
                    null,
                    baseList,
                    default,
                    members ),
                TypeKind.RecordStruct => RecordDeclaration(
                        default,
                        SyntaxTokenList.Create( Token( SyntaxKind.PartialKeyword ) ),
                        Token( SyntaxKind.RecordKeyword ),
                        Identifier( type.Name ),
                        null!,
                        null!,
                        baseList!,
                        default,
                        members )
                    .WithOpenBraceToken( Token( SyntaxKind.OpenBraceToken ) )
                    .WithCloseBraceToken( Token( SyntaxKind.CloseBraceToken ) )
                    .WithClassOrStructKeyword( Token( SyntaxKind.StructKeyword ) ),
                _ => throw new ArgumentOutOfRangeException( nameof(type) )
            };

        private static MemberDeclarationSyntax AddHeader( MemberDeclarationSyntax node )
            => node switch
            {
                NamespaceDeclarationSyntax ns => ns.WithLeadingTrivia( GetHeader() ),
                ClassDeclarationSyntax c => c.WithLeadingTrivia( GetHeader() ),
                StructDeclarationSyntax s => s.WithLeadingTrivia( GetHeader() ),
                RecordDeclarationSyntax r => r.WithLeadingTrivia( GetHeader() ),
                _ => node
            };

        private static SyntaxTriviaList GetHeader()
        {
            const string generatedByMetalama = " Generated by Metalama to support the code editing experience. This is NOT the code that gets executed.";

            return TriviaList(
                Trivia(
                    DocumentationCommentTrivia(
                        SyntaxKind.SingleLineDocumentationCommentTrivia,
                        List(
                            new XmlNodeSyntax[]
                            {
                                XmlText()
                                    .WithTextTokens(
                                        TokenList(
                                            XmlTextLiteral(
                                                TriviaList( DocumentationCommentExterior( "///" ) ),
                                                " ",
                                                " ",
                                                TriviaList() ) ) ),
                                XmlExampleElement(
                                        SingletonList<XmlNodeSyntax>(
                                            XmlText()
                                                .WithTextTokens(
                                                    TokenList(
                                                        XmlTextNewLine(
                                                            TriviaList(),
                                                            "\n",
                                                            "\n",
                                                            TriviaList() ),
                                                        XmlTextLiteral(
                                                            TriviaList( DocumentationCommentExterior( "///" ) ),
                                                            generatedByMetalama,
                                                            generatedByMetalama,
                                                            TriviaList() ),
                                                        XmlTextNewLine(
                                                            TriviaList(),
                                                            "\n",
                                                            "\n",
                                                            TriviaList() ),
                                                        XmlTextLiteral(
                                                            TriviaList( DocumentationCommentExterior( "///" ) ),
                                                            " ",
                                                            " ",
                                                            TriviaList() ) ) ) ) )
                                    .WithStartTag( XmlElementStartTag( XmlName( Identifier( "generated" ) ) ) )
                                    .WithEndTag(
                                        XmlElementEndTag( XmlName( Identifier( "generated" ) ) )
                                            .WithGreaterThanToken( Token( SyntaxKind.GreaterThanToken ) ) ),
                                XmlText()
                                    .WithTextTokens(
                                        TokenList(
                                            XmlTextNewLine(
                                                TriviaList(),
                                                "\n",
                                                "\n",
                                                TriviaList() ) ) )
                            } ) ) ),
                LineFeed,
                LineFeed );
        }
    }
}