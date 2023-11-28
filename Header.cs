using System;
using System.IO;

namespace NitroHelper
{
  public enum UnitCode : byte
  {
    NDS = 0,
    NDS_DSi = 2,
    DSi = 3
  }

  public class Header
  {
    // https://dsibrew.org/wiki/DSi_cartridge_header
    // https://github.com/R-YaTian/TinkeDSi/blob/main/Tinke/Nitro/Estructuras.cs
    public char[] gameTitle = new char[12];
    public char[] gameCode = new char[4];
    public char[] makerCode = new char[2];
    public UnitCode unitCode;
    public byte encryptionSeed;
    public int size;
    public byte[] reserved = new byte[7];
    public byte twlInternalFlags; // bit0 - Has TWL-Exclusive Region, bit1 - Modcrypted, bit2 - Debug, bit3 - Disable Debug
    public byte permitsFlags;  // bit0 - Permit Receiving Normal Jump, bit1 - Permit Receiving Temporary Jump, bit6 - For Korea, bit7 - For China
    public byte ROMversion;
    public byte internalFlags; // bit0 - ARM9 Compressed, bit1 - ARM7 Compressed, bit2 - Auto Boot, bit3 - Disable/Clear Initial Program Loader Memory Pad(?), bit4 - Cache ROM Reads, bit6 - ROM Type Not Specified, bit7 - RomPulledOutType = ROMID?
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
    public uint FAToffset;
    public uint FATsize;
    public uint ARM9overlayOffset;
    public uint ARM9overlaySize;
    public uint ARM7overlayOffset;
    public uint ARM7overlaySize;
    public uint flagsRead;
    public uint flagsInit;
    public uint bannerOffset;
    public ushort secureCRC16;
    public ushort ROMtimeout;
    public uint ARM9autoload;
    public uint ARM7autoload;
    public ulong secureDisable;
    public uint ROMsize;
    public uint headerSize;
    public byte[] reserved2 = new byte[56];
    public byte[] logo = new byte[156];
    public ushort logoCRC16;
    public ushort headerCRC16;
    public bool secureCRC;
    public bool logoCRC;
    public bool headerCRC;
    public uint debug_romOffset;
    public uint debug_size;
    public uint debug_ramAddress;
    public uint reserved3;
    public byte[] reserved4 = new byte[16];

    // DSi extended stuff below
    public byte[][] global_mbk_setting = new byte[5][]; //[5][4];
    public uint[] arm9_mbk_setting = new uint[3];     //[3];
    public uint[] arm7_mbk_setting = new uint[3];     //[3];
    public uint mbk9_wramcnt_setting;

    public uint region_flags;           // Region flags (bit0=JPN, bit1=USA, bit2=EUR, bit3=AUS, bit4=CHN, bit5=KOR, bit6-31=Reserved) (FFFFFFFFh=Region Free)
    public uint access_control;
    public uint scfg_ext_mask;
    public byte[] appflags;             //[4]; bit24 - Use TWL Sound Codec bit25 Require EULA Agreement, bit26 - Has Sub Banner, bit27 - Show Nintendo Wi-Fi Connection icon in Launcher, bit28 - Show DS Wireless icon in Launcher, bit31 - Developer App

    public uint dsi9_rom_offset;
    public uint offset_0x1C4;
    public uint dsi9_ram_address;
    public uint dsi9_size;
    public uint dsi7_rom_offset;
    public uint offset_0x1D4;
    public uint dsi7_ram_address;
    public uint dsi7_size;

    public uint digest_ntr_start;       // 0x4000
    public uint digest_ntr_size;        // ntr rom size without header 0x4000
    public uint digest_twl_start;       // padding to digest_sector_size
    public uint digest_twl_size;        // arm9i and arm7i include

    public uint sector_hashtable_start; // padding to digest_sector_size
    public uint sector_hashtable_size;  // padding to (0x14 * digest_block_sectorcount)
    public uint block_hashtable_start;
    public uint block_hashtable_size;

    public uint digest_sector_size;
    public uint digest_block_sectorcount;
    public uint banner_size;
    public uint offset_0x20C;

    public uint total_rom_size;
    public uint offset_0x214;
    public uint offset_0x218;
    public uint offset_0x21C;

    public uint modcrypt1_start;        // dsi9_rom_offset
    public uint modcrypt1_size;
    public uint modcrypt2_start;        // dsi7_rom_offset;
    public uint modcrypt2_size;

    public uint tid_low;                // inversed GAME ID
    public uint tid_high;
    public uint public_sav_size;
    public uint private_sav_size;

