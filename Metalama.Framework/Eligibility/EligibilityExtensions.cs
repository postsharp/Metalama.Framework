// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility.Implementation;
using Metalama.Framework.Project;
using System;
using System.Linq;

namespace Metalama.Framework.Eligibility;

/// <summary>
/// Extension methods for <see cref="IEligibilityBuilder"/>.
/// </summary>
/// <seealso href="@eligibility"/>
[CompileTime]
public static partial class EligibilityExtensions
{
    /// <summary>
    /// Gets an <see cref="IEligibilityBuilder"/> for the declaring type of the member validated by the given <see cref="IEligibilityBuilder"/>.
    /// </summary>
    public static IEligibilityBuilder<INamedType> DeclaringType<T>( this IEligibilityBuilder<T> eligibilityBuilder )
        where T : class, IMemberOrNamedType
        => new ChildEligibilityBuilder<T, INamedType>(
            eligibilityBuilder,
            declaration => declaration as INamedType ?? declaration.DeclaringType!,
            declarationDescription => $"the declaring type '{declarationDescription.Object.DeclaringType}'" );

    /// <summary>
    /// Gets an <see cref="IEligibilityBuilder"/> for the declaring method or property of the parameter validated by the given <see cref="IEligibilityBuilder"/>.
    /// </summary>
    public static IEligibilityBuilder<IHasParameters> DeclaringMember( this IEligibilityBuilder<IParameter> eligibilityBuilder )
        => new ChildEligibilityBuilder<IParameter, IHasParameters>(
            eligibilityBuilder,
            parameter => parameter.DeclaringMember,
            description => $"the parent member '{description.Object.DeclaringMember}'" );

    /// <summary>
    /// Gets an object that allows to convert the given <see cref="IEligibilityBuilder"/> into an <see cref="IEligibilityBuilder"/> for a more specific type.
    /// </summary>
    public static Converter<T> Convert<T>( this IEligibilityBuilder<T> eligibilityBuilder )
        where T : class
        => new( eligibilityBuilder );

    /// <summary>
    /// Gets an <see cref="IEligibilityBuilder"/> for the return type of the method validated by the given <see cref="IEligibilityBuilder"/>.
    /// </summary>
    public static IEligibilityBuilder<IType> ReturnType( this IEligibilityBuilder<IMethod> eligibilityBuilder )
        => new ChildEligibilityBuilder<IMethod, IType>(
            eligibilityBuilder,
            declaration => declaration.ReturnType,
            declarationDescription => $"the return type of {declarationDescription}" );

    public static IEligibilityBuilder<IParameter> ReturnParameter( this IEligibilityBuilder<IMethod> eligibilityBuilder )
        => new ChildEligibilityBuilder<IMethod, IParameter>(
            eligibilityBuilder,
            method => method.ReturnParameter,
            m => $"the return parameter of {m}" );

    /// <summary>
    /// Gets an <see cref="IEligibilityBuilder"/> for a parameter of the method validated by the given <see cref="IEligibilityBuilder"/>,
    /// identified by index.
    /// </summary>
    public static IEligibilityBuilder<IParameter> Parameter( this IEligibilityBuilder<IHasParameters> eligibilityBuilder, int index )
        => new ChildEligibilityBuilder<IHasParameters, IParameter>(
            eligibilityBuilder,
            declaration => declaration.Parameters[index],
            method => $"the {index + 1}-th parameter of {method}",
            method => index < method.Parameters.Count,
            method => $"{method} has fewer than {index + 1} parameter(s)" );

    /// <summary>
    /// Gets an <see cref="IEligibilityBuilder"/> for a parameter of the method validated by the given <see cref="IEligibilityBuilder"/>,
    /// identified by name.
    /// </summary>
    public static IEligibilityBuilder<IParameter> Parameter( this IEligibilityBuilder<IHasParameters> eligibilityBuilder, string name )
        => new ChildEligibilityBuilder<IHasParameters, IParameter>(
            eligibilityBuilder,
            declaration => declaration.Parameters[name],
            method => $"parameter '{name}' of {method}",
            method => method.Parameters.All( p => p.Name != name ),
            method => $"{method} has no parameter named '{name}'" );

