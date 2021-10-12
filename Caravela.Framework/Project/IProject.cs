// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Validation;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Caravela.Framework.Project
{
    [InternalImplement]
    [CompileTimeOnly]
    public interface IProject
    {
        /// <summary>
        /// Gets the path to the <c>csproj</c> file.
        /// </summary>
        string? Path { get; }

        /// <summary>
        /// Gets the list of assembly references of the current project.
        /// </summary>
        ImmutableArray<IAssemblyIdentity> AssemblyReferences { get; }

        /// <summary>
        /// Gets the list of defined preprocessor symbols like <c>DEBUG</c>, <c>TRACE</c>, <c>NET5_0</c> and so on.
        /// </summary>
        ImmutableHashSet<string> PreprocessorSymbols { get; }

        /// <summary>
        /// Gets the name of the build configuration, for instance <c>Debug</c> or <c>Release</c>.
        /// </summary>
        string? Configuration { get; }

        /// <summary>
        /// Gets the identifier of the target framework, for instance <c>netstandard2.0</c>.
        /// </summary>
        string? TargetFramework { get; }

        /// <summary>
        /// Gets the set of properties passed from MSBuild. To expose an MSBuild property to this collection,
        /// define the <c>CompilerVisibleProperty</c> item. 
        /// </summary>
        bool TryGetProperty( string name, [NotNullWhen( true )] out string? value );

        /// <summary>
        /// Gets a project data extension or creates a new instance if none has been created before. If the type may implement <see cref="IProjectData"/>,
        /// new instances will be initialized using <see cref="IProjectData.Initialize"/>.
        /// </summary>
        /// <typeparam name="T">The data type, which may implement <see cref="IProjectData"/>.</typeparam>
        T Data<T>()
            where T : class, IProjectData, new();

        /// <summary>
        /// Gets an <see cref="IServiceProvider"/> that gives access to the compiler services exposed using the <c>[CompileTimePlugIn]</c> facility.
        /// Only interfaces that derive from <see cref="IService"/> are accessible from this property.
        /// </summary>
        IServiceProvider ServiceProvider { get; }
    }
}