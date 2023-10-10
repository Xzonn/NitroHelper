using System.IO;
using System.Text;

namespace NitroHelper
{
  public class Banner
  {
    public ushort version;
    public ushort bannerCRC16;
    public bool bannerCRC;
    public byte[] reserved = new byte[28];
    public byte[] tileData = new byte[512];
    public byte[] palette = new byte[32];
    public string japaneseTitle = "";
    public string englishTitle = "";
    public string frenchTitle = "";
    public string germanTitle = "";
    public string italianTitle = "";
    public string spanishTitle = "";

    public Banner(string filePath, uint offset = 0) : this(true, File.OpenRead(filePath), offset) { }

    public Banner(Stream stream, uint offset = 0) : this(false, stream, offset) { }

    private Banner(bool close, Stream stream, uint offset = 0)
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

      if (close) { stream.Close(); }
    }

    public void WriteTo(string filePath, uint offset = 0) => WriteTo(true, File.Create(filePath), offset);

    public void WriteTo(Stream stream, uint offset = 0) => WriteTo(false, stream, offset);

    private void WriteTo(bool close, Stream stream, uint offset = 0)
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

      bw.WritePadding(0x200);

      // Re-caclulate CRC16
      var currentPosition = stream.Position;
      BinaryReader br = new BinaryReader(stream);
      stream.Position = offset + 0x20;
      ushort newCRC16 = CRC16.Calculate(br.ReadBytes(0x820));
      stream.Position = offset + 0x02;
      bw.Write(newCRC16);
      stream.Position = currentPosition;

      if (close) { stream.Close(); }
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
