using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Templating
{
    public partial class MetaSyntaxRewriter
    {
        /// <summary>
        /// Specifies how a <see cref="SyntaxNode"/> must be transformed.
        /// </summary>
        protected enum TransformationKind
        {
            /// <summary>
            /// No transformation. The original node is returned.
            /// </summary>
            None,
            
            /// <summary>
            /// The original node is cloned. This kind of transformation is currently only used
            /// to validate that the generated code is correct.
            /// </summary>
            Clone,
            
            /// <summary>
            /// The original node is transformed, i.e. the <c>Visit</c> method returns
            /// an expression that evaluates to an instance equivalent to the source one.
            /// </summary>
            Transform
        }
    }
}