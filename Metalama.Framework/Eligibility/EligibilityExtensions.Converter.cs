// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Eligibility.Implementation;

namespace Metalama.Framework.Eligibility;

public static partial class EligibilityExtensions
{
    public readonly struct Converter<TInput>
    {
        private readonly IEligibilityBuilder<TInput> _eligibilityBuilder;

        internal Converter( IEligibilityBuilder<TInput> eligibilityBuilder )
        {
            this._eligibilityBuilder = eligibilityBuilder;
        }

        public IEligibilityBuilder<TOutput> To<TOutput>()
            where TOutput : TInput
        {
            return new ChildEligibilityBuilder<TInput, TOutput>(
                this._eligibilityBuilder,
                d => (TOutput) d!,
                d => d.Description!,
                d => d is TOutput,
                d => $"{d} is not  {typeof(TOutput).Name}" );
        }
    }
}