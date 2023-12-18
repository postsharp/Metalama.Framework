// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Engine.Utilities.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace Metalama.Framework.Engine.Linking;

internal sealed partial class LinkerInjectionStep
{
    /// <summary>
    /// Collection of introduced members for given transformations. Id is added to the nodes to allow tracking.
    /// </summary>
    private sealed class SyntaxTransformationCollection
    {
        private readonly TransformationLinkerOrderComparer _comparer;
        private readonly ConcurrentBag<LinkerInjectedMember> _injectedMembers;
        private readonly ConcurrentDictionary<InsertPosition, UnsortedConcurrentLinkedList<LinkerInjectedMember>> _injectedMembersByInsertPosition;

        private readonly ConcurrentDictionary<BaseTypeDeclarationSyntax, UnsortedConcurrentLinkedList<LinkerInjectedInterface>>
            _injectedInterfacesByTargetTypeDeclaration;

        private readonly ConcurrentSet<VariableDeclaratorSyntax> _removedVariableDeclaratorSyntax;
        private readonly ConcurrentSet<PropertyDeclarationSyntax> _autoPropertyWithSynthesizedSetterSyntax;
        private readonly ConcurrentDictionary<PropertyDeclarationSyntax, ConcurrentLinkedList<AspectLinkerDeclarationFlags>> _additionalDeclarationFlags;

        private int _nextId;

        public IReadOnlyCollection<LinkerInjectedMember> InjectedMembers => this._injectedMembers;

        public SyntaxTransformationCollection( TransformationLinkerOrderComparer comparer )
        {
            this._comparer = comparer;
            this._injectedMembers = new ConcurrentBag<LinkerInjectedMember>();
            this._injectedMembersByInsertPosition = new ConcurrentDictionary<InsertPosition, UnsortedConcurrentLinkedList<LinkerInjectedMember>>();

            this._injectedInterfacesByTargetTypeDeclaration =
                new ConcurrentDictionary<BaseTypeDeclarationSyntax, UnsortedConcurrentLinkedList<LinkerInjectedInterface>>();

            this._removedVariableDeclaratorSyntax = new ConcurrentSet<VariableDeclaratorSyntax>();
            this._autoPropertyWithSynthesizedSetterSyntax = new ConcurrentSet<PropertyDeclarationSyntax>();
            this._additionalDeclarationFlags = new ConcurrentDictionary<PropertyDeclarationSyntax, ConcurrentLinkedList<AspectLinkerDeclarationFlags>>();
        }

        public void Add( IInjectMemberOrNamedTypeTransformation injectMemberTransformation, IEnumerable<InjectedMemberOrNamedType> injectedMembers )
        {
            foreach ( var injectedMember in injectedMembers )
            {
                var id = Interlocked.Increment( ref this._nextId ).ToString( CultureInfo.InvariantCulture );
                var idAnnotation = new SyntaxAnnotation( LinkerInjectionRegistry.InjectedNodeIdAnnotationId, id );

                // TODO: Roslyn adds Id annotation to nodes that are tracked, which we may use instead of our own annotation.
                var annotatedIntroducedSyntax = injectedMember.Syntax.WithAdditionalAnnotations( idAnnotation );

                // Any transformations of the introduced syntax node need to be done before this.
                var linkerInjectedMember = new LinkerInjectedMember( id, annotatedIntroducedSyntax, injectedMember );

                this._injectedMembers.Add( linkerInjectedMember );

                var nodes = this._injectedMembersByInsertPosition.GetOrAdd(
                    injectMemberTransformation.InsertPosition,
                    _ => new UnsortedConcurrentLinkedList<LinkerInjectedMember>() );

                nodes.Add( linkerInjectedMember );
            }
        }

        public void Add( IInjectInterfaceTransformation injectInterfaceTransformation, BaseTypeSyntax introducedInterface )
        {
            var targetTypeSymbol = ((INamedType) injectInterfaceTransformation.TargetDeclaration).GetSymbol();

            // Heuristic: select the file with the shortest path.
            var targetTypeDecl = (BaseTypeDeclarationSyntax) targetTypeSymbol.GetPrimaryDeclaration().AssertNotNull();

            var interfaceList =
                this._injectedInterfacesByTargetTypeDeclaration.GetOrAdd(
                    targetTypeDecl,
                    _ => new UnsortedConcurrentLinkedList<LinkerInjectedInterface>() );

            interfaceList.Add( new LinkerInjectedInterface( injectInterfaceTransformation, introducedInterface ) );
        }

        public void AddAutoPropertyWithSynthesizedSetter( PropertyDeclarationSyntax declaration )
        {
            Invariant.Assert( declaration.IsAutoPropertyDeclaration() && !declaration.HasSetterAccessorDeclaration() );

            this._autoPropertyWithSynthesizedSetterSyntax.Add( declaration );
        }

        // ReSharper disable once UnusedMember.Local
        public void AddDeclarationWithAdditionalFlags( PropertyDeclarationSyntax declaration, AspectLinkerDeclarationFlags flags )
        {
            var list = this._additionalDeclarationFlags.GetOrAdd( declaration, _ => new ConcurrentLinkedList<AspectLinkerDeclarationFlags>() );
            list.Add( flags );
        }

        public void AddRemovedSyntax( SyntaxNode removedSyntax )
        {
            switch ( removedSyntax )
            {
                case VariableDeclaratorSyntax variableDeclarator:
                    this._removedVariableDeclaratorSyntax.Add( variableDeclarator );

                    break;

                default:
                    throw new AssertionFailedException( $"{removedSyntax.Kind()} is not supported removed syntax." );
            }
        }

        public bool IsRemovedSyntax( VariableDeclaratorSyntax variableDeclarator ) => this._removedVariableDeclaratorSyntax.Contains( variableDeclarator );

        public bool IsAutoPropertyWithSynthesizedSetter( PropertyDeclarationSyntax propertyDeclaration )
            => this._autoPropertyWithSynthesizedSetterSyntax.Contains( propertyDeclaration );

        public IReadOnlyList<LinkerInjectedMember> GetInjectedMembersOnPosition( InsertPosition position )
        {
            if ( this._injectedMembersByInsertPosition.TryGetValue( position, out var injectedMembers ) )
            {
                // IMPORTANT - do not change the introduced node here.
                return injectedMembers.GetSortedItems( ( x, y ) => LinkerInjectedMemberComparer.Instance.Compare( x, y ) );
            }

            return Array.Empty<LinkerInjectedMember>();
        }

        public IReadOnlyList<LinkerInjectedInterface> GetIntroducedInterfacesForTypeDeclaration( BaseTypeDeclarationSyntax typeDeclaration )
        {
            if ( this._injectedInterfacesByTargetTypeDeclaration.TryGetValue( typeDeclaration, out var interfaceList ) )
            {
                return interfaceList.GetSortedItems( ( x, y ) => this._comparer.Compare( x.Transformation, y.Transformation ) );
            }

            return Array.Empty<LinkerInjectedInterface>();
        }

        public AspectLinkerDeclarationFlags GetAdditionalDeclarationFlags( PropertyDeclarationSyntax declaration )
        {
            if ( this._additionalDeclarationFlags.TryGetValue( declaration, out var list ) )
            {
                var finalFlags = AspectLinkerDeclarationFlags.None;

                foreach ( var flags in list )
                {
                    finalFlags |= flags;
                }

                return finalFlags;
            }

            return AspectLinkerDeclarationFlags.None;
        }
    }
}