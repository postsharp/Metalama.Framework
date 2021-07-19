// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Pipeline
{
    internal static class AspectPipelineAnnotations
    {
        /// <summary>
        /// Annotation that means that the syntax has been generated by Caravela. This is used to selectively format the code,
        /// and can be used in the future for syntax highlighting.
        /// </summary>
        public static readonly SyntaxAnnotation GeneratedCode = new( "Caravela_Generated" );

        /// <summary>
        /// Annotation that means that the syntax stems from source code. This can be added to a child node of a node annotated
        /// with <see cref="GeneratedCode"/>.
        /// </summary>
        public static readonly SyntaxAnnotation SourceCode = new( "Caravela_SourceCode" );
    }
}