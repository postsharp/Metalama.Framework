// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.HierarchicalOptions;
using Metalama.Framework.Engine.Introspection;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.UserCode;
using Metalama.Framework.Engine.Validation;
using Metalama.Framework.Fabrics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.Fabrics
{
    /// <summary>
    /// The top-level class that discovers, instantiates and executes fabrics. It exposes an <see cref="ExecuteFabrics"/>
    /// method, which returns the <see cref="PipelineContributorSources"/> object, which is then a part of the <see cref="AspectPipelineConfiguration"/>.
    /// </summary>
    internal sealed class FabricManager
    {
        private readonly IntrospectionPipelineListener? _listener;

        public ProjectServiceProvider ServiceProvider { get; }

        public BoundAspectClassCollection AspectClasses { get; }

        public FabricManager( BoundAspectClassCollection aspectClasses, in ProjectServiceProvider serviceProvider, CompileTimeProject compileTimeProject )
        {
            this.ServiceProvider = serviceProvider;
            this.CompileTimeProject = compileTimeProject;
            this.AspectClasses = aspectClasses;
            this._listener = serviceProvider.GetService<IntrospectionPipelineListener>();
        }

        public UserCodeInvoker UserCodeInvoker => this.ServiceProvider.GetRequiredService<UserCodeInvoker>();

        public CompileTimeProject CompileTimeProject { get; }

        public (PipelineContributorSources PipelineContributorSources, ImmutableArray<string> FabricTypeNames) ExecuteFabrics(
            CompileTimeProject compileTimeProject,
            CompilationModel compilationModel,
            ProjectModel project,
            IDiagnosticAdder diagnosticAdder )
        {
            // Discover the transitive fabrics from project dependencies, and execute them.
            var transitiveFabricTypes = new Tuple<CompileTimeProject, int>( compileTimeProject, 0 )
                .SelectManyRecursiveDistinct(
                    p => p.Item1.References.SelectAsReadOnlyList( r => new Tuple<CompileTimeProject, int>( r, p.Item2 + 1 ) ),
                    includeRoot: false )
                .GroupBy( t => t.Item1 )
                .Select( g => (Project: g.Key, Depth: g.Max( x => x.Item2 )) )
                .SelectMany( x => x.Project.TransitiveFabricTypes.SelectAsReadOnlyList( t => (x.Project, x.Depth, Type: t) ) )
                .OrderByDescending( x => x.Depth )
                .ThenBy( x => x.Type )
                .ToArray();

            var transitiveFabricTypeNames = transitiveFabricTypes.SelectAsReadOnlyList( x => x.Type );

            var transitiveFabricDrivers = transitiveFabricTypes
                .SelectMany( x => this.CreateDrivers( x.Project, x.Type, compilationModel, diagnosticAdder ) )
                .ToReadOnlyList();

            // Discover the fabrics inside the current project.
            var fabricTypeNames = compileTimeProject.FabricTypes
                .OrderBy( t => t )
                .ToImmutableArray();

            var fabricDrivers = fabricTypeNames
                .SelectMany( x => this.CreateDrivers( compileTimeProject, x, compilationModel, diagnosticAdder ) )
                .ToOrderedList( x => x );

            var allFabricTypeNames = fabricTypeNames.Concat( transitiveFabricTypeNames ).ToImmutableArray();

            var typeFabricDrivers = fabricDrivers.OfType<TypeFabricDriver>().Concat( transitiveFabricDrivers.OfType<TypeFabricDriver>() ).ToImmutableArray();

            var aspectSources = ImmutableArray.CreateBuilder<IAspectSource>();
            var validatorSources = ImmutableArray.CreateBuilder<IValidatorSource>();
            var optionsSources = ImmutableArray.CreateBuilder<IHierarchicalOptionsSource>();

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
                        optionsSources.AddRange( result.OptionsSources );
                        this._listener?.AddStaticFabricResult( result );
                    }
                    else
                    {
                        this._listener?.AddStaticFabricFailure( driver );
                    }
                }
            }

            Execute( fabricDrivers.OfType<ProjectFabricDriver>() );
            Execute( transitiveFabricDrivers.OfType<ProjectFabricDriver>() );
            project.Freeze();
            Execute( fabricDrivers.OfType<NamespaceFabricDriver>() );

            var pipelineContributorSources = new PipelineContributorSources(
                aspectSources.ToImmutable(),
                validatorSources.ToImmutable(),
                optionsSources.ToImmutableArray() );

            return (pipelineContributorSources, allFabricTypeNames);
        }

        private IEnumerable<FabricDriver> CreateDrivers(
            CompileTimeProject compileTimeProject,
            string fabricTypeName,
            CompilationModel compilation,
            IDiagnosticAdder diagnostics )
        {
            var fabricType = compileTimeProject.GetType( fabricTypeName );
            var constructor = fabricType.GetConstructor( Type.EmptyTypes );

            if ( constructor == null )
            {
                diagnostics.Report( GeneralDiagnosticDescriptors.TypeMustHavePublicDefaultConstructor.CreateRoslynDiagnostic( null, fabricType ) );

                return Enumerable.Empty<FabricDriver>();
            }

            var executionContext = new UserCodeExecutionContext(
                this.ServiceProvider,
                diagnostics,
                UserCodeDescription.Create( "instantiating the fabric ", fabricType ) );

            if ( !this.UserCodeInvoker.TryInvoke( () => Activator.CreateInstance( fabricType ), executionContext, out var fabric ) )
            {
                return Enumerable.Empty<FabricDriver>();
            }

            switch ( fabric )
            {
                case TypeFabric typeFabric:
                    return TypeFabricDriver.Create( this, compileTimeProject, typeFabric, compilation );

                case TransitiveProjectFabric transitiveCompilationFabric:
                    return new[] { ProjectFabricDriver.Create( this, compileTimeProject, transitiveCompilationFabric, compilation.RoslynCompilation ) };

                case ProjectFabric compilationFabric:
                    return new[] { ProjectFabricDriver.Create( this, compileTimeProject, compilationFabric, compilation.RoslynCompilation ) };

                case NamespaceFabric namespaceFabric:
                    return new[] { NamespaceFabricDriver.Create( this, compileTimeProject, namespaceFabric, compilation.RoslynCompilation ) };

                default:
                    throw new AssertionFailedException( $"Unexpected fabric type: '{fabricType.FullName}." );
            }
        }
    }
}