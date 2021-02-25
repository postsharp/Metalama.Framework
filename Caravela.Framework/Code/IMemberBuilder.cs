namespace Caravela.Framework.Code
{
    /// <summary>
    /// Allows to complete the construction of a member that has been created by an advice.
    /// </summary>
    // ReSharper disable once PossibleInterfaceMemberAmbiguity
    public interface IMemberBuilder : IMember, ICodeElementBuilder
    {
        /// <summary>
        /// Gets or sets the accessibility of the member.
        /// </summary>
        new Accessibility Accessibility { get; set; }

        /// <summary>
        /// Gets or sets the member name.
        /// </summary>
        new string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the member is <c>static</c>.
        /// </summary>
        new bool IsStatic { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the member is <c>virtual</c>.
        /// </summary>
        new bool IsVirtual { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the member is <c>sealed</c>.
        /// </summary>
        new bool IsSealed { get; set; }
    }
}