    /// <summary>
    /// Gets an <see cref="IEligibilityBuilder"/> for the type of the declaration validated by the given <see cref="IEligibilityBuilder"/>.
    /// </summary>
    public static IEligibilityBuilder<IType> Type( this IEligibilityBuilder<IHasType> eligibilityBuilder )
        => new ChildEligibilityBuilder<IHasType, IType>(
            eligibilityBuilder,
            declaration => declaration.Type,
            declaration => $"the type of {declaration}",
            _ => true,
            _ => $"" );

    /// <summary>
    /// Gets an <see cref="IEligibilityBuilder"/> for the same declaration as the given <see cref="IEligibilityBuilder"/>
    /// but that is applicable only to specified <see cref="EligibleScenarios"/>.
    /// </summary>
    public static IEligibilityBuilder<T> ForScenarios<T>( this IEligibilityBuilder<T> eligibilityBuilder, EligibleScenarios excludedScenarios )
        where T : class
        => new ExcludedScenarioEligibilityBuilder<T>( eligibilityBuilder, EligibleScenarios.All & ~excludedScenarios );

    /// <summary>
    /// Gets an <see cref="IEligibilityBuilder"/> for the same declaration as the given <see cref="IEligibilityBuilder"/>
    /// but that is not applicable to specified <see cref="EligibleScenarios"/>.
    /// </summary>
    public static IEligibilityBuilder<T> ExceptForScenarios<T>( this IEligibilityBuilder<T> eligibilityBuilder, EligibleScenarios excludedScenarios )
        where T : class
        => new ExcludedScenarioEligibilityBuilder<T>( eligibilityBuilder, excludedScenarios );

    /// <summary>
    /// Gets an <see cref="IEligibilityBuilder"/> for the same declaration as the given <see cref="IEligibilityBuilder"/>
    /// but that is not applicable when the aspect is inheritable and is applied to a declaration that can be inherited or overridden.
    /// </summary>
    public static IEligibilityBuilder<T> ExceptForInheritance<T>( this IEligibilityBuilder<T> eligibilityBuilder )
        where T : class
        => new ExcludedScenarioEligibilityBuilder<T>( eligibilityBuilder, EligibleScenarios.Inheritance );

    /// <summary>
    /// Adds a group of conditions to the given <see cref="IEligibilityBuilder"/>, where all conditions must be satisfied
    /// by the declaration in order to be eligible for the aspect.
    /// </summary>
    public static void MustSatisfyAny<T>( this IEligibilityBuilder<T> eligibilityBuilder, params Action<IEligibilityBuilder<T>>[] requirements )
        where T : class
        => eligibilityBuilder.Aggregate( BooleanCombinationOperator.Or, requirements );

    /// <summary>
    /// Adds a group of conditions to the given <see cref="IEligibilityBuilder"/>, where at least one condition must be satisfied
    /// by the declaration in order to be eligible for the aspect.
    /// </summary>
    public static void MustSatisfyAll<T>( this IEligibilityBuilder<T> eligibilityBuilder, params Action<IEligibilityBuilder<T>>[] requirements )
        where T : class
        => eligibilityBuilder.Aggregate( BooleanCombinationOperator.And, requirements );

    /// <summary>
    /// Adds a rule to the given <see cref="IEligibilityBuilder"/>, but only if the validate object satisfies a given predicate.
    /// Otherwise, the rule is ignored.
    /// </summary>
    public static IEligibilityBuilder<T> If<T>( this IEligibilityBuilder<T> eligibilityBuilder, Predicate<T> condition )
        where T : class
        => new ConditionalEligibilityBuilder<T>( eligibilityBuilder, condition );

