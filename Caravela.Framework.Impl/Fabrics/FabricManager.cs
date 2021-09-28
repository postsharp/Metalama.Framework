// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Fabrics;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.CompileTime;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.Fabrics
{
    internal sealed class FabricManager
    {
        private readonly FabricAspectSource _aspectSource;
        private readonly FabricContext _context;

        public FabricManager( FabricContext context )
        {
            this._context = context;
            this._aspectSource = new FabricAspectSource( context );
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

            this.RegisterFabrics( transitiveFabricTypes );

            // Discover the fabrics inside the current project.
            var fabrics =
                compileTimeProject.FabricTypes
                    .OrderBy( t => t )
                    .Select( compileTimeProject.GetType )
                    .Select( x => this.CreateDriver( x, runTimeCompilation ) )
                    .OrderBy( x => x.Kind )
                    .ThenBy( x => x.OrderingKey );

            this.RegisterFabrics( fabrics );
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
            var fabric = (IFabric) this._context.UserCodeInvoker.Invoke( () => Activator.CreateInstance( fabricType ) );

            switch ( fabric )
            {
                case ITypeFabric typeFabric:
                    return new TypeFabricDriver( this._context, typeFabric, runTimeCompilation );

                case ITransitiveProjectFabric transitiveCompilationFabric:
                    return new ProjectFabricDriver( this._context, transitiveCompilationFabric, runTimeCompilation );

                case IProjectFabric compilationFabric:
                    return new ProjectFabricDriver( this._context, compilationFabric, runTimeCompilation );

                case INamespaceFabric namespaceFabric:
                    return new NamespaceFabricDriver( this._context, namespaceFabric, runTimeCompilation );

                default:
                    throw new AssertionFailedException();
            }
        }
    }
}