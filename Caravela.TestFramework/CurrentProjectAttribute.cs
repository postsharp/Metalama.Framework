// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.TestFramework.XunitFramework;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Reflection;
using Xunit.Sdk;

namespace Caravela.TestFramework
{
    /// <summary>
    /// An implementation of <c>SyntaxAttribute</c> that generates test cases from files in the current project. To be used with <c>[Theory]</c>.
    /// This attribute will not include subdirectories that contain a file named <c>_Runner.cs</c>.
    /// It also takes into account the <c>caravelaTests.config</c> file.
    /// </summary>
    public sealed class CurrentProjectAttribute : DataAttribute
    {
        public override IEnumerable<object[]> GetData( MethodInfo testMethod )
        {
            var discoverer = new TestDiscoverer( new ReflectionAssemblyInfo( testMethod.DeclaringType!.Assembly ) );

            var projectDirectory = discoverer.FindProjectDirectory();

            foreach ( var testCase in discoverer.Discover( projectDirectory, ImmutableHashSet<string>.Empty ) )
            {
                if ( testCase.SkipReason == null )
                {
                    var relativePath = Path.GetRelativePath( projectDirectory, testCase.FullPath );

                    yield return new object[] { relativePath };
                }
            }
        }
    }
}