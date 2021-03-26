using Caravela.Framework.Project;
using Caravela.Framework.Tests.Integration.Templating;
using System.Collections.Generic;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.TestInputs.Highlighting.WhileStatements.WhileStatements
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
                i++;
            }

            int j = 0;
            while (j < runTimeObject.runTimeList.Count)
            {
                runTimeObject.runTimeList[j].ToString();
                var x = runTimeObject.runTimeList[j];
                x.ToString();
                j++;
            }

            return proceed();
        }
    }
}
