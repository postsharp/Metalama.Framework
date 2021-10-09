// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Fabrics;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Project;
using Caravela.Framework.Validation;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Caravela.Framework.Impl.Fabrics
{
    /// <summary>
    /// The base class for fabric drivers, which are responsible for ordering and executing fabrics.
    /// </summary>
    internal abstract class FabricDriver : IComparable<FabricDriver>
    {
        protected AspectProjectConfiguration Configuration { get; }

        public IFabric Fabric { get; }

        public Compilation Compilation { get; }

        protected FabricDriver( AspectProjectConfiguration configuration, IFabric fabric, Compilation runTimeCompilation )
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

        public abstract void Execute( IAspectBuilderInternal aspectBuilder, FabricTemplateClass fabricTemplateClass );

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

        protected abstract class BaseBuilder<T> : IAmender<T>
            where T : class, IDeclaration
        {
            private readonly FabricDriver _parent;
            private readonly IAspectBuilderInternal _aspectBuilder;
            private readonly AspectProjectConfiguration _context;

            protected BaseBuilder( FabricDriver parent, T target, AspectProjectConfiguration context, IAspectBuilderInternal aspectBuilder )
            {
                this._parent = parent;
                this._aspectBuilder = aspectBuilder;
                this._context = context;
                this.Target = target;
            }

            public IProject Project => this.Target.Compilation.Project;

            public T Target { get; }

            public IDiagnosticSink Diagnostics => this._aspectBuilder.Diagnostics;

            private void RegisterAspectSource( IAspectSource aspectSource ) => this._aspectBuilder.AddAspectSource( aspectSource );

            public IDeclarationSelection<TChild> WithMembers<TChild>( Func<T, IEnumerable<TChild>> selector )
                where TChild : class, IDeclaration
                => new DeclarationSelection<TChild>(
                    new AspectPredecessor(AspectPredecessorKind.Fabric, this._parent.Fabric),
                    this.RegisterAspectSource,
                    compilation =>
                    {
                        var targetDeclaration = compilation.Factory.GetDeclaration( this.Target );

                        return this._context.UserCodeInvoker.Wrap( this._context.UserCodeInvoker.Invoke( () => selector( targetDeclaration ) ) );
                    },
                    this._context );

            [Obsolete( "Not implemented." )]
            public void AddValidator( Action<ValidateDeclarationContext<T>> validator ) => throw new NotImplementedException();

            [Obsolete( "Not implemented." )]
            public void AddAnnotation<TTarget, TAspect, TAnnotation>( Func<TTarget, TAnnotation?> provider )
                where TTarget : class, IDeclaration
                where TAspect : IAspect
                where TAnnotation : IAnnotation<TTarget, TAspect>
                => throw new NotImplementedException();
        }

        
    }
}