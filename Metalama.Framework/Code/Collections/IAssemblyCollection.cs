using Metalama.Framework.Aspects;
using System.Collections.Generic;

namespace Metalama.Framework.Code.Collections;

[CompileTime]
public interface IAssemblyCollection : IReadOnlyCollection<IAssembly>
{
    IEnumerable<IAssembly> OfName( string name );
}