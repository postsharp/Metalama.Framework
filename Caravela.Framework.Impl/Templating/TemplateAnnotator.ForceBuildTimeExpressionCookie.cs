using System;

namespace PostSharp.Caravela.AspectWorkbench
{
    internal partial class TemplateAnnotator
    {
        private readonly struct ForceBuildTimeExpressionCookie : IDisposable
        {
            private readonly TemplateAnnotator _parent;
            private readonly bool _initialValue;

            public ForceBuildTimeExpressionCookie(TemplateAnnotator parent, bool initialValue)
            {
                this._parent = parent;
                this._initialValue = initialValue;
            }

            public void Dispose()
            {
                this._parent._forceCompileTimeOnlyExpression = this._initialValue;
            }
        }
    }
}