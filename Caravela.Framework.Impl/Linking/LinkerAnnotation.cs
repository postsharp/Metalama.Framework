using System;
using System.Collections.Generic;
using System.Text;

namespace Caravela.Framework.Impl.Linking
{
    public record LinkerAnnotation(string AspectTypeName, string PartName, string Order)
    {
        public override string ToString()
        {
            return $"AspectTypeName$PartName$Order";
        }

        public static LinkerAnnotation FromString(string str)
        {
            string[] parts = str.Split( '$' );

            return new LinkerAnnotation( parts[0], parts[1], parts[2] );
        }
    }
}
