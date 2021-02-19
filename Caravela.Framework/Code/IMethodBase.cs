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
        IMethodList LocalFunctions { get; }

        /// <summary>
        /// Gets the list of parameters of the current method.
        /// </summary>
        IParameterList Parameters { get; }

        /// <summary>
        /// Gets the kind of method (such as <see cref="Code.MethodKind.Default"/> or <see cref="Code.MethodKind.PropertyGet"/>.
        /// </summary>
        MethodKind MethodKind { get; }
    }
}