// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Engine.AspectOrdering;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.AspectWeavers;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Fabrics;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Fabrics
{
    /// <summary>
    /// The top-level aspect class integrating the fabrics feature in the aspect pipeline. It is used as an 'identity'
    /// class. The real class is <see cref="FabricAggregateAspectClass"/>, which is instantiated in the middle of the pipeline,
    /// while <see cref="FabricTopLevelAspectClass"/> must exist while the pipeline is being instantiated.
    /// </summary>
    internal sealed class FabricTopLevelAspectClass : IBoundAspectClass
    {
        public const string FabricAspectName = "<Fabric>";

        public AspectLayer Layer { get; }

        string IAspectClass.FullName => FabricAspectName;

        public string ShortName => FabricAspectName;

        string IAspectClass.DisplayName => FabricAspectName;

        string? IAspectClass.Description => null;

        bool IAspectClass.IsAbstract => false;

        public bool? IsInheritable => false;

        public bool IsAttribute => false;

        public Type Type => typeof(Fabric);

        public EditorExperienceOptions EditorExperienceOptions => EditorExperienceOptions.Default;

        public FabricTopLevelAspectClass( ProjectServiceProvider serviceProvider, CompilationModel compilation, CompileTimeProject project )
        {
            this.Layer = new AspectLayer( this, null );
            this.AspectDriver = new AspectDriver( serviceProvider, this, compilation );
            this.Project = project;
        }

        public IAspectDriver AspectDriver { get; }

        public Location? GetDiagnosticLocation( Compilation compilation ) => null;

        public CompileTimeProject? Project { get; }

        ImmutableArray<TemplateClass> IAspectClassImpl.TemplateClasses => ImmutableArray<TemplateClass>.Empty;

        SyntaxAnnotation IAspectClassImpl.GeneratedCodeAnnotation => throw new NotSupportedException();

        public ImmutableArray<AspectLayer> Layers { get; } = ImmutableArray.Create( new AspectLayer( "<Fabric>", null ) );

        EligibleScenarios IAspectClassImpl.GetEligibility( IDeclaration obj, bool isInheritable ) => EligibleScenarios.Aspect;

        ITemplateReflectionContext IAspectClassImpl.GetTemplateReflectionContext( CompilationContext compilationContext ) => throw new NotSupportedException();

        EligibleScenarios IEligibilityRule<IDeclaration>.GetEligibility( IDeclaration obj ) => EligibleScenarios.Aspect;

        FormattableString IEligibilityRule<IDeclaration>.GetIneligibilityJustification(
            EligibleScenarios requestedEligibility,
            IDescribedObject<IDeclaration> describedObject )
            => throw new AssertionFailedException( "This aspect is always eligible." );

        public string DiagnosticSourceDescription => "top-level fabric";
    }
}