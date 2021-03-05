using Caravela.Framework.Project;
using Caravela.TestFramework.Templating;
using System.Collections.Generic;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.TestInputs.Highlighting.DoWhileStatements.DoWhileStatementsWithBreak
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
            int i = 0;

            do
            {
                this.compileTimeList[i].ToString();
                var x = this.compileTimeList[i];
                x.ToString();
                i++;
                break;
            }
            while (i < this.compileTimeList.Count);

            int j = 0;
            do
            {
                this.runTimeList[j].ToString();
                var x = this.runTimeList[j];
                x.ToString();
                j++;
                break;
            }
            while (j < this.runTimeList.Count);

            //TODO: Should a build-time index be allowed?
            this.compileTimeIndex = 0;
            do
            {
                this.compileTimeList[this.compileTimeIndex].ToString();
                var x = this.compileTimeList[this.compileTimeIndex];
                x.ToString();
                this.compileTimeIndex++;
                break;
            }
            while (this.compileTimeIndex < this.compileTimeList.Count);

            this.compileTimeIndex = 0;
            do
            {
                this.runTimeList[this.compileTimeIndex].ToString();
                var x = this.runTimeList[this.compileTimeIndex];
                x.ToString();
                this.compileTimeIndex++;
                break;
            }
            while (this.compileTimeIndex < this.runTimeList.Count);

            return proceed();
        }
    }
}
