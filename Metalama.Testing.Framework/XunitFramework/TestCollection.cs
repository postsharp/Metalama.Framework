// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Testing.Framework.XunitFramework
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