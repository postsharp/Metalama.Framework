// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Introspection;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Utilities.UserCode;
using Metalama.Framework.Engine.Validation;
using Metalama.Framework.Fabrics;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.Fabrics
{
    /// <summary>
    /// The top-level class that discovers, instantiates and executes fabrics. It exposes an <see cref="ExecuteFabrics"/>
    /// method, which returns the <see cref="FabricsConfiguration"/> object, which is then a part of the <see cref="AspectPipelineConfiguration"/>.
    /// </summary>
    internal sealed class FabricManager
    {
        private readonly IntrospectionPipelineListener? _listener;

        public IServiceProvider ServiceProvider { get; }

        public BoundAspectClassCollection AspectClasses { get; }

        public FabricManager( BoundAspectClassCollection aspectClasses, IServiceProvider serviceProvider, CompileTimeProject compileTimeProject )
        {
            this.ServiceProvider = serviceProvider;
            this.CompileTimeProject = compileTimeProject;
            this.AspectClasses = aspectClasses;
            this._listener = serviceProvider.GetService<IntrospectionPipelineListener>();
        }

        public UserCodeInvoker UserCodeInvoker => this.ServiceProvider.GetRequiredService<UserCodeInvoker>();

        public CompileTimeProject CompileTimeProject { get; }

        public FabricsConfiguration ExecuteFabrics(
            CompileTimeProject compileTimeProject,
            CompilationModel compilationModel,
            ProjectModel project,
            IDiagnosticAdder diagnosticAdder )
        {
            // Discover the transitive fabrics from project dependencies, and execute them.
            var transitiveFabricTypes = new Tuple<CompileTimeProject, int>( compileTimeProject, 0 )
                .SelectManyRecursive( p => p.Item1.References.Select( r => new Tuple<CompileTimeProject, int>( r, p.Item2 + 1 ) ), false, false )
                .GroupBy( t => t.Item1 )
                .Select( g => (Project: g.Key, Depth: g.Max( x => x.Item2 )) )
                .SelectMany( x => x.Project.TransitiveFabricTypes.Select( t => (x.Project, x.Depth, Type: t) ) )
                .OrderByDescending( x => x.Depth )
                .ThenBy( x => x.Type )
                .Select( x => x.Project.GetType( x.Type ) )
                .Select( x => (StaticFabricDriver?) this.CreateDriver( x, compilationModel.RoslynCompilation, diagnosticAdder ) )
                .WhereNotNull();

            // Discover the fabrics inside the current project.
            var fabrics =
                compileTimeProject.FabricTypes
                    .OrderBy( t => t )
                    .Select( compileTimeProject.GetType )
                    .Select( x => this.CreateDriver( x, compilationModel.RoslynCompilation, diagnosticAdder ) )
                    .WhereNotNull()
                    .OrderBy( x => x )
                    .ToList();

            var typeFabricDrivers = fabrics.OfType<TypeFabricDriver>().ToImmutableArray();

            var aspectSources = ImmutableArray.CreateBuilder<IAspectSource>();
            var validatorSources = ImmutableArray.CreateBuilder<IValidatorSource>();

            if ( !typeFabricDrivers.IsEmpty )
            {
                aspectSources.Add( new FabricAspectSource( this, typeFabricDrivers ) );
            }

            // Execute static drivers now.

            void Execute( IEnumerable<StaticFabricDriver> drivers )
            {
                foreach ( var driver in drivers )
                {
                    if ( driver.TryExecute( project, compilationModel, diagnosticAdder, out var result ) )
                    {
                        aspectSources.AddRange( result.AspectSources );
                        validatorSources.AddRange( result.ValidatorSources );
                        this._listener?.AddStaticFabricResult( result );
                    }
                    else
                    {
                        this._listener?.AddStaticFabricFailure( driver );
                    }
                }
            }

            Execute( fabrics.OfType<ProjectFabricDriver>() );
            Execute( transitiveFabricTypes );
            project.Freeze();
            Execute( fabrics.OfType<NamespaceFabricDriver>() );

            return new FabricsConfiguration( aspectSources.ToImmutable(), validatorSources.ToImmutable() );
        }

        private FabricDriver? CreateDriver( Type fabricType, Compilation runTimeCompilation, IDiagnosticAdder diagnostics )
        {
            var constructor = fabricType.GetConstructor( Type.EmptyTypes );

            if ( constructor == null )
            {
                diagnostics.Report( GeneralDiagnosticDescriptors.TypeMustHavePublicDefaultConstructor.CreateRoslynDiagnostic( null, fabricType ) );

                return null;
            }

            var executionContext = new UserCodeExecutionContext( this.ServiceProvider, diagnostics, UserCodeMemberInfo.FromMemberInfo( constructor ) );

            if ( !this.UserCodeInvoker.TryInvoke( () => Activator.CreateInstance( fabricType ), executionContext, out var fabric ) )
            {
                return null;
            }

            switch ( fabric )
            {
                case TypeFabric typeFabric:
                    return TypeFabricDriver.Create( this, typeFabric, runTimeCompilation );

                case TransitiveProjectFabric transitiveCompilationFabric:
                    return ProjectFabricDriver.Create( this, transitiveCompilationFabric, runTimeCompilation );

                case ProjectFabric compilationFabric:
                    return ProjectFabricDriver.Create( this, compilationFabric, runTimeCompilation );

                case NamespaceFabric namespaceFabric:
                    return NamespaceFabricDriver.Create( this, namespaceFabric, runTimeCompilation );

                default:
                    throw new AssertionFailedException( $"Unexpected fabric type: '{fabricType.FullName}." );
            }
        }
    }
}