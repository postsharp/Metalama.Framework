// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.Utilities.Threading;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Fabrics;

/// <summary>
/// The top-level <see cref="IAspectSource"/> that implements fabrics.
/// </summary>
internal sealed class FabricAspectSource : IAspectSource
{
    private readonly FabricManager _fabricManager;

    private readonly ImmutableArray<TypeFabricDriver> _drivers; // Note that this list is ordered.
    private readonly ImmutableArray<IAspectClass> _aspectClasses;
    private readonly IConcurrentTaskRunner _concurrentTaskRunner;

    public FabricAspectSource( FabricManager fabricManager, ImmutableArray<TypeFabricDriver> drivers )
    {
        this._concurrentTaskRunner = fabricManager.ServiceProvider.GetRequiredService<IConcurrentTaskRunner>();
        this._fabricManager = fabricManager;
        this._drivers = drivers;
        this._aspectClasses = ImmutableArray.Create<IAspectClass>( fabricManager.AspectClasses[FabricTopLevelAspectClass.FabricAspectName] );
    }

    ImmutableArray<IAspectClass> IAspectSource.AspectClasses => this._aspectClasses;

    Task IAspectSource.AddAspectInstancesAsync(
        IAspectClass aspectClass,
        OutboundActionCollectionContext context )
    {
        var compilation = context.Compilation;

        // Group drivers by their target declaration.
        var driversByTarget =
            this._drivers.Select( x => (Driver: x, Target: x.GetTargetIfInPartialCompilation( compilation )) )
                .Where( x => x.Target != null )
                .GroupBy( x => x.Target );

        return this._concurrentTaskRunner.RunInParallelAsync( driversByTarget, ProcessDriver, context.CancellationToken );

        // Process target declarations.
        void ProcessDriver( IGrouping<IDeclaration?, (TypeFabricDriver Driver, IDeclaration? Target)> driverGroup )
        {
            var target = driverGroup.Key!;

            // Create template classes for all fabrics.
            var compileTimeProject = this._fabricManager.CompileTimeProject;

            var drivers = driverGroup
                .Select(
                    x => new FabricTemplateClass(
                        this._fabricManager.ServiceProvider,
                        x.Driver,
                        x.Driver.CompileTimeProject.TemplateReflectionContext ?? compilation.CompilationContext,
                        context.Collector,
                        null ) )
                .ToImmutableArray();

            // Create an aggregate aspect class composed of all fabric classes.
            var aggregateClass = new FabricAggregateAspectClass( compileTimeProject, drivers.As<TemplateClass>() );

            // Create a TemplateInstance for each fabric.
            var templateInstances = drivers.Select( d => new TemplateClassInstance( TemplateProvider.FromInstance( d.Driver.Fabric ), d ) )
                .ToImmutableArray();

            // Create an IAspect.
            IAspect aspect = target switch
            {
                INamedType => new FabricAspect<INamedType>( drivers ),
                INamespace => new FabricAspect<INamespace>( drivers ),
                ICompilation => new FabricAspect<ICompilation>( drivers ),
                _ => throw new AssertionFailedException( $"Unexpected fabric target: '{target}'." )
            };

            // Creates the aggregate AspectInstance for the target declaration.
            var aggregateInstance = new AspectInstance( aspect, target, aggregateClass, templateInstances, default );

            context.Collector.AddAspectInstance( aggregateInstance );
        }
    }
}