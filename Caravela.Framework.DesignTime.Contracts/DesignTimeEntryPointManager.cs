using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Caravela.Framework.DesignTime.Contracts
{
    public static class DesignTimeEntryPointManager
    {
        private static volatile TaskCompletionSource<IDesignTimeEntryPoint> registrationTask= new TaskCompletionSource<IDesignTimeEntryPoint>();

        static readonly Version matchAllVersion = new Version( 9999, 99 ); 

        private static readonly object sync = new object();
        private static ImmutableHashSet<IDesignTimeEntryPoint> entryPoints = ImmutableHashSet<IDesignTimeEntryPoint>.Empty;

        private static readonly ConcurrentDictionary<Version, Task<IDesignTimeEntryPoint>> _getVersionedEntryPointTasks =
            new ConcurrentDictionary<Version, Task<IDesignTimeEntryPoint>>();
        

        


        public static async ValueTask<IDesignTimeEntryPoint> GetDesignTimeEntryPoint( Project project, CancellationToken cancellationToken )
        {
            // TODO: this design probably does not support the operation of adding a Caravela reference to a project, i.e. this function is modeled
            // as a pure function but the project is not immutable.

            if ( !TryGetReferencedVersion(project, out var frameworkVersion) )
            {
                return null;
            }

            var task = _getVersionedEntryPointTasks.GetOrAdd( frameworkVersion,  GetDesignTimeVersionedEntryPointCore );

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

        private static async Task<IDesignTimeEntryPoint> GetDesignTimeVersionedEntryPointCore(Version version)
        {

            while (true)
            {
                lock ( sync )
                {
                    foreach ( var entryPoint in entryPoints )
                    {
                        if ( version == matchAllVersion || entryPoint.Version == version )
                        {
                            return entryPoint;
                        }
                    }
                }

                await registrationTask.Task;;

            }
        }

        private static bool IsOurReference( string name )
            => string.Equals( name, "Caravela.Framework", StringComparison.OrdinalIgnoreCase );

        private static bool TryGetReferencedVersion( Project project, out Version version )
        {
            foreach ( var reference in project.MetadataReferences )
            {
                switch ( reference )
                {
                    case CompilationReference cr:
                        if ( IsOurReference(cr.Compilation.AssemblyName))
                        {
                            // TODO: Have the real version.
                            version = matchAllVersion;
                            return true;
                        }
                        break;

                    case PortableExecutableReference per:
                        if ( IsOurReference( Path.GetFileNameWithoutExtension( per.FilePath ) ) )
                        {
                            if ( AssemblyIdentity.TryParseDisplayName( per.Display, out var assemblyIdentity ) )
                            {
                                version = assemblyIdentity.Version;
                            }
                            else
                            {
                                version = matchAllVersion;
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
                    version = matchAllVersion;
                    return true;
                }
            }

            version = null;
            return false;
        }

        public static void RegisterEntryPoint( IDesignTimeEntryPoint entryPoint )
        {
            lock ( sync )
            {
                entryPoint.Unloaded += OnUnloaded;
                entryPoints = entryPoints.Add( entryPoint );

                // The order here is important.
                var oldRegistrationTask = registrationTask;
                registrationTask = new TaskCompletionSource<IDesignTimeEntryPoint>();
                oldRegistrationTask.SetResult( entryPoint );

            }
        }

        private static void OnUnloaded( IDesignTimeEntryPoint entryPoint )
        {
            lock ( sync )
            {
                entryPoints =  entryPoints.Remove( entryPoint );
            }
        }

     


    }
}