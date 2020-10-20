using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Caravela.Reactive;
using Caravela.Reactive.Implementation;
using static System.Math;

namespace Caravela.Framework.Impl.Reactive
{
    class ComputeInheritanceDepthOperator : ReactiveCollectionOperator<INamedType, (INamedType type, int depth)>
    {
        private readonly Dictionary<INamedType, int> _depth = new Dictionary<INamedType, int>( EqualityComparerFactory.GetEqualityComparer<INamedType>() );
        const int computePendingMarker = -1;

        public ComputeInheritanceDepthOperator( IReactiveCollection<INamedType> source ) : base( source )
        {
        }

        int ComputeDepth( INamedType type )
        {
            if ( !this._depth.TryGetValue( type, out var myDepth ) )
            {
                // Detect cycles.
                Debug.Assert( myDepth != computePendingMarker );
                this._depth[type] = computePendingMarker;


                int baseDepth = -1;

                // Nested types are processed after their containing type.
                if ( type.ContainingElement is INamedType containingType )
                {
                    baseDepth = Max( baseDepth, this.ComputeDepth( containingType ) );
                }

                // Base types are processed before derived types.
                if ( type.BaseType != null && type.BaseType is INamedType namedType )
                {
                    baseDepth = Max( baseDepth, this.ComputeDepth( namedType ) );

                    // We require nested types of the base type to be processed before derived types.
                    // This can cause cycles in computing the inheritance depth. The cycle could be addressed
                    // by taking an arbitrary decision and emitting a warning, however this interface does
                    // not support emitting warnings.
                    foreach ( var nestedType in namedType.GetTypeInfo().NestedTypes )
                    {
                        baseDepth = Max( baseDepth, this.ComputeDepth( nestedType ) );
                    }

                }

                // Implemented interfaces are processed before their implementations.
                foreach ( var interfaceImplementation in type.ImplementedInterfaces )
                {
                    baseDepth = Max( baseDepth, this.ComputeDepth( interfaceImplementation ) );
                }

                myDepth = baseDepth + 1;

                this._depth[type] = myDepth;

            }

            return myDepth;

        }

        protected override IEnumerable<(INamedType type, int depth)> EvaluateFunction( IEnumerable<INamedType> source )
        {
            // TODO: add dependencies to BaseType and ImplementedInterfaces. This is not necessary until we support really reactive sources.

            foreach ( var type in source )
            {
                yield return (type, this.ComputeDepth( type ));
            }
        }

        protected override void OnSourceItemAdded( IReactiveSubscription sourceSubscription, INamedType item, in IncrementalUpdateToken updateToken )
        {
            // Incremental updates not implemented.
            updateToken.SignalChange( true );
        }

        protected override void OnSourceItemRemoved( IReactiveSubscription sourceSubscription, INamedType item, in IncrementalUpdateToken updateToken )
        {
            // Incremental updates not implemented.
            updateToken.SignalChange( true );
        }

        protected override void OnSourceItemReplaced( IReactiveSubscription sourceSubscription, INamedType oldItem, INamedType newItem, in IncrementalUpdateToken updateToken )
        {
            // Incremental updates not implemented.
            updateToken.SignalChange( true );
        }
    }
}
