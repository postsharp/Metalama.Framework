using Caravela.Framework.Validation;
using System.Collections.Generic;

namespace Caravela.Framework.Code
{
    [InternalImplement]
    public interface IProject
    {
        /// <summary>
        /// Gets the path to the <c>csproj</c> file.
        /// </summary>
        string Path { get; }
        
        string AssemblyName { get; }

        /// <summary>
        /// Symbols like <c>DEBUG</c>, <c>TRACE</c> (also named constants).
        /// </summary>
        string DefinedSymbols { get; }
        
        /// <summary>
        /// Gets the name of the build configuration, for instance <c>Debug</c> or <c>Release</c>.
        /// </summary>
        string Configuration { get; }
        
        /// <summary>
        /// Gets the identifier of the target framework, for instance <c>netstandard2.0</c>.
        /// </summary>
        string TargetFramework { get; }

        /// <summary>
        /// Gets the set of properties passed from MSBuild. To expose an MSBuild property to this collection,
        /// define the <c>CompilerVisibleProperty</c> item. 
        /// </summary>
        IReadOnlyDictionary<string, string> Properties { get; }

        /// <summary>
        /// Gets or creates a project extension and creates a new instance if not has been created before.
        /// </summary>
        /// <remarks>
        /// If this method is called when the project is read-only, a new instance but read-only instance is returned.  
        /// </remarks>
        /// <typeparam name="T">Extension type.</typeparam>
        T Extension<T>()
            where T : IProjectExtension, new();
    }
}