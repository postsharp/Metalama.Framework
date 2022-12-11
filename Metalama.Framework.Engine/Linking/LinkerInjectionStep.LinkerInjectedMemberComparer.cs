// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Linking;

internal partial class LinkerInjectionStep
{
    private sealed class LinkerInjectedMemberComparer : IComparer<LinkerInjectedMember>
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

        public static LinkerInjectedMemberComparer Instance { get; } = new();

        private LinkerInjectedMemberComparer() { }

        public int Compare( LinkerInjectedMember? x, LinkerInjectedMember? y )
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
            var typeComparison = GetTransformationTypeOrder( x.Transformation ).CompareTo( GetTransformationTypeOrder( y.Transformation ) );

            if ( typeComparison != 0 )
            {
                return typeComparison;
            }

            // Order by aspect layer.
            var aspectLayerComparison = x.Transformation.OrderWithinPipeline.CompareTo( y.Transformation.OrderWithinPipeline );

            if ( aspectLayerComparison != 0 )
            {
                return aspectLayerComparison;
            }

            // Order by aspect instance in the current type.
            var aspectInstanceOrderComparison =
                x.Transformation.OrderWithinPipelineStepAndType.CompareTo( y.Transformation.OrderWithinPipelineStepAndType );

            if ( aspectInstanceOrderComparison != 0 )
            {
                return aspectInstanceOrderComparison;
            }

            // Order by adding order within the aspect instance.
            var adviceOrderComparison =
                x.Transformation.OrderWithinPipelineStepAndTypeAndAspectInstance.CompareTo( y.Transformation.OrderWithinPipelineStepAndTypeAndAspectInstance );

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
                if ( x.Transformation is IReplaceMemberTransformation { ReplacedMember: { } replacedMemberRefX }
                     && replacedMemberRefX.Target == y.Transformation )
                {
                    return 1;
                }

                if ( y.Transformation is IReplaceMemberTransformation { ReplacedMember: { } replacedMemberRefY }
                     && replacedMemberRefY.Target == x.Transformation )
                {
                    return -1;
                }
            }

            // TODO: At this point, all should be sorted, but mocks are not setting the order properties.
            // throw new AssertionFailedException( $"'{x}' and '{y}' are not strongly ordered" );
            return StringComparer.Ordinal.Compare( x.Syntax.ToString(), y.Syntax.ToString() );
        }

        private static int GetKindOrder( DeclarationKind kind ) => _orderedDeclarationKinds.TryGetValue( kind, out var order ) ? order : 10;

        private static int GetAccessibilityOrder( Accessibility accessibility )
            => _orderedAccessibilities.TryGetValue( accessibility, out var order ) ? order : 10;

        private static int GetTransformationTypeOrder( IInjectMemberTransformation injectMemberTransformation )
            => injectMemberTransformation is IOverrideDeclarationTransformation ? 0 : 1;

        private static int GetSemanticOrder( InjectedMemberSemantic semantic ) => semantic != InjectedMemberSemantic.InitializerMethod ? 0 : 1;

        private static IMemberOrNamedType GetDeclaration( InjectedMember injectedMember )
        {
            var declaration = injectedMember.Declaration ?? injectedMember.DeclarationBuilder as IMember;

            if ( declaration == null && injectedMember.Transformation is IOverrideDeclarationTransformation overridden )
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