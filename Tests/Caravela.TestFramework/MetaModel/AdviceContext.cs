using System.Threading;
using Caravela.Framework.Project;

namespace Caravela.TestFramework.MetaModel
{
    [CompileTime]
    public static class AdviceContext
    {
        private static readonly AsyncLocal<IAdviceContext> _current = new AsyncLocal<IAdviceContext>();

        internal static IAdviceContext Current
        {
            get => _current.Value;
            set => _current.Value = value;
        }

        public static IMethodAdviceContext Method => Current.MethodAdviceContext;


        [Proceed]
        public static dynamic Proceed() => Current.ProceedImpl;

        public static T BuildTime<T>( T expression ) => expression;
    }
}