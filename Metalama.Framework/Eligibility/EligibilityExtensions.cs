// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility.Implementation;
using Metalama.Framework.Project;
using System;
using System.Linq;

namespace Metalama.Framework.Eligibility
{
    /// <summary>
    /// Extension methods for <see cref="IEligibilityBuilder"/>.
    /// </summary>
    /// <seealso href="@eligibility"/>
    [CompileTime]
    public static partial class EligibilityExtensions
    {
        /// <summary>
        /// Gets an <see cref="IEligibilityBuilder"/> for the declaring type of the member validated by the current <see cref="IEligibilityBuilder"/>.
        /// </summary>
        public static IEligibilityBuilder<INamedType> DeclaringType<T>( this IEligibilityBuilder<T> eligibilityBuilder )
            where T : IMemberOrNamedType
        {
            return new ChildEligibilityBuilder<T, INamedType>(
                eligibilityBuilder,
                declaration => declaration as INamedType ?? declaration.DeclaringType!,
                declarationDescription => $"the declaring type '{declarationDescription.Object.DeclaringType}'" );
        }

        public static Converter<T> Convert<T>( this IEligibilityBuilder<T> eligibilityBuilder ) => new( eligibilityBuilder );

        /// <summary>
        /// Gets an <see cref="IEligibilityBuilder"/> for the return type of the method validated by the current <see cref="IEligibilityBuilder"/>.
        /// </summary>
        public static IEligibilityBuilder<IType> ReturnType( this IEligibilityBuilder<IMethod> eligibilityBuilder )
        {
            return new ChildEligibilityBuilder<IMethod, IType>(
                eligibilityBuilder,
                declaration => declaration.ReturnType,
                declarationDescription => $"the return type of {declarationDescription}" );
        }

        /// <summary>
        /// Gets an <see cref="IEligibilityBuilder"/> for a parameter of the method validated by the current <see cref="IEligibilityBuilder"/>,
        /// identified by index.
        /// </summary>
        public static IEligibilityBuilder<IParameter> Parameter( this IEligibilityBuilder<IHasParameters> eligibilityBuilder, int index )
        {
            return new ChildEligibilityBuilder<IHasParameters, IParameter>(
                eligibilityBuilder,
                declaration => declaration.Parameters[index],
                method => $"the {index + 1}-th parameter of {method}",
                method => index < method.Parameters.Count,
                method => $"{method} has fewer than {index + 1} parameter(s)" );
        }

        /// <summary>
        /// Gets an <see cref="IEligibilityBuilder"/> for a parameter of the method validated by the current <see cref="IEligibilityBuilder"/>,
        /// identified by name.
        /// </summary>
        public static IEligibilityBuilder<IParameter> Parameter( this IEligibilityBuilder<IHasParameters> eligibilityBuilder, string name )
        {
            return new ChildEligibilityBuilder<IHasParameters, IParameter>(
                eligibilityBuilder,
                declaration => declaration.Parameters[name],
                method => $"parameter '{name}' of {method}",
                method => method.Parameters.All( p => p.Name != name ),
                method => $"{method} has no parameter named '{name}'" );
        }

        /// <summary>
        /// Gets an <see cref="IEligibilityBuilder"/> for the type of the declaration validated by the current <see cref="IEligibilityBuilder"/>.
        /// </summary>
        public static IEligibilityBuilder<IType> Type( this IEligibilityBuilder<IHasType> eligibilityBuilder )
        {
            return new ChildEligibilityBuilder<IHasType, IType>(
                eligibilityBuilder,
                declaration => declaration.Type,
                declaration => $"the type of {declaration}",
                _ => true,
                _ => $"" );
        }

        public static IEligibilityBuilder<INamedType> DeclaringType( this IEligibilityBuilder<IMember> eligibilityBuilder )
            => new ChildEligibilityBuilder<IMember, INamedType>(
                eligibilityBuilder,
                d => d.DeclaringType,
                d => $"{d.Object.DeclaringType}",
                _ => true,
                _ => $"" );

