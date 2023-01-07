using PdfLexer.DOM;
using PdfLexer.Parsers;
using System.Buffers;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace PdfLexer.Encryption;

internal enum FilterType
{
    Identity,
    AES,
    RC4
}

internal class StandardEncryption : IDecryptionHandler
{
    private readonly byte[] _baseKey;
    private readonly PdfDictionary _trailer;
    private readonly ParsingContext _ctx;
    private readonly StandardEncryptionInfo _ei;
    private static readonly byte[] padding = new byte[] {
        0x28, 0xBF, 0x4E, 0x5E, 0x4E, 0x75, 0x8A, 0x41, 0x64, 0x00, 0x4E, 0x56, 0xFF, 0xFA, 0x01, 0x08,
        0x2E, 0x2E, 0x00, 0xB6, 0xD0, 0x68, 0x3E, 0x80, 0x2F, 0x0C, 0xA9, 0xFE, 0x64, 0x53, 0x69, 0x7A,
    };
    private readonly byte[] _id;

    private FilterType aesString;
    private FilterType aesStreams;
    private FilterType aesEmbedded;

    public StandardEncryption(ParsingContext ctx, PdfDictionary trailer)
    {
        _trailer = trailer;
        _ctx = ctx;
        var id = trailer.Get<PdfArray>(PdfName.ID);
        if (id != null && id.Count > 0)
        {
            var i1 = id[0];
            if (i1.Type == PdfObjectType.StringObj)
            {
                _id = i1.GetAs<PdfString>().GetRawBytes();
            }
            else
            {
                _id = new byte[0];
            }
        }
        else
        {
            _id = new byte[0];
        }


        var enc = _trailer.Get<PdfDictionary>(PdfName.Encrypt);
        if (enc == null)
        {
            throw new PdfLexerException("Standard encryption missing encrypt dict");
        }

        _ei = new StandardEncryptionInfo(enc);
        _baseKey = Initialize(_ei);
        _ctx.IsEncrypted = true;
        lastkey = empty;
    }

    private int R;

    private byte[] Initialize(StandardEncryptionInfo ei)
    {
        // TODO validation
        var v = (int)ei.V;
        if (v != 1 && v != 2 && v != 4 && v != 5)
        {
            _ctx.Error("Encrypt V is not 1, 2, 4, or 5");
        }

        R = ei.R ?? 1;

        if (v == 4 || v == 5)
        {
            var cf = ei.CF;
            if (cf == null)
            {
                _ctx.Error("Encrypt dict missing CF.");
            }
            else
            {
                aesString = GetFilterType(ei, cf, PdfName.StrF);
                aesStreams = GetFilterType(ei, cf, PdfName.StmF);
                aesEmbedded = GetFilterType(ei, cf, PdfName.EFF, aesStreams);
            }
        }
        else
        {
            aesString = FilterType.RC4;
            aesStreams = FilterType.RC4;
            aesEmbedded = FilterType.RC4;
        }

        var ownerKey = GetKeyFromOwnerPass(ei);
        if (ownerKey != null)
        {
            return ownerKey; // TODO, may need both
        }
        _ctx.Options.OwnerPass = null;

        var userEmpty = GetKeyFromUserPassword(ei, empty);
        if (userEmpty != null) { return userEmpty; }

        var userKey = GetKeyFromUserPassword(ei);
        if (userKey == null)
        {
            throw new PdfLexerPasswordException("Passwords provided were not valid for pdf.");
            // _ctx.Error("Passwords provided were not valid for pdf.");
            // return empty;
        }

        return userKey;
    }

    private FilterType GetFilterType(StandardEncryptionInfo ei, PdfDictionary cf, PdfName key, FilterType def = FilterType.Identity)
    {
        var val = ei.NativeObject.Get<PdfName>(key);
        if (val == null) { return def; }


        var stmf = cf.Get<PdfDictionary>(val);
        if (stmf == null)
        {
            return def;
        }
        var cfm = stmf.Get<PdfName>(PdfName.CFM);
        if (cfm == null)
        {
            return def;
        }
        var sv = cfm.Value;
        return sv == "AESV2" || sv == "AESV3" ? FilterType.AES : FilterType.RC4;

    }
    private byte[]? GetKeyFromOwnerPass(StandardEncryptionInfo info)
    {
        if (info.R == 5 || info.R == 6)
        {
            return GetKeyFromOwnerPassAES256(info);
        }

        var key = BaseKey(info, _ctx.Options.OwnerPass, _ctx.Options.UserPass);

        if (info.O?.Value == null) { throw new PdfLexerException("O entry not in standard encryption info"); }
        var upw = info.O.GetRawBytes();

        switch (info.R)
        {
            case 2:
                upw = Rc4Encrypt(key, upw);
                break;
            case 3:
            case 4:
                for (var i = 19; i >= 0; i--)
                {
                    var newKey = key.ToArray();
                    for (var j = 0; j < newKey.Length; j++)
                    {
                        newKey[j] ^= (byte)i;
                    }

                    upw = Rc4Encrypt(newKey, upw);
                }
                break;
        }

        return GetKeyFromUserPassword(info, upw);
    }

