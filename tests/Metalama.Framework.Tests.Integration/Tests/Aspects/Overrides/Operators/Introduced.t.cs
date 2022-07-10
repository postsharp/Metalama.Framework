[Introduction]
[Override]
internal class TargetClass
{
    public static global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Operators.Introduced.TargetClass operator +(global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Operators.Introduced.TargetClass a, global::System.Int32 b)
    {
        global::System.Console.WriteLine("This is the override.");
        global::System.Console.WriteLine("This is the introduced operator.");
        return default(global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Operators.Introduced.TargetClass);
    }

    public static explicit operator global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Operators.Introduced.TargetClass(global::System.Int32 a)
    {
        global::System.Console.WriteLine("This is the override.");
        global::System.Console.WriteLine("This is the introduced operator.");
        return default(global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Operators.Introduced.TargetClass);
    }

    public static global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Operators.Introduced.TargetClass operator -(global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Operators.Introduced.TargetClass a)
    {
        global::System.Console.WriteLine("This is the override.");
        global::System.Console.WriteLine("This is the introduced operator.");
        return default(global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Operators.Introduced.TargetClass);
    }
}