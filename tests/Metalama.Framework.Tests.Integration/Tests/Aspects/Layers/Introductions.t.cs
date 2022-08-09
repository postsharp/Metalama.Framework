[MyAspect]
internal class C
{
    public void InSourceCode()
    {
        global::System.Console.WriteLine("Overridden in Layer Second");
        global::System.Console.WriteLine("Overridden in Layer First");
        goto __aspect_return_1;

    __aspect_return_1: return;
    }


    public void IntroducedInFirstLayer()
    {
        global::System.Console.WriteLine("Overridden in Layer Second");
        return;
    }

    public void IntroducedInSecondLayer()
    {
    }
}