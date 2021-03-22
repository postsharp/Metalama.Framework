// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.Diagnostics;
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
            private static int _id;
            private readonly DiagnosticSink _diagnosticSink;

            private readonly IReadOnlyList<IMemberIntroduction> _memberIntroductors;
            private readonly IReadOnlyList<IInterfaceImplementationIntroduction> _interfaceImplementationIntroductors;
            private readonly ImmutableMultiValueDictionary<MemberDeclarationSyntax, IntroducedMember> _introducedMemberLookup;

            public ImmutableMultiValueDictionary<IMemberIntroduction, int> IntroducedSyntax { get; private set; }

            public AddIntroducedElementsRewriter( IEnumerable<ISyntaxTreeTransformation> introductions, DiagnosticSink diagnosticSink )
            {
                this._diagnosticSink = diagnosticSink;
                this._memberIntroductors = introductions.OfType<IMemberIntroduction>().ToList();
                this._interfaceImplementationIntroductors = introductions.OfType<IInterfaceImplementationIntroduction>().ToList();

                var introducedMembers = this._memberIntroductors
                    .SelectMany( t => t.GetIntroducedMembers( new MemberIntroductionContext( diagnosticSink ) ).Select( x => (Introductor: t, Introduced: x) ) )
                    .ToList();

                this._introducedMemberLookup = introducedMembers.ToMultiValueDictionary( x => x.Introductor.InsertPositionNode, x => x.Introduced );
                this.IntroducedSyntax = ImmutableMultiValueDictionary<IMemberIntroduction, int>.Empty;
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
                    var introductorSyntaxPairs = this._introducedMemberLookup[position].Select( i => (i.Introductor, i.Syntax, Id: _id++) ).ToList();
                    this.IntroducedSyntax = this.IntroducedSyntax.AddRange( introductorSyntaxPairs, x => x.Introductor, x => x.Id );

                    members.AddRange( introductorSyntaxPairs.Select( x => x.Syntax.WithAdditionalAnnotations( new SyntaxAnnotation( _introducedSyntaxAnnotationId, x.Id.ToString() ) ) ) );
                }
            }
        }
    }
}
