using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using PdfLexer.Fonts;
using PdfLexer.Parsers;
using PdfLexer.Serializers;
using Xunit;

namespace PdfLexer.Tests
{
    public class StringTests
    {
        [InlineData("(Test)", "Test")]
        [InlineData("(Test\\ Test)", "Test Test")]  // ignore random \
        [InlineData("(Test \\t)", "Test \t")] // tab
        [InlineData("(Test \\n\\r)", "Test \n\r")] // keep new lines by espace
        [InlineData("(Test \\\nNextLine)", "Test NextLine")] // allows split strings by line
        [InlineData("(Test \\\rNextLine)", "Test NextLine")]
        [InlineData("(Test \\\r\nNextLine)", "Test NextLine")]
        [InlineData("(Test Test\rTest )", "Test Test\rTest ")] // keeps new lines
        [InlineData("(Test Test\r\nTest )", "Test Test\r\nTest ")]
        [InlineData("(Test Test\nTest )", "Test Test\nTest ")]
        [InlineData("(Test Test Test )", "Test Test Test ")]
        [InlineData("(Test (Test) Test )", "Test (Test) Test ")]
        [InlineData("(Test \\\\(Test) Test )", "Test \\(Test) Test ")]
        [InlineData("(Test \\) Test )", "Test ) Test ")]
        [InlineData("(Test \\\\\\) Test )", "Test \\) Test ")]
        [InlineData("()", "")]
        [InlineData("(\\171)", "y")] // octal
        [InlineData("( \\1a)", " 1a")]
        [InlineData("( \\171a)", " ya")]
        [InlineData("(partial octal \\53\\171)", "partial octal +y")]
        [Theory]
        public void It_Parses_Literals_Reader(string input, string output)
        {
            var bytes = Encoding.ASCII.GetBytes(input);
            var ms = new MemoryStream(bytes);
            var parser = new StringParser(new ParsingContext());
            var span = new Span<byte>(bytes);
            var result = parser.Parse(span);
            Assert.Equal(output, result.Value);
        }

        [InlineData("(Test)", "Test")]
        [InlineData("(Test \\t)", "Test \t")] // tab
        [InlineData("(Test \\n\\r)", "Test \n\r")] // keep new lines by espace
        [InlineData("(Test Test Test )", "Test Test Test ")]
        [InlineData("(Test \\\\\\(Test Test )", "Test \\(Test Test ")]
        [InlineData("(Test \\) Test )", "Test ) Test ")]
        [InlineData("(Test \\\\\\) Test )", "Test \\) Test ")]
        [InlineData("(Test \\036 Test )", "Test \u001E Test ")]
        [InlineData("()", "")]
        [InlineData("(\\216\\217)", "\u008e\u008f")] // octal unhappy
        [InlineData("(Test \\310Line)", "Test ÈLine")] // "happy" > 128 chars
        [Theory]
        public void It_Parses_And_Serializes_Literals_Reader(string input, string output)
        {
            var bytes = Encoding.ASCII.GetBytes(input);
            var ms = new MemoryStream(bytes);
            var parser = new StringParser(new ParsingContext());
            var serializer = new StringSerializer();

            // scan past
            int s = 0;
            var succeeded = StringParser.AdvancePastStringLiteral(bytes, ref s);
            Assert.True(succeeded);
            Assert.Equal(input.Length, s);

            var span = new Span<byte>(bytes);
            var result = parser.Parse(span);
            Assert.Equal(output, result.Value);

            var rms = new MemoryStream();
            serializer.WriteToStream(result, rms);
            Assert.Equal(input, Encoding.ASCII.GetString(rms.ToArray()));
        }

        [InlineData("<> ", "")]
        [InlineData("<303132> ", "012")]
        [InlineData("<30313> ", "010")]
        [Theory]
        public void It_Gets_Hex(string input, string output)
        {
            var bytes = Encoding.ASCII.GetBytes(input);
            var ms = new MemoryStream(bytes);
            var parser = new StringParser(new ParsingContext());
            var span = new Span<byte>(bytes);
            var result = parser.Parse(span);
            Assert.Equal(output, result.Value);
        }

        [InlineData("(Test) ", 6)]
        [InlineData("(Test Test Test )\r\n", 17)]
        [InlineData("(Test (Test) Test )\n", 19)]
        [InlineData("(Test \\\\(Test) Test )\n", 21)]
        [InlineData("(Test \\) Test )\n", 15)]
        [InlineData("(Test \\\\\\) Test )\n", 17)]
        [InlineData("() ", 2)]
        [InlineData("()", 2)]
        [Theory]
        public void It_Skips_String_Literals(string input, int pos)
        {
            var bytes = Encoding.ASCII.GetBytes(input);
            var seq = new ReadOnlySequence<byte>(bytes);
            var reader = new SequenceReader<byte>(seq);
            var result = StringParser.TryAdvancePastString(ref reader);
            Assert.True(result);
            Assert.Equal(seq.GetPosition(pos), reader.Position);
        }

