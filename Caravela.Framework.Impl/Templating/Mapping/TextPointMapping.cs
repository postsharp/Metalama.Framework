// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.IO;

namespace Caravela.Framework.Impl.Templating.Mapping
{
    /// <summary>
    /// Represents a mapping between a source and a target <see cref="TextPoint"/>.
    /// </summary>
    internal record TextPointMapping( TextPoint Source, TextPoint Target )
    {
        public void Write( BinaryWriter writer )
        {
            this.Source.Write( writer );
            this.Target.Write( writer );
        }

        public static TextPointMapping Read( BinaryReader reader ) => new( TextPoint.Read( reader ), TextPoint.Read( reader ) );
    }
}