using System;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Impl.Templating.MetaModel;
using Caravela.Framework.Impl.Transformations;
using Caravela.Framework.Sdk;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl
{
    class AspectDriver : IAspectDriver
    {
        public INamedType AspectType { get; }

        private readonly ICompilation _compilation;
        private readonly CompileTimeAssemblyLoader _compileTimeAssemblyLoader;

        private readonly IReactiveCollection<(IAttribute attribute, IMethod method)> _declarativeAdviceAttributes;

        public AspectDriver( INamedType aspectType, ICompilation compilation, CompileTimeAssemblyLoader compileTimeAssemblyLoader )
        {
            this.AspectType = aspectType;

            this._compilation = compilation;
            this._compileTimeAssemblyLoader = compileTimeAssemblyLoader;

            var iAdviceAttribute = compilation.GetTypeByReflectionType( typeof( IAdviceAttribute ) ).AssertNotNull();

            this._declarativeAdviceAttributes =
                from method in aspectType.AllMethods
                from attribute in method.Attributes
                where attribute.Type.Is( iAdviceAttribute )
                select (attribute, method);
        }

        internal AspectInstanceResult EvaluateAspect( AspectInstance aspectInstance )
        {
            var aspect = aspectInstance.Aspect;

            return aspectInstance.CodeElement switch
            {
                INamedType type => this.EvaluateAspect( type, aspect ),
                IMethod method => this.EvaluateAspect( method, aspect )
            };
        }

        private AspectInstanceResult EvaluateAspect<T>( T codeElement, IAspect aspect )
            where T : ICodeElement
        {
            if (aspect is not IAspect<T> aspectOfT)
            {
                // TODO: should the diagnostic be applied to the attribute, if one exists?
                var diagnostic = Diagnostic.Create(
                    GeneralDiagnosticDescriptors.AspectAppliedToIncorrectElement, codeElement.GetSyntaxNode().GetLocation(), this.AspectType, codeElement, codeElement.Kind );

                return new( ImmutableArray.Create( diagnostic ), ImmutableArray.Create<AdviceInstance>(), ImmutableArray.Create<AspectInstance>() );
            }

            var declarativeAdvices = this._declarativeAdviceAttributes.GetValue().Select( x => this.CreateDeclarativeAdvice( codeElement, x.attribute, x.method ) );

            var aspectBuilder = new AspectBuilder<T>( codeElement, declarativeAdvices );

            aspectOfT.Initialize( aspectBuilder );

            return aspectBuilder.ToResult();
        }

        public const string OriginalMemberSuffix = "_Original";

        private AdviceInstance CreateDeclarativeAdvice<T>( T codeElement, IAttribute attribute, IMethod templateMethod ) where T : ICodeElement
        {
            var overrideMethodTemplateAttribute = this._compilation.GetTypeByReflectionType( typeof( OverrideMethodTemplateAttribute ) ).AssertNotNull();

            if ( attribute.Type.Is( overrideMethodTemplateAttribute ) )
            {
                // TODO: is it possible that codeElement is not IMethod?
                var targetMethod = (IMethod) codeElement;

                // TODO: this should probably all be cached somewhere, possibly as a Func

                // TODO: diagnostic if templateMethod is a local function
                var aspect = this._compileTimeAssemblyLoader.CreateInstance( ((INamedType) templateMethod.ContainingElement!).GetSymbol() );

                string templateMethodName = templateMethod.Name + TemplateCompiler.TemplateMethodSuffix;

                // TODO: fully set up TemplateContext
                var proceed = new ProceedImpl( (MethodDeclarationSyntax) targetMethod.GetSyntaxNode() );
                var templateContext = new TemplateContextImpl( targetMethod, targetMethod.DeclaringType!, this._compilation );

                var methodBody = new TemplateDriver( aspect.GetType().GetMethod( templateMethodName ) ).ExpandDeclaration( aspect, proceed, templateContext );

                return new( new OverrideMethodAdvice( targetMethod, new OverriddenMethod( targetMethod, methodBody ) ) );
            }
            else
                throw new NotImplementedException();
        }
    }
}