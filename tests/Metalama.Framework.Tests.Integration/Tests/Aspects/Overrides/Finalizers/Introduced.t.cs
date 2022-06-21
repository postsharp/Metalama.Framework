[Override]
internal class TargetClass
{


    ~TargetClass()
    {
        global::System.Console.WriteLine("This is the override.");
        global::System.Console.WriteLine("This is the introduction.");
        goto __aspect_return_1;

    __aspect_return_1: return;
    }
}