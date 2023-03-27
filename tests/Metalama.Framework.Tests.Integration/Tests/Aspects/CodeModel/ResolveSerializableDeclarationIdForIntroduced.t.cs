[IntroduceMembers, Serialize]
class C
{
  C(global::System.Int32 x = 42)
  {
  }
  C(string id) : this()
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
      "C._field.get",
      "C._field.get@<return>",
      "C._field.set",
      "C._field.set@value",
      "C._field.set@<return>",
      "C.C()",
      "x",
      "C.C(string)",
      "C.C(string)/id",
      "C.Event.add",
      "C.Event.add@value",
      "C.Event.add@<return>",
      "C.Finalize",
      "<return>",
      "C.this[].get",
      "C.this[].get@index",
      "C.this[].get@<return>",
      "C.Property.get",
      "C.Property.get@<return>",
      "C.M",
      "p",
      "<return>",
      "T",
      "C.op_Addition",
      "x",
      "y",
      "<return>",
      "C.op_Explicit",
      "x",
      "<return>",
      "C.op_LogicalNot",
      "x",
      "<return>",
      "C.Event.remove",
      "C.Event.remove@value",
      "C.Event.remove@<return>",
      "C.this[].set",
      "C.this[].set@index",
      "C.this[].set@value",
      "C.this[].set@<return>",
      "C.Property.set",
      "C.Property.set@value",
      "C.Property.set@<return>",
      "C.this[]",
      "index",
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