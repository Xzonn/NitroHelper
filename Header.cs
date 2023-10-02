using System;
using System.IO;

namespace NitroHelper
{
  public class Header
  {
    public char[] gameTitle = new char[12];
    public char[] gameCode = new char[4];
    public char[] makerCode = new char[2];
    public byte unitCode;
    public byte encryptionSeed;
    public int size;
    public byte[] reserved = new byte[9];
    public byte ROMversion;
    public byte internalFlags;
    public uint ARM9romOffset;
    public uint ARM9entryAddress;
    public uint ARM9ramAddress;
    public uint ARM9size;
    public uint ARM7romOffset;
    public uint ARM7entryAddress;
    public uint ARM7ramAddress;
    public uint ARM7size;
    public uint FNToffset;
    public uint FNTsize;
    public uint FAToffset;            // File Allocation Table offset
    public uint FATsize;              // File Allocation Table size
    public uint ARM9overlayOffset;      // ARM9 overlay file offset
    public uint ARM9overlaySize;
    public uint ARM7overlayOffset;
    public uint ARM7overlaySize;
    public uint flagsRead;            // Control register flags for read
    public uint flagsInit;            // Control register flags for init
    public uint bannerOffset;           // Icon + titles offset
    public ushort secureCRC16;          // Secure area CRC16 0x4000 - 0x7FFF
    public ushort ROMtimeout;
    public uint ARM9autoload;
    public uint ARM7autoload;
    public ulong secureDisable;        // Magic number for unencrypted mode
    public uint ROMsize;
    public uint headerSize;
    public byte[] reserved2 = new byte[56];            // 56 bytes
    public byte[] logo = new byte[156];               // 156 bytes de un logo de nintendo usado para comprobaciones de seguridad
    public ushort logoCRC16;
    public ushort headerCRC16;
    public bool secureCRC;
    public bool logoCRC;
    public bool headerCRC;
    public uint debug_romOffset;      // only if debug
    public uint debug_size;           // version with
    public uint debug_ramAddress;     // 0 = none, SIO and 8 MB
    public uint reserved3;            // Zero filled transfered and stored but not used
    public byte[] reserved4 = Array.Empty<byte>();          // 0x90 bytes => Zero filled transfered but not stored in RAM
    public readonly bool nitrocode;
    public readonly bool decrypted;

    public Header(string file, uint offset = 0) : this(true, File.OpenRead(file), offset) { }

    public Header(Stream stream, uint offset = 0) : this(false, stream, offset) { }

    private Header(bool close, Stream stream, uint offset = 0)
    {
      BinaryReader br = new BinaryReader(stream);

      gameTitle = br.ReadChars(12);
      gameCode = br.ReadChars(4);
      makerCode = br.ReadChars(2);
      unitCode = br.ReadByte();
      encryptionSeed = br.ReadByte();
      size = (int)Math.Pow(2, 17 + br.ReadByte());
      reserved = br.ReadBytes(9);
      ROMversion = br.ReadByte();
      internalFlags = br.ReadByte();
      ARM9romOffset = br.ReadUInt32();
      ARM9entryAddress = br.ReadUInt32();
      ARM9ramAddress = br.ReadUInt32();
      ARM9size = br.ReadUInt32();
      ARM7romOffset = br.ReadUInt32();
      ARM7entryAddress = br.ReadUInt32();
      ARM7ramAddress = br.ReadUInt32();
      ARM7size = br.ReadUInt32();
      FNToffset = br.ReadUInt32();
      FNTsize = br.ReadUInt32();
      FAToffset = br.ReadUInt32();
      FATsize = br.ReadUInt32();
      ARM9overlayOffset = br.ReadUInt32();
      ARM9overlaySize = br.ReadUInt32();
      ARM7overlayOffset = br.ReadUInt32();
      ARM7overlaySize = br.ReadUInt32();
      flagsRead = br.ReadUInt32();
      flagsInit = br.ReadUInt32();
      bannerOffset = br.ReadUInt32();
      secureCRC16 = br.ReadUInt16();
      ROMtimeout = br.ReadUInt16();
      ARM9autoload = br.ReadUInt32();
      ARM7autoload = br.ReadUInt32();
      secureDisable = br.ReadUInt64();
      ROMsize = br.ReadUInt32();
      headerSize = br.ReadUInt32();
      reserved2 = br.ReadBytes(56);
      logo = br.ReadBytes(156); // Logo de Nintendo utilizado para comprobaciones
      logoCRC16 = br.ReadUInt16();
      headerCRC16 = br.ReadUInt16();
      debug_romOffset = br.ReadUInt32();
      debug_size = br.ReadUInt32();
      debug_ramAddress = br.ReadUInt32();
      reserved3 = br.ReadUInt32();
      reserved4 = br.ReadBytes((int)(headerSize - (stream.Position - offset)));

      if (br.BaseStream.Length >= 0x4000)
      {
        var position = br.BaseStream.Position;
        br.BaseStream.Position = offset + 0x0;
        headerCRC = CRC16.Calculate(br.ReadBytes(0x15E)) == headerCRC16;
        logoCRC = CRC16.Calculate(logo) == logoCRC16;

        // Nitrocode?
        br.BaseStream.Position = offset + ARM9romOffset + ARM9size;
        nitrocode = br.ReadUInt32() == 0xDEC00621;

        // ROM Type
        // https://github.com/devkitPro/ndstool/blob/a0ae6b5b7604e89dc94a2db01a97efcec41fc9fc/source/header.cpp#L42
        br.BaseStream.Position = offset + 0x4000;
        if (br.ReadUInt64() == 0xE7FFDEFFE7FFDEFF) { decrypted = true; }

        br.BaseStream.Position = offset + 0x4000;
        byte[] secureArea = br.ReadBytes(0x4000);
        if (decrypted) { Encrypt.EncryptArm9(gameCode, ref secureArea); }
        secureCRC = CRC16.Calculate(secureArea) == secureCRC16;

        br.BaseStream.Position = position;
      }

      if (close) { stream.Close(); }
    }

