[Introduction]
internal class TargetClass
{
    public static TargetClass operator -(TargetClass a)
    {
        global::System.Console.WriteLine("This is the introduced operator.");
        Console.WriteLine("This is the original operator.");
        return new TargetClass();
    }

    public static TargetClass operator +(TargetClass a, TargetClass b)
    {
        global::System.Console.WriteLine("This is the introduced operator.");
        Console.WriteLine("This is the original operator.");
        return new TargetClass();
    }

    public static explicit operator TargetClass(int a)
    {
        global::System.Console.WriteLine("This is the introduced operator.");
        Console.WriteLine("This is the original operator.");
        return new TargetClass();
    }
}