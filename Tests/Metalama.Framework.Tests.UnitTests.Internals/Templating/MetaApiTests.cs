// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.Templating
{
    public class MetaApiTests : TestBase
    {
        [Fact]
        public async Task OutOfContext()
        {
            Assert.Throws<InvalidOperationException>( () => meta.Base );
            Assert.Throws<InvalidOperationException>( () => meta.Tags );
            Assert.Throws<InvalidOperationException>( () => meta.Target );
            Assert.Throws<InvalidOperationException>( () => meta.This );
            Assert.Throws<InvalidOperationException>( () => meta.BaseStatic );
            Assert.Throws<InvalidOperationException>( () => meta.ThisStatic );
            Assert.Throws<InvalidOperationException>( meta.Proceed );
            await Assert.ThrowsAsync<InvalidOperationException>( meta.ProceedAsync );
            Assert.Throws<InvalidOperationException>( meta.ProceedEnumerable );
            Assert.Throws<InvalidOperationException>( meta.ProceedEnumerator );

            Assert.Throws<InvalidOperationException>( () => meta.CompileTime( 0 ) );
            Assert.Throws<InvalidOperationException>( () => meta.RunTime( 0 ) );
            Assert.Throws<InvalidOperationException>( meta.DebugBreak );
            Assert.Throws<InvalidOperationException>( () => meta.InsertComment( "" ) );
            Assert.Throws<InvalidOperationException>( () => meta.DefineExpression( "", out _ ) );
            Assert.Throws<InvalidOperationException>( () => meta.ParseExpression( "" ) );

#if NET5_0_OR_GREATER
            Assert.Throws<InvalidOperationException>( meta.ProceedAsyncEnumerable );
            Assert.Throws<InvalidOperationException>( meta.ProceedAsyncEnumerator );
#endif
        }
    }
}