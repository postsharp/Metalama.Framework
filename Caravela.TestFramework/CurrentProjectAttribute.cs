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