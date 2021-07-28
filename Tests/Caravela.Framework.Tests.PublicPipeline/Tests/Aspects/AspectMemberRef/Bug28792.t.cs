[RegistryStorage("Animals")]
class Animals
{


    private int _turtles;
    public int Turtles {get    {
            var value = global::Microsoft.Win32.Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Company\\Product\\Animals", "Turtles", null);
            if (value != null)
            {
                return (int)global::System.Convert.ChangeType(value, typeof(global::System.Int32));
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