// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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