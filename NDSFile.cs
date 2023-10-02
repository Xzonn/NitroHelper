using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NitroHelper
{
  public class NDSFile
  {
    public string filePath;
    public Header header;
    public Banner banner;
    public FileAllocationTable fatTable;
    public FileNameTable fntTable;
    public sFolder root { get => fntTable.root; }
    public sFolder data { get => fntTable.root.folders[0]; }
    public sFolder overlay { get => fntTable.root.folders[1]; }

    public NDSFile(string _filePath)
    {
      filePath = _filePath;
      var fileStream = File.OpenRead(filePath);

      // Load header, banner, fatTable
      header = new Header(fileStream);
      banner = new Banner(fileStream, header.bannerOffset);
      fatTable = new FileAllocationTable(fileStream, header.FAToffset, header.FATsize);
      fntTable = new FileNameTable(fatTable, fileStream, header.FNToffset);

      // Load data and overlay
      var overlay = new sFolder()
      {
        name = "overlay",
        files = new List<sFile>(),
      };
      overlay.files.AddRange(Overlay.ReadBasicOverlays(fileStream, header.ARM9overlayOffset, header.ARM9overlaySize, true, fatTable));
      overlay.files.AddRange(Overlay.ReadBasicOverlays(fileStream, header.ARM7overlayOffset, header.ARM7overlaySize, false, fatTable));
      root.folders.Add(overlay);

      // Add root
      root.files.Add(new sFile()
      {
        name = "header.bin",
        offset = 0,
        size = header.headerSize,
        path = "",
      });

      root.files.Add(new sFile()
      {
        name = "banner.bin",
        offset = header.bannerOffset,
        size = 0x840,
        path = "",
      });

      root.files.Add(new sFile()
      {
        name = "fnt.bin",
        offset = header.FNToffset,
        size = header.FNTsize,
        path = "",
      });

      root.files.Add(new sFile()
      {
        name = "fat.bin",
        offset = header.FAToffset,
        size = header.FATsize,
        path = "",
      });

      root.files.Add(new sFile()
      {
        name = "arm9.bin",
        offset = header.ARM9romOffset,
        size = header.ARM9size + (uint)(header.nitrocode ? 12 : 0),
        path = "",
      });

      root.files.Add(new sFile()
      {
        name = "arm7.bin",
        offset = header.ARM7romOffset,
        size = header.ARM7size,
        path = "",
      });

      if (header.ARM9overlaySize != 0)
      {
        root.files.Add(new sFile()
        {
          name = "overarm9.bin",
          offset = header.ARM9overlayOffset,
          size = header.ARM9overlaySize,
          path = "",
        });
      }

      if (header.ARM7overlaySize != 0)
      {
        root.files.Add(new sFile()
        {
          name = "overarm7.bin",
          offset = header.ARM7overlayOffset,
          size = header.ARM7overlaySize,
          path = "",
        });
      }

      fileStream.Close();
    }

    public void SaveAs(string outputPath)
    {
      /* ROM sections:
       *
       * Header (0x0000-0x4000)
       * ARM9 Binary
       *   |_ARM9
       *   |_ARM9 Overlays Tables
       *   |_ARM9 Overlays
       * ARM7 Binary
       *   |_ARM7
       *   |_ARM7 Overlays Tables
       *   |_ARM7 Overlays
       * FNT (File Name Table)
       *   |_Main tables
       *   |_Subtables (names)
       * FileAllocationTable (File Allocation Table)
       *   |_Files offset
       *     |_Start offset
       *     |_End offset
       * Banner
       *   |_Header 0x20
       *   |_Icon (Bitmap + palette) 0x200 + 0x20
       *   |_Game titles (Japanese, English, French, German, Italian, Spanish) 6 * 0x100
       * Files...
      */

      // Get special files
      var systemFiles = root.files;
      sFile fnt = systemFiles.Find(sFile => sFile.name == "fnt.bin");
      sFile fat = systemFiles.Find(sFile => sFile.name == "fat.bin");
      sFile arm9 = systemFiles.Find(sFile => sFile.name == "arm9.bin");
      sFile arm7 = systemFiles.Find(sFile => sFile.name == "arm7.bin");
      sFile headerFile = systemFiles.Find(sFile => sFile.name == "header.bin");
      sFile bannerFile = systemFiles.Find(sFile => sFile.name == "banner.bin");

      int index = systemFiles.FindIndex(sFile => sFile.name == "overarm9.bin");
      sFile y9 = new sFile();
      List<sFile> ov9 = new List<sFile>();
      if (index != -1)
      {
        y9 = systemFiles[index];
        ov9 = overlay.files.FindAll(sFile => sFile.name.StartsWith("overlay_"));
      }

      index = systemFiles.FindIndex(sFile => sFile.name == "overarm7.bin");
      sFile y7 = new sFile();
      List<sFile> ov7 = new List<sFile>();
      if (index != -1)
      {
        y7 = systemFiles[index];
        ov7 = overlay.files.FindAll(sFile => sFile.name.StartsWith("overlay7_"));
      }

      if (headerFile.GetPath(filePath) != filePath)
      {
        var newHeader = new Header(headerFile.path, headerFile.offset);
        header.gameTitle = newHeader.gameTitle;
        header.gameCode = newHeader.gameCode;
      }

      var outputStream = File.Create(outputPath);
      var originalStream = File.OpenRead(filePath);
      var or = new BinaryReader(originalStream);
      var bw = new BinaryWriter(outputStream);

      // Write ARM9
      var originalNitrocodePosition = header.ARM9romOffset + header.ARM9size;
      bw.BaseStream.Position = header.headerSize;
      header.ARM9romOffset = (uint)bw.BaseStream.Position;
      header.ARM9size = arm9.size - (uint)(header.nitrocode ? 12 : 0);
      WriteFile(bw, or, arm9);

      header.ARM9overlayOffset = 0;
      uint fatArm9overlayOffset = 0;
      if (header.ARM9overlaySize != 0)
      {
        // ARM9 Overlays Tables
        header.ARM9overlayOffset = (uint)bw.BaseStream.Position;
        header.ARM9overlaySize = y9.size;

        WriteFile(bw, or, y9);
        fatArm9overlayOffset = (uint)bw.BaseStream.Position;

        foreach (var overlay in ov9)
        {
          WriteFile(bw, or, overlay);
        }
      }

      // Write ARM7
      header.ARM7romOffset = (uint)bw.BaseStream.Position;
      header.ARM7size = arm7.size;
      WriteFile(bw, or, arm7);

      header.ARM7overlayOffset = 0;
      uint fatArm7overlayOffset = 0;
      if (header.ARM7overlaySize != 0)
      {
        // ARM7 Overlays Tables
        header.ARM7overlayOffset = (uint)bw.BaseStream.Position;
        header.ARM7overlaySize = y7.size;

        WriteFile(bw, or, y7);
        fatArm7overlayOffset = (uint)bw.BaseStream.Position;

        foreach (var overlay in ov7)
        {
          WriteFile(bw, or, overlay);
        }
      }

      // Write FNT
      header.FNToffset = (uint)bw.BaseStream.Position;
      header.FNTsize = fnt.size;
      WriteFile(bw, or, fnt);

      // Write FAT
      header.FAToffset = (uint)bw.BaseStream.Position;
      FileAllocationTable.WriteTo(outputStream, root, header.FAToffset, fatTable.sortedIDs, fatArm9overlayOffset, fatArm7overlayOffset);

      // Write banner
      header.bannerOffset = (uint)bw.BaseStream.Position;
      if (bannerFile.GetPath(filePath) != filePath)
      {
        banner = new Banner(bannerFile.path, bannerFile.offset);
      }
      banner.WriteTo(outputStream, header.bannerOffset);

      // Write files
      for (int i = 0; i < fatTable.sortedIDs.Length; i++)
      {
        if (i == 0 & fatTable.sortedIDs[i] > fatTable.sortedIDs.Length)
          continue;

        sFile currFile = FileNameTable.FindFile(fatTable.sortedIDs[i], root);
        if (currFile == null || currFile.name.StartsWith("overlay"))
        { // Los overlays no van en esta sección
          continue;
        }

        WriteFile(bw, or, currFile, i < fatTable.sortedIDs.Length - 1);
      }

      // Update the ROM size values of the header
      header.ROMsize = (uint)bw.BaseStream.Position;
      header.size = (int)Math.Pow(2, Math.Ceiling(Math.Log(bw.BaseStream.Position, 2)));

      // Get Secure CRC
      BinaryReader br = new BinaryReader(outputStream);
      outputStream.Position = 0x4000;
      byte[] secureArea = br.ReadBytes(0x4000);
      if (header.decrypted) { Encrypt.EncryptArm9(header.gameCode, ref secureArea); }
      ushort newSecureCRC16 = CRC16.Calculate(secureArea);
      header.secureCRC16 = newSecureCRC16;

      // Write header
      header.WriteTo(outputStream, 0);
      outputStream.Position = header.ROMsize;
      bw.Write(Enumerable.Repeat((byte)0xFF, (int)(header.size - header.ROMsize)).ToArray());

      originalStream.Close();
      outputStream.Close();
    }

    void WriteFile(BinaryWriter bw, BinaryReader or, sFile file, bool writePadding = true)
    {
      var reader = or;
      if (file.GetPath(filePath) != filePath)
      {
        reader = new BinaryReader(File.OpenRead(file.path));
      }
      reader.BaseStream.Position = file.offset;
      bw.Write(reader.ReadBytes((int)file.size));
      if (reader != or)
      {
        reader.Close();
      }

      if (writePadding)
      {
        WritePadding(bw, 0x200);
      }
    }

    void WritePadding(BinaryWriter bw, int paddingBase)
    {
      bw.Write(Enumerable.Repeat((byte)0xFF, (paddingBase - (int)bw.BaseStream.Position % paddingBase) % paddingBase).ToArray());
    }
  }
}
