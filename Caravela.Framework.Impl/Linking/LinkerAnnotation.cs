namespace Caravela.Framework.Impl.Linking
{
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
    public record LinkerAnnotation( string? AspectTypeName, string? PartName, string? Order, string? Verb )
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
    {
        public override string ToString()
        {
            return $"{this.AspectTypeName}${this.PartName}${this.Order}${this.Verb}";
        }

        public static LinkerAnnotation FromString( string str )
        {
            var parts = str.Split( '$' );

            return new LinkerAnnotation(
                parts[0] == "null" ? null : parts[0],
                parts[1] == "null" ? null : parts[1],
                parts[2] == "null" ? null : parts[2],
                parts[3] == "null" ? null : parts[3] );
        }
    }
}
