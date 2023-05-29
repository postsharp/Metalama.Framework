namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Contracts.Iterator;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

public sealed class TestAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        base.BuildAspect( builder );

        foreach (var method in builder.Target.Methods)
        {
            foreach (var parameter in method.Parameters)
            {
                builder.Advice.AddContract(
                    parameter,
                    nameof(ValidateParameter),
                    args: new { parameterName = parameter.Name } );
            }
        }
    }

    [Template]
    private void ValidateParameter( dynamic? value, [CompileTime] string parameterName )
    {
        if (value is null)
        {
            throw new ArgumentNullException( parameterName );
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

    public IEnumerator<string> EnumeratorT(string text)
    {
        yield return "Hello";
        yield return text;
    }
}