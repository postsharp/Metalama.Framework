// unset

using System;
using Caravela.Framework.Code;

namespace Caravela.Framework.Impl.Transformations
{
    internal sealed class ParameterBuilder : CodeElementBuilder, IParameterBuilder
    {
        private bool _hasDefaultValue;
        private object? _defaultValue;

        public RefKind RefKind { get; }

        public IType Type { get; set; }

        public string? Name { get; }

        public int Index { get; }

        public OptionalValue DefaultValue { get; set; }

        public bool IsParams { get; set; }

        public override ICodeElement? ContainingElement { get; }

        public override CodeElementKind ElementKind => CodeElementKind.Parameter;

        public ParameterBuilder( IMethod containingMethod, int index, string name, IType type, RefKind refKind ) : base()
        {
            this.ContainingElement = containingMethod;
            this.Index = index;
            this.Name = name;
            this.Type = type;
            this.RefKind = refKind;
        }

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
        {
            throw new NotImplementedException();
        }

        public override bool Equals( ICodeElement other ) => throw new NotImplementedException();
    }
}