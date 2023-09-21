// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;
using System;

namespace Metalama.Framework.Engine.Aspects;

internal partial class EligibilityHelper
{
    private sealed class LocalFunctionEligibilityRule : IEligibilityRule<IDeclaration>
    {
        public static LocalFunctionEligibilityRule Instance { get; } = new();

        private LocalFunctionEligibilityRule() { }

        public EligibleScenarios GetEligibility( IDeclaration obj )
            => ((IMethod) obj).MethodKind == MethodKind.LocalFunction ? EligibleScenarios.None : EligibleScenarios.All;

        public FormattableString? GetIneligibilityJustification( EligibleScenarios requestedEligibility, IDescribedObject<IDeclaration> describedObject )
            => ((IMethod) describedObject.Object).MethodKind == MethodKind.LocalFunction
                ? $"{describedObject} is a local function"
                : (FormattableString?) null;
    }
}