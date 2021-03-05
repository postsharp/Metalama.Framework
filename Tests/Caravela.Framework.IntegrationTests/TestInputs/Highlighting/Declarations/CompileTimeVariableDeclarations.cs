using Caravela.Framework.Project;
using Caravela.TestFramework.Templating;
using System;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.TestInputs.Highlighting.Declarations.CompileTimeVariableDeclarations
{
    class Aspect
    {
        [CompileTime]
        private void CompileTimeMethod()
        {
        }

        [TestTemplate]
        dynamic Template()
        {
            //TODO: Should all these be marked as runtime? (Those initialized by literals yes.)
            int scalar = 0;
            int[] array = new int[10];
            object @object = "";
            string @string = "";
            Action action = this.CompileTimeMethod;
            (int, byte) tuple = (0, 1);
            Tuple<int, byte> generic = new Tuple<int, byte>(2, 3);

            return proceed();
        }
    }
}