    /// <summary>
    /// Adds a condition to the given <see cref="IEligibilityBuilder"/>, where the condition must be
    /// satisfied by the declaration in order to be eligible for the aspect. The new rule is given as a <see cref="Predicate{T}"/>.
    /// </summary>
    /// <param name="eligibilityBuilder">The parent <see cref="IEligibilityBuilder"/>.</param>
    /// <param name="predicate">A predicate that returns <c>true</c> if the declaration is eligible for the aspect.</param>
    /// <param name="getJustification">A delegate called in case <paramref name="predicate"/> returns <c>false</c> and when
    /// the justification of the non-ineligibility is required. This delegate must return a <see cref="FormattableString"/>, i.e. a C#
    /// interpolated string (<c>$"like {this}"</c>).</param>
    public static void MustSatisfy<T>(
        this IEligibilityBuilder<T> eligibilityBuilder,
        Predicate<T> predicate,
        Func<IDescribedObject<T>, FormattableString> getJustification )
        where T : class
        => eligibilityBuilder.AddRule( new EligibilityRule<T>( eligibilityBuilder.IneligibleScenarios, predicate, getJustification ) );

    /// <summary>
    /// Adds a condition to the given <see cref="IEligibilityBuilder"/>, where the condition must be
    /// satisfied by the declaration in order to be eligible for the aspect. The new rule is built using a child <see cref="IEligibilityBuilder"/>.
    /// </summary>
    public static void MustSatisfy<T>( this IEligibilityBuilder<T> eligibilityBuilder, Action<IEligibilityBuilder<T>> requirement )
        where T : class
        => requirement( eligibilityBuilder );

    /// <summary>
    /// Requires the target method not to have <c>ref</c> or <c>out</c> parameters.
    /// </summary>
    public static void MustNotHaveRefOrOutParameter( this IEligibilityBuilder<IMethod> eligibilityBuilder )
        => eligibilityBuilder.AddRule(
            new EligibilityRule<IMethod>(
                eligibilityBuilder.IneligibleScenarios,
                m => !m.Parameters.Any( p => p.RefKind is RefKind.Out or RefKind.Ref ),
                d => $"{d} cannot have any ref or out parameter" ) );

