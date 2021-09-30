class TargetCode
    {
        [Aspect]
        T Method<T,S>(T a, S b)
{
            return a;
}
    }
