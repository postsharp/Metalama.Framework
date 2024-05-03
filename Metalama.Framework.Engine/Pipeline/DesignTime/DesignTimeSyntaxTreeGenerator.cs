// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Linking;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.SyntaxGeneration;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities.Comparers;
using Metalama.Framework.Engine.Utilities.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using RefKind = Metalama.Framework.Code.RefKind;
using TypeKind = Metalama.Framework.Code.TypeKind;
using VarianceKind = Metalama.Framework.Code.VarianceKind;

namespace Metalama.Framework.Engine.Pipeline.DesignTime
{
    internal static class DesignTimeSyntaxTreeGenerator
    {
        public static async Task<IReadOnlyCollection<IntroducedSyntaxTree>> GenerateDesignTimeSyntaxTreesAsync(
            ProjectServiceProvider serviceProvider,
            PartialCompilation partialCompilation,
            CompilationModel initialCompilationModel,
            CompilationModel finalCompilationModel,
            IReadOnlyCollection<ITransformation> transformations,
            UserDiagnosticSink diagnostics,
            TestableCancellationToken cancellationToken )
        {
            var additionalSyntaxTreeDictionary = new ConcurrentDictionary<string, IntroducedSyntaxTree>();

            var useNullability = partialCompilation.InitialCompilation.Options.NullableContextOptions != NullableContextOptions.Disable;

            var lexicalScopeFactory = new LexicalScopeFactory( finalCompilationModel );
            var injectionHelperProvider = new LinkerInjectionHelperProvider( finalCompilationModel, useNullability );
            var injectionNameProvider = new LinkerInjectionNameProvider( finalCompilationModel, injectionHelperProvider );
            var aspectReferenceSyntaxProvider = new LinkerAspectReferenceSyntaxProvider();

            // Get all observable transformations except replacements, because replacements are not visible at design time.
            var observableTransformations =
                transformations
                    .Where(
                        t => t.Observability == TransformationObservability.Always && t is not IReplaceMemberTransformation
                                                                                   && t.TargetDeclaration is INamedType or IConstructor )
                    .GroupBy(
                        t =>
                            t.TargetDeclaration switch
                            {
                                INamedType namedType => namedType,
                                IConstructor constructor => constructor.DeclaringType,
                                _ => throw new AssertionFailedException( $"Unsupported: {t.TargetDeclaration.DeclarationKind}" )
                            } );

            var taskScheduler = serviceProvider.GetRequiredService<IConcurrentTaskRunner>();

            await taskScheduler.RunConcurrentlyAsync( observableTransformations, ProcessTransformationsOnType, cancellationToken );

            void ProcessTransformationsOnType( IGrouping<INamedType, ITransformation> transformationsOnType )
            {
                cancellationToken.ThrowIfCancellationRequested();
                var declaringType = transformationsOnType.Key;

                if ( !declaringType.IsPartial )
                {
                    // If the type is not marked as partial, we can emit a diagnostic and a code fix, but not a partial class itself.
                    diagnostics.Report(
                        GeneralDiagnosticDescriptors.TypeNotPartial.CreateRoslynDiagnostic( declaringType.GetDiagnosticLocation(), declaringType ) );

                    return;
                }

                var orderedTransformations = transformationsOnType.OrderBy( x => x, TransformationLinkerOrderComparer.Instance );

                // Process members.
                BaseListSyntax? baseList = null;

                var members = List<MemberDeclarationSyntax>();
                var syntaxGenerationContext = finalCompilationModel.CompilationContext.GetSyntaxGenerationContext( SyntaxGenerationOptions.Formatted, true );

                foreach ( var transformation in orderedTransformations )
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if ( transformation is IInjectMemberTransformation injectMemberTransformation )
                    {
                        // TODO: Provide other implementations or allow nulls (because this pipeline should not execute anything).
                        // TODO: Implement support for initializable transformations.
                        var introductionContext = new MemberInjectionContext(
                            serviceProvider,
                            diagnostics,
                            injectionNameProvider,
                            aspectReferenceSyntaxProvider,
                            lexicalScopeFactory,
                            syntaxGenerationContext,
                            finalCompilationModel );

                        var injectedMembers = injectMemberTransformation.GetInjectedMembers( introductionContext )
                            .Select( m => m.Syntax );

                        members = members.AddRange( injectedMembers );
                    }

                    if ( transformation is IInjectInterfaceTransformation injectInterfaceTransformation )
                    {
                        baseList ??= BaseList();
                        baseList = baseList.AddTypes( injectInterfaceTransformation.GetSyntax( syntaxGenerationContext.Options ) );
                    }
                }

                members = members.AddRange(
                    CreateInjectedConstructors( initialCompilationModel, finalCompilationModel, syntaxGenerationContext, transformationsOnType.Key ) );

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

                var index = 1;

                while ( !additionalSyntaxTreeDictionary.TryAdd(
                           syntaxTreeName,
                           new IntroducedSyntaxTree( syntaxTreeName, originalSyntaxTree, generatedSyntaxTree ) ) )
                {
                    index++;
                    syntaxTreeName = $"{declaringType.FullName}_{index}.cs";
                }
            }

