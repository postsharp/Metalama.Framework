// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CompileTime;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.Fabrics
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

        public string DisplayName => FabricTopLevelAspectClass.FabricAspectName;

        public string? Description => null;

        public bool IsAbstract => false;

        public bool IsInherited => false;

        public CompileTimeProject Project { get; }

        public ImmutableArray<TemplateClass> TemplateClasses { get; }

        public EligibleScenarios GetEligibility( IDeclaration targetDeclaration ) => EligibleScenarios.Aspect;
    }
}