// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.TestFramework.XunitFramework
{
    internal class TestCollection : LongLivedMarshalByRefObject, ITestCollection, ITestAssembly
    {
        private readonly TestFactory _factory;

        public TestCollection( TestFactory factory )
        {
            this._factory = factory;
        }

        void IXunitSerializable.Deserialize( IXunitSerializationInfo info ) { }

        void IXunitSerializable.Serialize( IXunitSerializationInfo info ) { }

        ITypeInfo ITestCollection.CollectionDefinition => null!;

        string ITestCollection.DisplayName => "All tests";

        ITestAssembly ITestCollection.TestAssembly => this;

        Guid ITestCollection.UniqueID { get; } = Guid.NewGuid();

        IAssemblyInfo ITestAssembly.Assembly => this._factory.AssemblyInfo;

        string ITestAssembly.ConfigFileName => null!;
    }
}