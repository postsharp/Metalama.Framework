using PostSharp.Engineering.BuildTools.Console;
using System;
using System.CommandLine.Invocation;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace PostSharp.Engineering.BuildTools.Build
{
    public class BuildContext
    {
        public BuildOptions Options { get; }
        public ConsoleHelper Console { get; }

        public string RepoDirectory { get; }

        private BuildContext( BuildOptions options, ConsoleHelper console, string repoDirectory )
        {
            this.Options = options;
            this.Console = console;
            this.RepoDirectory = repoDirectory;
        }

        public static bool TryCreate( InvocationContext invocationContext, BuildOptions options,
            [NotNullWhen( true )] out BuildContext? buildContext )
        {
            var console = new ConsoleHelper( invocationContext.Console );
            var repoDirectory = FindRepoDirectory( Environment.CurrentDirectory );

            if ( repoDirectory == null )
            {
                console.WriteError( "This tool must be called from a git repository." );
                buildContext = null;
                return false;
            }

            buildContext = new BuildContext( options, console, repoDirectory );
            return true;
        }

        private static string? FindRepoDirectory( string directory )
        {
            if ( File.Exists( Path.Combine( directory, ".git" ) ) )
            {
                return directory;
            }
            else
            {
                var parentDirectory = Path.GetDirectoryName( directory );
                if ( parentDirectory != null )
                {
                    return FindRepoDirectory( parentDirectory );
                }
                else
                {
                    return null;
                }
            }
        }
    }
}