// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.IO;
using Xunit.Abstractions;

namespace Caravela.TestFramework.XunitFramework
{
    internal class TestCase : ITestCase, ISourceInformation
    {
        private TestFactory _factory;
        private string _relativePath;

        public TestCase( TestFactory factory, string relativePath )
        {
            this._factory = factory;
            this._relativePath = relativePath;
        }

        public string FullPath => Path.Combine( this._factory.ProjectProperties.ProjectDirectory, this._relativePath );

        void IXunitSerializable.Deserialize( IXunitSerializationInfo info )
        {
            this._factory = new TestFactory( this._factory.ProjectProperties, info.GetValue<string>( "basePath" ), info.GetValue<string>( "assemblyName" ) );
            this._relativePath = info.GetValue<string>( "relativePath" );
        }

        void IXunitSerializable.Serialize( IXunitSerializationInfo info )
        {
            info.AddValue( "basePath", this._factory.ProjectProperties );
            info.AddValue( "relativePath", this._relativePath );
            info.AddValue( "assemblyName", this._factory.AssemblyInfo.Name );
        }

        string ITestCase.DisplayName => Path.GetFileNameWithoutExtension( this._relativePath );

        public string? SkipReason
            => TestInput.FromFile( this._factory.ProjectProperties, this._factory.DirectoryOptionsReader, this._relativePath ).Options.SkipReason!;

        ISourceInformation ITestCase.SourceInformation
        {
            get => this;
            set => throw new NotSupportedException();
        }

        Dictionary<string, List<string>> ITestCase.Traits { get; } = new();

        string ITestCase.UniqueID => this._relativePath;

        ITestMethod ITestCase.TestMethod => this._factory.GetTestMethod( this._relativePath );

        object[] ITestCase.TestMethodArguments => Array.Empty<object>();

        string ISourceInformation.FileName
        {
            get => Path.Combine( this._factory.ProjectProperties.ProjectDirectory, this._relativePath );
            set => throw new NotSupportedException();
        }

        int? ISourceInformation.LineNumber
        {
            get => 1;
            set => throw new NotSupportedException();
        }
    }
}