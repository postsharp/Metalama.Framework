internal class TargetCode
{
  [AddParameter]
  TargetCode(string s, global::System.Int32 arg = default(global::System.Int32))
  {
  }
  TargetCode(int i) : this(i.ToString(), 42)
  {
  }
}