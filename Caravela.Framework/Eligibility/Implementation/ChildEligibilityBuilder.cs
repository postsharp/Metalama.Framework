// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using System;

#pragma warning disable 618 // Not implemented.

namespace Caravela.Framework.Eligibility.Implementation
{
    internal class ChildEligibilityBuilder<TParent, TChild> : IEligibilityBuilder<TChild>
    {
        private readonly IEligibilityBuilder<TParent> _parent;
        private readonly Func<TParent, TChild> _getChild;
        private readonly Func<IDescribedObject<TParent>, FormattableString> _getChildDescription;
        private readonly Predicate<TParent>? _canGetChild;
        private readonly Func<IDescribedObject<TParent>, FormattableString>? _cannotGetChildJustification;

        public ChildEligibilityBuilder(
            IEligibilityBuilder<TParent> parent,
            Func<TParent, TChild> getChild,
            Func<IDescribedObject<TParent>, FormattableString> getChildDescription,
            Predicate<TParent>? canGetChild = null,
            Func<IDescribedObject<TParent>, FormattableString>? cannotGetChildJustification = null )
        {
            this._parent = parent;
            this._canGetChild = canGetChild;
            this._cannotGetChildJustification = cannotGetChildJustification;
            this._getChild = getChild;
            this._getChildDescription = getChildDescription;

            if ( canGetChild != null && cannotGetChildJustification == null )
            {
                throw new ArgumentNullException( nameof(cannotGetChildJustification), "This argument must be specified when 'canGetChild' is specified." );
            }
        }

        public EligibleScenarios IneligibleScenarios => this._parent.IneligibleScenarios;

        public void AddRule( IEligibilityRule<TChild> rule )
        {
            this._parent.AddRule( new ChildRule( this, rule ) );
        }

        // This method is not supported because the predicates are added to the parent. This class is never used alone. 
        IEligibilityRule<IDeclaration> IEligibilityBuilder.Build() => throw new NotSupportedException();

        private class ChildRule : IEligibilityRule<TParent>
        {
            private readonly ChildEligibilityBuilder<TParent, TChild> _parent;
            private readonly IEligibilityRule<TChild> _childRule;

            public ChildRule( ChildEligibilityBuilder<TParent, TChild> parent, IEligibilityRule<TChild> childRule )
            {
                this._parent = parent;
                this._childRule = childRule;
            }

            public EligibleScenarios GetEligibility( TParent obj )
            {
                if ( this._parent._canGetChild != null && !this._parent._canGetChild( obj ) )
                {
                    return this._parent.IneligibleScenarios;
                }

                return this._childRule.GetEligibility( this._parent._getChild( obj ) );
            }

            public FormattableString? GetIneligibilityJustification(
                EligibleScenarios requestedEligibility,
                IDescribedObject<TParent> describedObject )
            {
                if ( this._parent._canGetChild != null && !this._parent._canGetChild( describedObject.Object ) )
                {
                    return this._parent._cannotGetChildJustification!( describedObject );
                }

                var child = this._parent._getChild( describedObject.Object );

                return this._childRule.GetIneligibilityJustification(
                    requestedEligibility,
                    new DescribedObject<TChild>(
                        child,
                        this._parent._getChildDescription( describedObject ) ) );
            }
        }
    }
}