        /// <summary>
        /// Gets an <see cref="IEligibilityBuilder"/> for the same declaration as the current <see cref="IEligibilityBuilder"/>
        /// but that is applicable only to specified <see cref="EligibleScenarios"/>.
        /// </summary>
        public static IEligibilityBuilder<T> ForScenarios<T>( this IEligibilityBuilder<T> eligibilityBuilder, EligibleScenarios excludedScenarios )
        {
            return new ExcludedScenarioEligibilityBuilder<T>( eligibilityBuilder, EligibleScenarios.All & ~excludedScenarios );
        }

        /// <summary>
        /// Gets an <see cref="IEligibilityBuilder"/> for the same declaration as the current <see cref="IEligibilityBuilder"/>
        /// but that is not applicable to specified <see cref="EligibleScenarios"/>.
        /// </summary>
        public static IEligibilityBuilder<T> ExceptForScenarios<T>( this IEligibilityBuilder<T> eligibilityBuilder, EligibleScenarios excludedScenarios )
        {
            return new ExcludedScenarioEligibilityBuilder<T>( eligibilityBuilder, excludedScenarios );
        }

        /// <summary>
        /// Gets an <see cref="IEligibilityBuilder"/> for the same declaration as the current <see cref="IEligibilityBuilder"/>
        /// but that is not applicable when the aspect is inheritable and is applied to a declaration that can be inherited or overridden.
        /// </summary>
        public static IEligibilityBuilder<T> ExceptForInheritance<T>( this IEligibilityBuilder<T> eligibilityBuilder )
        {
            return new ExcludedScenarioEligibilityBuilder<T>( eligibilityBuilder, EligibleScenarios.Inheritance );
        }

        /// <summary>
        /// Adds a group of conditions to the current <see cref="IEligibilityBuilder"/>, where all conditions must be satisfied
        /// by the declaration in order to be eligible for the aspect.
        /// </summary>
        public static void MustSatisfyAny<T>( this IEligibilityBuilder<T> eligibilityBuilder, params Action<IEligibilityBuilder<T>>[] requirements )
            where T : class
        {
            eligibilityBuilder.Aggregate( BooleanCombinationOperator.Or, requirements );
        }

        /// <summary>
        /// Adds a group of conditions to the current <see cref="IEligibilityBuilder"/>, where at least one condition must be satisfied
        /// by the declaration in order to be eligible for the aspect.
        /// </summary>
        public static void MustSatisfyAll<T>( this IEligibilityBuilder<T> eligibilityBuilder, params Action<IEligibilityBuilder<T>>[] requirements )
            where T : class
        {
            eligibilityBuilder.Aggregate( BooleanCombinationOperator.And, requirements );
        }

        public static IEligibilityBuilder<T> If<T>( this IEligibilityBuilder<T> eligibilityBuilder, Predicate<T> condition )
            where T : class
        {
            return new ConditionalEligibilityBuilder<T>( eligibilityBuilder, condition );
        }

        /// <summary>
        /// Adds a condition to the current <see cref="IEligibilityBuilder"/>, where the condition must be
        /// satisfied by the declaration in order to be eligible for the aspect.
        /// </summary>
        /// <param name="eligibilityBuilder"></param>
        /// <param name="predicate">A predicate that returns <c>true</c> if the declaration is eligible for the aspect.</param>
        /// <param name="getJustification">A delegate called in case <paramref name="predicate"/> returns <c>false</c> and when
        /// the justification of the non-ineligibility is required. This delegate must return a <see cref="FormattableString"/>, i.e. a C#
        /// interpolated string (<c>$"like {this}"</c>).</param>
        public static void MustSatisfy<T>(
            this IEligibilityBuilder<T> eligibilityBuilder,
            Predicate<T> predicate,
            Func<IDescribedObject<T>, FormattableString> getJustification )
        {
            eligibilityBuilder.AddRule( new EligibilityRule<T>( eligibilityBuilder.IneligibleScenarios, predicate, getJustification ) );
        }

