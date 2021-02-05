using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Caravela.Framework.DesignTime.Contracts
{
    /// <summary>
    /// Exposes a global connection point between compiler assemblies, included in NuGet packages and loaded by Roslyn,
    /// and the UI assemblies, included in the VSX and loaded by Visual Studio. Compiler assemblies register
    /// themselves using <see cref="IDesignTimeEntryPointManager.RegisterServiceProvider"/> and UI assemblies get the
    /// interface using <see cref="IDesignTimeEntryPointManager.GetServiceProviderAsync"/>.
    /// Since VS session can contain projects with several versions of Caravela, this class has the responsibility
    /// to match versions.
    /// </summary>
    public class DesignTimeEntryPointManager : IDesignTimeEntryPointManager
    {
        private const string _appDomainDataName = "Caravela.Framework.DesignTime.Contracts.DesignTimeEntryPointManager";

        public static IDesignTimeEntryPointManager Instance { get; private set; }

        static DesignTimeEntryPointManager()
        {
            // Note that there maybe many instances of this class in the AppDomain, so it needs to make sure it uses a shared point of contact.
            // We're using a named AppDomain data slot for this. We have to synchronize access using a named semaphore.
                
            using var semaphore = new Semaphore( 1, 1, _appDomainDataName );
            try
            {
                semaphore.WaitOne();
                var oldInstance = (IDesignTimeEntryPointManager?) AppDomain.CurrentDomain.GetData( _appDomainDataName );
                if ( oldInstance != null )
                {
                    Instance = oldInstance;
                }
                else
                {
                    Instance = new DesignTimeEntryPointManager();
                    AppDomain.CurrentDomain.SetData( _appDomainDataName, Instance );

                }

            }
            finally
            {
                semaphore.Release();
            }
        }

        private DesignTimeEntryPointManager()
        {
            
        }
        
        
        
        private volatile TaskCompletionSource<ICompilerServiceProvider> _registrationTask = new TaskCompletionSource<ICompilerServiceProvider>();
        internal static Version MatchAllVersion { get; } = new Version( 9999, 99 );
        
        private readonly object _sync = new object();
        private ImmutableHashSet<ICompilerServiceProvider> _entryPoints = ImmutableHashSet<ICompilerServiceProvider>.Empty;

        private static readonly ConcurrentDictionary<Version, Task<ICompilerServiceProvider>> _getProviderTasks =
            new ConcurrentDictionary<Version, Task<ICompilerServiceProvider>>();


        async ValueTask<ICompilerServiceProvider?> IDesignTimeEntryPointManager.GetServiceProviderAsync( Version version, CancellationToken cancellationToken )
        {

            var task = _getProviderTasks.GetOrAdd( version, this.GetProviderForVersion );

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

        private  async Task<ICompilerServiceProvider> GetProviderForVersion(Version version)
        {

            while (true)
            {
                lock ( this._sync )
                {
                    foreach ( var entryPoint in this._entryPoints )
                    {
                        if ( version == MatchAllVersion || entryPoint.Version == version )
                        {
                            return entryPoint;
                        }
                    }
                }

                await this._registrationTask.Task;
            }
        }


        void IDesignTimeEntryPointManager.RegisterServiceProvider( ICompilerServiceProvider entryPoint )
        {
            lock ( this._sync )
            {
                entryPoint.Unloaded += this.OnUnloaded;
                this._entryPoints = this._entryPoints.Add( entryPoint );

                // The order here is important.
                var oldRegistrationTask = this._registrationTask;
                this._registrationTask = new TaskCompletionSource<ICompilerServiceProvider>();
                oldRegistrationTask.SetResult( entryPoint );

            }
        }

        Version IDesignTimeEntryPointManager.Version =>  this.GetType().Assembly.GetName().Version;

        private void OnUnloaded( ICompilerServiceProvider entryPoint )
        {
            lock ( this._sync )
            {
                this._entryPoints = this._entryPoints.Remove( entryPoint );
            }
        }

    }

    public static class DesignTimeEntryPointManagerExtensions
    {
        
        
        private static bool IsOurReference( string? name )
            => string.Equals( name, "Caravela.Framework", StringComparison.OrdinalIgnoreCase );

        private static bool TryGetReferencedVersion( Project project, [NotNullWhen(true)] out Version? version )
        {
            // Note that it is not enough that this method returns true for a project to get Caravela UI services.
            // The other condition is that at least one other project in the solution references the Caravela analyzer,
            // and that this analyzer is loaded by Visual Studio.

            if ( IsOurReference(project.AssemblyName))
            {
                version = DesignTimeEntryPointManager.MatchAllVersion;
                return true;
            }

            foreach ( var reference in project.MetadataReferences )
            {
                switch ( reference )
                {
                    case CompilationReference cr:
                        if ( IsOurReference(cr.Compilation.AssemblyName))
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