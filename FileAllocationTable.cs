using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NitroHelper
{
  public class FileAllocationTable
  {
    public class FATItem
    {
      public ushort id;
      public uint offset;
      public uint size;
    }
    public List<FATItem> fatTable = new List<FATItem>();
    public ushort[] sortedIDs;

    public FileAllocationTable(string romFile, uint fatOffset, uint fatSize) : this(File.OpenRead(romFile), fatOffset, fatSize) { }

    public FileAllocationTable(Stream stream, uint fatOffset, uint fatSize)
    {
      BinaryReader br = new BinaryReader(stream);
      br.BaseStream.Position = fatOffset;

      for (ushort i = 0; i < fatSize / 0x08; i++)
      {
        // Number of files
        var offset = br.ReadUInt32();
        var size = br.ReadUInt32() - offset;
        fatTable.Add(new FATItem()
        {
          id = i,
          offset = offset,
          size = size,
        });
      }

      var _ = fatTable.ToList();
      _.Sort(Sort);
      sortedIDs = _.Select(x => x.id).ToArray();
    }

    public static void WriteTo(string fileOut, sFolder root, uint FAToffset, ushort[] sortedIDs, uint offsetOv9, uint offsetOv7)
      => WriteTo(File.Create(fileOut), root, FAToffset, sortedIDs, offsetOv9, offsetOv7);

    public static void WriteTo(Stream stream, sFolder root, uint FAToffset, ushort[] sortedIDs, uint offsetOv9, uint offsetOv7)
    {
      BinaryWriter bw = new BinaryWriter(stream);

      int num_files = sortedIDs.Length;

      // Set the first file offset
      uint offset = (uint)(FAToffset + num_files * 0x08);
      if ((offset % 0x200) != 0)
      {
        offset += 0x200 - (offset % 0x200);
      }
      offset += 0xA00;

      byte[] buffer = new byte[num_files * 8];
      int zero_files = 0;
      byte[] temp;
      for (int i = 0; i < num_files; i++)
      {
        sFile currFile = FileNameTable.FindFile(sortedIDs[i], root);

        if (currFile == null)
        {
          zero_files++;
        }
        else if (currFile.name.StartsWith("overlay_"))
        {
          Array.Copy(BitConverter.GetBytes(offsetOv9), 0, buffer, sortedIDs[i] * 8, 4);
          offsetOv9 += currFile.size;
          Array.Copy(BitConverter.GetBytes(offsetOv9), 0, buffer, sortedIDs[i] * 8 + 4, 4);
          if (offsetOv9 % 0x200 != 0)
          {
            offsetOv9 += 0x200 - (offsetOv9 % 0x200);
          }
        }
        else if (currFile.name.StartsWith("overlay7_"))
        {
          Array.Copy(BitConverter.GetBytes(offsetOv7), 0, buffer, sortedIDs[i] * 8, 4);
          offsetOv7 += currFile.size;
          Array.Copy(BitConverter.GetBytes(offsetOv7), 0, buffer, sortedIDs[i] * 8 + 4, 4);
          if (offsetOv7 % 0x200 != 0)
          {
            offsetOv7 += 0x200 - (offsetOv7 % 0x200);
          }
        }
        else
        {
          Array.Copy(BitConverter.GetBytes(offset), 0, buffer, sortedIDs[i] * 8, 4);
          offset += currFile.size;
          Array.Copy(BitConverter.GetBytes(offset), 0, buffer, sortedIDs[i] * 8 + 4, 4);
          if (offset % 0x200 != 0)
          {
            offset += 0x200 - (offset % 0x200);
          }
        }
      }

      bw.Write(buffer);

      temp = BitConverter.GetBytes((uint)0);
      for (int i = 0; i < zero_files; i++)
      {
        Array.Copy(temp, 0, buffer, sortedIDs[i] * 8, 4);
        Array.Copy(temp, 0, buffer, sortedIDs[i] * 8 + 4, 4);
      }

      int rem = (int)bw.BaseStream.Position % 0x200;
      if (rem != 0)
      {
        while (rem < 0x200)
        {
          bw.Write((byte)0xFF);
          rem++;
        }
      }

      bw.Flush();
    }

    private static int Sort(FATItem f1, FATItem f2)
    {
      if (f1.offset > f2.offset)
      {
        return 1;
      }
      else if (f1.offset < f2.offset)
      {
        return -1;
      }
      else
      {
        if (f1.id > f2.id)
        {
          return 1;
        }
        else if (f1.id < f2.id)
        {
          return -1;
        }
        else  // Impossible
        {
          return 0;
        }
      }
    }
  }
}