// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.SyntaxSerialization;
using System;
using System.Reflection;

namespace Metalama.Framework.Engine.ReflectionMocks
{
    internal sealed class CompileTimeParameterInfo : ParameterInfo, ICompileTimeReflectionObject<IParameter>
    {
        public ISdkRef<IParameter> Target { get; }

        private CompileTimeParameterInfo( IParameter parameter )
        {
            this.Target = parameter.ToTypedRef();
        }

        public static ParameterInfo Create( IParameter parameter ) => new CompileTimeParameterInfo( parameter );

        public bool IsAssignable => false;

        public IType Type => TypeFactory.GetType( typeof(ParameterInfo) );

        public Type ReflectionType => typeof(ParameterInfo);

        public RefKind RefKind => RefKind.None;

        public ref object? Value => ref RefHelper.Wrap( this );

        public TypedExpressionSyntax ToTypedExpressionSyntax( ISyntaxGenerationContext syntaxGenerationContext )
            => CompileTimeMocksHelper.ToTypedExpressionSyntax( this, CompileTimeParameterInfoSerializer.SerializeParameter, syntaxGenerationContext );
    }
}