    public void WriteTo(string filePath, uint offset = 0) => WriteTo(true, File.Create(filePath), offset);

    public void WriteTo(Stream stream, uint offset = 0) => WriteTo(false, stream, offset);

    private void WriteTo(bool close, Stream stream, uint offset = 0)
    {
      BinaryWriter bw = new BinaryWriter(stream);
      stream.Position = offset;

      bw.Write(gameTitle);
      bw.Write(gameCode);
      bw.Write(makerCode);
      bw.Write(unitCode);
      bw.Write(encryptionSeed);
      bw.Write((byte)(Math.Log(size, 2) - 17));
      bw.Write(reserved);
      bw.Write(ROMversion);
      bw.Write(internalFlags);
      bw.Write(ARM9romOffset);
      bw.Write(ARM9entryAddress);
      bw.Write(ARM9ramAddress);
      bw.Write(ARM9size);
      bw.Write(ARM7romOffset);
      bw.Write(ARM7entryAddress);
      bw.Write(ARM7ramAddress);
      bw.Write(ARM7size);
      bw.Write(FNToffset);
      bw.Write(FNTsize);
      bw.Write(FAToffset);
      bw.Write(FATsize);
      bw.Write(ARM9overlayOffset);
      bw.Write(ARM9overlaySize);
      bw.Write(ARM7overlayOffset);
      bw.Write(ARM7overlaySize);
      bw.Write(flagsRead);
      bw.Write(flagsInit);
      bw.Write(bannerOffset);
      bw.Write(secureCRC16);
      bw.Write(ROMtimeout);
      bw.Write(ARM9autoload);
      bw.Write(ARM7autoload);
      bw.Write(secureDisable);
      bw.Write(ROMsize);
      bw.Write(headerSize);
      bw.Write(reserved2);
      bw.Write(logo);

      ushort newLogoCRC16 = CRC16.Calculate(logo);
      bw.Write(newLogoCRC16);

      bw.Write(headerCRC16);
      bw.Write(debug_romOffset);
      bw.Write(debug_size);
      bw.Write(debug_ramAddress);
      bw.Write(reserved3);
      bw.Write(reserved4);

      // Re-caclulate CRC16
      var currentPosition = stream.Position;
      BinaryReader br = new BinaryReader(stream);
      stream.Position = offset;
      ushort newCRC16 = CRC16.Calculate(br.ReadBytes(0x15E));
      bw.Write(newCRC16);
      stream.Position = currentPosition;

      if (close) { stream.Close(); }
    }
  }
}
