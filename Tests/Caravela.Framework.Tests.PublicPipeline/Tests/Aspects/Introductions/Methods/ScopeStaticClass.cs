﻿using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Methods.ScopeStaticClass
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        [Introduce(Scope = IntroductionScope.Default)]
        public int DefaultScope()
        {
            Console.WriteLine("This is introduced method.");
            return 42;
        }

        [Introduce(Scope = IntroductionScope.Default)]
        public static int DefaultScopeStatic()
        {
            Console.WriteLine("This is introduced method.");
            return 42;
        }

        [Introduce(Scope = IntroductionScope.Static)]
        public int StaticScope()
        {
            Console.WriteLine("This is introduced method.");
            return 42;
        }

        [Introduce(Scope = IntroductionScope.Static)]
        public static int StaticScopeStatic()
        {
            Console.WriteLine("This is introduced method.");
            return 42;
        }

        [Introduce(Scope = IntroductionScope.Target)]
        public int TargetScope()
        {
            Console.WriteLine("This is introduced method.");
            return 42;
        }

        [Introduce(Scope = IntroductionScope.Target)]
        public static int TargetScopeStatic()
        {
            Console.WriteLine("This is introduced method.");
            return 42;
        }
    }

    // <target>
    [Introduction]
    internal static class TargetClass
    {
    }
}
