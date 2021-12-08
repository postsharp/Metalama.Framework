// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CodeModel
{
    internal interface IDeclarationImpl : ISdkDeclaration, IDeclarationInternal, IDiagnosticLocationImpl
    {
        new Ref<IDeclaration> ToRef();

        /// <summary>
        /// Gets the <see cref="SyntaxReference"/> syntaxes that declare the current declaration.
        /// In case of a member introduction, this returns the syntax references of the type.
        /// In case of a type introduction, this returns an empty list.
        /// </summary>
        ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get; }

        /// <summary>
        /// Gets a value indicating whether a declaration can be inherited or overridden.
        /// </summary>
        bool CanBeInherited { get; }

        IEnumerable<IDeclaration> GetDerivedDeclarations( bool deep = true );
    }

    internal interface IMemberImpl : IDeclarationImpl, IMember
    {
        IMember? OverriddenMember { get; }
    }
}