    /// <summary>
    /// Requires the target member to have exactly one of the given accessibilities.
    /// </summary>
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
                    accessibilities.Select( a => string.Format( MetalamaExecutionContext.Current.FormatProvider, "{0}", a ) ) );

                return $"{member} must be {formattedAccessibilities}";
            } );

    /// <summary>
    /// Requires the target property or indexer to be writable.
    /// </summary>
    public static void MustBeWritable( this IEligibilityBuilder<IFieldOrPropertyOrIndexer> eligibilityBuilder )
        => eligibilityBuilder.MustSatisfy(
            member => member.Writeability != Writeability.None,
            member => $"{member} must be writable" );

    /// <summary>
    /// Requires the target field, property or indexer to be writable.
    /// </summary>
    public static void MustBeReadable( this IEligibilityBuilder<IFieldOrPropertyOrIndexer> eligibilityBuilder )
        => eligibilityBuilder.MustSatisfyAny(
            b => b.MustBeOfType( typeof(IField) ),
            b => b.Convert().To<IFieldOrProperty>().MustSatisfy( d => d.GetMethod != null, d => $"{d} must have a getter" ) );

    /// <summary>
    /// Requires the target parameter to be writable, i.e. <c>ref</c> or <c>out</c>.
    /// </summary>
    public static void MustBeWritable( this IEligibilityBuilder<IParameter> eligibilityBuilder )
        => eligibilityBuilder.MustSatisfy(
            p => p.RefKind is RefKind.Ref or RefKind.Out,
            member => $"{member} must be an 'out' or 'ref' parameter" );

    /// <summary>
    /// Requires the parameter to be readable, i.e. not <c>out</c>.
    /// </summary>
    public static void MustBeReadable( this IEligibilityBuilder<IParameter> eligibilityBuilder )
        => eligibilityBuilder.MustSatisfy(
            p => p.RefKind != RefKind.Out,
            member => $"{member} must not be an 'out' parameter" );

    /// <summary>
    /// Requires the parameter to be the return parameter.
    /// </summary>
    public static void MustBeReturnParameter( this IEligibilityBuilder<IParameter> eligibilityBuilder )
        => eligibilityBuilder.MustSatisfy(
            p => p.IsReturnParameter,
            member => $"{member} must be the return value parameter" );

    /// <summary>
    /// Forbids the parameter from being the return parameter.
    /// </summary>
    public static void MustNotBeReturnParameter( this IEligibilityBuilder<IParameter> eligibilityBuilder )
        => eligibilityBuilder.MustSatisfy(
            p => !p.IsReturnParameter,
            member => $"{member} must not be the return value parameter" );

    /// <summary>
    /// Requires the parameter to be <c>ref</c>.
    /// </summary>
    public static void MustBeRef( this IEligibilityBuilder<IParameter> eligibilityBuilder )
        => eligibilityBuilder.MustSatisfy(
            p => p.RefKind == RefKind.Ref,
            member => $"{member} must be a 'ref' parameter" );

    /// <summary>
    /// Requires the parameter not to be <c>void</c>.
    /// </summary>
    public static void MustNotBeVoid( this IEligibilityBuilder<IParameter> eligibilityBuilder )
        => eligibilityBuilder.MustSatisfy(
            p => !p.Type.Is( SpecialType.Void ),
            member => $"{member} must not be void" );

    /// <summary>
    /// Requires the declaration to be explicitly declared in source code.
    /// </summary>
    public static void MustBeExplicitlyDeclared( this IEligibilityBuilder<IDeclaration> eligibilityBuilder )
        => eligibilityBuilder.MustSatisfy(
            m => !m.IsImplicitlyDeclared,
            m => $"{m} must be explicitly declared" );

    /// <summary>
    /// Forbids the field, property or indexer from being <c>ref</c> or <c>ref readonly</c>.
    /// </summary>
    public static void MustNotBeRef( this IEligibilityBuilder<IFieldOrPropertyOrIndexer> eligibilityBuilder )
        => eligibilityBuilder.MustSatisfy( f => f.RefKind == RefKind.None, f => $"{f} must not be 'ref'" );

    /// <summary>
    /// Forbids the type from being <c>ref struct</c>.
    /// </summary>
    public static void MustNotBeRef( this IEligibilityBuilder<INamedType> eligibilityBuilder )
        => eligibilityBuilder.MustSatisfy( f => !f.IsRef, f => $"{f} must not be a 'ref struct'" );

    /// <summary>
    /// Forbids the method from returning a <c>ref</c>.
    /// </summary>
    public static void MustNotBeRef( this IEligibilityBuilder<IMethod> eligibilityBuilder )
        => eligibilityBuilder.MustSatisfy( f => f.ReturnParameter.RefKind == RefKind.None, f => $"{f} must not be a 'ref' method" );

    private static string GetInterfaceName<T>() => GetInterfaceName( typeof(T) );

    private static string GetInterfaceName( Type type ) => GetInterfaceName( type.Name );

    private static string GetInterfaceName( string typeName )
        => typeName switch
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
            _ => typeName
        };

    /// <summary>
    /// Requires the validated object to be of a certain type. Note that this validates the object itself, not the declaration
    /// that it represents. For instance, if the object is an <see cref="IParameter"/> and the <paramref name="type"/> parameter
    /// is set to <c>string</c>, this method will fail with an exception no conversion exists from <see cref="IParameter"/> to <c>string</c>.
    /// </summary>
    public static void MustBeOfType<T>( this IEligibilityBuilder<T> eligibilityBuilder, Type type )
        where T : class
    {
        if ( !typeof(T).IsAssignableFrom( type ) )
        {
            throw new ArgumentOutOfRangeException( nameof(type), $"An object of type '{typeof(T)}' can never be converted to the type '{type}'." );
        }

        eligibilityBuilder.MustSatisfy(
            member => type.IsAssignableFrom( member.GetType() ),
            d => $"{d} cannot be converted to an {GetInterfaceName<T>()}" );
    }

    /// <summary>
    /// Requires the validated object to be of one of the specified types. Note that this validates the object itself, not the declaration
    /// that it represents. For instance, if the object is an <see cref="IParameter"/> and the <paramref name="types"/> parameter
    /// is set to <c>string</c>, this method will fail with an exception no conversion exists from <see cref="IParameter"/> to <c>string</c>.
    /// </summary>
    public static void MustBeOfAnyType<T>(
        this IEligibilityBuilder<T> eligibilityBuilder,
        params Type[] types )
        where T : class
    {
        foreach ( var type in types )
        {
            if ( !typeof(T).IsAssignableFrom( type ) )
            {
                throw new ArgumentOutOfRangeException( nameof(types), $"An object of type '{typeof(T)}' can never be converted to the type '{type}'." );
            }
        }

        eligibilityBuilder.MustSatisfy(
            t => types.Any( i => i.IsAssignableFrom( t.GetType() ) ),
            member => $"{member} cannot be converted to an {string.Join( " or ", types.Select( GetInterfaceName ) )}" );
    }

    /// <summary>
    /// Requires the target type to be run-time, as opposed to compile-time or run-time-or-compile-time.
    /// </summary>
    /// <seealso cref="CompileTimeAttribute"/>
    /// <seealso cref="RunTimeOrCompileTimeAttribute"/>
    public static void MustBeRunTimeOnly( this IEligibilityBuilder<INamedType> eligibilityBuilder )
        => eligibilityBuilder.MustSatisfy(
            member => member.ExecutionScope == ExecutionScope.RunTime,
            member => $"the execution scope of {member} must run-time but is {member.Object.ExecutionScope}" );

    /// <summary>
    /// Requires the target member or type to be static.
    /// </summary>
    public static void MustBeStatic( this IEligibilityBuilder<IMemberOrNamedType> eligibilityBuilder )
        => eligibilityBuilder.MustSatisfy(
            member => member.IsStatic,
            member => $"{member} must be static" );

    /// <summary>
    /// Forbids the target member or type from being static.
    /// </summary>
    public static void MustNotBeStatic( this IEligibilityBuilder<IMemberOrNamedType> eligibilityBuilder )
        => eligibilityBuilder.MustSatisfy(
            member => !member.IsStatic,
            member => $"{member} must not be static" );

    /// <summary>
    /// Forbids the target method from being extern.
    /// </summary>
    public static void MustNotBeExtern( this IEligibilityBuilder<IMethod> eligibilityBuilder )
        => eligibilityBuilder.MustSatisfy(
            member => !member.IsExtern,
            member => $"{member} must not be extern" );

    /// <summary>
    /// Forbids the target member or type from being abstract.
    /// </summary>
    public static void MustNotBeAbstract( this IEligibilityBuilder<IMemberOrNamedType> eligibilityBuilder )
        => eligibilityBuilder.MustSatisfy(
            member => !member.IsAbstract,
            member => $"{member} must not be abstract" );

    /// <summary>
    /// Requires the target type to be convertible to a given type (specified as a reflection <see cref="System.Type"/>).
    /// </summary>
    public static void MustBe( this IEligibilityBuilder<IType> eligibilityBuilder, Type type, ConversionKind conversionKind = ConversionKind.Default )
        => eligibilityBuilder.MustSatisfy(
            t => t.Is( type, conversionKind ),
            member => $"{member} must be a '{GetInterfaceName( type )}'" );

    /// <summary>
    /// Requires the target type to be convertible to a given type (specified as an <see cref="IType"/>).
    /// </summary>
    public static void MustBe( this IEligibilityBuilder<IType> eligibilityBuilder, IType type, ConversionKind conversionKind = ConversionKind.Default )
        => eligibilityBuilder.MustSatisfy(
            t => t.Is( type, conversionKind ),
            member => $"{member} must be of type '{type}'" );

    /// <summary>
    /// Requires the target type to be convertible to a given type (specified as a type parameter).
    /// </summary>
    public static void MustBe<T>( this IEligibilityBuilder<IType> eligibilityBuilder, ConversionKind conversionKind = ConversionKind.Default )
        => eligibilityBuilder.MustBe( typeof(T), conversionKind );

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

    /// <summary>
    /// Requires the target declaration to have an aspect of a given type.
    /// </summary>
    /// <param name="eligibilityBuilder">An <see cref="IEligibilityBuilder{T}"/> for the target declaration.</param>
    /// <param name="aspectType">The exact aspect type. Derived types are not taken into account.</param>
    public static void MustHaveAspectOfType( this IEligibilityBuilder<IDeclaration> eligibilityBuilder, Type aspectType )
        => eligibilityBuilder.MustSatisfy(
            d => d.Enhancements().HasAspect( aspectType ),
            d => $"{d} must have an aspect of type {aspectType.Name}" );

    /// <summary>
    /// Forbids the target declaration from having an aspect of a given type.
    /// </summary>
    /// <param name="eligibilityBuilder">An <see cref="IEligibilityBuilder{T}"/> for the target declaration.</param>
    /// <param name="aspectType">The exact aspect type. Derived types are not taken into account.</param>
    public static void MustNotHaveAspectOfType( this IEligibilityBuilder<IDeclaration> eligibilityBuilder, Type aspectType )
        => eligibilityBuilder.MustSatisfy(
            d => !d.Enhancements().HasAspect( aspectType ),
            d => $"{d} must not have an aspect of type {aspectType.Name}" );

    /// <summary>
    /// Determines whether the given declaration is an eligible target for a specified aspect type given as a type parameter.
    /// </summary>
    /// <param name="declaration">The declaration for which eligibility is determined.</param>
    /// <param name="scenarios">The scenarios for which eligibility is determined. The default value is <see cref="EligibleScenarios.Aspect"/>.</param>
    /// <typeparam name="T">The aspect type.</typeparam>
    /// <returns><c>true</c> if <paramref name="declaration"/> is eligible for the aspect type <typeparamref name="T"/> for any of the specified <paramref name="scenarios"/>.</returns>
    public static bool IsAspectEligible<T>( this IDeclaration declaration, EligibleScenarios scenarios = EligibleScenarios.Aspect )
        where T : IAspect
        => MetalamaExecutionContext.Current.ServiceProvider.GetRequiredService<IEligibilityService>().IsEligible( typeof(T), declaration, scenarios );

    /// <summary>
    /// Determines whether the given declaration is an eligible target for a specified aspect type given as a reflection <see cref="Type"/>.
    /// </summary>
    /// <param name="declaration">The declaration for which eligibility is determined.</param>
    /// <param name="aspectType">The aspect type.</param>
    /// <param name="scenarios">The scenarios for which eligibility is determined. The default value is <see cref="EligibleScenarios.Aspect"/>.</param>
    /// <returns><c>true</c> if <paramref name="declaration"/> is eligible for the given <paramref name="aspectType"/> for any of the specified <paramref name="scenarios"/>.</returns>
    public static bool IsAspectEligible( this IDeclaration declaration, Type aspectType, EligibleScenarios scenarios = EligibleScenarios.Aspect )
        => MetalamaExecutionContext.Current.ServiceProvider.GetRequiredService<IEligibilityService>().IsEligible( aspectType, declaration, scenarios );

    /// <summary>
    /// Determines whether the given declaration is an eligible target for a specified kind of advice.
    /// </summary>
    /// <param name="declaration">The declaration for which eligibility is determined.</param>
    /// <param name="adviceKind">Tha advice kind, but not <see cref="AdviceKind.AddContract"/>.</param>
    /// <returns><c>true</c> if <paramref name="declaration"/> is eligible for the given <paramref name="adviceKind"/>.</returns>
    /// <seealso cref="IsContractAdviceEligible"/>
    public static bool IsAdviceEligible( this IDeclaration declaration, AdviceKind adviceKind )
        => (EligibilityRuleFactory.GetAdviceEligibilityRule( adviceKind ).GetEligibility( declaration ) & EligibleScenarios.Aspect) != 0;

    /// <summary>
    ///  Determines whether the given declaration is an eligible target for an <see cref="AdviceKind.AddContract"/> advice for a given <see cref="ContractDirection"/>.
    /// </summary>
    /// <param name="declaration">The declaration for which eligibility is determined.</param>
    /// <param name="contractDirection">The contract direction.</param>
    /// <returns><c>true</c> if <paramref name="declaration"/> is eligible for an <see cref="AdviceKind.AddContract"/> advice for the given <paramref name="contractDirection"/>.</returns>
    public static bool IsContractAdviceEligible( this IDeclaration declaration, ContractDirection contractDirection = ContractDirection.Default )
        => (EligibilityRuleFactory.GetContractAdviceEligibilityRule( contractDirection ).GetEligibility( declaration ) & EligibleScenarios.Aspect) != 0;
}