using Caravela.Framework.Project;
using Caravela.Framework.Tests.Integration.Templating;
using System.Collections.Generic;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.TestInputs.Highlighting.DoWhileStatements.DoWhileStatementsCallingProceedWithContinue
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

            do
            {
                compileTimeObject.compileTimeList[i].ToString();
                var x = compileTimeObject.compileTimeList[i];
                x.ToString();
                proceed();
                i++;
                continue;
            }
            while (i < compileTimeObject.compileTimeList.Count);

            int j = 0;
            do
            {
                runTimeObject.runTimeList[j].ToString();
                var x = runTimeObject.runTimeList[j];
                x.ToString();
                proceed();
                j++;
                continue;
            }
            while (j < runTimeObject.runTimeList.Count);

            return proceed();
        }
    }
}
