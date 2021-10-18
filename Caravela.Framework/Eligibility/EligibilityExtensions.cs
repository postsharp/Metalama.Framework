// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Eligibility.Implementation;
using System;
using System.Linq;

namespace Caravela.Framework.Eligibility
{
    /// <summary>
    /// Extension methods for <see cref="IEligibilityBuilder"/>.
    /// </summary>
    public static class EligibilityExtensions
    {
        public static IEligibilityBuilder<INamedType> DeclaringType<T>( this IEligibilityBuilder<T> eligibilityBuilder )
            where T : IMember
            => new ChildEligibilityBuilder<T, INamedType>(
                eligibilityBuilder,
                declaration => declaration.DeclaringType,
                declarationDescription => $"the declaring type '{declarationDescription.Object.DeclaringType}'" );

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
                declaration => declaration.Type,
                declarationDescription => $"the type of {declarationDescription}" );

        public static IEligibilityBuilder<IParameter> Parameter( this IEligibilityBuilder<IHasParameters> eligibilityBuilder, int index )
            => new ChildEligibilityBuilder<IHasParameters, IParameter>(
                eligibilityBuilder,
                declaration => declaration.Parameters[index],
                method => $"the {index + 1}-th parameter of {method}",
                method => index < method.Parameters.Count,
                method => $"{method} has fewer than {index + 1} parameter(s)" );

        public static IEligibilityBuilder<IParameter> Parameter( this IEligibilityBuilder<IHasParameters> eligibilityBuilder, string name )
            => new ChildEligibilityBuilder<IHasParameters, IParameter>(
                eligibilityBuilder,
                declaration => declaration.Parameters[name],
                method => $"parameter '{name}' of {method}",
                method => method.Parameters.All( p => p.Name != name ),
                method => $"{method} has no parameter named '{name}'" );
        
        public static IEligibilityBuilder<IType> Type( this IEligibilityBuilder<IHasType> eligibilityBuilder )
            => new ChildEligibilityBuilder<IHasType, IType>(
                eligibilityBuilder,
                declaration => declaration.Type,
                declaration => $"the type of {declaration}",
                method => true,
                method => $"" );

        public static IEligibilityBuilder<T> ExceptForScenarios<T>( this IEligibilityBuilder<T> eligibilityBuilder, EligibleScenarios excludedScenarios )
            => new ExcludedScenarioEligibilityBuilder<T>( eligibilityBuilder, excludedScenarios );

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

        public static void MustSatisfy<T>(
            this IEligibilityBuilder<T> eligibilityBuilder,
            Predicate<T> predicate,
            Func<IDescribedObject<T>, FormattableString> getJustification )
            => eligibilityBuilder.AddRule( new EligibilityRule<T>( eligibilityBuilder.IneligibleScenarios, predicate, getJustification ) );

        public static void MustNotHaveRefOrOutParameter( this IEligibilityBuilder<IMethod> eligibilityBuilder )
            => eligibilityBuilder.AddRule(
                new EligibilityRule<IMethod>(
                    eligibilityBuilder.IneligibleScenarios,
                    m => !m.Parameters.Any( p => p.RefKind is RefKind.Out or RefKind.Ref ),
                    d => $"{d} cannot have any ref or out parameter") );

        public static void MustHaveAccessibility(
            this IEligibilityBuilder<IMemberOrNamedType> eligibilityBuilder,
            Accessibility accessibility,
            params Accessibility[] otherAccessibilities )
            => eligibilityBuilder.MustSatisfy(
                member => member.Accessibility == accessibility || otherAccessibilities.Contains( member.Accessibility ),
                member =>
                {
                    var accessibilities = new[] { accessibility }.Concat( otherAccessibilities ).ToArray();

                    var formattedAccessibilities = string.Join(
                        " or ",
                        accessibilities.Select( a => string.Format( CaravelaServices.FormatProvider, "{0}", a ) ) );

                    return $"{member} must be {formattedAccessibilities}";
                } );

        public static void MustBeStatic( this IEligibilityBuilder<IMemberOrNamedType> eligibilityBuilder )
            => eligibilityBuilder.MustSatisfy(
                member => member.IsStatic,
                member => $"{member} must be static" );

        public static void MustBeNonStatic( this IEligibilityBuilder<IMemberOrNamedType> eligibilityBuilder )
            => eligibilityBuilder.MustSatisfy(
                member => !member.IsStatic,
                member => $"{member} must be non-static" );

        public static void MustBeNonAbstract( this IEligibilityBuilder<IMemberOrNamedType> eligibilityBuilder )
            => eligibilityBuilder.MustSatisfy(
                member => !member.IsAbstract,
                member => $"{member} must be non-abstract" );

        public static void MustBe( this IEligibilityBuilder<IType> eligibilityBuilder, Type type, ConversionKind conversionKind = ConversionKind.Default)
            => eligibilityBuilder.MustSatisfy(
                t => t.Is( type, conversionKind ),
                member => $"{member} must be of type '{type}'" );

        public static void MustBe<T>( this IEligibilityBuilder<IType> eligibilityBuilder, ConversionKind conversionKind = ConversionKind.Default ) => eligibilityBuilder.MustBe( typeof(T), conversionKind );
    }
}