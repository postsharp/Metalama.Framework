[Introduction]
internal class TargetClass<T>
{

    public static global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Operators.Target_GenericType.TargetClass<T> operator +(global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Operators.Target_GenericType.TargetClass<T> a, global::System.Int32 b)
    {
        global::System.Console.WriteLine("This is the introduced operator.");
        return default(global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Operators.Target_GenericType.TargetClass<T>);
    }

    public static explicit operator global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Operators.Target_GenericType.TargetClass<T>(global::System.Int32 a)
    {
        global::System.Console.WriteLine("This is the introduced operator.");
        return default(global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Operators.Target_GenericType.TargetClass<T>);
    }

    public static global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Operators.Target_GenericType.TargetClass<T> operator -(global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Operators.Target_GenericType.TargetClass<T> a)
    {
        global::System.Console.WriteLine("This is the introduced operator.");
        return default(global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Operators.Target_GenericType.TargetClass<T>);
    }
}