    public byte[] reserved5;             //[0xB0];
    public byte[] age_ratings;           //[0x10];
    public byte[] hmac_arm9;             //[20];
    public byte[] hmac_arm7;             //[20];
    public byte[] hmac_digest_master;    //[20];
    public byte[] hmac_icon_title;       //[20];
    public byte[] hmac_arm9i;            //[20];
    public byte[] hmac_arm7i;            //[20];
    public byte[] reserved6;             //[40];
    public byte[] hmac_arm9_no_secure;   //[20];
    public byte[] reserved7;             //[0xA4C];
    public byte[] debug_args;            //[0x180];
    public byte[] rsa_signature;         //[0x80];

    public bool trimmedRom;
    public bool doublePadding;

    /* Parental Control Age Ratings (for different countries/areas)
     *     Bit7: Rating exists for local country/area
     *     Bit6: Game is prohibited in local country/area?
     *     Bit5-0: Age rating for local country/area(years)
     *     2F0h 1    CERO(Japan)       (0=None/A, 12=B, 15=C, 17=D, 18=Z)
     *     2F1h 1    ESRB(US/Canada)   (0=None, 3=EC, 6=E, 10=E10+, 13=T, 17=M)
     *     2F2h 1    Reserved          (0=None)
     *     2F3h 1    USK(Germany)      (0=None, 6=6+, 12=12+, 16=16+, 18=18+)
     *     2F4h 1    PEGI(Pan-Europe)  (0=None, 3=3+, 7=7+, 12=12+, 16=16+, 18=18+)
     *     2F5h 1    Reserved          (0=None)
     *     2F6h 1    PEGI(Portugal)    (0=None, 4=4+, 6=6+, 12=12+, 16=16+, 18=18+)
     *     2F7h 1    PEGI and BBFC(UK) (0=None, 3, 4=4+/U, 7, 8=8+/PG, 12, 15, 16, 18)
     *     2F8h 1    AGCB(Australia)   (0=None/G, 7=PG, 14=M, 15=MA15+, plus 18=R18+?)
     *     2F9h 1    GRB(South Korea)  (0=None, 12=12+, 15=15+, 18=18+)
     *     2FAh 6    Reserved(6x)      (0=None) */

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
      unitCode = (UnitCode)br.ReadByte();
      encryptionSeed = br.ReadByte();
      size = (int)Math.Pow(2, 17 + br.ReadByte());
      reserved = br.ReadBytes(7);
      twlInternalFlags = br.ReadByte();
      permitsFlags = br.ReadByte();
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
      logo = br.ReadBytes(156);
      logoCRC16 = br.ReadUInt16();
      headerCRC16 = br.ReadUInt16();
      debug_romOffset = br.ReadUInt32();
      debug_size = br.ReadUInt32();
      debug_ramAddress = br.ReadUInt32();
      reserved3 = br.ReadUInt32();

