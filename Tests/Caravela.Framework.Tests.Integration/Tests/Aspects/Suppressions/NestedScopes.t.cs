// Warning CS0219 on `y`: `The variable 'y' is assigned but its value is never used`
internal class TargetClass
{
#pragma warning disable CS0219
    [SuppressWarning]
    private void M2(string m)
    {
        this.__Override__M2__By__Caravela_Framework_Tests_Integration_Aspects_Suppressions_NestedScopes_SuppressWarningAttribute(m);
    }
#pragma warning restore CS0219
#pragma warning disable CS0219


    private void __Override__M2__By__Caravela_Framework_Tests_Integration_Aspects_Suppressions_NestedScopes_SuppressWarningAttribute(global::System.String m)
    {
        int a = 0;
        int x = 0;



        int y = 0;
        return;
    }
#pragma warning restore CS0219
        
        
    private void M1( string m ) 
    {
#pragma warning disable CS0219
        int x = 0;
#pragma warning restore CS0219

#pragma warning disable CS0219, 219


        // CS0219 expected 
        int y = 0;
    }
}