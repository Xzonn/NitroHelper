using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace NitroHelper
{
  public class TWL
  {
    private static byte[] modcryptCmnKey =
      {
        0x79, 0x3E, 0x4F, 0x1A, 0x5F, 0x0F, 0x68, 0x2A, 0x58, 0x02, 0x59, 0x29, 0x4E,
        0xFB, 0xFE, 0xFF
      };

    internal static byte[] hmac_sha1_key =
      {
        0x21, 0x06, 0xC0, 0xDE, 0xBA, 0x98, 0xCE, 0x3F, 0xA6, 0x92, 0xE3, 0x9D, 0x46, 0xF2, 0xED, 0x01,
        0x76, 0xE3, 0xCC, 0x08, 0x56, 0x23, 0x63, 0xFA, 0xCA, 0xD4, 0xEC, 0xDF, 0x9A, 0x62, 0x78, 0x34,
        0x8F, 0x6D, 0x63, 0x3C, 0xFE, 0x22, 0xCA, 0x92, 0x20, 0x88, 0x97, 0x23, 0xD2, 0xCF, 0xAE, 0xC2,
        0x32, 0x67, 0x8D, 0xFE, 0xCA, 0x83, 0x64, 0x98, 0xAC, 0xFD, 0x3E, 0x37, 0x87, 0x46, 0x58, 0x24,
      };

    internal static byte[] rsaPublicKey =
      {
        0x95, 0x6F, 0x79, 0x0D, 0xF0, 0x8B, 0xB8, 0x5A, 0x76, 0xAA, 0xEF, 0xA2, 0x7F, 0xE8, 0x74, 0x75,
        0x8B, 0xED, 0x9E, 0xDF, 0x9E, 0x9A, 0x67, 0x0C, 0xD8, 0x18, 0xBE, 0xB9, 0xB2, 0x88, 0x52, 0x03,
        0xB3, 0xFA, 0x11, 0xAE, 0xAA, 0x18, 0x65, 0x13, 0xB5, 0xD6, 0xBB, 0x85, 0xA3, 0x84, 0xD0, 0xD0,
        0xEF, 0xB3, 0x66, 0xCB, 0xC6, 0x05, 0x1A, 0xAA, 0x86, 0x82, 0x7A, 0xB7, 0x43, 0x11, 0xF5, 0x9C,
        0x9B, 0xFC, 0x6C, 0x70, 0x79, 0xD5, 0xF1, 0x7B, 0xD0, 0x81, 0x9F, 0x52, 0x20, 0x56, 0x73, 0x8C,
        0x72, 0x1F, 0x40, 0xCF, 0x23, 0x61, 0x93, 0x25, 0x90, 0xA3, 0xC5, 0xDC, 0x94, 0xCF, 0xD1, 0x7A,
        0x8C, 0xBC, 0x95, 0x4A, 0x91, 0x8A, 0xA8, 0x58, 0xF4, 0xD8, 0x04, 0xBA, 0xF7, 0xD3, 0xC1, 0xC4,
        0xD7, 0xB8, 0xF0, 0x77, 0x01, 0x2F, 0xA1, 0x70, 0x26, 0x0B, 0x2C, 0x04, 0x90, 0x56, 0xF3, 0xA5
      };

    internal static byte[] rsaSignatureMask =
      {
        0x00, 0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0xCC, 0xCC, 0xCC, 0xCC,
        0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC
      };

    internal static byte[] rsaFontKey =
      {
        0x9F, 0x80, 0xBC, 0x5F, 0xB6, 0xB6, 0x1D, 0x2A, 0x46, 0x02, 0x52, 0x64, 0xB2, 0xA3, 0x86, 0xCE,
        0xE6, 0x54, 0xD3, 0xA9, 0x70, 0x5B, 0xE3, 0xC2, 0x10, 0xA9, 0xB5, 0x2F, 0x38, 0xC5, 0x51, 0xFB,
        0xB5, 0xD1, 0x80, 0xFD, 0xFF, 0x20, 0x65, 0xC1, 0x28, 0x4D, 0x56, 0xBE, 0xFB, 0xBD, 0x3F, 0xE4,
        0xBA, 0xF7, 0x9C, 0x3A, 0x33, 0x74, 0x74, 0x9D, 0xDB, 0xDD, 0x9E, 0x86, 0x05, 0x2C, 0xAD, 0xFC,
        0x93, 0xFA, 0xFB, 0x08, 0xEA, 0x71, 0x18, 0x36, 0xC5, 0xDC, 0x4C, 0x06, 0x34, 0x57, 0xA7, 0x8F,
        0x4E, 0x82, 0xF7, 0xB3, 0xE2, 0x9C, 0xE4, 0x72, 0xE3, 0xDC, 0x60, 0xAF, 0xCC, 0x18, 0xE2, 0xD4,
        0xEF, 0xD2, 0x76, 0x47, 0x31, 0xE6, 0x14, 0x0E, 0x1D, 0x26, 0xB5, 0x85, 0x97, 0xBC, 0xC6, 0xB6,
        0xD8, 0xE7, 0x69, 0x2D, 0x2C, 0x26, 0xFB, 0x5F, 0x70, 0x9E, 0x19, 0x9C, 0x6B, 0x02, 0x6D, 0x97
      };

    private uint firstDSiHashOffset;
    private uint dsiHashSize;
    bool twlEncrypted;

    public byte[][] Overlays9Sha1Hmac { get; private set; }
    public byte[][] Header2Data { get; private set; }
    public byte[] DSi9Data { get; private set; }
    public byte[] DSi7Data { get; private set; }
    public byte[] Hashtable1Data { get; private set; }
    public byte[] Hashtable2Data { get; private set; }

    public TWL(Header header, sFile[] overlays, string file, uint offset = 0) : this(true, header, overlays, File.OpenRead(file), offset) { }

    public TWL(Header header, sFile[] overlays, Stream stream, uint offset = 0) : this(false, header, overlays, stream, offset) { }

    public TWL(bool close, Header header, sFile[] overlays, Stream stream, uint offset = 0)
    {
      firstDSiHashOffset = header.digest_ntr_size / header.digest_sector_size * 0x14;
      dsiHashSize = header.digest_twl_size / header.digest_sector_size * 0x14;

      // Read data
      BinaryReader br = new BinaryReader(stream);
      stream.Position = header.sector_hashtable_start;
      Hashtable1Data = br.ReadBytes((int)header.sector_hashtable_size);
      stream.Position = header.block_hashtable_start;
      Hashtable2Data = br.ReadBytes((int)header.block_hashtable_size);
      stream.Position = header.dsi9_rom_offset;
      DSi9Data = br.ReadBytes(Math.Max((int)header.modcrypt1_size, (int)header.dsi9_size));
      stream.Position = header.dsi7_rom_offset;
      DSi7Data = br.ReadBytes(Math.Max((int)header.modcrypt2_size, (int)header.dsi7_size));
      if (!header.trimmedRom)
      {
        Header2Data = new byte[3][];
        for (int i = 0; i < 3; i++)
        {
          //stream.Position = header.digest_ntr_start + 0x4000;
          stream.Position = header.digest_twl_start - 0x3000 + 0x1000 * i;
          Header2Data[i] = br.ReadBytes(0x1000);
        }
      }

      // Calc SHA1-HMAC of overlays9
      HMACSHA1 hmac = new HMACSHA1(hmac_sha1_key);
      Overlays9Sha1Hmac = new byte[overlays.Length][];
      for (int i = 0; i < overlays.Length; i++)
      {
        stream.Position = overlays[i].offset;
        byte[] ovlData = br.ReadBytes((int)overlays[i].size);
        Overlays9Sha1Hmac[i] = hmac.ComputeHash(ovlData, 0, ovlData.Length);
      }

      // Check for encryption modcrypt section
      twlEncrypted = false; // (header.twlInternalFlags & 2) > 0;
      if (header.modcrypt1_start >= header.digest_twl_start && header.modcrypt1_start < 0xFFFFFFFF)
      {
        uint modcryptHashedSectorIndex = (header.modcrypt1_start - header.digest_twl_start) / header.digest_sector_size;
        uint modcryptHashedSectorOff = modcryptHashedSectorIndex * header.digest_sector_size;
        uint hashOff = modcryptHashedSectorIndex * 0x14;

        stream.Position = header.digest_twl_start + modcryptHashedSectorOff;
        byte[] firstModcryptBlock = br.ReadBytes((int)header.digest_sector_size);
        byte[] hash = hmac.ComputeHash(firstModcryptBlock, 0, (int)header.digest_sector_size);
        for (int i = 0; i < 20 && !twlEncrypted; i++) { twlEncrypted = hash[i] != Hashtable1Data[firstDSiHashOffset + hashOff + i]; }
      }
      if (close) { stream.Close(); }

      // Decrypt modcrypt sections
      if (twlEncrypted)
      {
        byte[] key = AES128KeyGenerate(header);
        byte[] counter = new byte[16];
        if (header.modcrypt1_size > 0 && header.modcrypt1_size < 0xFFFFFFFF)
        {
          Array.Copy(header.hmac_arm9, 0, counter, 0, 16);
          uint _offset = header.modcrypt1_start - header.dsi9_rom_offset;
          byte[] decrypted = AES128CTRCrypt(key, counter, DSi9Data, _offset, header.modcrypt1_size);
          if (_offset == 0 && decrypted.Length == DSi9Data.Length)
          {
            DSi9Data = decrypted;
          }
          else
          {
            Array.Copy(decrypted, 0, DSi9Data, _offset, decrypted.Length);
          }
        }

        if (header.modcrypt2_size > 0 && header.modcrypt2_size < 0xFFFFFFFF)
        {
          Array.Copy(header.hmac_arm7, 0, counter, 0, 16);
          uint _offset = header.modcrypt2_start - header.dsi7_rom_offset;
          byte[] decrypted = AES128CTRCrypt(key, counter, DSi7Data, _offset, header.modcrypt2_size);
          if (_offset == 0 && decrypted.Length == DSi7Data.Length)
          {
            DSi7Data = decrypted;
          }
          else
          {
            Array.Copy(decrypted, 0, DSi7Data, _offset, decrypted.Length);
          }
        }
      }

      hmac.Clear();
      hmac.Dispose();
      twlEncrypted = (header.twlInternalFlags & 2) > 0;
    }

    public void UpdateOverlays9Sha1Hmac(ref sFile arm9, Header header, List<sFile> ov9, HMACSHA1 hmac = null)
    {
      if (hmac == null) hmac = new HMACSHA1(hmac_sha1_key);
      byte[][] hashes = new byte[ov9.Count][];
      bool changed = false;
      for (int i = 0; i < ov9.Count; i++)
      {
        Stream str = File.OpenRead(ov9[i].path);
        byte[] buffer = new byte[ov9[i].size];
        str.Position = ov9[i].offset;
        str.Read(buffer, 0, buffer.Length);
        str.Close();
        hashes[i] = hmac.ComputeHash(buffer);
        for (int j = 0; j < hashes[i].Length && !changed; j++) changed |= hashes[i][j] != Overlays9Sha1Hmac[i][j];
      }

      if (changed)
      {
        // Read arm9
        BinaryReader br = new BinaryReader(File.OpenRead(arm9.path));
        br.BaseStream.Position = arm9.offset;
        byte[] arm9Data = br.ReadBytes((int)arm9.size);
        br.Close();

        // Decompress arm9
        uint initptr = BitConverter.ToUInt32(header.reserved2, 0) & 0x3FFF;
        uint hdrptr = BitConverter.ToUInt32(arm9Data, (int)initptr + 0x14) - header.ARM9ramAddress;
        bool cmparm9 = ARM9BLZ.Decompress(arm9Data, header, out arm9Data);

        // Get hmac _offset
        uint offset = 0;
        uint end = BitConverter.ToUInt32(arm9Data, (int)initptr + 8) - header.ARM9ramAddress;
        for (long i = end - 0x14 * ov9.Count; i >= 0 && offset == 0; i--)
        {
          bool cond = arm9Data[i] == Overlays9Sha1Hmac[0][0];
          for (int j = 1; j < 20 && cond; j++) cond = arm9Data[i + j] == Overlays9Sha1Hmac[0][j];
          if (cond) offset = (uint)i;
        }

        // Write new hash
        if (offset > 0)
        {
          for (int i = 0; i < ov9.Count; i++) Array.Copy(hashes[i], 0, arm9Data, offset + i * 0x14, 20);
          if (!cmparm9)
          {
            arm9Data = ARM9BLZ.Compress(arm9Data, header, arm9.size - hdrptr);
          }

          string arm9Binary = Path.GetTempFileName();
          File.WriteAllBytes(arm9Binary, arm9Data);
          arm9.path = arm9Binary;
          arm9.offset = 0;
          arm9.size = (uint)arm9Data.Length;
        }
        else Console.WriteLine("Overlays9 has been modified but can't update hashes in ARM9.bin!");
      }
    }

    public void ImportArm9iData(string filePath, uint offset, uint size)
    {
      uint sizePad = size;
      if (size % 0x10 != 0) sizePad += 0x10 - size % 0x10;
      DSi9Data = new byte[sizePad];
      for (long i = size; i < sizePad; i++) DSi9Data[i] = 0xFF;

      Stream str = File.OpenRead(filePath);
      str.Position = offset;
      str.Read(DSi9Data, 0, (int)size);
      str.Close();
    }

    public void ImportArm7iData(string filePath, uint offset, uint size)
    {
      uint sizePad = size;
      if (size % 0x10 != 0) sizePad += 0x10 - size % 0x10;
      DSi7Data = new byte[sizePad];
      for (long i = size; i < sizePad; i++) DSi7Data[i] = 0xFF;

      Stream str = File.OpenRead(filePath);
      str.Position = offset;
      str.Read(DSi7Data, 0, (int)size);
      str.Close();
    }

    public void WriteTo(string filePath, Header header, out byte[] digest_master_hash, uint offset = 0) => WriteTo(true, File.Create(filePath), header, out digest_master_hash, offset);

    public void WriteTo(Stream stream, Header header, out byte[] digest_master_hash, uint offset = 0) => WriteTo(false, stream, header, out digest_master_hash, offset);

    private void WriteTo(bool close, Stream stream, Header header, out byte[] digest_master_hash, uint offset = 0)
    {
      BinaryWriter bw = new BinaryWriter(stream);

      // Write DSi ARM sections and padding
      while (stream.Position < header.sector_hashtable_start) bw.Write((byte)0xFF);
      stream.Position = header.sector_hashtable_start + header.sector_hashtable_size;
      while (stream.Position < header.block_hashtable_start) bw.Write((byte)0xFF);
      stream.Position = header.block_hashtable_start + header.block_hashtable_size;
      while (stream.Position < header.dsi9_rom_offset) bw.Write((byte)0xFF);
      if (Header2Data != null && !header.trimmedRom)
      {
        stream.Position = header.digest_twl_start - 0x3000;
        for (int j = 0; j < 3; j++) { bw.Write(Header2Data[j]); }
      }

      bw.Write(DSi9Data, 0, DSi9Data.Length);
      while (stream.Position < header.dsi7_rom_offset) bw.Write((byte)0xFF);
      bw.Write(DSi7Data, 0, DSi7Data.Length);
      while (stream.Position < header.total_rom_size) bw.Write((byte)0xFF);
      long pos = stream.Position;

      // Compute NTR Secure Area Hashtable
      int i = 0;
      HMACSHA1 hmac = new HMACSHA1(hmac_sha1_key);
      BinaryReader br = new BinaryReader(stream);
      stream.Position = header.digest_ntr_start;
      byte[] secureArea = br.ReadBytes(0x4000);
      if (header.decrypted) { Arm9Encryptor.Encrypt(header.gameCode, ref secureArea); }
      while (i < 0x4000 / header.digest_sector_size)
      {
        byte[] hash = hmac.ComputeHash(secureArea, (int)(i * header.digest_sector_size), (int)header.digest_sector_size);
        stream.Position = header.sector_hashtable_start + i * 0x14;
        bw.Write(hash);
        i++;
      }

      // Compute NTR Hashtable
      stream.Position = header.digest_ntr_start + 0x4000;
      while (stream.Position < header.digest_ntr_start + header.digest_ntr_size)
      {
        byte[] hash = hmac.ComputeHash(br.ReadBytes((int)header.digest_sector_size));
        long tmp = stream.Position;
        stream.Position = header.sector_hashtable_start + i * 0x14;
        bw.Write(hash);
        stream.Position = tmp;
        i++;
      }

      // Compute TWL Hashtable
      stream.Position = header.digest_twl_start;
      while (stream.Position < header.digest_twl_start + header.digest_twl_size)
      {
        byte[] hash = hmac.ComputeHash(br.ReadBytes((int)header.digest_sector_size));
        long tmp = stream.Position;
        stream.Position = header.sector_hashtable_start + i * 0x14;
        bw.Write(hash);
        stream.Position = tmp;
        i++;
      }

      // Compute Secondary Hashtable
      i = 0;
      stream.Position = header.sector_hashtable_start;
      while (stream.Position < header.sector_hashtable_start + header.sector_hashtable_size)
      {
        byte[] hash = hmac.ComputeHash(br.ReadBytes((int)header.digest_block_sectorcount * 0x14));
        long tmp = stream.Position;
        stream.Position = header.block_hashtable_start + i * 0x14;
        bw.Write(hash);
        stream.Position = tmp;
        i++;
      }

      // Compute Master Hashtable
      stream.Position = header.block_hashtable_start;
      digest_master_hash = hmac.ComputeHash(br.ReadBytes((int)header.block_hashtable_size));

      // Encrypt DSi sections
      if (twlEncrypted)
      {
        byte[] key = AES128KeyGenerate(header);
        byte[] counter9 = new byte[16];
        byte[] counter7 = new byte[16];
        Array.Copy(header.hmac_arm9, 0, counter9, 0, 16);
        Array.Copy(header.hmac_arm7, 0, counter7, 0, 16);
        stream.Position = header.dsi9_rom_offset;
        if (header.modcrypt1_size > 0) bw.Write(AES128CTRCrypt(key, counter9, DSi9Data, 0, header.modcrypt1_size));
        stream.Position = header.dsi7_rom_offset;
        if (header.modcrypt2_size > 0) bw.Write(AES128CTRCrypt(key, counter7, DSi7Data, 0, header.modcrypt2_size));
      }

      stream.Position = pos;
      if (close) { stream.Close(); }
    }

    public static void UpdateHeaderSignatures(ref BinaryWriter bw, ref Header header, string header_file, bool keep_original)
    {
      long pos = bw.BaseStream.Position;

      // Update digest master hash
      bw.BaseStream.Position = 0x328;
      bw.Write(header.hmac_digest_master);

      // Read signed header data
      BinaryReader br = new BinaryReader(File.OpenRead(header_file));
      byte[] hdrSignedData = br.ReadBytes(0xE00);
      Array.Copy(header.hmac_digest_master, 0, hdrSignedData, 0x328, 0x14);
      br.Close();

      // Verify RSA Signature
      RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(1024);
      RSAParameters rsaKey = new RSAParameters
      {
        Exponent = new byte[] { 1, 0, 1 },
        Modulus = (byte[])rsaPublicKey.Clone(),
        D = null // In future: here set Private key
      };
      rsa.ImportParameters(rsaKey);
      bool verify = rsa.VerifyData(hdrSignedData, new SHA1CryptoServiceProvider(), header.rsa_signature);
      if (!verify)
      {
        // RSA encrypt signature
        if (rsaKey.D != null)
        {
          header.rsa_signature = rsa.SignData(hdrSignedData, new SHA1CryptoServiceProvider());
        }
        else
        {
          // Calc SHA1 hash
          SHA1 sha1 = new SHA1CryptoServiceProvider();
          byte[] hash = sha1.ComputeHash(hdrSignedData);
          //Array.Reverse(hash);
          sha1.Clear();
          sha1.Dispose();

          if (!keep_original)
          {
            // Set unencrypted signature for no$gba compatible
            header.rsa_signature = (byte[])rsaSignatureMask.Clone();
            Array.Copy(hash, 0, header.rsa_signature, 0x80 - 0x14, 0x14);
          }
        }

        // Write signature
        bw.BaseStream.Position = 0xF80;
        bw.Write(header.rsa_signature, 0, 0x80);
      }

      bw.BaseStream.Position = pos;
    }

    private static byte[] AES128CTRCrypt(byte[] key, byte[] counter, byte[] data, uint offset, uint size)
    {
      AES128CounterMode aes128 = new AES128CounterMode(counter);
      ICryptoTransform ict = aes128.CreateEncryptor(key, null);
      return ict.TransformFinalBlock(data, (int)offset, (int)size);
    }

    private static byte[] AES128KeyGenerate(Header hdr)
    {
      bool debug = ((hdr.twlInternalFlags & 4) > 0) || ((hdr.appflags[3] & 0x80) > 0);
      if (debug)
      {
        byte[] key = new byte[16];
        Array.Copy(Encoding.ASCII.GetBytes(hdr.gameTitle), 0, key, 0, 12);
        Array.Copy(Encoding.ASCII.GetBytes(hdr.gameCode), 0, key, 12, 4);
        return key;
      }
      else
      {
        byte[] keyX = new byte[16];
        byte[] keyY = new byte[16];
        Array.Copy(Encoding.ASCII.GetBytes("Nintendo"), 0, keyX, 0, 8);
        Array.Copy(Encoding.ASCII.GetBytes(hdr.gameCode), 0, keyX, 8, 4);
        for (int j = 0; j < 4; j++) keyX[12 + j] = (byte)hdr.gameCode[3 - j];
        //Array.Copy(BitConverter.GetBytes(header.tid_low), 0, keyX, 12, 4);
        Array.Copy(hdr.hmac_arm9i, 0, keyY, 0, 16);
        return AES128KeyGenerate(keyX, keyY);
      }
    }

    private static byte[] AES128KeyGenerate(byte[] keyX, byte[] keyY)
    {
      // Key = ((Key_X XOR Key_Y) + FFFEFB4E295902582A680F5F1A4F3E79h) ROL 42
      byte[] key = new byte[16];
      for (int i = 0; i < 16; i++) key[i] = (byte)(keyX[i] ^ keyY[i]);

      ulong[] tmp = new ulong[2];
      ulong[] xyKey = new[] { BitConverter.ToUInt64(key, 0), BitConverter.ToUInt64(key, 8) };
      ulong[] cKey = new[] { BitConverter.ToUInt64(modcryptCmnKey, 0), BitConverter.ToUInt64(modcryptCmnKey, 8) };
      tmp[0] = (cKey[0] >> 1) + (xyKey[0] >> 1) + (cKey[0] & xyKey[0] & 1);
      tmp[0] = tmp[0] >> 63;
      cKey[0] = cKey[0] + xyKey[0];
      cKey[1] = cKey[1] + xyKey[1] + tmp[0];

      int shift = 42;
      tmp[0] = cKey[0] << shift;
      tmp[1] = cKey[1] << shift;
      tmp[0] |= (cKey[1] >> (64 - shift));
      tmp[1] |= (cKey[0] >> (64 - shift));
      cKey[0] = tmp[0];
      cKey[1] = tmp[1];

      Array.Copy(BitConverter.GetBytes(cKey[0]), 0, key, 0, 8);
      Array.Copy(BitConverter.GetBytes(cKey[1]), 0, key, 8, 8);

      return key;
    }
  }
}
