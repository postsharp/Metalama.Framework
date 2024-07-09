using System;
using System.Collections.Generic;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Fabrics;
using Metalama.Framework.Validation;

#pragma warning disable CS0168, CS8618, CS0169

namespace Metalama.Framework.Tests.Integration.Validation.TypeFabric_
{
    internal class ValidatedClass
    {
        public static void Method( object o ) { }

        private class Fabric : TypeFabric
        {
            private static readonly DiagnosticDefinition<(ReferenceKinds ReferenceKinds, IDeclaration Declaration)> _warning =
                new( "MY001", Severity.Warning, "Reference constraint of type '{0}' in declaration '{1}'." );

            public override void AmendType( ITypeAmender amender )
            {
                amender.ValidateInboundReferences( Validate, ReferenceGranularity.ParameterOrAttribute, ReferenceKinds.All );
            }

            private static void Validate( ReferenceValidationContext context )
            {
                context.Diagnostics.Report( r => _warning.WithArguments( ( r.ReferenceKind, r.OriginDeclaration ) ) );
            }
        }
    }

    internal class DerivedClass : ValidatedClass
    {
        // Field type.
        private ValidatedClass? _field1;

        // Typeof in field initializer.
        private Type _field2 = typeof(ValidatedClass);

        private ValidatedClass? Method( ValidatedClass[] param1, List<ValidatedClass> param2 )
        {
            ValidatedClass variable;
            Method( typeof(ValidatedClass) );

            return null;
        }
    }

    internal class ReferencingClass
    {
        private void ReferencingMethod()
        {
            ValidatedClass variable;
            ValidatedClass.Method( typeof(ValidatedClass) );
        }
    }
}