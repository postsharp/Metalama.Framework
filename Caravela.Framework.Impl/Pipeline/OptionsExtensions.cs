namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// Extension methods for <see cref="IConfigOptions"/>.
    /// </summary>
    public static class OptionsExtensions
    {
        public static bool GetBooleanOption( this IConfigOptions options, string name, bool defaultValue = false )
        {
            if ( options.TryGetValue( $"build_property.{name}", out var flagString ) && bool.TryParse( flagString, out var flagValue ) )
            {
                return flagValue;
            }
            else
            {
                return defaultValue;
            }
        }
    }
}