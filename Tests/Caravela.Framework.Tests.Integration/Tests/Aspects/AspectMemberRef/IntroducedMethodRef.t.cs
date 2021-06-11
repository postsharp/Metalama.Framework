[TestOutput]
[Retry]
class Program
{
    private void IntroducedMethod1(string name)
    {
        this.IntroducedMethod2("IntroducedMethod1");
    }

    private void IntroducedMethod2(string name)
    {
        this.IntroducedMethod1("IntroducedMethod2");
    }
}