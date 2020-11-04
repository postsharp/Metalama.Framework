using Caravela.Framework.Project;
using Caravela.Framework.TestApp.Library;

namespace Caravela.Framework.TestApp
{
    [CompileTime]
    public static class DebugInfo
    {
        public static string GetInfo() => ProcessInfo.GetInfo();
    }
}
