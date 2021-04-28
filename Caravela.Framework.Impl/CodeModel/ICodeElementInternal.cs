// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Links;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.CodeModel
{
    internal interface ICodeElementInternal : ISdkCodeElement
    {
        CodeElementLink<ICodeElement> ToLink();

        /// <summary>
        /// Gets the <see cref="SyntaxReference"/> syntaxes that declare the current code element.
        /// In case of a member introduction, this returns the syntax references of the type.
        /// In case of a type introduction, this returns an empty list.
        /// </summary>
        ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get; }
    }
}