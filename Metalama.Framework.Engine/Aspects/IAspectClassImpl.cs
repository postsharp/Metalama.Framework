// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Engine.AspectOrdering;
using Metalama.Framework.Engine.CompileTime;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Aspects
{
    /// <summary>
    /// Exposes the semantics of an aspect class used by the implementation of the aspect.
    /// </summary>
    internal interface IAspectClassImpl : IAspectClass, IEligibilityRule<IDeclaration>, IDiagnosticSource
    {
        CompileTimeProject? Project { get; }

        ImmutableArray<TemplateClass> TemplateClasses { get; }

        /// <summary>
        /// Gets the <see cref="SyntaxAnnotation"/> that must be added to code generated by the current aspect class.
        /// </summary>
        SyntaxAnnotation GeneratedCodeAnnotation { get; }

        ImmutableArray<AspectLayer> Layers { get; }

        /// <summary>
        /// Gets the eligibility of a an aspect instance of the current aspect class without when knowing whether the aspect instance is inheritable.
        /// </summary>
        EligibleScenarios GetEligibility( IDeclaration obj, bool isInheritable );

        /// <summary>
        /// Gets the closure of all descendant classes.
        /// </summary>
        IReadOnlyCollection<IAspectClassImpl> DescendantClassesAndSelf { get; }
    }
}