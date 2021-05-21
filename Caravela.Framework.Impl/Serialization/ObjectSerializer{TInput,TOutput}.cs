// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Caravela.Framework.Impl.Serialization
{
    /// <summary>
    /// As <see cref="ObjectSerializer"/>, except strongly-typed.
    /// </summary>
    internal abstract class ObjectSerializer<TInput, TOutput> : ObjectSerializer
        where TInput : TOutput
    {
        /// <inheritdoc />
        public sealed override ExpressionSyntax Serialize( object obj, ICompilationElementFactory syntaxFactory ) => this.Serialize( (TInput) obj, syntaxFactory );

        /// <summary>
        /// Serializes an object of a type supported by this object serializer into a Roslyn expression that creates such an object.
        /// </summary>
        /// <param name="obj">An object to serialize. Not null.</param>
        /// <param name="syntaxFactory"></param>
        /// <returns>An expression that creates such an object.</returns>
        public abstract ExpressionSyntax Serialize( TInput obj, ICompilationElementFactory syntaxFactory );

        protected ObjectSerializer( SyntaxSerializationService service ) : base( service ) { }

        public sealed override Type InputType => typeof(TInput);

        public sealed override Type OutputType => typeof(TOutput);
    }
}