// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using Caravela.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Linking
{

    internal partial class LinkerIntroductionStep
    {
        private class Rewriter : CSharpSyntaxRewriter
        {
            private readonly DiagnosticSink _diagnosticSink;
            private readonly LinkerTransformationRegistry _transformationRegistry;

            public Rewriter( LinkerTransformationRegistry transformationRegistry, DiagnosticSink diagnosticSink ) : base()
            {
                this._diagnosticSink = diagnosticSink;
                this._transformationRegistry = transformationRegistry;
            }

            public override SyntaxNode? VisitClassDeclaration( ClassDeclarationSyntax node )
            {
                var members = new List<MemberDeclarationSyntax>( node.Members.Count );

                foreach ( var member in node.Members )
                {
                    members.Add( member );



                    AddIntroductionsOnPosition( members, member );
                }

                AddIntroductionsOnPosition( members, node );

                return node.WithMembers( List( members ) );

                void AddIntroductionsOnPosition( List<MemberDeclarationSyntax> members, MemberDeclarationSyntax position )
                {
                    var introducedSyntaxNodes = this._transformationRegistry.GetIntroducedSyntaxNodesOnPosition( position );

                    members.AddRange( introducedSyntaxNodes );
                }
            }
        }
    }
}
