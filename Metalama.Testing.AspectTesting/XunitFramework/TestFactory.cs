// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Metalama.Testing.AspectTesting.XunitFramework
{
    internal sealed class TestFactory
    {
        public TestProjectProperties ProjectProperties { get; }

        public string ProjectName { get; }

        private readonly ConcurrentDictionary<string, TestMethod> _methods = new();
        private readonly ConcurrentDictionary<string, TestClass> _types = new();

        public TestDirectoryOptionsReader DirectoryOptionsReader { get; }

        public TestFactory( TestProjectProperties projectProperties, string directory, string assemblyName )
            : this( projectProperties, new TestDirectoryOptionsReader( directory ), LoadAssembly( assemblyName ) ) { }

        private static ReflectionAssemblyInfo LoadAssembly( string assemblyName )
        {
            var assembly = Assembly.Load( assemblyName );

            return new ReflectionAssemblyInfo( assembly );
        }

        public TestFactory( TestProjectProperties projectProperties, TestDirectoryOptionsReader directoryOptionsReader, IAssemblyInfo assemblyInfo )
        {
            this.DirectoryOptionsReader = directoryOptionsReader;

            this.ProjectProperties = projectProperties;
            this.ProjectName = Path.GetFileName( this.ProjectProperties.ProjectDirectory );
            this.Collection = new TestCollection( this );
            this.AssemblyInfo = assemblyInfo;
        }

        public TestMethod GetTestMethod( string relativePath ) => this._methods.GetOrAdd( relativePath, p => new TestMethod( this, p ) );

        public TestClass GetTestType( string? relativePath ) => this._types.GetOrAdd( relativePath ?? "", p => new TestClass( this, p ) );

        public TestCollection Collection { get; }

        public IAssemblyInfo AssemblyInfo { get; }
    }
}