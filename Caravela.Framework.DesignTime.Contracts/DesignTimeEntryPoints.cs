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
    public static class DesignTimeEntryPoints
    {
        private static volatile TaskCompletionSource<IDesignTimeEntryPoint> registrationTask= new TaskCompletionSource<IDesignTimeEntryPoint>();

        
        private static readonly object sync = new object();
        private static ImmutableHashSet<IDesignTimeEntryPoint> entryPoints = ImmutableHashSet<IDesignTimeEntryPoint>.Empty;

        private static ConcurrentDictionary<Project, Task<IDesignTimeEntryPoint>> getProjectTasks =
            new ConcurrentDictionary<Project, Task<IDesignTimeEntryPoint>>();
        

        


        public static async ValueTask<IDesignTimeEntryPoint> GetDesignTimeEntryPoint( Project project, CancellationToken cancellationToken )
        {
            // TODO: this design probably does not support the operation of adding a Caravela reference to a project, i.e. this function is modeled
            // as a pure function but the project is not immutable.

            if ( !IsProjectSupported(project) )
            {
                return null;
            }

            var task = getProjectTasks.GetOrAdd( project,  GetDesignTimeProjectEntryPointCore );
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

        private static async Task<IDesignTimeEntryPoint> GetDesignTimeProjectEntryPointCore(Project project)
        {

            while (true)
            {
                lock ( sync )
                {
                    foreach ( var entryPoint in entryPoints )
                    {
                        if ( entryPoint.HandlesProject( project ) )
                        {
                            return entryPoint;
                        }
                    }
                }

                await registrationTask.Task;;

            }
        }

        private static bool IsProjectSupported( Project project ) =>
             project.MetadataReferences.Any(r => r switch
                    {
                        CompilationReference cr => string.Equals( cr.Compilation.AssemblyName, "Caravela.Framework", StringComparison.OrdinalIgnoreCase ),
                        PortableExecutableReference per => string.Equals( Path.GetFileNameWithoutExtension( per.FilePath ), "Caravela.Framework", StringComparison.OrdinalIgnoreCase ),
                        _ => false
                    }
                    ) ||
            project.ProjectReferences.Any(r => string.Equals( project.Solution.GetProject(r.ProjectId )!.AssemblyName, "Caravela.Framework", StringComparison.OrdinalIgnoreCase ) );


        public static void RegisterEntryPoint( IDesignTimeEntryPoint entryPoint )
        {
            lock ( sync )
            {
                entryPoint.Disposed += OnDisposed;
                entryPoints = entryPoints.Add( entryPoint );

                // The order here is important.
                var oldRegistrationTask = registrationTask;
                registrationTask = new TaskCompletionSource<IDesignTimeEntryPoint>();
                oldRegistrationTask.SetResult( entryPoint );

            }
        }

        private static void OnDisposed( IDesignTimeEntryPoint entryPoint )
        {
            lock ( sync )
            {
                entryPoints =  entryPoints.Remove( entryPoint );
            }
        }

     


    }
}