        /// <summary>
        /// Requires the target method not to have <c>ref</c> or <c>out</c> parameters.
        /// </summary>
        public static void MustNotHaveRefOrOutParameter( this IEligibilityBuilder<IMethod> eligibilityBuilder )
        {
            eligibilityBuilder.AddRule(
                new EligibilityRule<IMethod>(
                    eligibilityBuilder.IneligibleScenarios,
                    m => !m.Parameters.Any( p => p.RefKind is RefKind.Out or RefKind.Ref ),
                    d => $"{d} cannot have any ref or out parameter" ) );
        }

        /// <summary>
        /// Requires the target member to have exactly one of the given accessibilities.
        /// </summary>
        public static void MustHaveAccessibility(
            this IEligibilityBuilder<IMemberOrNamedType> eligibilityBuilder,
            Accessibility accessibility,
            params Accessibility[] otherAccessibilities )
        {
            eligibilityBuilder.MustSatisfy(
                member => member.Accessibility == accessibility || otherAccessibilities.Contains( member.Accessibility ),
                member =>
                {
                    var accessibilities = new[] { accessibility }.Concat( otherAccessibilities ).ToArray();

                    var formattedAccessibilities = string.Join(
                        " or ",
                        accessibilities.Select( a => string.Format( MetalamaExecutionContext.Current.FormatProvider, "{0}", a ) ) );

                    return $"{member} must be {formattedAccessibilities}";
                } );
        }

        /// <summary>
        /// Requires the target property or indexer to be writable.
        /// </summary>
        public static void MustBeWritable<T>( this IEligibilityBuilder<T> eligibilityBuilder )
            where T : IPropertyOrIndexer
        {
            eligibilityBuilder.MustSatisfy(
                member => member.Writeability != Writeability.None,
                member => $"{member} must be a writable {GetInterfaceName<T>()}" );
        }

        /// <summary>
        /// Requires the target field, property or indexer to be writable.
        /// </summary>
        public static void MustBeReadable( this IEligibilityBuilder<IFieldOrPropertyOrIndexer> eligibilityBuilder )
        {
            eligibilityBuilder.MustSatisfyAny(
                b => b.MustBe<IField>(),
                b => b.Convert().To<IFieldOrProperty>().MustSatisfy( d => d.GetMethod != null, d => $"{d} must have a getter" ) );
        }

        /// <summary>
        /// Requires the target property or indexer to be writable.
        /// </summary>
        public static void MustBeReadable( this IEligibilityBuilder<IPropertyOrIndexer> eligibilityBuilder )
        {
            eligibilityBuilder.MustSatisfy( d => d.GetMethod != null, d => $"{d} must have a getter" );
        }

        /// <summary>
        /// Requires the target parameter to be writable, i.e. <c>ref</c> or <c>out</c>.
        /// </summary>
        public static void MustBeWritable( this IEligibilityBuilder<IParameter> eligibilityBuilder )
        {
            eligibilityBuilder.MustSatisfy(
                p => p.RefKind is RefKind.Ref or RefKind.Out,
                member => $"{member} must be an 'out' or 'ref' parameter" );
        }

        /// <summary>
        /// Requires the parameter to be readable, i.e. not <c>out</c>.
        /// </summary>
        public static void MustBeReadable( this IEligibilityBuilder<IParameter> eligibilityBuilder )
        {
            eligibilityBuilder.MustSatisfy(
                p => p.RefKind != RefKind.Out,
                member => $"{member} must not be an 'out' parameter" );
        }

        /// <summary>
        /// Requires the parameter to be <c>ref</c>.
        /// </summary>
        public static void MustBeRef( this IEligibilityBuilder<IParameter> eligibilityBuilder )
        {
            eligibilityBuilder.MustSatisfy(
                p => p.RefKind == RefKind.Ref,
                member => $"{member} must be a 'ref' parameter" );
        }

        /// <summary>
        /// Requires the parameter not to be <c>void</c>.
        /// </summary>
        public static void MustNotBeVoid( this IEligibilityBuilder<IParameter> eligibilityBuilder )
        {
            eligibilityBuilder.MustSatisfy(
                p => !p.Type.Is( SpecialType.Void ),
                member => $"{member} must not have type 'void'" );
        }

