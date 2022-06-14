[Aspect1]
[Aspect2]
internal class Foo
{
    public static void SourceMethod()
    {
    global::System.Console.WriteLine("Executing Aspect1 on SourceMethod. Methods present before applying Aspect1: IntroducedMethod1, IntroducedMethod2, SourceMethod");
        global::System.Console.WriteLine("Executing Aspect2 on SourceMethod. Methods present before applying Aspect2: IntroducedMethod2, SourceMethod");
            Console.WriteLine( "Method defined in source code." );
    goto __aspect_return_1;

__aspect_return_1:    return;
    }


public static void IntroducedMethod1()
{
    global::System.Console.WriteLine("Method introduced by Aspect1.");
}

public static void IntroducedMethod2()
{
    global::System.Console.WriteLine("Executing Aspect1 on IntroducedMethod2. Methods present before applying Aspect1: IntroducedMethod1, IntroducedMethod2, SourceMethod");
        global::System.Console.WriteLine("Method introduced by Aspect2.");
    return;
}}