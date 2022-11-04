// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Eligibility.Implementation;

namespace Metalama.Framework.Eligibility;

public static partial class EligibilityExtensions
{
    /// <summary>
    /// A helper type that allows to convert an <see cref="IEligibilityBuilder{T}"/> for a type to an <see cref="IEligibilityBuilder{T}"/> of another type.  
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    public readonly struct Converter<TInput>
        where TInput : class
    {
        private readonly IEligibilityBuilder<TInput> _eligibilityBuilder;

        internal Converter( IEligibilityBuilder<TInput> eligibilityBuilder )
        {
            this._eligibilityBuilder = eligibilityBuilder;
        }

        /// <summary>
        /// Gets an <see cref="IEligibilityBuilder{T}"/> for another type.
        /// </summary>
        public IEligibilityBuilder<TOutput> To<TOutput>()
            where TOutput : class, TInput
            => new ChildEligibilityBuilder<TInput, TOutput>(
                this._eligibilityBuilder,
                d => (TOutput) d,
                d => d.Description!,
                d => d is TOutput,
                d => $"{d} must be a {GetInterfaceName<TOutput>()}" );

        public IEligibilityBuilder<TOutput> When<TOutput>()
            where TOutput : class, TInput
            => this._eligibilityBuilder.If( d => d is TOutput ).Convert().To<TOutput>();
    }
}