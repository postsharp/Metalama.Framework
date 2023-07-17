// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Transformations;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace Metalama.Framework.Engine.Linking;

internal sealed partial class LinkerInjectionStep
{
    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed class MemberLevelTransformations
    {
        // TODO: this class is no longer used concurrently, and is being added in transformation order.

        private ConcurrentLinkedList<LinkerInsertedStatement>? _unorderedStatements;
        private ConcurrentLinkedList<IntroduceParameterTransformation>? _unorderedParameters;
        private ConcurrentLinkedList<IntroduceConstructorInitializerArgumentTransformation>? _unorderedArguments;

        public ImmutableArray<LinkerInsertedStatement> Statements { get; private set; }

        public ImmutableArray<IntroduceParameterTransformation> Parameters { get; private set; }

        public ImmutableArray<IntroduceConstructorInitializerArgumentTransformation> Arguments { get; private set; }

        private static ImmutableArray<T> Sort<T>(
            ConcurrentLinkedList<T>? input,
            Func<T, ITransformation> getTransformation,
            TransformationLinkerOrderComparer comparer )
        {
            if ( input == null || input.Count == 0 )
            {
                return ImmutableArray<T>.Empty;
            }
            else if ( input.Count == 1 )
            {
                return input.ToImmutableArray();
            }
            else
            {
                // Insert statements must be executed in inverse order (because we need the forward execution order and not the override order)
                // except within an aspect, where the order needs to be preserved.
                return input.OrderBy( getTransformation, comparer )
                    .GroupBy( x => getTransformation( x ).ParentAdvice.Aspect )
                    .Reverse()
                    .SelectMany( x => x )
                    .ToImmutableArray();
            }
        }

        public void Sort( TransformationLinkerOrderComparer comparer )
        {
            this.Statements = Sort( this._unorderedStatements, s => s.ParentTransformation, comparer );

            this.Arguments = this._unorderedArguments?.OrderBy( a => a.ParameterIndex ).ToImmutableArray()
                             ?? ImmutableArray<IntroduceConstructorInitializerArgumentTransformation>.Empty;

            this.Parameters = this._unorderedParameters?.OrderBy( p => p.Parameter.Index ).ToImmutableArray()
                              ?? ImmutableArray<IntroduceParameterTransformation>.Empty;
        }

        public void Add( LinkerInsertedStatement statement ) => LazyInitializer.EnsureInitialized( ref this._unorderedStatements ).Add( statement );

        public void Add( IntroduceParameterTransformation transformation )
            => LazyInitializer.EnsureInitialized( ref this._unorderedParameters ).Add( transformation );

        public void Add( IntroduceConstructorInitializerArgumentTransformation argument )
            => LazyInitializer.EnsureInitialized( ref this._unorderedArguments ).Add( argument );
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed class TypeLevelTransformations
    {
        // Currently unused, but might be useful in the future.
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public bool AddExplicitDefaultConstructor { get; }
    }
}