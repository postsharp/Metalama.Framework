using Caravela.Framework.Code;
using Caravela.Framework.Fabrics;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// Represents the relationship that an object (attribute, fabric, aspect) has created or required another aspect.
    /// These relationships are exposed on <see cref="IAspectInstance.Predecessors"/>.
    /// </summary>
    public readonly struct AspectPredecessor
    {
        public AspectPredecessorKind Kind { get; }

        /// <summary>
        /// Gets the object that created the aspect instance. It can be an <see cref="IAspect"/>, an <see cref="IFabric"/>, or an <see cref="IAttribute"/>.
        /// </summary>
        public object Instance { get; }

        internal AspectPredecessor( AspectPredecessorKind kind, object instance )
        {
            this.Kind = kind;
            this.Instance = instance;
        }
    }
}