using Caravela.Framework.Project;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.TestInputs.Highlighting.MemberAccess.CompileTimeFieldMemberAccess
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            CompileTimeClass compileTimeClass = new();
            //TODO: The compileTimeClass should probably not be highlighted as a template keyword.
            compileTimeClass.field.ToString();
            return proceed();
        }
    }

    [CompileTime]
    class CompileTimeClass
    {
        //TODO: Should this declaration be highlighted?
        public int field;
    }
}
