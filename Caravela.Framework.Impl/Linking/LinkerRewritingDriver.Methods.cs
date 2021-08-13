// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Formatting;
using Caravela.Framework.Impl.Linking.Inlining;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Linking
{
    internal partial class LinkerRewritingDriver
    {
        /// <summary>
        /// Determines whether the method will be discarded in the final compilation (unreferenced or inlined declarations).
        /// </summary>
        /// <param name="referencedMethod">Override method symbol or overridden method symbol.</param>
        /// <returns></returns>
        private bool IsDiscarded( IMethodSymbol referencedMethod, ResolvedAspectReferenceSemantic semantic )
        {
            if ( referencedMethod.MethodKind != MethodKind.Ordinary )
            {
                throw new AssertionFailedException();
            }

            if ( this._analysisRegistry.IsOverride( referencedMethod ) )
            {
                var aspectReferences = this._analysisRegistry.GetAspectReferences( referencedMethod, semantic );
                var overrideTarget = this._analysisRegistry.GetOverrideTarget( referencedMethod );
                var lastOverride = this._analysisRegistry.GetLastOverride( overrideTarget.AssertNotNull() );

                if ( SymbolEqualityComparer.Default.Equals( referencedMethod, lastOverride ) )
                {
                    return this.IsInlineable( referencedMethod, semantic );
                }
                else
                {
                    return this.IsInlineable( referencedMethod, semantic ) || aspectReferences.Count == 0;
                }
            }
            else
            {
                return false;
            }
        }

        private bool IsInlineable( IMethodSymbol inlinedMethod, ResolvedAspectReferenceSemantic semantic )
        {
            switch ( inlinedMethod.MethodKind )
            {
                case MethodKind.Ordinary:
                case MethodKind.ExplicitInterfaceImplementation:
                    if ( GetDeclarationFlags( inlinedMethod ).HasFlag( LinkerDeclarationFlags.NotInlineable ) )
                    {
                        return false;
                    }

                    if ( this._analysisRegistry.IsLastOverride( inlinedMethod ) )
                    {
                        // TODO: Seems weird to return true here, what if a condition later returns false?
                        return true;
                    }

                    var aspectReferences = this._analysisRegistry.GetAspectReferences( inlinedMethod, semantic );

                    if ( aspectReferences.Count != 1 )
                    {
                        return false;
                    }

                    return this.IsInlineableReference( aspectReferences[0], MethodKind.Ordinary );

                default:
                    throw new AssertionFailedException();
            }
        }

        private bool HasAnyAspectReferences( IMethodSymbol symbol, ResolvedAspectReferenceSemantic semantic )
            => this._analysisRegistry.GetAspectReferences( symbol, semantic ).Count > 0;

        private IReadOnlyList<MemberDeclarationSyntax> RewriteMethod( MethodDeclarationSyntax methodDeclaration, IMethodSymbol symbol )
        {
            if ( this._analysisRegistry.IsOverrideTarget( symbol ) )
            {
                var members = new List<MemberDeclarationSyntax>();
                var lastOverride = (IMethodSymbol) this._analysisRegistry.GetLastOverride( symbol );

                if ( this.IsInlineable( lastOverride, ResolvedAspectReferenceSemantic.Default ) )
                {
                    members.Add( GetLinkedDeclaration( lastOverride.IsAsync ) );
                }
                else
                {
                    members.Add( GetTrampolineMethod( methodDeclaration, lastOverride ) );
                }

                if ( !this.IsInlineable( symbol, ResolvedAspectReferenceSemantic.Original )
                     && this.HasAnyAspectReferences( symbol, ResolvedAspectReferenceSemantic.Original ) )
                {
                    members.Add( this.GetOriginalImplMethod( methodDeclaration, symbol ) );
                }

                return members;
            }
            else if ( this._analysisRegistry.IsOverride( symbol ) )
            {
                if ( this.IsDiscarded( (ISymbol) symbol, ResolvedAspectReferenceSemantic.Default ) )
                {
                    return Array.Empty<MemberDeclarationSyntax>();
                }

                return new[] { GetLinkedDeclaration( symbol.IsAsync ) };
            }
            else
            {
                throw new AssertionFailedException();
            }

            MethodDeclarationSyntax GetLinkedDeclaration( bool isAsync )
            {
                var linkedBody = this.GetLinkedBody(
                    this.GetBodySource( symbol ),
                    InliningContext.Create( this, symbol ) );

                var modifiers = methodDeclaration.Modifiers;

                if ( isAsync && !symbol.IsAsync )
                {
                    modifiers = modifiers.Add( Token( TriviaList( ElasticSpace ), SyntaxKind.AsyncKeyword, TriviaList( ElasticSpace ) ) );
                }
                else if ( !isAsync && symbol.IsAsync )
                {
                    modifiers = TokenList( modifiers.Where( m => m.Kind() != SyntaxKind.AsyncKeyword ) );
                }

                return methodDeclaration
                    .WithExpressionBody( null )
                    .WithModifiers( modifiers )
                    .WithBody( linkedBody )
                    .WithLeadingTrivia( methodDeclaration.GetLeadingTrivia() )
                    .WithTrailingTrivia( methodDeclaration.GetTrailingTrivia() );
            }
        }

        private MemberDeclarationSyntax GetOriginalImplMethod( MethodDeclarationSyntax method, IMethodSymbol symbol )
        {
            var returnType = AsyncHelper.GetIntermediateMethodReturnType( this.IntermediateCompilation, symbol, method.ReturnType );

            var modifiers = symbol
                .GetSyntaxModifierList( ModifierCategories.Static | ModifierCategories.Unsafe | ModifierCategories.Async )
                .Insert( 0, Token( SyntaxKind.PrivateKeyword ) );

            return
                MethodDeclaration(
                        List<AttributeListSyntax>(),
                        modifiers,
                        returnType,
                        null,
                        Identifier( GetOriginalImplMemberName( symbol ) ),
                        method.TypeParameterList,
                        method.ParameterList,
                        method.ConstraintClauses,
                        null,
                        null )
                    .NormalizeWhitespace()
                    .WithLeadingTrivia( ElasticLineFeed )
                    .WithTrailingTrivia( ElasticLineFeed )
                    .WithBody( method.Body.AddSourceCodeAnnotation() )
                    .WithExpressionBody( method.ExpressionBody.AddSourceCodeAnnotation() )
                    .AddGeneratedCodeAnnotation();
        }
    }
}