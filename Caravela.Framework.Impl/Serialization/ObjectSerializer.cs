// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Runtime.CompilerServices;

namespace Caravela.Framework.Impl.Serialization
{
    /// <summary>
    /// An object serializer can be registered with <see cref="SyntaxSerializationService"/> to serialize objects of a specific type into Roslyn creation expressions.
    /// </summary>
    internal abstract class ObjectSerializer
    {
        public SyntaxSerializationService Service { get; }

        protected ObjectSerializer( SyntaxSerializationService service )
        {
            this.Service = service;
        }

        /// <summary>
        /// Serializes an object of a type supported by this object serializer into a Roslyn expression that creates such an object.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="syntaxFactory"></param>
        /// <returns>An expression that creates such an object.</returns>
        public abstract ExpressionSyntax Serialize( object obj, ISyntaxFactory syntaxFactory );

        public virtual bool CanSerializeType( ITypeSymbol type, Location diagnosticLocation, IDiagnosticAdder diagnosticAdder ) => true;

        /// <summary>
        /// Throws a <see cref="InvalidUserCodeException"/> if we are in an infinite recursion cycle because of an attempt to serialize <paramref name="obj"/>.
        /// </summary>
        protected static void ThrowIfStackTooDeep( object obj )
        {
            try
            {
                RuntimeHelpers.EnsureSufficientExecutionStack();
            }
            catch
            {
                throw SerializationDiagnosticDescriptors.CycleInSerialization.CreateException( obj.GetType() );
            }
        }
    }
}