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
            var mementoType =
                builder.Advice.IntroduceType(
                    builder.Target,
                    "Memento",
                    TypeKind.Class,
                    buildType: b =>
                    {
                        b.Accessibility = Accessibility.Public;
                    }).Declaration;

            var mementoFields = new List<IField>();

            foreach (var fieldOrProperty in builder.Target.FieldsAndProperties)
            {
                if (fieldOrProperty is not { IsAutoPropertyOrField: true, IsImplicitlyDeclared: false })
                {
                    continue;
                }

                var fieldResult = builder.Advice.IntroduceField(
                    mementoType,
                    nameof(MementoField),
                    buildField: b =>
                    {
                        b.Name = fieldOrProperty.Name;
                        b.Type = fieldOrProperty.Type;
                        b.Accessibility = Accessibility.Public;
                    });

                mementoFields.Add(fieldResult.Declaration);
            }

            builder.Advice.ImplementInterface(mementoType, typeof(IMemento));

            builder.Advice.IntroduceConstructor(
                mementoType,
                nameof(MementoConstructorTemplate),
                buildConstructor: b =>
                {
                    foreach (var mementoField in mementoFields)
                    {
                        b.AddParameter(mementoField.Name, mementoField.Type);
                    }
                },
                args: new { fields = mementoFields });

            builder.Advice.ImplementInterface(builder.Target, typeof(IOriginator), tags: new { mementoType = mementoType });
        }

        [Template]
        private object? MementoField;

        [InterfaceMember]
        public IMemento Save()
        {
            var mementoType = (INamedType)meta.Tags["mementoType"];

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
            var mementoType = (INamedType)meta.Tags["mementoType"];

            foreach (var fieldOrProperty in meta.Target.Type.FieldsAndProperties.Where(f => f.IsAutoPropertyOrField == true && !f.IsImplicitlyDeclared))
            {
                var mementoField = mementoType.FieldsAndProperties.OfName(fieldOrProperty.Name).Single();

                fieldOrProperty.Value = mementoField.With((IExpression)meta.Cast(mementoType, memento)).Value;
            }
        }

        [Template]
        public void MementoConstructorTemplate([CompileTime] List<IField> fields)
        {
            int i = meta.CompileTime(0);

            foreach (var parameter in meta.Target.Constructor.Parameters)
            {
                fields[i].Value = parameter;
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
