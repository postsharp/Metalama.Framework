using Caravela.Framework.Code;
using System.Linq;

namespace Caravela.Framework.Project
{
    public interface IQueryableCompilation
    {
        IQueryable<INamedType> Types { get; }
    }

    public interface IQueryableNamespace
    {
        IQueryable<INamedType> Types { get; }
    }
}