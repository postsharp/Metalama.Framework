using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Builders;

namespace Caravela.Framework.Impl.CodeModel.Links
{
    /// <summary>
    /// A weakly typed base for <see cref="ICodeElementLink{T}"/>.
    /// </summary>
    internal interface ICodeElementLink
    {
        /// <summary>
        /// Gets the target object (typically a symbol or a <see cref="CodeElementBuilder"/>) pointed at by the link.
        /// </summary>
        object? Target { get; }
    }
        
    /// <summary>
    /// Represents a link that can be resolved to an element of code by <see cref="GetForCompilation"/>.
    /// </summary>
    /// <typeparam name="T">The type of the target object of the link.</typeparam>
    internal interface ICodeElementLink<out T> : ICodeElementLink
        where T : ICodeElement
    {
        /// <summary>
        /// Gets the target code element for a given <see cref="CompilationModel"/>.
        /// </summary>
        /// <param name="compilation"></param>
        /// <returns></returns>
        T GetForCompilation( CompilationModel compilation );
    }
}