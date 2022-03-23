// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Metalama.Framework.Engine.Linking
{
    internal class LinkerCodeTransformationRegistry
    {
        /// <summary>
        /// Gets code transformations.
        /// </summary>
        public IReadOnlyDictionary<string, IReadOnlyList<CodeTransformationMark>> CodeTransformations { get; }

        private readonly HashSet<ISymbol> _declarationsWithCodeTransformations;

        public LinkerCodeTransformationRegistry(
            IReadOnlyDictionary<string, IReadOnlyList<CodeTransformationMark>> codeTransformations )
        {
            this.CodeTransformations = codeTransformations;
            this._declarationsWithCodeTransformations = new HashSet<ISymbol>(StructuralSymbolComparer.Default);

            foreach ( var codeTransformationMark in codeTransformations.Values.SelectMany( x => x ) )
            {
                this._declarationsWithCodeTransformations.Add( codeTransformationMark.Source.TargetDeclaration.GetSymbol().AssertNotNull() );
            }
        }

        public bool HasCodeTransformations(ISymbol symbol )
        {
            return this._declarationsWithCodeTransformations.Contains( symbol );
        }
    }
}