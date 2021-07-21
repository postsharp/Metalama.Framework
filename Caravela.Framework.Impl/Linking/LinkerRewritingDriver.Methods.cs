// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Formatting;
using Caravela.Framework.Impl.Linking.Inlining;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Linking
{
    internal partial class LinkerRewritingDriver
    {
        /// <summary>
        /// Determines whether the method will be discarded in the final compilation (unreferenced or inlined declarations).
        /// </summary>
        /// <param name="symbol">Override method symbol or overridden method symbol.</param>
        /// <returns></returns>
        private bool IsDiscarded( IMethodSymbol symbol, ResolvedAspectReferenceSemantic semantic )
        {
            if ( symbol.MethodKind != MethodKind.Ordinary )
            {
                throw new AssertionFailedException();
            }

            if ( this._analysisRegistry.IsOverride( symbol ) )
            {
                var aspectReferences = this._analysisRegistry.GetAspectReferences( symbol, semantic );
                var overrideTarget = this._analysisRegistry.GetOverrideTarget( symbol );
                var lastOverride = this._analysisRegistry.GetLastOverride( overrideTarget.AssertNotNull() );

                if ( SymbolEqualityComparer.Default.Equals( symbol, lastOverride ) )
                {
                    return this.IsInlineable( symbol, semantic );
                }
                else
                {
                    return this.IsInlineable( symbol, semantic ) || aspectReferences.Count == 0;
                }
            }
            else
            {
                return false;
            }
        }

        private bool IsInlineable( IMethodSymbol symbol, ResolvedAspectReferenceSemantic semantic )
        {
            switch ( symbol.MethodKind )
            {
                case MethodKind.Ordinary:
                case MethodKind.ExplicitInterfaceImplementation:
                    if ( GetDeclarationFlags( symbol ).HasFlag( LinkerDeclarationFlags.NotInlineable ) )
                    {
                        return false;
                    }

                    if ( this._analysisRegistry.IsLastOverride( symbol ) )
                    {
                        return true;
                    }

                    var aspectReferences = this._analysisRegistry.GetAspectReferences( symbol, semantic );

                    if ( aspectReferences.Count != 1 )
                    {
                        return false;
                    }

                    return this.IsInlineableReference( aspectReferences[0] );

                default:
                    throw new AssertionFailedException();
            }
        }

        private bool HasAnyAspectReferences( IMethodSymbol symbol, ResolvedAspectReferenceSemantic semantic )
        {
            return this._analysisRegistry.GetAspectReferences( symbol, semantic ).Count > 0;
        }

        public IReadOnlyList<MemberDeclarationSyntax> RewriteMethod( MethodDeclarationSyntax methodDeclaration, IMethodSymbol symbol )
        {
            if ( this._analysisRegistry.IsOverrideTarget( symbol ) )
            {
                var members = new List<MemberDeclarationSyntax>();
                var lastOverride = (IMethodSymbol) this._analysisRegistry.GetLastOverride( symbol );

                if ( this.IsInlineable( lastOverride, ResolvedAspectReferenceSemantic.Default ) )
                {
                    members.Add( GetLinkedDeclaration() );
                }
                else
                {
                    members.Add( GetTrampolineMethod( methodDeclaration, lastOverride ) );
                }

                if ( !this.IsInlineable( symbol, ResolvedAspectReferenceSemantic.Original )
                     && this.HasAnyAspectReferences( symbol, ResolvedAspectReferenceSemantic.Original ) )
                {
                    members.Add( GetOriginalImplMethod( methodDeclaration, symbol ) );
                }

                return members;
            }
            else if ( this._analysisRegistry.IsOverride( symbol ) )
            {
                if ( this.IsDiscarded( symbol, ResolvedAspectReferenceSemantic.Default ) )
                {
                    return Array.Empty<MemberDeclarationSyntax>();
                }

                return new[] { GetLinkedDeclaration() };
            }
            else
            {
                throw new AssertionFailedException();
            }

            MethodDeclarationSyntax GetLinkedDeclaration()
            {
                return methodDeclaration
                    .WithExpressionBody( null )
                    .WithBody(
                        this.GetLinkedBody(
                            this.GetBodySource( symbol ),
                            InliningContext.Create( this, symbol ) ) )
                    .WithLeadingTrivia( methodDeclaration.GetLeadingTrivia() )
                    .WithTrailingTrivia( methodDeclaration.GetTrailingTrivia() );
            }
        }

        private static MemberDeclarationSyntax GetOriginalImplMethod( MethodDeclarationSyntax method, IMethodSymbol symbol )
        {
            return
                MethodDeclaration(
                        List<AttributeListSyntax>(),
                        symbol.IsStatic
                            ? TokenList( Token( SyntaxKind.PrivateKeyword ), Token( SyntaxKind.StaticKeyword ) )
                            : TokenList( Token( SyntaxKind.PrivateKeyword ) ),
                        method.ReturnType,
                        null,
                        Identifier( GetOriginalImplMemberName( method.Identifier.ValueText ) ),
                        method.TypeParameterList,
                        method.ParameterList,
                        method.ConstraintClauses,
                        null,
                        null )
                    .NormalizeWhitespace()
                    .WithLeadingTrivia( ElasticLineFeed )
                    .WithTrailingTrivia( ElasticLineFeed )
                    .WithBody( method.Body )
                    .WithExpressionBody( method.ExpressionBody )
                    .AddGeneratedCodeAnnotation();
        }
    }
}