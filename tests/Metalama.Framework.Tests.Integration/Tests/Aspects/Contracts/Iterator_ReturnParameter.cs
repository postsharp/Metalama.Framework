namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Contracts.Iterator_ReturnParameter;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;

public sealed class TestAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        base.BuildAspect( builder );

        foreach (var method in builder.Target.Methods)
        {
            builder.Advice.AddContract(
                method.ReturnParameter,
                nameof(ValidateParameter));
        }
    }

    [Template]
    private void ValidateParameter(dynamic? value)
    {
        if (meta.Target.Parameter.Type.Is(SpecialType.IEnumerable)
            || meta.Target.Parameter.Type.Is(TypeFactory.GetType(SpecialType.IEnumerable_T).WithTypeArguments(TypeFactory.GetType(SpecialType.String))))
        {
            foreach (var item in value!)
            {
                if (item is null)
                {
                    throw new ArgumentNullException("<return>");
                }
            }
        }
        else
        {
            while (value!.MoveNext())
            {
                if (value.Current is null)
                {
                    throw new ArgumentNullException("<return>");
                }
            }
        }
    }
}

// <target>
[Test]
public class TestClass
{
    public IEnumerable? Enumerable(string text)
    {
        yield return "Hello";
        yield return text;
    }

    public IEnumerator Enumerator(string text)
    {
        yield return "Hello";
        yield return text;
    }

    public IEnumerable<string> EnumerableT( string text )
    {
        yield return "Hello";
        yield return text;
    }

    public IEnumerator<string> EnumeratorT( string text )
    {
        yield return "Hello";
        yield return text;
    }
}