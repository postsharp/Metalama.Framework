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
    /// <para>A filter aspect can apply to the input or output data flow, or to both data flows, according to the <see cref="FilterDirection"/> value
    /// passed to the constructor. Since the current class does not know the value of this parameter before it is instantiated, this class cannot
    /// set the eligibility conditions using the <see cref="BuildEligibility(Metalama.Framework.Eligibility.IEligibilityBuilder{Metalama.Framework.Code.IFieldOrPropertyOrIndexer})"/> method.
    /// If a derived class targets a specific <see cref="FilterDirection"/> (i.e. if the choice is not left to the user),
    /// its implementation of <see cref="BuildEligibility(Metalama.Framework.Eligibility.IEligibilityBuilder{Metalama.Framework.Code.IFieldOrPropertyOrIndexer})"/>
    /// can call <see cref="BuildEligibilityForDirection(Metalama.Framework.Eligibility.IEligibilityBuilder{Metalama.Framework.Code.IFieldOrPropertyOrIndexer},Metalama.Framework.Aspects.FilterDirection)"/>
    /// methods. This means that eligibility can be checked upfront by the IDE before suggesting the code actions.
    /// </para>
    /// <para>
    /// In any case, this aspect verifies the eligibility of the target with respect to the specific <see cref="FilterDirection"/> and target declaration. This verification
    /// cannot be skipped.
    /// </para>
    /// </remarks>
    [AttributeUsage( AttributeTargets.ReturnValue | AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property )]
    public abstract class FilterAspect : Aspect, IAspect<IParameter>, IAspect<IFieldOrPropertyOrIndexer>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterAspect"/> class.
        /// </summary>
        /// <param name="direction">The direction of the data flow (<see cref="FilterDirection.Input"/>,  <see cref="FilterDirection.Output"/> or <see cref="FilterDirection.Both"/>)
        /// to which this filter applies. See the <see cref="FilterDirection"/> for details.</param>
        protected FilterAspect( FilterDirection direction = FilterDirection.Default )
        {
            this.Direction = direction;
        }

        /// <summary>
        /// Gets the direction of the data flow (<see cref="FilterDirection.Input"/>,  <see cref="FilterDirection.Output"/> or <see cref="FilterDirection.Both"/>)
        /// to which this filter applies.
        /// </summary>
        /// <remarks>
        /// It is the responsibility of the <i>author</i> of the aspect, and not of its <i>user</i>, to define the eligible directions of a filter.
        /// </remarks>
        protected FilterDirection Direction { get; }

        private static IEligibilityRule<IParameter>? GetParameterEligibilityRule( FilterDirection direction )
            => direction switch
            {
                FilterDirection.Default => null,
                FilterDirection.Both => _parameterEligibilityBoth,
                FilterDirection.Input => _parameterEligibilityInput,
                FilterDirection.Output => _parameterEligibilityOutput,
                _ => throw new ArgumentOutOfRangeException( nameof(direction) )
            };

        private static IEligibilityRule<IFieldOrPropertyOrIndexer>? GetPropertyEligibilityRule( FilterDirection direction )
            => direction switch
            {
                FilterDirection.Default => null,
                FilterDirection.Both => _propertyOrIndexerEligibilityBoth,
                FilterDirection.Input => _propertyOrIndexerEligibilityInput,
                FilterDirection.Output => _propertyOrIndexerEligibilityOutput,
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

            builder.Advice.AddFilter( builder.Target, nameof(this.Filter), this.Direction );
        }

        public virtual void BuildAspect( IAspectBuilder<IParameter> builder )
        {
            var eligibilityRule = GetParameterEligibilityRule( this.Direction );

            if ( eligibilityRule != null && !builder.VerifyEligibility( eligibilityRule ) )
            {
                // The aspect cannot be applied, but errors have been reported by the CheckEligibility method.

                return;
            }

            builder.Advice.AddFilter( builder.Target, nameof(this.Filter), this.Direction );
        }

        public virtual void BuildEligibility( IEligibilityBuilder<IFieldOrPropertyOrIndexer> builder ) { }

        /// <summary>
        /// Populates the <see cref="IEligibilityBuilder"/> for a field, property or indexer when the <see cref="FilterDirection"/> is known.
        /// </summary>
        protected static void BuildEligibilityForDirection( IEligibilityBuilder<IFieldOrPropertyOrIndexer> builder, FilterDirection direction )
        {
            if ( direction != FilterDirection.Default )
            {
                builder.MustSatisfyAny(
                    b => b.MustBe<IField>(),
                    b => b.Convert().To<IPropertyOrIndexer>().AddRule( GetPropertyEligibilityRule( direction )! ) );
            }
        }

        /// <summary>
        /// Populates the <see cref="IEligibilityBuilder"/> for a parameter when the <see cref="FilterDirection"/> is known.
        /// </summary>
        protected static void BuildEligibilityForDirection( IEligibilityBuilder<IParameter> builder, FilterDirection direction )
        {
            if ( direction != FilterDirection.Default )
            {
                builder.AddRule( GetParameterEligibilityRule( direction )! );
            }
        }

        public virtual void BuildEligibility( IEligibilityBuilder<IParameter> builder ) { }

        [Template]
        public abstract void Filter( dynamic? value );
    }
}