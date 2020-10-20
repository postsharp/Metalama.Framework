using System;
using System.Collections.Generic;
using System.Text;
using Caravela.Reactive;
using Caravela.Reactive.Implementation;
using static System.Math;

namespace Caravela.Framework.Impl.CodeModel
{
    class ComputeInheritanceDepthOperator : ReactiveCollectionOperator<IType, (IType type, int depth)>
    {
        private readonly Dictionary<IType, int> _depth = new Dictionary<IType, int>( EqualityComparerFactory.GetEqualityComparer<IType>() );

        public ComputeInheritanceDepthOperator( IReactiveCollection<IType> source ) : base( source )
        {
        }

        int ComputeDepth( IType type )
        {
            if ( !this._depth.TryGetValue( type, out var myDepth ) )
            {
                int baseDepth = -1;

                if ( type.BaseType != null )
                {
                    baseDepth = Max( baseDepth, this.ComputeDepth( type.BaseType ) );
                }
                foreach ( var interfaceImplementation in type.ImplementedInterfaces )
                {
                    baseDepth = Max( baseDepth, this.ComputeDepth( interfaceImplementation ));
                }

                myDepth = baseDepth + 1;

                this._depth[type] = myDepth;

            }

            return myDepth;

        }

        protected override IEnumerable<(IType type, int depth)> EvaluateFunction( IEnumerable<IType> source )
        {
            // TODO: add dependencies to BaseType and ImplementedInterfaces. This is not necessary until we support really reactive sources.

            foreach ( var type in source )
            {
                yield return (type, this.ComputeDepth( type ));
            }
        }

        protected override void OnSourceItemAdded( IReactiveSubscription sourceSubscription, IType item, in IncrementalUpdateToken updateToken )
        {
            // Incremental updates not implemented.
            updateToken.SignalChange( true );
        }

        protected override void OnSourceItemRemoved( IReactiveSubscription sourceSubscription, IType item, in IncrementalUpdateToken updateToken )
        {
            // Incremental updates not implemented.
            updateToken.SignalChange( true );
        }

        protected override void OnSourceItemReplaced( IReactiveSubscription sourceSubscription, IType oldItem, IType newItem, in IncrementalUpdateToken updateToken )
        {
            // Incremental updates not implemented.
            updateToken.SignalChange( true );
        }
    }
}
