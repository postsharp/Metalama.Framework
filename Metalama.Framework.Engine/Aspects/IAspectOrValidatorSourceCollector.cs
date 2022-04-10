// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Validation;

namespace Metalama.Framework.Engine.Aspects;

internal interface IAspectOrValidatorSourceCollector
{
    void AddAspectSource( IAspectSource aspectSource );

    void AddValidatorSource( IValidatorSource validatorSource );
}