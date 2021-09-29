// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace Caravela.Framework.Impl.Fabrics
{
    /// <summary>
    /// The top-level <see cref="IAspectSource"/> that implements fabrics.
    /// </summary>
    internal class FabricAspectSource : IAspectSource
    {
        private readonly FabricContext _context;

        private readonly List<FabricDriver> _drivers = new(); // Note that this list is ordered.
        private readonly IEnumerable<IAspectClass> _aspectClasses;

        public FabricAspectSource( FabricContext context )
        {
            this._context = context;
            this._aspectClasses = new[] { context.AspectClasses[FabricTopLevelAspectClass.FabricAspectName] };
        }

        public void Register( FabricDriver driver )
        {
            this._drivers.Add( driver );
        }

        AspectSourcePriority IAspectSource.Priority => AspectSourcePriority.Programmatic;

        IEnumerable<IAspectClass> IAspectSource.AspectClasses => this._aspectClasses;

        IEnumerable<IDeclaration> IAspectSource.GetExclusions( INamedType aspectType ) => Array.Empty<IDeclaration>();

        IEnumerable<AspectInstance> IAspectSource.GetAspectInstances(
            CompilationModel compilation,
            IAspectClass aspectClass,
            IDiagnosticAdder diagnosticAdder,
            CancellationToken cancellationToken )
        {
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
                var drivers = driverGroup
                    .Select( x => x.Driver )
                    .Select(
                        x => new FabricTemplateClass(
                            x,
                            this._context.ServiceProvider,
                            compilation.RoslynCompilation,
                            diagnosticAdder,
                            null,
                            this._context.CompileTimeProject ) )
                    .ToImmutableArray();

                // Create an aggregate aspect class composed of all fabric classes.
                var aggregateClass = new FabricAggregateAspectClass( this._context.CompileTimeProject, drivers.As<TemplateClass>() );
                
                // Create a TemplateInstance for all fabrics.
                var templateInstances = drivers.Select( d => new TemplateClassInstance( d.Driver.Fabric, d, target ) ).ToImmutableArray();

                // Create an IAspect.
                IAspect aspect = target switch
                {
                    INamedType => new FabricAspect<INamedType>( drivers ),
                    INamespace => new FabricAspect<INamespace>( drivers ),
                    ICompilation => new FabricAspect<ICompilation>( drivers ),
                    _ => throw new AssertionFailedException()
                };

                // Creates the aggregate AspectInstance for the target declaration.
                var aggregateInstance = new AspectInstance( aspect, target, aggregateClass, templateInstances );

                yield return aggregateInstance;
            }
        }
    }
}