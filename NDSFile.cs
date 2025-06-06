﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NitroHelper
{
  public class NDSFile
  {
    public string filePath;
    public Header header;
    public Banner banner;
    public FileAllocationTable fatTable;
    public FileNameTable fntTable;
    public OverlayTable overlay9Table;
    public OverlayTable overlay7Table;
    public sFolder root { get => fntTable.root; }
    public sFolder data { get => fntTable.root.folders[0]; }
    public sFolder overlay { get => fntTable.root.folders[1]; }
    public TWL twl;

    public NDSFile(string _filePath)
    {
      filePath = _filePath;
      var fileStream = File.OpenRead(filePath);

      // Load header, banner, fatTable
      header = new Header(fileStream);
      banner = new Banner(fileStream, header.bannerOffset, header.banner_size);
      fatTable = new FileAllocationTable(fileStream, header.FAToffset, header.FATsize);
      fntTable = new FileNameTable(fatTable, fileStream, header.FNToffset);

      // Load data and overlays
      var overlay = new sFolder()
      {
        name = "overlay",
        files = new List<sFile>(),
      };
      overlay9Table = new OverlayTable(fileStream, header.ARM9overlayOffset, header.ARM9overlaySize, true);
      var overlay9Files = overlay9Table.ReadBasicOverlays(fatTable);
      overlay.files.AddRange(overlay9Files);

      overlay7Table = new OverlayTable(fileStream, header.ARM7overlayOffset, header.ARM7overlaySize, false);
      overlay.files.AddRange(overlay7Table.ReadBasicOverlays(fatTable));
      root.folders.Add(overlay);

      // Read DSi stuff
      if (((byte)header.unitCode & 2) > 0 && (header.twlInternalFlags & 1) > 0)
      {
        // Read TWL rom data if the DSi ROM is valid 
        if (header.tid_high != 0 && header.tid_high != 0xFFFFFFFF)
        {
          // NOTE: Some DSi Enhanced ROMs is invalid!
          try
          {
            twl = new TWL(header, overlay9Files, fileStream);
          }
          catch { }
        }
      }

      // Add root
      root.files.Add(new sFile()
      {
        name = "header.bin",
        offset = 0,
        size = header.headerSize,
      });

      root.files.Add(new sFile()
      {
        name = "banner.bin",
        offset = header.bannerOffset,
        size = 0x840,
      });

      root.files.Add(new sFile()
      {
        name = "fnt.bin",
        offset = header.FNToffset,
        size = header.FNTsize,
      });

      root.files.Add(new sFile()
      {
        name = "fat.bin",
        offset = header.FAToffset,
        size = header.FATsize,
      });

      root.files.Add(new sFile()
      {
        name = "arm9.bin",
        offset = header.ARM9romOffset,
        size = header.ARM9size + (uint)(header.nitrocode ? 12 : 0),
      });

      root.files.Add(new sFile()
      {
        name = "arm7.bin",
        offset = header.ARM7romOffset,
        size = header.ARM7size,
      });

      if (header.ARM9overlaySize != 0)
      {
        root.files.Add(new sFile()
        {
          name = "overarm9.bin",
          offset = header.ARM9overlayOffset,
          size = header.ARM9overlaySize,
        });
      }

      if (header.ARM7overlaySize != 0)
      {
        root.files.Add(new sFile()
        {
          name = "overarm7.bin",
          offset = header.ARM7overlayOffset,
          size = header.ARM7overlaySize,
        });
      }

      fileStream.Close();
    }

    public void SaveAs(string outputPath)
    {
      var outputStream = File.Create(outputPath);
      SaveAs(outputStream);
      outputStream.Close();
    }

    public void SaveAs(Stream outputStream)
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
        if (y9.TryGetStream(out var stream))
        {
          overlay9Table = new OverlayTable(stream, 0, (uint)stream.Length, true);
        }
        ov9 = overlay.files.FindAll(sFile => sFile.name.StartsWith("overlay_"));
        ov9.Sort((sFile1, sFile2) => sFile1.id.CompareTo(sFile2.id));
        foreach (var ov9File in ov9)
        {
          var item = overlay9Table.overlayTable.Find(_ => _.fileId == ov9File.id);
          byte reservedType = (byte)((item.reserved & 0xFF000000) >> 24);
          if (reservedType % 2 > 0) { item.reserved = (uint)(reservedType << 24) + (ov9File.size & 0xFFFFFF); }
        }
      }

      index = systemFiles.FindIndex(sFile => sFile.name == "overarm7.bin");
      sFile y7 = new sFile();
      List<sFile> ov7 = new List<sFile>();
      if (index != -1)
      {
        y7 = systemFiles[index];
        if (y7.TryGetStream(out var stream))
        {
          overlay7Table = new OverlayTable(stream, 0, (uint)stream.Length, true);
        }
        ov7 = overlay.files.FindAll(sFile => sFile.name.StartsWith("overlay7_"));
        ov7.Sort((sFile1, sFile2) => sFile1.id.CompareTo(sFile2.id));
        foreach (var ov7File in ov7)
        {
          var item = overlay7Table.overlayTable.Find(_ => _.fileId == ov7File.id);
          byte reservedType = (byte)((item.reserved & 0xFF000000) >> 24);
          if (reservedType % 2 > 0) { item.reserved = (uint)(reservedType << 24) + (ov7File.size & 0xFFFFFF); }
          if (item.reserved > 0) { item.reserved = (item.reserved & 0xFF000000) + (ov7File.size & 0xFFFFFF); }
        }
      }

      if (headerFile.TryGetStream(out var stream1))
      {
        var newHeader = new Header(stream1);
        header.gameTitle = newHeader.gameTitle;
        header.gameCode = newHeader.gameCode;
      }

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
        header.ARM9overlaySize = overlay9Table.WriteTo(outputStream, header.ARM9overlayOffset);
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
        header.ARM7overlaySize = overlay7Table.WriteTo(outputStream, header.ARM7overlayOffset);
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
      FileAllocationTable.WriteTo(outputStream, root, header.FAToffset, fatTable.sortedIDs, header.banner_size, fatArm9overlayOffset, fatArm7overlayOffset);

      // Write banner
      header.bannerOffset = (uint)bw.BaseStream.Position;
      if (bannerFile.TryGetStream(out var stream2))
      {
        banner = new Banner(stream2, 0, header.banner_size);
      }
      header.banner_size = banner.WriteTo(outputStream, header.bannerOffset);

      // Get overlays (all system files)
      var ovlMaxId = overlay.files.Count - 1;
      // Write files
      for (int i = 0; i < fatTable.sortedIDs.Length; i++)
      {
        if (i == 0 & fatTable.sortedIDs[i] > fatTable.sortedIDs.Length) { continue; }
        if (fatTable.sortedIDs[i] <= ovlMaxId)
        {
          // Exclude overlay files by ID, because some files have name with "overlay" on begin
          continue;
        }

        sFile currFile = FileNameTable.FindFile(fatTable.sortedIDs[i], root);
        if (currFile == null)
        {
          // Overlays is not in this section
          continue;
        }

        WriteFile(bw, or, currFile, i < fatTable.sortedIDs.Length - 1);
      }

      if (twl != null && ((byte)header.unitCode & 2) > 0)
      {
        twl.WriteTo(outputStream, header, out header.hmac_digest_master);
        // TWL.UpdateHeaderSignatures(ref bw, ref header, header_file, keep_original);
      }

      // Update the ROM size values of the header
      header.ROMsize = (uint)bw.BaseStream.Position;
      header.size = (int)Math.Pow(2, Math.Ceiling(Math.Log(bw.BaseStream.Position, 2)));

      // Re-caclulate Secure CRC16
      BinaryReader br = new BinaryReader(outputStream);
      outputStream.Position = 0x4000;
      byte[] secureArea = br.ReadBytes(0x4000);
      if (header.decrypted) { Arm9Encryptor.Encrypt(header.gameCode, ref secureArea); }
      ushort newSecureCRC16 = CRC16.Calculate(secureArea);
      header.secureCRC16 = newSecureCRC16;

      // Write header
      header.WriteTo(outputStream, 0);
      outputStream.Position = header.ROMsize;

      // https://problemkaputt.de/gbatek-ds-wifi-nintendo-ds-download-play.htm
      if (header.dlp_signature.Length == 0x88)
      {
        bw.Write(header.dlp_signature);
      }
      bw.Write(Enumerable.Repeat((byte)0xFF, (int)(header.size - outputStream.Position)).ToArray());

      originalStream.Close();
    }

    void WriteFile(BinaryWriter bw, BinaryReader or, sFile file, bool writePadding = true)
    {
      var reader = or;
      if (file.TryGetStream(out var stream))
      {
        reader = new BinaryReader(stream);
      }
      reader.BaseStream.Position = file.offset;
      bw.Write(reader.ReadBytes((int)file.size));
      if (reader != or)
      {
        stream.Close();
        reader.Close();
      }

      if (writePadding)
      {
        bw.WritePadding(0x200);
      }
    }

    public override string ToString()
    {
      return $"NDS ROM: {new string(header.gameTitle)} ({new string(header.gameCode)})";
    }
  }
}
