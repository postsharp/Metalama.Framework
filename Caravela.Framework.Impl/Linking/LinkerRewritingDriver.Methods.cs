// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Linking.Inlining;
using Microsoft.CodeAnalysis;
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
        private bool IsDiscarded( IMethodSymbol symbol )
        {
            if ( symbol.MethodKind != MethodKind.Ordinary )
            {
                throw new AssertionFailedException();
            }

            if (this._analysisRegistry.IsOverride(symbol))
            {
                var aspectReferences = this._analysisRegistry.GetAspectReferences( symbol );
                var overrideTarget = this._analysisRegistry.GetOverrideTarget( symbol );
                var lastOverride = this._analysisRegistry.GetLastOverride( overrideTarget.AssertNotNull() );

                if (SymbolEqualityComparer.Default.Equals(symbol, lastOverride))
                {
                    return this.IsInlineable( symbol ) && !this.GetLinkerOptions( symbol ).ForceNotDiscardable;
                }
                else
                {
                    return (this.IsInlineable( symbol ) || aspectReferences.Count == 0) && !this.GetLinkerOptions( symbol ).ForceNotDiscardable;
                }
            }
            else
            {
                return false;
            }
        }

        private bool IsInlineable( IMethodSymbol symbol )
        {
            if ( symbol.MethodKind != MethodKind.Ordinary )
            {
                throw new AssertionFailedException();
            }

            if ( this.GetLinkerOptions( symbol ).ForceNotInlineable )
            {
                return false;
            }

            var aspectReferences = this._analysisRegistry.GetAspectReferences( symbol );

            if ( aspectReferences.Count > 1 )
            {
                return false;
            }

            return aspectReferences.Count == 0 || this.IsInlineableReference( aspectReferences[0] );
        }

        public IReadOnlyList<MemberDeclarationSyntax> RewriteMethod( MethodDeclarationSyntax methodDeclaration, IMethodSymbol symbol )
        {
            if ( this._analysisRegistry.IsOverrideTarget( symbol ) )
            {
                var members = new List<MemberDeclarationSyntax>();
                var lastOverride = (IMethodSymbol) this._analysisRegistry.GetLastOverride( symbol );

                if ( this.IsInlineable( lastOverride ) )
                {
                    members.Add( GetLinkedDeclaration() );
                }
                else
                {
                    members.Add( GetTrampolineMethod( methodDeclaration, lastOverride ) );
                }

                if (!this.IsInlineable(symbol))
                {
                    members.Add( GetOriginalImplMethod( methodDeclaration ) );
                }

                return members;
            }
            else if (this._analysisRegistry.IsOverride(symbol))
            {
                if ( this.IsDiscarded( symbol ) )
                {
                    return Array.Empty<MemberDeclarationSyntax>();
                }

                return new[]
                {
                    GetLinkedDeclaration()
                };
            }
            else
            {
                throw new AssertionFailedException();
            }

            MethodDeclarationSyntax GetLinkedDeclaration()
            {
                return methodDeclaration
                    .WithBody( 
                        this.GetLinkedBody( 
                            this.GetBodySource( symbol ), 
                            InliningContext.Create( this, symbol ) ) )
                    .WithLeadingTrivia( methodDeclaration.GetLeadingTrivia() )
                    .WithTrailingTrivia( methodDeclaration.GetTrailingTrivia() );
            }
        }

        private static MemberDeclarationSyntax GetOriginalImplMethod( MethodDeclarationSyntax method )
        {
            return method.WithIdentifier( Identifier( GetOriginalImplMemberName( method.Identifier.ValueText ) ) );
        }
    }
}
