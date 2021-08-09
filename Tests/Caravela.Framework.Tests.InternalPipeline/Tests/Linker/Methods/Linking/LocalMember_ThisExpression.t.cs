class Target
{
    void Bar()
    {
        // Should invoke original code.
        this.__Bar__OriginalImpl();
        // Should invoke override 2.
        this.Bar_Override2();
        // Should invoke the final declaration.
        this.Bar();
        // Should invoke the final declaration.
        this.Bar();
    }

    private void __Bar__OriginalImpl()
    {
    }


    void Bar_Override1()
    {
        // Should invoke original code.
        this.__Bar__OriginalImpl();
        // Should invoke original code.
        this.__Bar__OriginalImpl();
        // Should invoke override 1.
        this.Bar_Override1();
        // Should invoke the final declaration.
        this.Bar();
    }

    void Bar_Override2()
    {
        // Should invoke original code.
        this.__Bar__OriginalImpl();
        // Should invoke override 1.
        this.Bar_Override1();
        // Should invoke override 2.
        this.Bar_Override2();
        // Should invoke the final declaration.
        this.Bar();
    }
}