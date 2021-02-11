﻿using System;
using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Transformations;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl
{
    internal sealed class AspectPartResult
    {
        
        // TODO: should this be reactive or handled as a side value?
        public CompilationModel Compilation { get; }

        public IReadOnlyList<Diagnostic> Diagnostics { get; }

        public IReadOnlyList<AspectInstance> Aspects { get; }
        
        public IReadOnlyList<Advice> Advices { get; }
        
        public IReadOnlyList<INonObservableTransformation> Transformations { get; }

        
        public AspectPartResult( RoslynBasedCompilationModel compilation, CompileTimeAssemblyLoader loader )
            : this
            (compilation,
                Array.Empty<Diagnostic>(),
                  compilation.GetAspectsFromAttributes(loader).ToList(),
                  Array.Empty<Advice>(),
                Array.Empty<INonObservableTransformation>())
        {
        }

        private AspectPartResult(
            CompilationModel compilation, 
            IReadOnlyList<Diagnostic> diagnostics, 
            IReadOnlyList<AspectInstance> aspects,
            IReadOnlyList<Advice> advices, 
            IReadOnlyList<INonObservableTransformation> transformations )
        {
            this.Diagnostics = diagnostics;
            this.Compilation = compilation;
            this.Aspects = aspects;
            this.Advices = advices;
            this.Transformations = transformations;
        }

        public AspectPartResult WithNewResults( CompilationModel compilation,
            IReadOnlyList<Diagnostic> additionalDiagnostics,
            IReadOnlyList<AspectInstance> additionalAspects,
            IReadOnlyList<Advice> additionalAdvices, 
            IReadOnlyList<INonObservableTransformation> additionalTransformations )
        {
            return new ( 
                compilation,
                this.Diagnostics.Concat( additionalDiagnostics ),
                additionalAspects.Concat( additionalAspects ),
                this.Advices.Concat( additionalAdvices ),
                this.Transformations.Concat( additionalTransformations )
                );
        }

    }

    internal static class EnumerableExtensions
    {
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
