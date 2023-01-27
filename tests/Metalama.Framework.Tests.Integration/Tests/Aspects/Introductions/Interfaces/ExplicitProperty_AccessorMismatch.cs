﻿using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

#pragma warning disable CS0067

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ExplicitProperty_AccessorMismatch
{
    /*
     * Error when accessors of explicit property interface member don't match the interface.
     */

    public interface IInterface
    {
        int TemplateWithGet { set; }
        int TemplateWithSet { get; }
        int TemplateWithInit { set; }
        int TemplateWithoutInit { init; }
    }

    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> aspectBuilder )
        {
            aspectBuilder.Advice.ImplementInterface( aspectBuilder.Target, typeof(IInterface) );
        }

        [InterfaceMember( IsExplicit = true )]
        int TemplateWithGet 
        { 
            get
            {
                return 42;
            }
            set
            {
            }
        }

        [InterfaceMember(IsExplicit = true)]
        public int TemplateWithSet
        {
            get
            {
                return 42;
            }
            set
            {
            }
        }

        [InterfaceMember(IsExplicit = true)]
        int TemplateWithInit
        {
            init
            {
            }
        }

        [InterfaceMember(IsExplicit = true)]
        int TemplateWithoutInit
        {
            set
            {
            }
        }
    }

    // <target>
    [Introduction]
    public class TargetClass { }
}