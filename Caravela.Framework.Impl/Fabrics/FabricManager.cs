// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Fabrics;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.ServiceProvider;
using Caravela.Framework.Impl.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.Fabrics
{
    internal sealed class FabricManager
    {
        private readonly UserCodeInvoker _userCodeInvoker;
        private readonly IServiceProvider _serviceProvider;

        public FabricManager( IServiceProvider serviceProvider )
        {
            this._serviceProvider = serviceProvider;
            this._userCodeInvoker = serviceProvider.GetService<UserCodeInvoker>();
        }

        public FabricResult ExecuteFabrics( CompileTimeProject compileTimeProject, IProject project, AspectClassRegistry aspectClasses )
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
                .Select( fabricType => this.CreateDriver( fabricType, aspectClasses ) );

            FabricResult result = new();
            ExecuteFabrics( transitiveFabricTypes, project, ref result );

            // Discover the fabrics inside the current project.
            var fabrics =
                compileTimeProject.FabricTypes
                    .OrderBy( t => t )
                    .Select( compileTimeProject.GetType )
                    .Select( fabricType => this.CreateDriver( fabricType, aspectClasses ) )
                    .OrderBy( x => x.Kind )
                    .ThenBy( x => x.OrderingKey );

            ExecuteFabrics( fabrics, project, ref result );

            return result;
        }

        private static void ExecuteFabrics( IEnumerable<FabricDriver> drivers, IProject project, ref FabricResult result )
        {
            foreach ( var driver in drivers )
            {
                var partialResult = driver.Execute( project );
                result = result.Merge( partialResult );
            }
        }

        private FabricDriver CreateDriver( Type fabricType, AspectClassRegistry aspectClasses )
        {
            var fabric = (IFabric) this._userCodeInvoker.Invoke( () => Activator.CreateInstance( fabricType ) );

            switch ( fabric )
            {
                case ITypeFabric typeFabric:
                    return new TypeFabricDriver( this._serviceProvider, aspectClasses, typeFabric );

                case ITransitiveProjectFabric transitiveCompilationFabric:
                    return new CompilationFabricDriver( this._serviceProvider, aspectClasses, transitiveCompilationFabric );

                case IProjectFabric compilationFabric:
                    return new CompilationFabricDriver( this._serviceProvider, aspectClasses, compilationFabric );

                case INamespaceFabric namespaceFabric:
                    return new NamespaceFabricDriver( this._serviceProvider, aspectClasses, namespaceFabric );

                default:
                    throw new AssertionFailedException();
            }
        }
    }
}