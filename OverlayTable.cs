using System.Collections.Generic;
using System.IO;

namespace NitroHelper
{
  public class OverlayTable
  {
    public class OverlayItem
    {
      public uint overlayId;
      public uint ramAddress;
      public uint ramSize;
      public uint bssSize;
      public uint staticInitialiserStartAddress;
      public uint staticInitialiserEndAddress;
      public uint fileId;
      public uint reserved;

      public override string ToString()
      {
        return $"Overaly item #{overlayId,-3} (0x{overlayId:x02}): ramAddress: 0x{ramAddress:x08}, ramSize: 0x{ramSize:x08}, bssSize: 0x{bssSize:x08}, staticInitialiserStartAddress: 0x{staticInitialiserStartAddress:x08}, staticInitialiserEndAddress: 0x{staticInitialiserEndAddress:x08}, reserved: 0x{reserved:x08}";
      }
    }

    public readonly bool isArm9 = false;
    public List<OverlayItem> overlayTable = new List<OverlayItem>();

    public OverlayTable(string filePath, uint offset, uint size, bool arm9) : this(true, File.OpenRead(filePath), offset, size, arm9) { }

    public OverlayTable(Stream stream, uint offset, uint size, bool arm9) : this(false, stream, offset, size, arm9) { }

    private OverlayTable(bool close, Stream stream, uint offset, uint size, bool arm9)
    {
      isArm9 = arm9;

      BinaryReader br = new BinaryReader(stream);
      stream.Position = offset;
      for (int i = 0; i < size / 0x20; i++)
      {
        var item = new OverlayItem()
        {
          overlayId = br.ReadUInt32(),
          ramAddress = br.ReadUInt32(),
          ramSize = br.ReadUInt32(),
          bssSize = br.ReadUInt32(),
          staticInitialiserStartAddress = br.ReadUInt32(),
          staticInitialiserEndAddress = br.ReadUInt32(),
          fileId = br.ReadUInt32(),
          reserved = br.ReadUInt32(),
        };
        overlayTable.Add(item);
      }

      if (close) { stream.Close(); }
    }

    public sFile[] ReadBasicOverlays(FileAllocationTable fatTable)
    {
      sFile[] overlays = new sFile[overlayTable.Count];
      for (int i = 0; i < overlays.Length; i++)
      {
        overlays[i] = new sFile
        {
          name = $"overlay{(isArm9 ? "" : "7")}_{overlayTable[i].overlayId:d04}.bin",
          id = (ushort)overlayTable[i].fileId,
          offset = fatTable.fatTable[(int)overlayTable[i].fileId].offset,
          size = fatTable.fatTable[(int)overlayTable[i].fileId].size,
        };
      }

      return overlays;
    }

    public void WriteTo(string path, uint offset = 0) => WriteTo(true, File.Create(path), offset);

    public void WriteTo(Stream stream, uint offset = 0) => WriteTo(false, stream, offset);

    private void WriteTo(bool close, Stream stream, uint offset = 0)
    {
      BinaryWriter bw = new BinaryWriter(stream);
      stream.Position = offset;
      foreach (var item in overlayTable)
      {
        bw.Write(item.overlayId);
        bw.Write(item.ramAddress);
        bw.Write(item.ramSize);
        bw.Write(item.bssSize);
        bw.Write(item.staticInitialiserStartAddress);
        bw.Write(item.staticInitialiserEndAddress);
        bw.Write(item.fileId);
        bw.Write(item.reserved);
      }
      bw.WritePadding(0x200);

      if (close) { stream.Close(); }
    }

    public override string ToString()
    {
      return $"{(isArm9 ? "ARM9" : "ARM7")} overlay table: {overlayTable.Count} entries";
    }
  }
}
