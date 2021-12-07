// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;

namespace Metalama.TestFramework
{
    /// <summary>
    /// Represents a metadata reference. This class is JSON-serializable.
    /// </summary>
    public class TestAssemblyReference
    {
        public string? Path { get; set; }

        internal MetadataReference ToMetadataReference() => MetadataReference.CreateFromFile( this.Path! );

        public override string ToString() => this.Path ?? "<null>";
    }
}