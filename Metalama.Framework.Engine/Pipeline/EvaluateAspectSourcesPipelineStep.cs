// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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
        var aspectSourceResults = this._aspectSources.Select(
                s => s.GetAspectInstances( compilation, this.AspectLayer.AspectClass, this.Parent, cancellationToken ) )
            .ToList();

        var exclusions = new HashSet<IDeclaration>();

        foreach ( var exclusion in aspectSourceResults.SelectMany( x => x.Exclusions ) )
        {
            exclusions.Add( exclusion.GetTarget( compilation ) );
        }

        bool IsExcluded( IDeclaration declaration )
        {
            if ( exclusions.Count == 0 )
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

                    if ( x.Predecessor.Kind != AspectPredecessorKind.Attribute && IsExcluded( target ) )
                    {
                        return default;
                    }

                    return new ResolvedAspectInstance( x, target, x.ComputeEligibility( target ) );
                } )
            .Where( x => x.TargetDeclaration != null! )
            .ToList();

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