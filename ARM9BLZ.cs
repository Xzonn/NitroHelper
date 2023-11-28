using System;
using System.Linq;
using Xzonn.BlzHelper;

namespace NitroHelper
{
  internal class ARM9BLZ
  {
    /// <summary>
    /// Decompress ARM9.bin
    /// </summary>
    /// <param name="arm9Data">Compressed ARM9.bin data</param>
    /// <param name="header">ROM header</param>
    /// <param name="decompressed">Decompressed data</param>
    /// <returns>True if the decompression was successful.</returns>
    public static bool Decompress(byte[] arm9Data, Header header, out byte[] decompressed)
    {
      decompressed = arm9Data;
      uint nitrocode_length = 0;
      if (BitConverter.ToUInt32(arm9Data, 0xC) == 0xDEC00621)
      {
        nitrocode_length = 0x0C; //Nitrocode found.
      }
      uint initptr = BitConverter.ToUInt32(header.reserved2, 0) & 0x3FFF;
      uint hdrptr = BitConverter.ToUInt32(arm9Data, (int)initptr + 0x14);
      if (initptr == 0)
      {
        hdrptr = header.ARM9ramAddress + header.ARM9size;
      }
      uint postSize = (uint)arm9Data.Length - (hdrptr - header.ARM9ramAddress);
      bool cmparm9 = hdrptr > header.ARM9ramAddress && hdrptr + nitrocode_length >= header.ARM9ramAddress + arm9Data.Length;
      if (cmparm9)
      {
        decompressed = BLZ.Decompress(arm9Data.Take((int)(arm9Data.Length - nitrocode_length)).ToArray()).Concat(arm9Data.Skip((int)(arm9Data.Length - nitrocode_length))).ToArray();
      }

      return cmparm9;
    }

    /// <summary>
    /// Compress ARM9.bin
    /// </summary>
    /// <param name="arm9Data">Uncompressed ARM9.bin data</param>
    /// <param name="hdr">ROM header</param>
    /// <param name="postSize">Data size from the end what will be ignored.</param>
    /// <returns>Compressed data with uncompressed Secure Area (first 0x4000 bytes).</returns>
    public static byte[] Compress(byte[] arm9Data, Header hdr, uint postSize = 0)
    {
      uint nitrocode_length = 0;
      if (BitConverter.ToUInt32(arm9Data, 0xC) == 0xDEC00621)
      {
        nitrocode_length = 0x0C; //Nitrocode found.
      }
      var result = arm9Data.Take(0x4000).Concat(BLZ.Compress(arm9Data.Skip(0x4000).Take((int)(arm9Data.Length - 0x4000 - nitrocode_length)).ToArray())).Concat(arm9Data.Skip((int)(arm9Data.Length - nitrocode_length))).ToArray();

      // Update size
      uint initptr = BitConverter.ToUInt32(hdr.reserved2, 0) & 0x3FFF;
      if (initptr > 0)
      {
        uint hdrptr = (uint)result.Length - postSize + hdr.ARM9ramAddress;
        Array.Copy(BitConverter.GetBytes(hdrptr), 0, result, initptr + 0x14, 4);
      }

      return result;
    }
  }
}
