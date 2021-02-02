using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Caravela.Framework.DesignTime.Contracts
{
    /// <summary>
    /// Exposes a global connection point between compiler assemblies, included in NuGet packages and loaded by Roslyn,
    /// and the UI assemblies, included in the VSX and loaded by Visual Studio. Compiler assemblies register
    /// themselves using <see cref="RegisterServiceProvider"/> and UI assemblies get the interface using <see cref="GetServiceProviderAsync"/>.
    /// Since VS session can contain projects with several versions of Caravela, this class has the responsibility
    /// to match versions.
    /// </summary>
    public static class DesignTimeEntryPointManager
    {
        private static volatile TaskCompletionSource<ICompilerServiceProvider> _registrationTask = new TaskCompletionSource<ICompilerServiceProvider>();
        private static readonly Version _matchAllVersion = new Version( 9999, 99 ); 
        private static readonly object _sync = new object();
        private static ImmutableHashSet<ICompilerServiceProvider> _entryPoints = ImmutableHashSet<ICompilerServiceProvider>.Empty;

        private static readonly ConcurrentDictionary<Version, Task<ICompilerServiceProvider>> _getProviderTasks =
            new ConcurrentDictionary<Version, Task<ICompilerServiceProvider>>();
        

        /// <summary>
        /// Gets the <see cref="ICompilerService"/> for a specific project.
        /// </summary>
        /// <param name="project"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async ValueTask<ICompilerServiceProvider?> GetServiceProviderAsync( Project project, CancellationToken cancellationToken )
        {
            // TODO: this design probably does not support the operation of adding a Caravela reference to a project, i.e. this function is modeled
            // as a pure function but the project is not immutable.

            if ( !TryGetReferencedVersion(project, out var frameworkVersion) )
            {
                return null;
            }

            var task = _getProviderTasks.GetOrAdd( frameworkVersion,  GetProviderForVersion );

            if ( !task.IsCompleted )
            {
                var taskCancelled = new TaskCompletionSource<bool>();
                using ( cancellationToken.Register( () => taskCancelled.SetCanceled() ) )
                {
                    await Task.WhenAny( task, taskCancelled.Task );
                }

            }
            
            return task.Result;


        }

        private static async Task<ICompilerServiceProvider> GetProviderForVersion(Version version)
        {

            while (true)
            {
                lock ( _sync )
                {
                    foreach ( var entryPoint in _entryPoints )
                    {
                        if ( version == _matchAllVersion || entryPoint.Version == version )
                        {
                            return entryPoint;
                        }
                    }
                }

                await _registrationTask.Task;
            }
        }

        private static bool IsOurReference( string? name )
            => string.Equals( name, "Caravela.Framework", StringComparison.OrdinalIgnoreCase );

        private static bool TryGetReferencedVersion( Project project, [NotNullWhen(true)] out Version? version )
        {
            foreach ( var reference in project.MetadataReferences )
            {
                switch ( reference )
                {
                    case CompilationReference cr:
                        if ( IsOurReference(cr.Compilation.AssemblyName))
                        {
                            // TODO: Have the real version.
                            version = _matchAllVersion;
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
                                version = _matchAllVersion;
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
                    version = _matchAllVersion;
                    return true;
                }
            }

            version = null;
            return false;
        }

        public static void RegisterServiceProvider( ICompilerServiceProvider entryPoint )
        {
            lock ( _sync )
            {
                entryPoint.Unloaded += OnUnloaded;
                _entryPoints = _entryPoints.Add( entryPoint );

                // The order here is important.
                var oldRegistrationTask = _registrationTask;
                _registrationTask = new TaskCompletionSource<ICompilerServiceProvider>();
                oldRegistrationTask.SetResult( entryPoint );

            }
        }

        private static void OnUnloaded( ICompilerServiceProvider entryPoint )
        {
            lock ( _sync )
            {
                _entryPoints =  _entryPoints.Remove( entryPoint );
            }
        }

    }
}