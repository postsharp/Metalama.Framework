// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Transformations;
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

        private ConcurrentLinkedList<IntroduceParameterTransformation>? _unorderedParameters;
        private ConcurrentLinkedList<IntroduceConstructorInitializerArgumentTransformation>? _unorderedArguments;
        private ConcurrentLinkedList<SetInitializerExpressionTransformation>? _unorderedExpressions;

        public ImmutableArray<IntroduceParameterTransformation> Parameters { get; private set; }

        public ImmutableArray<IntroduceConstructorInitializerArgumentTransformation> Arguments { get; private set; }

        public ImmutableArray<SetInitializerExpressionTransformation> Expressions { get; private set; }

        public void Sort( TransformationLinkerOrderComparer comparer )
        {
            this.Arguments = this._unorderedArguments?.OrderBy( a => a.ParameterIndex ).ToImmutableArray()
                             ?? ImmutableArray<IntroduceConstructorInitializerArgumentTransformation>.Empty;

            this.Parameters = this._unorderedParameters?.OrderBy( p => p.Parameter.Index ).ToImmutableArray()
                              ?? ImmutableArray<IntroduceParameterTransformation>.Empty;

            this.Expressions = this._unorderedExpressions?.ToImmutableArray() ?? ImmutableArray<SetInitializerExpressionTransformation>.Empty;
        }

        public void Add( IntroduceParameterTransformation transformation )
            => LazyInitializer.EnsureInitialized( ref this._unorderedParameters ).Add( transformation );

        public void Add( IntroduceConstructorInitializerArgumentTransformation argument )
            => LazyInitializer.EnsureInitialized( ref this._unorderedArguments ).Add( argument );

        public void Add( SetInitializerExpressionTransformation transformation )
            => LazyInitializer.EnsureInitialized( ref this._unorderedExpressions ).Add( transformation );
    }

    // Currently unused, but might be useful in the future.
    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed class TypeLevelTransformations { }
}