// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Engine.AspectOrdering;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Fabrics;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Linq;

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

            this.GeneratedCodeAnnotation =
                MetalamaCompilerAnnotations.CreateGeneratedCodeAnnotation( "fabric " + string.Join( " or ", templateClasses.Select( x => x.FullName ) ) );
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

        public SyntaxAnnotation GeneratedCodeAnnotation { get; }

        public ImmutableArray<AspectLayer> Layers { get; } = ImmutableArray.Create( new AspectLayer( "<Fabric>", null ) );

        public EligibleScenarios GetEligibility( IDeclaration obj ) => EligibleScenarios.Aspect;

        public FormattableString? GetIneligibilityJustification( EligibleScenarios requestedEligibility, IDescribedObject<IDeclaration> describedObject )
            => throw new AssertionFailedException();
    }
}