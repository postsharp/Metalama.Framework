// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Engine.AspectOrdering;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CompileTime;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Tests.LinkerTests.Runner;

internal class TestAspectClass : IAspectClassImpl
{
    public CompileTimeProject? Project => throw new NotImplementedException();

    public ImmutableArray<TemplateClass> TemplateClasses => throw new NotImplementedException();

    public SyntaxAnnotation GeneratedCodeAnnotation => throw new NotImplementedException();

    public ImmutableArray<AspectLayer> Layers => throw new NotImplementedException();

    public IReadOnlyCollection<IAspectClassImpl> DescendantClassesAndSelf => throw new NotImplementedException();

    public string FullName => throw new NotImplementedException();

    public string ShortName => throw new NotImplementedException();

    public string DisplayName => throw new NotImplementedException();

    public string? Description => throw new NotImplementedException();

    public bool IsAbstract => throw new NotImplementedException();

    public bool? IsInheritable => throw new NotImplementedException();

    public bool IsAttribute => throw new NotImplementedException();

    public Type Type => throw new NotImplementedException();

    public EditorExperienceOptions EditorExperienceOptions => throw new NotImplementedException();

    public string DiagnosticSourceDescription => throw new NotImplementedException();

    public EligibleScenarios GetEligibility( IDeclaration obj, bool isInheritable )
    {
        throw new NotImplementedException();
    }

    public EligibleScenarios GetEligibility( IDeclaration obj )
    {
        throw new NotImplementedException();
    }

    public FormattableString? GetIneligibilityJustification( EligibleScenarios requestedEligibility, IDescribedObject<IDeclaration> describedObject )
    {
        throw new NotImplementedException();
    }
}
