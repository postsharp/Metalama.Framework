// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Caravela.Framework.Impl.Linking
{
    internal partial class LinkerIntroductionStep
    {
        /// <summary>
        /// Collection of introduced members for given transformations. Id is added to the nodes to allow tracking.
        /// </summary>
        private class IntroductionCollection
        {
            private readonly List<LinkerIntroducedMember> _introducedMembers;
            private readonly Dictionary<InsertPosition, List<LinkerIntroducedMember>> _introducedMembersByInsertPosition;
            private readonly Dictionary<BaseTypeDeclarationSyntax, List<BaseTypeSyntax>> _introducedInterfacesByTargetTypeDecl;

            private int _nextId;

            public IReadOnlyList<LinkerIntroducedMember> IntroducedMembers => this._introducedMembers;

            public IntroductionCollection()
            {
                this._introducedMembers = new List<LinkerIntroducedMember>();
                this._introducedMembersByInsertPosition = new Dictionary<InsertPosition, List<LinkerIntroducedMember>>();
                this._introducedInterfacesByTargetTypeDecl = new Dictionary<BaseTypeDeclarationSyntax, List<BaseTypeSyntax>>();
            }

            public void Add( IMemberIntroduction memberIntroduction, IEnumerable<IntroducedMember> introducedMembers )
            {
                foreach ( var introducedMember in introducedMembers )
                {
                    var id = Interlocked.Increment( ref this._nextId ).ToString();
                    var idAnnotation = new SyntaxAnnotation( LinkerIntroductionRegistry.IntroducedNodeIdAnnotationId, id );

                    // TODO: Roslyn adds Id annotation to nodes that are tracked, which we may use instead of our own annotation.
                    var annotatedIntroducedSyntax = introducedMember.Syntax.WithAdditionalAnnotations( idAnnotation );

                    // Any transformations of the introduced syntax node need to be done before this.
                    var linkerIntroducedMember = new LinkerIntroducedMember( id, annotatedIntroducedSyntax, introducedMember );

                    this._introducedMembers.Add( linkerIntroducedMember );

                    if ( !this._introducedMembersByInsertPosition.TryGetValue( memberIntroduction.InsertPosition, out var nodes ) )
                    {
                        this._introducedMembersByInsertPosition[memberIntroduction.InsertPosition] = nodes = new List<LinkerIntroducedMember>();
                    }

                    nodes.Add( linkerIntroducedMember );
                }
            }

            public void Add( IIntroducedInterface interfaceImplementationIntroduction, IEnumerable<BaseTypeSyntax> introducedInterfaces )
            {
                var targetTypeSymbol = ((INamedType) interfaceImplementationIntroduction.ContainingDeclaration).GetSymbol();

                // Heuristic: select the file with the shortest path.
                var targetTypeDecl = (BaseTypeDeclarationSyntax) targetTypeSymbol.GetPrimaryDeclaration().AssertNotNull();

                if ( !this._introducedInterfacesByTargetTypeDecl.TryGetValue( targetTypeDecl, out var interfaceList ) )
                {
                    this._introducedInterfacesByTargetTypeDecl[targetTypeDecl] = interfaceList = new List<BaseTypeSyntax>();
                }

                interfaceList.AddRange( introducedInterfaces );
            }

            public IEnumerable<LinkerIntroducedMember> GetIntroducedMembersOnPosition( InsertPosition position )
            {
                if ( this._introducedMembersByInsertPosition.TryGetValue( position, out var introducedMembers ) )
                {
                    // IMPORTANT - do not change the introduced node here.
                    return introducedMembers;
                }

                return Enumerable.Empty<LinkerIntroducedMember>();
            }

            public IReadOnlyList<BaseTypeSyntax> GetIntroducedInterfacesForTypeDecl( BaseTypeDeclarationSyntax typeDeclaration )
            {
                if ( this._introducedInterfacesByTargetTypeDecl.TryGetValue( typeDeclaration, out var interfaceList ) )
                {
                    return interfaceList;
                }

                return Array.Empty<BaseTypeSyntax>();
            }
        }
    }
}