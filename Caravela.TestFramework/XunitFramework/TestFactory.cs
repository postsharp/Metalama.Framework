// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Caravela.TestFramework.XunitFramework
{
    internal class TestFactory
    {
        public string ProjectDirectory { get; }
        
        public string ProjectName { get; }

        private readonly ConcurrentDictionary<string, TestMethod> _methods = new();
        private readonly ConcurrentDictionary<string, TestClass> _types = new();

        public TestDirectoryOptionsReader DirectoryOptionsReader { get; }

        public TestFactory( string baseDirectory, string assemblyName ) 
            : this( new TestDirectoryOptionsReader( baseDirectory ), LoadAssembly( assemblyName ) ) { }

        private static ReflectionAssemblyInfo LoadAssembly( string assemblyName )
        {
            var assembly = Assembly.Load( assemblyName );

            return new ReflectionAssemblyInfo( assembly );
        }

        public TestFactory( TestDirectoryOptionsReader directoryOptionsReader, IAssemblyInfo assemblyInfo )
        {
            this.DirectoryOptionsReader = directoryOptionsReader;
            
            this.ProjectDirectory = directoryOptionsReader.ProjectDirectory;
            this.ProjectName = Path.GetFileName( this.ProjectDirectory );
            this.Collection = new TestCollection( this );
            this.AssemblyInfo = assemblyInfo;
        }

        public TestMethod GetTestMethod( string relativePath ) => this._methods.GetOrAdd( relativePath, p => new TestMethod( this, p ) );

        public TestClass GetTestType( string? relativePath ) => this._types.GetOrAdd( relativePath ?? "", p => new TestClass( this, p ) );

        public TestCollection Collection { get; }

        public IAssemblyInfo AssemblyInfo { get; }
    }
}