using System;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.TestInputs.Highlighting.Declarations.CompileTimeVariableDeclarations
{
    [CompileTimeOnly]
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
        dynamic?emplate()
        {
            var compiletimeClassInstance = new CompileTimeClass();

            int scalar = meta.CompileTime(0);
            int[] array = meta.CompileTime(new int[10]);
            object @object = meta.CompileTime("");
            string @string = meta.CompileTime("");
            Action action = compiletimeClassInstance.CompileTimeMethod;
            (int, byte) tuple = meta.CompileTime((0, (byte)1));
            Tuple<int, byte> generic = meta.CompileTime(new Tuple<int, byte>(2, 3));

            return meta.Proceed();
        }
    }
}
