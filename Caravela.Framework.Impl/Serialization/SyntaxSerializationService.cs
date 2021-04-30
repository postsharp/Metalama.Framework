// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.ReflectionMocks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Serialization
{
    /// <summary>
    /// Serializes objects into Roslyn creation expressions that would create those objects. You can register additional serializers with an instance of this class
    /// to support additional types.
    /// </summary>
    internal class SyntaxSerializationService
    {
        private readonly ConcurrentDictionary<Type, ObjectSerializer> _serializers = new();
        private readonly ArraySerializer _arraySerializer;
        private readonly EnumSerializer _enumSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxSerializationService"/> class.
        /// </summary>
        public SyntaxSerializationService()
        {
            // Arrays, enums
            this._arraySerializer = new ArraySerializer( this );
            this._enumSerializer = new EnumSerializer( this );

            // Primitive types
            this.RegisterSerializer( typeof(char), new CharSerializer( this ) );
            this.RegisterSerializer( typeof(bool), new BoolSerializer( this ) );
            this.RegisterSerializer( typeof(byte), new ByteSerializer( this ) );
            this.RegisterSerializer( typeof(sbyte), new SByteSerializer( this ) );
            this.RegisterSerializer( typeof(ushort), new UShortSerializer( this ) );
            this.RegisterSerializer( typeof(short), new ShortSerializer( this ) );
            this.RegisterSerializer( typeof(uint), new UIntSerializer( this ) );
            this.RegisterSerializer( typeof(int), new IntSerializer( this ) );
            this.RegisterSerializer( typeof(ulong), new ULongSerializer( this ) );
            this.RegisterSerializer( typeof(long), new LongSerializer( this ) );
            this.RegisterSerializer( typeof(float), new FloatSerializer( this ) );
            this.RegisterSerializer( typeof(double), new DoubleSerializer( this ) );
            this.RegisterSerializer( typeof(decimal), new DecimalSerializer( this ) );
            this.RegisterSerializer( typeof(UIntPtr), new UIntPtrSerializer( this ) );
            this.RegisterSerializer( typeof(IntPtr), new IntPtrSerializer( this ) );

            // String
            this.RegisterSerializer( typeof(string), new StringSerializer( this ) );

            // Known simple system types
            this.RegisterSerializer( typeof(DateTime), new DateTimeSerializer( this ) );
            this.RegisterSerializer( typeof(Guid), new GuidSerializer( this ) );
            this.RegisterSerializer( typeof(TimeSpan), new TimeSpanSerializer( this ) );
            this.RegisterSerializer( typeof(DateTimeOffset), new DateTimeOffsetSerializer( this ) );
            this.RegisterSerializer( typeof(CultureInfo), new CultureInfoSerializer( this ) );

            // Collections
            this.RegisterSerializer( typeof(List<>), new ListSerializer( this ) );
            this.RegisterSerializer( typeof(Dictionary<,>), new DictionarySerializer( this ) );

            // Reflection types
            this.TypeSerializer = new TypeSerializer( this );
            this.CompileTimeMethodInfoSerializer = new CompileTimeMethodInfoSerializer( this );
            this.CompileTimePropertyInfoSerializer = new CompileTimePropertyInfoSerializer( this );
            this.RegisterSerializer( typeof(CompileTimeType), this.TypeSerializer );
            this.RegisterSerializer( typeof(CompileTimeMethodInfo), this.CompileTimeMethodInfoSerializer );
            this.RegisterSerializer( typeof(CompileTimePropertyInfo), this.CompileTimePropertyInfoSerializer );
            this.RegisterSerializer( typeof(CompileTimeConstructorInfo), new CompileTimeConstructorInfoSerializer( this ) );
            this.RegisterSerializer( typeof(CompileTimeEventInfo), new CompileTimeEventInfoSerializer( this ) );
            this.RegisterSerializer( typeof(CompileTimeParameterInfo), new CompileTimeParameterInfoSerializer( this ) );
            this.RegisterSerializer( typeof(CompileTimeReturnParameterInfo), new CompileTimeReturnParameterInfoSerializer( this ) );
            this.RegisterSerializer( typeof(CompileTimeFieldOrPropertyInfo), new CompileTimeFieldOrPropertyInfoSerializer( this ) );
        }

        internal TypeSerializer TypeSerializer { get; }

        internal CompileTimeMethodInfoSerializer CompileTimeMethodInfoSerializer { get; }
        
        internal CompileTimePropertyInfoSerializer CompileTimePropertyInfoSerializer { get; }

        /// <summary>
        /// Registers an additional serializer. See Remarks for generics.
        /// </summary>
        /// <remarks>
        /// For generic types, register the type without generic arguments, for example "List&lt;&gt;" rather than "List&lt;int&gt;". The serializer will handle
        /// lists of any element.
        /// </remarks>
        /// <param name="implementationType">The specific type that this serializer supports. It will be called for all objects that are of this type exactly.</param>
        /// <param name="serializer">A new serializer that supports that type.</param>
        public void RegisterSerializer( Type implementationType, ObjectSerializer serializer )
        {
            this.RegisterSerializer( implementationType, implementationType, serializer );
        }

        /// <summary>
        /// Registers an additional serializer. See Remarks for generics.
        /// </summary>
        /// <remarks>
        /// For generic types, register the type without generic arguments, for example "List&lt;&gt;" rather than "List&lt;int&gt;". The serializer will handle
        /// lists of any element.
        /// </remarks>
        /// <param name="contractType"></param>
        /// <param name="implementationType">The specific type that this serializer supports. It will be called for all objects that are of this type exactly.</param>
        /// <param name="serializer">A new serializer that supports that type.</param>
        public void RegisterSerializer( Type contractType, Type implementationType, ObjectSerializer serializer )
        {
            this._serializers[implementationType] = serializer;
        }

        public ObjectSerializer? GetSerializer( object o )
        {
            switch ( o )
            {
                case Enum:
                    return this._enumSerializer;

                case Array:
                    return this._arraySerializer;

                default:
                    var t = o.GetType();
                    Type mainType;

                    if ( t.IsGenericType )
                    {
                        mainType = t.GetGenericTypeDefinition();
                    }
                    else
                    {
                        mainType = t;
                    }

                    _ = this._serializers.TryGetValue( mainType, out var serializer );

                    return serializer;
            }
        }

        /// <summary>
        /// Serializes an object into a Roslyn expression that would create it. For example, serializes a list containing "4" and "8" into <c>new System.Collections.Generic.List&lt;System.Int32&gt;{4, 8}</c>.
        /// </summary>
        /// <param name="o">An object to serialize.</param>
        /// <param name="syntaxFactory"></param>
        /// <returns>An expression that would create the object.</returns>
        /// <exception cref="InvalidUserCodeException">When the object cannot be serialized, for example if it's of an unsupported type.</exception>
        public ExpressionSyntax Serialize( object? o, ISyntaxFactory syntaxFactory )
        {
            if ( o == null )
            {
                return LiteralExpression( SyntaxKind.NullLiteralExpression );
            }

            var serializer = this.GetSerializer( o );

            if ( serializer == null )
            {
                throw SerializationDiagnosticDescriptors.UnsupportedSerialization.CreateException( o.GetType() );
            }

            return serializer.Serialize( o, syntaxFactory );
        }
    }
}