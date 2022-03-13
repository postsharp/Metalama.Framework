// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace Metalama.Framework.Engine.Fabrics;

/// <summary>
/// The top-level <see cref="IAspectSource"/> that implements fabrics.
/// </summary>
internal class FabricAspectSource : IAspectSource
{
    private readonly FabricManager _fabricManager;

    private readonly ImmutableArray<TypeFabricDriver> _drivers; // Note that this list is ordered.
    private readonly ImmutableArray<IAspectClass> _aspectClasses;

    public FabricAspectSource( FabricManager fabricManager, ImmutableArray<TypeFabricDriver> drivers )
    {
        this._fabricManager = fabricManager;
        this._drivers = drivers;
        this._aspectClasses = ImmutableArray.Create<IAspectClass>( fabricManager.AspectClasses[FabricTopLevelAspectClass.FabricAspectName] );
    }

    ImmutableArray<IAspectClass> IAspectSource.AspectClasses => this._aspectClasses;

    AspectSourceResult IAspectSource.GetAspectInstances(
        CompilationModel compilation,
        IAspectClass aspectClass,
        IDiagnosticAdder diagnosticAdder,
        CancellationToken cancellationToken )
    {
        var aspectInstances = new List<AspectInstance>();

        // Group drivers by their target declaration.
        var driversByTarget =
            this._drivers
                .Select( x => (Driver: x, Target: x.GetTarget( compilation )) )
                .GroupBy( x => x.Target );

        // Process target declarations.
        foreach ( var driverGroup in driversByTarget )
        {
            var target = driverGroup.Key;

            // Create template classes for all fabrics.
            var compileTimeProject = this._fabricManager.CompileTimeProject;

            var drivers = driverGroup
                .Select( x => x.Driver )
                .Select(
                    x => new FabricTemplateClass(
                        x,
                        this._fabricManager.ServiceProvider,
                        compilation.RoslynCompilation,
                        diagnosticAdder,
                        null,
                        compileTimeProject ) )
                .ToImmutableArray();

            // Create an aggregate aspect class composed of all fabric classes.
            var aggregateClass = new FabricAggregateAspectClass( compileTimeProject, drivers.As<TemplateClass>() );

            // Create a TemplateInstance for all fabrics.
            var templateInstances = drivers.Select( d => new TemplateClassInstance( d.Driver.Fabric, d ) ).ToImmutableArray();

            // Create an IAspect.
            IAspect aspect = target switch
            {
                INamedType => new FabricAspect<INamedType>( drivers ),
                INamespace => new FabricAspect<INamespace>( drivers ),
                ICompilation => new FabricAspect<ICompilation>( drivers ),
                _ => throw new AssertionFailedException()
            };

            // Creates the aggregate AspectInstance for the target declaration.
            var aggregateInstance = new AspectInstance( aspect, target.ToTypedRef(), aggregateClass, templateInstances, default );

            aspectInstances.Add( aggregateInstance );
        }

        return new AspectSourceResult( aspectInstances );
    }
}