// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;

namespace Metalama.Framework.Eligibility;

public static partial class EligibilityRuleFactory
{
    private static class Contracts
    {
        private static readonly IEligibilityRule<IDeclaration> _contractEligibilityBoth;
        private static readonly IEligibilityRule<IDeclaration> _contractEligibilityInput;
        private static readonly IEligibilityRule<IDeclaration> _contractEligibilityOutput;
        private static readonly IEligibilityRule<IDeclaration> _contractEligibilityDefault;

        static Contracts()
        {
            // Eligibility rules for fields, properties and indexers. Note that we always skip constant foelds.
            var propertyOrIndexerEligibilityInput =
                CreateRule<IFieldOrPropertyOrIndexer>(
                    fieldOrPropertyOrIndexer =>
                    {
                        fieldOrPropertyOrIndexer.MustBeWritable();
                        fieldOrPropertyOrIndexer.MustBeExplicitlyDeclared();
                    } );

            var propertyOrIndexerEligibilityOutput =
                CreateRule<IFieldOrPropertyOrIndexer>(
                    fieldOrPropertyOrIndexer
                        => fieldOrPropertyOrIndexer.Convert()
                            .When<IPropertyOrIndexer>()
                            .MustSatisfy(
                                p =>
                                {
                                    p.MustBeReadable();
                                    p.MustBeExplicitlyDeclared();
                                } ) );

            var propertyOrIndexerEligibilityBoth =
                CreateRule<IFieldOrPropertyOrIndexer>(
                    builder =>
                    {
                        builder.MustBeReadable();
                        builder.MustBeWritable();
                    } );

            var propertyOrIndexerEligibilityDefault =
                CreateRule<IFieldOrPropertyOrIndexer>(
                    builder =>
                    {
                        builder.MustBeExplicitlyDeclared();
                        builder.Convert().When<IField>().MustBeWritable();
                    } );

            // Eligibility rules for parameters.
            var parameterEligibilityInput =
                CreateRule<IParameter>(
                    parameter =>
                    {
                        parameter.MustNotBeReturnParameter();
                        parameter.MustBeReadable();
                        parameter.DeclaringMember().MustBeExplicitlyDeclared();
                        parameter.ExceptForInheritance().DeclaringMember().MustNotBeAbstract();
                        parameter.MustNotBeVoid();
                    } );

            var parameterEligibilityOutput =
                CreateRule<IParameter>(
                    parameter =>
                    {
                        parameter.MustBeWritable();
                        parameter.DeclaringMember().MustBeExplicitlyDeclared();
                        parameter.MustSatisfy( p => p.DeclaringMember is not IConstructor, _ => $"output contracts on constructors are not supported" );
                        parameter.ExceptForInheritance().DeclaringMember().MustNotBeAbstract();
                        parameter.MustNotBeVoid();
                    } );

            var parameterEligibilityBoth =
                CreateRule<IParameter>(
                    parameter =>
                    {
                        parameter.MustNotBeReturnParameter();
                        parameter.MustBeRef();
                        parameter.DeclaringMember().MustBeExplicitlyDeclared();
                        parameter.MustSatisfy( p => p.DeclaringMember is not IConstructor, _ => $"output contracts on constructors are not supported" );
                        parameter.ExceptForInheritance().DeclaringMember().MustNotBeAbstract();
                        parameter.MustNotBeVoid();
                    } );

            var parameterEligibilityDefault =
                CreateRule<IParameter>(
                    parameter =>
                    {
                        parameter.DeclaringMember().MustBeExplicitlyDeclared();

                        parameter.MustSatisfy(
                            p => !(p.RefKind == RefKind.Out && p.DeclaringMember is IConstructor),
                            _ => $"output contracts on constructors are not supported" );

                        parameter.ExceptForInheritance().DeclaringMember().MustNotBeAbstract();
                        parameter.MustNotBeVoid();
                    } );

            _contractEligibilityBoth = CreateRule<IDeclaration>(
                d =>
                {
                    d.MustBeOfAnyType( typeof(IParameter), typeof(IFieldOrProperty) );
                    d.Convert().When<IParameter>().AddRule( parameterEligibilityBoth );
                    d.Convert().When<IFieldOrProperty>().AddRule( propertyOrIndexerEligibilityBoth );
                } );

            _contractEligibilityInput = CreateRule<IDeclaration>(
                d =>
                {
                    d.MustBeOfAnyType( typeof(IParameter), typeof(IFieldOrProperty) );
                    d.Convert().When<IParameter>().AddRule( parameterEligibilityInput );
                    d.Convert().When<IFieldOrProperty>().AddRule( propertyOrIndexerEligibilityInput );
                } );

            _contractEligibilityOutput = CreateRule<IDeclaration>(
                d =>
                {
                    d.MustBeOfAnyType( typeof(IParameter), typeof(IFieldOrProperty) );
                    d.Convert().When<IParameter>().AddRule( parameterEligibilityOutput );
                    d.Convert().When<IFieldOrProperty>().AddRule( propertyOrIndexerEligibilityOutput );
                } );

            _contractEligibilityDefault = CreateRule<IDeclaration>(
                d =>
                {
                    d.MustBeOfAnyType( typeof(IParameter), typeof(IFieldOrProperty) );
                    d.Convert().When<IParameter>().AddRule( parameterEligibilityDefault );
                    d.Convert().When<IFieldOrProperty>().AddRule( propertyOrIndexerEligibilityDefault );
                } );
        }

        public static IEligibilityRule<IDeclaration> GetEligibilityRule( ContractDirection direction )
            => direction switch
            {
                ContractDirection.Default => _contractEligibilityDefault,
                ContractDirection.Both => _contractEligibilityBoth,
                ContractDirection.Input => _contractEligibilityInput,
                ContractDirection.Output => _contractEligibilityOutput,
                _ => throw new ArgumentOutOfRangeException( nameof(direction) )
            };
    }
}