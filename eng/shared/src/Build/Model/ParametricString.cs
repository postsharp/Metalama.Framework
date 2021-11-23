namespace PostSharp.Engineering.BuildTools.Build.Model
{
    
    public readonly struct ParametricString
    {
        private readonly string? _value;

        private ParametricString( string value )
        {
            this._value = value;
        }

        public override string ToString() => this._value ?? "<null>";

        public string ToString( VersionInfo parameters ) =>
            this._value?
            .Replace( "$(PackageVersion)", parameters.PackageVersion )
            .Replace( "$(Configuration)", parameters.Configuration ) ?? "";

        public static implicit operator ParametricString( string value ) => new ParametricString(value);

    }
}
