// unset

using System;
using System.Collections.Generic;
using Caravela.Framework.Code;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MethodKind = Caravela.Framework.Code.MethodKind;
using RefKind = Caravela.Framework.Code.RefKind;

namespace Caravela.Framework.Impl.Transformations
{
    internal sealed class MethodTransformationBuilder : MemberTransformationBuilder, IMethodBuilder
    {

        private readonly List<ParameterBuilder> _parameters = new List<ParameterBuilder>();

        private readonly List<GenericParameterBuilder> _genericParameters = new List<GenericParameterBuilder>();

        public IParameterBuilder AddParameter( string name, IType type, RefKind refKind = RefKind.None, OptionalValue optionalValue = default )
        {
            var parameter = new ParameterBuilder( this, this._parameters.Count, name, type, refKind );
            parameter.DefaultValue = optionalValue;
            this._parameters.Add( parameter );
            return parameter;
        }

        public IGenericParameterBuilder AddGenericParameter( string name ) => throw new NotImplementedException();

        IParameterBuilder? IMethodBuilder.ReturnParameter => this.ReturnParameter;

        IType? IMethodBuilder.ReturnType
        {
            get => this.ReturnParameter?.Type;
            set
            {
                if ( this.ReturnParameter == null )
                {
                    throw new InvalidOperationException();
                }
                else if ( value == null )
                {
                    throw new ArgumentNullException( nameof( value ) );
                }

                this.ReturnParameter.Type = value;
            }
        }

        IType IMethod.ReturnType => this.ReturnParameter?.Type;

        public ParameterBuilder? ReturnParameter { get; }

        IParameter? IMethod.ReturnParameter => this.ReturnParameter;

        IReadOnlyList<IMethod> IMethod.LocalFunctions => this.LocalFunctions;

        IReadOnlyList<IParameter> IMethod.Parameters => this._parameters;

        IReadOnlyList<IGenericParameter> IMethod.GenericParameters => this._genericParameters;

        IReadOnlyList<IType> IMethod.GenericArguments => throw new NotImplementedException();

        bool IMethod.IsOpenGeneric => throw new NotImplementedException();

        public IReadOnlyList<IMethod> LocalFunctions => Array.Empty<IMethod>();

        // We don't currently support adding other methods than default ones.
        public MethodKind MethodKind => MethodKind.Default;
        IMethod IMethod.WithGenericArguments( params IType[] genericArguments ) => throw new NotImplementedException();

        bool IMethod.HasBase => throw new NotImplementedException();

        IMethodInvocation IMethod.Base => throw new NotImplementedException();

        public override CodeElementKind ElementKind => CodeElementKind.Method;

        public MethodTransformationBuilder( INamedType targetType, IMethod templateMethod, string name )
            : base( targetType )
        {
            this.Name = name;
        }

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => throw new NotImplementedException();

        public override bool Equals( ICodeElement other ) => throw new NotImplementedException();

        public override IEnumerable<IntroducedMember> GetIntroducedMembers() => throw new NotImplementedException();

        public override MemberDeclarationSyntax InsertPositionNode => throw new NotImplementedException();
        dynamic IMethodInvocation.Invoke( dynamic? instance, params dynamic[] args ) => throw new NotImplementedException();
    }
}