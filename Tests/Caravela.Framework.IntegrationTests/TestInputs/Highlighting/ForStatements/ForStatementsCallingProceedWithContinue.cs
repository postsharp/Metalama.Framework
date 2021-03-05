using Caravela.Framework.Project;
using Caravela.TestFramework.Templating;
using System.Collections.Generic;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.TestInputs.Highlighting.ForStatements.ForStatementsCallingProceedWithContinue
{
    class Aspect
    {
        //TODO: This line should probably not be highlighted.
        private IList<int> runTimeList;

        //TODO: These lines should probably not be highlighted.
        [CompileTime]
        private IList<int> compileTimeList;

        [CompileTime]
        private int compileTimeIndex;

        [TestTemplate]
        dynamic Template()
        {
            for (int i = 0; i < this.compileTimeList.Count; i++)
            {
                this.compileTimeList[i].ToString();
                var x = this.compileTimeList[i];
                x.ToString();
                proceed();
                continue;
            }

            for (int i = 0; i < this.runTimeList.Count; i++)
            {
                this.runTimeList[i].ToString();
                var x = this.runTimeList[i];
                x.ToString();
                proceed();
                continue;
            }

            for (this.compileTimeIndex = 0; this.compileTimeIndex < this.compileTimeList.Count; this.compileTimeIndex++)
            {
                this.compileTimeList[this.compileTimeIndex].ToString();
                var x = this.compileTimeList[this.compileTimeIndex];
                x.ToString();
                proceed();
                continue;
            }

            for (this.compileTimeIndex = 0; this.compileTimeIndex < this.runTimeList.Count; this.compileTimeIndex++)
            {
                this.runTimeList[this.compileTimeIndex].ToString();
                var x = this.runTimeList[this.compileTimeIndex];
                x.ToString();
                proceed();
                continue;
            }

            return proceed();
        }
    }
}
