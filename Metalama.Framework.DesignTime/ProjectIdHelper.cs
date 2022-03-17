// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.DesignTime;

public static class ProjectIdHelper
{
    // The project id is passed to a constant, because that's the only public way to push a property to a compilation.
    private const string _projectIdPreprocessorSymbolPrefix = "MetalamaProjectId_";

    public static bool TryGetProjectId( Compilation compilation, [NotNullWhen( true )] out string? projectId )
    {
        var projectIdConstant = compilation.SyntaxTrees.First()
            .Options.PreprocessorSymbolNames.FirstOrDefault( x => x.StartsWith( _projectIdPreprocessorSymbolPrefix, StringComparison.OrdinalIgnoreCase ) );

        projectId = projectIdConstant?.Substring( _projectIdPreprocessorSymbolPrefix.Length );

        return !string.IsNullOrEmpty( projectId );
    }
}