        /// <summary>
        /// Requires the declaration to be explicitly declared in source code.
        /// </summary>
        public static void MustBeExplicitlyDeclared( this IEligibilityBuilder<IDeclaration> eligibilityBuilder )
        {
            eligibilityBuilder.MustSatisfy(
                m => !m.IsImplicitlyDeclared,
                m => $"{m} must be explicitly declared" );
        }

        private static string GetInterfaceName<T>() => GetInterfaceName( typeof(T) );

        private static string GetInterfaceName( Type type )
            => type.Name switch
            {
                nameof(IMethod) => "method",
                nameof(IField) => "field",
                nameof(INamedType) => "type",
                nameof(IProperty) => "property",
                nameof(IFieldOrProperty) => "field or a property",
                nameof(IFieldOrPropertyOrIndexer) => "field, property or indexer",
                nameof(IPropertyOrIndexer) => "property or indexer",
                nameof(IMember) => "method, constructor, field, property, indexer or event",
                nameof(IMemberOrNamedType) => "method, constructor, field, property, indexer, event or type",
                nameof(IEvent) => "event",
                nameof(IConstructor) => "constructor",
                nameof(IMethodBase) => "method or constructor",
                nameof(IParameter) => "parameter",
                _ => type.Name
            };

        /// <summary>
        /// Requires the target member or type to be of a certain type.
        /// </summary>
        public static void MustBe<T>( this IEligibilityBuilder<IDeclaration> eligibilityBuilder )
            where T : IDeclaration
        {
            eligibilityBuilder.MustSatisfy(
                member => member is T,
                _ => $"must be a {GetInterfaceName<T>()}" );
        }

        /// <summary>
        /// Requires the target member or type to be static.
        /// </summary>
        public static void MustBeStatic<T>( this IEligibilityBuilder<T> eligibilityBuilder )
            where T : IMemberOrNamedType
        {
            eligibilityBuilder.MustSatisfy(
                member => member.IsStatic,
                member => $"{member} must be a static {GetInterfaceName<T>()}" );
        }

        /// <summary>
        /// Requires the target member or type to be non-static.
        /// </summary>
        public static void MustBeNonStatic<T>( this IEligibilityBuilder<T> eligibilityBuilder )
            where T : IMemberOrNamedType
        {
            eligibilityBuilder.MustSatisfy(
                member => !member.IsStatic,
                member => $"{member} must be a non-static {GetInterfaceName<T>()}" );
        }

        /// <summary>
        /// Requires the target member or type to be non-abstract.
        /// </summary>
        public static void MustBeNonAbstract<T>( this IEligibilityBuilder<T> eligibilityBuilder )
            where T : IMemberOrNamedType
        {
            eligibilityBuilder.MustSatisfy(
                member => !member.IsAbstract,
                member => $"{member} must be a non-abstract {GetInterfaceName<T>()} " );
        }

        /// <summary>
        /// Requires the target type to be convertible to a given type (specified as a reflection <see cref="System.Type"/>).
        /// </summary>
        public static void MustBe( this IEligibilityBuilder<IType> eligibilityBuilder, Type type, ConversionKind conversionKind = ConversionKind.Default )
        {
            eligibilityBuilder.MustSatisfy(
                t => t.Is( type, conversionKind ),
                member => $"{member} must be of type '{type}'" );
        }

        /// <summary>
        /// Requires the target type to be convertible to a given type (specified as an <see cref="IType"/>).
        /// </summary>
        public static void MustBe( this IEligibilityBuilder<IType> eligibilityBuilder, IType type, ConversionKind conversionKind = ConversionKind.Default )
        {
            eligibilityBuilder.MustSatisfy(
                t => t.Is( type, conversionKind ),
                member => $"{member} must be of type '{type}'" );
        }

        /// <summary>
        /// Requires the target type to be convertible to a given type (specified as a type parameter).
        /// </summary>
        public static void MustBe<T>( this IEligibilityBuilder<IType> eligibilityBuilder, ConversionKind conversionKind = ConversionKind.Default )
        {
            eligibilityBuilder.MustBe( typeof(T), conversionKind );
        }

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
    }
}