// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Serialization;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.Serialization
{
    public class CanSerializeTests : SerializerTestsBase
    {
        private readonly SerializableTypes _serializableTypes;

        public CanSerializeTests()
        {
            this._serializableTypes = this.SerializationService.GetSerializableTypes( this.SerializationContext.Compilation );
        }

        private void AssertCanSerialize( bool expected, Type type )
        {
            var diagnosticList = new DiagnosticList();
            var result = this._serializableTypes.IsSerializable( this.SerializationContext.GetTypeSymbol( type ), Location.None, diagnosticList );

            Assert.Equal( expected, result );

            if ( !result )
            {
                Assert.NotEmpty( diagnosticList );
            }
            else
            {
                Assert.Empty( diagnosticList );
            }
        }

        [Fact]
        public void Serializable()
        {
            this.AssertCanSerialize( true, typeof(int) );
            this.AssertCanSerialize( true, typeof(Guid) );
            this.AssertCanSerialize( true, typeof(DateTime) );
            this.AssertCanSerialize( true, typeof(MethodInfo) );
            this.AssertCanSerialize( true, typeof(MethodBase) );
        }

        [Fact]
        public void Nullable()
        {
            this.AssertCanSerialize( true, typeof(int?) );
        }

        [Fact]
        public void Generic()
        {
            this.AssertCanSerialize( true, typeof(IEnumerable<int>) );
            this.AssertCanSerialize( true, typeof(List<int>) );
            this.AssertCanSerialize( true, typeof(List<List<int>>) );
            this.AssertCanSerialize( true, typeof(List<Guid?>) );
        }

        [Fact]
        public void NonSerializable()
        {
            this.AssertCanSerialize( false, typeof(AppDomain) );
            this.AssertCanSerialize( false, typeof(IEnumerable<AppDomain>) );
            this.AssertCanSerialize( false, typeof(Queue<>) );
        }
    }
}