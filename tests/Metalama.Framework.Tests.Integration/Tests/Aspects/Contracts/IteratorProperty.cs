namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Contracts.IteratorProperty;

using System;
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

        foreach (var property in builder.Target.Properties)
        {
            builder.Advice.AddContract(
                property,
                nameof(ValidateParameter), 
                direction: ContractDirection.Input);

            // #32616
            //builder.Advice.AddContract(
            //    property,
            //    nameof(ValidateParameter),
            //    direction: ContractDirection.Output);
        }
    }

    [Template]
    private void ValidateParameter( dynamic? value )
    {
        if (value is null)
        {
            throw new ArgumentNullException();
        }
    }
}

// <target>
[Test]
public class TestClass
{
    public IEnumerable<string> Enumerable
    {
        get
        {
            yield return "Hello";
        }
        set
        {
        }
    }

    public IEnumerator<string> Enumerator
    {
        get
        {
            yield return "Hello";
        }
        set
        {
        }
    }
}