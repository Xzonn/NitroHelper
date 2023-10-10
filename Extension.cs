using System.IO;
using System.Linq;

namespace NitroHelper
{
  public static class Extension
  {
    public static void WritePadding(this BinaryWriter bw, int paddingBase)
    {
      bw.Write(Enumerable.Repeat((byte)0xFF, (paddingBase - (int)bw.BaseStream.Position % paddingBase) % paddingBase).ToArray());
    }
  }
}
