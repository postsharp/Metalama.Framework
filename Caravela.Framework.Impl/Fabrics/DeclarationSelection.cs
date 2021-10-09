// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Attribute = System.Attribute;

namespace Caravela.Framework.Impl.Fabrics
{
    /// <summary>
    /// An implementation of <see cref="IDeclarationSelection{TDeclaration}"/>, which offers a fluent
    /// API to programmatically add children aspects.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class DeclarationSelection<T> : IDeclarationSelection<T>
        where T : class, IDeclaration
    {
        private readonly AspectPredecessor _predecessor;
        private readonly Action<IAspectSource> _registerAspectSource;
        private readonly Func<CompilationModel, IEnumerable<T>> _selector;
        private readonly AspectProjectConfiguration _projectConfiguration;

        public DeclarationSelection(
            AspectPredecessor predecessor,
            Action<IAspectSource> registerAspectSource,
            Func<CompilationModel, IEnumerable<T>> selectTargets,
            AspectProjectConfiguration projectConfiguration )
        {
            this._predecessor = predecessor;
            this._registerAspectSource = registerAspectSource;
            this._selector = selectTargets;
            this._projectConfiguration = projectConfiguration;
        }

        private AspectClass GetAspectClass<TAspect>()
            where TAspect : IAspect
        {
            var aspectClass = this._projectConfiguration.GetAspectClass( typeof(TAspect).FullName );

            if ( aspectClass.IsAbstract )
            {
                throw new ArgumentOutOfRangeException( nameof(TAspect), UserMessageFormatter.Format( $"'{typeof(TAspect)}' is an abstract type." ) );
            }

            return (AspectClass) aspectClass;
        }

        private void RegisterAspectSource( IAspectSource aspectSource ) => this._registerAspectSource( aspectSource );

        public IDeclarationSelection<T> AddAspect<TAspect>( Func<T, Expression<Func<TAspect>>> createAspect )
            where TAspect : Attribute, IAspect<T>
        {
            var aspectClass = this.GetAspectClass<TAspect>();

            this.RegisterAspectSource(
                new ProgrammaticAspectSource<TAspect, T>(
                    aspectClass,
                    ( compilation ) => this._selector( compilation )
                        .Select(
                            t => new AspectInstance(
                                this._projectConfiguration.ServiceProvider,
                                Expression.Lambda<Func<IAspect>>(
                                    this._projectConfiguration.UserCodeInvoker.Invoke( () => createAspect( t ) ).Body,
                                    Array.Empty<ParameterExpression>() ),
                                t,
                                aspectClass,
                                this._predecessor ) ) ) );

            return this;
        }

        public IDeclarationSelection<T> AddAspect<TAspect>( Func<T, TAspect> createAspect )
            where TAspect : Attribute, IAspect<T>
        {
            var aspectClass = this.GetAspectClass<TAspect>();

            this.RegisterAspectSource(
                new ProgrammaticAspectSource<TAspect, T>(
                    aspectClass,
                    ( compilation ) => this._selector( compilation )
                        .Select(
                            t => new AspectInstance(
                                this._projectConfiguration.UserCodeInvoker.Invoke( () => createAspect( t ) ),
                                t,
                                aspectClass,
                                _predecessor ) ) ) );
            
            return this;
        }

        public IDeclarationSelection<T> AddAspect<TAspect>()
            where TAspect : Attribute, IAspect<T>, new()
        {
            var aspectClass = this.GetAspectClass<TAspect>();

            this.RegisterAspectSource(
                new ProgrammaticAspectSource<TAspect, T>(
                    aspectClass,
                    ( compilation ) => this._selector( compilation )
                        .Select(
                            t => new AspectInstance(
                                new TAspect(),
                                t,
                                aspectClass,
                                _predecessor ) ) ) );
            
            return this;
        }

        [Obsolete( "Not implemented." )]
        public IDeclarationSelection<T> RequireAspect<TTarget, TAspect>( TTarget target )
            where TTarget : class, IDeclaration
            where TAspect : IAspect<TTarget>, new()
            => throw new NotImplementedException();

        [Obsolete( "Not implemented." )]
        public IDeclarationSelection<T> AddAnnotation<TAspect, TAnnotation>( Func<T, TAnnotation> getAnnotation )
            where TAspect : IAspect
            where TAnnotation : IAnnotation<T, TAspect>, IEligible<T>
            => throw new NotImplementedException();
    }
}