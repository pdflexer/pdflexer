using System.Collections.Generic;

namespace PdfLexer.Fonts.Predefined {
	internal class TimesRomanGlyphs {
		static Glyph space = new Glyph { Char = (char)32, w0 = 0.25F, IsWordSpace = true, CodePoint = 32, Name = "space", Undefined = false, BBox = new decimal[] { 0m, 0m, 0m, 0m }, Kernings = new Dictionary<char,float> {  ['\u0041'] = 55f, ['\u00C1'] = 55f, ['\u0102'] = 55f, ['\u00C2'] = 55f, ['\u00C4'] = 55f, ['\u00C0'] = 55f, ['\u0100'] = 55f, ['\u0104'] = 55f, ['\u00C5'] = 55f, ['\u00C3'] = 55f, ['\u0054'] = 18f, ['\u0164'] = 18f, ['\u0162'] = 18f, ['\u0056'] = 50f, ['\u0057'] = 30f, ['\u0059'] = 90f, ['\u00DD'] = 90f, ['\u0178'] = 90f, } };
		static Glyph exclam = new Glyph { Char = (char)33, w0 = 0.333F, IsWordSpace = false, CodePoint = 33, Name = "exclam", Undefined = false, BBox = new decimal[] { 0.13m, -0.009m, 0.238m, 0.676m }};
		static Glyph quotedbl = new Glyph { Char = (char)34, w0 = 0.408F, IsWordSpace = false, CodePoint = 34, Name = "quotedbl", Undefined = false, BBox = new decimal[] { 0.077m, 0.431m, 0.331m, 0.676m }};
		static Glyph numbersign = new Glyph { Char = (char)35, w0 = 0.5F, IsWordSpace = false, CodePoint = 35, Name = "numbersign", Undefined = false, BBox = new decimal[] { 0.005m, 0m, 0.496m, 0.662m }};
		static Glyph dollar = new Glyph { Char = (char)36, w0 = 0.5F, IsWordSpace = false, CodePoint = 36, Name = "dollar", Undefined = false, BBox = new decimal[] { 0.044m, -0.087m, 0.457m, 0.727m }};
		static Glyph percent = new Glyph { Char = (char)37, w0 = 0.833F, IsWordSpace = false, CodePoint = 37, Name = "percent", Undefined = false, BBox = new decimal[] { 0.061m, -0.013m, 0.772m, 0.676m }};
		static Glyph ampersand = new Glyph { Char = (char)38, w0 = 0.778F, IsWordSpace = false, CodePoint = 38, Name = "ampersand", Undefined = false, BBox = new decimal[] { 0.042m, -0.013m, 0.75m, 0.676m }};
		static Glyph quoteright = new Glyph { Char = (char)8217, w0 = 0.333F, IsWordSpace = false, CodePoint = 39, Name = "quoteright", Undefined = false, BBox = new decimal[] { 0.079m, 0.433m, 0.218m, 0.676m }, Kernings = new Dictionary<char,float> {  ['\u0064'] = 50f, ['\u0111'] = 50f, ['\u006C'] = 10f, ['\u013A'] = 10f, ['\u013C'] = 10f, ['\u0142'] = 10f, ['\u2019'] = 74f, ['\u0072'] = 50f, ['\u0155'] = 50f, ['\u0159'] = 50f, ['\u0157'] = 50f, ['\u0073'] = 55f, ['\u015B'] = 55f, ['\u0161'] = 55f, ['\u015F'] = 55f, ['\u0219'] = 55f, ['\u0020'] = 74f, ['\u0074'] = 18f, ['\u0163'] = 18f, ['\u0076'] = 50f, } };
		static Glyph parenleft = new Glyph { Char = (char)40, w0 = 0.333F, IsWordSpace = false, CodePoint = 40, Name = "parenleft", Undefined = false, BBox = new decimal[] { 0.048m, -0.177m, 0.304m, 0.676m }};
		static Glyph parenright = new Glyph { Char = (char)41, w0 = 0.333F, IsWordSpace = false, CodePoint = 41, Name = "parenright", Undefined = false, BBox = new decimal[] { 0.029m, -0.177m, 0.285m, 0.676m }};
		static Glyph asterisk = new Glyph { Char = (char)42, w0 = 0.5F, IsWordSpace = false, CodePoint = 42, Name = "asterisk", Undefined = false, BBox = new decimal[] { 0.069m, 0.265m, 0.432m, 0.676m }};
		static Glyph plus = new Glyph { Char = (char)43, w0 = 0.564F, IsWordSpace = false, CodePoint = 43, Name = "plus", Undefined = false, BBox = new decimal[] { 0.03m, 0m, 0.534m, 0.506m }};
		static Glyph comma = new Glyph { Char = (char)44, w0 = 0.25F, IsWordSpace = false, CodePoint = 44, Name = "comma", Undefined = false, BBox = new decimal[] { 0.056m, -0.141m, 0.195m, 0.102m }, Kernings = new Dictionary<char,float> {  ['\u201D'] = 70f, ['\u2019'] = 70f, } };
		static Glyph hyphen = new Glyph { Char = (char)45, w0 = 0.333F, IsWordSpace = false, CodePoint = 45, Name = "hyphen", Undefined = false, BBox = new decimal[] { 0.039m, 0.194m, 0.285m, 0.257m }};
		static Glyph period = new Glyph { Char = (char)46, w0 = 0.25F, IsWordSpace = false, CodePoint = 46, Name = "period", Undefined = false, BBox = new decimal[] { 0.07m, -0.011m, 0.181m, 0.1m }, Kernings = new Dictionary<char,float> {  ['\u201D'] = 70f, ['\u2019'] = 70f, } };
		static Glyph slash = new Glyph { Char = (char)47, w0 = 0.278F, IsWordSpace = false, CodePoint = 47, Name = "slash", Undefined = false, BBox = new decimal[] { -0.009m, -0.014m, 0.287m, 0.676m }};
		static Glyph zero = new Glyph { Char = (char)48, w0 = 0.5F, IsWordSpace = false, CodePoint = 48, Name = "zero", Undefined = false, BBox = new decimal[] { 0.024m, -0.014m, 0.476m, 0.676m }};
		static Glyph one = new Glyph { Char = (char)49, w0 = 0.5F, IsWordSpace = false, CodePoint = 49, Name = "one", Undefined = false, BBox = new decimal[] { 0.111m, 0m, 0.394m, 0.676m }};
		static Glyph two = new Glyph { Char = (char)50, w0 = 0.5F, IsWordSpace = false, CodePoint = 50, Name = "two", Undefined = false, BBox = new decimal[] { 0.03m, 0m, 0.475m, 0.676m }};
		static Glyph three = new Glyph { Char = (char)51, w0 = 0.5F, IsWordSpace = false, CodePoint = 51, Name = "three", Undefined = false, BBox = new decimal[] { 0.043m, -0.014m, 0.431m, 0.676m }};
		static Glyph four = new Glyph { Char = (char)52, w0 = 0.5F, IsWordSpace = false, CodePoint = 52, Name = "four", Undefined = false, BBox = new decimal[] { 0.012m, 0m, 0.472m, 0.676m }};
		static Glyph five = new Glyph { Char = (char)53, w0 = 0.5F, IsWordSpace = false, CodePoint = 53, Name = "five", Undefined = false, BBox = new decimal[] { 0.032m, -0.014m, 0.438m, 0.688m }};
		static Glyph six = new Glyph { Char = (char)54, w0 = 0.5F, IsWordSpace = false, CodePoint = 54, Name = "six", Undefined = false, BBox = new decimal[] { 0.034m, -0.014m, 0.468m, 0.684m }};
		static Glyph seven = new Glyph { Char = (char)55, w0 = 0.5F, IsWordSpace = false, CodePoint = 55, Name = "seven", Undefined = false, BBox = new decimal[] { 0.02m, -0.008m, 0.449m, 0.662m }};
		static Glyph eight = new Glyph { Char = (char)56, w0 = 0.5F, IsWordSpace = false, CodePoint = 56, Name = "eight", Undefined = false, BBox = new decimal[] { 0.056m, -0.014m, 0.445m, 0.676m }};
		static Glyph nine = new Glyph { Char = (char)57, w0 = 0.5F, IsWordSpace = false, CodePoint = 57, Name = "nine", Undefined = false, BBox = new decimal[] { 0.03m, -0.022m, 0.459m, 0.676m }};
		static Glyph colon = new Glyph { Char = (char)58, w0 = 0.278F, IsWordSpace = false, CodePoint = 58, Name = "colon", Undefined = false, BBox = new decimal[] { 0.081m, -0.011m, 0.192m, 0.459m }};
		static Glyph semicolon = new Glyph { Char = (char)59, w0 = 0.278F, IsWordSpace = false, CodePoint = 59, Name = "semicolon", Undefined = false, BBox = new decimal[] { 0.08m, -0.141m, 0.219m, 0.459m }};
		static Glyph less = new Glyph { Char = (char)60, w0 = 0.564F, IsWordSpace = false, CodePoint = 60, Name = "less", Undefined = false, BBox = new decimal[] { 0.028m, -0.008m, 0.536m, 0.514m }};
		static Glyph equal = new Glyph { Char = (char)61, w0 = 0.564F, IsWordSpace = false, CodePoint = 61, Name = "equal", Undefined = false, BBox = new decimal[] { 0.03m, 0.12m, 0.534m, 0.386m }};
		static Glyph greater = new Glyph { Char = (char)62, w0 = 0.564F, IsWordSpace = false, CodePoint = 62, Name = "greater", Undefined = false, BBox = new decimal[] { 0.028m, -0.008m, 0.536m, 0.514m }};
		static Glyph question = new Glyph { Char = (char)63, w0 = 0.444F, IsWordSpace = false, CodePoint = 63, Name = "question", Undefined = false, BBox = new decimal[] { 0.068m, -0.008m, 0.414m, 0.676m }};
		static Glyph at = new Glyph { Char = (char)64, w0 = 0.921F, IsWordSpace = false, CodePoint = 64, Name = "at", Undefined = false, BBox = new decimal[] { 0.116m, -0.014m, 0.809m, 0.676m }};
		static Glyph A = new Glyph { Char = (char)65, w0 = 0.722F, IsWordSpace = false, CodePoint = 65, Name = "A", Undefined = false, BBox = new decimal[] { 0.015m, 0m, 0.706m, 0.674m }, Kernings = new Dictionary<char,float> {  ['\u0043'] = 40f, ['\u0106'] = 40f, ['\u010C'] = 40f, ['\u00C7'] = 40f, ['\u0047'] = 40f, ['\u011E'] = 40f, ['\u0122'] = 40f, ['\u004F'] = 55f, ['\u00D3'] = 55f, ['\u00D4'] = 55f, ['\u00D6'] = 55f, ['\u00D2'] = 55f, ['\u0150'] = 55f, ['\u014C'] = 55f, ['\u00D8'] = 55f, ['\u00D5'] = 55f, ['\u0051'] = 55f, ['\u0054'] = 111f, ['\u0164'] = 111f, ['\u0162'] = 111f, ['\u0055'] = 55f, ['\u00DA'] = 55f, ['\u00DB'] = 55f, ['\u00DC'] = 55f, ['\u00D9'] = 55f, ['\u0170'] = 55f, ['\u016A'] = 55f, ['\u0172'] = 55f, ['\u016E'] = 55f, ['\u0056'] = 135f, ['\u0057'] = 90f, ['\u0059'] = 105f, ['\u00DD'] = 105f, ['\u0178'] = 105f, ['\u2019'] = 111f, ['\u0076'] = 74f, ['\u0077'] = 92f, ['\u0079'] = 92f, ['\u00FD'] = 92f, ['\u00FF'] = 92f, } };
		static Glyph B = new Glyph { Char = (char)66, w0 = 0.667F, IsWordSpace = false, CodePoint = 66, Name = "B", Undefined = false, BBox = new decimal[] { 0.017m, 0m, 0.593m, 0.662m }, Kernings = new Dictionary<char,float> {  ['\u0041'] = 35f, ['\u00C1'] = 35f, ['\u0102'] = 35f, ['\u00C2'] = 35f, ['\u00C4'] = 35f, ['\u00C0'] = 35f, ['\u0100'] = 35f, ['\u0104'] = 35f, ['\u00C5'] = 35f, ['\u00C3'] = 35f, ['\u0055'] = 10f, ['\u00DA'] = 10f, ['\u00DB'] = 10f, ['\u00DC'] = 10f, ['\u00D9'] = 10f, ['\u0170'] = 10f, ['\u016A'] = 10f, ['\u0172'] = 10f, ['\u016E'] = 10f, } };
		static Glyph C = new Glyph { Char = (char)67, w0 = 0.667F, IsWordSpace = false, CodePoint = 67, Name = "C", Undefined = false, BBox = new decimal[] { 0.028m, -0.014m, 0.633m, 0.676m }};
		static Glyph D = new Glyph { Char = (char)68, w0 = 0.722F, IsWordSpace = false, CodePoint = 68, Name = "D", Undefined = false, BBox = new decimal[] { 0.016m, 0m, 0.685m, 0.662m }, Kernings = new Dictionary<char,float> {  ['\u0041'] = 40f, ['\u00C1'] = 40f, ['\u0102'] = 40f, ['\u00C2'] = 40f, ['\u00C4'] = 40f, ['\u00C0'] = 40f, ['\u0100'] = 40f, ['\u0104'] = 40f, ['\u00C5'] = 40f, ['\u00C3'] = 40f, ['\u0056'] = 40f, ['\u0057'] = 30f, ['\u0059'] = 55f, ['\u00DD'] = 55f, ['\u0178'] = 55f, } };
		static Glyph E = new Glyph { Char = (char)69, w0 = 0.611F, IsWordSpace = false, CodePoint = 69, Name = "E", Undefined = false, BBox = new decimal[] { 0.012m, 0m, 0.597m, 0.662m }};
		static Glyph F = new Glyph { Char = (char)70, w0 = 0.556F, IsWordSpace = false, CodePoint = 70, Name = "F", Undefined = false, BBox = new decimal[] { 0.012m, 0m, 0.546m, 0.662m }, Kernings = new Dictionary<char,float> {  ['\u0041'] = 74f, ['\u00C1'] = 74f, ['\u0102'] = 74f, ['\u00C2'] = 74f, ['\u00C4'] = 74f, ['\u00C0'] = 74f, ['\u0100'] = 74f, ['\u0104'] = 74f, ['\u00C5'] = 74f, ['\u00C3'] = 74f, ['\u0061'] = 15f, ['\u00E1'] = 15f, ['\u0103'] = 15f, ['\u00E2'] = 15f, ['\u00E4'] = 15f, ['\u00E0'] = 15f, ['\u0101'] = 15f, ['\u0105'] = 15f, ['\u00E5'] = 15f, ['\u00E3'] = 15f, ['\u002C'] = 80f, ['\u006F'] = 15f, ['\u00F3'] = 15f, ['\u00F4'] = 15f, ['\u00F6'] = 15f, ['\u00F2'] = 15f, ['\u0151'] = 15f, ['\u014D'] = 15f, ['\u00F8'] = 15f, ['\u00F5'] = 15f, ['\u002E'] = 80f, } };
		static Glyph G = new Glyph { Char = (char)71, w0 = 0.722F, IsWordSpace = false, CodePoint = 71, Name = "G", Undefined = false, BBox = new decimal[] { 0.032m, -0.014m, 0.709m, 0.676m }};
		static Glyph H = new Glyph { Char = (char)72, w0 = 0.722F, IsWordSpace = false, CodePoint = 72, Name = "H", Undefined = false, BBox = new decimal[] { 0.019m, 0m, 0.702m, 0.662m }};
		static Glyph I = new Glyph { Char = (char)73, w0 = 0.333F, IsWordSpace = false, CodePoint = 73, Name = "I", Undefined = false, BBox = new decimal[] { 0.018m, 0m, 0.315m, 0.662m }};
		static Glyph J = new Glyph { Char = (char)74, w0 = 0.389F, IsWordSpace = false, CodePoint = 74, Name = "J", Undefined = false, BBox = new decimal[] { 0.01m, -0.014m, 0.37m, 0.662m }, Kernings = new Dictionary<char,float> {  ['\u0041'] = 60f, ['\u00C1'] = 60f, ['\u0102'] = 60f, ['\u00C2'] = 60f, ['\u00C4'] = 60f, ['\u00C0'] = 60f, ['\u0100'] = 60f, ['\u0104'] = 60f, ['\u00C5'] = 60f, ['\u00C3'] = 60f, } };
		static Glyph K = new Glyph { Char = (char)75, w0 = 0.722F, IsWordSpace = false, CodePoint = 75, Name = "K", Undefined = false, BBox = new decimal[] { 0.034m, 0m, 0.723m, 0.662m }, Kernings = new Dictionary<char,float> {  ['\u004F'] = 30f, ['\u00D3'] = 30f, ['\u00D4'] = 30f, ['\u00D6'] = 30f, ['\u00D2'] = 30f, ['\u0150'] = 30f, ['\u014C'] = 30f, ['\u00D8'] = 30f, ['\u00D5'] = 30f, ['\u0065'] = 25f, ['\u00E9'] = 25f, ['\u011B'] = 25f, ['\u00EA'] = 25f, ['\u00EB'] = 25f, ['\u0117'] = 25f, ['\u00E8'] = 25f, ['\u0113'] = 25f, ['\u0119'] = 25f, ['\u006F'] = 35f, ['\u00F3'] = 35f, ['\u00F4'] = 35f, ['\u00F6'] = 35f, ['\u00F2'] = 35f, ['\u0151'] = 35f, ['\u014D'] = 35f, ['\u00F8'] = 35f, ['\u00F5'] = 35f, ['\u0075'] = 15f, ['\u00FA'] = 15f, ['\u00FB'] = 15f, ['\u00FC'] = 15f, ['\u00F9'] = 15f, ['\u0171'] = 15f, ['\u016B'] = 15f, ['\u0173'] = 15f, ['\u016F'] = 15f, ['\u0079'] = 25f, ['\u00FD'] = 25f, ['\u00FF'] = 25f, } };
		static Glyph L = new Glyph { Char = (char)76, w0 = 0.611F, IsWordSpace = false, CodePoint = 76, Name = "L", Undefined = false, BBox = new decimal[] { 0.012m, 0m, 0.598m, 0.662m }, Kernings = new Dictionary<char,float> {  ['\u0054'] = 92f, ['\u0164'] = 92f, ['\u0162'] = 92f, ['\u0056'] = 100f, ['\u0057'] = 74f, ['\u0059'] = 100f, ['\u00DD'] = 100f, ['\u0178'] = 100f, ['\u2019'] = 92f, ['\u0079'] = 55f, ['\u00FD'] = 55f, ['\u00FF'] = 55f, } };
		static Glyph M = new Glyph { Char = (char)77, w0 = 0.889F, IsWordSpace = false, CodePoint = 77, Name = "M", Undefined = false, BBox = new decimal[] { 0.012m, 0m, 0.863m, 0.662m }};
		static Glyph N = new Glyph { Char = (char)78, w0 = 0.722F, IsWordSpace = false, CodePoint = 78, Name = "N", Undefined = false, BBox = new decimal[] { 0.012m, -0.011m, 0.707m, 0.662m }, Kernings = new Dictionary<char,float> {  ['\u0041'] = 35f, ['\u00C1'] = 35f, ['\u0102'] = 35f, ['\u00C2'] = 35f, ['\u00C4'] = 35f, ['\u00C0'] = 35f, ['\u0100'] = 35f, ['\u0104'] = 35f, ['\u00C5'] = 35f, ['\u00C3'] = 35f, } };
		static Glyph O = new Glyph { Char = (char)79, w0 = 0.722F, IsWordSpace = false, CodePoint = 79, Name = "O", Undefined = false, BBox = new decimal[] { 0.034m, -0.014m, 0.688m, 0.676m }, Kernings = new Dictionary<char,float> {  ['\u0041'] = 35f, ['\u00C1'] = 35f, ['\u0102'] = 35f, ['\u00C2'] = 35f, ['\u00C4'] = 35f, ['\u00C0'] = 35f, ['\u0100'] = 35f, ['\u0104'] = 35f, ['\u00C5'] = 35f, ['\u00C3'] = 35f, ['\u0054'] = 40f, ['\u0164'] = 40f, ['\u0162'] = 40f, ['\u0056'] = 50f, ['\u0057'] = 35f, ['\u0058'] = 40f, ['\u0059'] = 50f, ['\u00DD'] = 50f, ['\u0178'] = 50f, } };
		static Glyph P = new Glyph { Char = (char)80, w0 = 0.556F, IsWordSpace = false, CodePoint = 80, Name = "P", Undefined = false, BBox = new decimal[] { 0.016m, 0m, 0.542m, 0.662m }, Kernings = new Dictionary<char,float> {  ['\u0041'] = 92f, ['\u00C1'] = 92f, ['\u0102'] = 92f, ['\u00C2'] = 92f, ['\u00C4'] = 92f, ['\u00C0'] = 92f, ['\u0100'] = 92f, ['\u0104'] = 92f, ['\u00C5'] = 92f, ['\u00C3'] = 92f, ['\u0061'] = 15f, ['\u00E1'] = 15f, ['\u0103'] = 15f, ['\u00E2'] = 15f, ['\u00E4'] = 15f, ['\u00E0'] = 15f, ['\u0101'] = 15f, ['\u0105'] = 15f, ['\u00E5'] = 15f, ['\u00E3'] = 15f, ['\u002C'] = 111f, ['\u002E'] = 111f, } };
		static Glyph Q = new Glyph { Char = (char)81, w0 = 0.722F, IsWordSpace = false, CodePoint = 81, Name = "Q", Undefined = false, BBox = new decimal[] { 0.034m, -0.178m, 0.701m, 0.676m }, Kernings = new Dictionary<char,float> {  ['\u0055'] = 10f, ['\u00DA'] = 10f, ['\u00DB'] = 10f, ['\u00DC'] = 10f, ['\u00D9'] = 10f, ['\u0170'] = 10f, ['\u016A'] = 10f, ['\u0172'] = 10f, ['\u016E'] = 10f, } };
		static Glyph R = new Glyph { Char = (char)82, w0 = 0.667F, IsWordSpace = false, CodePoint = 82, Name = "R", Undefined = false, BBox = new decimal[] { 0.017m, 0m, 0.659m, 0.662m }, Kernings = new Dictionary<char,float> {  ['\u004F'] = 40f, ['\u00D3'] = 40f, ['\u00D4'] = 40f, ['\u00D6'] = 40f, ['\u00D2'] = 40f, ['\u0150'] = 40f, ['\u014C'] = 40f, ['\u00D8'] = 40f, ['\u00D5'] = 40f, ['\u0054'] = 60f, ['\u0164'] = 60f, ['\u0162'] = 60f, ['\u0055'] = 40f, ['\u00DA'] = 40f, ['\u00DB'] = 40f, ['\u00DC'] = 40f, ['\u00D9'] = 40f, ['\u0170'] = 40f, ['\u016A'] = 40f, ['\u0172'] = 40f, ['\u016E'] = 40f, ['\u0056'] = 80f, ['\u0057'] = 55f, ['\u0059'] = 65f, ['\u00DD'] = 65f, ['\u0178'] = 65f, } };
		static Glyph S = new Glyph { Char = (char)83, w0 = 0.556F, IsWordSpace = false, CodePoint = 83, Name = "S", Undefined = false, BBox = new decimal[] { 0.042m, -0.014m, 0.491m, 0.676m }};
		static Glyph T = new Glyph { Char = (char)84, w0 = 0.611F, IsWordSpace = false, CodePoint = 84, Name = "T", Undefined = false, BBox = new decimal[] { 0.017m, 0m, 0.593m, 0.662m }, Kernings = new Dictionary<char,float> {  ['\u0041'] = 93f, ['\u00C1'] = 93f, ['\u0102'] = 93f, ['\u00C2'] = 93f, ['\u00C4'] = 93f, ['\u00C0'] = 93f, ['\u0100'] = 93f, ['\u0104'] = 93f, ['\u00C5'] = 93f, ['\u00C3'] = 93f, ['\u004F'] = 18f, ['\u00D3'] = 18f, ['\u00D4'] = 18f, ['\u00D6'] = 18f, ['\u00D2'] = 18f, ['\u0150'] = 18f, ['\u014C'] = 18f, ['\u00D8'] = 18f, ['\u00D5'] = 18f, ['\u0061'] = 80f, ['\u00E1'] = 80f, ['\u0103'] = 80f, ['\u00E2'] = 80f, ['\u00E4'] = 40f, ['\u00E0'] = 40f, ['\u0101'] = 40f, ['\u0105'] = 80f, ['\u00E5'] = 80f, ['\u00E3'] = 40f, ['\u003A'] = 50f, ['\u002C'] = 74f, ['\u0065'] = 70f, ['\u00E9'] = 70f, ['\u011B'] = 70f, ['\u00EA'] = 70f, ['\u00EB'] = 30f, ['\u0117'] = 70f, ['\u00E8'] = 70f, ['\u0113'] = 30f, ['\u0119'] = 70f, ['\u002D'] = 92f, ['\u0069'] = 35f, ['\u00ED'] = 35f, ['\u012F'] = 35f, ['\u006F'] = 80f, ['\u00F3'] = 80f, ['\u00F4'] = 80f, ['\u00F6'] = 80f, ['\u00F2'] = 80f, ['\u0151'] = 80f, ['\u014D'] = 80f, ['\u00F8'] = 80f, ['\u00F5'] = 80f, ['\u002E'] = 74f, ['\u0072'] = 35f, ['\u0155'] = 35f, ['\u0159'] = 35f, ['\u0157'] = 35f, ['\u003B'] = 55f, ['\u0075'] = 45f, ['\u00FA'] = 45f, ['\u00FB'] = 45f, ['\u00FC'] = 45f, ['\u00F9'] = 45f, ['\u0171'] = 45f, ['\u016B'] = 45f, ['\u0173'] = 45f, ['\u016F'] = 45f, ['\u0077'] = 80f, ['\u0079'] = 80f, ['\u00FD'] = 80f, ['\u00FF'] = 80f, } };
		static Glyph U = new Glyph { Char = (char)85, w0 = 0.722F, IsWordSpace = false, CodePoint = 85, Name = "U", Undefined = false, BBox = new decimal[] { 0.014m, -0.014m, 0.705m, 0.662m }, Kernings = new Dictionary<char,float> {  ['\u0041'] = 40f, ['\u00C1'] = 40f, ['\u0102'] = 40f, ['\u00C2'] = 40f, ['\u00C4'] = 40f, ['\u00C0'] = 40f, ['\u0100'] = 40f, ['\u0104'] = 40f, ['\u00C5'] = 40f, ['\u00C3'] = 40f, } };
		static Glyph V = new Glyph { Char = (char)86, w0 = 0.722F, IsWordSpace = false, CodePoint = 86, Name = "V", Undefined = false, BBox = new decimal[] { 0.016m, -0.011m, 0.697m, 0.662m }, Kernings = new Dictionary<char,float> {  ['\u0041'] = 135f, ['\u00C1'] = 135f, ['\u0102'] = 135f, ['\u00C2'] = 135f, ['\u00C4'] = 135f, ['\u00C0'] = 135f, ['\u0100'] = 135f, ['\u0104'] = 135f, ['\u00C5'] = 135f, ['\u00C3'] = 135f, ['\u0047'] = 15f, ['\u011E'] = 15f, ['\u0122'] = 15f, ['\u004F'] = 40f, ['\u00D3'] = 40f, ['\u00D4'] = 40f, ['\u00D6'] = 40f, ['\u00D2'] = 40f, ['\u0150'] = 40f, ['\u014C'] = 40f, ['\u00D8'] = 40f, ['\u00D5'] = 40f, ['\u0061'] = 111f, ['\u00E1'] = 111f, ['\u0103'] = 111f, ['\u00E2'] = 71f, ['\u00E4'] = 71f, ['\u00E0'] = 71f, ['\u0101'] = 71f, ['\u0105'] = 111f, ['\u00E5'] = 111f, ['\u00E3'] = 71f, ['\u003A'] = 74f, ['\u002C'] = 129f, ['\u0065'] = 111f, ['\u00E9'] = 111f, ['\u011B'] = 71f, ['\u00EA'] = 71f, ['\u00EB'] = 71f, ['\u0117'] = 111f, ['\u00E8'] = 71f, ['\u0113'] = 71f, ['\u0119'] = 111f, ['\u002D'] = 100f, ['\u0069'] = 60f, ['\u00ED'] = 60f, ['\u00EE'] = 20f, ['\u00EF'] = 20f, ['\u00EC'] = 20f, ['\u012B'] = 20f, ['\u012F'] = 60f, ['\u006F'] = 129f, ['\u00F3'] = 129f, ['\u00F4'] = 129f, ['\u00F6'] = 89f, ['\u00F2'] = 89f, ['\u0151'] = 129f, ['\u014D'] = 89f, ['\u00F8'] = 129f, ['\u00F5'] = 89f, ['\u002E'] = 129f, ['\u003B'] = 74f, ['\u0075'] = 75f, ['\u00FA'] = 75f, ['\u00FB'] = 75f, ['\u00FC'] = 75f, ['\u00F9'] = 75f, ['\u0171'] = 75f, ['\u016B'] = 75f, ['\u0173'] = 75f, ['\u016F'] = 75f, } };
		static Glyph W = new Glyph { Char = (char)87, w0 = 0.944F, IsWordSpace = false, CodePoint = 87, Name = "W", Undefined = false, BBox = new decimal[] { 0.005m, -0.011m, 0.932m, 0.662m }, Kernings = new Dictionary<char,float> {  ['\u0041'] = 120f, ['\u00C1'] = 120f, ['\u0102'] = 120f, ['\u00C2'] = 120f, ['\u00C4'] = 120f, ['\u00C0'] = 120f, ['\u0100'] = 120f, ['\u0104'] = 120f, ['\u00C5'] = 120f, ['\u00C3'] = 120f, ['\u004F'] = 10f, ['\u00D3'] = 10f, ['\u00D4'] = 10f, ['\u00D6'] = 10f, ['\u00D2'] = 10f, ['\u0150'] = 10f, ['\u014C'] = 10f, ['\u00D8'] = 10f, ['\u00D5'] = 10f, ['\u0061'] = 80f, ['\u00E1'] = 80f, ['\u0103'] = 80f, ['\u00E2'] = 80f, ['\u00E4'] = 80f, ['\u00E0'] = 80f, ['\u0101'] = 80f, ['\u0105'] = 80f, ['\u00E5'] = 80f, ['\u00E3'] = 80f, ['\u003A'] = 37f, ['\u002C'] = 92f, ['\u0065'] = 80f, ['\u00E9'] = 80f, ['\u011B'] = 80f, ['\u00EA'] = 80f, ['\u00EB'] = 40f, ['\u0117'] = 80f, ['\u00E8'] = 40f, ['\u0113'] = 40f, ['\u0119'] = 80f, ['\u002D'] = 65f, ['\u0069'] = 40f, ['\u00ED'] = 40f, ['\u012F'] = 40f, ['\u006F'] = 80f, ['\u00F3'] = 80f, ['\u00F4'] = 80f, ['\u00F6'] = 80f, ['\u00F2'] = 80f, ['\u0151'] = 80f, ['\u014D'] = 80f, ['\u00F8'] = 80f, ['\u00F5'] = 80f, ['\u002E'] = 92f, ['\u003B'] = 37f, ['\u0075'] = 50f, ['\u00FA'] = 50f, ['\u00FB'] = 50f, ['\u00FC'] = 50f, ['\u00F9'] = 50f, ['\u0171'] = 50f, ['\u016B'] = 50f, ['\u0173'] = 50f, ['\u016F'] = 50f, ['\u0079'] = 73f, ['\u00FD'] = 73f, ['\u00FF'] = 73f, } };
		static Glyph X = new Glyph { Char = (char)88, w0 = 0.722F, IsWordSpace = false, CodePoint = 88, Name = "X", Undefined = false, BBox = new decimal[] { 0.01m, 0m, 0.704m, 0.662m }};
		static Glyph Y = new Glyph { Char = (char)89, w0 = 0.722F, IsWordSpace = false, CodePoint = 89, Name = "Y", Undefined = false, BBox = new decimal[] { 0.022m, 0m, 0.703m, 0.662m }, Kernings = new Dictionary<char,float> {  ['\u0041'] = 120f, ['\u00C1'] = 120f, ['\u0102'] = 120f, ['\u00C2'] = 120f, ['\u00C4'] = 120f, ['\u00C0'] = 120f, ['\u0100'] = 120f, ['\u0104'] = 120f, ['\u00C5'] = 120f, ['\u00C3'] = 120f, ['\u004F'] = 30f, ['\u00D3'] = 30f, ['\u00D4'] = 30f, ['\u00D6'] = 30f, ['\u00D2'] = 30f, ['\u0150'] = 30f, ['\u014C'] = 30f, ['\u00D8'] = 30f, ['\u00D5'] = 30f, ['\u0061'] = 100f, ['\u00E1'] = 100f, ['\u0103'] = 100f, ['\u00E2'] = 100f, ['\u00E4'] = 60f, ['\u00E0'] = 60f, ['\u0101'] = 60f, ['\u0105'] = 100f, ['\u00E5'] = 100f, ['\u00E3'] = 60f, ['\u003A'] = 92f, ['\u002C'] = 129f, ['\u0065'] = 100f, ['\u00E9'] = 100f, ['\u011B'] = 100f, ['\u00EA'] = 100f, ['\u00EB'] = 60f, ['\u0117'] = 100f, ['\u00E8'] = 60f, ['\u0113'] = 60f, ['\u0119'] = 100f, ['\u002D'] = 111f, ['\u0069'] = 55f, ['\u00ED'] = 55f, ['\u012F'] = 55f, ['\u006F'] = 110f, ['\u00F3'] = 110f, ['\u00F4'] = 110f, ['\u00F6'] = 70f, ['\u00F2'] = 70f, ['\u0151'] = 110f, ['\u014D'] = 70f, ['\u00F8'] = 110f, ['\u00F5'] = 70f, ['\u002E'] = 129f, ['\u003B'] = 92f, ['\u0075'] = 111f, ['\u00FA'] = 111f, ['\u00FB'] = 111f, ['\u00FC'] = 71f, ['\u00F9'] = 71f, ['\u0171'] = 111f, ['\u016B'] = 71f, ['\u0173'] = 111f, ['\u016F'] = 111f, } };
		static Glyph Z = new Glyph { Char = (char)90, w0 = 0.611F, IsWordSpace = false, CodePoint = 90, Name = "Z", Undefined = false, BBox = new decimal[] { 0.009m, 0m, 0.597m, 0.662m }};
		static Glyph bracketleft = new Glyph { Char = (char)91, w0 = 0.333F, IsWordSpace = false, CodePoint = 91, Name = "bracketleft", Undefined = false, BBox = new decimal[] { 0.088m, -0.156m, 0.299m, 0.662m }};
		static Glyph backslash = new Glyph { Char = (char)92, w0 = 0.278F, IsWordSpace = false, CodePoint = 92, Name = "backslash", Undefined = false, BBox = new decimal[] { -0.009m, -0.014m, 0.287m, 0.676m }};
		static Glyph bracketright = new Glyph { Char = (char)93, w0 = 0.333F, IsWordSpace = false, CodePoint = 93, Name = "bracketright", Undefined = false, BBox = new decimal[] { 0.034m, -0.156m, 0.245m, 0.662m }};
		static Glyph asciicircum = new Glyph { Char = (char)94, w0 = 0.469F, IsWordSpace = false, CodePoint = 94, Name = "asciicircum", Undefined = false, BBox = new decimal[] { 0.024m, 0.297m, 0.446m, 0.662m }};
		static Glyph underscore = new Glyph { Char = (char)95, w0 = 0.5F, IsWordSpace = false, CodePoint = 95, Name = "underscore", Undefined = false, BBox = new decimal[] { 0m, -0.125m, 0.5m, -0.075m }};
		static Glyph quoteleft = new Glyph { Char = (char)8216, w0 = 0.333F, IsWordSpace = false, CodePoint = 96, Name = "quoteleft", Undefined = false, BBox = new decimal[] { 0.115m, 0.433m, 0.254m, 0.676m }, Kernings = new Dictionary<char,float> {  ['\u0041'] = 80f, ['\u00C1'] = 80f, ['\u0102'] = 80f, ['\u00C2'] = 80f, ['\u00C4'] = 80f, ['\u00C0'] = 80f, ['\u0100'] = 80f, ['\u0104'] = 80f, ['\u00C5'] = 80f, ['\u00C3'] = 80f, ['\u2018'] = 74f, } };
		static Glyph a = new Glyph { Char = (char)97, w0 = 0.444F, IsWordSpace = false, CodePoint = 97, Name = "a", Undefined = false, BBox = new decimal[] { 0.037m, -0.01m, 0.442m, 0.46m }, Kernings = new Dictionary<char,float> {  ['\u0076'] = 20f, ['\u0077'] = 15f, } };
		static Glyph b = new Glyph { Char = (char)98, w0 = 0.5F, IsWordSpace = false, CodePoint = 98, Name = "b", Undefined = false, BBox = new decimal[] { 0.003m, -0.01m, 0.468m, 0.683m }, Kernings = new Dictionary<char,float> {  ['\u002E'] = 40f, ['\u0075'] = 20f, ['\u00FA'] = 20f, ['\u00FB'] = 20f, ['\u00FC'] = 20f, ['\u00F9'] = 20f, ['\u0171'] = 20f, ['\u016B'] = 20f, ['\u0173'] = 20f, ['\u016F'] = 20f, ['\u0076'] = 15f, } };
		static Glyph c = new Glyph { Char = (char)99, w0 = 0.444F, IsWordSpace = false, CodePoint = 99, Name = "c", Undefined = false, BBox = new decimal[] { 0.025m, -0.01m, 0.412m, 0.46m }, Kernings = new Dictionary<char,float> {  ['\u0079'] = 15f, ['\u00FD'] = 15f, ['\u00FF'] = 15f, } };
		static Glyph d = new Glyph { Char = (char)100, w0 = 0.5F, IsWordSpace = false, CodePoint = 100, Name = "d", Undefined = false, BBox = new decimal[] { 0.027m, -0.01m, 0.491m, 0.683m }};
		static Glyph e = new Glyph { Char = (char)101, w0 = 0.444F, IsWordSpace = false, CodePoint = 101, Name = "e", Undefined = false, BBox = new decimal[] { 0.025m, -0.01m, 0.424m, 0.46m }, Kernings = new Dictionary<char,float> {  ['\u0067'] = 15f, ['\u011F'] = 15f, ['\u0123'] = 15f, ['\u0076'] = 25f, ['\u0077'] = 25f, ['\u0078'] = 15f, ['\u0079'] = 15f, ['\u00FD'] = 15f, ['\u00FF'] = 15f, } };
		static Glyph f = new Glyph { Char = (char)102, w0 = 0.333F, IsWordSpace = false, CodePoint = 102, Name = "f", Undefined = false, BBox = new decimal[] { 0.02m, 0m, 0.383m, 0.683m }, Kernings = new Dictionary<char,float> {  ['\u0061'] = 10f, ['\u00E1'] = 10f, ['\u0103'] = 10f, ['\u00E2'] = 10f, ['\u00E4'] = 10f, ['\u00E0'] = 10f, ['\u0101'] = 10f, ['\u0105'] = 10f, ['\u00E5'] = 10f, ['\u00E3'] = 10f, ['\u0131'] = 50f, ['\u0066'] = 25f, ['\u0069'] = 20f, ['\u00ED'] = 20f, ['\u2019'] = -55f, } };
		static Glyph g = new Glyph { Char = (char)103, w0 = 0.5F, IsWordSpace = false, CodePoint = 103, Name = "g", Undefined = false, BBox = new decimal[] { 0.028m, -0.218m, 0.47m, 0.46m }, Kernings = new Dictionary<char,float> {  ['\u0061'] = 5f, ['\u00E1'] = 5f, ['\u0103'] = 5f, ['\u00E2'] = 5f, ['\u00E4'] = 5f, ['\u00E0'] = 5f, ['\u0101'] = 5f, ['\u0105'] = 5f, ['\u00E5'] = 5f, ['\u00E3'] = 5f, } };
		static Glyph h = new Glyph { Char = (char)104, w0 = 0.5F, IsWordSpace = false, CodePoint = 104, Name = "h", Undefined = false, BBox = new decimal[] { 0.009m, 0m, 0.487m, 0.683m }, Kernings = new Dictionary<char,float> {  ['\u0079'] = 5f, ['\u00FD'] = 5f, ['\u00FF'] = 5f, } };
		static Glyph i = new Glyph { Char = (char)105, w0 = 0.278F, IsWordSpace = false, CodePoint = 105, Name = "i", Undefined = false, BBox = new decimal[] { 0.016m, 0m, 0.253m, 0.683m }, Kernings = new Dictionary<char,float> {  ['\u0076'] = 25f, } };
		static Glyph j = new Glyph { Char = (char)106, w0 = 0.278F, IsWordSpace = false, CodePoint = 106, Name = "j", Undefined = false, BBox = new decimal[] { -0.07m, -0.218m, 0.194m, 0.683m }};
		static Glyph k = new Glyph { Char = (char)107, w0 = 0.5F, IsWordSpace = false, CodePoint = 107, Name = "k", Undefined = false, BBox = new decimal[] { 0.007m, 0m, 0.505m, 0.683m }, Kernings = new Dictionary<char,float> {  ['\u0065'] = 10f, ['\u00E9'] = 10f, ['\u011B'] = 10f, ['\u00EA'] = 10f, ['\u00EB'] = 10f, ['\u0117'] = 10f, ['\u00E8'] = 10f, ['\u0113'] = 10f, ['\u0119'] = 10f, ['\u006F'] = 10f, ['\u00F3'] = 10f, ['\u00F4'] = 10f, ['\u00F6'] = 10f, ['\u00F2'] = 10f, ['\u0151'] = 10f, ['\u014D'] = 10f, ['\u00F8'] = 10f, ['\u00F5'] = 10f, ['\u0079'] = 15f, ['\u00FD'] = 15f, ['\u00FF'] = 15f, } };
		static Glyph l = new Glyph { Char = (char)108, w0 = 0.278F, IsWordSpace = false, CodePoint = 108, Name = "l", Undefined = false, BBox = new decimal[] { 0.019m, 0m, 0.257m, 0.683m }, Kernings = new Dictionary<char,float> {  ['\u0077'] = 10f, } };
		static Glyph m = new Glyph { Char = (char)109, w0 = 0.778F, IsWordSpace = false, CodePoint = 109, Name = "m", Undefined = false, BBox = new decimal[] { 0.016m, 0m, 0.775m, 0.46m }};
		static Glyph n = new Glyph { Char = (char)110, w0 = 0.5F, IsWordSpace = false, CodePoint = 110, Name = "n", Undefined = false, BBox = new decimal[] { 0.016m, 0m, 0.485m, 0.46m }, Kernings = new Dictionary<char,float> {  ['\u0076'] = 40f, ['\u0079'] = 15f, ['\u00FD'] = 15f, ['\u00FF'] = 15f, } };
		static Glyph o = new Glyph { Char = (char)111, w0 = 0.5F, IsWordSpace = false, CodePoint = 111, Name = "o", Undefined = false, BBox = new decimal[] { 0.029m, -0.01m, 0.47m, 0.46m }, Kernings = new Dictionary<char,float> {  ['\u0076'] = 15f, ['\u0077'] = 25f, ['\u0079'] = 10f, ['\u00FD'] = 10f, ['\u00FF'] = 10f, } };
		static Glyph p = new Glyph { Char = (char)112, w0 = 0.5F, IsWordSpace = false, CodePoint = 112, Name = "p", Undefined = false, BBox = new decimal[] { 0.005m, -0.217m, 0.47m, 0.46m }, Kernings = new Dictionary<char,float> {  ['\u0079'] = 10f, ['\u00FD'] = 10f, ['\u00FF'] = 10f, } };
		static Glyph q = new Glyph { Char = (char)113, w0 = 0.5F, IsWordSpace = false, CodePoint = 113, Name = "q", Undefined = false, BBox = new decimal[] { 0.024m, -0.217m, 0.488m, 0.46m }};
		static Glyph r = new Glyph { Char = (char)114, w0 = 0.333F, IsWordSpace = false, CodePoint = 114, Name = "r", Undefined = false, BBox = new decimal[] { 0.005m, 0m, 0.335m, 0.46m }, Kernings = new Dictionary<char,float> {  ['\u002C'] = 40f, ['\u0067'] = 18f, ['\u011F'] = 18f, ['\u0123'] = 18f, ['\u002D'] = 20f, ['\u002E'] = 55f, } };
		static Glyph s = new Glyph { Char = (char)115, w0 = 0.389F, IsWordSpace = false, CodePoint = 115, Name = "s", Undefined = false, BBox = new decimal[] { 0.051m, -0.01m, 0.348m, 0.46m }};
		static Glyph t = new Glyph { Char = (char)116, w0 = 0.278F, IsWordSpace = false, CodePoint = 116, Name = "t", Undefined = false, BBox = new decimal[] { 0.013m, -0.01m, 0.279m, 0.579m }};
		static Glyph u = new Glyph { Char = (char)117, w0 = 0.5F, IsWordSpace = false, CodePoint = 117, Name = "u", Undefined = false, BBox = new decimal[] { 0.009m, -0.01m, 0.479m, 0.45m }};
		static Glyph v = new Glyph { Char = (char)118, w0 = 0.5F, IsWordSpace = false, CodePoint = 118, Name = "v", Undefined = false, BBox = new decimal[] { 0.019m, -0.014m, 0.477m, 0.45m }, Kernings = new Dictionary<char,float> {  ['\u0061'] = 25f, ['\u00E1'] = 25f, ['\u0103'] = 25f, ['\u00E2'] = 25f, ['\u00E4'] = 25f, ['\u00E0'] = 25f, ['\u0101'] = 25f, ['\u0105'] = 25f, ['\u00E5'] = 25f, ['\u00E3'] = 25f, ['\u002C'] = 65f, ['\u0065'] = 15f, ['\u00E9'] = 15f, ['\u011B'] = 15f, ['\u00EA'] = 15f, ['\u00EB'] = 15f, ['\u0117'] = 15f, ['\u00E8'] = 15f, ['\u0113'] = 15f, ['\u0119'] = 15f, ['\u006F'] = 20f, ['\u00F3'] = 20f, ['\u00F4'] = 20f, ['\u00F6'] = 20f, ['\u00F2'] = 20f, ['\u0151'] = 20f, ['\u014D'] = 20f, ['\u00F8'] = 20f, ['\u00F5'] = 20f, ['\u002E'] = 65f, } };
		static Glyph w = new Glyph { Char = (char)119, w0 = 0.722F, IsWordSpace = false, CodePoint = 119, Name = "w", Undefined = false, BBox = new decimal[] { 0.021m, -0.014m, 0.694m, 0.45m }, Kernings = new Dictionary<char,float> {  ['\u0061'] = 10f, ['\u00E1'] = 10f, ['\u0103'] = 10f, ['\u00E2'] = 10f, ['\u00E4'] = 10f, ['\u00E0'] = 10f, ['\u0101'] = 10f, ['\u0105'] = 10f, ['\u00E5'] = 10f, ['\u00E3'] = 10f, ['\u002C'] = 65f, ['\u006F'] = 10f, ['\u00F3'] = 10f, ['\u00F4'] = 10f, ['\u00F6'] = 10f, ['\u00F2'] = 10f, ['\u0151'] = 10f, ['\u014D'] = 10f, ['\u00F8'] = 10f, ['\u00F5'] = 10f, ['\u002E'] = 65f, } };
		static Glyph x = new Glyph { Char = (char)120, w0 = 0.5F, IsWordSpace = false, CodePoint = 120, Name = "x", Undefined = false, BBox = new decimal[] { 0.017m, 0m, 0.479m, 0.45m }, Kernings = new Dictionary<char,float> {  ['\u0065'] = 15f, ['\u00E9'] = 15f, ['\u011B'] = 15f, ['\u00EA'] = 15f, ['\u00EB'] = 15f, ['\u0117'] = 15f, ['\u00E8'] = 15f, ['\u0113'] = 15f, ['\u0119'] = 15f, } };
		static Glyph y = new Glyph { Char = (char)121, w0 = 0.5F, IsWordSpace = false, CodePoint = 121, Name = "y", Undefined = false, BBox = new decimal[] { 0.014m, -0.218m, 0.475m, 0.45m }, Kernings = new Dictionary<char,float> {  ['\u002C'] = 65f, ['\u002E'] = 65f, } };
		static Glyph z = new Glyph { Char = (char)122, w0 = 0.444F, IsWordSpace = false, CodePoint = 122, Name = "z", Undefined = false, BBox = new decimal[] { 0.027m, 0m, 0.418m, 0.45m }};
		static Glyph braceleft = new Glyph { Char = (char)123, w0 = 0.48F, IsWordSpace = false, CodePoint = 123, Name = "braceleft", Undefined = false, BBox = new decimal[] { 0.1m, -0.181m, 0.35m, 0.68m }};
		static Glyph bar = new Glyph { Char = (char)124, w0 = 0.2F, IsWordSpace = false, CodePoint = 124, Name = "bar", Undefined = false, BBox = new decimal[] { 0.067m, -0.218m, 0.133m, 0.782m }};
		static Glyph braceright = new Glyph { Char = (char)125, w0 = 0.48F, IsWordSpace = false, CodePoint = 125, Name = "braceright", Undefined = false, BBox = new decimal[] { 0.13m, -0.181m, 0.38m, 0.68m }};
		static Glyph asciitilde = new Glyph { Char = (char)126, w0 = 0.541F, IsWordSpace = false, CodePoint = 126, Name = "asciitilde", Undefined = false, BBox = new decimal[] { 0.04m, 0.183m, 0.502m, 0.323m }};
		static Glyph exclamdown = new Glyph { Char = (char)161, w0 = 0.333F, IsWordSpace = false, CodePoint = 161, Name = "exclamdown", Undefined = false, BBox = new decimal[] { 0.097m, -0.218m, 0.205m, 0.467m }};
		static Glyph cent = new Glyph { Char = (char)162, w0 = 0.5F, IsWordSpace = false, CodePoint = 162, Name = "cent", Undefined = false, BBox = new decimal[] { 0.053m, -0.138m, 0.448m, 0.579m }};
		static Glyph sterling = new Glyph { Char = (char)163, w0 = 0.5F, IsWordSpace = false, CodePoint = 163, Name = "sterling", Undefined = false, BBox = new decimal[] { 0.012m, -0.008m, 0.49m, 0.676m }};
		static Glyph fraction = new Glyph { Char = (char)8260, w0 = 0.167F, IsWordSpace = false, CodePoint = 164, Name = "fraction", Undefined = false, BBox = new decimal[] { -0.168m, -0.014m, 0.331m, 0.676m }};
		static Glyph yen = new Glyph { Char = (char)165, w0 = 0.5F, IsWordSpace = false, CodePoint = 165, Name = "yen", Undefined = false, BBox = new decimal[] { -0.053m, 0m, 0.512m, 0.662m }};
		static Glyph florin = new Glyph { Char = (char)402, w0 = 0.5F, IsWordSpace = false, CodePoint = 166, Name = "florin", Undefined = false, BBox = new decimal[] { 0.007m, -0.189m, 0.49m, 0.676m }};
		static Glyph section = new Glyph { Char = (char)167, w0 = 0.5F, IsWordSpace = false, CodePoint = 167, Name = "section", Undefined = false, BBox = new decimal[] { 0.07m, -0.148m, 0.426m, 0.676m }};
		static Glyph currency = new Glyph { Char = (char)164, w0 = 0.5F, IsWordSpace = false, CodePoint = 168, Name = "currency", Undefined = false, BBox = new decimal[] { -0.022m, 0.058m, 0.522m, 0.602m }};
		static Glyph quotesingle = new Glyph { Char = (char)39, w0 = 0.18F, IsWordSpace = false, CodePoint = 169, Name = "quotesingle", Undefined = false, BBox = new decimal[] { 0.048m, 0.431m, 0.133m, 0.676m }};
		static Glyph quotedblleft = new Glyph { Char = (char)8220, w0 = 0.444F, IsWordSpace = false, CodePoint = 170, Name = "quotedblleft", Undefined = false, BBox = new decimal[] { 0.043m, 0.433m, 0.414m, 0.676m }, Kernings = new Dictionary<char,float> {  ['\u0041'] = 80f, ['\u00C1'] = 80f, ['\u0102'] = 80f, ['\u00C2'] = 80f, ['\u00C4'] = 80f, ['\u00C0'] = 80f, ['\u0100'] = 80f, ['\u0104'] = 80f, ['\u00C5'] = 80f, ['\u00C3'] = 80f, } };
		static Glyph guillemotleft = new Glyph { Char = (char)171, w0 = 0.5F, IsWordSpace = false, CodePoint = 171, Name = "guillemotleft", Undefined = false, BBox = new decimal[] { 0.042m, 0.033m, 0.456m, 0.416m }};
		static Glyph guilsinglleft = new Glyph { Char = (char)8249, w0 = 0.333F, IsWordSpace = false, CodePoint = 172, Name = "guilsinglleft", Undefined = false, BBox = new decimal[] { 0.063m, 0.033m, 0.285m, 0.416m }};
		static Glyph guilsinglright = new Glyph { Char = (char)8250, w0 = 0.333F, IsWordSpace = false, CodePoint = 173, Name = "guilsinglright", Undefined = false, BBox = new decimal[] { 0.048m, 0.033m, 0.27m, 0.416m }};
		static Glyph fi = new Glyph { Char = (char)64257, w0 = 0.556F, IsWordSpace = false, CodePoint = 174, Name = "fi", Undefined = false, BBox = new decimal[] { 0.031m, 0m, 0.521m, 0.683m }};
		static Glyph fl = new Glyph { Char = (char)64258, w0 = 0.556F, IsWordSpace = false, CodePoint = 175, Name = "fl", Undefined = false, BBox = new decimal[] { 0.032m, 0m, 0.521m, 0.683m }};
		static Glyph endash = new Glyph { Char = (char)8211, w0 = 0.5F, IsWordSpace = false, CodePoint = 177, Name = "endash", Undefined = false, BBox = new decimal[] { 0m, 0.201m, 0.5m, 0.25m }};
		static Glyph dagger = new Glyph { Char = (char)8224, w0 = 0.5F, IsWordSpace = false, CodePoint = 178, Name = "dagger", Undefined = false, BBox = new decimal[] { 0.059m, -0.149m, 0.442m, 0.676m }};
		static Glyph daggerdbl = new Glyph { Char = (char)8225, w0 = 0.5F, IsWordSpace = false, CodePoint = 179, Name = "daggerdbl", Undefined = false, BBox = new decimal[] { 0.058m, -0.153m, 0.442m, 0.676m }};
		static Glyph periodcentered = new Glyph { Char = (char)183, w0 = 0.25F, IsWordSpace = false, CodePoint = 180, Name = "periodcentered", Undefined = false, BBox = new decimal[] { 0.07m, 0.199m, 0.181m, 0.31m }};
		static Glyph paragraph = new Glyph { Char = (char)182, w0 = 0.453F, IsWordSpace = false, CodePoint = 182, Name = "paragraph", Undefined = false, BBox = new decimal[] { -0.022m, -0.154m, 0.45m, 0.662m }};
		static Glyph bullet = new Glyph { Char = (char)8226, w0 = 0.35F, IsWordSpace = false, CodePoint = 183, Name = "bullet", Undefined = false, BBox = new decimal[] { 0.04m, 0.196m, 0.31m, 0.466m }};
		static Glyph quotesinglbase = new Glyph { Char = (char)8218, w0 = 0.333F, IsWordSpace = false, CodePoint = 184, Name = "quotesinglbase", Undefined = false, BBox = new decimal[] { 0.079m, -0.141m, 0.218m, 0.102m }};
		static Glyph quotedblbase = new Glyph { Char = (char)8222, w0 = 0.444F, IsWordSpace = false, CodePoint = 185, Name = "quotedblbase", Undefined = false, BBox = new decimal[] { 0.045m, -0.141m, 0.416m, 0.102m }};
		static Glyph quotedblright = new Glyph { Char = (char)8221, w0 = 0.444F, IsWordSpace = false, CodePoint = 186, Name = "quotedblright", Undefined = false, BBox = new decimal[] { 0.03m, 0.433m, 0.401m, 0.676m }};
		static Glyph guillemotright = new Glyph { Char = (char)187, w0 = 0.5F, IsWordSpace = false, CodePoint = 187, Name = "guillemotright", Undefined = false, BBox = new decimal[] { 0.044m, 0.033m, 0.458m, 0.416m }};
		static Glyph ellipsis = new Glyph { Char = (char)8230, w0 = 1F, IsWordSpace = false, CodePoint = 188, Name = "ellipsis", Undefined = false, BBox = new decimal[] { 0.111m, -0.011m, 0.888m, 0.1m }};
		static Glyph perthousand = new Glyph { Char = (char)8240, w0 = 1F, IsWordSpace = false, CodePoint = 189, Name = "perthousand", Undefined = false, BBox = new decimal[] { 0.007m, -0.019m, 0.994m, 0.706m }};
		static Glyph questiondown = new Glyph { Char = (char)191, w0 = 0.444F, IsWordSpace = false, CodePoint = 191, Name = "questiondown", Undefined = false, BBox = new decimal[] { 0.03m, -0.218m, 0.376m, 0.466m }};
		static Glyph grave = new Glyph { Char = (char)96, w0 = 0.333F, IsWordSpace = false, CodePoint = 193, Name = "grave", Undefined = false, BBox = new decimal[] { 0.019m, 0.507m, 0.242m, 0.678m }};
		static Glyph acute = new Glyph { Char = (char)180, w0 = 0.333F, IsWordSpace = false, CodePoint = 194, Name = "acute", Undefined = false, BBox = new decimal[] { 0.093m, 0.507m, 0.317m, 0.678m }};
		static Glyph circumflex = new Glyph { Char = (char)710, w0 = 0.333F, IsWordSpace = false, CodePoint = 195, Name = "circumflex", Undefined = false, BBox = new decimal[] { 0.011m, 0.507m, 0.322m, 0.674m }};
		static Glyph tilde = new Glyph { Char = (char)732, w0 = 0.333F, IsWordSpace = false, CodePoint = 196, Name = "tilde", Undefined = false, BBox = new decimal[] { 0.001m, 0.532m, 0.331m, 0.638m }};
		static Glyph macron = new Glyph { Char = (char)175, w0 = 0.333F, IsWordSpace = false, CodePoint = 197, Name = "macron", Undefined = false, BBox = new decimal[] { 0.011m, 0.547m, 0.322m, 0.601m }};
		static Glyph breve = new Glyph { Char = (char)728, w0 = 0.333F, IsWordSpace = false, CodePoint = 198, Name = "breve", Undefined = false, BBox = new decimal[] { 0.026m, 0.507m, 0.307m, 0.664m }};
		static Glyph dotaccent = new Glyph { Char = (char)729, w0 = 0.333F, IsWordSpace = false, CodePoint = 199, Name = "dotaccent", Undefined = false, BBox = new decimal[] { 0.118m, 0.581m, 0.216m, 0.681m }};
		static Glyph dieresis = new Glyph { Char = (char)168, w0 = 0.333F, IsWordSpace = false, CodePoint = 200, Name = "dieresis", Undefined = false, BBox = new decimal[] { 0.018m, 0.581m, 0.315m, 0.681m }};
		static Glyph ring = new Glyph { Char = (char)730, w0 = 0.333F, IsWordSpace = false, CodePoint = 202, Name = "ring", Undefined = false, BBox = new decimal[] { 0.067m, 0.512m, 0.266m, 0.711m }};
		static Glyph cedilla = new Glyph { Char = (char)184, w0 = 0.333F, IsWordSpace = false, CodePoint = 203, Name = "cedilla", Undefined = false, BBox = new decimal[] { 0.052m, -0.215m, 0.261m, 0m }};
		static Glyph hungarumlaut = new Glyph { Char = (char)733, w0 = 0.333F, IsWordSpace = false, CodePoint = 205, Name = "hungarumlaut", Undefined = false, BBox = new decimal[] { -0.003m, 0.507m, 0.377m, 0.678m }};
		static Glyph ogonek = new Glyph { Char = (char)731, w0 = 0.333F, IsWordSpace = false, CodePoint = 206, Name = "ogonek", Undefined = false, BBox = new decimal[] { 0.062m, -0.165m, 0.243m, 0m }};
		static Glyph caron = new Glyph { Char = (char)711, w0 = 0.333F, IsWordSpace = false, CodePoint = 207, Name = "caron", Undefined = false, BBox = new decimal[] { 0.011m, 0.507m, 0.322m, 0.674m }};
		static Glyph emdash = new Glyph { Char = (char)8212, w0 = 1F, IsWordSpace = false, CodePoint = 208, Name = "emdash", Undefined = false, BBox = new decimal[] { 0m, 0.201m, 1m, 0.25m }};
		static Glyph AE = new Glyph { Char = (char)198, w0 = 0.889F, IsWordSpace = false, CodePoint = 225, Name = "AE", Undefined = false, BBox = new decimal[] { 0m, 0m, 0.863m, 0.662m }};
		static Glyph ordfeminine = new Glyph { Char = (char)170, w0 = 0.276F, IsWordSpace = false, CodePoint = 227, Name = "ordfeminine", Undefined = false, BBox = new decimal[] { 0.004m, 0.394m, 0.27m, 0.676m }};
		static Glyph Lslash = new Glyph { Char = (char)321, w0 = 0.611F, IsWordSpace = false, CodePoint = 232, Name = "Lslash", Undefined = false, BBox = new decimal[] { 0.012m, 0m, 0.598m, 0.662m }, Kernings = new Dictionary<char,float> {  ['\u0054'] = 92f, ['\u0164'] = 92f, ['\u0162'] = 92f, ['\u0056'] = 100f, ['\u0057'] = 74f, ['\u0059'] = 100f, ['\u00DD'] = 100f, ['\u0178'] = 100f, ['\u2019'] = 92f, ['\u0079'] = 55f, ['\u00FD'] = 55f, ['\u00FF'] = 55f, } };
		static Glyph Oslash = new Glyph { Char = (char)216, w0 = 0.722F, IsWordSpace = false, CodePoint = 233, Name = "Oslash", Undefined = false, BBox = new decimal[] { 0.034m, -0.08m, 0.688m, 0.734m }, Kernings = new Dictionary<char,float> {  ['\u0041'] = 35f, ['\u00C1'] = 35f, ['\u0102'] = 35f, ['\u00C2'] = 35f, ['\u00C4'] = 35f, ['\u00C0'] = 35f, ['\u0100'] = 35f, ['\u0104'] = 35f, ['\u00C5'] = 35f, ['\u00C3'] = 35f, ['\u0054'] = 40f, ['\u0164'] = 40f, ['\u0162'] = 40f, ['\u0056'] = 50f, ['\u0057'] = 35f, ['\u0058'] = 40f, ['\u0059'] = 50f, ['\u00DD'] = 50f, ['\u0178'] = 50f, } };
		static Glyph OE = new Glyph { Char = (char)338, w0 = 0.889F, IsWordSpace = false, CodePoint = 234, Name = "OE", Undefined = false, BBox = new decimal[] { 0.03m, -0.006m, 0.885m, 0.668m }};
		static Glyph ordmasculine = new Glyph { Char = (char)186, w0 = 0.31F, IsWordSpace = false, CodePoint = 235, Name = "ordmasculine", Undefined = false, BBox = new decimal[] { 0.006m, 0.394m, 0.304m, 0.676m }};
		static Glyph ae = new Glyph { Char = (char)230, w0 = 0.667F, IsWordSpace = false, CodePoint = 241, Name = "ae", Undefined = false, BBox = new decimal[] { 0.038m, -0.01m, 0.632m, 0.46m }};
		static Glyph dotlessi = new Glyph { Char = (char)305, w0 = 0.278F, IsWordSpace = false, CodePoint = 245, Name = "dotlessi", Undefined = false, BBox = new decimal[] { 0.016m, 0m, 0.253m, 0.46m }};
		static Glyph lslash = new Glyph { Char = (char)322, w0 = 0.278F, IsWordSpace = false, CodePoint = 248, Name = "lslash", Undefined = false, BBox = new decimal[] { 0.019m, 0m, 0.259m, 0.683m }, Kernings = new Dictionary<char,float> {  ['\u0077'] = 10f, } };
		static Glyph oslash = new Glyph { Char = (char)248, w0 = 0.5F, IsWordSpace = false, CodePoint = 249, Name = "oslash", Undefined = false, BBox = new decimal[] { 0.029m, -0.112m, 0.47m, 0.551m }, Kernings = new Dictionary<char,float> {  ['\u0076'] = 15f, ['\u0077'] = 25f, ['\u0079'] = 10f, ['\u00FD'] = 10f, ['\u00FF'] = 10f, } };
		static Glyph oe = new Glyph { Char = (char)339, w0 = 0.722F, IsWordSpace = false, CodePoint = 250, Name = "oe", Undefined = false, BBox = new decimal[] { 0.03m, -0.01m, 0.69m, 0.46m }};
		static Glyph germandbls = new Glyph { Char = (char)223, w0 = 0.5F, IsWordSpace = false, CodePoint = 251, Name = "germandbls", Undefined = false, BBox = new decimal[] { 0.012m, -0.009m, 0.468m, 0.683m }};
		static Glyph Idieresis = new Glyph { Char = (char)207, w0 = 0.333F, IsWordSpace = false, Name = "Idieresis", Undefined = false, BBox = new decimal[] { 0.018m, 0m, 0.315m, 0.835m }};
		static Glyph eacute = new Glyph { Char = (char)233, w0 = 0.444F, IsWordSpace = false, Name = "eacute", Undefined = false, BBox = new decimal[] { 0.025m, -0.01m, 0.424m, 0.678m }, Kernings = new Dictionary<char,float> {  ['\u0067'] = 15f, ['\u011F'] = 15f, ['\u0123'] = 15f, ['\u0076'] = 25f, ['\u0077'] = 25f, ['\u0078'] = 15f, ['\u0079'] = 15f, ['\u00FD'] = 15f, ['\u00FF'] = 15f, } };
		static Glyph abreve = new Glyph { Char = (char)259, w0 = 0.444F, IsWordSpace = false, Name = "abreve", Undefined = false, BBox = new decimal[] { 0.037m, -0.01m, 0.442m, 0.664m }, Kernings = new Dictionary<char,float> {  ['\u0076'] = 20f, ['\u0077'] = 15f, } };
		static Glyph uhungarumlaut = new Glyph { Char = (char)369, w0 = 0.5F, IsWordSpace = false, Name = "uhungarumlaut", Undefined = false, BBox = new decimal[] { 0.009m, -0.01m, 0.501m, 0.678m }};
		static Glyph ecaron = new Glyph { Char = (char)283, w0 = 0.444F, IsWordSpace = false, Name = "ecaron", Undefined = false, BBox = new decimal[] { 0.025m, -0.01m, 0.424m, 0.674m }, Kernings = new Dictionary<char,float> {  ['\u0067'] = 15f, ['\u011F'] = 15f, ['\u0123'] = 15f, ['\u0076'] = 25f, ['\u0077'] = 25f, ['\u0078'] = 15f, ['\u0079'] = 15f, ['\u00FD'] = 15f, ['\u00FF'] = 15f, } };
		static Glyph Ydieresis = new Glyph { Char = (char)376, w0 = 0.722F, IsWordSpace = false, Name = "Ydieresis", Undefined = false, BBox = new decimal[] { 0.022m, 0m, 0.703m, 0.835m }, Kernings = new Dictionary<char,float> {  ['\u0041'] = 120f, ['\u00C1'] = 120f, ['\u0102'] = 120f, ['\u00C2'] = 120f, ['\u00C4'] = 120f, ['\u00C0'] = 120f, ['\u0100'] = 120f, ['\u0104'] = 120f, ['\u00C5'] = 120f, ['\u00C3'] = 120f, ['\u004F'] = 30f, ['\u00D3'] = 30f, ['\u00D4'] = 30f, ['\u00D6'] = 30f, ['\u00D2'] = 30f, ['\u0150'] = 30f, ['\u014C'] = 30f, ['\u00D8'] = 30f, ['\u00D5'] = 30f, ['\u0061'] = 100f, ['\u00E1'] = 100f, ['\u0103'] = 100f, ['\u00E2'] = 100f, ['\u00E4'] = 60f, ['\u00E0'] = 60f, ['\u0101'] = 60f, ['\u0105'] = 100f, ['\u00E5'] = 100f, ['\u00E3'] = 100f, ['\u003A'] = 92f, ['\u002C'] = 129f, ['\u0065'] = 100f, ['\u00E9'] = 100f, ['\u011B'] = 100f, ['\u00EA'] = 100f, ['\u00EB'] = 60f, ['\u0117'] = 100f, ['\u00E8'] = 60f, ['\u0113'] = 60f, ['\u0119'] = 100f, ['\u002D'] = 111f, ['\u0069'] = 55f, ['\u00ED'] = 55f, ['\u012F'] = 55f, ['\u006F'] = 110f, ['\u00F3'] = 110f, ['\u00F4'] = 110f, ['\u00F6'] = 70f, ['\u00F2'] = 70f, ['\u0151'] = 110f, ['\u014D'] = 70f, ['\u00F8'] = 110f, ['\u00F5'] = 70f, ['\u002E'] = 129f, ['\u003B'] = 92f, ['\u0075'] = 111f, ['\u00FA'] = 111f, ['\u00FB'] = 111f, ['\u00FC'] = 71f, ['\u00F9'] = 71f, ['\u0171'] = 111f, ['\u016B'] = 71f, ['\u0173'] = 111f, ['\u016F'] = 111f, } };
		static Glyph divide = new Glyph { Char = (char)247, w0 = 0.564F, IsWordSpace = false, Name = "divide", Undefined = false, BBox = new decimal[] { 0.03m, -0.01m, 0.534m, 0.516m }};
		static Glyph Yacute = new Glyph { Char = (char)221, w0 = 0.722F, IsWordSpace = false, Name = "Yacute", Undefined = false, BBox = new decimal[] { 0.022m, 0m, 0.703m, 0.89m }, Kernings = new Dictionary<char,float> {  ['\u0041'] = 120f, ['\u00C1'] = 120f, ['\u0102'] = 120f, ['\u00C2'] = 120f, ['\u00C4'] = 120f, ['\u00C0'] = 120f, ['\u0100'] = 120f, ['\u0104'] = 120f, ['\u00C5'] = 120f, ['\u00C3'] = 120f, ['\u004F'] = 30f, ['\u00D3'] = 30f, ['\u00D4'] = 30f, ['\u00D6'] = 30f, ['\u00D2'] = 30f, ['\u0150'] = 30f, ['\u014C'] = 30f, ['\u00D8'] = 30f, ['\u00D5'] = 30f, ['\u0061'] = 100f, ['\u00E1'] = 100f, ['\u0103'] = 100f, ['\u00E2'] = 100f, ['\u00E4'] = 60f, ['\u00E0'] = 60f, ['\u0101'] = 60f, ['\u0105'] = 100f, ['\u00E5'] = 100f, ['\u00E3'] = 60f, ['\u003A'] = 92f, ['\u002C'] = 129f, ['\u0065'] = 100f, ['\u00E9'] = 100f, ['\u011B'] = 100f, ['\u00EA'] = 100f, ['\u00EB'] = 60f, ['\u0117'] = 100f, ['\u00E8'] = 60f, ['\u0113'] = 60f, ['\u0119'] = 100f, ['\u002D'] = 111f, ['\u0069'] = 55f, ['\u00ED'] = 55f, ['\u012F'] = 55f, ['\u006F'] = 110f, ['\u00F3'] = 110f, ['\u00F4'] = 110f, ['\u00F6'] = 70f, ['\u00F2'] = 70f, ['\u0151'] = 110f, ['\u014D'] = 70f, ['\u00F8'] = 110f, ['\u00F5'] = 70f, ['\u002E'] = 129f, ['\u003B'] = 92f, ['\u0075'] = 111f, ['\u00FA'] = 111f, ['\u00FB'] = 111f, ['\u00FC'] = 71f, ['\u00F9'] = 71f, ['\u0171'] = 111f, ['\u016B'] = 71f, ['\u0173'] = 111f, ['\u016F'] = 111f, } };
		static Glyph Acircumflex = new Glyph { Char = (char)194, w0 = 0.722F, IsWordSpace = false, Name = "Acircumflex", Undefined = false, BBox = new decimal[] { 0.015m, 0m, 0.706m, 0.886m }, Kernings = new Dictionary<char,float> {  ['\u0043'] = 40f, ['\u0106'] = 40f, ['\u010C'] = 40f, ['\u00C7'] = 40f, ['\u0047'] = 40f, ['\u011E'] = 40f, ['\u0122'] = 40f, ['\u004F'] = 55f, ['\u00D3'] = 55f, ['\u00D4'] = 55f, ['\u00D6'] = 55f, ['\u00D2'] = 55f, ['\u0150'] = 55f, ['\u014C'] = 55f, ['\u00D8'] = 55f, ['\u00D5'] = 55f, ['\u0051'] = 55f, ['\u0054'] = 111f, ['\u0164'] = 111f, ['\u0162'] = 111f, ['\u0055'] = 55f, ['\u00DA'] = 55f, ['\u00DB'] = 55f, ['\u00DC'] = 55f, ['\u00D9'] = 55f, ['\u0170'] = 55f, ['\u016A'] = 55f, ['\u0172'] = 55f, ['\u016E'] = 55f, ['\u0056'] = 135f, ['\u0057'] = 90f, ['\u0059'] = 105f, ['\u00DD'] = 105f, ['\u0178'] = 105f, ['\u2019'] = 111f, ['\u0076'] = 74f, ['\u0077'] = 92f, ['\u0079'] = 92f, ['\u00FD'] = 92f, ['\u00FF'] = 92f, } };
		static Glyph aacute = new Glyph { Char = (char)225, w0 = 0.444F, IsWordSpace = false, Name = "aacute", Undefined = false, BBox = new decimal[] { 0.037m, -0.01m, 0.442m, 0.678m }, Kernings = new Dictionary<char,float> {  ['\u0076'] = 20f, ['\u0077'] = 15f, } };
		static Glyph Ucircumflex = new Glyph { Char = (char)219, w0 = 0.722F, IsWordSpace = false, Name = "Ucircumflex", Undefined = false, BBox = new decimal[] { 0.014m, -0.014m, 0.705m, 0.886m }, Kernings = new Dictionary<char,float> {  ['\u0041'] = 40f, ['\u00C1'] = 40f, ['\u0102'] = 40f, ['\u00C2'] = 40f, ['\u00C4'] = 40f, ['\u00C0'] = 40f, ['\u0100'] = 40f, ['\u0104'] = 40f, ['\u00C5'] = 40f, ['\u00C3'] = 40f, } };
		static Glyph yacute = new Glyph { Char = (char)253, w0 = 0.5F, IsWordSpace = false, Name = "yacute", Undefined = false, BBox = new decimal[] { 0.014m, -0.218m, 0.475m, 0.678m }, Kernings = new Dictionary<char,float> {  ['\u002C'] = 65f, ['\u002E'] = 65f, } };
		static Glyph scommaaccent = new Glyph { Char = (char)537, w0 = 0.389F, IsWordSpace = false, Name = "scommaaccent", Undefined = false, BBox = new decimal[] { 0.051m, -0.218m, 0.348m, 0.46m }};
		static Glyph ecircumflex = new Glyph { Char = (char)234, w0 = 0.444F, IsWordSpace = false, Name = "ecircumflex", Undefined = false, BBox = new decimal[] { 0.025m, -0.01m, 0.424m, 0.674m }, Kernings = new Dictionary<char,float> {  ['\u0067'] = 15f, ['\u011F'] = 15f, ['\u0123'] = 15f, ['\u0076'] = 25f, ['\u0077'] = 25f, ['\u0078'] = 15f, ['\u0079'] = 15f, ['\u00FD'] = 15f, ['\u00FF'] = 15f, } };
		static Glyph Uring = new Glyph { Char = (char)366, w0 = 0.722F, IsWordSpace = false, Name = "Uring", Undefined = false, BBox = new decimal[] { 0.014m, -0.014m, 0.705m, 0.898m }, Kernings = new Dictionary<char,float> {  ['\u0041'] = 40f, ['\u00C1'] = 40f, ['\u0102'] = 40f, ['\u00C2'] = 40f, ['\u00C4'] = 40f, ['\u00C0'] = 40f, ['\u0100'] = 40f, ['\u0104'] = 40f, ['\u00C5'] = 40f, ['\u00C3'] = 40f, } };
		static Glyph Udieresis = new Glyph { Char = (char)220, w0 = 0.722F, IsWordSpace = false, Name = "Udieresis", Undefined = false, BBox = new decimal[] { 0.014m, -0.014m, 0.705m, 0.835m }, Kernings = new Dictionary<char,float> {  ['\u0041'] = 40f, ['\u00C1'] = 40f, ['\u0102'] = 40f, ['\u00C2'] = 40f, ['\u00C4'] = 40f, ['\u00C0'] = 40f, ['\u0100'] = 40f, ['\u0104'] = 40f, ['\u00C5'] = 40f, ['\u00C3'] = 40f, } };
		static Glyph aogonek = new Glyph { Char = (char)261, w0 = 0.444F, IsWordSpace = false, Name = "aogonek", Undefined = false, BBox = new decimal[] { 0.037m, -0.165m, 0.469m, 0.46m }, Kernings = new Dictionary<char,float> {  ['\u0076'] = 20f, ['\u0077'] = 15f, } };
		static Glyph Uacute = new Glyph { Char = (char)218, w0 = 0.722F, IsWordSpace = false, Name = "Uacute", Undefined = false, BBox = new decimal[] { 0.014m, -0.014m, 0.705m, 0.89m }, Kernings = new Dictionary<char,float> {  ['\u0041'] = 40f, ['\u00C1'] = 40f, ['\u0102'] = 40f, ['\u00C2'] = 40f, ['\u00C4'] = 40f, ['\u00C0'] = 40f, ['\u0100'] = 40f, ['\u0104'] = 40f, ['\u00C5'] = 40f, ['\u00C3'] = 40f, } };
		static Glyph uogonek = new Glyph { Char = (char)371, w0 = 0.5F, IsWordSpace = false, Name = "uogonek", Undefined = false, BBox = new decimal[] { 0.009m, -0.155m, 0.487m, 0.45m }};
		static Glyph Edieresis = new Glyph { Char = (char)203, w0 = 0.611F, IsWordSpace = false, Name = "Edieresis", Undefined = false, BBox = new decimal[] { 0.012m, 0m, 0.597m, 0.835m }};
		static Glyph Dcroat = new Glyph { Char = (char)272, w0 = 0.722F, IsWordSpace = false, Name = "Dcroat", Undefined = false, BBox = new decimal[] { 0.016m, 0m, 0.685m, 0.662m }, Kernings = new Dictionary<char,float> {  ['\u0041'] = 40f, ['\u00C1'] = 40f, ['\u0102'] = 40f, ['\u00C2'] = 40f, ['\u00C4'] = 40f, ['\u00C0'] = 40f, ['\u0100'] = 40f, ['\u0104'] = 40f, ['\u00C5'] = 40f, ['\u00C3'] = 40f, ['\u0056'] = 40f, ['\u0057'] = 30f, ['\u0059'] = 55f, ['\u00DD'] = 55f, ['\u0178'] = 55f, } };
		static Glyph commaaccent = new Glyph { Char = (char)63171, w0 = 0.25F, IsWordSpace = false, Name = "commaaccent", Undefined = false, BBox = new decimal[] { 0.059m, -0.218m, 0.184m, -0.05m }};
		static Glyph copyright = new Glyph { Char = (char)169, w0 = 0.76F, IsWordSpace = false, Name = "copyright", Undefined = false, BBox = new decimal[] { 0.038m, -0.014m, 0.722m, 0.676m }};
		static Glyph Emacron = new Glyph { Char = (char)274, w0 = 0.611F, IsWordSpace = false, Name = "Emacron", Undefined = false, BBox = new decimal[] { 0.012m, 0m, 0.597m, 0.813m }};
		static Glyph ccaron = new Glyph { Char = (char)269, w0 = 0.444F, IsWordSpace = false, Name = "ccaron", Undefined = false, BBox = new decimal[] { 0.025m, -0.01m, 0.412m, 0.674m }, Kernings = new Dictionary<char,float> {  ['\u0079'] = 15f, ['\u00FD'] = 15f, ['\u00FF'] = 15f, } };
		static Glyph aring = new Glyph { Char = (char)229, w0 = 0.444F, IsWordSpace = false, Name = "aring", Undefined = false, BBox = new decimal[] { 0.037m, -0.01m, 0.442m, 0.711m }, Kernings = new Dictionary<char,float> {  ['\u0076'] = 20f, ['\u0077'] = 15f, } };
		static Glyph Ncommaaccent = new Glyph { Char = (char)325, w0 = 0.722F, IsWordSpace = false, Name = "Ncommaaccent", Undefined = false, BBox = new decimal[] { 0.012m, -0.198m, 0.707m, 0.662m }, Kernings = new Dictionary<char,float> {  ['\u0041'] = 35f, ['\u00C1'] = 35f, ['\u0102'] = 35f, ['\u00C2'] = 35f, ['\u00C4'] = 35f, ['\u00C0'] = 35f, ['\u0100'] = 35f, ['\u0104'] = 35f, ['\u00C5'] = 35f, ['\u00C3'] = 35f, } };
		static Glyph lacute = new Glyph { Char = (char)314, w0 = 0.278F, IsWordSpace = false, Name = "lacute", Undefined = false, BBox = new decimal[] { 0.019m, 0m, 0.29m, 0.89m }, Kernings = new Dictionary<char,float> {  ['\u0077'] = 10f, } };
		static Glyph agrave = new Glyph { Char = (char)224, w0 = 0.444F, IsWordSpace = false, Name = "agrave", Undefined = false, BBox = new decimal[] { 0.037m, -0.01m, 0.442m, 0.678m }, Kernings = new Dictionary<char,float> {  ['\u0076'] = 20f, ['\u0077'] = 15f, } };
		static Glyph Tcommaaccent = new Glyph { Char = (char)354, w0 = 0.611F, IsWordSpace = false, Name = "Tcommaaccent", Undefined = false, BBox = new decimal[] { 0.017m, -0.218m, 0.593m, 0.662m }, Kernings = new Dictionary<char,float> {  ['\u0041'] = 93f, ['\u00C1'] = 93f, ['\u0102'] = 93f, ['\u00C2'] = 93f, ['\u00C4'] = 93f, ['\u00C0'] = 93f, ['\u0100'] = 93f, ['\u0104'] = 93f, ['\u00C5'] = 93f, ['\u00C3'] = 93f, ['\u004F'] = 18f, ['\u00D3'] = 18f, ['\u00D4'] = 18f, ['\u00D6'] = 18f, ['\u00D2'] = 18f, ['\u0150'] = 18f, ['\u014C'] = 18f, ['\u00D8'] = 18f, ['\u00D5'] = 18f, ['\u0061'] = 80f, ['\u00E1'] = 80f, ['\u0103'] = 80f, ['\u00E2'] = 80f, ['\u00E4'] = 40f, ['\u00E0'] = 40f, ['\u0101'] = 40f, ['\u0105'] = 80f, ['\u00E5'] = 80f, ['\u00E3'] = 40f, ['\u003A'] = 50f, ['\u002C'] = 74f, ['\u0065'] = 70f, ['\u00E9'] = 70f, ['\u011B'] = 70f, ['\u00EA'] = 30f, ['\u00EB'] = 30f, ['\u0117'] = 70f, ['\u00E8'] = 30f, ['\u0113'] = 70f, ['\u0119'] = 70f, ['\u002D'] = 92f, ['\u0069'] = 35f, ['\u00ED'] = 35f, ['\u012F'] = 35f, ['\u006F'] = 80f, ['\u00F3'] = 80f, ['\u00F4'] = 80f, ['\u00F6'] = 80f, ['\u00F2'] = 80f, ['\u0151'] = 80f, ['\u014D'] = 80f, ['\u00F8'] = 80f, ['\u00F5'] = 80f, ['\u002E'] = 74f, ['\u0072'] = 35f, ['\u0155'] = 35f, ['\u0159'] = 35f, ['\u0157'] = 35f, ['\u003B'] = 55f, ['\u0075'] = 45f, ['\u00FA'] = 45f, ['\u00FB'] = 45f, ['\u00FC'] = 45f, ['\u00F9'] = 45f, ['\u0171'] = 45f, ['\u016B'] = 45f, ['\u0173'] = 45f, ['\u016F'] = 45f, ['\u0077'] = 80f, ['\u0079'] = 80f, ['\u00FD'] = 80f, ['\u00FF'] = 80f, } };
		static Glyph Cacute = new Glyph { Char = (char)262, w0 = 0.667F, IsWordSpace = false, Name = "Cacute", Undefined = false, BBox = new decimal[] { 0.028m, -0.014m, 0.633m, 0.89m }};
		static Glyph atilde = new Glyph { Char = (char)227, w0 = 0.444F, IsWordSpace = false, Name = "atilde", Undefined = false, BBox = new decimal[] { 0.037m, -0.01m, 0.442m, 0.638m }, Kernings = new Dictionary<char,float> {  ['\u0076'] = 20f, ['\u0077'] = 15f, } };
		static Glyph Edotaccent = new Glyph { Char = (char)278, w0 = 0.611F, IsWordSpace = false, Name = "Edotaccent", Undefined = false, BBox = new decimal[] { 0.012m, 0m, 0.597m, 0.835m }};
		static Glyph scaron = new Glyph { Char = (char)353, w0 = 0.389F, IsWordSpace = false, Name = "scaron", Undefined = false, BBox = new decimal[] { 0.039m, -0.01m, 0.35m, 0.674m }};
		static Glyph scedilla = new Glyph { Char = (char)351, w0 = 0.389F, IsWordSpace = false, Name = "scedilla", Undefined = false, BBox = new decimal[] { 0.051m, -0.215m, 0.348m, 0.46m }};
		static Glyph iacute = new Glyph { Char = (char)237, w0 = 0.278F, IsWordSpace = false, Name = "iacute", Undefined = false, BBox = new decimal[] { 0.016m, 0m, 0.29m, 0.678m }, Kernings = new Dictionary<char,float> {  ['\u0076'] = 25f, } };
		static Glyph lozenge = new Glyph { Char = (char)9674, w0 = 0.471F, IsWordSpace = false, Name = "lozenge", Undefined = false, BBox = new decimal[] { 0.013m, 0m, 0.459m, 0.724m }};
		static Glyph Rcaron = new Glyph { Char = (char)344, w0 = 0.667F, IsWordSpace = false, Name = "Rcaron", Undefined = false, BBox = new decimal[] { 0.017m, 0m, 0.659m, 0.886m }, Kernings = new Dictionary<char,float> {  ['\u004F'] = 40f, ['\u00D3'] = 40f, ['\u00D4'] = 40f, ['\u00D6'] = 40f, ['\u00D2'] = 40f, ['\u0150'] = 40f, ['\u014C'] = 40f, ['\u00D8'] = 40f, ['\u00D5'] = 40f, ['\u0054'] = 60f, ['\u0164'] = 60f, ['\u0162'] = 60f, ['\u0055'] = 40f, ['\u00DA'] = 40f, ['\u00DB'] = 40f, ['\u00DC'] = 40f, ['\u00D9'] = 40f, ['\u0170'] = 40f, ['\u016A'] = 40f, ['\u0172'] = 40f, ['\u016E'] = 40f, ['\u0056'] = 80f, ['\u0057'] = 55f, ['\u0059'] = 65f, ['\u00DD'] = 65f, ['\u0178'] = 65f, } };
		static Glyph Gcommaaccent = new Glyph { Char = (char)290, w0 = 0.722F, IsWordSpace = false, Name = "Gcommaaccent", Undefined = false, BBox = new decimal[] { 0.032m, -0.218m, 0.709m, 0.676m }};
		static Glyph ucircumflex = new Glyph { Char = (char)251, w0 = 0.5F, IsWordSpace = false, Name = "ucircumflex", Undefined = false, BBox = new decimal[] { 0.009m, -0.01m, 0.479m, 0.674m }};
		static Glyph acircumflex = new Glyph { Char = (char)226, w0 = 0.444F, IsWordSpace = false, Name = "acircumflex", Undefined = false, BBox = new decimal[] { 0.037m, -0.01m, 0.442m, 0.674m }, Kernings = new Dictionary<char,float> {  ['\u0076'] = 20f, ['\u0077'] = 15f, } };
		static Glyph Amacron = new Glyph { Char = (char)256, w0 = 0.722F, IsWordSpace = false, Name = "Amacron", Undefined = false, BBox = new decimal[] { 0.015m, 0m, 0.706m, 0.813m }, Kernings = new Dictionary<char,float> {  ['\u0043'] = 40f, ['\u0106'] = 40f, ['\u010C'] = 40f, ['\u00C7'] = 40f, ['\u0047'] = 40f, ['\u011E'] = 40f, ['\u0122'] = 40f, ['\u004F'] = 55f, ['\u00D3'] = 55f, ['\u00D4'] = 55f, ['\u00D6'] = 55f, ['\u00D2'] = 55f, ['\u0150'] = 55f, ['\u014C'] = 55f, ['\u00D8'] = 55f, ['\u00D5'] = 55f, ['\u0051'] = 55f, ['\u0054'] = 111f, ['\u0164'] = 111f, ['\u0162'] = 111f, ['\u0055'] = 55f, ['\u00DA'] = 55f, ['\u00DB'] = 55f, ['\u00DC'] = 55f, ['\u00D9'] = 55f, ['\u0170'] = 55f, ['\u016A'] = 55f, ['\u0172'] = 55f, ['\u016E'] = 55f, ['\u0056'] = 135f, ['\u0057'] = 90f, ['\u0059'] = 105f, ['\u00DD'] = 105f, ['\u0178'] = 105f, ['\u2019'] = 111f, ['\u0076'] = 74f, ['\u0077'] = 92f, ['\u0079'] = 92f, ['\u00FD'] = 92f, ['\u00FF'] = 92f, } };
		static Glyph rcaron = new Glyph { Char = (char)345, w0 = 0.333F, IsWordSpace = false, Name = "rcaron", Undefined = false, BBox = new decimal[] { 0.005m, 0m, 0.335m, 0.674m }, Kernings = new Dictionary<char,float> {  ['\u002C'] = 40f, ['\u0067'] = 18f, ['\u011F'] = 18f, ['\u0123'] = 18f, ['\u002D'] = 20f, ['\u002E'] = 55f, } };
		static Glyph ccedilla = new Glyph { Char = (char)231, w0 = 0.444F, IsWordSpace = false, Name = "ccedilla", Undefined = false, BBox = new decimal[] { 0.025m, -0.215m, 0.412m, 0.46m }, Kernings = new Dictionary<char,float> {  ['\u0079'] = 15f, ['\u00FD'] = 15f, ['\u00FF'] = 15f, } };
		static Glyph Zdotaccent = new Glyph { Char = (char)379, w0 = 0.611F, IsWordSpace = false, Name = "Zdotaccent", Undefined = false, BBox = new decimal[] { 0.009m, 0m, 0.597m, 0.835m }};
		static Glyph Thorn = new Glyph { Char = (char)222, w0 = 0.556F, IsWordSpace = false, Name = "Thorn", Undefined = false, BBox = new decimal[] { 0.016m, 0m, 0.542m, 0.662m }};
		static Glyph Omacron = new Glyph { Char = (char)332, w0 = 0.722F, IsWordSpace = false, Name = "Omacron", Undefined = false, BBox = new decimal[] { 0.034m, -0.014m, 0.688m, 0.813m }, Kernings = new Dictionary<char,float> {  ['\u0041'] = 35f, ['\u00C1'] = 35f, ['\u0102'] = 35f, ['\u00C2'] = 35f, ['\u00C4'] = 35f, ['\u00C0'] = 35f, ['\u0100'] = 35f, ['\u0104'] = 35f, ['\u00C5'] = 35f, ['\u00C3'] = 35f, ['\u0054'] = 40f, ['\u0164'] = 40f, ['\u0162'] = 40f, ['\u0056'] = 50f, ['\u0057'] = 35f, ['\u0058'] = 40f, ['\u0059'] = 50f, ['\u00DD'] = 50f, ['\u0178'] = 50f, } };
		static Glyph Racute = new Glyph { Char = (char)340, w0 = 0.667F, IsWordSpace = false, Name = "Racute", Undefined = false, BBox = new decimal[] { 0.017m, 0m, 0.659m, 0.89m }, Kernings = new Dictionary<char,float> {  ['\u004F'] = 40f, ['\u00D3'] = 40f, ['\u00D4'] = 40f, ['\u00D6'] = 40f, ['\u00D2'] = 40f, ['\u0150'] = 40f, ['\u014C'] = 40f, ['\u00D8'] = 40f, ['\u00D5'] = 40f, ['\u0054'] = 60f, ['\u0164'] = 60f, ['\u0162'] = 60f, ['\u0055'] = 40f, ['\u00DA'] = 40f, ['\u00DB'] = 40f, ['\u00DC'] = 40f, ['\u00D9'] = 40f, ['\u0170'] = 40f, ['\u016A'] = 40f, ['\u0172'] = 40f, ['\u016E'] = 40f, ['\u0056'] = 80f, ['\u0057'] = 55f, ['\u0059'] = 65f, ['\u00DD'] = 65f, ['\u0178'] = 65f, } };
		static Glyph Sacute = new Glyph { Char = (char)346, w0 = 0.556F, IsWordSpace = false, Name = "Sacute", Undefined = false, BBox = new decimal[] { 0.042m, -0.014m, 0.491m, 0.89m }};
		static Glyph dcaron = new Glyph { Char = (char)271, w0 = 0.588F, IsWordSpace = false, Name = "dcaron", Undefined = false, BBox = new decimal[] { 0.027m, -0.01m, 0.589m, 0.695m }};
		static Glyph Umacron = new Glyph { Char = (char)362, w0 = 0.722F, IsWordSpace = false, Name = "Umacron", Undefined = false, BBox = new decimal[] { 0.014m, -0.014m, 0.705m, 0.813m }, Kernings = new Dictionary<char,float> {  ['\u0041'] = 40f, ['\u00C1'] = 40f, ['\u0102'] = 40f, ['\u00C2'] = 40f, ['\u00C4'] = 40f, ['\u00C0'] = 40f, ['\u0100'] = 40f, ['\u0104'] = 40f, ['\u00C5'] = 40f, ['\u00C3'] = 40f, } };
		static Glyph uring = new Glyph { Char = (char)367, w0 = 0.5F, IsWordSpace = false, Name = "uring", Undefined = false, BBox = new decimal[] { 0.009m, -0.01m, 0.479m, 0.711m }};
		static Glyph threesuperior = new Glyph { Char = (char)179, w0 = 0.3F, IsWordSpace = false, Name = "threesuperior", Undefined = false, BBox = new decimal[] { 0.015m, 0.262m, 0.291m, 0.676m }};
		static Glyph Ograve = new Glyph { Char = (char)210, w0 = 0.722F, IsWordSpace = false, Name = "Ograve", Undefined = false, BBox = new decimal[] { 0.034m, -0.014m, 0.688m, 0.89m }, Kernings = new Dictionary<char,float> {  ['\u0041'] = 35f, ['\u00C1'] = 35f, ['\u0102'] = 35f, ['\u00C2'] = 35f, ['\u00C4'] = 35f, ['\u00C0'] = 35f, ['\u0100'] = 35f, ['\u0104'] = 35f, ['\u00C5'] = 35f, ['\u00C3'] = 35f, ['\u0054'] = 40f, ['\u0164'] = 40f, ['\u0162'] = 40f, ['\u0056'] = 50f, ['\u0057'] = 35f, ['\u0058'] = 40f, ['\u0059'] = 50f, ['\u00DD'] = 50f, ['\u0178'] = 50f, } };
		static Glyph Agrave = new Glyph { Char = (char)192, w0 = 0.722F, IsWordSpace = false, Name = "Agrave", Undefined = false, BBox = new decimal[] { 0.015m, 0m, 0.706m, 0.89m }, Kernings = new Dictionary<char,float> {  ['\u0043'] = 40f, ['\u0106'] = 40f, ['\u010C'] = 40f, ['\u00C7'] = 40f, ['\u0047'] = 40f, ['\u011E'] = 40f, ['\u0122'] = 40f, ['\u004F'] = 55f, ['\u00D3'] = 55f, ['\u00D4'] = 55f, ['\u00D6'] = 55f, ['\u00D2'] = 55f, ['\u0150'] = 55f, ['\u014C'] = 55f, ['\u00D8'] = 55f, ['\u00D5'] = 55f, ['\u0051'] = 55f, ['\u0054'] = 111f, ['\u0164'] = 111f, ['\u0162'] = 111f, ['\u0055'] = 55f, ['\u00DA'] = 55f, ['\u00DB'] = 55f, ['\u00DC'] = 55f, ['\u00D9'] = 55f, ['\u0170'] = 55f, ['\u016A'] = 55f, ['\u0172'] = 55f, ['\u016E'] = 55f, ['\u0056'] = 135f, ['\u0057'] = 90f, ['\u0059'] = 105f, ['\u00DD'] = 105f, ['\u0178'] = 105f, ['\u2019'] = 111f, ['\u0076'] = 74f, ['\u0077'] = 92f, ['\u0079'] = 92f, ['\u00FD'] = 92f, ['\u00FF'] = 92f, } };
		static Glyph Abreve = new Glyph { Char = (char)258, w0 = 0.722F, IsWordSpace = false, Name = "Abreve", Undefined = false, BBox = new decimal[] { 0.015m, 0m, 0.706m, 0.876m }, Kernings = new Dictionary<char,float> {  ['\u0043'] = 40f, ['\u0106'] = 40f, ['\u010C'] = 40f, ['\u00C7'] = 40f, ['\u0047'] = 40f, ['\u011E'] = 40f, ['\u0122'] = 40f, ['\u004F'] = 55f, ['\u00D3'] = 55f, ['\u00D4'] = 55f, ['\u00D6'] = 55f, ['\u00D2'] = 55f, ['\u0150'] = 55f, ['\u014C'] = 55f, ['\u00D8'] = 55f, ['\u00D5'] = 55f, ['\u0051'] = 55f, ['\u0054'] = 111f, ['\u0164'] = 111f, ['\u0162'] = 111f, ['\u0055'] = 55f, ['\u00DA'] = 55f, ['\u00DB'] = 55f, ['\u00DC'] = 55f, ['\u00D9'] = 55f, ['\u0170'] = 55f, ['\u016A'] = 55f, ['\u0172'] = 55f, ['\u016E'] = 55f, ['\u0056'] = 135f, ['\u0057'] = 90f, ['\u0059'] = 105f, ['\u00DD'] = 105f, ['\u0178'] = 105f, ['\u2019'] = 111f, ['\u0076'] = 74f, ['\u0077'] = 92f, ['\u0079'] = 92f, ['\u00FD'] = 92f, ['\u00FF'] = 92f, } };
		static Glyph multiply = new Glyph { Char = (char)215, w0 = 0.564F, IsWordSpace = false, Name = "multiply", Undefined = false, BBox = new decimal[] { 0.038m, 0.008m, 0.527m, 0.497m }};
		static Glyph uacute = new Glyph { Char = (char)250, w0 = 0.5F, IsWordSpace = false, Name = "uacute", Undefined = false, BBox = new decimal[] { 0.009m, -0.01m, 0.479m, 0.678m }};
		static Glyph Tcaron = new Glyph { Char = (char)356, w0 = 0.611F, IsWordSpace = false, Name = "Tcaron", Undefined = false, BBox = new decimal[] { 0.017m, 0m, 0.593m, 0.886m }, Kernings = new Dictionary<char,float> {  ['\u0041'] = 93f, ['\u00C1'] = 93f, ['\u0102'] = 93f, ['\u00C2'] = 93f, ['\u00C4'] = 93f, ['\u00C0'] = 93f, ['\u0100'] = 93f, ['\u0104'] = 93f, ['\u00C5'] = 93f, ['\u00C3'] = 93f, ['\u004F'] = 18f, ['\u00D3'] = 18f, ['\u00D4'] = 18f, ['\u00D6'] = 18f, ['\u00D2'] = 18f, ['\u0150'] = 18f, ['\u014C'] = 18f, ['\u00D8'] = 18f, ['\u00D5'] = 18f, ['\u0061'] = 80f, ['\u00E1'] = 80f, ['\u0103'] = 80f, ['\u00E2'] = 80f, ['\u00E4'] = 40f, ['\u00E0'] = 40f, ['\u0101'] = 40f, ['\u0105'] = 80f, ['\u00E5'] = 80f, ['\u00E3'] = 40f, ['\u003A'] = 50f, ['\u002C'] = 74f, ['\u0065'] = 70f, ['\u00E9'] = 70f, ['\u011B'] = 70f, ['\u00EA'] = 30f, ['\u00EB'] = 30f, ['\u0117'] = 70f, ['\u00E8'] = 70f, ['\u0113'] = 30f, ['\u0119'] = 70f, ['\u002D'] = 92f, ['\u0069'] = 35f, ['\u00ED'] = 35f, ['\u012F'] = 35f, ['\u006F'] = 80f, ['\u00F3'] = 80f, ['\u00F4'] = 80f, ['\u00F6'] = 80f, ['\u00F2'] = 80f, ['\u0151'] = 80f, ['\u014D'] = 80f, ['\u00F8'] = 80f, ['\u00F5'] = 80f, ['\u002E'] = 74f, ['\u0072'] = 35f, ['\u0155'] = 35f, ['\u0159'] = 35f, ['\u0157'] = 35f, ['\u003B'] = 55f, ['\u0075'] = 45f, ['\u00FA'] = 45f, ['\u00FB'] = 45f, ['\u00FC'] = 45f, ['\u00F9'] = 45f, ['\u0171'] = 45f, ['\u016B'] = 45f, ['\u0173'] = 45f, ['\u016F'] = 45f, ['\u0077'] = 80f, ['\u0079'] = 80f, ['\u00FD'] = 80f, ['\u00FF'] = 80f, } };
		static Glyph partialdiff = new Glyph { Char = (char)8706, w0 = 0.476F, IsWordSpace = false, Name = "partialdiff", Undefined = false, BBox = new decimal[] { 0.017m, -0.038m, 0.459m, 0.71m }};
		static Glyph ydieresis = new Glyph { Char = (char)255, w0 = 0.5F, IsWordSpace = false, Name = "ydieresis", Undefined = false, BBox = new decimal[] { 0.014m, -0.218m, 0.475m, 0.623m }, Kernings = new Dictionary<char,float> {  ['\u002C'] = 65f, ['\u002E'] = 65f, } };
		static Glyph Nacute = new Glyph { Char = (char)323, w0 = 0.722F, IsWordSpace = false, Name = "Nacute", Undefined = false, BBox = new decimal[] { 0.012m, -0.011m, 0.707m, 0.89m }, Kernings = new Dictionary<char,float> {  ['\u0041'] = 35f, ['\u00C1'] = 35f, ['\u0102'] = 35f, ['\u00C2'] = 35f, ['\u00C4'] = 35f, ['\u00C0'] = 35f, ['\u0100'] = 35f, ['\u0104'] = 35f, ['\u00C5'] = 35f, ['\u00C3'] = 35f, } };
		static Glyph icircumflex = new Glyph { Char = (char)238, w0 = 0.278F, IsWordSpace = false, Name = "icircumflex", Undefined = false, BBox = new decimal[] { -0.016m, 0m, 0.295m, 0.674m }, Kernings = new Dictionary<char,float> {  ['\u0076'] = 25f, } };
		static Glyph Ecircumflex = new Glyph { Char = (char)202, w0 = 0.611F, IsWordSpace = false, Name = "Ecircumflex", Undefined = false, BBox = new decimal[] { 0.012m, 0m, 0.597m, 0.886m }};
		static Glyph adieresis = new Glyph { Char = (char)228, w0 = 0.444F, IsWordSpace = false, Name = "adieresis", Undefined = false, BBox = new decimal[] { 0.037m, -0.01m, 0.442m, 0.623m }, Kernings = new Dictionary<char,float> {  ['\u0076'] = 20f, ['\u0077'] = 15f, } };
		static Glyph edieresis = new Glyph { Char = (char)235, w0 = 0.444F, IsWordSpace = false, Name = "edieresis", Undefined = false, BBox = new decimal[] { 0.025m, -0.01m, 0.424m, 0.623m }, Kernings = new Dictionary<char,float> {  ['\u0067'] = 15f, ['\u011F'] = 15f, ['\u0123'] = 15f, ['\u0076'] = 25f, ['\u0077'] = 25f, ['\u0078'] = 15f, ['\u0079'] = 15f, ['\u00FD'] = 15f, ['\u00FF'] = 15f, } };
		static Glyph cacute = new Glyph { Char = (char)263, w0 = 0.444F, IsWordSpace = false, Name = "cacute", Undefined = false, BBox = new decimal[] { 0.025m, -0.01m, 0.413m, 0.678m }, Kernings = new Dictionary<char,float> {  ['\u0079'] = 15f, ['\u00FD'] = 15f, ['\u00FF'] = 15f, } };
		static Glyph nacute = new Glyph { Char = (char)324, w0 = 0.5F, IsWordSpace = false, Name = "nacute", Undefined = false, BBox = new decimal[] { 0.016m, 0m, 0.485m, 0.678m }, Kernings = new Dictionary<char,float> {  ['\u0076'] = 40f, ['\u0079'] = 15f, ['\u00FD'] = 15f, ['\u00FF'] = 15f, } };
		static Glyph umacron = new Glyph { Char = (char)363, w0 = 0.5F, IsWordSpace = false, Name = "umacron", Undefined = false, BBox = new decimal[] { 0.009m, -0.01m, 0.479m, 0.601m }};
		static Glyph Ncaron = new Glyph { Char = (char)327, w0 = 0.722F, IsWordSpace = false, Name = "Ncaron", Undefined = false, BBox = new decimal[] { 0.012m, -0.011m, 0.707m, 0.886m }, Kernings = new Dictionary<char,float> {  ['\u0041'] = 35f, ['\u00C1'] = 35f, ['\u0102'] = 35f, ['\u00C2'] = 35f, ['\u00C4'] = 35f, ['\u00C0'] = 35f, ['\u0100'] = 35f, ['\u0104'] = 35f, ['\u00C5'] = 35f, ['\u00C3'] = 35f, } };
		static Glyph Iacute = new Glyph { Char = (char)205, w0 = 0.333F, IsWordSpace = false, Name = "Iacute", Undefined = false, BBox = new decimal[] { 0.018m, 0m, 0.317m, 0.89m }};
		static Glyph plusminus = new Glyph { Char = (char)177, w0 = 0.564F, IsWordSpace = false, Name = "plusminus", Undefined = false, BBox = new decimal[] { 0.03m, 0m, 0.534m, 0.506m }};
		static Glyph brokenbar = new Glyph { Char = (char)166, w0 = 0.2F, IsWordSpace = false, Name = "brokenbar", Undefined = false, BBox = new decimal[] { 0.067m, -0.143m, 0.133m, 0.707m }};
		static Glyph registered = new Glyph { Char = (char)174, w0 = 0.76F, IsWordSpace = false, Name = "registered", Undefined = false, BBox = new decimal[] { 0.038m, -0.014m, 0.722m, 0.676m }};
		static Glyph Gbreve = new Glyph { Char = (char)286, w0 = 0.722F, IsWordSpace = false, Name = "Gbreve", Undefined = false, BBox = new decimal[] { 0.032m, -0.014m, 0.709m, 0.876m }};
		static Glyph Idotaccent = new Glyph { Char = (char)304, w0 = 0.333F, IsWordSpace = false, Name = "Idotaccent", Undefined = false, BBox = new decimal[] { 0.018m, 0m, 0.315m, 0.835m }};
		static Glyph summation = new Glyph { Char = (char)8721, w0 = 0.6F, IsWordSpace = false, Name = "summation", Undefined = false, BBox = new decimal[] { 0.015m, -0.01m, 0.585m, 0.706m }};
		static Glyph Egrave = new Glyph { Char = (char)200, w0 = 0.611F, IsWordSpace = false, Name = "Egrave", Undefined = false, BBox = new decimal[] { 0.012m, 0m, 0.597m, 0.89m }};
		static Glyph racute = new Glyph { Char = (char)341, w0 = 0.333F, IsWordSpace = false, Name = "racute", Undefined = false, BBox = new decimal[] { 0.005m, 0m, 0.335m, 0.678m }, Kernings = new Dictionary<char,float> {  ['\u002C'] = 40f, ['\u0067'] = 18f, ['\u011F'] = 18f, ['\u0123'] = 18f, ['\u002D'] = 20f, ['\u002E'] = 55f, } };
		static Glyph omacron = new Glyph { Char = (char)333, w0 = 0.5F, IsWordSpace = false, Name = "omacron", Undefined = false, BBox = new decimal[] { 0.029m, -0.01m, 0.47m, 0.601m }, Kernings = new Dictionary<char,float> {  ['\u0076'] = 15f, ['\u0077'] = 25f, ['\u0079'] = 10f, ['\u00FD'] = 10f, ['\u00FF'] = 10f, } };
		static Glyph Zacute = new Glyph { Char = (char)377, w0 = 0.611F, IsWordSpace = false, Name = "Zacute", Undefined = false, BBox = new decimal[] { 0.009m, 0m, 0.597m, 0.89m }};
		static Glyph Zcaron = new Glyph { Char = (char)381, w0 = 0.611F, IsWordSpace = false, Name = "Zcaron", Undefined = false, BBox = new decimal[] { 0.009m, 0m, 0.597m, 0.886m }};
		static Glyph greaterequal = new Glyph { Char = (char)8805, w0 = 0.549F, IsWordSpace = false, Name = "greaterequal", Undefined = false, BBox = new decimal[] { 0.026m, 0m, 0.523m, 0.666m }};
		static Glyph Eth = new Glyph { Char = (char)208, w0 = 0.722F, IsWordSpace = false, Name = "Eth", Undefined = false, BBox = new decimal[] { 0.016m, 0m, 0.685m, 0.662m }};
		static Glyph Ccedilla = new Glyph { Char = (char)199, w0 = 0.667F, IsWordSpace = false, Name = "Ccedilla", Undefined = false, BBox = new decimal[] { 0.028m, -0.215m, 0.633m, 0.676m }};
		static Glyph lcommaaccent = new Glyph { Char = (char)316, w0 = 0.278F, IsWordSpace = false, Name = "lcommaaccent", Undefined = false, BBox = new decimal[] { 0.019m, -0.218m, 0.257m, 0.683m }, Kernings = new Dictionary<char,float> {  ['\u0077'] = 10f, } };
		static Glyph tcaron = new Glyph { Char = (char)357, w0 = 0.326F, IsWordSpace = false, Name = "tcaron", Undefined = false, BBox = new decimal[] { 0.013m, -0.01m, 0.318m, 0.722m }};
		static Glyph eogonek = new Glyph { Char = (char)281, w0 = 0.444F, IsWordSpace = false, Name = "eogonek", Undefined = false, BBox = new decimal[] { 0.025m, -0.165m, 0.424m, 0.46m }, Kernings = new Dictionary<char,float> {  ['\u0067'] = 15f, ['\u011F'] = 15f, ['\u0123'] = 15f, ['\u0076'] = 25f, ['\u0077'] = 25f, ['\u0078'] = 15f, ['\u0079'] = 15f, ['\u00FD'] = 15f, ['\u00FF'] = 15f, } };
		static Glyph Uogonek = new Glyph { Char = (char)370, w0 = 0.722F, IsWordSpace = false, Name = "Uogonek", Undefined = false, BBox = new decimal[] { 0.014m, -0.165m, 0.705m, 0.662m }, Kernings = new Dictionary<char,float> {  ['\u0041'] = 40f, ['\u00C1'] = 40f, ['\u0102'] = 40f, ['\u00C2'] = 40f, ['\u00C4'] = 40f, ['\u00C0'] = 40f, ['\u0100'] = 40f, ['\u0104'] = 40f, ['\u00C5'] = 40f, ['\u00C3'] = 40f, } };
		static Glyph Aacute = new Glyph { Char = (char)193, w0 = 0.722F, IsWordSpace = false, Name = "Aacute", Undefined = false, BBox = new decimal[] { 0.015m, 0m, 0.706m, 0.89m }, Kernings = new Dictionary<char,float> {  ['\u0043'] = 40f, ['\u0106'] = 40f, ['\u010C'] = 40f, ['\u00C7'] = 40f, ['\u0047'] = 40f, ['\u011E'] = 40f, ['\u0122'] = 40f, ['\u004F'] = 55f, ['\u00D3'] = 55f, ['\u00D4'] = 55f, ['\u00D6'] = 55f, ['\u00D2'] = 55f, ['\u0150'] = 55f, ['\u014C'] = 55f, ['\u00D8'] = 55f, ['\u00D5'] = 55f, ['\u0051'] = 55f, ['\u0054'] = 111f, ['\u0164'] = 111f, ['\u0162'] = 111f, ['\u0055'] = 55f, ['\u00DA'] = 55f, ['\u00DB'] = 55f, ['\u00DC'] = 55f, ['\u00D9'] = 55f, ['\u0170'] = 55f, ['\u016A'] = 55f, ['\u0172'] = 55f, ['\u016E'] = 55f, ['\u0056'] = 135f, ['\u0057'] = 90f, ['\u0059'] = 105f, ['\u00DD'] = 105f, ['\u0178'] = 105f, ['\u2019'] = 111f, ['\u0076'] = 74f, ['\u0077'] = 92f, ['\u0079'] = 92f, ['\u00FD'] = 92f, ['\u00FF'] = 92f, } };
		static Glyph Adieresis = new Glyph { Char = (char)196, w0 = 0.722F, IsWordSpace = false, Name = "Adieresis", Undefined = false, BBox = new decimal[] { 0.015m, 0m, 0.706m, 0.835m }, Kernings = new Dictionary<char,float> {  ['\u0043'] = 40f, ['\u0106'] = 40f, ['\u010C'] = 40f, ['\u00C7'] = 40f, ['\u0047'] = 40f, ['\u011E'] = 40f, ['\u0122'] = 40f, ['\u004F'] = 55f, ['\u00D3'] = 55f, ['\u00D4'] = 55f, ['\u00D6'] = 55f, ['\u00D2'] = 55f, ['\u0150'] = 55f, ['\u014C'] = 55f, ['\u00D8'] = 55f, ['\u00D5'] = 55f, ['\u0051'] = 55f, ['\u0054'] = 111f, ['\u0164'] = 111f, ['\u0162'] = 111f, ['\u0055'] = 55f, ['\u00DA'] = 55f, ['\u00DB'] = 55f, ['\u00DC'] = 55f, ['\u00D9'] = 55f, ['\u0170'] = 55f, ['\u016A'] = 55f, ['\u0172'] = 55f, ['\u016E'] = 55f, ['\u0056'] = 135f, ['\u0057'] = 90f, ['\u0059'] = 105f, ['\u00DD'] = 105f, ['\u0178'] = 105f, ['\u2019'] = 111f, ['\u0076'] = 74f, ['\u0077'] = 92f, ['\u0079'] = 92f, ['\u00FD'] = 92f, ['\u00FF'] = 92f, } };
		static Glyph egrave = new Glyph { Char = (char)232, w0 = 0.444F, IsWordSpace = false, Name = "egrave", Undefined = false, BBox = new decimal[] { 0.025m, -0.01m, 0.424m, 0.678m }, Kernings = new Dictionary<char,float> {  ['\u0067'] = 15f, ['\u011F'] = 15f, ['\u0123'] = 15f, ['\u0076'] = 25f, ['\u0077'] = 25f, ['\u0078'] = 15f, ['\u0079'] = 15f, ['\u00FD'] = 15f, ['\u00FF'] = 15f, } };
		static Glyph zacute = new Glyph { Char = (char)378, w0 = 0.444F, IsWordSpace = false, Name = "zacute", Undefined = false, BBox = new decimal[] { 0.027m, 0m, 0.418m, 0.678m }};
		static Glyph iogonek = new Glyph { Char = (char)303, w0 = 0.278F, IsWordSpace = false, Name = "iogonek", Undefined = false, BBox = new decimal[] { 0.016m, -0.165m, 0.265m, 0.683m }, Kernings = new Dictionary<char,float> {  ['\u0076'] = 25f, } };
		static Glyph Oacute = new Glyph { Char = (char)211, w0 = 0.722F, IsWordSpace = false, Name = "Oacute", Undefined = false, BBox = new decimal[] { 0.034m, -0.014m, 0.688m, 0.89m }, Kernings = new Dictionary<char,float> {  ['\u0041'] = 35f, ['\u00C1'] = 35f, ['\u0102'] = 35f, ['\u00C2'] = 35f, ['\u00C4'] = 35f, ['\u00C0'] = 35f, ['\u0100'] = 35f, ['\u0104'] = 35f, ['\u00C5'] = 35f, ['\u00C3'] = 35f, ['\u0054'] = 40f, ['\u0164'] = 40f, ['\u0162'] = 40f, ['\u0056'] = 50f, ['\u0057'] = 35f, ['\u0058'] = 40f, ['\u0059'] = 50f, ['\u00DD'] = 50f, ['\u0178'] = 50f, } };
		static Glyph oacute = new Glyph { Char = (char)243, w0 = 0.5F, IsWordSpace = false, Name = "oacute", Undefined = false, BBox = new decimal[] { 0.029m, -0.01m, 0.47m, 0.678m }, Kernings = new Dictionary<char,float> {  ['\u0076'] = 15f, ['\u0077'] = 25f, ['\u0079'] = 10f, ['\u00FD'] = 10f, ['\u00FF'] = 10f, } };
		static Glyph amacron = new Glyph { Char = (char)257, w0 = 0.444F, IsWordSpace = false, Name = "amacron", Undefined = false, BBox = new decimal[] { 0.037m, -0.01m, 0.442m, 0.601m }, Kernings = new Dictionary<char,float> {  ['\u0076'] = 20f, ['\u0077'] = 15f, } };
		static Glyph sacute = new Glyph { Char = (char)347, w0 = 0.389F, IsWordSpace = false, Name = "sacute", Undefined = false, BBox = new decimal[] { 0.051m, -0.01m, 0.348m, 0.678m }};
		static Glyph idieresis = new Glyph { Char = (char)239, w0 = 0.278F, IsWordSpace = false, Name = "idieresis", Undefined = false, BBox = new decimal[] { -0.009m, 0m, 0.288m, 0.623m }, Kernings = new Dictionary<char,float> {  ['\u0076'] = 25f, } };
		static Glyph Ocircumflex = new Glyph { Char = (char)212, w0 = 0.722F, IsWordSpace = false, Name = "Ocircumflex", Undefined = false, BBox = new decimal[] { 0.034m, -0.014m, 0.688m, 0.886m }, Kernings = new Dictionary<char,float> {  ['\u0041'] = 35f, ['\u00C1'] = 35f, ['\u0102'] = 35f, ['\u00C2'] = 35f, ['\u00C4'] = 35f, ['\u00C0'] = 35f, ['\u0100'] = 35f, ['\u0104'] = 35f, ['\u00C5'] = 35f, ['\u00C3'] = 35f, ['\u0054'] = 40f, ['\u0164'] = 40f, ['\u0162'] = 40f, ['\u0056'] = 50f, ['\u0057'] = 35f, ['\u0058'] = 40f, ['\u0059'] = 50f, ['\u00DD'] = 50f, ['\u0178'] = 50f, } };
		static Glyph Ugrave = new Glyph { Char = (char)217, w0 = 0.722F, IsWordSpace = false, Name = "Ugrave", Undefined = false, BBox = new decimal[] { 0.014m, -0.014m, 0.705m, 0.89m }, Kernings = new Dictionary<char,float> {  ['\u0041'] = 40f, ['\u00C1'] = 40f, ['\u0102'] = 40f, ['\u00C2'] = 40f, ['\u00C4'] = 40f, ['\u00C0'] = 40f, ['\u0100'] = 40f, ['\u0104'] = 40f, ['\u00C5'] = 40f, ['\u00C3'] = 40f, } };
		static Glyph Delta = new Glyph { Char = (char)8710, w0 = 0.612F, IsWordSpace = false, Name = "Delta", Undefined = false, BBox = new decimal[] { 0.006m, 0m, 0.608m, 0.688m }};
		static Glyph thorn = new Glyph { Char = (char)254, w0 = 0.5F, IsWordSpace = false, Name = "thorn", Undefined = false, BBox = new decimal[] { 0.005m, -0.217m, 0.47m, 0.683m }};
		static Glyph twosuperior = new Glyph { Char = (char)178, w0 = 0.3F, IsWordSpace = false, Name = "twosuperior", Undefined = false, BBox = new decimal[] { 0.001m, 0.27m, 0.296m, 0.676m }};
		static Glyph Odieresis = new Glyph { Char = (char)214, w0 = 0.722F, IsWordSpace = false, Name = "Odieresis", Undefined = false, BBox = new decimal[] { 0.034m, -0.014m, 0.688m, 0.835m }, Kernings = new Dictionary<char,float> {  ['\u0041'] = 35f, ['\u00C1'] = 35f, ['\u0102'] = 35f, ['\u00C2'] = 35f, ['\u00C4'] = 35f, ['\u00C0'] = 35f, ['\u0100'] = 35f, ['\u0104'] = 35f, ['\u00C5'] = 35f, ['\u00C3'] = 35f, ['\u0054'] = 40f, ['\u0164'] = 40f, ['\u0162'] = 40f, ['\u0056'] = 50f, ['\u0057'] = 35f, ['\u0058'] = 40f, ['\u0059'] = 50f, ['\u00DD'] = 50f, ['\u0178'] = 50f, } };
		static Glyph mu = new Glyph { Char = (char)181, w0 = 0.5F, IsWordSpace = false, Name = "mu", Undefined = false, BBox = new decimal[] { 0.036m, -0.218m, 0.512m, 0.45m }};
		static Glyph igrave = new Glyph { Char = (char)236, w0 = 0.278F, IsWordSpace = false, Name = "igrave", Undefined = false, BBox = new decimal[] { -0.008m, 0m, 0.253m, 0.678m }, Kernings = new Dictionary<char,float> {  ['\u0076'] = 25f, } };
		static Glyph ohungarumlaut = new Glyph { Char = (char)337, w0 = 0.5F, IsWordSpace = false, Name = "ohungarumlaut", Undefined = false, BBox = new decimal[] { 0.029m, -0.01m, 0.491m, 0.678m }, Kernings = new Dictionary<char,float> {  ['\u0076'] = 15f, ['\u0077'] = 25f, ['\u0079'] = 10f, ['\u00FD'] = 10f, ['\u00FF'] = 10f, } };
		static Glyph Eogonek = new Glyph { Char = (char)280, w0 = 0.611F, IsWordSpace = false, Name = "Eogonek", Undefined = false, BBox = new decimal[] { 0.012m, -0.165m, 0.597m, 0.662m }};
		static Glyph dcroat = new Glyph { Char = (char)273, w0 = 0.5F, IsWordSpace = false, Name = "dcroat", Undefined = false, BBox = new decimal[] { 0.027m, -0.01m, 0.5m, 0.683m }};
		static Glyph threequarters = new Glyph { Char = (char)190, w0 = 0.75F, IsWordSpace = false, Name = "threequarters", Undefined = false, BBox = new decimal[] { 0.015m, -0.014m, 0.718m, 0.676m }};
		static Glyph Scedilla = new Glyph { Char = (char)350, w0 = 0.556F, IsWordSpace = false, Name = "Scedilla", Undefined = false, BBox = new decimal[] { 0.042m, -0.215m, 0.491m, 0.676m }};
		static Glyph lcaron = new Glyph { Char = (char)318, w0 = 0.344F, IsWordSpace = false, Name = "lcaron", Undefined = false, BBox = new decimal[] { 0.019m, 0m, 0.347m, 0.695m }};
		static Glyph Kcommaaccent = new Glyph { Char = (char)310, w0 = 0.722F, IsWordSpace = false, Name = "Kcommaaccent", Undefined = false, BBox = new decimal[] { 0.034m, -0.198m, 0.723m, 0.662m }, Kernings = new Dictionary<char,float> {  ['\u004F'] = 30f, ['\u00D3'] = 30f, ['\u00D4'] = 30f, ['\u00D6'] = 30f, ['\u00D2'] = 30f, ['\u0150'] = 30f, ['\u014C'] = 30f, ['\u00D8'] = 30f, ['\u00D5'] = 30f, ['\u0065'] = 25f, ['\u00E9'] = 25f, ['\u011B'] = 25f, ['\u00EA'] = 25f, ['\u00EB'] = 25f, ['\u0117'] = 25f, ['\u00E8'] = 25f, ['\u0113'] = 25f, ['\u0119'] = 25f, ['\u006F'] = 35f, ['\u00F3'] = 35f, ['\u00F4'] = 35f, ['\u00F6'] = 35f, ['\u00F2'] = 35f, ['\u0151'] = 35f, ['\u014D'] = 35f, ['\u00F8'] = 35f, ['\u00F5'] = 35f, ['\u0075'] = 15f, ['\u00FA'] = 15f, ['\u00FB'] = 15f, ['\u00FC'] = 15f, ['\u00F9'] = 15f, ['\u0171'] = 15f, ['\u016B'] = 15f, ['\u0173'] = 15f, ['\u016F'] = 15f, ['\u0079'] = 25f, ['\u00FD'] = 25f, ['\u00FF'] = 25f, } };
		static Glyph Lacute = new Glyph { Char = (char)313, w0 = 0.611F, IsWordSpace = false, Name = "Lacute", Undefined = false, BBox = new decimal[] { 0.012m, 0m, 0.598m, 0.89m }, Kernings = new Dictionary<char,float> {  ['\u0054'] = 92f, ['\u0164'] = 92f, ['\u0162'] = 92f, ['\u0056'] = 100f, ['\u0057'] = 74f, ['\u0059'] = 100f, ['\u00DD'] = 100f, ['\u0178'] = 100f, ['\u2019'] = 92f, ['\u0079'] = 55f, ['\u00FD'] = 55f, ['\u00FF'] = 55f, } };
		static Glyph trademark = new Glyph { Char = (char)8482, w0 = 0.98F, IsWordSpace = false, Name = "trademark", Undefined = false, BBox = new decimal[] { 0.03m, 0.256m, 0.957m, 0.662m }};
		static Glyph edotaccent = new Glyph { Char = (char)279, w0 = 0.444F, IsWordSpace = false, Name = "edotaccent", Undefined = false, BBox = new decimal[] { 0.025m, -0.01m, 0.424m, 0.623m }, Kernings = new Dictionary<char,float> {  ['\u0067'] = 15f, ['\u011F'] = 15f, ['\u0123'] = 15f, ['\u0076'] = 25f, ['\u0077'] = 25f, ['\u0078'] = 15f, ['\u0079'] = 15f, ['\u00FD'] = 15f, ['\u00FF'] = 15f, } };
		static Glyph Igrave = new Glyph { Char = (char)204, w0 = 0.333F, IsWordSpace = false, Name = "Igrave", Undefined = false, BBox = new decimal[] { 0.018m, 0m, 0.315m, 0.89m }};
		static Glyph Imacron = new Glyph { Char = (char)298, w0 = 0.333F, IsWordSpace = false, Name = "Imacron", Undefined = false, BBox = new decimal[] { 0.011m, 0m, 0.322m, 0.813m }};
		static Glyph Lcaron = new Glyph { Char = (char)317, w0 = 0.611F, IsWordSpace = false, Name = "Lcaron", Undefined = false, BBox = new decimal[] { 0.012m, 0m, 0.598m, 0.676m }, Kernings = new Dictionary<char,float> {  ['\u2019'] = 92f, ['\u0079'] = 55f, ['\u00FD'] = 55f, ['\u00FF'] = 55f, } };
		static Glyph onehalf = new Glyph { Char = (char)189, w0 = 0.75F, IsWordSpace = false, Name = "onehalf", Undefined = false, BBox = new decimal[] { 0.031m, -0.014m, 0.746m, 0.676m }};
		static Glyph lessequal = new Glyph { Char = (char)8804, w0 = 0.549F, IsWordSpace = false, Name = "lessequal", Undefined = false, BBox = new decimal[] { 0.026m, 0m, 0.523m, 0.666m }};
		static Glyph ocircumflex = new Glyph { Char = (char)244, w0 = 0.5F, IsWordSpace = false, Name = "ocircumflex", Undefined = false, BBox = new decimal[] { 0.029m, -0.01m, 0.47m, 0.674m }, Kernings = new Dictionary<char,float> {  ['\u0076'] = 15f, ['\u0077'] = 25f, ['\u0079'] = 10f, ['\u00FD'] = 10f, ['\u00FF'] = 10f, } };
		static Glyph ntilde = new Glyph { Char = (char)241, w0 = 0.5F, IsWordSpace = false, Name = "ntilde", Undefined = false, BBox = new decimal[] { 0.016m, 0m, 0.485m, 0.638m }, Kernings = new Dictionary<char,float> {  ['\u0076'] = 40f, ['\u0079'] = 15f, ['\u00FD'] = 15f, ['\u00FF'] = 15f, } };
		static Glyph Uhungarumlaut = new Glyph { Char = (char)368, w0 = 0.722F, IsWordSpace = false, Name = "Uhungarumlaut", Undefined = false, BBox = new decimal[] { 0.014m, -0.014m, 0.705m, 0.89m }, Kernings = new Dictionary<char,float> {  ['\u0041'] = 40f, ['\u00C1'] = 40f, ['\u0102'] = 40f, ['\u00C2'] = 40f, ['\u00C4'] = 40f, ['\u00C0'] = 40f, ['\u0100'] = 40f, ['\u0104'] = 40f, ['\u00C5'] = 40f, ['\u00C3'] = 40f, } };
		static Glyph Eacute = new Glyph { Char = (char)201, w0 = 0.611F, IsWordSpace = false, Name = "Eacute", Undefined = false, BBox = new decimal[] { 0.012m, 0m, 0.597m, 0.89m }};
		static Glyph emacron = new Glyph { Char = (char)275, w0 = 0.444F, IsWordSpace = false, Name = "emacron", Undefined = false, BBox = new decimal[] { 0.025m, -0.01m, 0.424m, 0.601m }, Kernings = new Dictionary<char,float> {  ['\u0067'] = 15f, ['\u011F'] = 15f, ['\u0123'] = 15f, ['\u0076'] = 25f, ['\u0077'] = 25f, ['\u0078'] = 15f, ['\u0079'] = 15f, ['\u00FD'] = 15f, ['\u00FF'] = 15f, } };
		static Glyph gbreve = new Glyph { Char = (char)287, w0 = 0.5F, IsWordSpace = false, Name = "gbreve", Undefined = false, BBox = new decimal[] { 0.028m, -0.218m, 0.47m, 0.664m }, Kernings = new Dictionary<char,float> {  ['\u0061'] = 5f, ['\u00E1'] = 5f, ['\u0103'] = 5f, ['\u00E2'] = 5f, ['\u00E4'] = 5f, ['\u00E0'] = 5f, ['\u0101'] = 5f, ['\u0105'] = 5f, ['\u00E5'] = 5f, ['\u00E3'] = 5f, } };
		static Glyph onequarter = new Glyph { Char = (char)188, w0 = 0.75F, IsWordSpace = false, Name = "onequarter", Undefined = false, BBox = new decimal[] { 0.037m, -0.014m, 0.718m, 0.676m }};
		static Glyph Scaron = new Glyph { Char = (char)352, w0 = 0.556F, IsWordSpace = false, Name = "Scaron", Undefined = false, BBox = new decimal[] { 0.042m, -0.014m, 0.491m, 0.886m }};
		static Glyph Scommaaccent = new Glyph { Char = (char)536, w0 = 0.556F, IsWordSpace = false, Name = "Scommaaccent", Undefined = false, BBox = new decimal[] { 0.042m, -0.218m, 0.491m, 0.676m }};
		static Glyph Ohungarumlaut = new Glyph { Char = (char)336, w0 = 0.722F, IsWordSpace = false, Name = "Ohungarumlaut", Undefined = false, BBox = new decimal[] { 0.034m, -0.014m, 0.688m, 0.89m }, Kernings = new Dictionary<char,float> {  ['\u0041'] = 35f, ['\u00C1'] = 35f, ['\u0102'] = 35f, ['\u00C2'] = 35f, ['\u00C4'] = 35f, ['\u00C0'] = 35f, ['\u0100'] = 35f, ['\u0104'] = 35f, ['\u00C5'] = 35f, ['\u00C3'] = 35f, ['\u0054'] = 40f, ['\u0164'] = 40f, ['\u0162'] = 40f, ['\u0056'] = 50f, ['\u0057'] = 35f, ['\u0058'] = 40f, ['\u0059'] = 50f, ['\u00DD'] = 50f, ['\u0178'] = 50f, } };
		static Glyph degree = new Glyph { Char = (char)176, w0 = 0.4F, IsWordSpace = false, Name = "degree", Undefined = false, BBox = new decimal[] { 0.057m, 0.39m, 0.343m, 0.676m }};
		static Glyph ograve = new Glyph { Char = (char)242, w0 = 0.5F, IsWordSpace = false, Name = "ograve", Undefined = false, BBox = new decimal[] { 0.029m, -0.01m, 0.47m, 0.678m }, Kernings = new Dictionary<char,float> {  ['\u0076'] = 15f, ['\u0077'] = 25f, ['\u0079'] = 10f, ['\u00FD'] = 10f, ['\u00FF'] = 10f, } };
		static Glyph Ccaron = new Glyph { Char = (char)268, w0 = 0.667F, IsWordSpace = false, Name = "Ccaron", Undefined = false, BBox = new decimal[] { 0.028m, -0.014m, 0.633m, 0.886m }};
		static Glyph ugrave = new Glyph { Char = (char)249, w0 = 0.5F, IsWordSpace = false, Name = "ugrave", Undefined = false, BBox = new decimal[] { 0.009m, -0.01m, 0.479m, 0.678m }};
		static Glyph radical = new Glyph { Char = (char)8730, w0 = 0.453F, IsWordSpace = false, Name = "radical", Undefined = false, BBox = new decimal[] { 0.002m, -0.06m, 0.452m, 0.768m }};
		static Glyph Dcaron = new Glyph { Char = (char)270, w0 = 0.722F, IsWordSpace = false, Name = "Dcaron", Undefined = false, BBox = new decimal[] { 0.016m, 0m, 0.685m, 0.886m }, Kernings = new Dictionary<char,float> {  ['\u0041'] = 40f, ['\u00C1'] = 40f, ['\u0102'] = 40f, ['\u00C2'] = 40f, ['\u00C4'] = 40f, ['\u00C0'] = 40f, ['\u0100'] = 40f, ['\u0104'] = 40f, ['\u00C5'] = 40f, ['\u00C3'] = 40f, ['\u0056'] = 40f, ['\u0057'] = 30f, ['\u0059'] = 55f, ['\u00DD'] = 55f, ['\u0178'] = 55f, } };
		static Glyph rcommaaccent = new Glyph { Char = (char)343, w0 = 0.333F, IsWordSpace = false, Name = "rcommaaccent", Undefined = false, BBox = new decimal[] { 0.005m, -0.218m, 0.335m, 0.46m }, Kernings = new Dictionary<char,float> {  ['\u002C'] = 40f, ['\u0067'] = 18f, ['\u011F'] = 18f, ['\u0123'] = 18f, ['\u002D'] = 20f, ['\u002E'] = 55f, } };
		static Glyph Ntilde = new Glyph { Char = (char)209, w0 = 0.722F, IsWordSpace = false, Name = "Ntilde", Undefined = false, BBox = new decimal[] { 0.012m, -0.011m, 0.707m, 0.85m }, Kernings = new Dictionary<char,float> {  ['\u0041'] = 35f, ['\u00C1'] = 35f, ['\u0102'] = 35f, ['\u00C2'] = 35f, ['\u00C4'] = 35f, ['\u00C0'] = 35f, ['\u0100'] = 35f, ['\u0104'] = 35f, ['\u00C5'] = 35f, ['\u00C3'] = 35f, } };
		static Glyph otilde = new Glyph { Char = (char)245, w0 = 0.5F, IsWordSpace = false, Name = "otilde", Undefined = false, BBox = new decimal[] { 0.029m, -0.01m, 0.47m, 0.638m }, Kernings = new Dictionary<char,float> {  ['\u0076'] = 15f, ['\u0077'] = 25f, ['\u0079'] = 10f, ['\u00FD'] = 10f, ['\u00FF'] = 10f, } };
		static Glyph Rcommaaccent = new Glyph { Char = (char)342, w0 = 0.667F, IsWordSpace = false, Name = "Rcommaaccent", Undefined = false, BBox = new decimal[] { 0.017m, -0.198m, 0.659m, 0.662m }, Kernings = new Dictionary<char,float> {  ['\u004F'] = 40f, ['\u00D3'] = 40f, ['\u00D4'] = 40f, ['\u00D6'] = 40f, ['\u00D2'] = 40f, ['\u0150'] = 40f, ['\u014C'] = 40f, ['\u00D8'] = 40f, ['\u00D5'] = 40f, ['\u0054'] = 60f, ['\u0164'] = 60f, ['\u0162'] = 60f, ['\u0055'] = 40f, ['\u00DA'] = 40f, ['\u00DB'] = 40f, ['\u00DC'] = 40f, ['\u00D9'] = 40f, ['\u0170'] = 40f, ['\u016A'] = 40f, ['\u0172'] = 40f, ['\u016E'] = 40f, ['\u0056'] = 80f, ['\u0057'] = 55f, ['\u0059'] = 65f, ['\u00DD'] = 65f, ['\u0178'] = 65f, } };
		static Glyph Lcommaaccent = new Glyph { Char = (char)315, w0 = 0.611F, IsWordSpace = false, Name = "Lcommaaccent", Undefined = false, BBox = new decimal[] { 0.012m, -0.218m, 0.598m, 0.662m }, Kernings = new Dictionary<char,float> {  ['\u0054'] = 92f, ['\u0164'] = 92f, ['\u0162'] = 92f, ['\u0056'] = 100f, ['\u0057'] = 74f, ['\u0059'] = 100f, ['\u00DD'] = 100f, ['\u0178'] = 100f, ['\u2019'] = 92f, ['\u0079'] = 55f, ['\u00FD'] = 55f, ['\u00FF'] = 55f, } };
		static Glyph Atilde = new Glyph { Char = (char)195, w0 = 0.722F, IsWordSpace = false, Name = "Atilde", Undefined = false, BBox = new decimal[] { 0.015m, 0m, 0.706m, 0.85m }, Kernings = new Dictionary<char,float> {  ['\u0043'] = 40f, ['\u0106'] = 40f, ['\u010C'] = 40f, ['\u00C7'] = 40f, ['\u0047'] = 40f, ['\u011E'] = 40f, ['\u0122'] = 40f, ['\u004F'] = 55f, ['\u00D3'] = 55f, ['\u00D4'] = 55f, ['\u00D6'] = 55f, ['\u00D2'] = 55f, ['\u0150'] = 55f, ['\u014C'] = 55f, ['\u00D8'] = 55f, ['\u00D5'] = 55f, ['\u0051'] = 55f, ['\u0054'] = 111f, ['\u0164'] = 111f, ['\u0162'] = 111f, ['\u0055'] = 55f, ['\u00DA'] = 55f, ['\u00DB'] = 55f, ['\u00DC'] = 55f, ['\u00D9'] = 55f, ['\u0170'] = 55f, ['\u016A'] = 55f, ['\u0172'] = 55f, ['\u016E'] = 55f, ['\u0056'] = 135f, ['\u0057'] = 90f, ['\u0059'] = 105f, ['\u00DD'] = 105f, ['\u0178'] = 105f, ['\u2019'] = 111f, ['\u0076'] = 74f, ['\u0077'] = 92f, ['\u0079'] = 92f, ['\u00FD'] = 92f, ['\u00FF'] = 92f, } };
		static Glyph Aogonek = new Glyph { Char = (char)260, w0 = 0.722F, IsWordSpace = false, Name = "Aogonek", Undefined = false, BBox = new decimal[] { 0.015m, -0.165m, 0.738m, 0.674m }, Kernings = new Dictionary<char,float> {  ['\u0043'] = 40f, ['\u0106'] = 40f, ['\u010C'] = 40f, ['\u00C7'] = 40f, ['\u0047'] = 40f, ['\u011E'] = 40f, ['\u0122'] = 40f, ['\u004F'] = 55f, ['\u00D3'] = 55f, ['\u00D4'] = 55f, ['\u00D6'] = 55f, ['\u00D2'] = 55f, ['\u0150'] = 55f, ['\u014C'] = 55f, ['\u00D8'] = 55f, ['\u00D5'] = 55f, ['\u0051'] = 55f, ['\u0054'] = 111f, ['\u0164'] = 111f, ['\u0162'] = 111f, ['\u0055'] = 55f, ['\u00DA'] = 55f, ['\u00DB'] = 55f, ['\u00DC'] = 55f, ['\u00D9'] = 55f, ['\u0170'] = 55f, ['\u016A'] = 55f, ['\u0172'] = 55f, ['\u016E'] = 55f, ['\u0056'] = 135f, ['\u0057'] = 90f, ['\u0059'] = 105f, ['\u00DD'] = 105f, ['\u0178'] = 105f, ['\u2019'] = 111f, ['\u0076'] = 74f, ['\u0077'] = 52f, ['\u0079'] = 52f, ['\u00FD'] = 52f, ['\u00FF'] = 52f, } };
		static Glyph Aring = new Glyph { Char = (char)197, w0 = 0.722F, IsWordSpace = false, Name = "Aring", Undefined = false, BBox = new decimal[] { 0.015m, 0m, 0.706m, 0.898m }, Kernings = new Dictionary<char,float> {  ['\u0043'] = 40f, ['\u0106'] = 40f, ['\u010C'] = 40f, ['\u00C7'] = 40f, ['\u0047'] = 40f, ['\u011E'] = 40f, ['\u0122'] = 40f, ['\u004F'] = 55f, ['\u00D3'] = 55f, ['\u00D4'] = 55f, ['\u00D6'] = 55f, ['\u00D2'] = 55f, ['\u0150'] = 55f, ['\u014C'] = 55f, ['\u00D8'] = 55f, ['\u00D5'] = 55f, ['\u0051'] = 55f, ['\u0054'] = 111f, ['\u0164'] = 111f, ['\u0162'] = 111f, ['\u0055'] = 55f, ['\u00DA'] = 55f, ['\u00DB'] = 55f, ['\u00DC'] = 55f, ['\u00D9'] = 55f, ['\u0170'] = 55f, ['\u016A'] = 55f, ['\u0172'] = 55f, ['\u016E'] = 55f, ['\u0056'] = 135f, ['\u0057'] = 90f, ['\u0059'] = 105f, ['\u00DD'] = 105f, ['\u0178'] = 105f, ['\u2019'] = 111f, ['\u0076'] = 74f, ['\u0077'] = 92f, ['\u0079'] = 92f, ['\u00FD'] = 92f, ['\u00FF'] = 92f, } };
		static Glyph Otilde = new Glyph { Char = (char)213, w0 = 0.722F, IsWordSpace = false, Name = "Otilde", Undefined = false, BBox = new decimal[] { 0.034m, -0.014m, 0.688m, 0.85m }, Kernings = new Dictionary<char,float> {  ['\u0041'] = 35f, ['\u00C1'] = 35f, ['\u0102'] = 35f, ['\u00C2'] = 35f, ['\u00C4'] = 35f, ['\u00C0'] = 35f, ['\u0100'] = 35f, ['\u0104'] = 35f, ['\u00C5'] = 35f, ['\u00C3'] = 35f, ['\u0054'] = 40f, ['\u0164'] = 40f, ['\u0162'] = 40f, ['\u0056'] = 50f, ['\u0057'] = 35f, ['\u0058'] = 40f, ['\u0059'] = 50f, ['\u00DD'] = 50f, ['\u0178'] = 50f, } };
		static Glyph zdotaccent = new Glyph { Char = (char)380, w0 = 0.444F, IsWordSpace = false, Name = "zdotaccent", Undefined = false, BBox = new decimal[] { 0.027m, 0m, 0.418m, 0.623m }};
		static Glyph Ecaron = new Glyph { Char = (char)282, w0 = 0.611F, IsWordSpace = false, Name = "Ecaron", Undefined = false, BBox = new decimal[] { 0.012m, 0m, 0.597m, 0.886m }};
		static Glyph Iogonek = new Glyph { Char = (char)302, w0 = 0.333F, IsWordSpace = false, Name = "Iogonek", Undefined = false, BBox = new decimal[] { 0.018m, -0.165m, 0.315m, 0.662m }};
		static Glyph kcommaaccent = new Glyph { Char = (char)311, w0 = 0.5F, IsWordSpace = false, Name = "kcommaaccent", Undefined = false, BBox = new decimal[] { 0.007m, -0.218m, 0.505m, 0.683m }, Kernings = new Dictionary<char,float> {  ['\u0065'] = 10f, ['\u00E9'] = 10f, ['\u011B'] = 10f, ['\u00EA'] = 10f, ['\u00EB'] = 10f, ['\u0117'] = 10f, ['\u00E8'] = 10f, ['\u0113'] = 10f, ['\u0119'] = 10f, ['\u006F'] = 10f, ['\u00F3'] = 10f, ['\u00F4'] = 10f, ['\u00F6'] = 10f, ['\u00F2'] = 10f, ['\u0151'] = 10f, ['\u014D'] = 10f, ['\u00F8'] = 10f, ['\u00F5'] = 10f, ['\u0079'] = 15f, ['\u00FD'] = 15f, ['\u00FF'] = 15f, } };
		static Glyph minus = new Glyph { Char = (char)8722, w0 = 0.564F, IsWordSpace = false, Name = "minus", Undefined = false, BBox = new decimal[] { 0.03m, 0.22m, 0.534m, 0.286m }};
		static Glyph Icircumflex = new Glyph { Char = (char)206, w0 = 0.333F, IsWordSpace = false, Name = "Icircumflex", Undefined = false, BBox = new decimal[] { 0.011m, 0m, 0.322m, 0.886m }};
		static Glyph ncaron = new Glyph { Char = (char)328, w0 = 0.5F, IsWordSpace = false, Name = "ncaron", Undefined = false, BBox = new decimal[] { 0.016m, 0m, 0.485m, 0.674m }, Kernings = new Dictionary<char,float> {  ['\u0076'] = 40f, ['\u0079'] = 15f, ['\u00FD'] = 15f, ['\u00FF'] = 15f, } };
		static Glyph tcommaaccent = new Glyph { Char = (char)355, w0 = 0.278F, IsWordSpace = false, Name = "tcommaaccent", Undefined = false, BBox = new decimal[] { 0.013m, -0.218m, 0.279m, 0.579m }};
		static Glyph logicalnot = new Glyph { Char = (char)172, w0 = 0.564F, IsWordSpace = false, Name = "logicalnot", Undefined = false, BBox = new decimal[] { 0.03m, 0.108m, 0.534m, 0.386m }};
		static Glyph odieresis = new Glyph { Char = (char)246, w0 = 0.5F, IsWordSpace = false, Name = "odieresis", Undefined = false, BBox = new decimal[] { 0.029m, -0.01m, 0.47m, 0.623m }, Kernings = new Dictionary<char,float> {  ['\u0076'] = 15f, ['\u0077'] = 25f, ['\u0079'] = 10f, ['\u00FD'] = 10f, ['\u00FF'] = 10f, } };
		static Glyph udieresis = new Glyph { Char = (char)252, w0 = 0.5F, IsWordSpace = false, Name = "udieresis", Undefined = false, BBox = new decimal[] { 0.009m, -0.01m, 0.479m, 0.623m }};
		static Glyph notequal = new Glyph { Char = (char)8800, w0 = 0.549F, IsWordSpace = false, Name = "notequal", Undefined = false, BBox = new decimal[] { 0.012m, -0.031m, 0.537m, 0.547m }};
		static Glyph gcommaaccent = new Glyph { Char = (char)291, w0 = 0.5F, IsWordSpace = false, Name = "gcommaaccent", Undefined = false, BBox = new decimal[] { 0.028m, -0.218m, 0.47m, 0.749m }, Kernings = new Dictionary<char,float> {  ['\u0061'] = 5f, ['\u00E1'] = 5f, ['\u0103'] = 5f, ['\u00E2'] = 5f, ['\u00E4'] = 5f, ['\u00E0'] = 5f, ['\u0101'] = 5f, ['\u0105'] = 5f, ['\u00E5'] = 5f, ['\u00E3'] = 5f, } };
		static Glyph eth = new Glyph { Char = (char)240, w0 = 0.5F, IsWordSpace = false, Name = "eth", Undefined = false, BBox = new decimal[] { 0.029m, -0.01m, 0.471m, 0.686m }};
		static Glyph zcaron = new Glyph { Char = (char)382, w0 = 0.444F, IsWordSpace = false, Name = "zcaron", Undefined = false, BBox = new decimal[] { 0.027m, 0m, 0.418m, 0.674m }};
		static Glyph ncommaaccent = new Glyph { Char = (char)326, w0 = 0.5F, IsWordSpace = false, Name = "ncommaaccent", Undefined = false, BBox = new decimal[] { 0.016m, -0.218m, 0.485m, 0.46m }, Kernings = new Dictionary<char,float> {  ['\u0076'] = 40f, ['\u0079'] = 15f, ['\u00FD'] = 15f, ['\u00FF'] = 15f, } };
		static Glyph onesuperior = new Glyph { Char = (char)185, w0 = 0.3F, IsWordSpace = false, Name = "onesuperior", Undefined = false, BBox = new decimal[] { 0.057m, 0.27m, 0.248m, 0.676m }};
		static Glyph imacron = new Glyph { Char = (char)299, w0 = 0.278F, IsWordSpace = false, Name = "imacron", Undefined = false, BBox = new decimal[] { 0.006m, 0m, 0.271m, 0.601m }, Kernings = new Dictionary<char,float> {  ['\u0076'] = 25f, } };
		static Glyph Euro = new Glyph { Char = (char)8364, w0 = 0.5F, IsWordSpace = false, Name = "Euro", Undefined = false, BBox = new decimal[] { 0m, 0m, 0m, 0m }};
		public static Glyph[] AllGlyphs = new Glyph[] {
			space,
			exclam,
			quotedbl,
			numbersign,
			dollar,
			percent,
			ampersand,
			quoteright,
			parenleft,
			parenright,
			asterisk,
			plus,
			comma,
			hyphen,
			period,
			slash,
			zero,
			one,
			two,
			three,
			four,
			five,
			six,
			seven,
			eight,
			nine,
			colon,
			semicolon,
			less,
			equal,
			greater,
			question,
			at,
			A,
			B,
			C,
			D,
			E,
			F,
			G,
			H,
			I,
			J,
			K,
			L,
			M,
			N,
			O,
			P,
			Q,
			R,
			S,
			T,
			U,
			V,
			W,
			X,
			Y,
			Z,
			bracketleft,
			backslash,
			bracketright,
			asciicircum,
			underscore,
			quoteleft,
			a,
			b,
			c,
			d,
			e,
			f,
			g,
			h,
			i,
			j,
			k,
			l,
			m,
			n,
			o,
			p,
			q,
			r,
			s,
			t,
			u,
			v,
			w,
			x,
			y,
			z,
			braceleft,
			bar,
			braceright,
			asciitilde,
			exclamdown,
			cent,
			sterling,
			fraction,
			yen,
			florin,
			section,
			currency,
			quotesingle,
			quotedblleft,
			guillemotleft,
			guilsinglleft,
			guilsinglright,
			fi,
			fl,
			endash,
			dagger,
			daggerdbl,
			periodcentered,
			paragraph,
			bullet,
			quotesinglbase,
			quotedblbase,
			quotedblright,
			guillemotright,
			ellipsis,
			perthousand,
			questiondown,
			grave,
			acute,
			circumflex,
			tilde,
			macron,
			breve,
			dotaccent,
			dieresis,
			ring,
			cedilla,
			hungarumlaut,
			ogonek,
			caron,
			emdash,
			AE,
			ordfeminine,
			Lslash,
			Oslash,
			OE,
			ordmasculine,
			ae,
			dotlessi,
			lslash,
			oslash,
			oe,
			germandbls,
			Idieresis,
			eacute,
			abreve,
			uhungarumlaut,
			ecaron,
			Ydieresis,
			divide,
			Yacute,
			Acircumflex,
			aacute,
			Ucircumflex,
			yacute,
			scommaaccent,
			ecircumflex,
			Uring,
			Udieresis,
			aogonek,
			Uacute,
			uogonek,
			Edieresis,
			Dcroat,
			commaaccent,
			copyright,
			Emacron,
			ccaron,
			aring,
			Ncommaaccent,
			lacute,
			agrave,
			Tcommaaccent,
			Cacute,
			atilde,
			Edotaccent,
			scaron,
			scedilla,
			iacute,
			lozenge,
			Rcaron,
			Gcommaaccent,
			ucircumflex,
			acircumflex,
			Amacron,
			rcaron,
			ccedilla,
			Zdotaccent,
			Thorn,
			Omacron,
			Racute,
			Sacute,
			dcaron,
			Umacron,
			uring,
			threesuperior,
			Ograve,
			Agrave,
			Abreve,
			multiply,
			uacute,
			Tcaron,
			partialdiff,
			ydieresis,
			Nacute,
			icircumflex,
			Ecircumflex,
			adieresis,
			edieresis,
			cacute,
			nacute,
			umacron,
			Ncaron,
			Iacute,
			plusminus,
			brokenbar,
			registered,
			Gbreve,
			Idotaccent,
			summation,
			Egrave,
			racute,
			omacron,
			Zacute,
			Zcaron,
			greaterequal,
			Eth,
			Ccedilla,
			lcommaaccent,
			tcaron,
			eogonek,
			Uogonek,
			Aacute,
			Adieresis,
			egrave,
			zacute,
			iogonek,
			Oacute,
			oacute,
			amacron,
			sacute,
			idieresis,
			Ocircumflex,
			Ugrave,
			Delta,
			thorn,
			twosuperior,
			Odieresis,
			mu,
			igrave,
			ohungarumlaut,
			Eogonek,
			dcroat,
			threequarters,
			Scedilla,
			lcaron,
			Kcommaaccent,
			Lacute,
			trademark,
			edotaccent,
			Igrave,
			Imacron,
			Lcaron,
			onehalf,
			lessequal,
			ocircumflex,
			ntilde,
			Uhungarumlaut,
			Eacute,
			emacron,
			gbreve,
			onequarter,
			Scaron,
			Scommaaccent,
			Ohungarumlaut,
			degree,
			ograve,
			Ccaron,
			ugrave,
			radical,
			Dcaron,
			rcommaaccent,
			Ntilde,
			otilde,
			Rcommaaccent,
			Lcommaaccent,
			Atilde,
			Aogonek,
			Aring,
			Otilde,
			zdotaccent,
			Ecaron,
			Iogonek,
			kcommaaccent,
			minus,
			Icircumflex,
			ncaron,
			tcommaaccent,
			logicalnot,
			odieresis,
			udieresis,
			notequal,
			gcommaaccent,
			eth,
			zcaron,
			ncommaaccent,
			onesuperior,
			imacron,
			Euro,
		};
		public static Glyph?[] DefaultEncoding = new Glyph?[] {
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			space,
			exclam,
			quotedbl,
			numbersign,
			dollar,
			percent,
			ampersand,
			quoteright,
			parenleft,
			parenright,
			asterisk,
			plus,
			comma,
			hyphen,
			period,
			slash,
			zero,
			one,
			two,
			three,
			four,
			five,
			six,
			seven,
			eight,
			nine,
			colon,
			semicolon,
			less,
			equal,
			greater,
			question,
			at,
			A,
			B,
			C,
			D,
			E,
			F,
			G,
			H,
			I,
			J,
			K,
			L,
			M,
			N,
			O,
			P,
			Q,
			R,
			S,
			T,
			U,
			V,
			W,
			X,
			Y,
			Z,
			bracketleft,
			backslash,
			bracketright,
			asciicircum,
			underscore,
			quoteleft,
			a,
			b,
			c,
			d,
			e,
			f,
			g,
			h,
			i,
			j,
			k,
			l,
			m,
			n,
			o,
			p,
			q,
			r,
			s,
			t,
			u,
			v,
			w,
			x,
			y,
			z,
			braceleft,
			bar,
			braceright,
			asciitilde,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			exclamdown,
			cent,
			sterling,
			fraction,
			yen,
			florin,
			section,
			currency,
			quotesingle,
			quotedblleft,
			guillemotleft,
			guilsinglleft,
			guilsinglright,
			fi,
			fl,
			null,
			endash,
			dagger,
			daggerdbl,
			periodcentered,
			null,
			paragraph,
			bullet,
			quotesinglbase,
			quotedblbase,
			quotedblright,
			guillemotright,
			ellipsis,
			perthousand,
			null,
			questiondown,
			null,
			grave,
			acute,
			circumflex,
			tilde,
			macron,
			breve,
			dotaccent,
			dieresis,
			null,
			ring,
			cedilla,
			null,
			hungarumlaut,
			ogonek,
			caron,
			emdash,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			AE,
			null,
			ordfeminine,
			null,
			null,
			null,
			null,
			Lslash,
			Oslash,
			OE,
			ordmasculine,
			null,
			null,
			null,
			null,
			null,
			ae,
			null,
			null,
			null,
			dotlessi,
			null,
			null,
			lslash,
			oslash,
			oe,
			germandbls,
			null,
			null,
			null,
			null,
		};
	};
}
