using Caravela.Framework.Project;
using Caravela.TestFramework.Templating;
using System.Collections.Generic;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.TestInputs.Highlighting.ForEachStatements.ForEachStatementsCallingProceedWithBreak
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
                x.ToString();
                proceed();
                break;
            }

            foreach (var x in runTimeEnumerable)
            {
                x.ToString();
                proceed();
                break;
            }

            return proceed();
        }
    }
}
