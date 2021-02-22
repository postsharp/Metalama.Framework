using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Serialization.Reflection;
using Microsoft.CodeAnalysis;
using System;
using System.Reflection;

namespace Caravela.Framework.Impl.ReflectionMocks
{
    internal class CompileTimeEventInfo : EventInfo, IReflectionMockMember
    {
        public CompileTimeEventInfo( ISymbol symbol, IType containingType )
        {
            this.Symbol = symbol;
            this.ContainingType = containingType;
        }

        public static CompileTimeEventInfo Create( IEvent @event )
        {
            var fullEvent = (Event) @event;
            return new CompileTimeEventInfo( fullEvent.Symbol, fullEvent.DeclaringType );
        }

        public ISymbol Symbol { get; }

        public IType ContainingType { get; }

        public override object[] GetCustomAttributes( bool inherit ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override object[] GetCustomAttributes( Type attributeType, bool inherit ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override bool IsDefined( Type attributeType, bool inherit ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override Type DeclaringType => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override string Name => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override Type ReflectedType => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override MethodInfo GetAddMethod( bool nonPublic ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override MethodInfo GetRaiseMethod( bool nonPublic ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override MethodInfo GetRemoveMethod( bool nonPublic ) => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public override EventAttributes Attributes => throw CompileTimeMocksHelper.CreateNotSupportedException();

        public ITypeSymbol? DeclaringTypeSymbol => throw new NotImplementedException();
    }
}