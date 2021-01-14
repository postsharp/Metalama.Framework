using Caravela.Framework.Impl.Templating.Serialization.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating.Serialization
{
    /// <summary>
    /// Serializes objects into Roslyn creation expressions that would create those objects. You can register additional serializers with an instance of this class
    /// to support additional types.
    /// </summary>
    class ObjectSerializers
    {
        private readonly ConcurrentDictionary<Type, ObjectSerializer> _serializers = new ConcurrentDictionary<Type, ObjectSerializer>();
        private readonly EnumSerializer _enumSerializer;
        private readonly ArraySerializer _arraySerializer;

        /// <summary>
        /// Initializes a new instance of <see cref="ObjectSerializers"/>.
        /// </summary>
        public ObjectSerializers()
        {
            // Arrays, enums
            this._enumSerializer = new EnumSerializer();
            this._arraySerializer = new ArraySerializer(this);
            
            // Primitive types
            this.RegisterSerializer( typeof(char),   new CharSerializer() );
            this.RegisterSerializer( typeof(bool),   new BoolSerializer() );
            this.RegisterSerializer( typeof(byte),   new ByteSerializer() );
            this.RegisterSerializer( typeof(sbyte),  new SByteSerializer() );
            this.RegisterSerializer( typeof(ushort), new UShortSerializer() );
            this.RegisterSerializer( typeof(short),  new ShortSerializer() );
            this.RegisterSerializer( typeof(uint),   new UIntSerializer() );
            this.RegisterSerializer( typeof(int),    new IntSerializer() );
            this.RegisterSerializer( typeof(ulong),  new ULongSerializer() );
            this.RegisterSerializer( typeof(long),   new LongSerializer() );
            this.RegisterSerializer( typeof(float),  new FloatSerializer() );
            this.RegisterSerializer( typeof(double), new DoubleSerializer() );
            this.RegisterSerializer( typeof(decimal),new DecimalSerializer() );
            this.RegisterSerializer( typeof(UIntPtr),new UIntPtrSerializer() );
            this.RegisterSerializer( typeof(IntPtr), new IntPtrSerializer() );
            
            // String
            this.RegisterSerializer( typeof(string), new StringSerializer() );
            
            // Known simple system types
            this.RegisterSerializer( typeof(DateTime), new DateTimeSerializer() );
            this.RegisterSerializer( typeof(Guid), new GuidSerializer() );
            this.RegisterSerializer( typeof(TimeSpan), new TimeSpanSerializer() );
            this.RegisterSerializer( typeof(DateTimeOffset), new DateTimeOffsetSerializer() );
            this.RegisterSerializer( typeof(CultureInfo), new CultureInfoSerializer() ); 
            
            // Collections
            this.RegisterSerializer( typeof(List<>), new ListSerializer(this) );
            this.RegisterSerializer( typeof(Dictionary<,>), new DictionarySerializer(this) ); 
            
            // Reflection types
            CaravelaTypeSerializer typeSerializer = new CaravelaTypeSerializer();
            CaravelaMethodInfoSerializer methodInfoSerializer = new CaravelaMethodInfoSerializer(typeSerializer);
            this.RegisterSerializer( typeof(CaravelaType), typeSerializer );
            this.RegisterSerializer( typeof(CaravelaMethodInfo), methodInfoSerializer );
            this.RegisterSerializer( typeof(CaravelaConstructorInfo), new CaravelaConstructorInfoSerializer(typeSerializer) );
            this.RegisterSerializer( typeof(CaravelaEventInfo), new CaravelaEventInfoSerializer(typeSerializer) );
            this.RegisterSerializer( typeof(CaravelaParameterInfo), new CaravelaParameterInfoSerializer(methodInfoSerializer) );
            this.RegisterSerializer( typeof(CaravelaReturnParameterInfoSerializer), new CaravelaReturnParameterInfoSerializer(methodInfoSerializer) );
            this.RegisterSerializer( typeof(CaravelaLocationInfo), new CaravelaLocationInfoSerializer(this, typeSerializer) );
        }
        
        /// <summary>
        /// Registers an additional serializer. See Remarks for generics.
        /// </summary>
        /// <remarks>
        /// For generic types, register the type without generic arguments, for example "List&lt;&gt;" rather than "List&lt;int&gt;". The serializer will handle
        /// lists of any element.
        /// </remarks>
        /// <param name="type">The specific type that this serializer supports. It will be called for all objects that are of this type exactly.</param>
        /// <param name="serializer">A new serializer that supports that type.</param>
        public void RegisterSerializer( Type type, ObjectSerializer serializer )
        {
            this._serializers.TryAdd( type, serializer );
        }
        
        /// <summary>
        /// Serializes an object into a Roslyn expression that would create it. For example, serializes a list containing "4" and "8" into <c>new System.Collections.Generic.List&lt;System.Int32&gt;{4, 8}</c>.
        /// </summary>
        /// <param name="o">An object to serialize.</param>
        /// <returns>An expression that would create the object.</returns>
        /// <exception cref="CaravelaException">When the object can't be serialized, for example if it's of an unsupported type.</exception>
        public ExpressionSyntax SerializeToRoslynCreationExpression( object? o )
        {
            if ( o == null )
            {
                return LiteralExpression( SyntaxKind.NullLiteralExpression );
            }
            if ( o is Enum e )
            {
                return this._enumSerializer.Serialize( e );
            }

            if ( o is Array a )
            {
                return this._arraySerializer.Serialize( a );
            }
            Type t = o.GetType();
            Type mainType;
            if ( t.IsGenericType )
            {
                mainType = t.GetGenericTypeDefinition();
            }
            else
            {
                mainType = t;
            }
            if (!this._serializers.TryGetValue( mainType, out ObjectSerializer serializer))
            {
                throw new CaravelaException( GeneralDiagnosticDescriptors.UnsupportedSerialization, mainType );
            }
            return serializer.SerializeObject( o );
        }
    }
}