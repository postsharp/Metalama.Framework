using System;
using System.Collections.Generic;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Validation;

#pragma warning disable CS0168, CS8618, CS0169

namespace Metalama.Framework.Tests.Integration.Validation.AllReferences
{
    internal class Aspect : TypeAspect
    {
        private static readonly DiagnosticDefinition<(ReferenceKinds ReferenceKinds, IDeclaration Declaration, string SyntaxKind)> _warning =
            new( "MY001", Severity.Warning, "Reference constraint of type '{0}' in declaration '{1}' (SyntaxKind={2})." );

        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.WithTarget().RegisterReferenceValidator( Validate, ReferenceKinds.All );
        }

        private static void Validate( in ReferenceValidationContext context )
        {
        
            context.Diagnostics.Report( _warning.WithArguments( ( context.ReferenceKinds, context.ReferencingDeclaration, context.Syntax.Kind ) ) );
        }
    }

    [Aspect]
    internal class ValidatedClass
    {
        public static void Method( object o ) { }
        
        public static int StaticField;
        public int InstanceField;
        
        
    }

    // <target>
    internal class DerivedClass : ValidatedClass
    {
        // Field type.
        private ValidatedClass _field1;

        // Typeof in field initializer.
        private Type _field2 = typeof(ValidatedClass);

        private ValidatedClass? Method( ValidatedClass[] param1, List<ValidatedClass> param2 )
        {
            ValidatedClass variable = new();
            var x = new ValidatedClass();
            _ = x.InstanceField;
            x.InstanceField = 5;
            x.InstanceField += 5;
            ValidatedClass.StaticField = 5;
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