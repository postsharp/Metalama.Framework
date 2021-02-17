using Caravela.Framework.Code;
using Caravela.Framework.Project;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// The base interface for all aspects. A class should not implement
    /// this interface, but the strongly-typed variant <see cref="IAspect{T}"/>.
    /// </summary>
    [CompileTime]
    public interface IAspect
    {
    }

    /// <summary>
    /// The base interface for all aspects, with the type parameter indicating to which types
    /// of declarations the aspect can be added.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IAspect<in T> : IAspect
        where T : ICodeElement
    {
        /// <summary>
        /// Initializes the aspect. The implementation must add advices or child aspects
        /// using the <paramref name="aspectBuilder"/> parameter.
        /// </summary>
        /// <param name="aspectBuilder">An object that allows the aspect to add advices and child
        /// aspects.</param>
        void Initialize( IAspectBuilder<T> aspectBuilder );
    }
}