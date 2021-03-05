using Caravela.Framework.Project;
using Caravela.TestFramework.Templating;
using System.Collections.Generic;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.TestInputs.Highlighting.ForEachStatements.ForEachStatements
{
    class Aspect
    {
        //TODO: This line should probably not be highlighted.
        private IEnumerable<int> runTimeEnumerable;

        //TODO: These lines should probably not be highlighted.
        [CompileTime]
        private IEnumerable<int> compileTimeEnumerable;

        [TestTemplate]
        dynamic Template()
        {
            foreach (var x in compileTimeEnumerable)
            {
                //TODO: x should not be highlighted as template keyword here.
                x.ToString();
            }

            foreach (var x in runTimeEnumerable)
            {
                x.ToString();
            }

            return proceed();
        }
    }
}
