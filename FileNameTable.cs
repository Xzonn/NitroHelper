using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NitroHelper
{
  public class sFile
  {
    public uint offset;           // Offset where the files inside of the file in path
    public uint size;             // Length of the file
    public string name = "";             // File name
    public ushort id = 0xFFFF;               // Internal id
    public string path = "";             // Path where the file is

    public string GetPath(string @default)
    {
      return string.IsNullOrEmpty(path) ? @default : path;
    }
  }

  public class sFolder
  {
    public List<sFile> files = new List<sFile>();           // List of files
    public List<sFolder> folders = new List<sFolder>();      // List of folders
    public string name = "";             // File name
    public ushort id = 0xFFFF;               // Internal id

    public uint mainOffset;           // OffSet de la SubTable relativa al archivo FNT
    public ushort firstFileId;      // ID del primer archivo que contiene. Puede corresponder a uno que contenga un directorio interno
    public ushort parentFolderId;   // ID del directorio padre de éste
  }

  public class FileNameTable
  {
    public sFolder root;

    public FileNameTable(FileAllocationTable fatTable, string filePath, uint offset = 0) : this(true, fatTable, File.OpenRead(filePath), offset) { }

    public FileNameTable(FileAllocationTable fatTable, Stream stream, uint offset = 0): this(false, fatTable, stream, offset) { }

    private FileNameTable(bool close, FileAllocationTable fatTable, Stream stream, uint offset = 0)
    {
      List<sFolder> mains = new List<sFolder>();

      BinaryReader br = new BinaryReader(stream);
      stream.Position = offset;

      stream.Position += 6;
      ushort number_directories = br.ReadUInt16();  // Get the total number of directories (mainTables)
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
        {                                      //  Error, in some cases the number of directories is wrong
          number_directories--;              // Found in FF Four Heroes of Light, Tetris Party deluxe
          i--;
          continue;
        }

        long currOffset = stream.Position;           // Posición guardada donde empieza la siguienta maintable
        stream.Position = offset + main.mainOffset;      // SubTable correspondiente

        // SubTable
        byte id = br.ReadByte();                            // Byte que identifica si es carpeta o archivo.
        ushort fileId = main.firstFileId;

        while (id != 0x0)   // Indicador de fin de la SubTable
        {
          if (id < 0x80)  // File
          {
            sFile currFile = new sFile()
            {
              name = Encoding.GetEncoding("shift_jis").GetString(br.ReadBytes(id)),
              id = fileId,
              offset = fatTable.fatTable[fileId].offset,
              size = fatTable.fatTable[fileId].size,
              path = "",
            };
            main.files.Add(currFile);
            fileId++;
          }
          if (id > 0x80)  // Directorio
          {
            sFolder currFolder = new sFolder()
            {
              name = Encoding.GetEncoding("shift_jis").GetString(br.ReadBytes(id - 0x80)),
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

      if (tables[folderId & 0xFFF].folders != null) // Si tiene carpetas dentro.
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
      if (currFolder.id == id) // Archivos descomprimidos
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
