// Final Compilation.Emit failed. 

// Error CS0127 on `return`: `Since 'LocalFunction()' returns void, a return keyword must not be followed by an object expression`

internal class C
{
    [TheAspect]
    private int M()
    {
        void LocalFunction()
        {
            _ = 5;
        }

        LocalFunction();
        return default;
    }
}
