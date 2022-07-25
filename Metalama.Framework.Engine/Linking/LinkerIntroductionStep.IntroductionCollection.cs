// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace Metalama.Framework.Engine.Linking
{
    internal partial class LinkerIntroductionStep
    {
        /// <summary>
        /// Collection of introduced members for given transformations. Id is added to the nodes to allow tracking.
        /// </summary>
        private class SyntaxTransformationCollection
        {
            private readonly List<LinkerIntroducedMember> _introducedMembers;
            private readonly Dictionary<InsertPosition, List<LinkerIntroducedMember>> _introducedMembersByInsertPosition;
            private readonly Dictionary<BaseTypeDeclarationSyntax, List<BaseTypeSyntax>> _introducedInterfacesByTargetTypeDeclaration;
            private readonly HashSet<VariableDeclaratorSyntax> _removedVariableDeclaratorSyntax;
            private readonly HashSet<PropertyDeclarationSyntax> _autoPropertyWithSynthesizedSetterSyntax;

            private int _nextId;

            public IReadOnlyList<LinkerIntroducedMember> IntroducedMembers => this._introducedMembers;

            public SyntaxTransformationCollection()
            {
                this._introducedMembers = new List<LinkerIntroducedMember>();
                this._introducedMembersByInsertPosition = new Dictionary<InsertPosition, List<LinkerIntroducedMember>>();
                this._introducedInterfacesByTargetTypeDeclaration = new Dictionary<BaseTypeDeclarationSyntax, List<BaseTypeSyntax>>();
                this._removedVariableDeclaratorSyntax = new HashSet<VariableDeclaratorSyntax>();
                this._autoPropertyWithSynthesizedSetterSyntax = new HashSet<PropertyDeclarationSyntax>();
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

                    if ( !this._introducedMembersByInsertPosition.TryGetValue( memberIntroduction.InsertPosition, out var nodes ) )
                    {
                        this._introducedMembersByInsertPosition[memberIntroduction.InsertPosition] = nodes = new List<LinkerIntroducedMember>();
                    }

                    nodes.Add( linkerIntroducedMember );
                }
            }

            public void Add( IIntroduceInterfaceTransformation interfaceImplementationIntroduction, BaseTypeSyntax introducedInterface )
            {
                var targetTypeSymbol = ((INamedType) interfaceImplementationIntroduction.ContainingDeclaration).GetSymbol();

                // Heuristic: select the file with the shortest path.
                var targetTypeDecl = (BaseTypeDeclarationSyntax) targetTypeSymbol.GetPrimaryDeclaration().AssertNotNull();

                if ( !this._introducedInterfacesByTargetTypeDeclaration.TryGetValue( targetTypeDecl, out var interfaceList ) )
                {
                    this._introducedInterfacesByTargetTypeDeclaration[targetTypeDecl] = interfaceList = new List<BaseTypeSyntax>();
                }

                interfaceList.Add( introducedInterface );
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

            public bool IsRemovedSyntax( VariableDeclaratorSyntax variableDeclarator )
            {
                return this._removedVariableDeclaratorSyntax.Contains( variableDeclarator );
            }

            public bool IsAutoPropertyWithSynthesizedSetter( PropertyDeclarationSyntax propertyDeclaration )
            {
                return this._autoPropertyWithSynthesizedSetterSyntax.Contains( propertyDeclaration );
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

            public IReadOnlyList<BaseTypeSyntax> GetIntroducedInterfacesForTypeDeclaration( BaseTypeDeclarationSyntax typeDeclaration )
            {
                if ( this._introducedInterfacesByTargetTypeDeclaration.TryGetValue( typeDeclaration, out var interfaceList ) )
                {
                    return interfaceList;
                }

                return Array.Empty<BaseTypeSyntax>();
            }
        }
    }
}