            return additionalSyntaxTreeDictionary.Values.AsReadOnly();
        }

        private static IReadOnlyList<ConstructorDeclarationSyntax> CreateInjectedConstructors(
            CompilationModel initialCompilationModel,
            CompilationModel finalCompilationModel,
            SyntaxGenerationContext syntaxGenerationContext,
            INamedType type )
        {
            // TODO: This will not work properly with universal constructor builders.
            var initialType = type.Translate( initialCompilationModel );

            var constructors = new List<ConstructorDeclarationSyntax>();
            var existingSignatures = new HashSet<(ISymbol Type, RefKind RefKind)[]>( new ConstructorSignatureEqualityComparer() );

            // Go through all types that will get generated constructors and index existing constructors.
            foreach ( var constructor in initialType.Constructors )
            {
                existingSignatures.Add(
                    constructor.Parameters.SelectAsArray(
                        p => ((ISymbol) p.Type.GetSymbol().AssertSymbolNullNotImplemented( UnsupportedFeatures.DesignTimeIntroducedTypes ), p.RefKind) ) );
            }

            foreach ( var constructor in type.Constructors )
            {
                var initialConstructor = constructor.Translate( initialCompilationModel );
                var finalConstructor = constructor.Translate( finalCompilationModel );

                // TODO: Currently we don't see introduced parameters in builder code model.
                var finalParameters = finalConstructor.Parameters.ToImmutableArray();

                var initialParameters = initialConstructor.Parameters.ToImmutableArray();

                if ( !existingSignatures.Add(
                        finalParameters.SelectAsArray(
                            p => ((ISymbol) p.Type.GetSymbol().AssertSymbolNullNotImplemented( UnsupportedFeatures.DesignTimeIntroducedTypes ), p.RefKind) ) ) )
                {
                    continue;
                }

                constructors.Add(
                    ConstructorDeclaration(
                        List<AttributeListSyntax>(),
                        finalConstructor.GetSyntaxModifierList(),
                        Identifier( finalConstructor.DeclaringType.Name ),
                        syntaxGenerationContext.SyntaxGenerator.ParameterList( finalParameters, initialCompilationModel ),
                        initialConstructor.IsImplicitlyDeclared
                            ? default
                            : ConstructorInitializer(
                                SyntaxKind.ThisConstructorInitializer,
                                ArgumentList(
                                    SeparatedList(
                                        initialParameters.SelectAsArray(
                                            p =>
                                                Argument(
                                                    p.DefaultValue != null
                                                        ? NameColon( p.Name )
                                                        : null,
                                                    GetArgumentRefToken( p ),
                                                    IdentifierName( p.Name ) ) ) ) ) ),
                        Block() ) );

                if ( initialConstructor.Parameters.Any( p => p.DefaultValue != null ) )
                {
                    // Target constructor has optional parameters.
                    // If there is no constructor without optional parameters, we need to generate it to avoid ambiguous match.

                    var nonOptionalParameters = initialParameters.Where( p => p.DefaultValue == null ).ToArray();
                    var optionalParameters = initialParameters.Where( p => p.DefaultValue != null ).ToArray();

                    if ( existingSignatures.Add(
                            nonOptionalParameters.SelectAsArray(
                                p => ((ISymbol) p.Type.GetSymbol().AssertSymbolNullNotImplemented( UnsupportedFeatures.DesignTimeIntroducedTypes ),
                                      p.RefKind) ) ) )
                    {
                        constructors.Add(
                            ConstructorDeclaration(
                                List<AttributeListSyntax>(),
                                initialConstructor.GetSyntaxModifierList(),
                                Identifier( initialConstructor.DeclaringType.Name ),
                                syntaxGenerationContext.SyntaxGenerator.ParameterList( nonOptionalParameters, initialCompilationModel ),
                                ConstructorInitializer(
                                    SyntaxKind.ThisConstructorInitializer,
                                    ArgumentList(
                                        SeparatedList(
                                                nonOptionalParameters.SelectAsArray(
                                                    p => Argument( null, GetArgumentRefToken( p ), IdentifierName( p.Name ) ) ) )
                                            .AddRange(
                                                optionalParameters.SelectAsArray(
                                                    p =>
                                                        Argument(
                                                            NameColon( p.Name ),
                                                            GetArgumentRefToken( p ),
                                                            LiteralExpression(
                                                                SyntaxKind.DefaultLiteralExpression,
                                                                Token( SyntaxKind.DefaultKeyword ) ) ) ) ) ) ),
                                Block() ) );
                    }
                }
            }

            return constructors;

            static SyntaxToken GetArgumentRefToken( IParameter p )
            {
                return p.RefKind switch
                {
                    RefKind.None or RefKind.In => default,
                    RefKind.Ref or RefKind.RefReadOnly => Token( SyntaxKind.RefKeyword ),
                    RefKind.Out => Token( SyntaxKind.OutKeyword ),
                    _ => throw new AssertionFailedException( $"Unsupported: {p.RefKind}" )
                };
            }
        }

