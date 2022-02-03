// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using System.Collections.Immutable;

namespace Metalama.Framework.Workspaces;

public interface ICompilationSet
{
    ImmutableArray<ICompilation> Compilations { get; }

    /// <summary>
    /// Gets all types in the current set of compilations, including nested types.
    /// </summary>
    ImmutableArray<INamedType> Types { get; }

    /// <summary>
    /// Gets all methods in the current set of compilations, except local methods.
    /// </summary>
    ImmutableArray<IMethod> Methods { get; }

    /// <summary>
    /// Gets all fields in the current set of compilations.
    /// </summary>
    ImmutableArray<IField> Fields { get; }

    /// <summary>
    /// Gets all properties in the current set of compilations.
    /// </summary>
    ImmutableArray<IProperty> Properties { get; }

    /// <summary>
    /// Gets all properties and properties in the current set of compilations.
    /// </summary>
    ImmutableArray<IFieldOrProperty> FieldsAndProperties { get; }

    /// <summary>
    /// Gets all constructors in the current set of compilations.
    /// </summary>
    ImmutableArray<IConstructor> Constructors { get; }

    /// <summary>
    /// Gets all events in the current set of compilations.
    /// </summary>
    ImmutableArray<IEvent> Events { get; }

    /// <summary>
    /// Gets all target frameworks of projects in the current set of compilations.
    /// </summary>
    ImmutableArray<string> TargetFrameworks { get; }
}