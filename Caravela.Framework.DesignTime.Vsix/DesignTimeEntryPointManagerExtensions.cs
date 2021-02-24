// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Caravela.Framework.DesignTime.Contracts;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.DesignTime.Vsix
{

    public static class DesignTimeEntryPointManagerExtensions
    {

        private static bool IsOurReference( string? name )
            => string.Equals( name, "Caravela.Framework", StringComparison.OrdinalIgnoreCase );

        private static bool TryGetReferencedVersion( Project project, [NotNullWhen( true )] out Version? version )
        {
            // Note that it is not enough that this method returns true for a project to get Caravela UI services.
            // The other condition is that at least one other project in the solution references the Caravela analyzer,
            // and that this analyzer is loaded by Visual Studio.

            if ( IsOurReference( project.AssemblyName ) )
            {
                version = DesignTimeEntryPointManager.MatchAllVersion;
                return true;
            }

            foreach ( var reference in project.MetadataReferences )
            {
                switch ( reference )
                {
                    case CompilationReference cr:
                        if ( IsOurReference( cr.Compilation.AssemblyName ) )
                        {
                            // TODO: Have the real version.
                            version = DesignTimeEntryPointManager.MatchAllVersion;
                            return true;
                        }

                        break;

                    case PortableExecutableReference per:
                        if ( IsOurReference( Path.GetFileNameWithoutExtension( per.FilePath ) ) )
                        {
                            if ( per.Display != null && AssemblyIdentity.TryParseDisplayName( per.Display, out var assemblyIdentity ) )
                            {
                                version = assemblyIdentity.Version;
                            }
                            else
                            {
                                version = DesignTimeEntryPointManager.MatchAllVersion;
                            }

                            return true;
                        }

                        break;
                }
            }

            foreach ( var reference in project.ProjectReferences )
            {
                var referencedProject = project.Solution.GetProject( reference.ProjectId );
                if ( referencedProject != null && IsOurReference( referencedProject.AssemblyName ) )
                {
                    // Matching all is intentional here. We are in the scenario when a Caravela developer builds Caravela.
                    version = DesignTimeEntryPointManager.MatchAllVersion;
                    return true;
                }
            }

            version = null;
            return false;
        }

        /// <summary>
        /// Gets the <see cref="ICompilerService"/> for a specific project.
        /// </summary>
        /// <param name="entryPointManager"></param>
        /// <param name="project"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async ValueTask<ICompilerServiceProvider?> GetServiceProviderAsync( this IDesignTimeEntryPointManager entryPointManager, Project project, CancellationToken cancellationToken )
        {

            if ( !TryGetReferencedVersion( project, out var version ) )
            {
                return null;
            }
            else
            {
                return await entryPointManager.GetServiceProviderAsync( version, cancellationToken );
            }
        }
    }
}