    private byte[]? GetKeyFromUserPassword(StandardEncryptionInfo info)
    {
        if (string.IsNullOrEmpty(_ctx.Options.UserPass))
        {
            return GetKeyFromUserPassword(info, empty);
        }
        return GetKeyFromUserPassword(info, Encoding.UTF8.GetBytes(_ctx.Options.UserPass));
    }

    private byte[]? GetKeyFromUserPassword(StandardEncryptionInfo info, byte[] userPw)
    {
        if (info.R == 5 || info.R == 6)
        {
            return GetKeyFromUserPasswordAES256(info, userPw);
        }

        var key = GetUserEncKey(info, userPw);
        var digest = CaclulateUserPasswordDigest(info, key);
        switch (info.R)
        {
            case 2:
                if (info.U?.GetRawBytes().SequenceEqual(digest) ?? false)
                {
                    return key;
                }
                return null;
            case 3:
            case 4:
            default:
                var u = info.U_Bytes;
                if (digest.Length < 16 || u.Length < 16) { return null; }
                for (var i = 0; i < 16; i++)
                {
                    if (u[i] != digest[i])
                    {
                        return null;
                    }
                }
                return key;
        }

    }

    private byte[]? GetKeyFromUserPasswordAES256(StandardEncryptionInfo info, byte[] userPw)
    {
        if (userPw.Length > 127)
        {
            userPw = userPw[..127];
        }

        byte[] key;
        var u = info.U_Bytes;
        if (info.R == 5)
        {
            var s = Sha265Hash(userPw, GetValidationSalt(u));

            if (u.Length < 32 || s.Length < 32) { return null; }
            for (var i = 0; i < 32; i++)
            {
                if (u[i] != s[i]) { return null; }
            }

            key = Sha265Hash(userPw, GetKeySalt(u));
        }
        else
        {
            var input = new byte[userPw.Length + 8];
            userPw.CopyTo(input, 0);
            GetValidationSalt(u).CopyTo(input, userPw.Length);
            var s = IsoHash(userPw, input, empty);
            if (u.Length < 32 || s.Length < 32) { return null; }
            for (var i = 0; i < 32; i++)
            {
                if (u[i] != s[i]) { return null; }
            }

            var ks = GetKeySalt(u);
            input = new byte[userPw.Length + ks.Length];
            userPw.CopyTo(input, 0);
            ks.CopyTo(input, userPw.Length);
            key = IsoHash(userPw, input, empty);
        }

        var iv = new byte[16];
        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.None;
        using var memoryStream = new MemoryStream(info.UE_Bytes);
        using var output = new MemoryStream();
        using var cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(key, iv), CryptoStreamMode.Read);
        cryptoStream.CopyTo(output);
        var result = output.ToArray();
        return result;
    }

    private byte[] GetValidationSalt(byte[] data)
    {
        return data[32..40];
    }

    private byte[] GetKeySalt(byte[] data)
    {
        return data[40..];
    }

    private static byte[] empty = new byte[0];
    private static byte[] Sha265Hash(byte[] input1, byte[] input2, byte[]? input3 = null)
    {
        using (var sha = SHA256.Create())
        {

            sha.TransformBlock(input1, 0, input1.Length, null, 0);
            sha.TransformBlock(input2, 0, input2.Length, null, 0);

            if (input3 != null)
            {
                sha.TransformFinalBlock(input3, 0, input3.Length);
            }
            else
            {
                sha.TransformFinalBlock(empty, 0, 0);
            }

            return sha.Hash;
        }
    }

    private byte[] CaclulateUserPasswordDigest(StandardEncryptionInfo info, byte[] key)
    {
        byte[] u;
        switch (info.R)
        {
            case 2:
                u = padding.ToArray();
                u = Rc4Encrypt(key, u);
                break;
            case 3:
            case 4:
            default:
                using (var md5 = MD5.Create())
                {
                    md5.TransformBlock(padding, 0, padding.Length, null, 0);
                    md5.TransformBlock(_id, 0, _id.Length, null, 0);
                    md5.TransformFinalBlock(empty, 0, 0);
                    u = md5.Hash;
                    u = Rc4Encrypt(key, u);
                    for (var i = 1; i <= 19; i++)
                    {
                        var newKey = key.ToArray();
                        for (var j = 0; j < newKey.Length; j++)
                        {
                            newKey[j] ^= (byte)i;
                        }
                        u = Rc4Encrypt(newKey, u);
                    }
                }
                break;
        }

        if (u.Length < 32)
        {
            var padded = new byte[32];
            u.CopyTo(padded, 0);
            for (var i = u.Length; i < padded.Length; i++)
            {
                padded[i] = 32;
            }
            return padded;
        }
        return u;
    }


    private byte[]? GetKeyFromOwnerPassAES256(StandardEncryptionInfo info)
    {
        if (string.IsNullOrEmpty(_ctx.Options.OwnerPass))
        {
            return null;
        }

        var opw = Encoding.UTF8.GetBytes(_ctx.Options.OwnerPass);
        if (opw.Length > 127)
        {
            opw = opw[..127];
        }

        var o = info.O_Bytes;
        ReadOnlySpan<byte> os = o;
        var u = info.U_Bytes;

        byte[] key;

        if (info.R == 5)
        {
            ReadOnlySpan<byte> s = Sha265Hash(opw, GetValidationSalt(o), u);
            
            if (!s.Slice(0,32).SequenceEqual(os.Slice(0,32))) { return null; }

            key = Sha265Hash(opw, GetKeySalt(o), u);
        } else
        {
            // password
            // validation salt
            // user bytes
            var input = new byte[opw.Length + 8 + u.Length];
            opw.CopyTo(input, 0);
            GetValidationSalt(o).CopyTo(input, opw.Length);
            u.CopyTo(input, opw.Length + 8);
            ReadOnlySpan<byte> s = IsoHash(opw, input, u);

            if (!s.SequenceEqual(os.Slice(0, 32))) { return null; }

            // password
            // key salt
            // user bytes
            var ks = GetKeySalt(o);
            input = new byte[opw.Length + ks.Length + u.Length];
            opw.CopyTo(input, 0);
            ks.CopyTo(input, opw.Length);
            u.CopyTo(input, opw.Length+ks.Length);
            key = IsoHash(opw, input, u);
        }

        var iv = new byte[16];

        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.None;
        using var memoryStream = new MemoryStream(info.OE_Bytes);
        using var output = new MemoryStream();
        using var cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(key, iv), CryptoStreamMode.Read);
        cryptoStream.CopyTo(output);
        var result = output.ToArray();
        return result;
    }

    private byte[] BaseKey(StandardEncryptionInfo info, string? owner, string? user)
    {
        Encoding enc = Encoding.UTF8;
        if (info.R != 5 && info.R != 6)
        {
            enc = Encoding.GetEncoding("ISO-8859-1");
        }
        byte[] data;
        if (owner != null)
        {
            data = enc.GetBytes(owner);
        }
        else if (user != null)
        {
            data = enc.GetBytes(user);
        }
        else
        {
            data = new byte[0];
        }
        byte[] key;
        if (data.Length >= 32)
        {
            key = data[..32];
        }
        else
        {
            key = new byte[32];
            Array.Copy(data, key, data.Length);
            Array.Copy(padding, 0, key, data.Length, 32 - data.Length);
        }

        using (var md5 = MD5.Create())
        {
            var k = md5.ComputeHash(key);

            if (info.R >= 3)
            {
                for (var i = 0; i < 50; i++)
                {
                    k = md5.ComputeHash(k);
                }
                return k[..Math.Min(info.Length / 8, k.Length)];
            }

            return k[..5];
        }
    }

    private byte[] GetUserEncKey(StandardEncryptionInfo info, byte[] userPw)
    {
        byte[] key;
        if (userPw.Length >= 32)
        {
            key = userPw[..32];
        }
        else
        {
            key = new byte[32];
            Array.Copy(userPw, key, userPw.Length);
            Array.Copy(padding, 0, key, userPw.Length, 32 - userPw.Length);
        }

        using (var md5 = MD5.Create())
        {
            md5.TransformBlock(key, 0, key.Length, null, 0);

            var o = info.O;
            if (o?.Value == null)
            {
                throw new PdfLexerException("standard encryption missing O");
            }
            var ob = o.GetRawBytes();
            md5.TransformBlock(ob, 0, ob.Length, null, 0);

            var p = (uint)(info.P ?? 0);
            md5.TransformBlock(new byte[4] { (byte)p, (byte)(p >> 8), (byte)(p >> 16), (byte)(p >> 24), }, 0, 4, null, 0);

            md5.TransformBlock(_id, 0, _id.Length, null, 0);

            if (info.R == 4 && !info.EncryptMetadata)
            {
                md5.TransformBlock(new byte[4] { 0xff, 0xff, 0xff, 0xff }, 0, 4, null, 0);
            }
            md5.TransformFinalBlock(empty, 0, 0);
            var k = md5.Hash;

            if (info.R >= 3)
            {
                var l = info.Length / 8;
                for (var i = 0; i < 50; i++)
                {
                    // md5.Clear();
                    // md5.TransformBlock(k, 0, l, null, 0);
                    // md5.TransformFinalBlock(empty, 0, 0);
                    // k = md5.Hash;
                    if (l < k.Length)
                    {
                        k = k[..l];
                    }
                    k = md5.ComputeHash(k);
                }
                if (l < k.Length)
                {
                    return k[..l];
                }
                return k;
            }

            return k[..5];
        }
    }


    private bool lastAes;
    private ulong lastobj;
    private byte[] lastkey;
    private byte[] GetKey(ulong id, bool aes)
    {
        if (aes && (R == 5 || R == 6))
        {
            return _baseKey;
        }
        if (id == lastobj && lastAes == aes) { return lastkey; }
        var key = new byte[_baseKey.Length + (aes ? 9 : 5)];
        Array.Copy(_baseKey, key, _baseKey.Length);
        var i = _baseKey.Length;
        key[i++] = (byte)(id >> 16);
        key[i++] = (byte)(id >> 24);
        key[i++] = (byte)(id >> 32);
        key[i++] = (byte)(id);
        key[i++] = (byte)(id >> 8);
        if (aes)
        {
            key[i++] = (byte)'s';
            key[i++] = (byte)'A';
            key[i++] = (byte)'l';
            key[i++] = (byte)'T';
        }
        using (var md5 = MD5.Create())
        {
            var hash = md5.ComputeHash(key);
            var l = _baseKey.Length + 5;
            if (l < 16)
            {
                hash = hash[..l];
            }
            lastkey = hash;
            lastobj = id;
            lastAes = aes;
            return hash;
        }
    }

    private byte[] ivBuffer = new byte[16];
    public ReadOnlySpan<byte> Decrypt(ulong id, CryptoType type, ReadOnlySpan<byte> data, Span<byte> writeBuffer)
    {
        var filter = type switch
        {
            CryptoType.Streams => aesStreams,
            CryptoType.Embedded => aesEmbedded,
            _ => aesString,
        };

        if (filter == FilterType.Identity) { return data; }

        var aes = filter == FilterType.AES;

        var key = GetKey(id, aes);

        if (aes)
        {
            data.Slice(0, 16).CopyTo(ivBuffer);

            

            using var enc = Aes.Create();
            enc.Key = key;
            enc.IV = ivBuffer;

#if NET5_0_OR_GREATER

            if (!enc.TryDecryptCbc(data.Slice(16), ivBuffer, writeBuffer, out int bytes))
            {
                _ctx.Error($"Failed to decrypt data of {type} type from obj {id}");
                return data;
            }

            return writeBuffer.Slice(0, bytes);
#else
            var ms = new MemoryStream(data.Slice(16).ToArray());
            var decryptor = enc.CreateDecryptor(enc.Key, enc.IV);
            var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            var mso = new MemoryStream();
            cs.CopyTo(mso);
            return mso.ToArray();
#endif



        }
        else
        {
            Rc4Encrypt(key, data, writeBuffer);
            return writeBuffer.Slice(0, data.Length);
        }

    }
    private static byte[] emptyIV = new byte[16];
    public Stream Decrypt(ulong id, CryptoType type, Stream data)
    {
        var filter = type switch
        {
            CryptoType.Streams => aesStreams,
            CryptoType.Embedded => aesEmbedded,
            _ => aesString,
        };

        return DecryptInternal(id, filter, data);
    }

    private Stream DecryptInternal(ulong id, FilterType filter, Stream data)
    {
        if (filter == FilterType.Identity) { return data; }

        var aes = filter == FilterType.AES;

        var key = GetKey(id, aes);

        if (aes)
        {
            var iv = new byte[16];
            data.FillArray(iv);

            var enc = Aes.Create();
            enc.Key = key;
            enc.IV = iv;
            var decryptor = enc.CreateDecryptor(enc.Key, enc.IV);
            var wrapper = new CryptoWrapper(enc, decryptor, data);
            return wrapper;
        }
        else
        {
            if (data is not MemoryStream ms)
            {
                ms = new MemoryStream();
                data.CopyTo(ms);
                data.Dispose();
            }

            return new MemoryStream(Rc4Encrypt(key, ms.ToArray()));
        }
    }

    public Stream DecryptCryptStream(ulong id, PdfDictionary? decodeParams, Stream data)
    {
        var nm = decodeParams?.Get<PdfName>(PdfName.Name);
        if (nm == null) // Identity
        {
            return data;
        }
        var cd = _ei.CF?.Get<PdfDictionary>(nm);
        if (cd == null)
        {
            _ctx.Error($"Stream {id} specified crypt filter {nm} but was not found in CF dict");
            return data;
        }

        var type = GetFilterType(_ei, _ei.CF!, nm);
        return DecryptInternal(id, type, data);
    }

    public static byte[] Rc4Encrypt(byte[] key, byte[] data)
    {
        var result = new byte[data.Length];
        Rc4Encrypt(key, data, result);
        return result;
    }


    // below functions Rc4Encrypt and IsoHash ported from:
    // https://github.com/mozilla/pdf.js/blob/master/src/core/crypto.js
    // originally licenses as:
    /* Copyright 2012 Mozilla Foundation
     *
     * Licensed under the Apache License, Version 2.0 (the "License");
     * you may not use this file except in compliance with the License.
     * You may obtain a copy of the License at
     *
     *     http://www.apache.org/licenses/LICENSE-2.0
     *
     * Unless required by applicable law or agreed to in writing, software
     * distributed under the License is distributed on an "AS IS" BASIS,
     * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
     * See the License for the specific language governing permissions and
     * limitations under the License.
     */
    public static void Rc4Encrypt(byte[] key, ReadOnlySpan<byte> data, Span<byte> result)
    {
        Span<byte> s = stackalloc byte[256];
        for (var i = 0; i < 256; i++)
        {
            s[i] = (byte)i;
        }

        var j = 0;
        for (var i = 0; i < 256; i++)
        {
            var temp = s[i];
            j = (j + s[i] + key[i % key.Length]) % 256;
            s[i] = s[j];
            s[j] = temp;
        }

        var a = 0; var b = 0;
        for (var i = 0; i < data.Length; i++)
        {
            a = (a + 1) & 0xff;
            var tmp = s[a];
            b = (b + tmp) & 0xff;
            var tmp2 = s[b];
            s[a] = tmp2;
            s[b] = tmp;
            result[i] = (byte)(data[i] ^ s[(tmp + tmp2) & 0xff]);
        }
    }
    private byte[] IsoHash(byte[] password, byte[] input, byte[] userBytes)
    {
        using var sha256 = SHA256.Create();
        using var sha384 = SHA384.Create();
        using var sha512 = SHA512.Create();

        var k = sha256.ComputeHash(input)[..32];
        var e = new byte[] { 0x00 };

        var i = 0;
        while (i < 64 || e[e.Length - 1] > i - 32)
        {
            var combinedLength = password.Length + k.Length + userBytes.Length;
            var combinedArray = new byte[combinedLength];
            var writeOffset = 0;
            password.CopyTo(combinedArray, 0);
            writeOffset += password.Length;
            k.CopyTo(combinedArray, writeOffset);
            writeOffset += k.Length;
            userBytes.CopyTo(combinedArray, writeOffset);

            var k1 = new byte[combinedLength * 64];
            var pos = 0;
            for (var j = 0; j < 64; j++)
            {
                combinedArray.CopyTo(k1, pos);
                pos += combinedLength;
            }

            using (var aes = Aes.Create())
            {
                aes.Key = k[..16];
                aes.IV = k[16..32];
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.None;
                using (var enc = aes.CreateEncryptor())
                {
                    e = enc.TransformFinalBlock(k1, 0, k1.Length);
                }
            }

            var r = 0;
            for (var ii = 0; ii < 16; ii++)
            {
                r += e[ii];
            }
            r = r % 3;
            if (r == 0)
            {
                k = sha256.ComputeHash(e);
            }
            else if (r == 1)
            {
                k = sha384.ComputeHash(e);
            }
            else if (r == 2)
            {
                k = sha512.ComputeHash(e);
            }
            i++;
        }
        return k[..32];
    }
}

internal class CryptoWrapper : CryptoStream
{
    private readonly Aes _aes;
    private readonly ICryptoTransform _xform;

    public CryptoWrapper(Aes aes, ICryptoTransform xform, Stream data) : base(data, xform, CryptoStreamMode.Read)
    {
        _aes = aes;
        _xform = xform;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _xform?.Dispose();
            _aes?.Dispose();
        }
    }
}

