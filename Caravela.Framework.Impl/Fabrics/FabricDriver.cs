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
using Caravela.Framework.Validation;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Caravela.Framework.Impl.Fabrics
{
    internal abstract class FabricDriver
    {
        protected AspectProjectConfiguration Configuration { get; }

        public IFabric Fabric { get; }

        protected FabricDriver( AspectProjectConfiguration configuration, IFabric fabric, Compilation runTimeCompilation )
        {
            this.Configuration = configuration;
            this.Fabric = fabric;
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
                    ReflectionMapper.GetInstance( runTimeCompilation ).GetTypeSymbol( fabric.GetType() );
            }
        }

        public INamedTypeSymbol FabricSymbol { get; }

        public abstract ISymbol TargetSymbol { get; }

        public string OriginalPath { get; }

        public abstract void Execute( IAspectBuilderInternal aspectBuilder, FabricTemplateClass fabricTemplateClass );

        public abstract FabricKind Kind { get; }

        public virtual string OrderingKey => DocumentationCommentId.CreateDeclarationId( this.FabricSymbol );

        public abstract IDeclaration GetTarget( CompilationModel compilation );

        protected abstract class BaseBuilder<T> : IFabricBuilder<T>
            where T : class, IDeclaration
        {
            private readonly IAspectBuilderInternal _aspectBuilder;
            private readonly AspectProjectConfiguration _context;

            protected BaseBuilder( T target, AspectProjectConfiguration context, IAspectBuilderInternal aspectBuilder )
            {
                this._aspectBuilder = aspectBuilder;
                this._context = context;
                this.Target = target;
            }

            public IProject Project => this.Target.Compilation.Project;

            public T Target { get; }

            public IDiagnosticSink Diagnostics => this._aspectBuilder.Diagnostics;

            protected void RegisterAspectSource( IAspectSource aspectSource ) => this._aspectBuilder.AddAspectSource( aspectSource );

            public IDeclarationSelection<TChild> WithMembers<TChild>( Func<T, IEnumerable<TChild>> selector )
                where TChild : class, IDeclaration
                => new DeclarationSelection<TChild>(
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