using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace PdfLexer.CMaps
{
    public class KnownCMaps
    {
        public static IReadOnlyList<string> Names = new List<string>
        {
            "78-euc-h",
            "78-euc-v",
            "78-h",
            "78-rksj-h",
            "78-rksj-v",
            "78-v",
            "78ms-rksj-h",
            "78ms-rksj-v",
            "83pv-rksj-h",
            "90ms-rksj-h",
            "90ms-rksj-v",
            "90msp-rksj-h",
            "90msp-rksj-v",
            "90pv-rksj-h",
            "90pv-rksj-v",
            "add-h",
            "add-rksj-h",
            "add-rksj-v",
            "add-v",
            "adobe-cns1-0",
            "adobe-cns1-1",
            "adobe-cns1-2",
            "adobe-cns1-3",
            "adobe-cns1-4",
            "adobe-cns1-5",
            "adobe-cns1-6",
            "adobe-cns1-ucs2",
            "adobe-gb1-0",
            "adobe-gb1-1",
            "adobe-gb1-2",
            "adobe-gb1-3",
            "adobe-gb1-4",
            "adobe-gb1-5",
            "adobe-gb1-ucs2",
            "adobe-japan1-0",
            "adobe-japan1-1",
            "adobe-japan1-2",
            "adobe-japan1-3",
            "adobe-japan1-4",
            "adobe-japan1-5",
            "adobe-japan1-6",
            "adobe-japan1-ucs2",
            "adobe-korea1-0",
            "adobe-korea1-1",
            "adobe-korea1-2",
            "adobe-korea1-ucs2",
            "b5-h",
            "b5-v",
            "b5pc-h",
            "b5pc-v",
            "cns-euc-h",
            "cns-euc-v",
            "cns1-h",
            "cns1-v",
            "cns2-h",
            "cns2-v",
            "eten-b5-h",
            "eten-b5-v",
            "etenms-b5-h",
            "etenms-b5-v",
            "ethk-b5-h",
            "ethk-b5-v",
            "euc-h",
            "euc-v",
            "ext-h",
            "ext-rksj-h",
            "ext-rksj-v",
            "ext-v",
            "gb-euc-h",
            "gb-euc-v",
            "gb-h",
            "gb-v",
            "gbk-euc-h",
            "gbk-euc-v",
            "gbk2k-h",
            "gbk2k-v",
            "gbkp-euc-h",
            "gbkp-euc-v",
            "gbpc-euc-h",
            "gbpc-euc-v",
            "gbt-euc-h",
            "gbt-euc-v",
            "gbt-h",
            "gbt-v",
            "gbtpc-euc-h",
            "gbtpc-euc-v",
            "h",
            "hankaku",
            "hiragana",
            "hkdla-b5-h",
            "hkdla-b5-v",
            "hkdlb-b5-h",
            "hkdlb-b5-v",
            "hkgccs-b5-h",
            "hkgccs-b5-v",
            "hkm314-b5-h",
            "hkm314-b5-v",
            "hkm471-b5-h",
            "hkm471-b5-v",
            "hkscs-b5-h",
            "hkscs-b5-v",
            "katakana",
            "ksc-euc-h",
            "ksc-euc-v",
            "ksc-h",
            "ksc-johab-h",
            "ksc-johab-v",
            "ksc-v",
            "kscms-uhc-h",
            "kscms-uhc-hw-h",
            "kscms-uhc-hw-v",
            "kscms-uhc-v",
            "kscpc-euc-h",
            "kscpc-euc-v",
            "nwp-h",
            "nwp-v",
            "rksj-h",
            "rksj-v",
            "roman",
            "unicns-ucs2-h",
            "unicns-ucs2-v",
            "unicns-utf16-h",
            "unicns-utf16-v",
            "unicns-utf32-h",
            "unicns-utf32-v",
            "unicns-utf8-h",
            "unicns-utf8-v",
            "unigb-ucs2-h",
            "unigb-ucs2-v",
            "unigb-utf16-h",
            "unigb-utf16-v",
            "unigb-utf32-h",
            "unigb-utf32-v",
            "unigb-utf8-h",
            "unigb-utf8-v",
            "unijis-ucs2-h",
            "unijis-ucs2-hw-h",
            "unijis-ucs2-hw-v",
            "unijis-ucs2-v",
            "unijis-utf16-h",
            "unijis-utf16-v",
            "unijis-utf32-h",
            "unijis-utf32-v",
            "unijis-utf8-h",
            "unijis-utf8-v",
            "unijis2004-utf16-h",
            "unijis2004-utf16-v",
            "unijis2004-utf32-h",
            "unijis2004-utf32-v",
            "unijis2004-utf8-h",
            "unijis2004-utf8-v",
            "unijispro-ucs2-hw-v",
            "unijispro-ucs2-v",
            "unijispro-utf8-v",
            "unijisx0213-utf32-h",
            "unijisx0213-utf32-v",
            "unijisx02132004-utf32-h",
            "unijisx02132004-utf32-v",
            "uniks-ucs2-h",
            "uniks-ucs2-v",
            "uniks-utf16-h",
            "uniks-utf16-v",
            "uniks-utf32-h",
            "uniks-utf32-v",
            "uniks-utf8-h",
            "uniks-utf8-v",
            "v",
            "wp-symbol",
        };
        public static (List<CRange> Ranges, Dictionary<uint, CResult> Mapping) GetCMap(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();

            var resource = name.ToLower() switch
            {
                "78-euc-h" => "PdfLexer.CMaps.Resources.78-EUC-H.bcmap",
                "78-euc-v" => "PdfLexer.CMaps.Resources.78-EUC-V.bcmap",
                "78-h" => "PdfLexer.CMaps.Resources.78-H.bcmap",
                "78-rksj-h" => "PdfLexer.CMaps.Resources.78-RKSJ-H.bcmap",
                "78-rksj-v" => "PdfLexer.CMaps.Resources.78-RKSJ-V.bcmap",
                "78-v" => "PdfLexer.CMaps.Resources.78-V.bcmap",
                "78ms-rksj-h" => "PdfLexer.CMaps.Resources.78ms-RKSJ-H.bcmap",
                "78ms-rksj-v" => "PdfLexer.CMaps.Resources.78ms-RKSJ-V.bcmap",
                "83pv-rksj-h" => "PdfLexer.CMaps.Resources.83pv-RKSJ-H.bcmap",
                "90ms-rksj-h" => "PdfLexer.CMaps.Resources.90ms-RKSJ-H.bcmap",
                "90ms-rksj-v" => "PdfLexer.CMaps.Resources.90ms-RKSJ-V.bcmap",
                "90msp-rksj-h" => "PdfLexer.CMaps.Resources.90msp-RKSJ-H.bcmap",
                "90msp-rksj-v" => "PdfLexer.CMaps.Resources.90msp-RKSJ-V.bcmap",
                "90pv-rksj-h" => "PdfLexer.CMaps.Resources.90pv-RKSJ-H.bcmap",
                "90pv-rksj-v" => "PdfLexer.CMaps.Resources.90pv-RKSJ-V.bcmap",
                "add-h" => "PdfLexer.CMaps.Resources.Add-H.bcmap",
                "add-rksj-h" => "PdfLexer.CMaps.Resources.Add-RKSJ-H.bcmap",
                "add-rksj-v" => "PdfLexer.CMaps.Resources.Add-RKSJ-V.bcmap",
                "add-v" => "PdfLexer.CMaps.Resources.Add-V.bcmap",
                "adobe-cns1-0" => "PdfLexer.CMaps.Resources.Adobe-CNS1-0.bcmap",
                "adobe-cns1-1" => "PdfLexer.CMaps.Resources.Adobe-CNS1-1.bcmap",
                "adobe-cns1-2" => "PdfLexer.CMaps.Resources.Adobe-CNS1-2.bcmap",
                "adobe-cns1-3" => "PdfLexer.CMaps.Resources.Adobe-CNS1-3.bcmap",
                "adobe-cns1-4" => "PdfLexer.CMaps.Resources.Adobe-CNS1-4.bcmap",
                "adobe-cns1-5" => "PdfLexer.CMaps.Resources.Adobe-CNS1-5.bcmap",
                "adobe-cns1-6" => "PdfLexer.CMaps.Resources.Adobe-CNS1-6.bcmap",
                "adobe-cns1-ucs2" => "PdfLexer.CMaps.Resources.Adobe-CNS1-UCS2.bcmap",
                "adobe-gb1-0" => "PdfLexer.CMaps.Resources.Adobe-GB1-0.bcmap",
                "adobe-gb1-1" => "PdfLexer.CMaps.Resources.Adobe-GB1-1.bcmap",
                "adobe-gb1-2" => "PdfLexer.CMaps.Resources.Adobe-GB1-2.bcmap",
                "adobe-gb1-3" => "PdfLexer.CMaps.Resources.Adobe-GB1-3.bcmap",
                "adobe-gb1-4" => "PdfLexer.CMaps.Resources.Adobe-GB1-4.bcmap",
                "adobe-gb1-5" => "PdfLexer.CMaps.Resources.Adobe-GB1-5.bcmap",
                "adobe-gb1-ucs2" => "PdfLexer.CMaps.Resources.Adobe-GB1-UCS2.bcmap",
                "adobe-japan1-0" => "PdfLexer.CMaps.Resources.Adobe-Japan1-0.bcmap",
                "adobe-japan1-1" => "PdfLexer.CMaps.Resources.Adobe-Japan1-1.bcmap",
                "adobe-japan1-2" => "PdfLexer.CMaps.Resources.Adobe-Japan1-2.bcmap",
                "adobe-japan1-3" => "PdfLexer.CMaps.Resources.Adobe-Japan1-3.bcmap",
                "adobe-japan1-4" => "PdfLexer.CMaps.Resources.Adobe-Japan1-4.bcmap",
                "adobe-japan1-5" => "PdfLexer.CMaps.Resources.Adobe-Japan1-5.bcmap",
                "adobe-japan1-6" => "PdfLexer.CMaps.Resources.Adobe-Japan1-6.bcmap",
                "adobe-japan1-ucs2" => "PdfLexer.CMaps.Resources.Adobe-Japan1-UCS2.bcmap",
                "adobe-korea1-0" => "PdfLexer.CMaps.Resources.Adobe-Korea1-0.bcmap",
                "adobe-korea1-1" => "PdfLexer.CMaps.Resources.Adobe-Korea1-1.bcmap",
                "adobe-korea1-2" => "PdfLexer.CMaps.Resources.Adobe-Korea1-2.bcmap",
                "adobe-korea1-ucs2" => "PdfLexer.CMaps.Resources.Adobe-Korea1-UCS2.bcmap",
                "b5-h" => "PdfLexer.CMaps.Resources.B5-H.bcmap",
                "b5-v" => "PdfLexer.CMaps.Resources.B5-V.bcmap",
                "b5pc-h" => "PdfLexer.CMaps.Resources.B5pc-H.bcmap",
                "b5pc-v" => "PdfLexer.CMaps.Resources.B5pc-V.bcmap",
                "cns-euc-h" => "PdfLexer.CMaps.Resources.CNS-EUC-H.bcmap",
                "cns-euc-v" => "PdfLexer.CMaps.Resources.CNS-EUC-V.bcmap",
                "cns1-h" => "PdfLexer.CMaps.Resources.CNS1-H.bcmap",
                "cns1-v" => "PdfLexer.CMaps.Resources.CNS1-V.bcmap",
                "cns2-h" => "PdfLexer.CMaps.Resources.CNS2-H.bcmap",
                "cns2-v" => "PdfLexer.CMaps.Resources.CNS2-V.bcmap",
                "eten-b5-h" => "PdfLexer.CMaps.Resources.ETen-B5-H.bcmap",
                "eten-b5-v" => "PdfLexer.CMaps.Resources.ETen-B5-V.bcmap",
                "etenms-b5-h" => "PdfLexer.CMaps.Resources.ETenms-B5-H.bcmap",
                "etenms-b5-v" => "PdfLexer.CMaps.Resources.ETenms-B5-V.bcmap",
                "ethk-b5-h" => "PdfLexer.CMaps.Resources.ETHK-B5-H.bcmap",
                "ethk-b5-v" => "PdfLexer.CMaps.Resources.ETHK-B5-V.bcmap",
                "euc-h" => "PdfLexer.CMaps.Resources.EUC-H.bcmap",
                "euc-v" => "PdfLexer.CMaps.Resources.EUC-V.bcmap",
                "ext-h" => "PdfLexer.CMaps.Resources.Ext-H.bcmap",
                "ext-rksj-h" => "PdfLexer.CMaps.Resources.Ext-RKSJ-H.bcmap",
                "ext-rksj-v" => "PdfLexer.CMaps.Resources.Ext-RKSJ-V.bcmap",
                "ext-v" => "PdfLexer.CMaps.Resources.Ext-V.bcmap",
                "gb-euc-h" => "PdfLexer.CMaps.Resources.GB-EUC-H.bcmap",
                "gb-euc-v" => "PdfLexer.CMaps.Resources.GB-EUC-V.bcmap",
                "gb-h" => "PdfLexer.CMaps.Resources.GB-H.bcmap",
                "gb-v" => "PdfLexer.CMaps.Resources.GB-V.bcmap",
                "gbk-euc-h" => "PdfLexer.CMaps.Resources.GBK-EUC-H.bcmap",
                "gbk-euc-v" => "PdfLexer.CMaps.Resources.GBK-EUC-V.bcmap",
                "gbk2k-h" => "PdfLexer.CMaps.Resources.GBK2K-H.bcmap",
                "gbk2k-v" => "PdfLexer.CMaps.Resources.GBK2K-V.bcmap",
                "gbkp-euc-h" => "PdfLexer.CMaps.Resources.GBKp-EUC-H.bcmap",
                "gbkp-euc-v" => "PdfLexer.CMaps.Resources.GBKp-EUC-V.bcmap",
                "gbpc-euc-h" => "PdfLexer.CMaps.Resources.GBpc-EUC-H.bcmap",
                "gbpc-euc-v" => "PdfLexer.CMaps.Resources.GBpc-EUC-V.bcmap",
                "gbt-euc-h" => "PdfLexer.CMaps.Resources.GBT-EUC-H.bcmap",
                "gbt-euc-v" => "PdfLexer.CMaps.Resources.GBT-EUC-V.bcmap",
                "gbt-h" => "PdfLexer.CMaps.Resources.GBT-H.bcmap",
                "gbt-v" => "PdfLexer.CMaps.Resources.GBT-V.bcmap",
                "gbtpc-euc-h" => "PdfLexer.CMaps.Resources.GBTpc-EUC-H.bcmap",
                "gbtpc-euc-v" => "PdfLexer.CMaps.Resources.GBTpc-EUC-V.bcmap",
                "h" => "PdfLexer.CMaps.Resources.H.bcmap",
                "identity-h" => "PdfLexer.CMaps.Resources.H.bcmap",
                "hankaku" => "PdfLexer.CMaps.Resources.Hankaku.bcmap",
                "hiragana" => "PdfLexer.CMaps.Resources.Hiragana.bcmap",
                "hkdla-b5-h" => "PdfLexer.CMaps.Resources.HKdla-B5-H.bcmap",
                "hkdla-b5-v" => "PdfLexer.CMaps.Resources.HKdla-B5-V.bcmap",
                "hkdlb-b5-h" => "PdfLexer.CMaps.Resources.HKdlb-B5-H.bcmap",
                "hkdlb-b5-v" => "PdfLexer.CMaps.Resources.HKdlb-B5-V.bcmap",
                "hkgccs-b5-h" => "PdfLexer.CMaps.Resources.HKgccs-B5-H.bcmap",
                "hkgccs-b5-v" => "PdfLexer.CMaps.Resources.HKgccs-B5-V.bcmap",
                "hkm314-b5-h" => "PdfLexer.CMaps.Resources.HKm314-B5-H.bcmap",
                "hkm314-b5-v" => "PdfLexer.CMaps.Resources.HKm314-B5-V.bcmap",
                "hkm471-b5-h" => "PdfLexer.CMaps.Resources.HKm471-B5-H.bcmap",
                "hkm471-b5-v" => "PdfLexer.CMaps.Resources.HKm471-B5-V.bcmap",
                "hkscs-b5-h" => "PdfLexer.CMaps.Resources.HKscs-B5-H.bcmap",
                "hkscs-b5-v" => "PdfLexer.CMaps.Resources.HKscs-B5-V.bcmap",
                "katakana" => "PdfLexer.CMaps.Resources.Katakana.bcmap",
                "ksc-euc-h" => "PdfLexer.CMaps.Resources.KSC-EUC-H.bcmap",
                "ksc-euc-v" => "PdfLexer.CMaps.Resources.KSC-EUC-V.bcmap",
                "ksc-h" => "PdfLexer.CMaps.Resources.KSC-H.bcmap",
                "ksc-johab-h" => "PdfLexer.CMaps.Resources.KSC-Johab-H.bcmap",
                "ksc-johab-v" => "PdfLexer.CMaps.Resources.KSC-Johab-V.bcmap",
                "ksc-v" => "PdfLexer.CMaps.Resources.KSC-V.bcmap",
                "kscms-uhc-h" => "PdfLexer.CMaps.Resources.KSCms-UHC-H.bcmap",
                "kscms-uhc-hw-h" => "PdfLexer.CMaps.Resources.KSCms-UHC-HW-H.bcmap",
                "kscms-uhc-hw-v" => "PdfLexer.CMaps.Resources.KSCms-UHC-HW-V.bcmap",
                "kscms-uhc-v" => "PdfLexer.CMaps.Resources.KSCms-UHC-V.bcmap",
                "kscpc-euc-h" => "PdfLexer.CMaps.Resources.KSCpc-EUC-H.bcmap",
                "kscpc-euc-v" => "PdfLexer.CMaps.Resources.KSCpc-EUC-V.bcmap",
                "nwp-h" => "PdfLexer.CMaps.Resources.NWP-H.bcmap",
                "nwp-v" => "PdfLexer.CMaps.Resources.NWP-V.bcmap",
                "rksj-h" => "PdfLexer.CMaps.Resources.RKSJ-H.bcmap",
                "rksj-v" => "PdfLexer.CMaps.Resources.RKSJ-V.bcmap",
                "roman" => "PdfLexer.CMaps.Resources.Roman.bcmap",
                "unicns-ucs2-h" => "PdfLexer.CMaps.Resources.UniCNS-UCS2-H.bcmap",
                "unicns-ucs2-v" => "PdfLexer.CMaps.Resources.UniCNS-UCS2-V.bcmap",
                "unicns-utf16-h" => "PdfLexer.CMaps.Resources.UniCNS-UTF16-H.bcmap",
                "unicns-utf16-v" => "PdfLexer.CMaps.Resources.UniCNS-UTF16-V.bcmap",
                "unicns-utf32-h" => "PdfLexer.CMaps.Resources.UniCNS-UTF32-H.bcmap",
                "unicns-utf32-v" => "PdfLexer.CMaps.Resources.UniCNS-UTF32-V.bcmap",
                "unicns-utf8-h" => "PdfLexer.CMaps.Resources.UniCNS-UTF8-H.bcmap",
                "unicns-utf8-v" => "PdfLexer.CMaps.Resources.UniCNS-UTF8-V.bcmap",
                "unigb-ucs2-h" => "PdfLexer.CMaps.Resources.UniGB-UCS2-H.bcmap",
                "unigb-ucs2-v" => "PdfLexer.CMaps.Resources.UniGB-UCS2-V.bcmap",
                "unigb-utf16-h" => "PdfLexer.CMaps.Resources.UniGB-UTF16-H.bcmap",
                "unigb-utf16-v" => "PdfLexer.CMaps.Resources.UniGB-UTF16-V.bcmap",
                "unigb-utf32-h" => "PdfLexer.CMaps.Resources.UniGB-UTF32-H.bcmap",
                "unigb-utf32-v" => "PdfLexer.CMaps.Resources.UniGB-UTF32-V.bcmap",
                "unigb-utf8-h" => "PdfLexer.CMaps.Resources.UniGB-UTF8-H.bcmap",
                "unigb-utf8-v" => "PdfLexer.CMaps.Resources.UniGB-UTF8-V.bcmap",
                "unijis-ucs2-h" => "PdfLexer.CMaps.Resources.UniJIS-UCS2-H.bcmap",
                "unijis-ucs2-hw-h" => "PdfLexer.CMaps.Resources.UniJIS-UCS2-HW-H.bcmap",
                "unijis-ucs2-hw-v" => "PdfLexer.CMaps.Resources.UniJIS-UCS2-HW-V.bcmap",
                "unijis-ucs2-v" => "PdfLexer.CMaps.Resources.UniJIS-UCS2-V.bcmap",
                "unijis-utf16-h" => "PdfLexer.CMaps.Resources.UniJIS-UTF16-H.bcmap",
                "unijis-utf16-v" => "PdfLexer.CMaps.Resources.UniJIS-UTF16-V.bcmap",
                "unijis-utf32-h" => "PdfLexer.CMaps.Resources.UniJIS-UTF32-H.bcmap",
                "unijis-utf32-v" => "PdfLexer.CMaps.Resources.UniJIS-UTF32-V.bcmap",
                "unijis-utf8-h" => "PdfLexer.CMaps.Resources.UniJIS-UTF8-H.bcmap",
                "unijis-utf8-v" => "PdfLexer.CMaps.Resources.UniJIS-UTF8-V.bcmap",
                "unijis2004-utf16-h" => "PdfLexer.CMaps.Resources.UniJIS2004-UTF16-H.bcmap",
                "unijis2004-utf16-v" => "PdfLexer.CMaps.Resources.UniJIS2004-UTF16-V.bcmap",
                "unijis2004-utf32-h" => "PdfLexer.CMaps.Resources.UniJIS2004-UTF32-H.bcmap",
                "unijis2004-utf32-v" => "PdfLexer.CMaps.Resources.UniJIS2004-UTF32-V.bcmap",
                "unijis2004-utf8-h" => "PdfLexer.CMaps.Resources.UniJIS2004-UTF8-H.bcmap",
                "unijis2004-utf8-v" => "PdfLexer.CMaps.Resources.UniJIS2004-UTF8-V.bcmap",
                "unijispro-ucs2-hw-v" => "PdfLexer.CMaps.Resources.UniJISPro-UCS2-HW-V.bcmap",
                "unijispro-ucs2-v" => "PdfLexer.CMaps.Resources.UniJISPro-UCS2-V.bcmap",
                "unijispro-utf8-v" => "PdfLexer.CMaps.Resources.UniJISPro-UTF8-V.bcmap",
                "unijisx0213-utf32-h" => "PdfLexer.CMaps.Resources.UniJISX0213-UTF32-H.bcmap",
                "unijisx0213-utf32-v" => "PdfLexer.CMaps.Resources.UniJISX0213-UTF32-V.bcmap",
                "unijisx02132004-utf32-h" => "PdfLexer.CMaps.Resources.UniJISX02132004-UTF32-H.bcmap",
                "unijisx02132004-utf32-v" => "PdfLexer.CMaps.Resources.UniJISX02132004-UTF32-V.bcmap",
                "uniks-ucs2-h" => "PdfLexer.CMaps.Resources.UniKS-UCS2-H.bcmap",
                "uniks-ucs2-v" => "PdfLexer.CMaps.Resources.UniKS-UCS2-V.bcmap",
                "uniks-utf16-h" => "PdfLexer.CMaps.Resources.UniKS-UTF16-H.bcmap",
                "uniks-utf16-v" => "PdfLexer.CMaps.Resources.UniKS-UTF16-V.bcmap",
                "uniks-utf32-h" => "PdfLexer.CMaps.Resources.UniKS-UTF32-H.bcmap",
                "uniks-utf32-v" => "PdfLexer.CMaps.Resources.UniKS-UTF32-V.bcmap",
                "uniks-utf8-h" => "PdfLexer.CMaps.Resources.UniKS-UTF8-H.bcmap",
                "uniks-utf8-v" => "PdfLexer.CMaps.Resources.UniKS-UTF8-V.bcmap",
                "v" => "PdfLexer.CMaps.Resources.V.bcmap",
                "identity-v" => "PdfLexer.CMaps.Resources.V.bcmap",
                "wp-symbol" => "PdfLexer.CMaps.Resources.WP-Symbol.bcmap",
                _ => null
            };
            if (resource == null) { return (null, null); }

            lock (globalCache)
            {
                if (globalCache.TryGetValue(resource, out var cached))
                {
                    return (cached.Ranges, cached.Mapping);
                }
            }

            using (Stream stream = assembly.GetManifestResourceStream(resource))
            {
                int total = (int)stream.Length;
                var rented = ArrayPool<byte>.Shared.Rent((int)total);
                int totalRead = 0;
                int read;
                while ((read = stream.Read(rented, totalRead, total - totalRead)) > 0)
                {
                    totalRead += read;
                }

                ReadOnlySpan<byte> data = rented;

                var result = BCMapReader.GetGlyphsFromToUnicode(data.Slice(0, total));
                ArrayPool<byte>.Shared.Return(rented);

                lock (globalCache)
                {
                    globalCache.AddOrUpdate(resource, new CMapData
                    {
                        Ranges = result.Ranges,
                        Mapping = result.Mapping
                    });
                }
                return result;
            }
        }

        private static ConditionalWeakTable<string, CMapData>
            globalCache = new ConditionalWeakTable<string, CMapData>();
    }

    // not referenced anywhere so will get cleared quickly, look into how to optimize caching
    public class CMapData
    {
        public List<CRange> Ranges { get; set; }
        public Dictionary<uint, CResult> Mapping { get; set; }
    }

    public struct CResult
    {
        public uint Code;
        public string MultiChar;
    }

    public struct CRange
    {
        public uint Start;
        public uint End;
        public int Bytes;

        public static int EstimateByteSize(List<CRange> ranges, uint cid)
        {
            var bytes = ranges.Select(x => x.Bytes).ToHashSet();
            foreach (var rng in ranges)
            {
                if (cid >= rng.Start && cid <= rng.End)
                {
                    return rng.Bytes;
                }
            }

            if (bytes.Contains(1) && cid < 256)
            {
                return 1;
            }
            else if (bytes.Contains(2) && cid <= 0xFFFF)
            {
                return 2;
            }
            else if (bytes.Contains(3) && cid <= 0xFFFFFF)
            {
                return 3;
            }
            else if (bytes.Contains(4))
            {
                return 4;
            }
            else
            {
                return cid < 256 ? 1 : 2; // fallback
            }
        }
    }
}