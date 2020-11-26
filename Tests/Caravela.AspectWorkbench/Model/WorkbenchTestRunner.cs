using Caravela.TestFramework.Templating;
using Microsoft.CodeAnalysis;

namespace Caravela.AspectWorkbench.Model
{
    public class WorkbenchTestRunner : TestRunner
    {
        public new Project CreateProject()
        {
            return base.CreateProject();
        }
    }
}
