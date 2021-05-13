// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis.Text;
using System.IO;

namespace Caravela.Framework.Impl.Templating.Mapping
{
    /// <summary>
    /// Represent a position in a text file.
    /// </summary>
    /// <param name="Character">Position of the character counted from the beginning of the file.</param>
    /// <param name="LinePosition">Line and column.</param>
    internal record TextPoint( int Character, LinePosition LinePosition )
    {
        public void Write( BinaryWriter writer )
        {
            writer.Write( this.Character );
            writer.Write( this.LinePosition.Line );
            writer.Write( this.LinePosition.Character );
        }

        public static TextPoint Read( BinaryReader reader ) => new( reader.ReadInt32(), new LinePosition( reader.ReadInt32(), reader.ReadInt32() ) );
    }
}