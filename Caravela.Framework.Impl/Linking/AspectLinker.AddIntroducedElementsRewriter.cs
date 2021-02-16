using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Linking
{
    internal partial class AspectLinker
    {
        public class AddIntroducedElementsRewriter : CSharpSyntaxRewriter
        {
            private readonly IReadOnlyList<IMemberIntroduction> _memberIntroductors;
            private readonly IReadOnlyList<IInterfaceImplementationIntroduction> _interfaceImplementationIntroductors;
            private readonly ImmutableMultiValueDictionary<MemberDeclarationSyntax, IntroducedMember> _introducedMemberLookup;

            public ImmutableMultiValueDictionary<ICodeElement, IntroducedMember> ElementOverrides { get; }

            public AddIntroducedElementsRewriter( IEnumerable<ISyntaxTreeIntroduction> introductions ) : base()
            {
                this._memberIntroductors = introductions.OfType<IMemberIntroduction>().ToList();
                this._interfaceImplementationIntroductors = introductions.OfType<IInterfaceImplementationIntroduction>().ToList();

                var introducedMembers = this._memberIntroductors
                    .SelectMany( t => t.GetIntroducedMembers().Select( x => (Introductor: t, Introduced: x, InsertPosition: t.InsertPositionNode) ) )
                    .ToList();

                this._introducedMemberLookup = introducedMembers.ToMultiValueDictionary( x => x.Introductor.InsertPositionNode, x => x.Introduced );
            }

            public override SyntaxNode? VisitClassDeclaration( ClassDeclarationSyntax node )
            {
                var members = new List<MemberDeclarationSyntax>( node.Members.Count );

                foreach ( var member in node.Members )
                {
                    members.Add( member );
                    members.AddRange( this._introducedMemberLookup[member].Select( i => i.Syntax ) );
                }

                members.AddRange( this._introducedMemberLookup[node].Select( i => i.Syntax ) );

                return node.WithMembers( List( members ) );
            }
        }
    }
}
