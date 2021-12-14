// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Fabrics;
using System;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Fabrics
{
    /// <summary>
    /// An aspect class that aggregates all fabrics on a given declaration.
    /// </summary>
    internal class FabricAggregateAspectClass : IAspectClassImpl
    {
        public FabricAggregateAspectClass( CompileTimeProject project, ImmutableArray<TemplateClass> templateClasses )
        {
            this.Project = project;
            this.TemplateClasses = templateClasses;
        }

        public string FullName => FabricTopLevelAspectClass.FabricAspectName;

        public string ShortName => FabricTopLevelAspectClass.FabricAspectName;

        public string DisplayName => FabricTopLevelAspectClass.FabricAspectName;

        public string? Description => null;

        public bool IsAbstract => false;

        public bool IsInherited => false;

        public bool IsAttribute => false;

        public Type Type => typeof(Fabric);

        public CompileTimeProject Project { get; }

        public ImmutableArray<TemplateClass> TemplateClasses { get; }

        public EligibleScenarios GetEligibility( IDeclaration obj ) => EligibleScenarios.Aspect;

        public FormattableString? GetIneligibilityJustification( EligibleScenarios requestedEligibility, IDescribedObject<IDeclaration> describedObject )
            => throw new AssertionFailedException();
    }
}