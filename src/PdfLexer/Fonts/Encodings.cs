﻿using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.Fonts
{
    // LZWStream decode stream PORTED FROM PDF.JS, PDF.JS is licensed as follows:
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
    internal class Encodings
    {
        public static string[] MacStandardGlyphOrdering = new string[] {
          ".notdef", ".null", "nonmarkingreturn", "space", "exclam", "quotedbl",
          "numbersign", "dollar", "percent", "ampersand", "quotesingle", "parenleft",
          "parenright", "asterisk", "plus", "comma", "hyphen", "period", "slash",
          "zero", "one", "two", "three", "four", "five", "six", "seven", "eight",
          "nine", "colon", "semicolon", "less", "equal", "greater", "question", "at",
          "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O",
          "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "bracketleft",
          "backslash", "bracketright", "asciicircum", "underscore", "grave", "a", "b",
          "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q",
          "r", "s", "t", "u", "v", "w", "x", "y", "z", "braceleft", "bar", "braceright",
          "asciitilde", "Adieresis", "Aring", "Ccedilla", "Eacute", "Ntilde",
          "Odieresis", "Udieresis", "aacute", "agrave", "acircumflex", "adieresis",
          "atilde", "aring", "ccedilla", "eacute", "egrave", "ecircumflex", "edieresis",
          "iacute", "igrave", "icircumflex", "idieresis", "ntilde", "oacute", "ograve",
          "ocircumflex", "odieresis", "otilde", "uacute", "ugrave", "ucircumflex",
          "udieresis", "dagger", "degree", "cent", "sterling", "section", "bullet",
          "paragraph", "germandbls", "registered", "copyright", "trademark", "acute",
          "dieresis", "notequal", "AE", "Oslash", "infinity", "plusminus", "lessequal",
          "greaterequal", "yen", "mu", "partialdiff", "summation", "product", "pi",
          "integral", "ordfeminine", "ordmasculine", "Omega", "ae", "oslash",
          "questiondown", "exclamdown", "logicalnot", "radical", "florin",
          "approxequal", "Delta", "guillemotleft", "guillemotright", "ellipsis",
          "nonbreakingspace", "Agrave", "Atilde", "Otilde", "OE", "oe", "endash",
          "emdash", "quotedblleft", "quotedblright", "quoteleft", "quoteright",
          "divide", "lozenge", "ydieresis", "Ydieresis", "fraction", "currency",
          "guilsinglleft", "guilsinglright", "fi", "fl", "daggerdbl", "periodcentered",
          "quotesinglbase", "quotedblbase", "perthousand", "Acircumflex",
          "Ecircumflex", "Aacute", "Edieresis", "Egrave", "Iacute", "Icircumflex",
          "Idieresis", "Igrave", "Oacute", "Ocircumflex", "apple", "Ograve", "Uacute",
          "Ucircumflex", "Ugrave", "dotlessi", "circumflex", "tilde", "macron",
          "breve", "dotaccent", "ring", "cedilla", "hungarumlaut", "ogonek", "caron",
          "Lslash", "lslash", "Scaron", "scaron", "Zcaron", "zcaron", "brokenbar",
          "Eth", "eth", "Yacute", "yacute", "Thorn", "thorn", "minus", "multiply",
          "onesuperior", "twosuperior", "threesuperior", "onehalf", "onequarter",
          "threequarters", "franc", "Gbreve", "gbreve", "Idotaccent", "Scedilla",
          "scedilla", "Cacute", "cacute", "Ccaron", "ccaron", "dcroat" 
        };

        public static string[] ExpertEncoding = new string[]
        {
            null, null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null, null,
            "space", "exclamsmall", "Hungarumlautsmall", null, "dollaroldstyle",
            "dollarsuperior", "ampersandsmall", "Acutesmall", "parenleftsuperior",
            "parenrightsuperior", "twodotenleader", "onedotenleader", "comma",
            "hyphen", "period", "fraction", "zerooldstyle", "oneoldstyle",
            "twooldstyle", "threeoldstyle", "fouroldstyle", "fiveoldstyle",
            "sixoldstyle", "sevenoldstyle", "eightoldstyle", "nineoldstyle", "colon",
            "semicolon", "commasuperior", "threequartersemdash", "periodsuperior",
            "questionsmall", null, "asuperior", "bsuperior", "centsuperior", "dsuperior",
            "esuperior", null, null, null, "isuperior", null, null, "lsuperior", "msuperior",
            "nsuperior", "osuperior", null, null, "rsuperior", "ssuperior", "tsuperior",
            null, "ff", "fi", "fl", "ffi", "ffl", "parenleftinferior", null,
            "parenrightinferior", "Circumflexsmall", "hyphensuperior", "Gravesmall",
            "Asmall", "Bsmall", "Csmall", "Dsmall", "Esmall", "Fsmall", "Gsmall",
            "Hsmall", "Ismall", "Jsmall", "Ksmall", "Lsmall", "Msmall", "Nsmall",
            "Osmall", "Psmall", "Qsmall", "Rsmall", "Ssmall", "Tsmall", "Usmall",
            "Vsmall", "Wsmall", "Xsmall", "Ysmall", "Zsmall", "colonmonetary",
            "onefitted", "rupiah", "Tildesmall", null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, "exclamdownsmall", "centoldstyle", "Lslashsmall",
            null, null, "Scaronsmall", "Zcaronsmall", "Dieresissmall", "Brevesmall",
            "Caronsmall", null, "Dotaccentsmall", null, null, "Macronsmall", null, null,
            "figuredash", "hypheninferior", null, null, "Ogoneksmall", "Ringsmall",
            "Cedillasmall", null, null, null, "onequarter", "onehalf", "threequarters",
            "questiondownsmall", "oneeighth", "threeeighths", "fiveeighths",
            "seveneighths", "onethird", "twothirds", null, null, "zerosuperior",
            "onesuperior", "twosuperior", "threesuperior", "foursuperior",
            "fivesuperior", "sixsuperior", "sevensuperior", "eightsuperior",
            "ninesuperior", "zeroinferior", "oneinferior", "twoinferior",
            "threeinferior", "fourinferior", "fiveinferior", "sixinferior",
            "seveninferior", "eightinferior", "nineinferior", "centinferior",
            "dollarinferior", "periodinferior", "commainferior", "Agravesmall",
            "Aacutesmall", "Acircumflexsmall", "Atildesmall", "Adieresissmall",
            "Aringsmall", "AEsmall", "Ccedillasmall", "Egravesmall", "Eacutesmall",
            "Ecircumflexsmall", "Edieresissmall", "Igravesmall", "Iacutesmall",
            "Icircumflexsmall", "Idieresissmall", "Ethsmall", "Ntildesmall",
            "Ogravesmall", "Oacutesmall", "Ocircumflexsmall", "Otildesmall",
            "Odieresissmall", "OEsmall", "Oslashsmall", "Ugravesmall", "Uacutesmall",
            "Ucircumflexsmall", "Udieresissmall", "Yacutesmall", "Thornsmall",
            "Ydieresissmall"
        };

        public static string[] MacExpertEncoding = new string[]
        {
            null, null, null, null, null, null, null, null, null, null, null, null, null, null,
  null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
  "space", "exclamsmall", "Hungarumlautsmall", "centoldstyle",
  "dollaroldstyle", "dollarsuperior", "ampersandsmall", "Acutesmall",
  "parenleftsuperior", "parenrightsuperior", "twodotenleader",
  "onedotenleader", "comma", "hyphen", "period", "fraction", "zerooldstyle",
  "oneoldstyle", "twooldstyle", "threeoldstyle", "fouroldstyle",
  "fiveoldstyle", "sixoldstyle", "sevenoldstyle", "eightoldstyle",
  "nineoldstyle", "colon", "semicolon", null, "threequartersemdash", null,
  "questionsmall", null, null, null, null, "Ethsmall", null, null, "onequarter",
  "onehalf", "threequarters", "oneeighth", "threeeighths", "fiveeighths",
  "seveneighths", "onethird", "twothirds", null, null, null, null, null, null, "ff",
  "fi", "fl", "ffi", "ffl", "parenleftinferior", null, "parenrightinferior",
  "Circumflexsmall", "hypheninferior", "Gravesmall", "Asmall", "Bsmall",
  "Csmall", "Dsmall", "Esmall", "Fsmall", "Gsmall", "Hsmall", "Ismall",
  "Jsmall", "Ksmall", "Lsmall", "Msmall", "Nsmall", "Osmall", "Psmall",
  "Qsmall", "Rsmall", "Ssmall", "Tsmall", "Usmall", "Vsmall", "Wsmall",
  "Xsmall", "Ysmall", "Zsmall", "colonmonetary", "onefitted", "rupiah",
  "Tildesmall", null, null, "asuperior", "centsuperior", null, null, null, null,
  "Aacutesmall", "Agravesmall", "Acircumflexsmall", "Adieresissmall",
  "Atildesmall", "Aringsmall", "Ccedillasmall", "Eacutesmall", "Egravesmall",
  "Ecircumflexsmall", "Edieresissmall", "Iacutesmall", "Igravesmall",
  "Icircumflexsmall", "Idieresissmall", "Ntildesmall", "Oacutesmall",
  "Ogravesmall", "Ocircumflexsmall", "Odieresissmall", "Otildesmall",
  "Uacutesmall", "Ugravesmall", "Ucircumflexsmall", "Udieresissmall", null,
  "eightsuperior", "fourinferior", "threeinferior", "sixinferior",
  "eightinferior", "seveninferior", "Scaronsmall", null, "centinferior",
  "twoinferior", null, "Dieresissmall", null, "Caronsmall", "osuperior",
  "fiveinferior", null, "commainferior", "periodinferior", "Yacutesmall", null,
  "dollarinferior", null, null, "Thornsmall", null, "nineinferior", "zeroinferior",
  "Zcaronsmall", "AEsmall", "Oslashsmall", "questiondownsmall",
  "oneinferior", "Lslashsmall", null, null, null, null, null, null, "Cedillasmall", null,
  null, null, null, null, "OEsmall", "figuredash", "hyphensuperior", null, null, null, null,
  "exclamdownsmall", null, "Ydieresissmall", null, "onesuperior", "twosuperior",
  "threesuperior", "foursuperior", "fivesuperior", "sixsuperior",
  "sevensuperior", "ninesuperior", "zerosuperior", null, "esuperior",
  "rsuperior", "tsuperior", null, null, "isuperior", "ssuperior", "dsuperior",
  null, null, null, null, null, "lsuperior", "Ogoneksmall", "Brevesmall",
  "Macronsmall", "bsuperior", "nsuperior", "msuperior", "commasuperior",
  "periodsuperior", "Dotaccentsmall", "Ringsmall", null, null, null, null
        };

        public static string[] MacRomanEncoding = new string[]
{
    null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
  null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
  "space", "exclam", "quotedbl", "numbersign", "dollar", "percent",
  "ampersand", "quotesingle", "parenleft", "parenright", "asterisk", "plus",
  "comma", "hyphen", "period", "slash", "zero", "one", "two", "three",
  "four", "five", "six", "seven", "eight", "nine", "colon", "semicolon",
  "less", "equal", "greater", "question", "at", "A", "B", "C", "D", "E", "F",
  "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U",
  "V", "W", "X", "Y", "Z", "bracketleft", "backslash", "bracketright",
  "asciicircum", "underscore", "grave", "a", "b", "c", "d", "e", "f", "g",
  "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v",
  "w", "x", "y", "z", "braceleft", "bar", "braceright", "asciitilde", null,
  "Adieresis", "Aring", "Ccedilla", "Eacute", "Ntilde", "Odieresis",
  "Udieresis", "aacute", "agrave", "acircumflex", "adieresis", "atilde",
  "aring", "ccedilla", "eacute", "egrave", "ecircumflex", "edieresis",
  "iacute", "igrave", "icircumflex", "idieresis", "ntilde", "oacute",
  "ograve", "ocircumflex", "odieresis", "otilde", "uacute", "ugrave",
  "ucircumflex", "udieresis", "dagger", "degree", "cent", "sterling",
  "section", "bullet", "paragraph", "germandbls", "registered", "copyright",
  "trademark", "acute", "dieresis", "notequal", "AE", "Oslash", "infinity",
  "plusminus", "lessequal", "greaterequal", "yen", "mu", "partialdiff",
  "summation", "product", "pi", "integral", "ordfeminine", "ordmasculine",
  "Omega", "ae", "oslash", "questiondown", "exclamdown", "logicalnot",
  "radical", "florin", "approxequal", "Delta", "guillemotleft",
  "guillemotright", "ellipsis", "space", "Agrave", "Atilde", "Otilde", "OE",
  "oe", "endash", "emdash", "quotedblleft", "quotedblright", "quoteleft",
  "quoteright", "divide", "lozenge", "ydieresis", "Ydieresis", "fraction",
  "currency", "guilsinglleft", "guilsinglright", "fi", "fl", "daggerdbl",
  "periodcentered", "quotesinglbase", "quotedblbase", "perthousand",
  "Acircumflex", "Ecircumflex", "Aacute", "Edieresis", "Egrave", "Iacute",
  "Icircumflex", "Idieresis", "Igrave", "Oacute", "Ocircumflex", "apple",
  "Ograve", "Uacute", "Ucircumflex", "Ugrave", "dotlessi", "circumflex",
  "tilde", "macron", "breve", "dotaccent", "ring", "cedilla", "hungarumlaut",
  "ogonek", "caron"
};
        public static string[] StandardEncoding = new string[]
{
    null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
  null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
  "space", "exclam", "quotedbl", "numbersign", "dollar", "percent",
  "ampersand", "quoteright", "parenleft", "parenright", "asterisk", "plus",
  "comma", "hyphen", "period", "slash", "zero", "one", "two", "three",
  "four", "five", "six", "seven", "eight", "nine", "colon", "semicolon",
  "less", "equal", "greater", "question", "at", "A", "B", "C", "D", "E", "F",
  "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U",
  "V", "W", "X", "Y", "Z", "bracketleft", "backslash", "bracketright",
  "asciicircum", "underscore", "quoteleft", "a", "b", "c", "d", "e", "f",
  "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u",
  "v", "w", "x", "y", "z", "braceleft", "bar", "braceright", "asciitilde",
  null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
  null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "exclamdown",
  "cent", "sterling", "fraction", "yen", "florin", "section", "currency",
  "quotesingle", "quotedblleft", "guillemotleft", "guilsinglleft",
  "guilsinglright", "fi", "fl", null, "endash", "dagger", "daggerdbl",
  "periodcentered", null, "paragraph", "bullet", "quotesinglbase",
  "quotedblbase", "quotedblright", "guillemotright", "ellipsis",
  "perthousand", null, "questiondown", null, "grave", "acute", "circumflex",
  "tilde", "macron", "breve", "dotaccent", "dieresis", null, "ring", "cedilla",
  null, "hungarumlaut", "ogonek", "caron", "emdash", null, null, null, null, null, null,
  null, null, null, null, null, null, null, null, null, null, "AE", null, "ordfeminine", null, null,
  null, null, "Lslash", "Oslash", "OE", "ordmasculine", null, null, null, null, null, "ae",
  null, null, null, "dotlessi", null, null, "lslash", "oslash", "oe", "germandbls", null,
  null, null, null
};
        public static string[] WinAnsiEncoding = new string[]
{
    null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
  null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
  "space", "exclam", "quotedbl", "numbersign", "dollar", "percent",
  "ampersand", "quotesingle", "parenleft", "parenright", "asterisk", "plus",
  "comma", "hyphen", "period", "slash", "zero", "one", "two", "three",
  "four", "five", "six", "seven", "eight", "nine", "colon", "semicolon",
  "less", "equal", "greater", "question", "at", "A", "B", "C", "D", "E", "F",
  "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U",
  "V", "W", "X", "Y", "Z", "bracketleft", "backslash", "bracketright",
  "asciicircum", "underscore", "grave", "a", "b", "c", "d", "e", "f", "g",
  "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v",
  "w", "x", "y", "z", "braceleft", "bar", "braceright", "asciitilde",
  "bullet", "Euro", "bullet", "quotesinglbase", "florin", "quotedblbase",
  "ellipsis", "dagger", "daggerdbl", "circumflex", "perthousand", "Scaron",
  "guilsinglleft", "OE", "bullet", "Zcaron", "bullet", "bullet", "quoteleft",
  "quoteright", "quotedblleft", "quotedblright", "bullet", "endash",
  "emdash", "tilde", "trademark", "scaron", "guilsinglright", "oe", "bullet",
  "zcaron", "Ydieresis", "space", "exclamdown", "cent", "sterling",
  "currency", "yen", "brokenbar", "section", "dieresis", "copyright",
  "ordfeminine", "guillemotleft", "logicalnot", "hyphen", "registered",
  "macron", "degree", "plusminus", "twosuperior", "threesuperior", "acute",
  "mu", "paragraph", "periodcentered", "cedilla", "onesuperior",
  "ordmasculine", "guillemotright", "onequarter", "onehalf", "threequarters",
  "questiondown", "Agrave", "Aacute", "Acircumflex", "Atilde", "Adieresis",
  "Aring", "AE", "Ccedilla", "Egrave", "Eacute", "Ecircumflex", "Edieresis",
  "Igrave", "Iacute", "Icircumflex", "Idieresis", "Eth", "Ntilde", "Ograve",
  "Oacute", "Ocircumflex", "Otilde", "Odieresis", "multiply", "Oslash",
  "Ugrave", "Uacute", "Ucircumflex", "Udieresis", "Yacute", "Thorn",
  "germandbls", "agrave", "aacute", "acircumflex", "atilde", "adieresis",
  "aring", "ae", "ccedilla", "egrave", "eacute", "ecircumflex", "edieresis",
  "igrave", "iacute", "icircumflex", "idieresis", "eth", "ntilde", "ograve",
  "oacute", "ocircumflex", "otilde", "odieresis", "divide", "oslash",
  "ugrave", "uacute", "ucircumflex", "udieresis", "yacute", "thorn",
  "ydieresis"
};
        public static string[] SymbolSetEncoding = new string[]
{
    null, null, null, null, null, null, null, null, null, null, null, null, null, null,
  null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
  "space", "exclam", "universal", "numbersign", "existential", "percent",
  "ampersand", "suchthat", "parenleft", "parenright", "asteriskmath", "plus",
  "comma", "minus", "period", "slash", "zero", "one", "two", "three", "four",
  "five", "six", "seven", "eight", "nine", "colon", "semicolon", "less",
  "equal", "greater", "question", "congruent", "Alpha", "Beta", "Chi",
  "Delta", "Epsilon", "Phi", "Gamma", "Eta", "Iota", "theta1", "Kappa",
  "Lambda", "Mu", "Nu", "Omicron", "Pi", "Theta", "Rho", "Sigma", "Tau",
  "Upsilon", "sigma1", "Omega", "Xi", "Psi", "Zeta", "bracketleft",
  "therefore", "bracketright", "perpendicular", "underscore", "radicalex",
  "alpha", "beta", "chi", "delta", "epsilon", "phi", "gamma", "eta", "iota",
  "phi1", "kappa", "lambda", "mu", "nu", "omicron", "pi", "theta", "rho",
  "sigma", "tau", "upsilon", "omega1", "omega", "xi", "psi", "zeta",
  "braceleft", "bar", "braceright", "similar", null, null, null, null, null, null, null,
  null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
  null, null, null, null, null, null, null, "Euro", "Upsilon1", "minute", "lessequal",
  "fraction", "infinity", "florin", "club", "diamond", "heart", "spade",
  "arrowboth", "arrowleft", "arrowup", "arrowright", "arrowdown", "degree",
  "plusminus", "second", "greaterequal", "multiply", "proportional",
  "partialdiff", "bullet", "divide", "notequal", "equivalence",
  "approxequal", "ellipsis", "arrowvertex", "arrowhorizex", "carriagereturn",
  "aleph", "Ifraktur", "Rfraktur", "weierstrass", "circlemultiply",
  "circleplus", "emptyset", "intersection", "union", "propersuperset",
  "reflexsuperset", "notsubset", "propersubset", "reflexsubset", "element",
  "notelement", "angle", "gradient", "registerserif", "copyrightserif",
  "trademarkserif", "product", "radical", "dotmath", "logicalnot",
  "logicaland", "logicalor", "arrowdblboth", "arrowdblleft", "arrowdblup",
  "arrowdblright", "arrowdbldown", "lozenge", "angleleft", "registersans",
  "copyrightsans", "trademarksans", "summation", "parenlefttp",
  "parenleftex", "parenleftbt", "bracketlefttp", "bracketleftex",
  "bracketleftbt", "bracelefttp", "braceleftmid", "braceleftbt", "braceex",
  null, "angleright", "integral", "integraltp", "integralex", "integralbt",
  "parenrighttp", "parenrightex", "parenrightbt", "bracketrighttp",
  "bracketrightex", "bracketrightbt", "bracerighttp", "bracerightmid",
  "bracerightbt", null
};
        public static string[] ZapfDingbatsEncoding = new string[]
{
    null, null, null, null, null, null, null, null, null, null, null, null, null, null,
  null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
  "space", "a1", "a2", "a202", "a3", "a4", "a5", "a119", "a118", "a117",
  "a11", "a12", "a13", "a14", "a15", "a16", "a105", "a17", "a18", "a19",
  "a20", "a21", "a22", "a23", "a24", "a25", "a26", "a27", "a28", "a6", "a7",
  "a8", "a9", "a10", "a29", "a30", "a31", "a32", "a33", "a34", "a35", "a36",
  "a37", "a38", "a39", "a40", "a41", "a42", "a43", "a44", "a45", "a46",
  "a47", "a48", "a49", "a50", "a51", "a52", "a53", "a54", "a55", "a56",
  "a57", "a58", "a59", "a60", "a61", "a62", "a63", "a64", "a65", "a66",
  "a67", "a68", "a69", "a70", "a71", "a72", "a73", "a74", "a203", "a75",
  "a204", "a76", "a77", "a78", "a79", "a81", "a82", "a83", "a84", "a97",
  "a98", "a99", "a100", null, "a89", "a90", "a93", "a94", "a91", "a92", "a205",
  "a85", "a206", "a86", "a87", "a88", "a95", "a96", null, null, null, null, null, null,
  null, null, null, null, null, null, null, null, null, null, null, null, null, "a101", "a102", "a103",
  "a104", "a106", "a107", "a108", "a112", "a111", "a110", "a109", "a120",
  "a121", "a122", "a123", "a124", "a125", "a126", "a127", "a128", "a129",
  "a130", "a131", "a132", "a133", "a134", "a135", "a136", "a137", "a138",
  "a139", "a140", "a141", "a142", "a143", "a144", "a145", "a146", "a147",
  "a148", "a149", "a150", "a151", "a152", "a153", "a154", "a155", "a156",
  "a157", "a158", "a159", "a160", "a161", "a163", "a164", "a196", "a165",
  "a192", "a166", "a167", "a168", "a169", "a170", "a171", "a172", "a173",
  "a162", "a174", "a175", "a176", "a177", "a178", "a179", "a193", "a180",
  "a199", "a181", "a200", "a182", null, "a201", "a183", "a184", "a197", "a185",
  "a194", "a198", "a186", "a195", "a187", "a188", "a189", "a190", "a191", null
};
        public static string[]? GetEncoding(PdfName encoding)
        {
            switch (encoding.Value)
            {
                case "/WinAnsiEncoding":
                    return WinAnsiEncoding;
                case "/StandardEncoding":
                    return StandardEncoding;
                case "/MacRomanEncoding":
                    return MacRomanEncoding;
                case "/SymbolSetEncoding":
                    return SymbolSetEncoding;
                case "/ZapfDingbatsEncoding":
                    return ZapfDingbatsEncoding;
                case "/ExpertEncoding":
                    return ExpertEncoding;
                case "/MacExpertEncoding":
                    return MacExpertEncoding;
                default:
                    return null;
            }
        }

        public static Glyph[] GetPartialGlyphs(string[] charNames)
        {
            var glyphs = new Glyph[charNames.Length];
            for (var i = 0; i < glyphs.Length; i++)
            {
                var nm = charNames[i];
                if (nm == null) { continue; }
                if (GlyphNames.Lookup.TryGetValue(nm, out var c))
                {
                    glyphs[i] = new Glyph
                    {
                        Name = nm,
                        CodePoint = (uint)i,
                        Char = c,
                        IsWordSpace = i == 32
                    };
                }
            }
            return glyphs;
        }
    }


}
