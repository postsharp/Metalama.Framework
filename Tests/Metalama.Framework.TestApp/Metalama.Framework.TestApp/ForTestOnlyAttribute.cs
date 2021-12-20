using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Validation;

namespace Metalama.Framework.TestApp
{
    internal class ForTestOnlyAttribute : Attribute, IAspect<IDeclaration>
    {
        static DiagnosticDefinition<IDeclaration> _error = new ("ERROR", Severity.Error, "'{0}' can only be used in test projects.");

        public void BuildAspect( IAspectBuilder<IDeclaration> builder )
        {
            builder.WithTarget().RegisterReferenceValidator(nameof(this.Validate), Validation.ReferenceKinds.All);

        }

        private void Validate( in ReferenceValidationContext context )
        {
            if ( !context.ReferencingType.DeclaringAssembly.Identity.Name.Contains("UnitTest"))
            {
                _error.WithArguments(context.ReferencedDeclaration).ReportTo(context.Diagnostics);
            }
        }
    }
}
