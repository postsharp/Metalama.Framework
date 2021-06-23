// <target>
[Retry]
class Program
{
    private void IntroducedMethod1(global::System.String name)
    {
        this.IntroducedMethod2("IntroducedMethod1");
    }

    private void IntroducedMethod2(global::System.String name)
    {
        this.IntroducedMethod1("IntroducedMethod2");
    }
}