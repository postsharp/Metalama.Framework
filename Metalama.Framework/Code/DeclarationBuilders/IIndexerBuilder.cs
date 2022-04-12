// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Framework.Code.DeclarationBuilders;

public interface IIndexerBuilder : IPropertyOrIndexerBuilder, IIndexer
{
    /// <summary>
    /// Adds a parameter to the current indexer and specifies its type using an <see cref="IType"/>.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="refKind"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    IParameterBuilder AddParameter( string name, IType type, RefKind refKind = RefKind.None, TypedConstant defaultValue = default );

    /// <summary>
    /// Adds a parameter to the current indexer and specifies its type using a reflection <see cref="Type"/>.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="refKind"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    IParameterBuilder AddParameter( string name, Type type, RefKind refKind = RefKind.None, object? defaultValue = null );
}