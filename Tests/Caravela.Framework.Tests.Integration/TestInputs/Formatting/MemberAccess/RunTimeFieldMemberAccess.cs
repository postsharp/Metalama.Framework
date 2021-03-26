using Caravela.Framework.Project;
using Caravela.Framework.Tests.Integration.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.TestInputs.Highlighting.MemberAccess.RunTimeFieldMemberAccess
{
    class RunTimeClass
    {
        public int field;
    }

    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            // TODO: The runTimeObject variable shouldn't be highlighted as a compile-time variable. #28397
            RunTimeClass runTimeObject = new();
            runTimeObject.field.ToString();
            return proceed();
        }
    }
}
