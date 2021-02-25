using System.Collections.Generic;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Read-only list of <see cref="IMember"/>.
    /// </summary>
    public interface IMemberList<out T> : IReadOnlyList<T>
        where T : IMember
    {
        /// <summary>
        /// Gets the set of members of a given name. Note that for named types, the short name will be matched,
        /// as opposed to the full, namespace-prefixed name.
        /// </summary>
        /// <param name="name">The member name (not including the namespace, for types).</param>
        /// <returns></returns>
        IEnumerable<T> OfName( string name );
    }
}