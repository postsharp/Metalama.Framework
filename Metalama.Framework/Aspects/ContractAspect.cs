// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
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
        private readonly ContractDirection _direction;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractAspect"/> class.
        /// </summary>
        /// <param name="direction">The direction of the data flow (<see cref="ContractDirection.Input"/>,  <see cref="ContractDirection.Output"/> or <see cref="ContractDirection.Both"/>)
        /// to which this contract applies. See the <see cref="ContractDirection"/> for details.</param>
        [Obsolete( "Specifying the direction in the constructor is obsolete. Override the GetDirection method." )]
        protected ContractAspect( ContractDirection direction )
        {
            this._direction = direction;
        }

        protected ContractAspect()
        {
            this._direction = ContractDirection.Default;
        }

        /// <summary>
        /// Gets or sets the direction of the data flow (<see cref="ContractDirection.Input"/>,  <see cref="ContractDirection.Output"/> or <see cref="ContractDirection.Both"/>)
        /// to which this contract applies.
        /// </summary>
        /// <remarks>
        /// In general, it is the responsibility of the <i>author</i> of the aspect, and not of its <i>user</i>, to define the eligible directions of a contract.
        /// However, the aspect's author can opt to allow users to define the contract direction by exposing this property as <c>public</c> in derived classes.
        /// </remarks>
        [PublicAPI]
        protected virtual ContractDirection GetDirection( IAspectBuilder builder ) => this._direction;

        private ContractDirection GetEffectiveDirection( IAspectBuilder aspectBuilder )
        {
            var direction = this.GetDirection( aspectBuilder );

            if ( direction == ContractDirection.Default )
            {
                // If the contract was inherited, we need to resolve the ContractDirection.Default value based on the base declaration.
                // Indeed, assuming the aspect is applied to a get-only property of an interface, the intent is to validate the getter's return value.
                // However, when a class implements this interface, it could implement the property with an automatic property, which would then
                // have an implicit setter, and this would change the interpretation of the default behavior.

                IDeclaration baseDeclaration;
                var predecessors = aspectBuilder.AspectInstance.Predecessors;

                if ( aspectBuilder.Target.DeclarationKind is DeclarationKind.Property &&
                     !predecessors.IsDefaultOrEmpty && predecessors[0].Kind == AspectPredecessorKind.Inherited )
                {
                    baseDeclaration = predecessors[0].Instance.TargetDeclaration.GetTarget( aspectBuilder.Target.Compilation );
                }
                else
                {
                    baseDeclaration = aspectBuilder.Target;
                }

                direction = ContractAspectHelper.GetEffectiveDirection( direction, baseDeclaration );
            }

            // Combine secondary instances if any.
            foreach ( var instance in aspectBuilder.AspectInstance.SecondaryInstances )
            {
                direction = direction.CombineWith( ((ContractAspect) instance.Aspect).GetDirection( aspectBuilder ) );
            }

            return direction;
        }

        public virtual void BuildAspect( IAspectBuilder<IFieldOrPropertyOrIndexer> builder )
        {
            var direction = this.GetEffectiveDirection( builder );
            var eligibilityRule = EligibilityRuleFactory.GetContractAdviceEligibilityRule( direction );

            if ( !builder.VerifyEligibility( eligibilityRule ) )
            {
                // The aspect cannot be applied, but errors have been reported by the CheckEligibility method.
                return;
            }

            builder.Advice.AddContract( builder.Target, nameof(this.Validate), direction );
        }

        public virtual void BuildAspect( IAspectBuilder<IParameter> builder )
        {
            var direction = this.GetEffectiveDirection( builder );
            var eligibilityRule = EligibilityRuleFactory.GetContractAdviceEligibilityRule( direction );

            if ( !builder.VerifyEligibility( eligibilityRule ) )
            {
                // The aspect cannot be applied, but errors have been reported by the CheckEligibility method.

                return;
            }

            // If the aspect is applied to a record positional parameter, add the contract to the corresponding property.
            var parameter = builder.Target;

            IProperty? property;

            if ( parameter.DeclaringMember is IConstructor { DeclaringType.TypeKind: TypeKind.RecordClass or TypeKind.RecordStruct } constructor &&
                 (property = constructor.DeclaringType.Properties.OfName( builder.Target.Name ).SingleOrDefault()) != null )
            {
                builder.Advice.AddContract( property, nameof(this.Validate), direction );
            }
            else
            {
                builder.Advice.AddContract( parameter, nameof(this.Validate), direction );
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