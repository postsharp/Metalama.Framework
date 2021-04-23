// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.Advices
{
    internal class AdviceResult
    {
        public ImmutableArray<Diagnostic> Diagnostics { get; }

        public ImmutableArray<IObservableTransformation> ObservableTransformations { get; }

        public ImmutableArray<INonObservableTransformation> NonObservableTransformations { get; }

        private AdviceResult(
            ImmutableArray<Diagnostic> diagnostic,
            ImmutableArray<IObservableTransformation> observableTransformations,
            ImmutableArray<INonObservableTransformation> nonObservableTransformations )
        {
            this.Diagnostics = diagnostic;
            this.ObservableTransformations = observableTransformations;
            this.NonObservableTransformations = nonObservableTransformations;
        }

        public static AdviceResult Create()
        {
            return new( ImmutableArray<Diagnostic>.Empty, ImmutableArray<IObservableTransformation>.Empty, ImmutableArray<INonObservableTransformation>.Empty );
        }

        public static AdviceResult Create( params ITransformation[] transformations )
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

            return new AdviceResult(
                ImmutableArray<Diagnostic>.Empty,
                observableTransformations != null ? observableTransformations.ToImmutable() : ImmutableArray<IObservableTransformation>.Empty,
                nonObservableTransformations != null ? nonObservableTransformations.ToImmutable() : ImmutableArray<INonObservableTransformation>.Empty );
        }

        public static AdviceResult Create( params Diagnostic[] diagnostics )
        {
            return new(
                ImmutableArray.Create( diagnostics ),
                ImmutableArray<IObservableTransformation>.Empty,
                ImmutableArray<INonObservableTransformation>.Empty );
        }

        public AdviceResult WithDiagnostics( params Diagnostic[] diagnostics )
        {
            return new( this.Diagnostics.AddRange( diagnostics ), this.ObservableTransformations, this.NonObservableTransformations );
        }
    }
}