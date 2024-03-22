using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.CompileTimeTypeSerialization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
[Inheritable]
public sealed class ListImplementedTypesAttribute : TypeAspect
{
    public ListImplementedTypesAttribute(Type type)
    {
        this.Type = type;
    }

    public Type Type { get; }

    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        var types = new[] { Type }.Concat(builder.AspectInstance.SecondaryInstances.Select(i => ((ListImplementedTypesAttribute)i.Aspect).Type));

        var sb = new StringBuilder();
        foreach (var type in types)
        {
            sb.Append($"{type}: {(builder.Target.Is(type) ? "" : "not ")}implemented; ");
        }

        builder.Advice.IntroduceField(builder.Target, "types", typeof(string), buildField: field => field.InitializerExpression = TypedConstant.Create(sb.ToString()));
    }
}

// <target>
[ListImplementedTypes(typeof(ICloneable))]
[ListImplementedTypes(typeof(ISomeInterface))]
[ListImplementedTypes(typeof(ISomeInterface.INested))]
[ListImplementedTypes(typeof(ISomeInterface[]))]
[ListImplementedTypes(typeof(List<ISomeInterface>))]
[ListImplementedTypes(typeof(ISomeInterface<int>))]
[ListImplementedTypes(typeof(ISomeInterface<DateTime>))]
[ListImplementedTypes(typeof(ISomeInterface<Type>))]
[ListImplementedTypes(typeof(E))]
public abstract class BaseClass : ISomeInterface
{
}

public interface ISomeInterface
{
    public interface INested { }
}

public interface ISomeInterface<T>
{
}

public enum E { }

// <target>
public sealed class TestClass : BaseClass, ICloneable
{
    public object Clone() => throw new NotImplementedException();
}
