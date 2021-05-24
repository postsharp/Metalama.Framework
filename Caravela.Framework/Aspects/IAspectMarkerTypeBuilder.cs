// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Eligibility;
using Caravela.Framework.Validation;

namespace Caravela.Framework.Aspects
{
    [InternalImplement]
    public interface IAspectMarkerTypeBuilder<out T>
    {
        string DisplayName { get; set; }

        string? Description { get; set; }

        IEligibilityBuilder<T> Eligibility { get; }
    }
}