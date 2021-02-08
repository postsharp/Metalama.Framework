using Caravela.Framework.Project;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Defines a method <see cref="ToDisplayString"/> that renders the current code element into a human-readable
    /// string.
    /// </summary>
    [CompileTime]
    public interface IDisplayable
    {
        /// <summary>
        /// Renders the current code element into a human-readable string.
        /// </summary>
        /// <param name="format">Reserved for future use. Specifies formatting options.</param>
        /// <param name="context">Reserved for future use. Specifies the context in which the string must be displayed. This allow to abbreviate a few pieces of information.</param>
        /// <returns>A human-readable string for the current code element.</returns>
        string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null );
    }
}