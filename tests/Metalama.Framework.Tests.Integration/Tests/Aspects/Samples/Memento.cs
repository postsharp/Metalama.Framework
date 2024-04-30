using System;
using System.Collections.Generic;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;

namespace Metalama.Framework.Tests.Integration.Aspects.Samples.Memento
{
    public class MementoAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            var result = builder.Advice.IntroduceType(builder.Target, "Memento", TypeKind.Class, buildType: b => { b.Accessibility = Accessibility.Public; });

            var mementoFields = new List<IField>();
            var mementoFieldRefs = new List<IRef<IDeclaration>>();

            foreach (var fieldOrProperty in builder.Target.FieldsAndProperties)
            {
                if (fieldOrProperty is not { IsAutoPropertyOrField: true, IsImplicitlyDeclared: false })
                {
                    continue;
                }

                var fieldResult = builder.Advice.IntroduceField(
                    result.Declaration,
                    nameof(MementoField),
                    buildField: b =>
                    {
                        b.Name = fieldOrProperty.Name;
                        b.Type = fieldOrProperty.Type;
                        b.Accessibility = Accessibility.Public;
                    });

                mementoFields.Add(fieldResult.Declaration);
                mementoFieldRefs.Add(fieldResult.Declaration.ToRef());
            }

            builder.Advice.ImplementInterface(result.Declaration, typeof(IMemento));

            builder.Advice.IntroduceConstructor(
                result.Declaration,
                nameof(MementoConstructorTemplate),
                buildConstructor: b =>
                {
                    foreach (var mementoField in mementoFields)
                    {
                        b.AddParameter(mementoField.Name, mementoField.Type);
                    }
                },
                args: new { fields = mementoFieldRefs });

            builder.Advice.ImplementInterface(builder.Target, typeof(IOriginator));
        }

        [Template]
        private object? MementoField;

        [InterfaceMember]
        public IMemento Save()
        {
            var mementoType = meta.Target.Type.NestedTypes.Single();

            return 
                BuildNewExpression(
                    mementoType, 
                    meta.Target.Type.FieldsAndProperties.Where(f => f.IsAutoPropertyOrField == true && !f.IsImplicitlyDeclared))
                .Value;
        }

        public IExpression BuildNewExpression(INamedType mementoType, IEnumerable<IFieldOrProperty> fieldsOrProperties)
        {
            ExpressionBuilder expressionBuilder = new ExpressionBuilder();
            expressionBuilder.AppendVerbatim("new ");
            expressionBuilder.AppendTypeName(mementoType);
            expressionBuilder.AppendVerbatim("(");

            bool first = true;

            foreach (var fieldOrProperty in fieldsOrProperties)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    expressionBuilder.AppendVerbatim(",");
                }

                expressionBuilder.AppendExpression(fieldOrProperty);
            }

            expressionBuilder.AppendVerbatim(")");

            return expressionBuilder.ToExpression();
        }

        [InterfaceMember]
        public void Restore(IMemento memento)
        {
            var mementoType = meta.Target.Type.NestedTypes.Single();

            foreach (var fieldOrProperty in meta.Target.Type.FieldsAndProperties.Where(f => f.IsAutoPropertyOrField == true && !f.IsImplicitlyDeclared))
            {
                var mementoField = mementoType.FieldsAndProperties.OfName(fieldOrProperty.Name).Single();

                fieldOrProperty.Value = mementoField.With((IExpression)meta.Cast(mementoType, memento)).Value;
            }
        }

        [Template]
        public void MementoConstructorTemplate([CompileTime] List<IRef<IDeclaration>> fields)
        {
            int i = meta.CompileTime(0);

            foreach (var parameter in meta.Target.Constructor.Parameters)
            {
                ((IFieldOrProperty)fields[i].GetTarget(ReferenceResolutionOptions.Default)).Value = parameter;
                i++;
            }
        }
    }

    public interface IOriginator
    {
        IMemento Save();

        void Restore(IMemento memento);
    }

    public interface IMemento;

    // <target>
    [Memento]
    internal class TargetClass
    {
        private int _state1;

        private int State2 { get; set; }
    }
}
