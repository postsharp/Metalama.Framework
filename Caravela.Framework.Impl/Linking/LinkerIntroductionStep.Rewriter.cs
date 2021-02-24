using Caravela.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Linking
{

    internal partial class LinkerIntroductionStep
    {
        private class Rewriter : CSharpSyntaxRewriter
        {
            private readonly DiagnosticSink _diagnosticSink;
            private readonly LinkerTransformationRegistry _introductionRegistry;

            public Rewriter( LinkerTransformationRegistry introductionRegistry, DiagnosticSink diagnosticSink ) : base()
            {
                this._diagnosticSink = diagnosticSink;
                this._introductionRegistry = introductionRegistry;
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
                    var introducedSyntaxNodes = this._introductionRegistry.GetIntroducedSyntaxNodesOnPosition( position );
                    members.AddRange( introducedSyntaxNodes );
                }
            }
        }
    }
}
