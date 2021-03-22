using Caravela.Framework.Project;
using Caravela.Framework.Tests.Integration.Templating;
using System;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.TestInputs.Highlighting.Declarations.UninitializedVariableDeclarations
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            int scalar;
            int[] array;
            object @object;
            string @string;
            Action action;
            (int, byte) tuple;
            Tuple<int, byte> generic;

            return proceed();
        }
    }
}
