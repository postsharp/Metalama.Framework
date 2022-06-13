using Metalama.Framework.Fabrics;
using System.Linq;

internal class Fabric : ProjectFabric
{
    public override void AmendProject(IProjectAmender project)
    {
        // Selecting the compiler-generated Main method causes an assertion failure.
        project.With( p => p.Types.SelectMany( t => t.Methods ) ).AddAspect<LogAttribute>();

        // Workaround:
        // project.With( p => p.Types.SelectMany( t => t.Methods.Where( m => m.Name != "<Main>$" ) ) ).AddAspect<LogAttribute>();
    }
}