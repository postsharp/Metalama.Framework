[Introduction]
internal class TargetClass
{


    public void IntroducedMethod_Void()
    {
        System.Console.WriteLine("This is introduced method.");
        Caravela.Framework.Aspects.__Void nic;
    }

    public int IntroducedMethod_Int()
    {
        System.Console.WriteLine("This is introduced method.");
        return default(int);
    }

    public int IntroducedMethod_Param(int x)
    {
        System.Console.WriteLine($"This is introduced method, x = {x}.");
        return default(int);
    }

    public static int IntroducedMethod_StaticSignature()
    {
        System.Console.WriteLine("This is introduced method.");
        return default(int);
    }

    public virtual int IntroducedMethod_VirtualExplicit()
    {
        System.Console.WriteLine("This is introduced method.");
        return default(int);
    }
}