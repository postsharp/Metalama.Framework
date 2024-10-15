[IntroduceMembers]
[Serialize]
internal class C
{
  private C(global::System.Int32 x = 42)
  {
  }
  private C(string id) : this()
  {
  }
  private global::System.Int32 _field;
  private global::System.Int32 Property { get; set; }
  private static global::System.String[] GetAllBuiltIds()
  {
    return new global::System.String[]
    {
      "C.Event",
      "C._field",
      "C._field.get()",
      "C._field.get()@<return>",
      "C._field.set(int)",
      "C._field.set(int)@value",
      "C._field.set(int)@<return>",
      "C.C()",
      "C.C(int)@x",
      "C.C(string)",
      "C.C(string)/id",
      "C.Event.add(EventHandler)",
      "C.Event.add(EventHandler)@value",
      "C.Event.add(EventHandler)@<return>",
      "C.~C()",
      "C.~C()@<return>",
      "C.this[].get(int)",
      "C.this[].get(int)@index",
      "C.this[].get(int)@<return>",
      "C.Property.get()",
      "C.Property.get()@<return>",
      "C.M<T>(ValueTuple<int, int>)",
      "C.M<T>(ValueTuple<int, int>)@p",
      "C.M<T>(ValueTuple<int, int>)@<return>",
      "C.M<T>(ValueTuple<int, int>)/T",
      "C.op_Addition(C, C)",
      "C.op_Addition(C, C)@x",
      "C.op_Addition(C, C)@y",
      "C.op_Addition(C, C)@<return>",
      "C.op_Explicit(C)",
      "C.op_Explicit(C)@x",
      "C.op_Explicit(C)@<return>",
      "C.op_LogicalNot(C)",
      "C.op_LogicalNot(C)@x",
      "C.op_LogicalNot(C)@<return>",
      "C.Event.remove(EventHandler)",
      "C.Event.remove(EventHandler)@value",
      "C.Event.remove(EventHandler)@<return>",
      "C.this[].set(int, int)",
      "C.this[].set(int, int)@index",
      "C.this[].set(int, int)@value",
      "C.this[].set(int, int)@<return>",
      "C.Property.set(int)",
      "C.Property.set(int)@value",
      "C.Property.set(int)@<return>",
      "C.this[int]",
      "C.this[int]@index",
      "C.Property",
      "C"
    };
  }
  private void M<T>((global::System.Int32 x, global::System.Int32 y) p)
  {
  }
  private event global::System.EventHandler? Event;
  ~C()
  {
  }
  public static global::System.Int32 operator +(global::Metalama.Framework.IntegrationTests.Aspects.CodeModel.ResolveSerializableDeclarationIdForIntroduced.C x, global::Metalama.Framework.IntegrationTests.Aspects.CodeModel.ResolveSerializableDeclarationIdForIntroduced.C y)
  {
    return (global::System.Int32)0;
  }
  public static explicit operator global::System.Boolean(global::Metalama.Framework.IntegrationTests.Aspects.CodeModel.ResolveSerializableDeclarationIdForIntroduced.C x)
  {
    return (global::System.Boolean)true;
  }
  public static global::System.Boolean operator !(global::Metalama.Framework.IntegrationTests.Aspects.CodeModel.ResolveSerializableDeclarationIdForIntroduced.C x)
  {
    return (global::System.Boolean)false;
  }
  private global::System.Int32 this[global::System.Int32 index]
  {
    get
    {
      return (global::System.Int32)0;
    }
    set
    {
    }
  }
}