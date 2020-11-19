using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating.Serialization
{
    public class ObjectSerializers
    {
        private readonly ConcurrentDictionary<Type, ObjectSerializer> _serializers = new ConcurrentDictionary<Type, ObjectSerializer>();
        private readonly EnumSerializer _enumSerializer;
        private readonly ArraySerializer _arraySerializer;

        public ObjectSerializers()
        {
            // Arrays, enums
            this._enumSerializer = new EnumSerializer();
            this._arraySerializer = new ArraySerializer(this);
            
            // Primitive types
            this._serializers.TryAdd( typeof(char),   new CharSerializer() );
            this._serializers.TryAdd( typeof(bool),   new BoolSerializer() );
            this._serializers.TryAdd( typeof(byte),   new ByteSerializer() );
            this._serializers.TryAdd( typeof(sbyte),  new SByteSerializer() );
            this._serializers.TryAdd( typeof(ushort), new UShortSerializer() );
            this._serializers.TryAdd( typeof(short),  new ShortSerializer() );
            this._serializers.TryAdd( typeof(uint),   new UIntSerializer() );
            this._serializers.TryAdd( typeof(int),    new IntSerializer() );
            this._serializers.TryAdd( typeof(ulong),  new ULongSerializer() );
            this._serializers.TryAdd( typeof(long),   new LongSerializer() );
            this._serializers.TryAdd( typeof(float),  new FloatSerializer() );
            this._serializers.TryAdd( typeof(double), new DoubleSerializer() );
            this._serializers.TryAdd( typeof(decimal),new DecimalSerializer() );
            this._serializers.TryAdd( typeof(UIntPtr),new UIntPtrSerializer() );
            this._serializers.TryAdd( typeof(IntPtr), new IntPtrSerializer() );
            
            // String
            this._serializers.TryAdd( typeof(string), new StringSerializer() );
            
            // Known simple system types
            this._serializers.TryAdd( typeof(DateTime), new DateTimeSerializer() );
            this._serializers.TryAdd( typeof(Guid), new GuidSerializer() );
            this._serializers.TryAdd( typeof(TimeSpan), new TimeSpanSerializer() );
            this._serializers.TryAdd( typeof(CultureInfo), new CultureInfoSerializer() ); 
            
            // Collections
            this._serializers.TryAdd( typeof(List<>), new ListSerializer(this) );
            this._serializers.TryAdd( typeof(Dictionary<,>), new DictionarySerializer(this) ); 
            
            // Reflection types
            // TODO reflection types
        }

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