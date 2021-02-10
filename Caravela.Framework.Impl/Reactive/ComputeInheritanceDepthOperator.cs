using System.Collections.Generic;
using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using static System.Math;

namespace Caravela.Framework.Impl.Reactive
{
    internal class ComputeInheritanceDepthOperator : ReactiveCollectionOperator<INamedType, (INamedType type, int depth)>
    {
        private const int _computePendingMarker = -1;
        private const int _cycleMarker = int.MaxValue;

        public ComputeInheritanceDepthOperator( IReactiveCollection<INamedType> source ) : base( source )
        {
        }

        protected override ReactiveOperatorResult<IEnumerable<(INamedType type, int depth)>> EvaluateFunction( IEnumerable<INamedType> source )
        {
            var depthDictionary = new Dictionary<INamedType, int>( EqualityComparerFactory.GetEqualityComparer<INamedType>() );
            var diagnostics = new List<Diagnostic>();

            int ComputeDepth( INamedType type )
            {
                if ( !depthDictionary.TryGetValue( type, out var myDepth ) )
                {
                    // Detect cycles.
                    if ( myDepth == _computePendingMarker )
                    {
                        // TODO: add proper diagnostic.
                        return _cycleMarker;
                    }

                    depthDictionary[type] = _computePendingMarker;

                    var baseDepth = -1;

                    // Nested types are processed after their containing type.
                    if ( type.ContainingElement is INamedType containingType )
                    {
                        baseDepth = Max( baseDepth, ComputeDepth( containingType ) );
                    }

                    // Base types are processed before derived types.
                    if ( type.BaseType != null )
                    {
                        baseDepth = Max( baseDepth, ComputeDepth( type.BaseType ) );

                        // We require nested types of the base type to be processed before derived types.
                        // This can cause cycles in computing the inheritance depth. The cycle could be addressed
                        // by taking an arbitrary decision and emitting a warning, however this interface does
                        // not support emitting warnings.
                        foreach ( var nestedType in type.BaseType.NestedTypes.GetValue() )
                        {
                            baseDepth = Max( baseDepth, ComputeDepth( nestedType ) );
                        }
                    }

                    // Implemented interfaces are processed before their implementations.
                    foreach ( var interfaceImplementation in type.ImplementedInterfaces.GetValue() )
                    {
                        baseDepth = Max( baseDepth, ComputeDepth( interfaceImplementation ) );
                    }

                    myDepth = baseDepth == _cycleMarker ? _cycleMarker : baseDepth + 1;

                    depthDictionary[type] = myDepth;
                }

                return myDepth;
            }

            // TODO: add dependencies to BaseType and ImplementedInterfaces. This is not necessary until we support really reactive sources.

            IEnumerable<(INamedType type, int depth)> Impl()
            {
                foreach ( var type in source )
                {
                    var depth = ComputeDepth( type );
                    if ( depth != _cycleMarker )
                    {
                        yield return (type, depth);
                    }
                    else
                    {
                        // We have a cycle for this type, so we skip it.
                        // TODO: implement better handing for nesting/base cycles or change the ordering rules.
                    }
                }
            }

            return new ( Impl(), ReactiveSideValues.Create( DiagnosticsSideValue.Get( diagnostics ) ) );
        }

        protected override void OnSourceItemAdded( IReactiveSubscription sourceSubscription, INamedType item, in IncrementalUpdateToken updateToken )
        {
            // Incremental updates not implemented.
            updateToken.SetBreakingChange();
        }

        protected override void OnSourceItemRemoved( IReactiveSubscription sourceSubscription, INamedType item, in IncrementalUpdateToken updateToken )
        {
            // Incremental updates not implemented.
            updateToken.SetBreakingChange();
        }

        protected override void OnSourceItemReplaced( IReactiveSubscription sourceSubscription, INamedType oldItem, INamedType newItem, in IncrementalUpdateToken updateToken )
        {
            // Incremental updates not implemented.
            updateToken.SetBreakingChange();
        }
    }
}
