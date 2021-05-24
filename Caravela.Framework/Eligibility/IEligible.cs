// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;

namespace Caravela.Framework.Eligibility
{
    public interface IEligible<in T>
        where T : class, IDeclaration
    {
        // Constraints on implementation:
        //  * must be public
        //  * must call the base method as the first statement
        //  * cannot reference any instance member (except the call to the base method).

        // On .NET Core, this method could have an empty default implementation.

        void BuildEligibility( IEligibilityBuilder<T> builder );
    }
}