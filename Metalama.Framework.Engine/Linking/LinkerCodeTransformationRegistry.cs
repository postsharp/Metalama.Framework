// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Transformations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Metalama.Framework.Engine.Linking
{
    internal class LinkerCodeTransformationRegistry
    {
        /// <summary>
        /// Gets code transformations.
        /// </summary>
        public IReadOnlyDictionary<string, IReadOnlyList<CodeTransformationMark>> CodeTransformations { get; }

        public LinkerCodeTransformationRegistry(
            IReadOnlyDictionary<string, IReadOnlyList<CodeTransformationMark>> codeTransformation )
        {
            this.CodeTransformations = codeTransformation;
        }
    }
}
