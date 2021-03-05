using Caravela.TestFramework.Templating;
using System;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.TestInputs.Highlighting.Declarations.RunTimeVariableDeclarations
{
    class Aspect
    {
        private void RunTimeMethod()
        {
        }

        private byte runTimeField = 1;

        private int[] runTimeArray = new int[11];

        private object runTimeObject = "";

        private string runTimeString = "";

        [TestTemplate]
        dynamic Template()
        {
            //TODO: Should all these be marked as compile-time variables?
            int scalar = this.runTimeField;
            int[] array = this.runTimeArray;
            object @object = this.runTimeObject;
            object constructedObject = new String(this.runTimeString.ToCharArray());
            string @string = this.runTimeString;
            Action action = this.RunTimeMethod;
            (int, byte) tuple = (this.runTimeField, this.runTimeField);
            Tuple<int, byte> generic = new Tuple<int, byte>(this.runTimeField, this.runTimeField);

            return proceed();
        }
    }
}
