// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Eligibility.Implementation;
using System;
using System.Linq;

namespace Caravela.Framework.Eligibility
{
    [Obsolete( "Not implemented." )]
    public static class EligibilityExtensions
    {
        public static IEligibilityBuilder<INamedType> DeclaringType<T>( this IEligibilityBuilder<T> eligibilityBuilder )
            where T : IMember
            => new ChildEligibilityBuilder<T, INamedType>(
                eligibilityBuilder,
                declaration => declaration.DeclaringType,
                declarationDescription => $"the declaring type of {declarationDescription}" );

        public static IEligibilityBuilder<IType> ReturnType( this IEligibilityBuilder<IMethod> eligibilityBuilder )
            => new ChildEligibilityBuilder<IMethod, IType>(
                eligibilityBuilder,
                declaration => declaration.ReturnType,
                declarationDescription => $"the return type of {declarationDescription}" );

        public static IEligibilityBuilder<IType> Type( this IEligibilityBuilder<IFieldOrProperty> eligibilityBuilder )
            => new ChildEligibilityBuilder<IFieldOrProperty, IType>(
                eligibilityBuilder,
                declaration => declaration.Type,
                declarationDescription => $"the type of {declarationDescription}" );

        public static IEligibilityBuilder<IType> Type( this IEligibilityBuilder<IParameter> eligibilityBuilder )
            => new ChildEligibilityBuilder<IParameter, IType>(
                eligibilityBuilder,
                declaration => declaration.ParameterType,
                declarationDescription => $"the type of {declarationDescription}" );

        public static IEligibilityBuilder<IParameter> Parameter( this IEligibilityBuilder<IMethodBase> eligibilityBuilder, int index )
            => new ChildEligibilityBuilder<IMethodBase, IParameter>(
                eligibilityBuilder,
                declaration => declaration.Parameters[index],
                method => $"the {index + 1}-th parameter of {method}",
                method => index < method.Parameters.Count,
                method => $"{method} has fewer than {index + 1} parameter(s)" );

        public static IEligibilityBuilder<IParameter> Parameter( this IEligibilityBuilder<IMethodBase> eligibilityBuilder, string name )
            => new ChildEligibilityBuilder<IMethodBase, IParameter>(
                eligibilityBuilder,
                declaration => declaration.Parameters[name],
                method => $"parameter '{name}' of {method}",
                method => !method.Parameters.Any( p => p.Name == name ),
                method => $"{method} has no parameter named '{name}'" );

        public static IEligibilityBuilder<T> ExceptForInheritance<T>( this IEligibilityBuilder<T> eligibilityBuilder )
            => new InheritanceOnlyEligibilityBuilder<T>( eligibilityBuilder );

        public static void MustSatisfyAny<T>( this IEligibilityBuilder<T> eligibilityBuilder, params Action<IEligibilityBuilder<T>>[] requirements )
            where T : class
            => eligibilityBuilder.Aggregate( BooleanCombinationOperator.Or, requirements );

        public static void MustSatisfyAll<T>( this IEligibilityBuilder<T> eligibilityBuilder, params Action<IEligibilityBuilder<T>>[] requirements )
            where T : class
            => eligibilityBuilder.Aggregate( BooleanCombinationOperator.And, requirements );

        private static void Aggregate<T>(
            this IEligibilityBuilder<T> eligibilityBuilder,
            BooleanCombinationOperator combinationOperator,
            params Action<IEligibilityBuilder<T>>[] requirements )
            where T : class
        {
            var orBuilder = new EligibilityBuilder<T>( combinationOperator );

            foreach ( var requirement in requirements )
            {
                requirement( orBuilder );
            }

            eligibilityBuilder.AddRule( orBuilder.Build() );
        }

        public static void Require<T>(
            this IEligibilityBuilder<T> eligibilityBuilder,
            Predicate<T> predicate,
            Func<IDescribedObject<T>, FormattableString> getJustification )
            => eligibilityBuilder.AddRule( new EligibilityRule<T>( eligibilityBuilder.Ineligibility, predicate, getJustification ) );

        public static void MustHaveAccessibility(
            this IEligibilityBuilder<IMemberOrNamedType> eligibilityBuilder,
            Accessibility accessibility,
            params Accessibility[] otherAccessibilities )
            => eligibilityBuilder.Require(
                member => member.Accessibility == accessibility || otherAccessibilities.Contains( member.Accessibility ),
                member =>
                {
                    var accessibilities = new[] { accessibility }.Concat( otherAccessibilities ).ToArray();

                    var formattedAccessibilities = string.Join(
                        " or ",
                        accessibilities.Select( a => string.Format( member.FormatProvider, "{0}", a ) ) );

                    return $"{member} must be {formattedAccessibilities}";
                } );

        public static void MustBeStatic( this IEligibilityBuilder<IMemberOrNamedType> eligibilityBuilder )
            => eligibilityBuilder.Require(
                member => member.IsStatic,
                member => $"{member} must be static" );

        public static void MustBeNonStatic( this IEligibilityBuilder<IMemberOrNamedType> eligibilityBuilder )
            => eligibilityBuilder.Require(
                member => !member.IsStatic,
                member => $"{member} must be non-static" );

        public static void MustBeNonAbstract( this IEligibilityBuilder<IMemberOrNamedType> eligibilityBuilder )
            => eligibilityBuilder.Require(
                member => !member.IsStatic,
                member => $"{member} must be non-static" );

        public static void MustBe( this IEligibilityBuilder<IType> eligibilityBuilder, Type type )
            => eligibilityBuilder.Require(
                t => t.Is( type ),
                member => $"{member} must be {type}" );

        internal static ( bool IsEligible, string? Justification ) IsEligible<T>(
            this IEligibilityRule<T> rule,
            T obj,
            EligibilityValue requiredEligibility,
            bool requiresJustification,
            IFormatProvider formatProvider )
            where T : class
        {
            var eligibility = rule.GetEligibility( obj );
            string? justification = null;

            if ( eligibility < requiredEligibility )
            {
                if ( requiresJustification )
                {
                    var describedObject = new DescribedObject<T>( obj, formatProvider );
                    justification = rule.GetIneligibilityJustification( requiredEligibility, describedObject )?.ToString( formatProvider );
                }

                return (false, justification);
            }
            else
            {
                return (true, null);
            }
        }
    }
}