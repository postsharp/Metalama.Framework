// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Attribute = System.Attribute;

namespace Caravela.Framework.Impl.Fabrics
{
    internal class DeclarationSelection<T> : IDeclarationSelection<T>
        where T : class, IDeclaration
    {
        private readonly Action<IAspectSource> _registerAspectSource;

        protected Func<CompilationModel, IEnumerable<T>> Selector { get; }

        protected FabricContext Context { get; }

        public DeclarationSelection(
            Action<IAspectSource> registerAspectSource,
            Func<CompilationModel, IEnumerable<T>> selectTargets,
            FabricContext context )
        {
            this._registerAspectSource = registerAspectSource;
            this.Selector = selectTargets;
            this.Context = context;
        }

        private AspectClass GetAspectClass<TAspect>()
            where TAspect : IAspect
        {
            var aspectClass = this.Context.AspectClasses[typeof(TAspect).FullName];

            if ( aspectClass.IsAbstract )
            {
                throw new ArgumentOutOfRangeException( nameof(TAspect), UserMessageFormatter.Format( $"'{typeof(TAspect)}' is an abstract type." ) );
            }

            return (AspectClass) aspectClass;
        }

        protected void RegisterAspectSource( IAspectSource aspectSource ) => this._registerAspectSource( aspectSource );

        public void AddAspect<TAspect>( Func<T, Expression<Func<TAspect>>> createAspect )
            where TAspect : Attribute, IAspect<T>
        {
            var aspectClass = this.GetAspectClass<TAspect>();

            this.RegisterAspectSource(
                new ProgrammaticAspectSource<TAspect, T>(
                    aspectClass,
                    compilation => this.Selector( compilation )
                        .Select(
                            t => new AspectInstance(
                                this.Context.ServiceProvider,
                                Expression.Lambda<Func<IAspect>>(
                                    this.Context.UserCodeInvoker.Invoke( () => createAspect( t ) ).Body,
                                    Array.Empty<ParameterExpression>() ),
                                t,
                                aspectClass ) ) ) );
        }

        public void AddAspect<TAspect>( Func<T, TAspect> createAspect )
            where TAspect : Attribute, IAspect<T>
        {
            var aspectClass = this.GetAspectClass<TAspect>();

            this.RegisterAspectSource(
                new ProgrammaticAspectSource<TAspect, T>(
                    aspectClass,
                    compilation => this.Selector( compilation )
                        .Select(
                            t => new AspectInstance(
                                this.Context.UserCodeInvoker.Invoke( () => createAspect( t ) ),
                                t,
                                aspectClass ) ) ) );
        }

        public void AddAspect<TAspect>()
            where TAspect : Attribute, IAspect<T>, new()
        {
            var aspectClass = this.GetAspectClass<TAspect>();

            this.RegisterAspectSource(
                new ProgrammaticAspectSource<TAspect, T>(
                    aspectClass,
                    compilation => this.Selector( compilation )
                        .Select(
                            t => new AspectInstance(
                                new TAspect(),
                                t,
                                aspectClass ) ) ) );
        }

        [Obsolete( "Not implemented." )]
        public void RequireAspect<TTarget, TAspect>( TTarget target )
            where TTarget : class, IDeclaration
            where TAspect : IAspect<TTarget>, new()
            => throw new NotImplementedException();

        [Obsolete( "Not implemented." )]
        public void AddAnnotation<TAspect, TAnnotation>( Func<T, TAnnotation> getAnnotation )
            where TAspect : IAspect
            where TAnnotation : IAnnotation<T, TAspect>, IEligible<T>
            => throw new NotImplementedException();
    }
}