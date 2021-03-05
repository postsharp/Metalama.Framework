using Caravela.Framework.Project;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.TestInputs.Highlighting.CastExpressions.RunTimeToCompileTimeCast
{
    class Aspect
    {
        [CompileTime]
        private class CompileTimeParent
        {
        }

        private class RunTimeChild : CompileTimeParent
        {
        }

        [TestTemplate]
        dynamic Template()
        {
            //TODO: Is it correct that everything is marked as compile-time?
            RunTimeChild child = new();
            
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
