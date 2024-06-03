﻿using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;

#pragma warning disable CS0067

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.Accessibility_CrossAssembly
{
    public interface IInterface
    {
        void Method();
        int Property { get; set; }
        int Property_PrivateSetter { get; }
        int Property_GetOnly { get; }
        int Property_ExpressionBody { get; }
        int AutoProperty { get; set; }
        int AutoProperty_PrivateSetter { get; set; }
        event EventHandler? EventField;
        event EventHandler? Event;
    }

    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> aspectBuilder)
        {
            aspectBuilder.Advice.ImplementInterface(aspectBuilder.Target, typeof(IInterface));
        }

        [InterfaceMember(IsExplicit = false)]
        private void Method()
        {
            Console.WriteLine("Introduced interface member");
        }

        [InterfaceMember(IsExplicit = false)]
        private int Property
        {
            get
            {
                return 42;
            }

            set
            {
            }
        }

        [InterfaceMember(IsExplicit = false)]
        public int Property_PrivateSetter
        {
            get
            {
                return 42;
            }

            private set
            {
            }
        }

        [InterfaceMember(IsExplicit = false)]
        private int Property_GetOnly
        {
            get
            {
                return 42;
            }
        }

        [InterfaceMember(IsExplicit = false)]
        private int Property_ExpressionBody => 42;

        [InterfaceMember(IsExplicit = false)]
        private int AutoProperty { get; set; }

        [InterfaceMember(IsExplicit = false)]
        public int AutoProperty_PrivateSetter { get; private set; }

        [InterfaceMember(IsExplicit = false)]
        private event EventHandler? EventField;

        [InterfaceMember(IsExplicit = false)]
        private event EventHandler? Event
        {
            add { }
            remove { }
        }
    }
}