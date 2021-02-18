using System;
using System.Diagnostics;

namespace Caravela.Framework.Impl.Linking
{
    public enum LinkerAnnotationOrder
    {
        /// <summary>
        /// Calls the semantic in the state it is after the current aspect has been applied.
        /// </summary>
        Default,

        /// <summary>
        /// Calls the semantic in the original order, before any transformation.
        /// </summary>
        Original,
    }

    public record LinkerAnnotation(

        // Name of the aspect that is adding the semantic invocation to the syntax tree.
        string? AspectTypeName,

        // Part of the aspect that is adding the semantic invocation to the syntax tree.
        string? PartName,

        // Determines which version of the semantic must be invoked.
        LinkerAnnotationOrder Order )
    {
        public override string ToString()
        {
            return $"{this.AspectTypeName}${this.PartName}${this.Order}";
        }

        public static LinkerAnnotation FromString( string str )
        {
            var parts = str.Split( '$' );

            Debug.Assert( Enum.TryParse<LinkerAnnotationOrder>( parts[2], out var order ), "Invalid order." );

            return new LinkerAnnotation( parts[0], parts[1] == "" ? null : parts[1], order );
        }
    }
}
