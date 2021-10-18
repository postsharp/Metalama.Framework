// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using Caravela.Framework.Impl.CompileTime;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.Aspects
{
    /// <summary>
    /// Exposes the semantics of an aspect class used by the implementation of the aspect.
    /// </summary>
    internal interface IAspectClassImpl : IAspectClass
    {
        CompileTimeProject? Project { get; }

        ImmutableArray<TemplateClass> TemplateClasses { get; }

        EligibleScenarios GetEligibility( IDeclaration targetDeclaration );
    }
}