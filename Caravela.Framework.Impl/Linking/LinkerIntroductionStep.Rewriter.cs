// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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
            private readonly IntroducedMemberCollection _introducedMemberCollection;

            public Rewriter( IntroducedMemberCollection introducedMemberCollection )
            {
                this._introducedMemberCollection = introducedMemberCollection;
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
                    foreach ( var introducedMember in this._introducedMemberCollection.GetIntroducedMembersOnPosition( position ) )
                    {
                        // Allow for tracking of the node inserted.
                        // IMPORTANT: This need to be here and cannot be in introducedMember.Syntax, result of TrackNodes is not trackable!
                        members.Add( introducedMember.Syntax.TrackNodes( introducedMember.Syntax ) );
                    }
                }
            }
        }
    }
}