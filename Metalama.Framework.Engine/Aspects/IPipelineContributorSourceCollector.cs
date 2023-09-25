// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.HierarchicalOptions;
using Metalama.Framework.Engine.Validation;

namespace Metalama.Framework.Engine.Aspects;

internal interface IPipelineContributorSourceCollector
{
    void AddAspectSource( IAspectSource aspectSource );

    void AddValidatorSource( IValidatorSource validatorSource );

    void AddOptionsSource( IHierarchicalOptionsSource hierarchicalOptionsSource );
}