      // DSi extended stuff below
      if (headerSize > 0x200 && ((byte)unitCode & 2) > 0)
      {
        reserved4 = br.ReadBytes(16);
        for (int i = 0; i < 5; i++) { global_mbk_setting[i] = br.ReadBytes(4); }
        for (int i = 0; i < 3; i++) { arm9_mbk_setting[i] = br.ReadUInt32(); }
        for (int i = 0; i < 3; i++) { arm7_mbk_setting[i] = br.ReadUInt32(); }
        mbk9_wramcnt_setting = br.ReadUInt32();

        region_flags = br.ReadUInt32();
        access_control = br.ReadUInt32();
        scfg_ext_mask = br.ReadUInt32();
        appflags = br.ReadBytes(4);

        dsi9_rom_offset = br.ReadUInt32();
        offset_0x1C4 = br.ReadUInt32();
        dsi9_ram_address = br.ReadUInt32();
        dsi9_size = br.ReadUInt32();
        dsi7_rom_offset = br.ReadUInt32();
        offset_0x1D4 = br.ReadUInt32();
        dsi7_ram_address = br.ReadUInt32();
        dsi7_size = br.ReadUInt32();

        digest_ntr_start = br.ReadUInt32();
        digest_ntr_size = br.ReadUInt32();
        digest_twl_start = br.ReadUInt32();
        digest_twl_size = br.ReadUInt32();

        sector_hashtable_start = br.ReadUInt32();
        sector_hashtable_size = br.ReadUInt32();
        block_hashtable_start = br.ReadUInt32();
        block_hashtable_size = br.ReadUInt32();

        digest_sector_size = br.ReadUInt32();
        digest_block_sectorcount = br.ReadUInt32();
        banner_size = br.ReadUInt32();
        offset_0x20C = br.ReadUInt32();

        total_rom_size = br.ReadUInt32();
        offset_0x214 = br.ReadUInt32();
        offset_0x218 = br.ReadUInt32();
        offset_0x21C = br.ReadUInt32();

        modcrypt1_start = br.ReadUInt32();
        modcrypt1_size = br.ReadUInt32();
        modcrypt2_start = br.ReadUInt32();
        modcrypt2_size = br.ReadUInt32();

        tid_low = br.ReadUInt32();
        tid_high = br.ReadUInt32();
        public_sav_size = br.ReadUInt32();
        private_sav_size = br.ReadUInt32();

        reserved5 = br.ReadBytes(0xB0);
        age_ratings = br.ReadBytes(0x10);
        hmac_arm9 = br.ReadBytes(20);
        hmac_arm7 = br.ReadBytes(20);
        hmac_digest_master = br.ReadBytes(20);
        hmac_icon_title = br.ReadBytes(20);
        hmac_arm9i = br.ReadBytes(20);
        hmac_arm7i = br.ReadBytes(20);
        reserved6 = br.ReadBytes(40);
        hmac_arm9_no_secure = br.ReadBytes(20);
        reserved7 = br.ReadBytes(0xA4C);
        debug_args = br.ReadBytes(0x180);
        rsa_signature = br.ReadBytes(0x80);

        //trimmedRom = br.BaseStream.Length == total_rom_size;
        doublePadding = (ARM9size % 0x400) < 0x200 && ARM7romOffset % 0x400 == 0 && ARM9overlayOffset % 0x400 == 0;
        doublePadding |= (ARM7size % 0x400) < 0x200 && FNToffset % 0x400 == 0 && ARM7overlayOffset % 0x400 == 0;
        doublePadding |= (FNTsize % 0x400) < 0x200 && FAToffset % 0x400 == 0;
        doublePadding |= (FATsize % 0x400) < 0x200 && bannerOffset % 0x400 == 0;
      }
      else
      {
        reserved4 = br.ReadBytes((int)(headerSize - (stream.Position - offset)));
      }
      if (total_rom_size != 0)
      {
        trimmedRom = total_rom_size - br.BaseStream.Length >= 0;
      }
      else
      {
        trimmedRom = br.BaseStream.Length != size;
      }

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
        if (decrypted) { Arm9Encryptor.Encrypt(gameCode, ref secureArea); }
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
      bw.Write((byte)unitCode);
      bw.Write(encryptionSeed);
      bw.Write((byte)(Math.Log(size, 2) - 17));
      bw.Write(reserved);
      bw.Write(twlInternalFlags);
      bw.Write(permitsFlags);
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

      // Write DSi rom info
      if (headerSize > 0x200 && ((byte)unitCode & 2) > 0)
      {
        for (int i = 0; i < 5; i++) bw.Write(global_mbk_setting[i]);
        for (int i = 0; i < 3; i++) bw.Write(arm9_mbk_setting[i]);
        for (int i = 0; i < 3; i++) bw.Write(arm7_mbk_setting[i]);
        bw.Write(mbk9_wramcnt_setting);

        bw.Write(region_flags);
        bw.Write(access_control);
        bw.Write(scfg_ext_mask);
        bw.Write(appflags);

        bw.Write(dsi9_rom_offset);
        bw.Write(offset_0x1C4);
        bw.Write(dsi9_ram_address);
        bw.Write(dsi9_size);
        bw.Write(dsi7_rom_offset);
        bw.Write(offset_0x1D4);
        bw.Write(dsi7_ram_address);
        bw.Write(dsi7_size);

        bw.Write(digest_ntr_start);
        bw.Write(digest_ntr_size);
        bw.Write(digest_twl_start);
        bw.Write(digest_twl_size);

        bw.Write(sector_hashtable_start);
        bw.Write(sector_hashtable_size);
        bw.Write(block_hashtable_start);
        bw.Write(block_hashtable_size);

        bw.Write(digest_sector_size);
        bw.Write(digest_block_sectorcount);
        bw.Write(banner_size);
        bw.Write(offset_0x20C);

        bw.Write(total_rom_size);
        bw.Write(offset_0x214);
        bw.Write(offset_0x218);
        bw.Write(offset_0x21C);

        bw.Write(modcrypt1_start);
        bw.Write(modcrypt1_size);
        bw.Write(modcrypt2_start);
        bw.Write(modcrypt2_size);

        bw.Write(tid_low);
        bw.Write(tid_high);
        bw.Write(public_sav_size);
        bw.Write(private_sav_size);

        bw.Write(reserved5);
        bw.Write(age_ratings);
        bw.Write(hmac_arm9);
        bw.Write(hmac_arm7);
        bw.Write(hmac_digest_master);
        bw.Write(hmac_icon_title);
        bw.Write(hmac_arm9i);
        bw.Write(hmac_arm7i);
        bw.Write(reserved6);
        bw.Write(hmac_arm9_no_secure);
        bw.Write(reserved7);
        bw.Write(debug_args);
        bw.Write(rsa_signature);
      }

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
