using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.TestInputs.Highlighting.CastExpressions.RunTimeToRunTimeCast
{
    class Aspect
    {
        private class RunTimeParent
        {
        }

        private class RunTimeChild : RunTimeParent
        {
        }

        [TestTemplate]
        dynamic Template()
        {
            //TODO: The variables probably shouldn't be marked as compile-time variables.
            RunTimeChild child = new();
            
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