        [Fact]
        public void It_Parses_And_Serialized_Literal_UTF16BE()
        {
            var root = PathUtil.GetPathFromSegmentOfCurrent("test");
            var data = File.ReadAllBytes(Path.Combine(root, "raw-utf16-string-literal.txt"));
            var parser = new StringParser(new ParsingContext());
            var result = parser.Parse(data);
            Assert.Equal(PdfStringType.Literal, result.StringType);
            Assert.Equal(PdfTextEncodingType.UTF16BE, result.Encoding);
            var serializer = new StringSerializer();
            var ms = new MemoryStream();
            serializer.WriteToStream(result, ms);
            var output = ms.ToArray();
            Assert.True(data.SequenceEqual(output));
        }

        [Fact]
        public void It_Parses_And_Serialized_Hex_UTF16BE()
        {
            var text = "<FEFF00540065007300740069006E006700480065007800550074006600310036>";
            var data = Encoding.ASCII.GetBytes(text);
            var parser = new StringParser(new ParsingContext());
            var result = parser.Parse(data);
            Assert.Equal("TestingHexUtf16", result.Value);
            Assert.Equal(PdfStringType.Hex, result.StringType);
            Assert.Equal(PdfTextEncodingType.UTF16BE, result.Encoding);
            var serializer = new StringSerializer();
            var ms = new MemoryStream();
            serializer.WriteToStream(result, ms);
            var output = ms.ToArray();
            Assert.True(data.SequenceEqual(output));
        }

