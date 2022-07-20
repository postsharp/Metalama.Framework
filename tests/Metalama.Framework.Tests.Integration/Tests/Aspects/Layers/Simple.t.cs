internal class C
{
    [MyAspect]
    public void M()
    {
        global::System.Console.WriteLine("Layer: Second");
        global::System.Console.WriteLine("Layer: ");
        goto __aspect_return_1;

    __aspect_return_1: return;

    }
}