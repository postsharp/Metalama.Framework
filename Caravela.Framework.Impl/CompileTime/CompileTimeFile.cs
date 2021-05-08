using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Caravela.Framework.Impl.CompileTime
{
    /// <summary>
    /// Represents a file in a <see cref="CompileTimeProject"/>. This class is serialized
    /// to Json as a part of the <see cref="CompileTimeProjectManifest"/>.
    /// </summary>
    internal sealed class CompileTimeFile
    {
        /// <summary>
        /// Gets the source path.
        /// </summary>
        public string SourcePath { get; }
        
        /// <summary>
        /// Gets the transformed path (relatively to the root of the archive).
        /// </summary>
        public string TransformedPath { get;  }
        
        /// <summary>
        /// Gets the hash of the source.
        /// </summary>
        public ImmutableArray<byte> SourceHash { get;  }
        
        /// <summary>
        /// Gets the algorithm used to produce <see cref="SourceHash"/>.
        /// </summary>
        public SourceHashAlgorithm SourceHashAlgorithm { get;  }

        public CompileTimeFile()
        {
            // Deserializer.
        }

        public CompileTimeFile( string transformedPath, SyntaxTree sourceSyntaxTree )
        {
            var sourceText = sourceSyntaxTree.GetText();
            
            this.SourcePath = sourceSyntaxTree.FilePath;
            this.TransformedPath = transformedPath;
            this.SourceHash = sourceText.GetChecksum();
            this.SourceHashAlgorithm = sourceText.ChecksumAlgorithm;
        }


        /// <summary>
        /// Determines if the current <see cref="CompileTimeFile"/> corresponds to a source <see cref="SyntaxTree"/>.
        /// </summary>
        public bool SourceEquals( SyntaxTree syntaxTree )
            => syntaxTree.FilePath == this.SourcePath && 
               this.SourceHashAlgorithm == syntaxTree.GetText(  ).ChecksumAlgorithm 
                ? Enumerable.SequenceEqual( syntaxTree.GetText().GetChecksum(), this.SourceHash )
                : throw new NotImplementedException("Comparing two files with different checksum algorithms is not implemented.");
    }
}