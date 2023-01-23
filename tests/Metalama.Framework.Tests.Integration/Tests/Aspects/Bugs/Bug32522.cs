// @RemoveOutputCode

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32522;

using Metalama.Framework.Aspects;
using System.Collections.Immutable;


[CompileTime]
class C
{
    void M()
    {
        var builder = ImmutableArray.CreateBuilder<string>();
        builder.Add( "");
        builder.ToImmutable();
    }
}