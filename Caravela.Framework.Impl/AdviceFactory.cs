using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Impl.Transformations;
using Caravela.Reactive;
using System.Linq;

namespace Caravela.Framework.Impl
{
    class AdviceFactory : IAdviceFactory
    {
        private readonly ICompilation _compilation;
        private readonly CompileTimeAssemblyLoader _compileTimeAssemblyLoader;
        readonly INamedType _templateType;

        public AdviceFactory( ICompilation compilation, CompileTimeAssemblyLoader compileTimeAssemblyLoader, INamedType templateType )
        {
            this._compilation = compilation;
            this._compileTimeAssemblyLoader = compileTimeAssemblyLoader;
            this._templateType = templateType;
        }

        public IOverrideMethodAdvice OverrideMethod( IMethod targetMethod, string defaultTemplate )
        {
            var templateMethod = this._templateType.AllMethods.Where( m => m.Name == defaultTemplate ).GetValue().Single();

            var aspect = this._compileTimeAssemblyLoader.CreateInstance( ((INamedType) templateMethod.ContainingElement!).GetSymbol() );

            string templateMethodName = templateMethod.Name + TemplateCompiler.TemplateMethodSuffix;

            var methodBody = new TemplateDriver( aspect.GetType().GetMethod( templateMethodName ) ).ExpandDeclaration( aspect, targetMethod, this._compilation );

            return new OverrideMethodAdvice( targetMethod, new OverriddenMethod( targetMethod, methodBody ) );
        }
    }
}