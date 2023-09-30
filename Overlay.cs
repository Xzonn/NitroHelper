namespace NitroHelper
{
  public static class Overlay
  {
    public static sFile[] ReadBasicOverlays(string filePath, uint offset, uint size, bool arm9, FileAllocationTable fatTable)
      => ReadBasicOverlays(File.OpenRead(filePath), offset, size, arm9, fatTable);

    public static sFile[] ReadBasicOverlays(Stream stream, uint offset, uint size, bool arm9, FileAllocationTable fatTable)
    {
      BinaryReader br = new(stream);
      stream.Position = offset;

      sFile[] overlays = new sFile[size / 0x20];

      for (int i = 0; i < overlays.Length; i++)
      {
        var overlayId = br.ReadUInt32();
        br.ReadBytes(20);
        var fileId = br.ReadUInt32();
        br.ReadBytes(4);
        overlays[i] = new sFile
        {
          name = $"overlay{(arm9 ? "" : "7")}_{overlayId:d04}.bin",
          id = (ushort)fileId,
          offset = fatTable.fatTable[(int)fileId].offset,
          size = fatTable.fatTable[(int)fileId].size,
          path = "",
        };
      }

      return overlays;
    }
  }
}