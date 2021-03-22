using Caravela.Framework.Project;
using Caravela.TestFramework.Templating;
using System;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.TestInputs.Highlighting.Declarations.CompileTimeVariableDeclarations
{
    [CompileTime]
    class CompileTimeClass
    {
        public void CompileTimeMethod()
        {
        }
    }

    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var compiletimeClassInstance = new CompileTimeClass();

            int scalar = compileTime(0);
            int[] array = compileTime(new int[10]);
            object @object = compileTime("");
            string @string = compileTime("");
            Action action = compiletimeClassInstance.CompileTimeMethod;
            (int, byte) tuple = compileTime((0, (byte)1));
            Tuple<int, byte> generic = compileTime(new Tuple<int, byte>(2, 3));

            return proceed();
        }
    }
}
