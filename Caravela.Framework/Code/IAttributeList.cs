using System.Collections.Generic;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Read-only list of <see cref="IAttribute"/>.
    /// </summary>
    public interface IAttributeList : IReadOnlyList<IAttribute>
    {
        // TODO: OfType
    }
}