        private static TypeDeclarationSyntax CreatePartialType( INamedType type, BaseListSyntax? baseList, SyntaxList<MemberDeclarationSyntax> members )
            => type.TypeKind switch
            {
                TypeKind.Class => ClassDeclaration(
                    attributeLists: default,
                    SyntaxTokenList.Create( Token( SyntaxKind.PartialKeyword ) ),
                    Identifier( type.Name ),
                    CreateTypeParameters( type ),
                    baseList,
                    constraintClauses: default,
                    members ),
                TypeKind.RecordClass => RecordDeclaration(
                    SyntaxKind.RecordDeclaration,
                    attributeLists: default,
                    SyntaxTokenList.Create( Token( SyntaxKind.PartialKeyword ) ),
                    keyword: Token( SyntaxKind.RecordKeyword ),
                    classOrStructKeyword: Token( SyntaxKind.ClassKeyword ),
                    Identifier( type.Name ),
                    CreateTypeParameters( type ),
                    parameterList: null,
                    baseList,
                    constraintClauses: default,
                    openBraceToken: Token( SyntaxKind.OpenBraceToken ),
                    members,
                    closeBraceToken: Token( SyntaxKind.CloseBraceToken ),
                    semicolonToken: default ),
                TypeKind.Struct => StructDeclaration(
                    attributeLists: default,
                    SyntaxTokenList.Create( Token( SyntaxKind.PartialKeyword ) ),
                    Identifier( type.Name ),
                    CreateTypeParameters( type ),
                    baseList,
                    constraintClauses: default,
                    members ),
                TypeKind.RecordStruct => RecordDeclaration(
                    SyntaxKind.RecordStructDeclaration,
                    attributeLists: default,
                    SyntaxTokenList.Create( Token( SyntaxKind.PartialKeyword ) ),
                    keyword: Token( SyntaxKind.RecordKeyword ),
                    classOrStructKeyword: Token( SyntaxKind.StructKeyword ),
                    Identifier( type.Name ),
                    CreateTypeParameters( type ),
                    parameterList: null,
                    baseList,
                    constraintClauses: default,
                    openBraceToken: Token( SyntaxKind.OpenBraceToken ),
                    members,
                    closeBraceToken: Token( SyntaxKind.CloseBraceToken ),
                    semicolonToken: default ),
                TypeKind.Interface => InterfaceDeclaration(
                    attributeLists: default,
                    SyntaxTokenList.Create( Token( SyntaxKind.PartialKeyword ) ),
                    Identifier( type.Name ),
                    CreateTypeParameters( type ),
                    baseList,
                    constraintClauses: default,
                    members ),
                _ => throw new AssertionFailedException( $"Unknown type kind: {type.TypeKind}." )
            };

        private static TypeParameterListSyntax? CreateTypeParameters( INamedType type )
        {
            if ( !type.IsGeneric )
            {
                return null;
            }

            static SyntaxKind GetVariance( VarianceKind variance )
            {
                return variance switch
                {
                    VarianceKind.None => SyntaxKind.None,
                    VarianceKind.In => SyntaxKind.InKeyword,
                    VarianceKind.Out => SyntaxKind.OutKeyword,
                    _ => throw new AssertionFailedException( $"Unknown variance: {variance}." )
                };
            }

            return TypeParameterList(
                SeparatedList(
                    type.TypeParameters.SelectAsReadOnlyList( tp => TypeParameter( tp.Name ).WithVarianceKeyword( Token( GetVariance( tp.Variance ) ) ) ) ) );
        }

        private static MemberDeclarationSyntax AddHeader( MemberDeclarationSyntax node )
            => node switch
            {
                NamespaceDeclarationSyntax or ClassDeclarationSyntax or StructDeclarationSyntax or RecordDeclarationSyntax or InterfaceDeclarationSyntax =>
                    node.WithLeadingTrivia( GetHeader() ),
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

        private sealed class ConstructorSignatureEqualityComparer : IEqualityComparer<(ISymbol Type, RefKind RefKind)[]>
        {
            private readonly StructuralSymbolComparer _symbolComparer = StructuralSymbolComparer.Default;

            public bool Equals( (ISymbol Type, RefKind RefKind)[]? x, (ISymbol Type, RefKind RefKind)[]? y )
            {
                if ( x == null || y == null )
                {
                    return x == null && y == null;
                }

                if ( x.Length != y.Length )
                {
                    return false;
                }

                for ( var i = 0; i < x.Length; i++ )
                {
                    if ( x[i].RefKind != y[i].RefKind )
                    {
                        return false;
                    }

                    if ( !this._symbolComparer.Equals( x[i].Type, y[i].Type ) )
                    {
                        return false;
                    }
                }

                return true;
            }

            public int GetHashCode( (ISymbol Type, RefKind RefKind)[] obj )
            {
                var hashCode = obj.Length;

                for ( var i = 0; i < obj.Length; i++ )
                {
                    hashCode = HashCode.Combine( hashCode, this._symbolComparer.GetHashCode( obj[i].Type ), (int) obj[i].RefKind );
                }

                return hashCode;
            }
        }
    }
}