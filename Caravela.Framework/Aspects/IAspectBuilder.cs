using Caravela.Framework.Code;

namespace Caravela.Framework.Aspects
{
    
    /// <summary>
    /// An object by the <see cref="IAspect{T}.Initialize"/> method of the aspect to provide advices and child
    /// aspects. This is a weakly-typed variant of the <see cref="IAspectBuilder{T}"/> interface. 
    /// </summary>
    public interface IAspectBuilder
    {
        /// <summary>
        /// Gets the declaration to which the aspect was added.
        /// </summary>
        ICodeElement TargetDeclaration { get; }

        /// <summary>
        /// Exposes methods that allow to create advices.
        /// </summary>
        IAdviceFactory AdviceFactory { get; }
    }
    
    /// <summary>
    /// An object by the <see cref="IAspect{T}.Initialize"/> method of the aspect to provide advices and child
    /// aspects. This is the strongly-typed variant of the <see cref="IAspectBuilder"/> interface. 
    /// </summary>
    public interface IAspectBuilder<out T> : IAspectBuilder
        where T : ICodeElement
    {
        /// <summary>
        /// Gets the declaration to which the aspect was added.
        /// </summary>
        new T TargetDeclaration { get; }
    }

}