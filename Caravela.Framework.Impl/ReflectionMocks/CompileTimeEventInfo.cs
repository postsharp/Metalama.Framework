// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CodeModel.References;
using System;
using System.Reflection;

namespace Caravela.Framework.Impl.ReflectionMocks
{
    internal class CompileTimeEventInfo : EventInfo, ICompileTimeReflectionObject<IEvent>
    {
        public ISdkRef<IEvent> Target { get; }

        public CompileTimeEventInfo( IEvent @event )
        {
            this.Target = @event.ToTypedRef();
        }

        public static CompileTimeEventInfo Create( IEvent @event )
        {
            return new CompileTimeEventInfo( @event );
        }

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
    }
}