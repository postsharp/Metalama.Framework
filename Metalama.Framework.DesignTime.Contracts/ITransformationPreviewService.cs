// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.DesignTime.Contracts
{
    /// <summary>
    /// Defines a method that allows to transform a single syntax tree in a compilation. This service
    /// is used to produce the diff view between original code and transform code.
    /// </summary>
    [Guid( "9601bb8b-917a-44d8-a1cf-80f7b6569d0d" )]
    [ComImport]
    public interface ITransformationPreviewService : ICompilerService
    {
        // Note: the C# idiomatic form of this method should return a Task<IPreviewTransformationResult?>. However, we're using [ComImport] and type equivalence
        // and it does not support any generic type in argument methods. An alternative design would be to stop using ComImport and type equivalence,
        // but in this case we would need a new assembly every time we want to add an interface.

        /// <summary>
        /// Transforms a single syntax tree in a compilation.
        /// </summary>
        /// <param name="compilation"></param>
        /// <param name="syntaxTree"></param>
        /// <param name="cancellationToken"></param>
        ValueTask<IPreviewTransformationResult> PreviewTransformationAsync(
            Compilation compilation,
            SyntaxTree syntaxTree,
            CancellationToken cancellationToken );
    }
}