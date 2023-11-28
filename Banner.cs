using System.IO;
using System.Text;

namespace NitroHelper
{
  public class Banner
  {
    public ushort version;      // 1 - Standard 0x840, 2 - with Cninese, 3 - add Cninese and Korean titles, 0x0103 - add DSi animated icon
    public ushort bannerCRC16;        // CRC-16 across entries 0020h..083Fh (all versions)
    public ushort bannerCRC16_2;        // CRC-16 across entries 0020h..093Fh (Version 0002h and up)
    public ushort bannerCRC16_3;        // CRC-16 across entries 0020h..0A3Fh (Version 0003h and up)
    public ushort bannerCRC16_i;        // CRC-16 across entries 1240h..23BFh (Version 0103h and up)
    public bool bannerCRC;
    public byte[] reserved = new byte[22];
    public byte[] tileData = new byte[512];
    public byte[] palette = new byte[32];
    public string japaneseTitle = "";
    public string englishTitle = "";
    public string frenchTitle = "";
    public string germanTitle = "";
    public string italianTitle = "";
    public string spanishTitle = "";
    // Version 2-3
    public string chineseTitle = ""; // 256 bytes
    public string koreanTitle = "";  // 256 bytes
    // DSi Enchansed
    public byte[] reservedDsi;  // 0x800 bytes reserved for Title 8..15
    public byte[] aniIconData;  // 0x1180 bytes (8 * tiles + 8 * palette + 8 * 16 bytes of animation sequence)
    // padding bytes
    //public byte[] padding1;   // 1C0h Unused/padding (FFh-filled) in Version 0001h
    public byte[] padding2;     // C0h  Unused/padding (FFh-filled) in Version 0002h
    public byte[] padding3;     // 1C0h Unused/padding (FFh-filled) in Version 0003h
    //public byte[] padding4;   // 40h  Unused/padding (FFh-filled) in Version 0103h

    public uint GetDefSize(uint hardBannerSize = 0)
    {
      switch (version)
      {
        case 1:
        default:
          return 0x840;
        case 2:
          return 0x940;
        case 3:
          return 0xA40;
        case 0x0103:
          return hardBannerSize > 0 && (int)hardBannerSize != -1 ? hardBannerSize : 0x23C0;
      }
    }

    public Banner(string filePath, uint offset = 0, uint size = 0) : this(true, File.OpenRead(filePath), offset, size) { }

    public Banner(Stream stream, uint offset = 0, uint size = 0) : this(false, stream, offset, size) { }

    private Banner(bool close, Stream stream, uint offset = 0, uint size = 0)
    {
      BinaryReader br = new BinaryReader(stream);
      br.BaseStream.Position = offset;

      version = br.ReadUInt16();
      bannerCRC16 = br.ReadUInt16();
      bannerCRC16_2 = br.ReadUInt16();
      bannerCRC16_3 = br.ReadUInt16();
      bannerCRC16_i = br.ReadUInt16();
      reserved = br.ReadBytes(0x16);
      tileData = br.ReadBytes(0x200);
      palette = br.ReadBytes(0x20);
      japaneseTitle = TitleToString(br.ReadBytes(0x100));
      englishTitle = TitleToString(br.ReadBytes(0x100));
      frenchTitle = TitleToString(br.ReadBytes(0x100));
      germanTitle = TitleToString(br.ReadBytes(0x100));
      italianTitle = TitleToString(br.ReadBytes(0x100));
      spanishTitle = TitleToString(br.ReadBytes(0x100));

      // Version 2-3
      //byte v = (byte)(version & 0xFF);
      if (version >= 2) { chineseTitle = TitleToString(br.ReadBytes(0x100)); }
      if (version >= 3) { koreanTitle = TitleToString(br.ReadBytes(0x100)); }
      if (version == 2) { padding2 = br.ReadBytes(0xC0); }
      if (version == 3) { padding3 = br.ReadBytes(0x1C0); }
      // DSi Enhanced
      if ((version >> 8) == 1)
      {
        reservedDsi = br.ReadBytes(0x800);
        if (size == 0 || size == 0xFFFFFFFF || size > 0x23C0) { size = 0x23C0; }
        aniIconData = br.ReadBytes((int)(offset + size - br.BaseStream.Position));
      }

      stream.Position = offset + 0x20;
      bannerCRC = CRC16.Calculate(br.ReadBytes(0x820)) == bannerCRC16;

      if (close) { stream.Close(); }
    }

    public uint WriteTo(string filePath, uint offset = 0) => WriteTo(true, File.Create(filePath), offset);

    public uint WriteTo(Stream stream, uint offset = 0) => WriteTo(false, stream, offset);

    private uint WriteTo(bool close, Stream stream, uint offset = 0)
    {
      BinaryWriter bw = new BinaryWriter(stream);
      stream.Position = offset;

      bw.Write(version);
      bw.Write(bannerCRC16);
      bw.Write(bannerCRC16_2);
      bw.Write(bannerCRC16_3);
      bw.Write(bannerCRC16_i);
      bw.Write(reserved);
      bw.Write(tileData);
      bw.Write(palette);
      bw.Write(StringToTitle(japaneseTitle));
      bw.Write(StringToTitle(englishTitle));
      bw.Write(StringToTitle(frenchTitle));
      bw.Write(StringToTitle(germanTitle));
      bw.Write(StringToTitle(italianTitle));
      bw.Write(StringToTitle(spanishTitle));

      // Version 2-3
      if (version >= 2) bw.Write(StringToTitle(chineseTitle));
      if (version >= 3) bw.Write(StringToTitle(koreanTitle));
      // DSi Enchansed
      if ((version >> 8) == 1)
      {
        bw.Write(reservedDsi);
        bw.Write(aniIconData);
      }

      uint size = (uint)(bw.BaseStream.Position - offset);
      bw.WritePadding(0x200);

      // Re-caclulate CRC16
      var currentPosition = stream.Position;
      BinaryReader br = new BinaryReader(stream);
      stream.Position = offset + 0x20;
      bannerCRC16 = CRC16.Calculate(br.ReadBytes(0x820));
      if (version >= 2)
      {
        stream.Position = offset + 0x20;
        bannerCRC16_2 = CRC16.Calculate(br.ReadBytes(0x920));
      }
      if (version >= 3)
      {
        stream.Position = offset + 0x20;
        bannerCRC16_3 = CRC16.Calculate(br.ReadBytes(0xA20));
      }
      if ((version >> 8) >= 1)
      {
        stream.Position = offset + 0x1240;
        bannerCRC16_i = CRC16.Calculate(br.ReadBytes(0x1180));
      }
      stream.Position = offset + 0x02;
      bw.Write(bannerCRC16);
      bw.Write(bannerCRC16_2);
      bw.Write(bannerCRC16_3);
      bw.Write(bannerCRC16_i);
      stream.Position = currentPosition;

      if (close) { stream.Close(); }
      return size;
    }

    private static string TitleToString(byte[] data)
    {
      string title = Encoding.Unicode.GetString(data);
      return title.Substring(0, title.IndexOf('\0'));
    }

    private static byte[] StringToTitle(string title)
    {
      byte[] data = new byte[0x100];
      Encoding.Unicode.GetBytes(title).CopyTo(data, 0);
      return data;
    }
  }
}