        [Fact]
        public void It_Parses_And_Serializes_Raw_Bytes()
        {
            var root = PathUtil.GetPathFromSegmentOfCurrent("test");
            var data = File.ReadAllBytes(Path.Combine(root, "raw-byte-string.txt"));
            var parser = new StringParser(new ParsingContext());
            var result = parser.Parse(data);
            Assert.Equal(PdfStringType.Literal, result.StringType);
            var serializer = new StringSerializer();
            var ms = new MemoryStream();
            serializer.WriteToStream(result, ms);
            var output = ms.ToArray();
            File.WriteAllBytes(Path.Combine(root, "raw-byte-string-out.txt"), output);
            Assert.True(data.SequenceEqual(output));
        }
        [Fact]
        public void Experiment()
        {
            var text = @"C 32 ; WX 250 ; N space ; B 0 0 0 0 ;
C 33 ; WX 333 ; N exclam ; B 130 -9 238 676 ;
C 34 ; WX 408 ; N quotedbl ; B 77 431 331 676 ;
C 35 ; WX 500 ; N numbersign ; B 5 0 496 662 ;
C 36 ; WX 500 ; N dollar ; B 44 -87 457 727 ;
C 37 ; WX 833 ; N percent ; B 61 -13 772 676 ;
C 38 ; WX 778 ; N ampersand ; B 42 -13 750 676 ;
C 39 ; WX 333 ; N quoteright ; B 79 433 218 676 ;
C 40 ; WX 333 ; N parenleft ; B 48 -177 304 676 ;
C 41 ; WX 333 ; N parenright ; B 29 -177 285 676 ;
C 42 ; WX 500 ; N asterisk ; B 69 265 432 676 ;
C 43 ; WX 564 ; N plus ; B 30 0 534 506 ;
C 44 ; WX 250 ; N comma ; B 56 -141 195 102 ;
C 45 ; WX 333 ; N hyphen ; B 39 194 285 257 ;
C 46 ; WX 250 ; N period ; B 70 -11 181 100 ;
C 47 ; WX 278 ; N slash ; B -9 -14 287 676 ;
C 48 ; WX 500 ; N zero ; B 24 -14 476 676 ;
C 49 ; WX 500 ; N one ; B 111 0 394 676 ;
C 50 ; WX 500 ; N two ; B 30 0 475 676 ;
C 51 ; WX 500 ; N three ; B 43 -14 431 676 ;
C 52 ; WX 500 ; N four ; B 12 0 472 676 ;
C 53 ; WX 500 ; N five ; B 32 -14 438 688 ;
C 54 ; WX 500 ; N six ; B 34 -14 468 684 ;
C 55 ; WX 500 ; N seven ; B 20 -8 449 662 ;
C 56 ; WX 500 ; N eight ; B 56 -14 445 676 ;
C 57 ; WX 500 ; N nine ; B 30 -22 459 676 ;
C 58 ; WX 278 ; N colon ; B 81 -11 192 459 ;
C 59 ; WX 278 ; N semicolon ; B 80 -141 219 459 ;
C 60 ; WX 564 ; N less ; B 28 -8 536 514 ;
C 61 ; WX 564 ; N equal ; B 30 120 534 386 ;
C 62 ; WX 564 ; N greater ; B 28 -8 536 514 ;
C 63 ; WX 444 ; N question ; B 68 -8 414 676 ;
C 64 ; WX 921 ; N at ; B 116 -14 809 676 ;
C 65 ; WX 722 ; N A ; B 15 0 706 674 ;
C 66 ; WX 667 ; N B ; B 17 0 593 662 ;
C 67 ; WX 667 ; N C ; B 28 -14 633 676 ;
C 68 ; WX 722 ; N D ; B 16 0 685 662 ;
C 69 ; WX 611 ; N E ; B 12 0 597 662 ;
C 70 ; WX 556 ; N F ; B 12 0 546 662 ;
C 71 ; WX 722 ; N G ; B 32 -14 709 676 ;
C 72 ; WX 722 ; N H ; B 19 0 702 662 ;
C 73 ; WX 333 ; N I ; B 18 0 315 662 ;
C 74 ; WX 389 ; N J ; B 10 -14 370 662 ;
C 75 ; WX 722 ; N K ; B 34 0 723 662 ;
C 76 ; WX 611 ; N L ; B 12 0 598 662 ;
C 77 ; WX 889 ; N M ; B 12 0 863 662 ;
C 78 ; WX 722 ; N N ; B 12 -11 707 662 ;
C 79 ; WX 722 ; N O ; B 34 -14 688 676 ;
C 80 ; WX 556 ; N P ; B 16 0 542 662 ;
C 81 ; WX 722 ; N Q ; B 34 -178 701 676 ;
C 82 ; WX 667 ; N R ; B 17 0 659 662 ;
C 83 ; WX 556 ; N S ; B 42 -14 491 676 ;
C 84 ; WX 611 ; N T ; B 17 0 593 662 ;
C 85 ; WX 722 ; N U ; B 14 -14 705 662 ;
C 86 ; WX 722 ; N V ; B 16 -11 697 662 ;
C 87 ; WX 944 ; N W ; B 5 -11 932 662 ;
C 88 ; WX 722 ; N X ; B 10 0 704 662 ;
C 89 ; WX 722 ; N Y ; B 22 0 703 662 ;
C 90 ; WX 611 ; N Z ; B 9 0 597 662 ;
C 91 ; WX 333 ; N bracketleft ; B 88 -156 299 662 ;
C 92 ; WX 278 ; N backslash ; B -9 -14 287 676 ;
C 93 ; WX 333 ; N bracketright ; B 34 -156 245 662 ;
C 94 ; WX 469 ; N asciicircum ; B 24 297 446 662 ;
C 95 ; WX 500 ; N underscore ; B 0 -125 500 -75 ;
C 96 ; WX 333 ; N quoteleft ; B 115 433 254 676 ;
C 97 ; WX 444 ; N a ; B 37 -10 442 460 ;
C 98 ; WX 500 ; N b ; B 3 -10 468 683 ;
C 99 ; WX 444 ; N c ; B 25 -10 412 460 ;
C 100 ; WX 500 ; N d ; B 27 -10 491 683 ;
C 101 ; WX 444 ; N e ; B 25 -10 424 460 ;
C 102 ; WX 333 ; N f ; B 20 0 383 683 ; L i fi ; L l fl ;
C 103 ; WX 500 ; N g ; B 28 -218 470 460 ;
C 104 ; WX 500 ; N h ; B 9 0 487 683 ;
C 105 ; WX 278 ; N i ; B 16 0 253 683 ;
C 106 ; WX 278 ; N j ; B -70 -218 194 683 ;
C 107 ; WX 500 ; N k ; B 7 0 505 683 ;
C 108 ; WX 278 ; N l ; B 19 0 257 683 ;
C 109 ; WX 778 ; N m ; B 16 0 775 460 ;
C 110 ; WX 500 ; N n ; B 16 0 485 460 ;
C 111 ; WX 500 ; N o ; B 29 -10 470 460 ;
C 112 ; WX 500 ; N p ; B 5 -217 470 460 ;
C 113 ; WX 500 ; N q ; B 24 -217 488 460 ;
C 114 ; WX 333 ; N r ; B 5 0 335 460 ;
C 115 ; WX 389 ; N s ; B 51 -10 348 460 ;
C 116 ; WX 278 ; N t ; B 13 -10 279 579 ;
C 117 ; WX 500 ; N u ; B 9 -10 479 450 ;
C 118 ; WX 500 ; N v ; B 19 -14 477 450 ;
C 119 ; WX 722 ; N w ; B 21 -14 694 450 ;
C 120 ; WX 500 ; N x ; B 17 0 479 450 ;
C 121 ; WX 500 ; N y ; B 14 -218 475 450 ;
C 122 ; WX 444 ; N z ; B 27 0 418 450 ;
C 123 ; WX 480 ; N braceleft ; B 100 -181 350 680 ;
C 124 ; WX 200 ; N bar ; B 67 -218 133 782 ;
C 125 ; WX 480 ; N braceright ; B 130 -181 380 680 ;
C 126 ; WX 541 ; N asciitilde ; B 40 183 502 323 ;
C 161 ; WX 333 ; N exclamdown ; B 97 -218 205 467 ;
C 162 ; WX 500 ; N cent ; B 53 -138 448 579 ;
C 163 ; WX 500 ; N sterling ; B 12 -8 490 676 ;
C 164 ; WX 167 ; N fraction ; B -168 -14 331 676 ;
C 165 ; WX 500 ; N yen ; B -53 0 512 662 ;
C 166 ; WX 500 ; N florin ; B 7 -189 490 676 ;
C 167 ; WX 500 ; N section ; B 70 -148 426 676 ;
C 168 ; WX 500 ; N currency ; B -22 58 522 602 ;
C 169 ; WX 180 ; N quotesingle ; B 48 431 133 676 ;
C 170 ; WX 444 ; N quotedblleft ; B 43 433 414 676 ;
C 171 ; WX 500 ; N guillemotleft ; B 42 33 456 416 ;
C 172 ; WX 333 ; N guilsinglleft ; B 63 33 285 416 ;
C 173 ; WX 333 ; N guilsinglright ; B 48 33 270 416 ;
C 174 ; WX 556 ; N fi ; B 31 0 521 683 ;
C 175 ; WX 556 ; N fl ; B 32 0 521 683 ;
C 177 ; WX 500 ; N endash ; B 0 201 500 250 ;
C 178 ; WX 500 ; N dagger ; B 59 -149 442 676 ;
C 179 ; WX 500 ; N daggerdbl ; B 58 -153 442 676 ;
C 180 ; WX 250 ; N periodcentered ; B 70 199 181 310 ;
C 182 ; WX 453 ; N paragraph ; B -22 -154 450 662 ;
C 183 ; WX 350 ; N bullet ; B 40 196 310 466 ;
C 184 ; WX 333 ; N quotesinglbase ; B 79 -141 218 102 ;
C 185 ; WX 444 ; N quotedblbase ; B 45 -141 416 102 ;
C 186 ; WX 444 ; N quotedblright ; B 30 433 401 676 ;
C 187 ; WX 500 ; N guillemotright ; B 44 33 458 416 ;
C 188 ; WX 1000 ; N ellipsis ; B 111 -11 888 100 ;
C 189 ; WX 1000 ; N perthousand ; B 7 -19 994 706 ;
C 191 ; WX 444 ; N questiondown ; B 30 -218 376 466 ;
C 193 ; WX 333 ; N grave ; B 19 507 242 678 ;
C 194 ; WX 333 ; N acute ; B 93 507 317 678 ;
C 195 ; WX 333 ; N circumflex ; B 11 507 322 674 ;
C 196 ; WX 333 ; N tilde ; B 1 532 331 638 ;
C 197 ; WX 333 ; N macron ; B 11 547 322 601 ;
C 198 ; WX 333 ; N breve ; B 26 507 307 664 ;
C 199 ; WX 333 ; N dotaccent ; B 118 581 216 681 ;
C 200 ; WX 333 ; N dieresis ; B 18 581 315 681 ;
C 202 ; WX 333 ; N ring ; B 67 512 266 711 ;
C 203 ; WX 333 ; N cedilla ; B 52 -215 261 0 ;
C 205 ; WX 333 ; N hungarumlaut ; B -3 507 377 678 ;
C 206 ; WX 333 ; N ogonek ; B 62 -165 243 0 ;
C 207 ; WX 333 ; N caron ; B 11 507 322 674 ;
C 208 ; WX 1000 ; N emdash ; B 0 201 1000 250 ;
C 225 ; WX 889 ; N AE ; B 0 0 863 662 ;
C 227 ; WX 276 ; N ordfeminine ; B 4 394 270 676 ;
C 232 ; WX 611 ; N Lslash ; B 12 0 598 662 ;
C 233 ; WX 722 ; N Oslash ; B 34 -80 688 734 ;
C 234 ; WX 889 ; N OE ; B 30 -6 885 668 ;
C 235 ; WX 310 ; N ordmasculine ; B 6 394 304 676 ;
C 241 ; WX 667 ; N ae ; B 38 -10 632 460 ;
C 245 ; WX 278 ; N dotlessi ; B 16 0 253 460 ;
C 248 ; WX 278 ; N lslash ; B 19 0 259 683 ;
C 249 ; WX 500 ; N oslash ; B 29 -112 470 551 ;
C 250 ; WX 722 ; N oe ; B 30 -10 690 460 ;
C 251 ; WX 500 ; N germandbls ; B 12 -9 468 683 ;
C -1 ; WX 333 ; N Idieresis ; B 18 0 315 835 ;
C -1 ; WX 444 ; N eacute ; B 25 -10 424 678 ;
C -1 ; WX 444 ; N abreve ; B 37 -10 442 664 ;
C -1 ; WX 500 ; N uhungarumlaut ; B 9 -10 501 678 ;
C -1 ; WX 444 ; N ecaron ; B 25 -10 424 674 ;
C -1 ; WX 722 ; N Ydieresis ; B 22 0 703 835 ;
C -1 ; WX 564 ; N divide ; B 30 -10 534 516 ;
C -1 ; WX 722 ; N Yacute ; B 22 0 703 890 ;
C -1 ; WX 722 ; N Acircumflex ; B 15 0 706 886 ;
C -1 ; WX 444 ; N aacute ; B 37 -10 442 678 ;
C -1 ; WX 722 ; N Ucircumflex ; B 14 -14 705 886 ;
C -1 ; WX 500 ; N yacute ; B 14 -218 475 678 ;
C -1 ; WX 389 ; N scommaaccent ; B 51 -218 348 460 ;
C -1 ; WX 444 ; N ecircumflex ; B 25 -10 424 674 ;
C -1 ; WX 722 ; N Uring ; B 14 -14 705 898 ;
C -1 ; WX 722 ; N Udieresis ; B 14 -14 705 835 ;
C -1 ; WX 444 ; N aogonek ; B 37 -165 469 460 ;
C -1 ; WX 722 ; N Uacute ; B 14 -14 705 890 ;
C -1 ; WX 500 ; N uogonek ; B 9 -155 487 450 ;
C -1 ; WX 611 ; N Edieresis ; B 12 0 597 835 ;
C -1 ; WX 722 ; N Dcroat ; B 16 0 685 662 ;
C -1 ; WX 250 ; N commaaccent ; B 59 -218 184 -50 ;
C -1 ; WX 760 ; N copyright ; B 38 -14 722 676 ;
C -1 ; WX 611 ; N Emacron ; B 12 0 597 813 ;
C -1 ; WX 444 ; N ccaron ; B 25 -10 412 674 ;
C -1 ; WX 444 ; N aring ; B 37 -10 442 711 ;
C -1 ; WX 722 ; N Ncommaaccent ; B 12 -198 707 662 ;
C -1 ; WX 278 ; N lacute ; B 19 0 290 890 ;
C -1 ; WX 444 ; N agrave ; B 37 -10 442 678 ;
C -1 ; WX 611 ; N Tcommaaccent ; B 17 -218 593 662 ;
C -1 ; WX 667 ; N Cacute ; B 28 -14 633 890 ;
C -1 ; WX 444 ; N atilde ; B 37 -10 442 638 ;
C -1 ; WX 611 ; N Edotaccent ; B 12 0 597 835 ;
C -1 ; WX 389 ; N scaron ; B 39 -10 350 674 ;
C -1 ; WX 389 ; N scedilla ; B 51 -215 348 460 ;
C -1 ; WX 278 ; N iacute ; B 16 0 290 678 ;
C -1 ; WX 471 ; N lozenge ; B 13 0 459 724 ;
C -1 ; WX 667 ; N Rcaron ; B 17 0 659 886 ;
C -1 ; WX 722 ; N Gcommaaccent ; B 32 -218 709 676 ;
C -1 ; WX 500 ; N ucircumflex ; B 9 -10 479 674 ;
C -1 ; WX 444 ; N acircumflex ; B 37 -10 442 674 ;
C -1 ; WX 722 ; N Amacron ; B 15 0 706 813 ;
C -1 ; WX 333 ; N rcaron ; B 5 0 335 674 ;
C -1 ; WX 444 ; N ccedilla ; B 25 -215 412 460 ;
C -1 ; WX 611 ; N Zdotaccent ; B 9 0 597 835 ;
C -1 ; WX 556 ; N Thorn ; B 16 0 542 662 ;
C -1 ; WX 722 ; N Omacron ; B 34 -14 688 813 ;
C -1 ; WX 667 ; N Racute ; B 17 0 659 890 ;
C -1 ; WX 556 ; N Sacute ; B 42 -14 491 890 ;
C -1 ; WX 588 ; N dcaron ; B 27 -10 589 695 ;
C -1 ; WX 722 ; N Umacron ; B 14 -14 705 813 ;
C -1 ; WX 500 ; N uring ; B 9 -10 479 711 ;
C -1 ; WX 300 ; N threesuperior ; B 15 262 291 676 ;
C -1 ; WX 722 ; N Ograve ; B 34 -14 688 890 ;
C -1 ; WX 722 ; N Agrave ; B 15 0 706 890 ;
C -1 ; WX 722 ; N Abreve ; B 15 0 706 876 ;
C -1 ; WX 564 ; N multiply ; B 38 8 527 497 ;
C -1 ; WX 500 ; N uacute ; B 9 -10 479 678 ;
C -1 ; WX 611 ; N Tcaron ; B 17 0 593 886 ;
C -1 ; WX 476 ; N partialdiff ; B 17 -38 459 710 ;
C -1 ; WX 500 ; N ydieresis ; B 14 -218 475 623 ;
C -1 ; WX 722 ; N Nacute ; B 12 -11 707 890 ;
C -1 ; WX 278 ; N icircumflex ; B -16 0 295 674 ;
C -1 ; WX 611 ; N Ecircumflex ; B 12 0 597 886 ;
C -1 ; WX 444 ; N adieresis ; B 37 -10 442 623 ;
C -1 ; WX 444 ; N edieresis ; B 25 -10 424 623 ;
C -1 ; WX 444 ; N cacute ; B 25 -10 413 678 ;
C -1 ; WX 500 ; N nacute ; B 16 0 485 678 ;
C -1 ; WX 500 ; N umacron ; B 9 -10 479 601 ;
C -1 ; WX 722 ; N Ncaron ; B 12 -11 707 886 ;
C -1 ; WX 333 ; N Iacute ; B 18 0 317 890 ;
C -1 ; WX 564 ; N plusminus ; B 30 0 534 506 ;
C -1 ; WX 200 ; N brokenbar ; B 67 -143 133 707 ;
C -1 ; WX 760 ; N registered ; B 38 -14 722 676 ;
C -1 ; WX 722 ; N Gbreve ; B 32 -14 709 876 ;
C -1 ; WX 333 ; N Idotaccent ; B 18 0 315 835 ;
C -1 ; WX 600 ; N summation ; B 15 -10 585 706 ;
C -1 ; WX 611 ; N Egrave ; B 12 0 597 890 ;
C -1 ; WX 333 ; N racute ; B 5 0 335 678 ;
C -1 ; WX 500 ; N omacron ; B 29 -10 470 601 ;
C -1 ; WX 611 ; N Zacute ; B 9 0 597 890 ;
C -1 ; WX 611 ; N Zcaron ; B 9 0 597 886 ;
C -1 ; WX 549 ; N greaterequal ; B 26 0 523 666 ;
C -1 ; WX 722 ; N Eth ; B 16 0 685 662 ;
C -1 ; WX 667 ; N Ccedilla ; B 28 -215 633 676 ;
C -1 ; WX 278 ; N lcommaaccent ; B 19 -218 257 683 ;
C -1 ; WX 326 ; N tcaron ; B 13 -10 318 722 ;
C -1 ; WX 444 ; N eogonek ; B 25 -165 424 460 ;
C -1 ; WX 722 ; N Uogonek ; B 14 -165 705 662 ;
C -1 ; WX 722 ; N Aacute ; B 15 0 706 890 ;
C -1 ; WX 722 ; N Adieresis ; B 15 0 706 835 ;
C -1 ; WX 444 ; N egrave ; B 25 -10 424 678 ;
C -1 ; WX 444 ; N zacute ; B 27 0 418 678 ;
C -1 ; WX 278 ; N iogonek ; B 16 -165 265 683 ;
C -1 ; WX 722 ; N Oacute ; B 34 -14 688 890 ;
C -1 ; WX 500 ; N oacute ; B 29 -10 470 678 ;
C -1 ; WX 444 ; N amacron ; B 37 -10 442 601 ;
C -1 ; WX 389 ; N sacute ; B 51 -10 348 678 ;
C -1 ; WX 278 ; N idieresis ; B -9 0 288 623 ;
C -1 ; WX 722 ; N Ocircumflex ; B 34 -14 688 886 ;
C -1 ; WX 722 ; N Ugrave ; B 14 -14 705 890 ;
C -1 ; WX 612 ; N Delta ; B 6 0 608 688 ;
C -1 ; WX 500 ; N thorn ; B 5 -217 470 683 ;
C -1 ; WX 300 ; N twosuperior ; B 1 270 296 676 ;
C -1 ; WX 722 ; N Odieresis ; B 34 -14 688 835 ;
C -1 ; WX 500 ; N mu ; B 36 -218 512 450 ;
C -1 ; WX 278 ; N igrave ; B -8 0 253 678 ;
C -1 ; WX 500 ; N ohungarumlaut ; B 29 -10 491 678 ;
C -1 ; WX 611 ; N Eogonek ; B 12 -165 597 662 ;
C -1 ; WX 500 ; N dcroat ; B 27 -10 500 683 ;
C -1 ; WX 750 ; N threequarters ; B 15 -14 718 676 ;
C -1 ; WX 556 ; N Scedilla ; B 42 -215 491 676 ;
C -1 ; WX 344 ; N lcaron ; B 19 0 347 695 ;
C -1 ; WX 722 ; N Kcommaaccent ; B 34 -198 723 662 ;
C -1 ; WX 611 ; N Lacute ; B 12 0 598 890 ;
C -1 ; WX 980 ; N trademark ; B 30 256 957 662 ;
C -1 ; WX 444 ; N edotaccent ; B 25 -10 424 623 ;
C -1 ; WX 333 ; N Igrave ; B 18 0 315 890 ;
C -1 ; WX 333 ; N Imacron ; B 11 0 322 813 ;
C -1 ; WX 611 ; N Lcaron ; B 12 0 598 676 ;
C -1 ; WX 750 ; N onehalf ; B 31 -14 746 676 ;
C -1 ; WX 549 ; N lessequal ; B 26 0 523 666 ;
C -1 ; WX 500 ; N ocircumflex ; B 29 -10 470 674 ;
C -1 ; WX 500 ; N ntilde ; B 16 0 485 638 ;
C -1 ; WX 722 ; N Uhungarumlaut ; B 14 -14 705 890 ;
C -1 ; WX 611 ; N Eacute ; B 12 0 597 890 ;
C -1 ; WX 444 ; N emacron ; B 25 -10 424 601 ;
C -1 ; WX 500 ; N gbreve ; B 28 -218 470 664 ;
C -1 ; WX 750 ; N onequarter ; B 37 -14 718 676 ;
C -1 ; WX 556 ; N Scaron ; B 42 -14 491 886 ;
C -1 ; WX 556 ; N Scommaaccent ; B 42 -218 491 676 ;
C -1 ; WX 722 ; N Ohungarumlaut ; B 34 -14 688 890 ;
C -1 ; WX 400 ; N degree ; B 57 390 343 676 ;
C -1 ; WX 500 ; N ograve ; B 29 -10 470 678 ;
C -1 ; WX 667 ; N Ccaron ; B 28 -14 633 886 ;
C -1 ; WX 500 ; N ugrave ; B 9 -10 479 678 ;
C -1 ; WX 453 ; N radical ; B 2 -60 452 768 ;
C -1 ; WX 722 ; N Dcaron ; B 16 0 685 886 ;
C -1 ; WX 333 ; N rcommaaccent ; B 5 -218 335 460 ;
C -1 ; WX 722 ; N Ntilde ; B 12 -11 707 850 ;
C -1 ; WX 500 ; N otilde ; B 29 -10 470 638 ;
C -1 ; WX 667 ; N Rcommaaccent ; B 17 -198 659 662 ;
C -1 ; WX 611 ; N Lcommaaccent ; B 12 -218 598 662 ;
C -1 ; WX 722 ; N Atilde ; B 15 0 706 850 ;
C -1 ; WX 722 ; N Aogonek ; B 15 -165 738 674 ;
C -1 ; WX 722 ; N Aring ; B 15 0 706 898 ;
C -1 ; WX 722 ; N Otilde ; B 34 -14 688 850 ;
C -1 ; WX 444 ; N zdotaccent ; B 27 0 418 623 ;
C -1 ; WX 611 ; N Ecaron ; B 12 0 597 886 ;
C -1 ; WX 333 ; N Iogonek ; B 18 -165 315 662 ;
C -1 ; WX 500 ; N kcommaaccent ; B 7 -218 505 683 ;
C -1 ; WX 564 ; N minus ; B 30 220 534 286 ;
C -1 ; WX 333 ; N Icircumflex ; B 11 0 322 886 ;
C -1 ; WX 500 ; N ncaron ; B 16 0 485 674 ;
C -1 ; WX 278 ; N tcommaaccent ; B 13 -218 279 579 ;
C -1 ; WX 564 ; N logicalnot ; B 30 108 534 386 ;
C -1 ; WX 500 ; N odieresis ; B 29 -10 470 623 ;
C -1 ; WX 500 ; N udieresis ; B 9 -10 479 623 ;
C -1 ; WX 549 ; N notequal ; B 12 -31 537 547 ;
C -1 ; WX 500 ; N gcommaaccent ; B 28 -218 470 749 ;
C -1 ; WX 500 ; N eth ; B 29 -10 471 686 ;
C -1 ; WX 444 ; N zcaron ; B 27 0 418 674 ;
C -1 ; WX 500 ; N ncommaaccent ; B 16 -218 485 460 ;
C -1 ; WX 300 ; N onesuperior ; B 57 270 248 676 ;
C -1 ; WX 278 ; N imacron ; B 6 0 271 601 ;
C -1 ; WX 500 ; N Euro ; B 0 0 0 0 ;";
            var output = new StringBuilder();
            var all = new Dictionary<string, Glyph>();
            var def = new Glyph[256];
            var defNames = new string[256];
            foreach (var line in text.Split('\n'))
            {
                var segs = line.Split(';');
                var w = int.Parse(segs[1].Trim().Split(' ')[1]) / 1000.0;
                var cc = int.Parse(segs[0].Trim().Split(' ')[1]);
                var gn = segs[2].Trim().Split(' ')[1];
                var uc = GlyphNames.Lookup[gn];
                var g = new Glyph();
                g.Char = uc;
                g.w0 = (float)w;
                g.IsWordSpace = cc == 32;
                g.BBox = new decimal[4];

                var b = segs[3].Trim().Split(' ');
                g.BBox[0] = decimal.Parse(b[1]) / 1000m;
                g.BBox[1] = decimal.Parse(b[2]) / 1000m;
                g.BBox[2] = decimal.Parse(b[3]) / 1000m;
                g.BBox[3] = decimal.Parse(b[4]) / 1000m;
                all[gn] = g;
                if (cc > 0)
                {
                    g.CodePoint = (ushort)cc;
                    defNames[cc] = gn;
                    def[cc] = g;
                }
            }

            output.Append("\tinternal class TimeRomanMetrics {\n");
            for (var i=0;i<def.Length;i++)
            {
                var g =  def[i];
                if (g == null) { continue; }
                output.Append($"\t\tstatic Glyph {defNames[i]} = new Glyph {{ Char = (char){(int)(g.Char)}, w0 = {g.w0}F, ");
                output.Append($"IsWordSpace = {g.IsWordSpace.ToString().ToLower()}, ");
                output.Append($"CodePoint = {g.CodePoint}, ");
                output.Append($"BBox = new decimal[] {{ {g.BBox[0].ToString()}m, {g.BBox[1].ToString()}m, {g.BBox[2].ToString()}m, {g.BBox[3].ToString()}m }}  }};\n");
            }
            output.Append("\t\tpublic static Glyph[] DefaultEncoding = new Glyph[] {\n");
            for (var i = 0; i < def.Length; i++)
            {
                var g = def[i];
                if (g == null) { output.Append($"\t\t\tnull,\n");  continue; }
                output.Append($"\t\t\t{defNames[i]},\n");
            }
            output.Append("\t\t};\n");
            output.Append("\t\tpublic static byte[] DefaultChars = new byte[] {\n");
            var fastLookup = def.Select((c, i) => (c,i)).Where(x => x.c != null && x.c.Char < 255).OrderBy(x => (int)x.c.Char)
                .ToDictionary(x=> (int)x.c.Char, v=> v.i);
            for (var i = 0; i < 255; i++)
            {
                if (fastLookup.TryGetValue(i, out var g)) {
                    
                    output.Append($"\t\t\t0x{g:x2},\n");
                } else
                {
                    output.Append($"\t\t\t0,\n");
                }
            }
            output.Append("\t\t};\n");
            output.Append("\t\tpublic static Dictionary<string, Glyph> AllGlyphs = new Dictionary<string, Glyph> {\n");
            foreach (var kvp in all)
            {
                var txt = "";
                if (defNames.Contains(kvp.Key))
                {
                    txt = kvp.Key;
                } else
                {
                    var g = kvp.Value;
                    txt = $"new Glyph {{ Char = (char){(int)(g.Char)}, w0 = {g.w0}F, ";
                    txt += $"IsWordSpace = {g.IsWordSpace.ToString().ToLower()}, ";
                    txt += $"CodePoint = {g.CodePoint}, ";
                    txt += $"BBox = new decimal[] {{ {g.BBox[0].ToString()}m, {g.BBox[1].ToString()}m, {g.BBox[2].ToString()}m, {g.BBox[3].ToString()}m }}  }}";
                }
                output.Append($"\t\t\t[\"{kvp.Key}\"] = {txt},\n");
            }
            output.Append("\t\t};\n");
            output.Append("\t\tpublic static Dictionary<char, byte> DefinedChars = new Dictionary<char, byte> {\n");
            foreach (var g in def)
            {
                if (g == null) { continue; }
                var c = (int)g.Char;
                output.Append($"\t\t\t['\\u{c:x4}'] = 0x{g.CodePoint:x2},\n");
            }
            output.Append("\t\t};\n");
            output.Append("\t}\n");
            var str = output.ToString();

        }
    }
}