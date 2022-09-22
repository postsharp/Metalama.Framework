// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Engine.Utilities.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace Metalama.Framework.Engine.Linking;

internal partial class LinkerIntroductionStep
{
    /// <summary>
    /// Collection of introduced members for given transformations. Id is added to the nodes to allow tracking.
    /// </summary>
    private class SyntaxTransformationCollection
    {
        private readonly TransformationLinkerOrderComparer _comparer;
        private readonly ConcurrentBag<LinkerIntroducedMember> _introducedMembers;
        private readonly ConcurrentDictionary<InsertPosition, UnsortedConcurrentLinkedList<LinkerIntroducedMember>> _introducedMembersByInsertPosition;

        private readonly ConcurrentDictionary<BaseTypeDeclarationSyntax, UnsortedConcurrentLinkedList<LinkerIntroducedInterface>>
            _introducedInterfacesByTargetTypeDeclaration;

        private readonly ConcurrentSet<VariableDeclaratorSyntax> _removedVariableDeclaratorSyntax;
        private readonly ConcurrentSet<PropertyDeclarationSyntax> _autoPropertyWithSynthesizedSetterSyntax;

        private int _nextId;

        public IReadOnlyCollection<LinkerIntroducedMember> IntroducedMembers => this._introducedMembers;

        public SyntaxTransformationCollection( TransformationLinkerOrderComparer comparer )
        {
            this._comparer = comparer;
            this._introducedMembers = new ConcurrentBag<LinkerIntroducedMember>();
            this._introducedMembersByInsertPosition = new ConcurrentDictionary<InsertPosition, UnsortedConcurrentLinkedList<LinkerIntroducedMember>>();

            this._introducedInterfacesByTargetTypeDeclaration =
                new ConcurrentDictionary<BaseTypeDeclarationSyntax, UnsortedConcurrentLinkedList<LinkerIntroducedInterface>>();

            this._removedVariableDeclaratorSyntax = new ConcurrentSet<VariableDeclaratorSyntax>();
            this._autoPropertyWithSynthesizedSetterSyntax = new ConcurrentSet<PropertyDeclarationSyntax>();
        }

        public void Add( IIntroduceMemberTransformation memberIntroduction, IEnumerable<IntroducedMember> introducedMembers )
        {
            foreach ( var introducedMember in introducedMembers )
            {
                var id = Interlocked.Increment( ref this._nextId ).ToString( CultureInfo.InvariantCulture );
                var idAnnotation = new SyntaxAnnotation( LinkerIntroductionRegistry.IntroducedNodeIdAnnotationId, id );

                // TODO: Roslyn adds Id annotation to nodes that are tracked, which we may use instead of our own annotation.
                var annotatedIntroducedSyntax = introducedMember.Syntax.WithAdditionalAnnotations( idAnnotation );

                // Any transformations of the introduced syntax node need to be done before this.
                var linkerIntroducedMember = new LinkerIntroducedMember( id, annotatedIntroducedSyntax, introducedMember );

                this._introducedMembers.Add( linkerIntroducedMember );

                var nodes = this._introducedMembersByInsertPosition.GetOrAdd(
                    memberIntroduction.InsertPosition,
                    _ => new UnsortedConcurrentLinkedList<LinkerIntroducedMember>() );

                nodes.Add( linkerIntroducedMember );
            }
        }

        public void Add( IIntroduceInterfaceTransformation interfaceImplementationIntroduction, BaseTypeSyntax introducedInterface )
        {
            var targetTypeSymbol = ((INamedType) interfaceImplementationIntroduction.TargetDeclaration).GetSymbol();

            // Heuristic: select the file with the shortest path.
            var targetTypeDecl = (BaseTypeDeclarationSyntax) targetTypeSymbol.GetPrimaryDeclaration().AssertNotNull();

            var interfaceList = this._introducedInterfacesByTargetTypeDeclaration.GetOrAdd(
                targetTypeDecl,
                _ => new UnsortedConcurrentLinkedList<LinkerIntroducedInterface>() );

            interfaceList.Add( new LinkerIntroducedInterface( interfaceImplementationIntroduction, introducedInterface ) );
        }

        public void AddAutoPropertyWithSynthesizedSetter( PropertyDeclarationSyntax declaration )
        {
            Invariant.Assert( declaration.IsAutoPropertyDeclaration() && !declaration.HasSetterAccessorDeclaration() );

            this._autoPropertyWithSynthesizedSetterSyntax.Add( declaration );
        }

        public void AddRemovedSyntax( SyntaxNode removedSyntax )
        {
            switch ( removedSyntax )
            {
                case VariableDeclaratorSyntax variableDeclarator:
                    this._removedVariableDeclaratorSyntax.Add( variableDeclarator );

                    break;

                default:
                    throw new AssertionFailedException();
            }
        }

        public bool IsRemovedSyntax( VariableDeclaratorSyntax variableDeclarator ) => this._removedVariableDeclaratorSyntax.Contains( variableDeclarator );

        public bool IsAutoPropertyWithSynthesizedSetter( PropertyDeclarationSyntax propertyDeclaration )
            => this._autoPropertyWithSynthesizedSetterSyntax.Contains( propertyDeclaration );

        public IReadOnlyList<LinkerIntroducedMember> GetIntroducedMembersOnPosition( InsertPosition position )
        {
            if ( this._introducedMembersByInsertPosition.TryGetValue( position, out var introducedMembers ) )
            {
                // IMPORTANT - do not change the introduced node here.
                return introducedMembers.GetSortedItems( ( x, y ) => LinkerIntroducedMemberComparer.Instance.Compare( x, y ) );
            }

            return Array.Empty<LinkerIntroducedMember>();
        }

        public IReadOnlyList<LinkerIntroducedInterface> GetIntroducedInterfacesForTypeDeclaration( BaseTypeDeclarationSyntax typeDeclaration )
        {
            if ( this._introducedInterfacesByTargetTypeDeclaration.TryGetValue( typeDeclaration, out var interfaceList ) )
            {
                return interfaceList.GetSortedItems( ( x, y ) => this._comparer.Compare( x.Transformation, y.Transformation ) );
            }

            return Array.Empty<LinkerIntroducedInterface>();
        }
    }
}