using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NitroHelper
{
  public class sFile
  {
    public uint offset;
    public uint size;
    public string name = "";
    public ushort id = 0xFFFF;
    public string path = "";

    public string GetPath(string @default)
    {
      return string.IsNullOrEmpty(path) ? @default : path;
    }
  }

  public class sFolder
  {
    public List<sFile> files = new List<sFile>();
    public List<sFolder> folders = new List<sFolder>();
    public string name = "";
    public ushort id = 0xFFFF;

    public uint mainOffset;
    public ushort firstFileId;
    public ushort parentFolderId;
  }

  public class FileNameTable
  {
    public sFolder root;

    public FileNameTable(FileAllocationTable fatTable, string filePath, uint offset = 0) : this(true, fatTable, File.OpenRead(filePath), offset) { }

    public FileNameTable(FileAllocationTable fatTable, Stream stream, uint offset = 0) : this(false, fatTable, stream, offset) { }

    private FileNameTable(bool close, FileAllocationTable fatTable, Stream stream, uint offset = 0)
    {
#if NET6_0_OR_GREATER
      Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
      List<sFolder> mains = new List<sFolder>();

      BinaryReader br = new BinaryReader(stream);
      stream.Position = offset;

      stream.Position += 6;
      ushort number_directories = br.ReadUInt16();
      stream.Position = offset;

      for (int i = 0; i < number_directories; i++)
      {
        sFolder main = new sFolder()
        {
          mainOffset = br.ReadUInt32(),
          firstFileId = br.ReadUInt16(),
          parentFolderId = br.ReadUInt16()
        };

        if (i != 0 && stream.Position > offset + mains[0].mainOffset)
        {
          number_directories--;
          i--;
          continue;
        }

        long currOffset = stream.Position;
        stream.Position = offset + main.mainOffset;


        byte id = br.ReadByte();
        ushort fileId = main.firstFileId;

        while (id != 0x0)
        {
          if (id < 0x80)
          {
            sFile currFile = new sFile()
            {
              name = Encoding.GetEncoding(932).GetString(br.ReadBytes(id)),
              id = fileId,
              offset = fatTable.fatTable[fileId].offset,
              size = fatTable.fatTable[fileId].size,
            };
            main.files.Add(currFile);
            fileId++;
          }
          if (id > 0x80)
          {
            sFolder currFolder = new sFolder()
            {
              name = Encoding.GetEncoding(932).GetString(br.ReadBytes(id - 0x80)),
              id = br.ReadUInt16(),
            };
            main.folders.Add(currFolder);
          }

          id = br.ReadByte();
        }

        mains.Add(main);
        stream.Position = currOffset;
      }

      root = new sFolder()
      {
        name = "root",
        folders = new List<sFolder>(),
        files = new List<sFile>(),
      };
      root.folders.Add(ConvertListToTree(mains, 0, "data"));
      root.folders[0].id = 0xFFFF;

      if (close) { stream.Close(); }
    }

    private static sFolder ConvertListToTree(List<sFolder> tables, int folderId, string folderName)
    {
      sFolder currFolder = new sFolder()
      {
        name = folderName,
        id = (ushort)folderId,
        files = tables[folderId & 0xFFF].files
      };

      if (tables[folderId & 0xFFF].folders != null)
      {
        currFolder.folders = new List<sFolder>();

        foreach (sFolder subFolder in tables[folderId & 0xFFF].folders)
        {
          currFolder.folders.Add(ConvertListToTree(tables, subFolder.id, subFolder.name));
        }
      }

      return currFolder;
    }

    public static sFile FindFile(int id, sFolder currFolder)
    {
      if (currFolder.id == id)
      {
        sFile folderFile = new sFile()
        {
          name = currFolder.name,
          id = currFolder.id,
        };

        return folderFile;
      }

      foreach (sFile archivo in currFolder.files)
      {
        if (archivo.id == id)
        {
          return archivo;
        }
      }

      foreach (sFolder subFolder in currFolder.folders)
      {
        sFile currFile = FindFile(id, subFolder);
        if (currFile != null && !string.IsNullOrEmpty(currFile.name))
        {
          return currFile;
        }
      }

      return null;
    }
  }
}
