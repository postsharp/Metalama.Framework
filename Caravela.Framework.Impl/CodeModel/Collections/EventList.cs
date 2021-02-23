using System.Collections.Generic;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Links;

namespace Caravela.Framework.Impl.CodeModel.Collections
{
    internal class EventList : MemberList<IEvent, MemberLink<IEvent>>, IEventList
    {
        public EventList( IEnumerable<MemberLink<IEvent>> sourceItems, CompilationModel compilation ) : base( sourceItems, compilation )
        {
        }
    }
}