// unset

using Caravela.Framework.Code;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Transformations
{
    internal sealed class MethodBuilder : MemberBuilder, IMethodBuilder
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
                    throw new ArgumentNullException( nameof(value) );
                }
                
                this.ReturnParameter.Type = value;
            }
        }

        IType IMethod.ReturnType => this.ReturnParameter?.Type;

        [Memo]
        public ParameterBuilder? ReturnParameter { get;}


        IParameter? IMethod.ReturnParameter => this.ReturnParameter;
        
        IReadOnlyList<IMethod> IMethod.LocalFunctions => this.LocalFunctions;
        IReadOnlyList<IParameter> IMethod.Parameters => this._parameters;

        IReadOnlyList<IGenericParameter> IMethod.GenericParameters => this._genericParameters;

     
        public IReadOnlyList<IMethod> LocalFunctions => Array.Empty<IMethod>();


        // We don't currently support adding other methods than default ones.
        public MethodKind MethodKind => MethodKind.Default;

        public override CodeElementKind ElementKind => CodeElementKind.Method;

        public MethodBuilder( INamedType targetType, IMethod templateMethod, string name )
            : base( targetType )
        {
            this.Name = name;
        }

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => throw new NotImplementedException();
        public override bool Equals( ICodeElement other ) => throw new NotImplementedException();
        protected override void ForEachChild( Action<CodeElementBuilder> action ) => throw new NotImplementedException();
        public override MemberDeclarationSyntax GenerateMember()
        {
            // TODO: Generate the method HERE with an implementation that just throws NotImplementedException.
            throw new NotImplementedException();
        }
    }
}