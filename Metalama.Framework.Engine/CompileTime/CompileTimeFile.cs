// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Templating.Mapping;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Reflection;

namespace Metalama.Framework.Engine.CompileTime
{
    /// <summary>
    /// Represents a file in a <see cref="CompileTimeProject"/>. This class is serialized
    /// to Json as a part of the <see cref="CompileTimeProjectManifest"/>.
    /// </summary>
    [Obfuscation( Exclude = true /* JSON */ )]
    internal sealed class CompileTimeFile
    {
        // TODO: Add serialization-deserialization tests because this is brittle.

        /// <summary>
        /// Gets the source path.
        /// </summary>
        public string SourcePath { get; init; }

        /// <summary>
        /// Gets the transformed path (relatively to the root of the archive).
        /// </summary>
        public string TransformedPath { get; init; }

        /// <summary>
        /// Gets the hash of the source.
        /// </summary>
        public ImmutableArray<byte> SourceHash { get; init; }

        /// <summary>
        /// Gets the algorithm used to produce <see cref="SourceHash"/>.
        /// </summary>
        public SourceHashAlgorithm SourceHashAlgorithm { get; init; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public CompileTimeFile()
#pragma warning restore CS8618 // Elements should appear in the correct order
        {
            // Deserializer.
        }

        public CompileTimeFile( TextMapFile textMapFile )
        {
            this.SourcePath = textMapFile.SourcePath;
            this.TransformedPath = textMapFile.TargetPath;
            this.SourceHash = ImmutableArray<byte>.Empty;
            this.SourceHashAlgorithm = SourceHashAlgorithm.None;
        }
    }
}