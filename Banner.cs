using System.IO;
using System.Text;

namespace NitroHelper
{
  public class Banner
  {
    public ushort version;      // Always 1
    public ushort bannerCRC16;        // CRC-16 of structure, not including first 32 bytes
    public bool bannerCRC;
    public byte[] reserved = new byte[28];     // 28 bytes
    public byte[] tileData = new byte[512];     // 512 bytes
    public byte[] palette = new byte[32];      // 32 bytes
    public string japaneseTitle = "";// 256 bytes
    public string englishTitle = ""; // 256 bytes
    public string frenchTitle = "";  // 256 bytes
    public string germanTitle = "";  // 256 bytes
    public string italianTitle = ""; // 256 bytes
    public string spanishTitle = ""; // 256 bytes

    public Banner(string filePath, uint offset = 0) : this(File.OpenRead(filePath), offset) { }

    public Banner(Stream stream, uint offset = 0)
    {
      BinaryReader br = new BinaryReader(stream);
      br.BaseStream.Position = offset;

      version = br.ReadUInt16();
      bannerCRC16 = br.ReadUInt16();
      reserved = br.ReadBytes(0x1C);
      tileData = br.ReadBytes(0x200);
      palette = br.ReadBytes(0x20);
      japaneseTitle = TitleToString(br.ReadBytes(0x100));
      englishTitle = TitleToString(br.ReadBytes(0x100));
      frenchTitle = TitleToString(br.ReadBytes(0x100));
      germanTitle = TitleToString(br.ReadBytes(0x100));
      italianTitle = TitleToString(br.ReadBytes(0x100));
      spanishTitle = TitleToString(br.ReadBytes(0x100));

      stream.Position = offset + 0x20;
      bannerCRC = CRC16.Calculate(br.ReadBytes(0x820)) == bannerCRC16;
    }

    public void WriteTo(string filePath) => WriteTo(File.Create(filePath));

    public void WriteTo(Stream stream, uint offset = 0)
    {
      BinaryWriter bw = new BinaryWriter(stream);
      stream.Position = offset;

      bw.Write(version);
      bw.Write(bannerCRC16);
      bw.Write(reserved);
      bw.Write(tileData);
      bw.Write(palette);
      bw.Write(StringToTitle(japaneseTitle));
      bw.Write(StringToTitle(englishTitle));
      bw.Write(StringToTitle(frenchTitle));
      bw.Write(StringToTitle(germanTitle));
      bw.Write(StringToTitle(italianTitle));
      bw.Write(StringToTitle(spanishTitle));

      int rem = (int)stream.Position % 0x200;
      if (rem != 0)
      {
        while (rem++ < 0x200)
        {
          bw.Write((byte)0xFF);
        }
      }

      // Re-caclulate CRC16
      var currentPosition = stream.Position;
      BinaryReader br = new BinaryReader(stream);
      stream.Position = offset + 0x20;
      ushort newCRC16 = CRC16.Calculate(br.ReadBytes(0x820));
      stream.Position = offset + 0x02;
      bw.Write(newCRC16);
      stream.Position = currentPosition;
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