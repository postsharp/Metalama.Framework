// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using System.Collections.Generic;
using System.Collections.Immutable;

#pragma warning disable 618 // Not implemented.

namespace Caravela.Framework.Eligibility.Implementation
{
    internal class EligibilityBuilder<T> : IEligibilityBuilder<T>
        where T : class
    {
        private readonly List<IEligibilityRule<T>> _predicates = new();
        private readonly BooleanCombinationOperator _combinationOperator;

        public EligibilityBuilder( BooleanCombinationOperator combinationOperator = BooleanCombinationOperator.And )
        {
            this._combinationOperator = combinationOperator;
        }

        public EligibleScenarios IneligibleScenarios => EligibleScenarios.None;

        public void AddRule( IEligibilityRule<T> rule ) => this._predicates.Add( rule );

        IEligibilityRule<IDeclaration> IEligibilityBuilder.Build() => new CastEligibilityRule<T, object>( this.Build() );

        public IEligibilityRule<T> Build()
        {
            var predicates = this._predicates.ToImmutableArray();

            return this._combinationOperator == BooleanCombinationOperator.Or
                ? new OrEligibilityRule<T>( predicates )
                : new AndEligibilityRule<T>( predicates );
        }
    }
}