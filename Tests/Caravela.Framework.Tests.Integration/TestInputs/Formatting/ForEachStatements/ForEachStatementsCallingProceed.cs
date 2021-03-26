using Caravela.Framework.Project;
using Caravela.Framework.Tests.Integration.Templating;
using System.Collections.Generic;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.TestInputs.Highlighting.ForEachStatements.ForEachStatementsCallingProceed
{
    class RunTimeClass
    {
        public IEnumerable<int> runTimeEnumerable;
    }

    [CompileTime]
    class CompileTimeClass
    {
        public IEnumerable<int> compileTimeEnumerable;
    }

    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var runTimeObject = new RunTimeClass();
            var compileTimeObject = new CompileTimeClass();

            foreach (var x in compileTimeObject.compileTimeEnumerable)
            {
                x.ToString();
                proceed();
            }

            foreach (var x in runTimeObject.runTimeEnumerable)
            {
                x.ToString();
                proceed();
            }

            return proceed();
        }
    }
}
