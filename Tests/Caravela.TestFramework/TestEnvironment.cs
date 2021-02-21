using System.Linq;
using System.Reflection;

namespace Caravela.TestFramework
{
    internal class TestEnvironment
    {
        public static string GetProjectDirectory( Assembly assembly ) =>
            assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single( a => a.Key == "ProjectDirectory" ).Value!;
    }
}
