[MyAspect]
internal class C
{
    int _instanceField = 5;
    private static int _staticField = 6;


    public void Method()
    {
        this._instanceField = this._instanceField;
        global::Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.FieldOrPropertyValueShortcuts.C._staticField = global::Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.FieldOrPropertyValueShortcuts.C._staticField;
    }
}