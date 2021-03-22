﻿using Caravela.Framework.Project;
using Caravela.TestFramework.Templating;
using System.Collections.Generic;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.TestInputs.Highlighting.ForStatements.ForStatementsCallingProceedWithContinue
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

            for (int i = 0; i < compileTimeObject.compileTimeList.Count; i++)
            {
                compileTimeObject.compileTimeList[i].ToString();
                var x = compileTimeObject.compileTimeList[i];
                x.ToString();
                proceed();
                continue;
            }

            for (int i = 0; i < runTimeObject.runTimeList.Count; i++)
            {
                runTimeObject.runTimeList[i].ToString();
                var x = runTimeObject.runTimeList[i];
                x.ToString();
                proceed();
                continue;
            }

            return proceed();
        }
    }
}
