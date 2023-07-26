// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Engine.Utilities.UserCode;
using System.Reflection;

namespace Metalama.Framework.Engine.ReflectionMocks
{
    internal sealed class CompileTimeReturnParameterInfo : ParameterInfo, ICompileTimeReflectionObject<IParameter>
    {
        public ISdkRef<IParameter> Target { get; }

        private CompileTimeReturnParameterInfo( IParameter returnParameter )
        {
            this.Target = returnParameter.ToTypedRef();
        }

        public static ParameterInfo Create( IParameter returnParameter )
        {
            return new CompileTimeReturnParameterInfo( returnParameter );
        }

        public bool IsAssignable => false;

        public IType Type => TypeFactory.GetType( typeof(ParameterInfo) );

        public RefKind RefKind => RefKind.None;

        public ref object? Value => ref RefHelper.Wrap( this );

        public TypedExpressionSyntax ToTypedExpressionSyntax( ISyntaxGenerationContext syntaxGenerationContext )
        {
            var generationContext = (SyntaxGenerationContext) syntaxGenerationContext;

            var compilation = UserCodeExecutionContext.Current.Compilation.AssertNotNull();

            var expression = CompileTimeReturnParameterInfoSerializer.SerializeParameter(
                this.Target.GetTarget( compilation ),
                new( compilation, generationContext ) );

            return new(
                new TypedExpressionSyntaxImpl(
                    expression,
                    this.Type,
                    generationContext,
                    true ) );
        }
    }
}