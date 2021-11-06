// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Fabrics;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CodeModel.References;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Project;
using Caravela.Framework.Validation;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;

namespace Caravela.Framework.Impl.Fabrics
{
    /// <summary>
    /// The base class for <see cref="ProjectFabricDriver"/> and <see cref="NamespaceFabricDriver"/>,
    /// which are executed when building the project configuration, not when executing the pipeline.
    /// </summary>
    internal abstract class StaticFabricDriver : FabricDriver
    {
        protected StaticFabricDriver( AspectPipelineConfiguration configuration, IFabric fabric, Compilation runTimeCompilation ) : 
            base(
                configuration,
                fabric,
                runTimeCompilation )
        {
            
        }

        protected abstract StaticFabricResult Execute( IProject project );
        
        

        protected class StaticAmender<T> : BaseAmender<T> 
            where T : class, IDeclaration
        {
            public List<IAspectSource> AspectSources { get; }
            public StaticAmender( IDeclarationRef<T> targetRef, IProject project, AspectPipelineConfiguration configuration, FabricInstance fabricInstance ) :
                base( targetRef, project, configuration, fabricInstance ) { }

            protected override void AddAspectSource( IAspectSource aspectSource ) => this.AspectSources.Add( aspectSource );

            public StaticFabricResult ToResult() => new StaticFabricResult( this.AspectSources.ToImmutableArray() );
        }
    }

    internal record StaticFabricResult( ImmutableArray<IAspectSource> AspectSources );
    
    /// <summary>
    /// The base class for fabric drivers, which are responsible for ordering and executing fabrics.
    /// </summary>
    internal abstract class FabricDriver : IComparable<FabricDriver>
    {
        protected AspectPipelineConfiguration Configuration { get; }

        public IFabric Fabric { get; }

        public Compilation Compilation { get; }

        protected FabricDriver( AspectPipelineConfiguration configuration, IFabric fabric, Compilation runTimeCompilation )
        {
            this.Configuration = configuration;
            this.Fabric = fabric;
            this.Compilation = runTimeCompilation;
            this.OriginalPath = this.Fabric.GetType().GetCustomAttribute<OriginalPathAttribute>().AssertNotNull().Path;

            // Get the original symbol for the fabric. If it has been moved, we have a custom attribute.
            var originalId = this.Fabric.GetType().GetCustomAttribute<OriginalIdAttribute>()?.Id;

            if ( originalId != null )
            {
                this.FabricSymbol =
                    (INamedTypeSymbol) DocumentationCommentId.GetFirstSymbolForDeclarationId( originalId, runTimeCompilation ).AssertNotNull();
            }
            else
            {
                this.FabricSymbol = (INamedTypeSymbol)
                    configuration.ServiceProvider.GetService<ReflectionMapperFactory>().GetInstance( runTimeCompilation ).GetTypeSymbol( fabric.GetType() );
            }
        }

        public INamedTypeSymbol FabricSymbol { get; }

        protected string OriginalPath { get; }

        public abstract void Execute( IAspectBuilderInternal aspectBuilder, FabricTemplateClass fabricTemplateClass, FabricInstance fabricInstance );

        public abstract FabricKind Kind { get; }

        public abstract IDeclaration GetTarget( CompilationModel compilation );

        public int CompareTo( FabricDriver? other )
        {
            if ( ReferenceEquals( this, other ) )
            {
                return 0;
            }

            if ( other == null )
            {
                return 1;
            }

            var kindComparison = this.Kind.CompareTo( other.Kind );

            if ( kindComparison != 0 )
            {
                return kindComparison;
            }

            var originalPathComparison = string.Compare( this.OriginalPath, other.OriginalPath, StringComparison.Ordinal );

            if ( originalPathComparison != 0 )
            {
                return originalPathComparison;
            }

            return this.CompareToCore( other );
        }

        protected virtual int CompareToCore( FabricDriver other )
        {
            // This implementation is common for type and namespace fabrics. It is overwritten for project fabrics.
            // With type and namespace fabrics, having several fabrics per type or namespace is not a useful use case.
            // If that happens, we sort by name of the fabric class. They are guaranteed to have the same parent type or
            // namespace, so the symbol name is sufficient.

            return string.Compare( this.FabricSymbol.Name, other.FabricSymbol.Name, StringComparison.Ordinal );
        }

        protected abstract class BaseAmender<T> : IAmender<T>
            where T : class, IDeclaration
        {
            // The Target property is protected (and not exposed to the API) because
            private readonly IDeclarationRef<T> _targetRef;
            private readonly FabricInstance _fabricInstance;
            private readonly AspectPipelineConfiguration _configuration;

            protected BaseAmender(
                IDeclarationRef<T> targetRef,
                IProject project,
                AspectPipelineConfiguration configuration,
                FabricInstance fabricInstance )
            {
                this._fabricInstance = fabricInstance;
                this._configuration = configuration;
                this._targetRef = targetRef;
                this.Project = project;
            }

            public IProject Project { get; }

            
            protected abstract void AddAspectSource( IAspectSource aspectSource );

            public IDeclarationSelection<TChild> WithMembers<TChild>( Func<T, IEnumerable<TChild>> selector )
                where TChild : class, IDeclaration
                => new DeclarationSelection<TChild>(
                    this._targetRef,
                    new AspectPredecessor( AspectPredecessorKind.Fabric, this._fabricInstance ),
                    this.AddAspectSource,
                    compilation =>
                    {
                        var targetDeclaration = this._targetRef.Resolve( compilation ).AssertNotNull();

                        return this._configuration.UserCodeInvoker.Wrap( this._configuration.UserCodeInvoker.Invoke( () => selector( targetDeclaration ) ) );
                    },
                    this._configuration );

            [Obsolete( "Not implemented." )]
            public void AddValidator( Action<ValidateDeclarationContext<T>> validator ) => throw new NotImplementedException();

            [Obsolete( "Not implemented." )]
            public void AddAnnotation<TTarget, TAspect, TAnnotation>( Func<TTarget, TAnnotation?> provider )
                where TTarget : class, IDeclaration
                where TAspect : IAspect
                where TAnnotation : IAnnotation<TTarget, TAspect>
                => throw new NotImplementedException();
        }

        public abstract FormattableString FormatPredecessor();

        public Location? GetDiagnosticLocation() => this.FabricSymbol.GetDiagnosticLocation();
    }
}