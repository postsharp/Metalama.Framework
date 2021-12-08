// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Impl.AspectOrdering;
using Metalama.Framework.Impl.Aspects;
using Metalama.Framework.Impl.CodeModel;
using Metalama.Framework.Impl.Transformations;
using Metalama.Framework.Impl.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Impl.Linking
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

                private readonly ImmutableDictionary<AspectLayerId, OrderedAspectLayer> _orderedAspectLayers;

                public LinkerIntroducedMemberComparer( ImmutableDictionary<AspectLayerId, OrderedAspectLayer> orderedAspectLayers )
                {
                    this._orderedAspectLayers = orderedAspectLayers;
                }

                private int GetLayerOrder( LinkerIntroducedMember m ) => this._orderedAspectLayers[m.Introduction.Advice.AspectLayerId].Order;

                public int Compare( LinkerIntroducedMember x, LinkerIntroducedMember y )
                {
                    if ( x == y )
                    {
                        return 0;
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
                            declaration.ToDisplayString( CodeDisplayFormat.MinimallyQualified ).TrimEnd( "" ),
                            otherDeclaration.ToDisplayString( CodeDisplayFormat.MinimallyQualified ).TrimEnd( "" ) );

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
                    var isExplicitInterfaceImplementationComparison =
                        declaration.IsExplicitInterfaceImplementation.CompareTo( otherDeclaration.IsExplicitInterfaceImplementation );

                    if ( isExplicitInterfaceImplementationComparison != 0 )
                    {
                        return -isExplicitInterfaceImplementationComparison;
                    }
                    else if ( declaration.IsExplicitInterfaceImplementation )
                    {
                        var interfaceComparison = string.Compare(
                            declaration.GetExplicitInterfaceImplementation().DeclaringType.FullName,
                            otherDeclaration.GetExplicitInterfaceImplementation().DeclaringType.FullName,
                            StringComparison.Ordinal );

                        if ( interfaceComparison != 0 )
                        {
                            return interfaceComparison;
                        }
                    }

                    // Order by type of introduction.
                    var typeComparison = GetTypeOrder( x.Introduction ).CompareTo( GetTypeOrder( y.Introduction ) );

                    if ( typeComparison != 0 )
                    {
                        return typeComparison;
                    }

                    var aspectLayerComparison = this.GetLayerOrder( x ).CompareTo( this.GetLayerOrder( y ) );

                    if ( aspectLayerComparison != 0 )
                    {
                        return aspectLayerComparison;
                    }

                    throw new AssertionFailedException( $"'{x}' and '{y}' are not strongly ordered" );
                }

                private static int GetKindOrder( DeclarationKind kind ) => _orderedDeclarationKinds.TryGetValue( kind, out var order ) ? order : 10;

                private static int GetAccessibilityOrder( Accessibility accessibility )
                    => _orderedAccessibilities.TryGetValue( accessibility, out var order ) ? order : 10;

                private static int GetTypeOrder( IMemberIntroduction introduction ) => introduction is IOverriddenDeclaration ? 0 : 1;

                private static IMember GetDeclaration( IntroducedMember introducedMember )
                {
                    var declaration = introducedMember.Declaration ?? introducedMember.Introduction as IMember;

                    if ( declaration == null && introducedMember.Introduction is IOverriddenDeclaration overridden )
                    {
                        declaration = (IMember) overridden.OverriddenDeclaration;
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