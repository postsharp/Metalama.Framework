using System;
using System.Collections.Generic;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl
{
    internal sealed class AspectPartResult
    {

        // TODO: should this be reactive or handled as a side value?
        public CompilationModel Compilation { get; }

        public IReadOnlyList<Diagnostic> Diagnostics { get; }

        public IReadOnlyList<IAspectSource> AspectSources { get; }

        public IReadOnlyList<Advice> Advices { get; }

        public IReadOnlyList<INonObservableTransformation> Transformations { get; }

        /// <summary>
        /// Builds the initial <see cref="AspectPartResult"/>.
        /// </summary>
        /// <param name="compilation"></param>
        /// <param name="loader"></param>
        public AspectPartResult( CompilationModel compilation, CompileTimeAssemblyLoader loader )
            : this
            ( compilation,
                Array.Empty<Diagnostic>(),
                new[] { new CompilationAspectSource( compilation, loader ) },
                  Array.Empty<Advice>(),
                Array.Empty<INonObservableTransformation>() )
        {
        }

        /// <summary>
        /// Incremental constructor.
        /// </summary>
        /// <param name="compilation"></param>
        /// <param name="diagnostics"></param>
        /// <param name="aspectSources"></param>
        /// <param name="advices"></param>
        /// <param name="transformations"></param>
        private AspectPartResult(
            CompilationModel compilation,
            IReadOnlyList<Diagnostic> diagnostics,
            IReadOnlyList<IAspectSource> aspectSources,
            IReadOnlyList<Advice> advices,
            IReadOnlyList<INonObservableTransformation> transformations )
        {
            this.Diagnostics = diagnostics;
            this.Compilation = compilation;
            this.AspectSources = aspectSources;
            this.Advices = advices;
            this.Transformations = transformations;
        }

        public AspectPartResult WithNewResults( CompilationModel compilation,
            IReadOnlyList<Diagnostic> additionalDiagnostics,
            IReadOnlyList<IAspectSource> additionalAspectSources,
            IReadOnlyList<Advice> additionalAdvices,
            IReadOnlyList<INonObservableTransformation> additionalTransformations )
        {
            return new (
                compilation,
                this.Diagnostics.Concat( additionalDiagnostics ),
                additionalAspectSources.Concat( additionalAspectSources ),
                this.Advices.Concat( additionalAdvices ),
                this.Transformations.Concat( additionalTransformations )
                );
        }
    }

    internal static class EnumerableExtensions
    {
        public static IReadOnlyList<T> ConcatNotNull<T>( this IReadOnlyList<T> a, T? b )
        {
            if ( b == null )
            {
                return a;
            }
            else if ( a == null || a.Count == 0 )
            {
                return new[] { b };
            }
            else
            {
                var l = new List<T>( a.Count + 1 );
                l.AddRange( a );
                l.Add( b );
                return l;
            }
        }

        public static IReadOnlyList<T> Concat<T>( this IReadOnlyList<T> a, IReadOnlyList<T>? b )
        {
            if ( b == null || b.Count == 0 )
            {
                return a;
            }
            else if ( a.Count == 0 )
            {
                return b;
            }
            else
            {
                var l = new List<T>( a.Count + b.Count );
                l.AddRange( a );
                l.AddRange( b );
                return l;
            }
        }
    }
}
