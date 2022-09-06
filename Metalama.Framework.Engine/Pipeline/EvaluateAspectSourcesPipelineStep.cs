// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Engine.AspectOrdering;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace Metalama.Framework.Engine.Pipeline;

/// <summary>
/// The <see cref="PipelineStage"/> that evaluates aspect sources and adds aspect instances to other steps. This step runs
/// in a fake depth numbered -1 because it needs to run before any other step within the aspect type.
/// </summary>
internal class EvaluateAspectSourcesPipelineStep : PipelineStep
{
    private readonly List<IAspectSource> _aspectSources = new();

    public EvaluateAspectSourcesPipelineStep( PipelineStepsState parent, OrderedAspectLayer aspectLayer ) : base(
        parent,
        new PipelineStepId( aspectLayer.AspectLayerId, -1, -1, PipelineStepPhase.Initialize, -1 ),
        aspectLayer ) { }

    public override CompilationModel Execute(
        CompilationModel compilation,
        CancellationToken cancellationToken )
    {
        var aspectClass = this.AspectLayer.AspectClass;

        var aspectSourceResults = this._aspectSources.Select( s => s.GetAspectInstances( compilation, aspectClass, this.Parent, cancellationToken ) )
            .ToList();

        HashSet<IDeclaration>? exclusions = null;

        foreach ( var exclusion in aspectSourceResults.SelectMany( x => x.Exclusions ) )
        {
            exclusions ??= new HashSet<IDeclaration>();

            exclusions.Add( exclusion.GetTarget( compilation ) );
        }

        bool IsExcluded( IDeclaration declaration )
        {
            if ( exclusions == null || exclusions.Count == 0 )
            {
                return false;
            }

            if ( exclusions.Contains( declaration ) )
            {
                return true;
            }

            if ( declaration.ContainingDeclaration != null )
            {
                return IsExcluded( declaration.ContainingDeclaration );
            }

            return false;
        }

        var aspectInstances = aspectSourceResults
            .SelectMany( x => x.AspectInstances )
            .Select(
                x =>
                {
                    var target = (IDeclarationImpl) x.TargetDeclaration.GetTarget( compilation );

                    if ( !x.Predecessors.IsDefaultOrEmpty && x.Predecessors[0].Kind != AspectPredecessorKind.Attribute && IsExcluded( target ) )
                    {
                        return default;
                    }

                    return new ResolvedAspectInstance( x, target, x.ComputeEligibility( target ) );
                } )
            .Where( x => x.TargetDeclaration != null! )
            .ToList();

        // Process aspect requirements.
        // We always add an aspect instance if there is a requirement, even if there is already an instance, because this has the side effect of exposing
        // the predecessors to the IAspectInstance in the last instance of the aggregated aspect.
        var requirements = aspectSourceResults.SelectMany( x => x.Requirements )
            .Select( x => (TargetDeclaration: x.TargetDeclaration.GetTarget( compilation ), x.Predecessor) )
            .GroupBy( x => x.TargetDeclaration );

        foreach ( var requirement in requirements )
        {
            var requirementTarget = requirement.Key;
            var predecessors = requirement.Select( x => new AspectPredecessor( AspectPredecessorKind.RequiredAspect, x.Predecessor ) );

            var stronglyTypedAspectClass = (AspectClass) aspectClass;
            var aspect = stronglyTypedAspectClass.CreateDefaultInstance();

            var aspectInstance = new AspectInstance( aspect, requirementTarget.ToTypedRef(), stronglyTypedAspectClass, predecessors.ToImmutableArray() );
            var eligibility = aspectInstance.ComputeEligibility( requirementTarget );

            if ( (eligibility & (EligibleScenarios.Aspect | EligibleScenarios.Inheritance)) != 0 )
            {
                aspectInstances.Add( new ResolvedAspectInstance( aspectInstance, (IDeclarationImpl) requirementTarget, eligibility ) );
            }
            else
            {
                // The situation should have been detected before.
                throw new AssertionFailedException( $"Cannot add the aspect '{aspectClass.ShortName}' to '{requirementTarget}' because of eligibility. " );
            }
        }

        // We assume that all aspect instances are eligible, but some are eligible only for inheritance.

        // Get the aspects that can be processed, i.e. they are not abstract-only.
        var concreteAspectInstances = aspectInstances
            .Where( a => a.Eligibility.IncludesAll( EligibleScenarios.Aspect ) )
            .ToList();

        // Gets aspects that can be inherited.
        var inheritableAspectInstances = aspectInstances
            .Where( a => a.Eligibility.IncludesAll( EligibleScenarios.Inheritance ) && a.AspectInstance.AspectClass.IsInherited )
            .ToList();

        // Gets aspects that have been inherited by the source. 
        var inheritedAspectInstancesInProject = inheritableAspectInstances
            .SelectMany(
                a => a.TargetDeclaration.GetDerivedDeclarations()
                    .Where( d => !IsExcluded( d ) )
                    .Select(
                        declaration =>
                            new ResolvedAspectInstance(
                                a.AspectInstance.CreateDerivedInstance( declaration ),
                                (IDeclarationImpl) declaration,
                                EligibleScenarios.Aspect ) ) )
            .ToList();

        // Index these aspects. 
        this.Parent.AddAspectInstances( concreteAspectInstances );
        this.Parent.AddAspectInstances( inheritedAspectInstancesInProject );
        this.Parent.AddInheritableAspectInstances( inheritableAspectInstances.Select( x => x.AspectInstance ).ToList() );

        return compilation.WithAspectInstances( concreteAspectInstances.Select( x => x.AspectInstance ).ToImmutableArray() );
    }

    public void AddAspectSource( IAspectSource aspectSource ) => this._aspectSources.Add( aspectSource );
}