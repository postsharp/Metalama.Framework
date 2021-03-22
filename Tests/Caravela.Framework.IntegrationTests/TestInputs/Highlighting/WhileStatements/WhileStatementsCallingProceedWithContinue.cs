using Caravela.Framework.Project;
using Caravela.TestFramework.Templating;
using System.Collections.Generic;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.TestInputs.Highlighting.WhileStatements.WhileStatementsCallingProceedWithContinue
{
    class RunTimeClass
    {
        public IList<int> runTimeList;
    }

    [CompileTime]
    class CompileTimeClass
    {
        public IList<int> compileTimeList;
    }

    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var runTimeObject = new RunTimeClass();
            var compileTimeObject = new CompileTimeClass();

            int i = 0;

            while (i < compileTimeObject.compileTimeList.Count)
            {
                compileTimeObject.compileTimeList[i].ToString();
                var x = compileTimeObject.compileTimeList[i];
                x.ToString();
                proceed();
                i++;
                continue;
            }

            int j = 0;
            while (j < runTimeObject.runTimeList.Count)
            {
                runTimeObject.runTimeList[j].ToString();
                var x = runTimeObject.runTimeList[j];
                x.ToString();
                proceed();
                j++;
                continue;
            }

            return proceed();
        }
    }
}
