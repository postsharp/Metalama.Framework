// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.Advices
{
    internal class AdviceImplementationResult
    {
        public ImmutableArray<Diagnostic> Diagnostics { get; }

        public ImmutableArray<IObservableTransformation> ObservableTransformations { get; }

        public ImmutableArray<INonObservableTransformation> NonObservableTransformations { get; }

        public AdviceImplementationResult(
            ImmutableArray<Diagnostic> diagnostic = default,
            ImmutableArray<IObservableTransformation> observableTransformations = default,
            ImmutableArray<INonObservableTransformation> nonObservableTransformations = default )
        {
            this.Diagnostics = diagnostic.IsDefault ? ImmutableArray<Diagnostic>.Empty : diagnostic;
            this.ObservableTransformations = observableTransformations.IsDefault ? ImmutableArray<IObservableTransformation>.Empty : observableTransformations;

            this.NonObservableTransformations =
                nonObservableTransformations.IsDefault ? ImmutableArray<INonObservableTransformation>.Empty : nonObservableTransformations;
        }

        public static AdviceImplementationResult Empty
            => new(
                ImmutableArray<Diagnostic>.Empty,
                ImmutableArray<IObservableTransformation>.Empty,
                ImmutableArray<INonObservableTransformation>.Empty );

        public static AdviceImplementationResult Create( params ITransformation[] transformations )
        {
            return Create( (IEnumerable<ITransformation>) transformations );
        }

        public static AdviceImplementationResult Create( IEnumerable<ITransformation> transformations, ImmutableArray<Diagnostic> diagnostics = default )
        {
            ImmutableArray<IObservableTransformation>.Builder? observableTransformations = null;
            ImmutableArray<INonObservableTransformation>.Builder? nonObservableTransformations = null;

            foreach ( var transformation in transformations )
            {
                if ( transformation is IObservableTransformation observableTransformation )
                {
                    if ( observableTransformations == null )
                    {
                        observableTransformations = ImmutableArray.CreateBuilder<IObservableTransformation>();
                    }

                    observableTransformations.Add( observableTransformation );
                }
                else if ( transformation is INonObservableTransformation nonObservableTransformation )
                {
                    if ( nonObservableTransformations == null )
                    {
                        nonObservableTransformations = ImmutableArray.CreateBuilder<INonObservableTransformation>();
                    }

                    nonObservableTransformations.Add( nonObservableTransformation );
                }
            }

            return new AdviceImplementationResult(
                diagnostics.IsDefault ? ImmutableArray<Diagnostic>.Empty : diagnostics,
                observableTransformations != null ? observableTransformations.ToImmutable() : ImmutableArray<IObservableTransformation>.Empty,
                nonObservableTransformations != null ? nonObservableTransformations.ToImmutable() : ImmutableArray<INonObservableTransformation>.Empty );
        }

        public static AdviceImplementationResult Create( params Diagnostic[] diagnostics )
        {
            return new AdviceImplementationResult(
                ImmutableArray.Create( diagnostics ),
                ImmutableArray<IObservableTransformation>.Empty,
                ImmutableArray<INonObservableTransformation>.Empty );
        }

        public AdviceImplementationResult WithTransformations( params ITransformation[] transformations )
        {
            return this.WithTransformations( (IReadOnlyList<ITransformation>) transformations );
        }

        public AdviceImplementationResult WithTransformations( IReadOnlyList<ITransformation> transformations )
        {
            return new AdviceImplementationResult(
                this.Diagnostics,
                this.ObservableTransformations.AddRange( transformations.OfType<IObservableTransformation>() ),
                this.NonObservableTransformations.AddRange( transformations.OfType<INonObservableTransformation>() ) );
        }

        public AdviceImplementationResult WithDiagnostics( params Diagnostic[] diagnostics )
        {
            return new AdviceImplementationResult(
                this.Diagnostics.AddRange( diagnostics ),
                this.ObservableTransformations,
                this.NonObservableTransformations );
        }
    }
}