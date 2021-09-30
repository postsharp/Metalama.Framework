﻿using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Caravela.TestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Caravela.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty
{
    public class DirtyAttribute : Attribute, IAspect<INamedType>
    {
        static readonly DiagnosticDefinition<INamedType> _mustHaveDirtyStateSetter = new
            ("MY001",
            Severity.Error,
            "The 'IDirty' interface is implemented manually on type '{0}', but the property 'DirtyState' does not have a property setter.");

        static readonly DiagnosticDefinition<IProperty> _dirtyStateSetterMustBeProtected = new
            ("MY002",
            Severity.Error,
            "The setter of the '{0}' property must be have the 'protected' accessibility.");

        public void BuildAspect(IAspectBuilder<INamedType> builder)
        {

            if (!builder.Target.ImplementedInterfaces.Any(i => i.Is(typeof(IDirty))))
            {
                builder.Advices.ImplementInterface(builder.Target, typeof(IDirty));
            }
            else
            {
                // If the type already implements IDirty, it must have a protected method called OnDirty, otherwise 
                // this is a contract violation.
                var dirtyStateProperty = builder.Target.Properties.Where(m => m.Name == nameof(this.DirtyState) && m.Parameters.Count == 0 && m.Type.Is(typeof(DirtyState))).SingleOrDefault();

                if (dirtyStateProperty?.SetMethod == null)
                {
                    builder.Diagnostics.Report(_mustHaveDirtyStateSetter, builder.Target);
                }
                else if (dirtyStateProperty.SetMethod.Accessibility != Accessibility.Protected)
                {
                    builder.Diagnostics.Report(_dirtyStateSetterMustBeProtected, dirtyStateProperty);
                }
            }

            var fieldsOrProperties = builder.Target.Properties
                .Cast<IFieldOrProperty>()
                .Concat(builder.Target.Fields)
                .Where(f => f.Writeability == Writeability.All);

            foreach (var fieldOrProperty in fieldsOrProperties)
            {
                builder.Advices.OverrideFieldOrPropertyAccessors(fieldOrProperty, null, nameof(OverrideSetter));
            }

            // TODO: This aspect is not complete. We should normally not set DirtyState to Clean after the object has been initialized,
            // but this is not possible in the current version of Caravela.

        }

        [InterfaceMember]
        public DirtyState DirtyState { get; protected set; }

        [Template]
        private void OverrideSetter()
        {
            // TODO: this syntax is ugly and it will be fixed.
            meta.Proceed();

            if (meta.This.DirtyState == DirtyState.Clean)
            {
                meta.This.DirtyState = DirtyState.Dirty;
            }
        }
    }

    public interface IDirty
    {
        DirtyState DirtyState { get; }
    }

    public enum DirtyState
    {
        Clean,
        Dirty
    }

    // <target>
    [Dirty]
    public class TargetClass
    {
        public int A { get; set; }
    }
}
