// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Fabrics;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Impl.Utilities;
using Caravela.Framework.Project;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Caravela.Framework.Impl.Fabrics
{
    /// <summary>
    /// The top-level class that discovers, instantiates and executes fabrics. It exposes an <see cref="AspectSource"/>
    /// property, which must be included in the list of aspect sources of the pipeline.
    /// </summary>
    internal sealed class FabricManager
    {
        public IServiceProvider ServiceProvider { get; }

        public BoundAspectClassCollection AspectClasses { get; }

        public FabricManager( BoundAspectClassCollection aspectClasses, IServiceProvider serviceProvider, CompileTimeProject compileTimeProject )
        {
            this.ServiceProvider = serviceProvider;
            this.CompileTimeProject = compileTimeProject;
            this.AspectClasses = aspectClasses;
        }

        public UserCodeInvoker UserCodeInvoker => this.ServiceProvider.GetService<UserCodeInvoker>();

        public CompileTimeProject CompileTimeProject { get; }

        public FabricsConfiguration ExecuteFabrics( CompileTimeProject compileTimeProject, Compilation runTimeCompilation, ProjectModel project )
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
                .Select( x => (StaticFabricDriver) this.CreateDriver( x, runTimeCompilation ) );

            // Discover the fabrics inside the current project.
            var fabrics =
                compileTimeProject.FabricTypes
                    .OrderBy( t => t )
                    .Select( compileTimeProject.GetType )
                    .Select( x => this.CreateDriver( x, runTimeCompilation ) )
                    .OrderBy( x => x )
                    .ToList();

            var typeFabricDrivers = fabrics.OfType<TypeFabricDriver>().ToImmutableArray();
            

            var aspectSources = ImmutableArray.CreateBuilder<IAspectSource>();

            if ( !typeFabricDrivers.IsEmpty )
            {
                aspectSources.Add( new FabricAspectSource( this, typeFabricDrivers ) );
            }

            // Execute static drivers now.
            
            using ( CaravelaExecutionContextImpl.WithContext( this.ServiceProvider, null, null ) )
            {
                void Execute( IEnumerable<StaticFabricDriver> drivers )
                {
                    foreach ( var driver in drivers )
                    {
                        var result = driver.Execute( project );
                        aspectSources.AddRange( result.AspectSources );
                    }
                }
                
                Execute( fabrics.OfType<ProjectFabricDriver>() );
                Execute( transitiveFabricTypes );
                project.Freeze();
                Execute( fabrics.OfType<NamespaceFabricDriver>() );
            }

            return new FabricsConfiguration( aspectSources.ToImmutable() );
        }

        private FabricDriver CreateDriver( Type fabricType, Compilation runTimeCompilation )
        {
            var fabric = (Fabric) this.UserCodeInvoker.Invoke( () => Activator.CreateInstance( fabricType ) );

            switch ( fabric )
            {
                case TypeFabric typeFabric:
                    return new TypeFabricDriver( this, typeFabric, runTimeCompilation );

                case TransitiveProjectFabric transitiveCompilationFabric:
                    return new ProjectFabricDriver( this, transitiveCompilationFabric, runTimeCompilation );

                case ProjectFabric compilationFabric:
                    return new ProjectFabricDriver( this, compilationFabric, runTimeCompilation );

                case NamespaceFabric namespaceFabric:
                    return new NamespaceFabricDriver( this, namespaceFabric, runTimeCompilation );

                default:
                    throw new AssertionFailedException();
            }
        }
    }
}