using System;
using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Linking
{
    /// <summary>
    /// Optimization structure which tracks introductions created by introduction step of the linker.
    /// </summary>
    internal class LinkerTransformationRegistry
    {
        private const string _introducedSyntaxAnnotationId = "AspectLinker_IntroducedSyntax";

        private bool _frozen;
        private int _nextAnnotationId;
        private readonly Dictionary<IMemberIntroduction, IReadOnlyList<IntroducedMember>> _introducedMembers;
        private readonly Dictionary<IntroducedMember, (SyntaxTree OriginalSyntaxTree, int AnnotationId, MemberDeclarationSyntax AnnotatedSyntax)> _introducedMemberToMarkId;
        private readonly Dictionary<int, (SyntaxTree OriginalSyntaxTree, IntroducedMember IntroducedMember, MemberDeclarationSyntax AnnotatedSyntax)> _introducedMarkIdToMember;
        private readonly Dictionary<SyntaxTree, SyntaxTree> _introducedTreeMap;

        public LinkerTransformationRegistry()
        {
            this._introducedMembers = new Dictionary<IMemberIntroduction, IReadOnlyList<IntroducedMember>>();
            this._introducedMemberToMarkId = new Dictionary<IntroducedMember, (SyntaxTree, int, MemberDeclarationSyntax)>();
            this._introducedMarkIdToMember = new Dictionary<int, (SyntaxTree, IntroducedMember, MemberDeclarationSyntax)>();
            this._introducedTreeMap = new Dictionary<SyntaxTree, SyntaxTree>();
        }

        public void RegisterIntroducedMembers( IMemberIntroduction memberIntroduction, IEnumerable<IntroducedMember> introducedMembers )
        {
            if ( this._frozen )
            {
                throw new InvalidOperationException();
            }

            this._introducedMembers.Add( memberIntroduction, introducedMembers.ToList() );

            foreach ( var introducedMember in introducedMembers )
            {
                var annotationId = this._nextAnnotationId++;
                var annotatedSyntax = introducedMember.Syntax.WithAdditionalAnnotations( new SyntaxAnnotation( _introducedSyntaxAnnotationId, annotationId.ToString() ) );

                this._introducedMemberToMarkId.Add( introducedMember, (memberIntroduction.TargetSyntaxTree, annotationId, annotatedSyntax) );
                this._introducedMarkIdToMember.Add( annotationId, (memberIntroduction.TargetSyntaxTree, introducedMember, annotatedSyntax) );
            }
        }

        public void RegisterIntermediateSyntaxTree( SyntaxTree originalTree, SyntaxTree intermediateTree )
        {
            if ( this._frozen )
            {
                throw new InvalidOperationException();
            }

            this._introducedTreeMap.Add( originalTree, intermediateTree );
        }

        public void Freeze()
        {
            this._frozen = true;
        }

        public IEnumerable<MemberDeclarationSyntax> GetIntroducedSyntaxNodesOnPosition( MemberDeclarationSyntax position )
        {
            // TODO: Optimize.
            return this._introducedMembers.SelectMany( kvp => kvp.Value.Select( i => (kvp.Key.InsertPositionNode, IntroducedMember: i) ) ).Where( p => p.InsertPositionNode == position ).Select( p => p.IntroducedMember.Syntax );
        }

        public IReadOnlyList<IntroducedMember> GetMethodOverridesForSymbol( IMethodSymbol symbol )
        {
            // TODO: Optimize.
            var introducedMembers = new List<IntroducedMember>();
            var declaringSyntax = symbol.DeclaringSyntaxReferences.Single().GetSyntax();

            var annotation = declaringSyntax.GetAnnotations( _introducedSyntaxAnnotationId ).SingleOrDefault();

            if ( annotation == null )
            {
                return Array.Empty<IntroducedMember>();
            }

            var id = int.Parse( annotation.Data );
            var memberRecord = this._introducedMarkIdToMember[id];

            return introducedMembers;
        }
    }
}
