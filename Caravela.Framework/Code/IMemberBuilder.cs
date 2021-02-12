namespace Caravela.Framework.Code
{
    public interface IMemberBuilder : IMember, ICodeElementBuilder
    {
        new Visibility Visibility { get; set; }

        /// <summary>
        /// Gets the member name.
        /// </summary>
        new string Name { get; set; }

        /// <summary>
        /// Gets a value indicating whether the member is <c>static</c>.
        /// </summary>
        new bool IsStatic { get; set; }

        /// <summary>
        /// Gets a value indicating whether the member is <c>virtual</c>.
        /// </summary>
        new bool IsVirtual { get; set; }

        new bool IsSealed { get; set; }
    }
}