class TargetCode
{
    class Nullable
    {
        private global::System.String? _field = null;
        [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Nullable.DynamicParameterContract.Aspect]
        public global::System.String? Field
        {
            get
            {
                return this._field;
            }
            set
            {
                value?.ToString();
                value!.ToString();
                this._field = value;
            }
        }
        private string? _property;
        [Aspect]
        public string? Property
        {
            get
            {
                return this._property;
            }
            set
            {
                value?.ToString();
                value!.ToString();
                this._property = value;
            }
        }
        [Aspect]
        public string? this[int i]
        {
            get
            {
                global::System.String? returnValue;
                returnValue = null;
                returnValue?.ToString();
                returnValue!.ToString();
                return returnValue;
            }
        }
        [return: Aspect]
        string? Method([Aspect] string? arg)
        {
            global::System.String? returnValue;
            arg?.ToString();
            arg!.ToString();
            returnValue = arg;
            returnValue?.ToString();
            returnValue!.ToString();
            return returnValue;
        }
    }
    class NotNullable
    {
        private global::System.String _field = null!;
        [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Nullable.DynamicParameterContract.Aspect]
        public global::System.String Field
        {
            get
            {
                return this._field;
            }
            set
            {
                value.ToString();
                value.ToString();
                this._field = value;
            }
        }
        private string _property = null!;
        [Aspect]
        public string Property
        {
            get
            {
                return this._property;
            }
            set
            {
                value.ToString();
                value.ToString();
                this._property = value;
            }
        }
        [Aspect]
        public string this[int i]
        {
            get
            {
                global::System.String returnValue;
                returnValue = null!;
                returnValue.ToString();
                returnValue.ToString();
                return returnValue;
            }
        }
        [return: Aspect]
        string Method([Aspect] string arg)
        {
            global::System.String returnValue;
            arg.ToString();
            arg.ToString();
            returnValue = arg;
            returnValue.ToString();
            returnValue.ToString();
            return returnValue;
        }
    }
#nullable disable
    class Oblivious
    {
        private global::System.String _field = null;
        [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Nullable.DynamicParameterContract.Aspect]
        public global::System.String Field
        {
            get
            {
                return this._field;
            }
            set
            {
                value?.ToString();
                value.ToString();
                this._field = value;
            }
        }
        private string _property;
        [Aspect]
        public string Property
        {
            get
            {
                return this._property;
            }
            set
            {
                value?.ToString();
                value.ToString();
                this._property = value;
            }
        }
        [Aspect]
        public string this[int i]
        {
            get
            {
                global::System.String returnValue;
                returnValue = null;
                returnValue?.ToString();
                returnValue.ToString();
                return returnValue;
            }
        }
        [return: Aspect]
        string Method([Aspect] string arg)
        {
            global::System.String returnValue;
            arg?.ToString();
            arg.ToString();
            returnValue = arg;
            returnValue?.ToString();
            returnValue.ToString();
            return returnValue;
        }
    }
}
