using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.IntegrationTests.Aspects.CodeModel.GetSerializableDeclarationIdForIntroduced;
using System;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable CS0067, CS0169, CS0618, CS0649

[assembly: AspectOrder( AspectOrderDirection.RunTime, typeof(SerializeAttribute), typeof(IntroduceMembersAttribute) )]

namespace Metalama.Framework.IntegrationTests.Aspects.CodeModel.GetSerializableDeclarationIdForIntroduced;

internal class IntroduceMembersAttribute : TypeAspect
{
    [Template]
    private void M<T>( (int x, int y) p ) { }

    [Template]
    private int _field;

    [Template]
    private event EventHandler? Event;

    [Template]
    private int Property { get; set; }

    [Template]
    private int IndexerGet( int i ) => 0;

    [Template]
    private void IndexerSet( int i, int value ) { }

    [Template]
    public static bool NotOperator( dynamic x ) => false;

    [Template]
    public static int PlusOperator( dynamic x, dynamic y ) => 0;

    [Template]
    public static bool CastOperator( dynamic x ) => true;

    [Template]
    private void Finalizer() { }

    [Template]
    private static string[] GetIds( [CompileTime] string[] ids ) => ids;

    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        base.BuildAspect( builder );

        var builderIds = new List<string>();
        var results = new List<IIntroductionAdviceResult<IDeclaration>>();

        results.Add( builder.Advice.IntroduceMethod( builder.Target, nameof(M), buildMethod: builder => builderIds.Add( builder.ToSerializableId().Id ) ) );
        results.Add( builder.Advice.IntroduceField( builder.Target, nameof(_field), buildField: builder => builderIds.Add( builder.ToSerializableId().Id ) ) );
        results.Add( builder.Advice.IntroduceEvent( builder.Target, nameof(Event), buildEvent: builder => builderIds.Add( builder.ToSerializableId().Id ) ) );

        results.Add(
            builder.Advice.IntroduceProperty( builder.Target, nameof(Property), buildProperty: builder => builderIds.Add( builder.ToSerializableId().Id ) ) );

        results.Add(
            builder.Advice.IntroduceIndexer(
                builder.Target,
                typeof(int),
                nameof(IndexerGet),
                nameof(IndexerSet),
                buildIndexer: builder => builderIds.Add( builder.ToSerializableId().Id ) ) );

        results.Add(
            builder.Advice.IntroduceUnaryOperator(
                builder.Target,
                nameof(NotOperator),
                builder.Target,
                TypeFactory.GetType( typeof(bool) ),
                OperatorKind.LogicalNot,
                buildOperator: builder => builderIds.Add( builder.ToSerializableId().Id ) ) );

        results.Add(
            builder.Advice.IntroduceBinaryOperator(
                builder.Target,
                nameof(PlusOperator),
                builder.Target,
                builder.Target,
                TypeFactory.GetType( typeof(int) ),
                OperatorKind.Addition,
                buildOperator: builder => builderIds.Add( builder.ToSerializableId().Id ) ) );

        results.Add(
            builder.Advice.IntroduceConversionOperator(
                builder.Target,
                nameof(CastOperator),
                builder.Target,
                TypeFactory.GetType( typeof(bool) ),
                buildOperator: builder => builderIds.Add( builder.ToSerializableId().Id ) ) );

        results.Add( builder.Advice.IntroduceFinalizer( builder.Target, nameof(Finalizer) ) );

        results.Add(
            builder.Advice.IntroduceParameter(
                builder.Target.Constructors.First(),
                "x",
                typeof(int),
                TypedConstant.Create( 42 ),
                pullAction: ( p, c ) =>
                {
                    try
                    {
                        builderIds.Add( p.ToSerializableId().Id );
                    }
                    catch (NotSupportedException ex)
                    {
                        builderIds.Add( $"{ex.GetType()}: {ex.Message}" );
                    }

                    return PullAction.None;
                } ) );

        builder.Advice.IntroduceMethod(
            builder.Target,
            nameof(GetIds),
            buildMethod: builder => builder.Name = "GetBuilderIds",
            args: new { ids = builderIds.ToArray() } );

        var builtIds = results.Select( r => r.Declaration.ToSerializableId().Id ).ToArray();

        builder.Advice.IntroduceMethod( builder.Target, nameof(GetIds), buildMethod: builder => builder.Name = "GetBuiltIds", args: new { ids = builtIds } );
    }
}

internal class SerializeAttribute : TypeAspect
{
    [Introduce]
    private static string[] GetAllBuiltIds() => meta.Target.Type.GetContainedDeclarations().Select( d => d.ToSerializableId().Id ).OrderBy( x => x ).ToArray();
}

[CompileTime]
internal static class TestDeclarationExtensions
{
    /// <summary>
    /// Select all declarations recursively contained in a given declaration (i.e. all descendants of the tree).
    /// </summary>
    public static IEnumerable<IDeclaration> GetContainedDeclarations( this IDeclaration declaration )
        => new[] { declaration }.SelectManyRecursive( GetDeclarations );

    /// <summary>
    /// Select declarations directly contained in a given declaration.
    /// </summary>
    internal static IEnumerable<IDeclaration> GetDeclarations( this IDeclaration declaration )
        => declaration switch
        {
            ICompilation compilation => new[] { compilation.GlobalNamespace },
            INamespace ns => ns.Namespaces.Concat<IDeclaration>( ns.Types ),
            INamedType namedType => new IEnumerable<IDeclaration>[]
                {
                    namedType.Types,
                    namedType.Methods,
                    namedType.Constructors,
                    namedType.Fields,
                    namedType.Properties,
                    namedType.Indexers,
                    namedType.Events,
                    namedType.TypeParameters
                }.SelectMany( x => x )
                .Concat( namedType.Finalizer == null ? Enumerable.Empty<IDeclaration>() : new[] { namedType.Finalizer } ),
            IMethod method => Enumerable
                .Concat<IDeclaration>( method.Parameters, method.TypeParameters )
                .Concat( method.ReturnParameter == null ? Enumerable.Empty<IDeclaration>() : new[] { method.ReturnParameter } ),
            IIndexer indexer => indexer.Parameters.Concat<IDeclaration>( indexer.Accessors ),
            IConstructor constructor => constructor.Parameters,
            IHasAccessors member => member.Accessors,
            _ => Enumerable.Empty<IDeclaration>()
        };
}

// <target>
[IntroduceMembers]
[Serialize]
internal class C
{
    private C() { }

    private C( string id ) : this() { }
}