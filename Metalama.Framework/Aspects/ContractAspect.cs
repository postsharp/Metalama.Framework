// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;
using System;

namespace Metalama.Framework.Aspects
{
    /// <summary>
    /// A base aspect that can validate or change the value of fields, properties, indexers, and parameters.
    /// </summary>
    /// <remarks>
    /// <para>A contract aspect can apply to the input or output data flow, or to both data flows, according to the <see cref="ContractDirection"/> value
    /// passed to the constructor. Since the current class does not know the value of this parameter before it is instantiated, this class cannot
    /// set the eligibility conditions using the <see cref="BuildEligibility(Metalama.Framework.Eligibility.IEligibilityBuilder{Metalama.Framework.Code.IFieldOrPropertyOrIndexer})"/> method.
    /// If a derived class targets a specific <see cref="ContractDirection"/> (i.e. if the choice is not left to the user),
    /// its implementation of <see cref="BuildEligibility(Metalama.Framework.Eligibility.IEligibilityBuilder{Metalama.Framework.Code.IFieldOrPropertyOrIndexer})"/>
    /// can call <see cref="BuildEligibilityForDirection(Metalama.Framework.Eligibility.IEligibilityBuilder{Metalama.Framework.Code.IFieldOrPropertyOrIndexer},ContractDirection)"/>
    /// methods. This means that eligibility can be checked upfront by the IDE before suggesting the code actions.
    /// </para>
    /// <para>
    /// In any case, this aspect verifies the eligibility of the target with respect to the specific <see cref="ContractDirection"/> and target declaration. This verification
    /// cannot be skipped.
    /// </para>
    /// </remarks>
    [AttributeUsage( AttributeTargets.ReturnValue | AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property )]
    public abstract class ContractAspect : Aspect, IAspect<IParameter>, IAspect<IFieldOrPropertyOrIndexer>
    {
        // Eligibility rules for properties.
        private static readonly IEligibilityRule<IFieldOrPropertyOrIndexer> _propertyOrIndexerEligibilityInput =
            EligibilityRuleFactory.CreateRule<IFieldOrPropertyOrIndexer>( builder => builder.Convert().To<IPropertyOrIndexer>().MustBeWritable() );

        private static readonly IEligibilityRule<IFieldOrPropertyOrIndexer> _propertyOrIndexerEligibilityOutput =
            EligibilityRuleFactory.CreateRule<IFieldOrPropertyOrIndexer>( builder => builder.Convert().To<IPropertyOrIndexer>().MustBeReadable() );

        private static readonly IEligibilityRule<IFieldOrPropertyOrIndexer> _propertyOrIndexerEligibilityBoth =
            EligibilityRuleFactory.CreateRule<IFieldOrPropertyOrIndexer>(
                builder => builder.Convert().To<IPropertyOrIndexer>().MustSatisfyAll( b => b.MustBeReadable(), b => b.MustBeWritable() ) );

        // Eligibility rules for parameters.
        private static readonly IEligibilityRule<IParameter> _parameterEligibilityInput =
            EligibilityRuleFactory.CreateRule<IParameter>( builder => builder.MustBeReadable() );

        private static readonly IEligibilityRule<IParameter> _parameterEligibilityOutput =
            EligibilityRuleFactory.CreateRule<IParameter>( builder => builder.MustBeWritable() );

        private static readonly IEligibilityRule<IParameter> _parameterEligibilityBoth =
            EligibilityRuleFactory.CreateRule<IParameter>( builder => builder.MustBeRef() );

        // Eligibility rules for return parameters.
        private static readonly IEligibilityRule<IParameter> _returnValueEligibilityMustNotBeVoid =
            EligibilityRuleFactory.CreateRule<IParameter>( builder => builder.MustNotBeVoid() );

        private static readonly Func<ContractDirection, IEligibilityRule<IParameter>> _returnValueEligibilityInvalidDirection =
            direction =>
                EligibilityRuleFactory.CreateRule<IParameter>(
                    builder => builder.MustSatisfy( x => false, x => $"Contract with \"{direction}\" direction is not valid on return parameter." ) );

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractAspect"/> class.
        /// </summary>
        /// <param name="direction">The direction of the data flow (<see cref="ContractDirection.Input"/>,  <see cref="ContractDirection.Output"/> or <see cref="ContractDirection.Both"/>)
        /// to which this contract applies. See the <see cref="ContractDirection"/> for details.</param>
        protected ContractAspect( ContractDirection direction = ContractDirection.Default )
        {
            this.Direction = direction;
        }

