// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Project;
using System;

namespace Metalama.Framework.Eligibility;

internal interface IEligibilityService : IService
{
    bool IsEligible( Type aspectType, IDeclaration declaration, EligibleScenarios scenarios );
}