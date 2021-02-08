namespace Caravela.Framework.Impl.Linking
{
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
    public record LinkerAnnotation( string AspectTypeName, string PartName, string Order )
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
    {
        public override string ToString()
        {
            return $"AspectTypeName$PartName$Order";
        }

        public static LinkerAnnotation FromString( string str )
        {
            var parts = str.Split( '$' );

            return new LinkerAnnotation( parts[0], parts[1], parts[2] );
        }
    }
}
