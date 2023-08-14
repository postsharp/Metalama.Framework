internal class TargetCode
{
    [Aspect]
    void M()
    {
        this.M();
        this.M();
        this.M();
        return;
    }
}
