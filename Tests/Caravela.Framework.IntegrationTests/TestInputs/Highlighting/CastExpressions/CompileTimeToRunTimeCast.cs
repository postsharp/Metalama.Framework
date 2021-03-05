using Caravela.Framework.Project;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.TestInputs.Highlighting.CastExpressions.CompileTimeToRunTimeCast
{
    class Aspect
    {
        private class RunTimeParent
        {
        }

        [CompileTime]
        private class CompileTimeChild : RunTimeParent
        {
        }

        [TestTemplate]
        dynamic Template()
        {
            //TODO: Is it correct that everything is marked as compile-time?
            CompileTimeChild child = new();
            
            var parentReference1 = (RunTimeParent)child;
            RunTimeParent parentReference2 = (RunTimeParent)child;
            RunTimeParent parentReference3 = child;

            var parentReference4 = child as RunTimeParent;
            RunTimeParent parentReference5 = child as RunTimeParent;

            if (child is RunTimeParent parentReference6)
            {
            }

            return proceed();
        }
    }
}
