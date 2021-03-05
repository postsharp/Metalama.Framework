using Caravela.Framework.Project;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.TestInputs.Highlighting.CastExpressions.CompileTimeToCompileTimeCast
{
    class Aspect
    {
        [CompileTime]
        private class CompileTimeParent
        {
        }

        [CompileTime]
        private class CompileTimeChild : CompileTimeParent
        {
        }

        [TestTemplate]
        dynamic Template()
        {
            CompileTimeChild child = new();

            //TODO: child shouldn't be highlighted as a template keyword.            
            var parentReference1 = (CompileTimeParent)child;
            CompileTimeParent parentReference2 = (CompileTimeParent)child;
            CompileTimeParent parentReference3 = child;

            var parentReference4 = child as CompileTimeParent;
            CompileTimeParent parentReference5 = child as CompileTimeParent;

            if (child is CompileTimeParent parentReference6)
            {
            }

            return proceed();
        }
    }
}
