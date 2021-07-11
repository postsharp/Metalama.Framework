[RegistryStorage("Animals")]
    class Animals
    {
private int _turtles;
        public int Turtles {get    {
        var type = global::System.Type.GetTypeFromHandle(global::Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle("T:System.Int32"));
        var value = global::Microsoft.Win32.Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Company\\Product\\Animals", "Turtles", null);
        if (value != null)
        {
            return (int)global::System.Convert.ChangeType(value, type);
        }
        else
        {
            return (int)(global::System.Int32)(0);
        }
    }

set    {
this._turtles=value;    }
}
    }