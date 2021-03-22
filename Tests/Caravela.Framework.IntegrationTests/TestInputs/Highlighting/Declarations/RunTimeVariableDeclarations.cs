﻿using Caravela.Framework.Project;
using Caravela.TestFramework.Templating;
using System;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.TestInputs.Highlighting.Declarations.RunTimeVariableDeclarations
{
    class RuntimeClass
    {
        public void RunTimeMethod()
        {
        }
    }

    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var runTimeClassInstance = new RuntimeClass();

            int scalar = 0;
            int[] array = new int[10];
            object @object = "";
            string @string = "";
            Action action = runTimeClassInstance.RunTimeMethod;
            (int, byte) tuple = (0, 1);
            Tuple<int, byte> generic = new Tuple<int, byte>(2, 3);

            return proceed();
        }
    }
}
