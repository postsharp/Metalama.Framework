namespace Caravela.Framework.Code
{
    /// <summary>
    /// Base interface for <see cref="IMethod"/>, <see cref="IProperty"/> and <see cref="IEvent"/>, but not <see cref="INamedType"/>.
    /// </summary>
    public interface IMember : ICodeElement
    {
        Accessibility Accessibility { get; }

        /// <summary>
        /// Gets the member name.
        /// </summary>
        string Name { get; }
        
        bool IsAbstract { get; }

        /// <summary>
        /// Gets a value indicating whether the member is <c>static</c>.
        /// </summary>
        bool IsStatic { get; }

        /// <summary>
        /// Gets a value indicating whether the member is <c>virtual</c>.
        /// </summary>
        bool IsVirtual { get; }

        bool IsSealed { get; }
        
        bool IsReadOnly { get; }
        
        bool IsOverride { get; }
        
        bool IsNew { get; }
        
        bool IsAsync { get; }

        /// <summary>
        /// Gets the type containing the current member, or <c>null</c> if the current member is not contained
        /// within a type (which should not happen in C#).
        /// </summary>
        INamedType DeclaringType { get; }

        
    }
}