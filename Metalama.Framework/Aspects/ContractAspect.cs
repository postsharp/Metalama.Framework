// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;
using System;
using System.Linq;

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

        public virtual void BuildAspect( IAspectBuilder<IFieldOrPropertyOrIndexer> builder )
        {
            var eligibilityRule = EligibilityRuleFactory.GetContractAdviceEligibilityRule( this.Direction );

            if ( !builder.VerifyEligibility( eligibilityRule ) )
            {
                // The aspect cannot be applied, but errors have been reported by the CheckEligibility method.
                return;
            }

            builder.Advice.AddContract( builder.Target, nameof(this.Validate), this.Direction );
        }

        public virtual void BuildAspect( IAspectBuilder<IParameter> builder )
        {
            var eligibilityRule = EligibilityRuleFactory.GetContractAdviceEligibilityRule( this.Direction );

            if ( !builder.VerifyEligibility( eligibilityRule ) )
            {
                // The aspect cannot be applied, but errors have been reported by the CheckEligibility method.

                return;
            }

            // If the aspect is applied to a record positional parameter, add the contract to the corresponding property.
            var parameter = builder.Target;

            IProperty? property;

            if ( parameter.DeclaringMember is IConstructor constructor && constructor.DeclaringType.TypeKind is TypeKind.RecordClass or TypeKind.RecordStruct &&
                 (property = constructor.DeclaringType.Properties.OfName( builder.Target.Name ).SingleOrDefault()) != null )
            {
                builder.Advice.AddContract( property, nameof(this.Validate), this.Direction );
            }
            else
            {
                builder.Advice.AddContract( parameter, nameof(this.Validate), this.Direction );
            }
        }

        public virtual void BuildEligibility( IEligibilityBuilder<IFieldOrPropertyOrIndexer> builder ) { }

        /// <summary>
        /// Populates the <see cref="IEligibilityBuilder"/> for a field, property or indexer when the <see cref="ContractDirection"/> is known.
        /// </summary>
        protected static void BuildEligibilityForDirection( IEligibilityBuilder<IFieldOrPropertyOrIndexer> builder, ContractDirection direction )
        {
            builder.AddRule( EligibilityRuleFactory.GetContractAdviceEligibilityRule( direction ) );
        }

        /// <summary>
        /// Populates the <see cref="IEligibilityBuilder"/> for a parameter when the <see cref="ContractDirection"/> is known.
        /// </summary>
        protected static void BuildEligibilityForDirection( IEligibilityBuilder<IParameter> builder, ContractDirection direction )
        {
            builder.AddRule( EligibilityRuleFactory.GetContractAdviceEligibilityRule( direction ) );
        }

        public virtual void BuildEligibility( IEligibilityBuilder<IParameter> builder ) { }

        [Template]
        public abstract void Validate( dynamic? value );
    }
}