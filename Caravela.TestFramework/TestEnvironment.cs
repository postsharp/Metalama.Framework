using System;
using System.Linq;
using System.Reflection;

namespace Caravela.TestFramework
{
    internal static class TestEnvironment
    {
        public static string GetProjectDirectory( Assembly assembly )
        {
            var projectDirectoryAttribute = assembly.GetCustomAttributes<AssemblyMetadataAttribute>().SingleOrDefault( a => a.Key == "ProjectDirectory" );

            if ( projectDirectoryAttribute == null )
            {
                throw new InvalidOperationException( "The test assembly must have a single AssemblyMetadataAttribute with Key = \"ProjectDirectory\"." );
            }

            if ( string.IsNullOrEmpty( projectDirectoryAttribute.Value ) )
            {
                throw new InvalidOperationException(
                    "The project directory cannot be null or empty."
                    + " The project directory is stored as a value of the AssemblyMetadataAttribute with Key = \"ProjectDirectory\"." );
            }

            return projectDirectoryAttribute.Value;
        }
    }
}
