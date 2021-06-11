[TestOutput]
[Retry]
class Program
{
    private void IntroducedMethod1(string name)
    {
        this.IntroducedProperty = name;
    }

    private global::System.String IntroducedProperty
    {
        get
        {
            return (string)"Program";
        }

        set
        {
        }
    }
}