        /// <summary>
        /// Gets the direction of the data flow (<see cref="ContractDirection.Input"/>,  <see cref="ContractDirection.Output"/> or <see cref="ContractDirection.Both"/>)
        /// to which this contract applies.
        /// </summary>
        /// <remarks>
        /// It is the responsibility of the <i>author</i> of the aspect, and not of its <i>user</i>, to define the eligible directions of a contract.
        /// </remarks>
        protected ContractDirection Direction { get; }

        private static IEligibilityRule<IParameter> GetParameterEligibilityRule( ContractDirection direction )
            => direction switch
            {
                ContractDirection.Default => EligibilityRuleFactory.CreateRule<IParameter>( builder => { } ),
                ContractDirection.Both => _parameterEligibilityBoth,
                ContractDirection.Input => _parameterEligibilityInput,
                ContractDirection.Output => _parameterEligibilityOutput,
                _ => throw new ArgumentOutOfRangeException( nameof(direction) )
            };

        private static IEligibilityRule<IParameter> GetReturnParameterEligibilityRule( ContractDirection direction )
            => direction switch
            {
                ContractDirection.Default => _returnValueEligibilityMustNotBeVoid,
                ContractDirection.Both => _returnValueEligibilityInvalidDirection( ContractDirection.Both ),
                ContractDirection.Input => _returnValueEligibilityInvalidDirection( ContractDirection.Input ),
                ContractDirection.Output => _returnValueEligibilityMustNotBeVoid,
                _ => throw new ArgumentOutOfRangeException( nameof(direction) )
            };

        private static IEligibilityRule<IFieldOrPropertyOrIndexer> GetPropertyEligibilityRule( ContractDirection direction )
            => direction switch
            {
                ContractDirection.Default => EligibilityRuleFactory.CreateRule<IFieldOrPropertyOrIndexer>( builder => { } ),
                ContractDirection.Both => _propertyOrIndexerEligibilityBoth,
                ContractDirection.Input => _propertyOrIndexerEligibilityInput,
                ContractDirection.Output => _propertyOrIndexerEligibilityOutput,
                _ => throw new ArgumentOutOfRangeException( nameof(direction) )
            };

        public virtual void BuildAspect( IAspectBuilder<IFieldOrPropertyOrIndexer> builder )
        {
            var eligibilityRule = builder.Target.DeclarationKind switch
            {
                DeclarationKind.Property or DeclarationKind.Indexer => GetPropertyEligibilityRule( this.Direction ),
                DeclarationKind.Field => null,
                _ => throw new ArgumentOutOfRangeException()
            };

            if ( eligibilityRule != null && !builder.VerifyEligibility( eligibilityRule ) )
            {
                // The aspect cannot be applied, but errors have been reported by the CheckEligibility method.
                return;
            }

            builder.Advice.AddContract( builder.Target, nameof(this.Validate), this.Direction );
        }

        public virtual void BuildAspect( IAspectBuilder<IParameter> builder )
        {
            var eligibilityRule =
                builder.Target.IsReturnParameter
                    ? GetReturnParameterEligibilityRule( this.Direction )
                    : GetParameterEligibilityRule( this.Direction );

            if ( eligibilityRule != null && !builder.VerifyEligibility( eligibilityRule ) )
            {
                // The aspect cannot be applied, but errors have been reported by the CheckEligibility method.

                return;
            }

            builder.Advice.AddContract( builder.Target, nameof(this.Validate), this.Direction );
        }

        public virtual void BuildEligibility( IEligibilityBuilder<IFieldOrPropertyOrIndexer> builder ) { }

        /// <summary>
        /// Populates the <see cref="IEligibilityBuilder"/> for a field, property or indexer when the <see cref="ContractDirection"/> is known.
        /// </summary>
        protected static void BuildEligibilityForDirection( IEligibilityBuilder<IFieldOrPropertyOrIndexer> builder, ContractDirection direction )
        {
            builder.If( p => p is IPropertyOrIndexer ).Convert().To<IPropertyOrIndexer>().AddRule( GetPropertyEligibilityRule( direction ) );
        }

        /// <summary>
        /// Populates the <see cref="IEligibilityBuilder"/> for a parameter when the <see cref="ContractDirection"/> is known.
        /// </summary>
        protected static void BuildEligibilityForDirection( IEligibilityBuilder<IParameter> builder, ContractDirection direction )
        {
            builder.If( p => p.IsReturnParameter ).AddRule( GetReturnParameterEligibilityRule( direction ) );
            builder.If( p => !p.IsReturnParameter ).AddRule( GetParameterEligibilityRule( direction ) );
        }

        public virtual void BuildEligibility( IEligibilityBuilder<IParameter> builder ) { }

        [Template]
        public abstract void Validate( dynamic? value );
    }
}