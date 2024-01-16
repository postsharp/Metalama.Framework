// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Linking;

internal sealed partial class LinkerInjectionStep
{
    /// <summary>
    /// Collection of introduced members for given transformations. Id is added to the nodes to allow tracking.
    /// </summary>
    private sealed class SyntaxTransformationCollection
    {
        private readonly TransformationLinkerOrderComparer _comparer;
        private readonly List<InjectedMember> _injectedMembers;
        private readonly ConcurrentDictionary<InsertPosition, List<InjectedMember>> _injectedMembersByInsertPosition;

        private readonly ConcurrentDictionary<BaseTypeDeclarationSyntax, List<LinkerInjectedInterface>> _injectedInterfacesByTargetTypeDeclaration;

        private readonly HashSet<VariableDeclaratorSyntax> _removedVariableDeclaratorSyntax;
        private readonly HashSet<PropertyDeclarationSyntax> _autoPropertyWithSynthesizedSetterSyntax;
        private readonly ConcurrentDictionary<PropertyDeclarationSyntax, List<AspectLinkerDeclarationFlags>> _additionalDeclarationFlags;

        public IReadOnlyCollection<InjectedMember> InjectedMembers => this._injectedMembers;

        public SyntaxTransformationCollection( TransformationLinkerOrderComparer comparer )
        {
            this._comparer = comparer;
            this._injectedMembers = new List<InjectedMember>();
            this._injectedMembersByInsertPosition = new ConcurrentDictionary<InsertPosition, List<InjectedMember>>();

            this._injectedInterfacesByTargetTypeDeclaration =
                new ConcurrentDictionary<BaseTypeDeclarationSyntax, List<LinkerInjectedInterface>>();

            this._removedVariableDeclaratorSyntax = new HashSet<VariableDeclaratorSyntax>();
            this._autoPropertyWithSynthesizedSetterSyntax = new HashSet<PropertyDeclarationSyntax>();
            this._additionalDeclarationFlags = new ConcurrentDictionary<PropertyDeclarationSyntax, List<AspectLinkerDeclarationFlags>>();
        }

        public void Add( IInjectMemberTransformation injectMemberTransformation, IEnumerable<InjectedMember> injectedMembers )
        {
            foreach ( var injectedMember in injectedMembers )
            {
                lock ( this._injectedMembers )
                {
                    this._injectedMembers.Add( injectedMember );
                }

                var nodes = this._injectedMembersByInsertPosition.GetOrAdd(
                    injectMemberTransformation.InsertPosition,
                    _ => new List<InjectedMember>() );

                lock ( nodes )
                {
                    nodes.Add( injectedMember );
                }
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
                    _ => new List<LinkerInjectedInterface>() );

            lock ( interfaceList )
            {
                interfaceList.Add( new LinkerInjectedInterface( injectInterfaceTransformation, introducedInterface ) );
            }
        }

        public void AddAutoPropertyWithSynthesizedSetter( PropertyDeclarationSyntax declaration )
        {
            Invariant.Assert( declaration.IsAutoPropertyDeclaration() && !declaration.HasSetterAccessorDeclaration() );

            lock ( this._autoPropertyWithSynthesizedSetterSyntax )
            {
                this._autoPropertyWithSynthesizedSetterSyntax.Add( declaration );
            }
        }

        // ReSharper disable once UnusedMember.Local
        public void AddDeclarationWithAdditionalFlags( PropertyDeclarationSyntax declaration, AspectLinkerDeclarationFlags flags )
        {
            var list = this._additionalDeclarationFlags.GetOrAdd( declaration, _ => new List<AspectLinkerDeclarationFlags>() );

            lock ( list )
            {
                list.Add( flags );
            }
        }

        public void AddRemovedSyntax( SyntaxNode removedSyntax )
        {
            switch ( removedSyntax )
            {
                case VariableDeclaratorSyntax variableDeclarator:
                    lock ( this._removedVariableDeclaratorSyntax )
                    {
                        this._removedVariableDeclaratorSyntax.Add( variableDeclarator );
                    }

                    break;

                default:
                    throw new AssertionFailedException( $"{removedSyntax.Kind()} is not supported removed syntax." );
            }
        }

        public bool IsRemovedSyntax( VariableDeclaratorSyntax variableDeclarator ) => this._removedVariableDeclaratorSyntax.Contains( variableDeclarator );

        public bool IsAutoPropertyWithSynthesizedSetter( PropertyDeclarationSyntax propertyDeclaration )
            => this._autoPropertyWithSynthesizedSetterSyntax.Contains( propertyDeclaration );

        public IReadOnlyList<InjectedMember> GetInjectedMembersOnPosition( InsertPosition position )
        {
            if ( this._injectedMembersByInsertPosition.TryGetValue( position, out var injectedMembers ) )
            {
                // IMPORTANT - do not change the introduced node here.
                injectedMembers.Sort( InjectedMemberComparer.Instance );

                return injectedMembers;
            }

            return Array.Empty<InjectedMember>();
        }

        public IReadOnlyList<LinkerInjectedInterface> GetIntroducedInterfacesForTypeDeclaration( BaseTypeDeclarationSyntax typeDeclaration )
        {
            if ( this._injectedInterfacesByTargetTypeDeclaration.TryGetValue( typeDeclaration, out var interfaceList ) )
            {
                interfaceList.Sort( ( x, y ) => this._comparer.Compare( x.Transformation, y.Transformation ) );

                return interfaceList;
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