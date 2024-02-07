#if TEST_OPTIONS
// @DesignTime
#endif

using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.DesignTime.IntroduceParameter
{
    public class Introduction1Attribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            foreach(var constructor in builder.Target.Constructors)
            {
                builder.Advice.IntroduceParameter(constructor, "introduced1", typeof(int), TypedConstant.Create(42));
            }
        }
    }
    public class Introduction2Attribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            foreach (var constructor in builder.Target.Constructors)
            {
                builder.Advice.IntroduceParameter(constructor, "introduced2", typeof(string), TypedConstant.Create("42"));
            }
        }
    }

    // <target>
    [Introduction1]
    [Introduction2]
    internal partial class ClassWithImplicitParameterless
    {
        public void Foo()
        {
            _ = new ClassWithImplicitParameterless();
        }
    }

    // <target>
    [Introduction1]
    [Introduction2]
    internal partial class ClassWithExplicitParameterless
    {
        public ClassWithExplicitParameterless()
        {
        }

        public void Foo()
        {
            _ = new ClassWithExplicitParameterless();
        }
    }

    // <target>
    [Introduction1]
    [Introduction2]
    internal partial class ClassWithParameters
    {
        public ClassWithParameters(int param)
        {
        }

        public void Foo()
        {
            _ = new ClassWithParameters(42);
        }
    }

    // <target>
    [Introduction1]
    [Introduction2]
    internal partial class ClassWithOptionalParameters
    {
        public ClassWithOptionalParameters(int param, int optParam = 42)
        {
        }

        public void Foo()
        {
            _ = new ClassWithOptionalParameters(42);
            _ = new ClassWithOptionalParameters(param: 42);
            _ = new ClassWithOptionalParameters(42, 42);
            _ = new ClassWithOptionalParameters(42, optParam: 42);
            _ = new ClassWithOptionalParameters(optParam: 42, param: 13);
        }
    }

    // <target>
    [Introduction1]
    [Introduction2]
    internal partial class ClassWithOptionalAndNonOptionalParameters
    {
        public ClassWithOptionalAndNonOptionalParameters(int param)
        {
        }

        public ClassWithOptionalAndNonOptionalParameters(int param, int optParam = 42)
        {
        }

        public void Foo()
        {
            _ = new ClassWithOptionalAndNonOptionalParameters(42);
            _ = new ClassWithOptionalAndNonOptionalParameters(param: 42);
            _ = new ClassWithOptionalAndNonOptionalParameters(42, 42);
            _ = new ClassWithOptionalAndNonOptionalParameters(42, optParam: 42);
            _ = new ClassWithOptionalAndNonOptionalParameters(optParam: 42, param: 13);
        }
    }
}