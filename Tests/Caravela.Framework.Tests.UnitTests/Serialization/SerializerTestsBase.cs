// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Caravela.Framework.Tests.UnitTests.Serialization
{
    public abstract class SerializerTestsBase : TestBase
    {
        private protected ISyntaxFactory SyntaxFactory { get; }

        private protected SyntaxSerializationService SerializationService { get; }

        protected ExpressionSyntax Serialize<T>( T o ) => this.SerializationService.Serialize( o, this.SyntaxFactory );

        public SerializerTestsBase()
        {
            // We need a syntax factory for an arbitrary compilation, but at least with standard references.
            // Note that we cannot easily get a reference to Caravela.Compiler.Interfaces this way because we have a reference assembly.
            this.SyntaxFactory = ReflectionMapper.GetInstance(
                CreateRoslynCompilation(
                    "/* No code is necessary, only references */",
                    additionalReferences: new[]
                    {
                        MetadataReference.CreateFromFile( typeof(ICompileTimeReflectionMember).Assembly.Location ),
                        MetadataReference.CreateFromFile( typeof(Queue<>).Assembly.Location )
                    } ) );

            this.SerializationService = new SyntaxSerializationService();
        }
    }
}