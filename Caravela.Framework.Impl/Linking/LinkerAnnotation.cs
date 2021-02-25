using System;

namespace Caravela.Framework.Impl.Linking
{
    internal enum LinkerAnnotationOrder
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

    internal record LinkerAnnotation(

        AspectLayerId AspectLayerId,

        // Determines which version of the semantic must be invoked.
        LinkerAnnotationOrder Order )
    {
        public override string ToString()
        {
            return $"{this.AspectLayerId.FullName}${this.Order}";
        }

        public static LinkerAnnotation FromString( string str )
        {
            var parts = str.Split( '$' );

            var parseSuccess = Enum.TryParse<LinkerAnnotationOrder>( parts[1], out var order );

            Invariant.Assert( parseSuccess );

            return new LinkerAnnotation( AspectLayerId.FromString( parts[0] ), order );
        }
    }
}
