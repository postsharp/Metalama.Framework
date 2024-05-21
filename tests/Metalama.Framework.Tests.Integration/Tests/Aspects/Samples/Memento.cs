using System;
using System.Collections.Generic;
using System.Linq;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;

#pragma warning disable CS0169, CS0649

namespace Metalama.Framework.Tests.Integration.Aspects.Samples.Memento
{
    public class MementoAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            var mementoType =
                builder.IntroduceClass(
                    "Memento",
                    buildType: b => { b.Accessibility = Accessibility.Public; } );

            var mementoFields = new List<IField>();

            foreach (var fieldOrProperty in builder.Target.FieldsAndProperties)
            {
                if (fieldOrProperty is not { IsAutoPropertyOrField: true, IsImplicitlyDeclared: false })
                {
                    continue;
                }

                var field = mementoType.IntroduceField(
                    nameof(MementoField),
                    buildField: b =>
                    {
                        b.Name = fieldOrProperty.Name;
                        b.Type = fieldOrProperty.Type;
                        b.Accessibility = Accessibility.Public;
                        b.Writeability = Writeability.ConstructorOnly;
                    } );

                mementoFields.Add( field.Declaration );
            }

            mementoType.ImplementInterface( typeof(IMemento) );

            mementoType.IntroduceConstructor(
                nameof(MementoConstructorTemplate),
                buildConstructor: b =>
                {
                    foreach (var mementoField in mementoFields)
                    {
                        b.AddParameter( mementoField.Name, mementoField.Type );
                    }
                },
                args: new { fields = mementoFields } );

            builder.ImplementInterface( typeof(IOriginator));

            var args = new { mementoType = mementoType.Declaration };

            builder.Advice.IntroduceMethod(builder.Target, nameof(Save), args: args);
            builder.Advice.IntroduceMethod(builder.Target, nameof(Restore), args: args);
        }

        [Template]
        private object? MementoField;

        [Template]
        public IMemento Save(INamedType mementoType)
        {
            return
                BuildNewExpression(
                        mementoType,
                        meta.Target.Type.FieldsAndProperties.Where(f => f.IsAutoPropertyOrField == true && !f.IsImplicitlyDeclared))
                    .Value;
        }

        public IExpression BuildNewExpression( INamedType mementoType, IEnumerable<IFieldOrProperty> fieldsOrProperties )
        {
            var expressionBuilder = new ExpressionBuilder();
            expressionBuilder.AppendVerbatim( "new " );
            expressionBuilder.AppendTypeName( mementoType );
            expressionBuilder.AppendVerbatim( "(" );

            var first = true;

            foreach (var fieldOrProperty in fieldsOrProperties)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    expressionBuilder.AppendVerbatim( "," );
                }

                expressionBuilder.AppendExpression( fieldOrProperty );
            }

            expressionBuilder.AppendVerbatim( ")" );

            return expressionBuilder.ToExpression();
        }

        [Template]
        public void Restore( IMemento memento, INamedType mementoType )
        {
            foreach (var fieldOrProperty in meta.Target.Type.FieldsAndProperties.Where( f => f.IsAutoPropertyOrField == true && !f.IsImplicitlyDeclared ))
            {
                var mementoField = mementoType.FieldsAndProperties.OfName( fieldOrProperty.Name ).Single();

                fieldOrProperty.Value = mementoField.With( (IExpression)meta.Cast( mementoType, memento ) ).Value;
            }
        }

        [Template]
        public void MementoConstructorTemplate( [CompileTime] List<IField> fields )
        {
            var i = meta.CompileTime( 0 );

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

        void Restore( IMemento memento );
    }

    public interface IMemento { }

    // <target>
    [Memento]
    internal class TargetClass
    {
        private int _state1;

        private int State2 { get; set; }
    }
}