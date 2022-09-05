// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Metalama.Framework.Engine.SyntaxSerialization
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
        /// <param name="serializationContext"></param>
        /// <returns>An expression that creates such an object.</returns>
        public abstract ExpressionSyntax Serialize( object obj, SyntaxSerializationContext serializationContext );

        /// <summary>
        /// Throws a <see cref="DiagnosticException"/> if we are in an infinite recursion cycle because of an attempt to serialize <paramref name="obj"/>.
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

        public abstract Type InputType { get; }

        /// <summary>
        /// Gets the run-time type, or <c>null</c> if the type is unknown by the serializer, then it is assumed to be
        /// the same as the input type.
        /// </summary>
        public abstract Type? OutputType { get; }

        public virtual ImmutableArray<Type> AdditionalSupportedTypes => ImmutableArray<Type>.Empty;

        public ImmutableArray<Type> AllSupportedTypes
        {
            get
            {
                var types = this.AdditionalSupportedTypes.Add( this.InputType );

                if ( this.OutputType != null )
                {
                    types = types.Add( this.OutputType );
                }

                return types;
            }
        }

        public virtual int Priority => 0;
    }
}