using Caravela.Framework.Code;

namespace Caravela.Framework.Impl.CodeModel
{

    internal abstract class Event : Member, IEvent
    {
        INamedType IEvent.EventType => this.EventType;

        public abstract NamedType EventType { get; }

        IMethod IEvent.Adder => this.Adder;

        public abstract Method Adder { get; }

        IMethod IEvent.Remover => this.Remover;

        public abstract Method Remover { get; }

        IMethod? IEvent.Raiser => this.Raiser;

        public abstract Method? Raiser{ get; }

        public override CodeElementKind ElementKind => CodeElementKind.Event;
    }
}
