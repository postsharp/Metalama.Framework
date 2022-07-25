using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Fabrics;
#pragma warning disable CS0067, CS8618, CA1822, CS0162, CS0169, CS0414

internal class Fabric : ProjectFabric
{
    public override void AmendProject(IProjectAmender amender) => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");


    private bool IsTypeEligible(INamedType type)
    {
        return type.Is(typeof(TestClass));
    }

    private bool IsPropertyEligible(IProperty property)
    {
        return true;
    }
}

#pragma warning restore CS0067, CS8618, CA1822, CS0162, CS0169, CS0414
#pragma warning disable CS0067, CS8618, CA1822, CS0162, CS0169, CS0414

public class PropOverride : OverrideFieldOrPropertyAspect
{
    public override dynamic? OverrideProperty { get => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time."); set => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time."); }

}

#pragma warning restore CS0067, CS8618, CA1822, CS0162, CS0169, CS0414

public class TestClass
{
    public string Prop132
    {
        get
        {
            return (global::System.String)this.Prop132_Source?.ToUpper();

        }
        set
        {
            if (1 == 1)
            {
                this.Prop132_Source = value;
            }

            if (1 == 1)
            {
                this.Prop132_Source = value;
            }

            if (1 == 1)
            {
                // x
            }

            if (1 == 1)
            {
                this.Prop132_Source = value;
            }

        }
    }
    private string Prop132_Source { get; set; } = ""
}

internal class Program
{
    private static void Main()
    {
        var widget = new TestClass();

        widget.Prop132 = "aaa";

        Console.WriteLine(widget.Prop132);
    }
}