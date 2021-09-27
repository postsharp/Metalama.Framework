// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Fabrics;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.ServiceProvider;
using Caravela.Framework.Impl.Utilities;
using Caravela.Framework.Validation;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.Fabrics
{
    internal record AspectClassRegistry( ImmutableDictionary<string, AspectClass> AspectClasses );

    internal abstract class FabricDriver
    {
        protected AspectClassRegistry AspectClasses { get; }

        protected IServiceProvider ServiceProvider { get; }

        protected IFabric Fabric { get; }

        protected UserCodeInvoker UserCodeInvoker { get; }

        protected FabricDriver( IServiceProvider serviceProvider, AspectClassRegistry aspectClasses, IFabric fabric )
        {
            this.AspectClasses = aspectClasses;
            this.ServiceProvider = serviceProvider;
            this.UserCodeInvoker = serviceProvider.GetService<UserCodeInvoker>();
            this.Fabric = fabric;
        }

        public abstract FabricResult Execute( IProject project );

        public abstract FabricKind Kind { get; }

        public abstract string OrderingKey { get; }

        protected abstract class BaseBuilder<T> : IFabricBuilder<T>, IFabricBuilderInternal
            where T : IDeclaration
        {
            private readonly IServiceProvider _serviceProvider;
            private AspectClassRegistry _aspectClasses;
            private readonly List<IAspectSource> _aspectSources = new();

            protected UserCodeInvoker UserCodeInvoker { get; }

            public BaseBuilder( IServiceProvider serviceProvider, IProject project, AspectClassRegistry aspectClasses )
            {
                this._serviceProvider = serviceProvider;
                this.UserCodeInvoker = serviceProvider.GetService<UserCodeInvoker>();
                this.Project = project;
                this._aspectClasses = aspectClasses;
            }

            public IReadOnlyList<IAspectSource> AspectSources => this._aspectSources;

            protected abstract T GetTargetDeclaration( CompilationModel compilation );

            public IProject Project { get; }

            public INamedTypeSelection WithTypes( Func<T, IEnumerable<INamedType>> typeQuery )
                => new NamedTypeSelection(
                    this.RegisterAspectSource,
                    compilation =>
                    {
                        var targetDeclaration = this.GetTargetDeclaration( compilation );

                        return this.UserCodeInvoker.Wrap( this.UserCodeInvoker.Invoke( () => typeQuery( targetDeclaration ) ) );
                    },
                    this._serviceProvider,
                    this._aspectClasses );

            protected void RegisterAspectSource( IAspectSource aspectSource ) => this._aspectSources.Add( aspectSource );

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