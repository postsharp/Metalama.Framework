using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using System;
using System.Reflection;

namespace Caravela.Framework.Impl.Templating.Serialization.Reflection
{
    [Obfuscation( Exclude = true )]
    internal class CaravelaEventInfo : EventInfo
    {
        public CaravelaEventInfo( ISymbol symbol, IType containingType )
        {
            this.Symbol = symbol;
            this.ContainingType = containingType;
        }
        public static CaravelaEventInfo Create( IEvent @event )
        {
            Event fullEvent = @event as Event;
            return new CaravelaEventInfo( fullEvent.Symbol, fullEvent.ContainingElement as IType );
        }

        public ISymbol Symbol { get; }
        public IType ContainingType { get; }

        public override object[] GetCustomAttributes( bool inherit ) => throw new NotImplementedException();

        public override object[] GetCustomAttributes( Type attributeType, bool inherit ) => throw new NotImplementedException();

        public override bool IsDefined( Type attributeType, bool inherit ) => throw new NotImplementedException();

        public override Type DeclaringType => throw new NotImplementedException();
        public override string Name => throw new NotImplementedException();
        public override Type ReflectedType => throw new NotImplementedException();
        public override MethodInfo GetAddMethod( bool nonPublic ) => throw new NotImplementedException();

        public override MethodInfo GetRaiseMethod( bool nonPublic ) => throw new NotImplementedException();

        public override MethodInfo GetRemoveMethod( bool nonPublic ) => throw new NotImplementedException();

        public override EventAttributes Attributes => throw new NotImplementedException();

    }
}