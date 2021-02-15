// unset

using System.Collections.Generic;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Represents a method or a constructor.
    /// </summary>
    public interface IMethodBase : IMember
    {
        /// <summary>
        /// Gets the list of local functions declared by the current method.
        /// </summary>
        IReadOnlyList<IMethod> LocalFunctions { get; }

        /// <summary>
        /// Gets the list of parameters of the current method.
        /// </summary>
        IReadOnlyList<IParameter> Parameters { get; }
        
        /// <summary>
        /// Gets the kind of method (such as <see cref="Code.MethodKind.Default"/> or <see cref="Code.MethodKind.PropertyGet"/>.
        /// </summary>
        MethodKind MethodKind { get; }

    }
}