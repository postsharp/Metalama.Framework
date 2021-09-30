// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Fabrics;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Pipeline;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.Fabrics
{
    internal sealed class FabricManager
    {
        private readonly FabricAspectSource _aspectSource;
        private readonly AspectProjectConfiguration _configuration;

        public FabricManager( AspectProjectConfiguration configuration )
        {
            this._configuration = configuration;
            this._aspectSource = new FabricAspectSource( configuration );
        }

        public IAspectSource AspectSource => this._aspectSource;

        public void ExecuteFabrics( CompileTimeProject compileTimeProject, Compilation runTimeCompilation )
        {
            // Discover the transitive fabrics from project dependencies, and execute them.
            var transitiveFabricTypes = new Tuple<CompileTimeProject, int>( compileTimeProject, 0 )
                .SelectManyRecursive( p => p.Item1.References.Select( r => new Tuple<CompileTimeProject, int>( r, p.Item2 + 1 ) ), false, false )
                .GroupBy( t => t.Item1 )
                .Select( g => (Project: g.Key, Depth: g.Max()) )
                .SelectMany( x => x.Project.TransitiveFabricTypes.Select( t => (x.Project, x.Depth, Type: t) ) )
                .OrderByDescending( x => x.Depth )
                .ThenBy( x => x.Type )
                .Select( x => x.Project.GetType( x.Type ) )
                .Select( x => this.CreateDriver( x, runTimeCompilation ) );

            // Discover the fabrics inside the current project.
            var fabrics =
                compileTimeProject.FabricTypes
                    .OrderBy( t => t )
                    .Select( compileTimeProject.GetType )
                    .Select( x => this.CreateDriver( x, runTimeCompilation ) )
                    .OrderBy( x => x )
                    .ToList();

            // Register the fabrics in the correct order.
            this.RegisterFabrics( fabrics.Where( f => f.Kind == FabricKind.Compilation ) );
            this.RegisterFabrics( transitiveFabricTypes );
            this.RegisterFabrics( fabrics.Where( f => f.Kind != FabricKind.Compilation ) );
        }

        private void RegisterFabrics( IEnumerable<FabricDriver> drivers )
        {
            foreach ( var driver in drivers )
            {
                this._aspectSource.Register( driver );
            }
        }

        private FabricDriver CreateDriver( Type fabricType, Compilation runTimeCompilation )
        {
            var fabric = (IFabric) this._configuration.UserCodeInvoker.Invoke( () => Activator.CreateInstance( fabricType ) );

            switch ( fabric )
            {
                case ITypeFabric typeFabric:
                    return new TypeFabricDriver( this._configuration, typeFabric, runTimeCompilation );

                case ITransitiveProjectFabric transitiveCompilationFabric:
                    return new ProjectFabricDriver( this._configuration, transitiveCompilationFabric, runTimeCompilation );

                case IProjectFabric compilationFabric:
                    return new ProjectFabricDriver( this._configuration, compilationFabric, runTimeCompilation );

                case INamespaceFabric namespaceFabric:
                    return new NamespaceFabricDriver( this._configuration, namespaceFabric, runTimeCompilation );

                default:
                    throw new AssertionFailedException();
            }
        }
    }
}