using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.IntegrationTests.Aspects.CodeModel.GetSerializableDeclarationIdForIntroduced;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#pragma warning disable CS0067, CS0169, CS0618, CS0649

[assembly: AspectOrder(typeof(SerializeAttribute), typeof(IntroduceMembersAttribute))]

namespace Metalama.Framework.IntegrationTests.Aspects.CodeModel.GetSerializableDeclarationIdForIntroduced;

class IntroduceMembersAttribute : TypeAspect
{
    [Template]
    void M<T>((int x, int y) p) { }
    [Template]
    int _field;
    [Template]
    event System.EventHandler? Event;
    [Template]
    int Property { get; set; }

    [Template]
    int IndexerGet(int i) => 0;
    [Template]
    void IndexerSet(int i, int value) { }

    [Template]
    static bool NotOperator(dynamic x) => false;
    [Template]
    static int PlusOperator(dynamic x, dynamic y) => 0;
    [Template]
    static bool CastOperator(dynamic x) => true;

    [Template]
    void Finalizer() { }

    [Template]
    static string[] GetIds([CompileTime] string[] ids) => ids;

    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        base.BuildAspect(builder);

        var builderIds = new List<string>();
        var results = new List<IIntroductionAdviceResult<IDeclaration>>();

        results.Add(builder.Advice.IntroduceMethod(builder.Target, nameof(M), buildMethod: builder => builderIds.Add(builder.ToSerializableId().Id)));
        results.Add(builder.Advice.IntroduceField(builder.Target, nameof(_field), buildField: builder => builderIds.Add(builder.ToSerializableId().Id)));
        results.Add(builder.Advice.IntroduceEvent(builder.Target, nameof(Event), buildEvent: builder => builderIds.Add(builder.ToSerializableId().Id)));
        results.Add(builder.Advice.IntroduceProperty(builder.Target, nameof(Property), buildProperty: builder => builderIds.Add(builder.ToSerializableId().Id)));
        results.Add(builder.Advice.IntroduceIndexer(builder.Target, typeof(int), nameof(IndexerGet), nameof(IndexerSet), buildIndexer: builder => builderIds.Add(builder.ToSerializableId().Id)));

        results.Add(builder.Advice.IntroduceUnaryOperator(
            builder.Target, nameof(NotOperator), builder.Target, TypeFactory.GetType(typeof(bool)), OperatorKind.LogicalNot,
            buildOperator: builder => builderIds.Add(builder.ToSerializableId().Id)));
        results.Add(builder.Advice.IntroduceBinaryOperator(
            builder.Target, nameof(PlusOperator), builder.Target, builder.Target, TypeFactory.GetType(typeof(int)), OperatorKind.Addition,
            buildOperator: builder => builderIds.Add(builder.ToSerializableId().Id)));
        results.Add(builder.Advice.IntroduceConversionOperator(
            builder.Target, nameof(CastOperator), builder.Target, TypeFactory.GetType(typeof(bool)),
            buildOperator: builder => builderIds.Add(builder.ToSerializableId().Id)));

        results.Add(builder.Advice.IntroduceFinalizer(builder.Target, nameof(Finalizer)));

        results.Add(builder.Advice.IntroduceParameter(
            builder.Target.Constructors.First(), "x", typeof(int), TypedConstant.Create(42),
            pullAction: (p, c) =>
            {
                try
                {
                    builderIds.Add(p.ToSerializableId().Id);
                }
                catch (NotSupportedException ex)
                {
                    builderIds.Add($"{ex.GetType()}: {ex.Message}");
                }

                return PullAction.None;
            }));

        builder.Advice.IntroduceMethod(builder.Target, nameof(GetIds), buildMethod: builder => builder.Name = "GetBuilderIds", args: new { ids = builderIds.ToArray() });

        var builtIds = results.Select(r => r.Declaration.ToSerializableId().Id).ToArray();

        builder.Advice.IntroduceMethod(builder.Target, nameof(GetIds), buildMethod: builder => builder.Name = "GetBuiltIds", args: new { ids = builtIds });
    }
}

class SerializeAttribute : TypeAspect
{
    [Introduce]
    static string[] GetAllBuiltIds()
        => meta.Target.Type.GetContainedDeclarations().Select(d => d.ToSerializableId().Id).OrderBy(x => x).ToArray();
}

[CompileTime]
static class TestDeclarationExtensions
{
    /// <summary>
    /// Select all declarations recursively contained in a given declaration (i.e. all descendants of the tree).
    /// </summary>
    public static IEnumerable<IDeclaration> GetContainedDeclarations(this IDeclaration declaration)
        => new[] { declaration }.SelectManyRecursive(GetDeclarations);

    /// <summary>
    /// Select declarations directly contained in a given declaration.
    /// </summary>
    internal static IEnumerable<IDeclaration> GetDeclarations(this IDeclaration declaration)
        => declaration switch
        {
            ICompilation compilation => new[] { compilation.GlobalNamespace },
            INamespace ns => Enumerable.Concat<IDeclaration>(ns.Namespaces, ns.Types),
            INamedType namedType => new IEnumerable<IDeclaration>[] {
                    namedType.NestedTypes,
                    namedType.Methods,
                    namedType.Constructors,
                    namedType.Fields,
                    namedType.Properties,
                    namedType.Indexers,
                    namedType.Events,
                    namedType.TypeParameters }.SelectMany(x => x)
                .Concat(namedType.Finalizer == null ? Enumerable.Empty<IDeclaration>() : new[] { namedType.Finalizer }),
            IMethod method => method.Parameters
                .Concat<IDeclaration>(method.TypeParameters)
                .Concat(method.ReturnParameter == null ? Enumerable.Empty<IDeclaration>() : new[] { method.ReturnParameter }),
            IIndexer indexer => indexer.Parameters.Concat<IDeclaration>(indexer.Accessors),
            IConstructor constructor => constructor.Parameters,
            IMemberWithAccessors member => member.Accessors,
            _ => Enumerable.Empty<IDeclaration>()
        };
}

// <target>
[IntroduceMembers, Serialize]
class C
{
    C() { }
    C(string id) : this() { }
}
