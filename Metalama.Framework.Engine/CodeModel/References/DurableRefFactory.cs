using Metalama.Framework.Code;
using Metalama.Framework.Engine.SerializableIds;

namespace Metalama.Framework.Engine.CodeModel.References;

internal static class DurableRefFactory
{
        
    public static IDurableRef<T> FromSymbolId<T>( in SymbolId symbolKey )
        where T : class, ICompilationElement
        => new SymbolIdRef<T>( symbolKey );

    public static IDurableRef<T> FromDeclarationId<T>( SerializableDeclarationId id )
        where T : class, ICompilationElement
        => new DeclarationIdRef<T>( id );

    public static IDurableRef<T> FromTypeId<T>( SerializableTypeId id )
        where T : class, IType
        => new TypeIdRef<T>( id );
}