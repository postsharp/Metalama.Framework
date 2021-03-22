using Caravela.Framework.Project;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.TestInputs.Highlighting.MemberAccess.CompileTimeFieldMemberAccess
{
    [CompileTime]
    class CompileTimeClass
    {
        public int field;
    }

    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            CompileTimeClass compiletimeClass = new();
            compiletimeClass.field.ToString();
            return proceed();
        }
    }
}
