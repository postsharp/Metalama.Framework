[Introduction]
internal class TargetClass
{

    public static global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Operators.Programmatic.TargetClass operator +(global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Operators.Programmatic.TargetClass a, global::System.Int32 b)
    {
        global::System.Console.WriteLine("This is the introduced operator.");
        return default(global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Operators.Programmatic.TargetClass);
    }

    public static explicit operator global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Operators.Programmatic.TargetClass(global::System.Int32 a)
    {
        global::System.Console.WriteLine("This is the introduced operator.");
        return default(global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Operators.Programmatic.TargetClass);
    }

    public static global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Operators.Programmatic.TargetClass operator -(global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Operators.Programmatic.TargetClass a)
    {
        global::System.Console.WriteLine("This is the introduced operator.");
        return default(global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Operators.Programmatic.TargetClass);
    }
}