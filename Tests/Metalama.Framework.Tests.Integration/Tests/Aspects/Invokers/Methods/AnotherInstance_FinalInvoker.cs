﻿using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

#pragma warning disable CS0067

namespace Metalama.Framework.IntegrationTests.Aspects.Invokers.Events.AnotherInstance_FinalInvoker
{
    [AttributeUsage( AttributeTargets.Class )]
    public class TestAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> aspectBuilder )
        {
            var overrideBuilder = aspectBuilder.Advices.IntroduceMethod( aspectBuilder.Target, nameof(OverrideMethod), whenExists: OverrideStrategy.Override );
            overrideBuilder.Name = "Foo";
            overrideBuilder.ReturnType = aspectBuilder.Target.Compilation.TypeFactory.GetSpecialType( SpecialType.Void );
        }

        [Template]
        public dynamic? OverrideMethod()
        {
            var x = meta.This;

            return meta.Target.Method.Invokers.Final.Invoke( x );
        }
    }

    // <target>
    [TestAttribute]
    internal class TargetClass
    {
        public void Foo() { }
    }
}