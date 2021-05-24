using System;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// An interface that aspects can optionally implement if they want to
    /// customize the aspect description, layers, and dependencies.
    /// (a replacement to custom attributes).
    /// </summary>
    [Obsolete( "Not implemented." )]
    public interface IAspectClassMetadata
    {
        void BuildAspectClass( IAspectClassMetadataBuilder builder );
    }
}