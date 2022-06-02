// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Engine.CompileTime;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Aspects
{
    /// <summary>
    /// Exposes the semantics of an aspect class used by the implementation of the aspect.
    /// </summary>
    internal interface IAspectClassImpl : IAspectClass, IEligibilityRule<IDeclaration>
    {
        CompileTimeProject? Project { get; }

        ImmutableArray<TemplateClass> TemplateClasses { get; }

        SyntaxAnnotation GeneratedCodeAnnotation { get; }
    }
}