using Caravela.TestFramework.Templating;
using System;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.TestInputs.Highlighting.Declarations.UninitializedVariableDeclarations
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            //TODO: Should all these be marked as compile-time variables? (The condition in VisitVariableDeclarator says that they should be run-time.)
            int scalar;
            int[] array;
            object @object;
            object constructedObject;
            string @string;
            Action action;
            (int, byte) tuple;
            Tuple<int, byte> generic;

            return proceed();
        }
    }
}
