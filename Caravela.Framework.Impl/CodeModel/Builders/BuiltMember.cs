// unset

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Links;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal abstract class BuiltMember : BuiltCodeElement, IMember, IMemberLink<IMember>
    {
        protected BuiltMember( CompilationModel compilation ) : base( compilation )
        {
        }

        public abstract MemberBuilder MemberBuilder { get; }

        public Accessibility Accessibility => this.MemberBuilder.Accessibility;

        public string Name => this.MemberBuilder.Name;

        public bool IsAbstract => this.MemberBuilder.IsAbstract;

        public bool IsStatic => this.MemberBuilder.IsStatic;

        public bool IsVirtual => this.MemberBuilder.IsVirtual;

        public bool IsSealed => this.MemberBuilder.IsSealed;

        public bool IsReadOnly => this.MemberBuilder.IsReadOnly;

        public bool IsOverride => this.MemberBuilder.IsOverride;

        public bool IsNew => this.MemberBuilder.IsNew;

        public bool IsAsync => this.MemberBuilder.IsAsync;

        public INamedType DeclaringType => this.Compilation.Factory.GetCodeElement( this.MemberBuilder.DeclaringType );

        IMember ICodeElementLink<IMember>.GetForCompilation( CompilationModel compilation ) => (IMember) this.GetForCompilation( compilation );

        public object? Target => throw new System.NotImplementedException();
    }
}