#if TEST_OPTIONS
// @RemoveOutputCode
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Validation;

#pragma warning disable CS0168, CS8618, CS0169, CS0219, CS0067

namespace Metalama.Framework.Tests.Integration.Validation.AllReferences
{
    internal class Aspect : TypeAspect
    {
        private static readonly DiagnosticDefinition<(ReferenceKinds ReferenceKinds, DeclarationKind ReferencingDeclarationKind, IDeclaration
            ReferencingDeclaration, DeclarationKind ReferencedDeclarationKind, IDeclaration ReferencedDeclaration, string SyntaxKind)> _warning =
            new( "MY001", Severity.Warning, "Reference constraint of type '{0}' to {3} '{4}' from {1} '{2}' (SyntaxKind={5})." );

        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder
                .Outbound
                .ValidateOutboundReferences( Validate, ReferenceGranularity.Member, ReferenceKinds.All );
        }

        private static void Validate( ReferenceValidationContext context )
        {
            context.Diagnostics.Report(
                x =>
                    _warning.WithArguments(
                        ( x.ReferenceKind, x.ReferencingDeclaration.DeclarationKind, x.ReferencingDeclaration,
                          context.Destination.Declaration.DeclarationKind, context.Destination.Declaration, x.Source.Kind ) ) );
        }
    }

    [Aspect]
    internal class ValidatedClass : Attribute
    {
        public static void Method( object o ) { }

        public virtual void VirtualMethod() { }

        public static int StaticField;
        public int InstanceField;

        public ValidatedClass() { }

        public ValidatedClass( int x ) { }
        
        public virtual int Property { get; set; }

        public virtual event Action TheEvent
        {
            add { }
            remove { }
        }
    }

    [Aspect]
    internal class ValidatedGenericClass<T> { }

    [Aspect]
    internal delegate void ValidatedDelegate();

    [Aspect]
    internal interface IValidatedInterface
    {
        void InterfaceMethod();
        int InterfaceProperty { get; set; }

        event Action InterfaceEvent;
    }

    internal class ExplicitInterfaceImplementation : IValidatedInterface
    {
        void IValidatedInterface.InterfaceMethod() { }

        int IValidatedInterface.InterfaceProperty { get; set; }

        event Action IValidatedInterface.InterfaceEvent
        {
            add { }
            remove { }
        }
    }

    // <target>
    [ValidatedClass]
    internal class DerivedClass : ValidatedClass
    {
        // Attribute on field.
        private int _f;

        // Field type.
        private ValidatedClass? _field1;

        // Typeof in field initializer.
        private Type _field2 = typeof(ValidatedClass);
        
        // Constructors
        public DerivedClass() { }

        public DerivedClass( int x ) : base( 5 ) { }

        // Override method.
        public override void VirtualMethod()
        {
            // Base method call.
            base.VirtualMethod();
        }

     
        // Parameters, return values.
        private ValidatedClass? Method( ValidatedClass[] param1, List<ValidatedClass> param2 )
        {
            // ObjectCreation
            ValidatedClass variable = new();
            var x = new ValidatedClass();
            
            // Default access
            _ = x.InstanceField;
            
            // Assignments
            x.InstanceField = 5;
            x.InstanceField += 5;
            StaticField = 5;
            
            // Invoke, typeof
            Method( typeof(ValidatedClass) );

            // nameof
            var y = nameof(ValidatedClass);
            var z = nameof(InstanceField);

            // Invoke event.
            this.FieldLikeEvent();
            this.FieldLikeEvent.Invoke(); // This is a normal access.
            
            // Event assignment
            this.TheEvent += () => { };
            this.TheEvent -= () => { };
            this.FieldLikeEvent += () => { };
            this.FieldLikeEvent -= () => { };

            return null;
        }

        // Automatic property.
        public ValidatedClass? AutomaticProperty { get; set; }
        
        // Override property.
        public override int Property
        {
            // Base property read.
            get => base.Property;
            
            // Base property assignment.
            set => base.Property = value;
        }




        // Events
        public event ValidatedDelegate? FieldLikeEvent;

        public event ValidatedDelegate ExplicitEvent
        {
            add { }
            remove { }
        }
        
        public override event Action TheEvent
        {
            add
            {
                base.TheEvent += value; }
            remove
            {
                base.TheEvent += value;
            }
        }
    }

    internal class ReferencingClass
    {
        private void ReferencingMethod()
        {
            // Local variable.
            ValidatedClass variable;

            // Typeof.
            ValidatedClass.Method( typeof(ValidatedClass) );

            // Type argument of generic type.
            List<ValidatedClass> list = new();

            // Type argument of generic method.
            _ = new object[0].OfType<ValidatedClass>();
        }
    }

    internal class GenericDerivedClass : ValidatedGenericClass<int> { }

    internal class ListOfValidated : List<ValidatedClass> { }

    [ValidatedClass]
    internal class AttributeTargets
    {
        [ValidatedClass]
        private int _field;

        [ValidatedClass]
        public int Property
        {
            [ValidatedClass]
            get;
            [ValidatedClass]
            set;
        }

        [ValidatedClass]
        [return: ValidatedClass]
        public int Method( [ValidatedClass] int p ) => p;
    }
}