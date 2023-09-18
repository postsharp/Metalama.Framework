using Metalama.Framework.Code;
using Metalama.Framework.Options;
using Metalama.Framework.Services;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.Engine.HierarchicalOptions;

/// <summary>
/// Provides <see cref="IHierarchicalOptions"/> defined in a referenced project or assembly.
/// </summary>
internal interface IExternalOptionsProvider : IProjectService
{
    bool TryGetOptions( IDeclaration declaration, Type optionsType, [NotNullWhen(true)] out IHierarchicalOptions? options );
}