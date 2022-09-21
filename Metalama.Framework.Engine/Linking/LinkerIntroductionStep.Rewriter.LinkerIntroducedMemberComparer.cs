// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.AspectOrdering;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Linking
{
    internal partial class LinkerIntroductionStep
    {
        private partial class Rewriter
        {
            private class LinkerIntroducedMemberComparer : IComparer<LinkerIntroducedMember>
            {
                private static readonly ImmutableDictionary<DeclarationKind, int> _orderedDeclarationKinds = new Dictionary<DeclarationKind, int>()
                {
                    { DeclarationKind.Field, 0 },
                    { DeclarationKind.Constructor, 1 },
                    { DeclarationKind.Property, 2 },
                    { DeclarationKind.Method, 3 },
                    { DeclarationKind.Event, 4 },
                    { DeclarationKind.NamedType, 5 }
                }.ToImmutableDictionary();

                private static readonly ImmutableDictionary<Accessibility, int> _orderedAccessibilities = new Dictionary<Accessibility, int>()
                {
                    { Accessibility.Public, 0 },
                    { Accessibility.Protected, 1 },
                    { Accessibility.ProtectedInternal, 2 },
                    { Accessibility.Internal, 3 },
                    { Accessibility.PrivateProtected, 4 },
                    { Accessibility.Private, 5 }
                }.ToImmutableDictionary();

                private readonly IReadOnlyDictionary<AspectLayerId, OrderedAspectLayer> _orderedAspectLayers;

                public LinkerIntroducedMemberComparer( IReadOnlyDictionary<AspectLayerId, OrderedAspectLayer> orderedAspectLayers )
                {
                    this._orderedAspectLayers = orderedAspectLayers;
                }

                private OrderedAspectLayer GetAspectLayer( LinkerIntroducedMember m ) => this._orderedAspectLayers[m.Introduction.ParentAdvice.AspectLayerId];

                public int Compare( LinkerIntroducedMember? x, LinkerIntroducedMember? y )
                {
                    if ( x == y )
                    {
                        return 0;
                    }

                    if ( x == null && y == null )
                    {
                        return 0;
                    }
                    else if ( x == null )
                    {
                        return 1;
                    }
                    else if ( y == null )
                    {
                        return -1;
                    }

                    var declaration = GetDeclaration( x );
                    var otherDeclaration = GetDeclaration( y );

                    // Order by kind.
                    var kindComparison = GetKindOrder( x.Kind ).CompareTo( GetKindOrder( y.Kind ) );

                    if ( kindComparison != 0 )
                    {
                        return kindComparison;
                    }

                    // Order by name.
                    var nameComparison = string.CompareOrdinal( declaration.Name, otherDeclaration.Name );

                    if ( nameComparison != 0 )
                    {
                        return nameComparison;
                    }

                    // Order by signature.
                    if ( declaration is IMethod )
                    {
                        var signatureComparison = string.CompareOrdinal(
                            declaration.ToDisplayString( CodeDisplayFormat.MinimallyQualified ).TrimSuffix( "" ),
                            otherDeclaration.ToDisplayString( CodeDisplayFormat.MinimallyQualified ).TrimSuffix( "" ) );

                        if ( signatureComparison != 0 )
                        {
                            return signatureComparison;
                        }
                    }

                    // Order by accessibility.
                    var accessibilityComparison =
                        GetAccessibilityOrder( declaration.Accessibility ).CompareTo( GetAccessibilityOrder( otherDeclaration.Accessibility ) );

                    if ( accessibilityComparison != 0 )
                    {
                        return accessibilityComparison;
                    }

                    // Order by implemented interface.
                    if ( declaration is IMember declarationMember && declaration is IMember otherDeclarationMember )
                    {
                        var isExplicitInterfaceImplementationComparison =
                            declarationMember.IsExplicitInterfaceImplementation.CompareTo( otherDeclarationMember.IsExplicitInterfaceImplementation );

                        if ( isExplicitInterfaceImplementationComparison != 0 )
                        {
                            return -isExplicitInterfaceImplementationComparison;
                        }
                        else if ( declarationMember.IsExplicitInterfaceImplementation )
                        {
                            var interfaceComparison = string.Compare(
                                declarationMember.GetExplicitInterfaceImplementation().DeclaringType.FullName,
                                otherDeclarationMember.GetExplicitInterfaceImplementation().DeclaringType.FullName,
                                StringComparison.Ordinal );

                            if ( interfaceComparison != 0 )
                            {
                                return interfaceComparison;
                            }
                        }
                    }

                    // Order by type of introduction.
                    var typeComparison = GetTransformationTypeOrder( x.Introduction ).CompareTo( GetTransformationTypeOrder( y.Introduction ) );

                    if ( typeComparison != 0 )
                    {
                        return typeComparison;
                    }

                    // Order by aspect layer.
                    var xLayer = this.GetAspectLayer( x );
                    var yLayer = this.GetAspectLayer( y );
                    var aspectLayerComparison = xLayer.Order.CompareTo( yLayer.Order );

                    if ( aspectLayerComparison != 0 )
                    {
                        return aspectLayerComparison;
                    }

                    // Order by aspect instance in the current type.
                    var aspectInstanceOrderComparison =
                        x.Introduction.ParentAdvice.Aspect.OrderWithinTypeAndAspectLayer.CompareTo(
                            y.Introduction.ParentAdvice.Aspect.OrderWithinTypeAndAspectLayer );

                    if ( aspectInstanceOrderComparison != 0 )
                    {
                        return aspectInstanceOrderComparison;
                    }

                    // Order by adding order within the aspect instance.
                    var adviceOrderComparison = x.Introduction.OrderWithinAspectInstance.CompareTo( y.Introduction.OrderWithinAspectInstance );

                    if ( adviceOrderComparison != 0 )
                    {
                        return adviceOrderComparison;
                    }

                    // Order by semantic.
                    var semanticComparison = GetSemanticOrder( x.Semantic ).CompareTo( GetSemanticOrder( y.Semantic ) );

                    if ( semanticComparison != 0 )
                    {
                        return semanticComparison;
                    }

                    {
                        // Order replaced declarations within the same layer.
                        if ( x.Introduction is IReplaceMemberTransformation { ReplacedMember: { } replacedMemberRefX }
                             && replacedMemberRefX.Target == y.Introduction )
                        {
                            return 1;
                        }

                        if ( y.Introduction is IReplaceMemberTransformation { ReplacedMember: { } replacedMemberRefY }
                             && replacedMemberRefY.Target == x.Introduction )
                        {
                            return -1;
                        }
                    }

                    throw new AssertionFailedException( $"'{x}' and '{y}' are not strongly ordered" );
                }

                private static int GetKindOrder( DeclarationKind kind ) => _orderedDeclarationKinds.TryGetValue( kind, out var order ) ? order : 10;

                private static int GetAccessibilityOrder( Accessibility accessibility )
                    => _orderedAccessibilities.TryGetValue( accessibility, out var order ) ? order : 10;

                private static int GetTransformationTypeOrder( IIntroduceMemberTransformation introduction ) => introduction is IOverriddenDeclaration ? 0 : 1;

                private static int GetSemanticOrder( IntroducedMemberSemantic semantic ) => semantic != IntroducedMemberSemantic.InitializerMethod ? 0 : 1;

                private static IMemberOrNamedType GetDeclaration( IntroducedMember introducedMember )
                {
                    var declaration = introducedMember.Declaration ?? introducedMember.Introduction as IMember;

                    if ( declaration == null && introducedMember.Introduction is IOverriddenDeclaration overridden )
                    {
                        declaration = (IMemberOrNamedType) overridden.OverriddenDeclaration;
                    }

                    if ( declaration == null )
                    {
                        throw new AssertionFailedException( "Dont know how to sort." );
                    }

                    return declaration;
                }
            }
        }
    }
}