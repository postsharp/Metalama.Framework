using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.TestInputs.Highlighting.MemberAccess.RunTimeFieldMemberAccess
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            //TODO: On the first line the runTimeClass is highlighted as a compile-time variable,
            // but on the second line it is not.
            RunTimeClass runTimeClass = new();
            runTimeClass.field.ToString();
            return proceed();
        }
    }

    class RunTimeClass
    {
        public int field;
    }
}
