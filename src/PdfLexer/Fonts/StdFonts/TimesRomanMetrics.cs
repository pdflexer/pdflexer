using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.Fonts.StdFonts
{
	internal class TimeRomanMetrics
	{
		static Glyph space = new Glyph { Char = (char)32, w0 = 0.25F, IsWordSpace = true, BBox = new decimal[] { 0m, 0m, 0m, 0m } };
		static Glyph exclam = new Glyph { Char = (char)33, w0 = 0.333F, IsWordSpace = false, BBox = new decimal[] { 0.13m, -0.009m, 0.238m, 0.676m } };
		static Glyph quotedbl = new Glyph { Char = (char)34, w0 = 0.408F, IsWordSpace = false, BBox = new decimal[] { 0.077m, 0.431m, 0.331m, 0.676m } };
		static Glyph numbersign = new Glyph { Char = (char)35, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.005m, 0m, 0.496m, 0.662m } };
		static Glyph dollar = new Glyph { Char = (char)36, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.044m, -0.087m, 0.457m, 0.727m } };
		static Glyph percent = new Glyph { Char = (char)37, w0 = 0.833F, IsWordSpace = false, BBox = new decimal[] { 0.061m, -0.013m, 0.772m, 0.676m } };
		static Glyph ampersand = new Glyph { Char = (char)38, w0 = 0.778F, IsWordSpace = false, BBox = new decimal[] { 0.042m, -0.013m, 0.75m, 0.676m } };
		static Glyph quoteright = new Glyph { Char = (char)8217, w0 = 0.333F, IsWordSpace = false, BBox = new decimal[] { 0.079m, 0.433m, 0.218m, 0.676m } };
		static Glyph parenleft = new Glyph { Char = (char)40, w0 = 0.333F, IsWordSpace = false, BBox = new decimal[] { 0.048m, -0.177m, 0.304m, 0.676m } };
		static Glyph parenright = new Glyph { Char = (char)41, w0 = 0.333F, IsWordSpace = false, BBox = new decimal[] { 0.029m, -0.177m, 0.285m, 0.676m } };
		static Glyph asterisk = new Glyph { Char = (char)42, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.069m, 0.265m, 0.432m, 0.676m } };
		static Glyph plus = new Glyph { Char = (char)43, w0 = 0.564F, IsWordSpace = false, BBox = new decimal[] { 0.03m, 0m, 0.534m, 0.506m } };
		static Glyph comma = new Glyph { Char = (char)44, w0 = 0.25F, IsWordSpace = false, BBox = new decimal[] { 0.056m, -0.141m, 0.195m, 0.102m } };
		static Glyph hyphen = new Glyph { Char = (char)45, w0 = 0.333F, IsWordSpace = false, BBox = new decimal[] { 0.039m, 0.194m, 0.285m, 0.257m } };
		static Glyph period = new Glyph { Char = (char)46, w0 = 0.25F, IsWordSpace = false, BBox = new decimal[] { 0.07m, -0.011m, 0.181m, 0.1m } };
		static Glyph slash = new Glyph { Char = (char)47, w0 = 0.278F, IsWordSpace = false, BBox = new decimal[] { -0.009m, -0.014m, 0.287m, 0.676m } };
		static Glyph zero = new Glyph { Char = (char)48, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.024m, -0.014m, 0.476m, 0.676m } };
		static Glyph one = new Glyph { Char = (char)49, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.111m, 0m, 0.394m, 0.676m } };
		static Glyph two = new Glyph { Char = (char)50, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.03m, 0m, 0.475m, 0.676m } };
		static Glyph three = new Glyph { Char = (char)51, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.043m, -0.014m, 0.431m, 0.676m } };
		static Glyph four = new Glyph { Char = (char)52, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.012m, 0m, 0.472m, 0.676m } };
		static Glyph five = new Glyph { Char = (char)53, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.032m, -0.014m, 0.438m, 0.688m } };
		static Glyph six = new Glyph { Char = (char)54, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.034m, -0.014m, 0.468m, 0.684m } };
		static Glyph seven = new Glyph { Char = (char)55, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.02m, -0.008m, 0.449m, 0.662m } };
		static Glyph eight = new Glyph { Char = (char)56, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.056m, -0.014m, 0.445m, 0.676m } };
		static Glyph nine = new Glyph { Char = (char)57, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.03m, -0.022m, 0.459m, 0.676m } };
		static Glyph colon = new Glyph { Char = (char)58, w0 = 0.278F, IsWordSpace = false, BBox = new decimal[] { 0.081m, -0.011m, 0.192m, 0.459m } };
		static Glyph semicolon = new Glyph { Char = (char)59, w0 = 0.278F, IsWordSpace = false, BBox = new decimal[] { 0.08m, -0.141m, 0.219m, 0.459m } };
		static Glyph less = new Glyph { Char = (char)60, w0 = 0.564F, IsWordSpace = false, BBox = new decimal[] { 0.028m, -0.008m, 0.536m, 0.514m } };
		static Glyph equal = new Glyph { Char = (char)61, w0 = 0.564F, IsWordSpace = false, BBox = new decimal[] { 0.03m, 0.12m, 0.534m, 0.386m } };
		static Glyph greater = new Glyph { Char = (char)62, w0 = 0.564F, IsWordSpace = false, BBox = new decimal[] { 0.028m, -0.008m, 0.536m, 0.514m } };
		static Glyph question = new Glyph { Char = (char)63, w0 = 0.444F, IsWordSpace = false, BBox = new decimal[] { 0.068m, -0.008m, 0.414m, 0.676m } };
		static Glyph at = new Glyph { Char = (char)64, w0 = 0.921F, IsWordSpace = false, BBox = new decimal[] { 0.116m, -0.014m, 0.809m, 0.676m } };
		static Glyph A = new Glyph { Char = (char)65, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.015m, 0m, 0.706m, 0.674m } };
		static Glyph B = new Glyph { Char = (char)66, w0 = 0.667F, IsWordSpace = false, BBox = new decimal[] { 0.017m, 0m, 0.593m, 0.662m } };
		static Glyph C = new Glyph { Char = (char)67, w0 = 0.667F, IsWordSpace = false, BBox = new decimal[] { 0.028m, -0.014m, 0.633m, 0.676m } };
		static Glyph D = new Glyph { Char = (char)68, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.016m, 0m, 0.685m, 0.662m } };
		static Glyph E = new Glyph { Char = (char)69, w0 = 0.611F, IsWordSpace = false, BBox = new decimal[] { 0.012m, 0m, 0.597m, 0.662m } };
		static Glyph F = new Glyph { Char = (char)70, w0 = 0.556F, IsWordSpace = false, BBox = new decimal[] { 0.012m, 0m, 0.546m, 0.662m } };
		static Glyph G = new Glyph { Char = (char)71, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.032m, -0.014m, 0.709m, 0.676m } };
		static Glyph H = new Glyph { Char = (char)72, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.019m, 0m, 0.702m, 0.662m } };
		static Glyph I = new Glyph { Char = (char)73, w0 = 0.333F, IsWordSpace = false, BBox = new decimal[] { 0.018m, 0m, 0.315m, 0.662m } };
		static Glyph J = new Glyph { Char = (char)74, w0 = 0.389F, IsWordSpace = false, BBox = new decimal[] { 0.01m, -0.014m, 0.37m, 0.662m } };
		static Glyph K = new Glyph { Char = (char)75, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.034m, 0m, 0.723m, 0.662m } };
		static Glyph L = new Glyph { Char = (char)76, w0 = 0.611F, IsWordSpace = false, BBox = new decimal[] { 0.012m, 0m, 0.598m, 0.662m } };
		static Glyph M = new Glyph { Char = (char)77, w0 = 0.889F, IsWordSpace = false, BBox = new decimal[] { 0.012m, 0m, 0.863m, 0.662m } };
		static Glyph N = new Glyph { Char = (char)78, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.012m, -0.011m, 0.707m, 0.662m } };
		static Glyph O = new Glyph { Char = (char)79, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.034m, -0.014m, 0.688m, 0.676m } };
		static Glyph P = new Glyph { Char = (char)80, w0 = 0.556F, IsWordSpace = false, BBox = new decimal[] { 0.016m, 0m, 0.542m, 0.662m } };
		static Glyph Q = new Glyph { Char = (char)81, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.034m, -0.178m, 0.701m, 0.676m } };
		static Glyph R = new Glyph { Char = (char)82, w0 = 0.667F, IsWordSpace = false, BBox = new decimal[] { 0.017m, 0m, 0.659m, 0.662m } };
		static Glyph S = new Glyph { Char = (char)83, w0 = 0.556F, IsWordSpace = false, BBox = new decimal[] { 0.042m, -0.014m, 0.491m, 0.676m } };
		static Glyph T = new Glyph { Char = (char)84, w0 = 0.611F, IsWordSpace = false, BBox = new decimal[] { 0.017m, 0m, 0.593m, 0.662m } };
		static Glyph U = new Glyph { Char = (char)85, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.014m, -0.014m, 0.705m, 0.662m } };
		static Glyph V = new Glyph { Char = (char)86, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.016m, -0.011m, 0.697m, 0.662m } };
		static Glyph W = new Glyph { Char = (char)87, w0 = 0.944F, IsWordSpace = false, BBox = new decimal[] { 0.005m, -0.011m, 0.932m, 0.662m } };
		static Glyph X = new Glyph { Char = (char)88, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.01m, 0m, 0.704m, 0.662m } };
		static Glyph Y = new Glyph { Char = (char)89, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.022m, 0m, 0.703m, 0.662m } };
		static Glyph Z = new Glyph { Char = (char)90, w0 = 0.611F, IsWordSpace = false, BBox = new decimal[] { 0.009m, 0m, 0.597m, 0.662m } };
		static Glyph bracketleft = new Glyph { Char = (char)91, w0 = 0.333F, IsWordSpace = false, BBox = new decimal[] { 0.088m, -0.156m, 0.299m, 0.662m } };
		static Glyph backslash = new Glyph { Char = (char)92, w0 = 0.278F, IsWordSpace = false, BBox = new decimal[] { -0.009m, -0.014m, 0.287m, 0.676m } };
		static Glyph bracketright = new Glyph { Char = (char)93, w0 = 0.333F, IsWordSpace = false, BBox = new decimal[] { 0.034m, -0.156m, 0.245m, 0.662m } };
		static Glyph asciicircum = new Glyph { Char = (char)94, w0 = 0.469F, IsWordSpace = false, BBox = new decimal[] { 0.024m, 0.297m, 0.446m, 0.662m } };
		static Glyph underscore = new Glyph { Char = (char)95, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0m, -0.125m, 0.5m, -0.075m } };
		static Glyph quoteleft = new Glyph { Char = (char)8216, w0 = 0.333F, IsWordSpace = false, BBox = new decimal[] { 0.115m, 0.433m, 0.254m, 0.676m } };
		static Glyph a = new Glyph { Char = (char)97, w0 = 0.444F, IsWordSpace = false, BBox = new decimal[] { 0.037m, -0.01m, 0.442m, 0.46m } };
		static Glyph b = new Glyph { Char = (char)98, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.003m, -0.01m, 0.468m, 0.683m } };
		static Glyph c = new Glyph { Char = (char)99, w0 = 0.444F, IsWordSpace = false, BBox = new decimal[] { 0.025m, -0.01m, 0.412m, 0.46m } };
		static Glyph d = new Glyph { Char = (char)100, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.027m, -0.01m, 0.491m, 0.683m } };
		static Glyph e = new Glyph { Char = (char)101, w0 = 0.444F, IsWordSpace = false, BBox = new decimal[] { 0.025m, -0.01m, 0.424m, 0.46m } };
		static Glyph f = new Glyph { Char = (char)102, w0 = 0.333F, IsWordSpace = false, BBox = new decimal[] { 0.02m, 0m, 0.383m, 0.683m } };
		static Glyph g = new Glyph { Char = (char)103, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.028m, -0.218m, 0.47m, 0.46m } };
		static Glyph h = new Glyph { Char = (char)104, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.009m, 0m, 0.487m, 0.683m } };
		static Glyph i = new Glyph { Char = (char)105, w0 = 0.278F, IsWordSpace = false, BBox = new decimal[] { 0.016m, 0m, 0.253m, 0.683m } };
		static Glyph j = new Glyph { Char = (char)106, w0 = 0.278F, IsWordSpace = false, BBox = new decimal[] { -0.07m, -0.218m, 0.194m, 0.683m } };
		static Glyph k = new Glyph { Char = (char)107, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.007m, 0m, 0.505m, 0.683m } };
		static Glyph l = new Glyph { Char = (char)108, w0 = 0.278F, IsWordSpace = false, BBox = new decimal[] { 0.019m, 0m, 0.257m, 0.683m } };
		static Glyph m = new Glyph { Char = (char)109, w0 = 0.778F, IsWordSpace = false, BBox = new decimal[] { 0.016m, 0m, 0.775m, 0.46m } };
		static Glyph n = new Glyph { Char = (char)110, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.016m, 0m, 0.485m, 0.46m } };
		static Glyph o = new Glyph { Char = (char)111, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.029m, -0.01m, 0.47m, 0.46m } };
		static Glyph p = new Glyph { Char = (char)112, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.005m, -0.217m, 0.47m, 0.46m } };
		static Glyph q = new Glyph { Char = (char)113, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.024m, -0.217m, 0.488m, 0.46m } };
		static Glyph r = new Glyph { Char = (char)114, w0 = 0.333F, IsWordSpace = false, BBox = new decimal[] { 0.005m, 0m, 0.335m, 0.46m } };
		static Glyph s = new Glyph { Char = (char)115, w0 = 0.389F, IsWordSpace = false, BBox = new decimal[] { 0.051m, -0.01m, 0.348m, 0.46m } };
		static Glyph t = new Glyph { Char = (char)116, w0 = 0.278F, IsWordSpace = false, BBox = new decimal[] { 0.013m, -0.01m, 0.279m, 0.579m } };
		static Glyph u = new Glyph { Char = (char)117, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.009m, -0.01m, 0.479m, 0.45m } };
		static Glyph v = new Glyph { Char = (char)118, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.019m, -0.014m, 0.477m, 0.45m } };
		static Glyph w = new Glyph { Char = (char)119, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.021m, -0.014m, 0.694m, 0.45m } };
		static Glyph x = new Glyph { Char = (char)120, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.017m, 0m, 0.479m, 0.45m } };
		static Glyph y = new Glyph { Char = (char)121, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.014m, -0.218m, 0.475m, 0.45m } };
		static Glyph z = new Glyph { Char = (char)122, w0 = 0.444F, IsWordSpace = false, BBox = new decimal[] { 0.027m, 0m, 0.418m, 0.45m } };
		static Glyph braceleft = new Glyph { Char = (char)123, w0 = 0.48F, IsWordSpace = false, BBox = new decimal[] { 0.1m, -0.181m, 0.35m, 0.68m } };
		static Glyph bar = new Glyph { Char = (char)124, w0 = 0.2F, IsWordSpace = false, BBox = new decimal[] { 0.067m, -0.218m, 0.133m, 0.782m } };
		static Glyph braceright = new Glyph { Char = (char)125, w0 = 0.48F, IsWordSpace = false, BBox = new decimal[] { 0.13m, -0.181m, 0.38m, 0.68m } };
		static Glyph asciitilde = new Glyph { Char = (char)126, w0 = 0.541F, IsWordSpace = false, BBox = new decimal[] { 0.04m, 0.183m, 0.502m, 0.323m } };
		static Glyph exclamdown = new Glyph { Char = (char)161, w0 = 0.333F, IsWordSpace = false, BBox = new decimal[] { 0.097m, -0.218m, 0.205m, 0.467m } };
		static Glyph cent = new Glyph { Char = (char)162, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.053m, -0.138m, 0.448m, 0.579m } };
		static Glyph sterling = new Glyph { Char = (char)163, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.012m, -0.008m, 0.49m, 0.676m } };
		static Glyph fraction = new Glyph { Char = (char)8260, w0 = 0.167F, IsWordSpace = false, BBox = new decimal[] { -0.168m, -0.014m, 0.331m, 0.676m } };
		static Glyph yen = new Glyph { Char = (char)165, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { -0.053m, 0m, 0.512m, 0.662m } };
		static Glyph florin = new Glyph { Char = (char)402, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.007m, -0.189m, 0.49m, 0.676m } };
		static Glyph section = new Glyph { Char = (char)167, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.07m, -0.148m, 0.426m, 0.676m } };
		static Glyph currency = new Glyph { Char = (char)164, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { -0.022m, 0.058m, 0.522m, 0.602m } };
		static Glyph quotesingle = new Glyph { Char = (char)39, w0 = 0.18F, IsWordSpace = false, BBox = new decimal[] { 0.048m, 0.431m, 0.133m, 0.676m } };
		static Glyph quotedblleft = new Glyph { Char = (char)8220, w0 = 0.444F, IsWordSpace = false, BBox = new decimal[] { 0.043m, 0.433m, 0.414m, 0.676m } };
		static Glyph guillemotleft = new Glyph { Char = (char)171, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.042m, 0.033m, 0.456m, 0.416m } };
		static Glyph guilsinglleft = new Glyph { Char = (char)8249, w0 = 0.333F, IsWordSpace = false, BBox = new decimal[] { 0.063m, 0.033m, 0.285m, 0.416m } };
		static Glyph guilsinglright = new Glyph { Char = (char)8250, w0 = 0.333F, IsWordSpace = false, BBox = new decimal[] { 0.048m, 0.033m, 0.27m, 0.416m } };
		static Glyph fi = new Glyph { Char = (char)64257, w0 = 0.556F, IsWordSpace = false, BBox = new decimal[] { 0.031m, 0m, 0.521m, 0.683m } };
		static Glyph fl = new Glyph { Char = (char)64258, w0 = 0.556F, IsWordSpace = false, BBox = new decimal[] { 0.032m, 0m, 0.521m, 0.683m } };
		static Glyph endash = new Glyph { Char = (char)8211, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0m, 0.201m, 0.5m, 0.25m } };
		static Glyph dagger = new Glyph { Char = (char)8224, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.059m, -0.149m, 0.442m, 0.676m } };
		static Glyph daggerdbl = new Glyph { Char = (char)8225, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.058m, -0.153m, 0.442m, 0.676m } };
		static Glyph periodcentered = new Glyph { Char = (char)183, w0 = 0.25F, IsWordSpace = false, BBox = new decimal[] { 0.07m, 0.199m, 0.181m, 0.31m } };
		static Glyph paragraph = new Glyph { Char = (char)182, w0 = 0.453F, IsWordSpace = false, BBox = new decimal[] { -0.022m, -0.154m, 0.45m, 0.662m } };
		static Glyph bullet = new Glyph { Char = (char)8226, w0 = 0.35F, IsWordSpace = false, BBox = new decimal[] { 0.04m, 0.196m, 0.31m, 0.466m } };
		static Glyph quotesinglbase = new Glyph { Char = (char)8218, w0 = 0.333F, IsWordSpace = false, BBox = new decimal[] { 0.079m, -0.141m, 0.218m, 0.102m } };
		static Glyph quotedblbase = new Glyph { Char = (char)8222, w0 = 0.444F, IsWordSpace = false, BBox = new decimal[] { 0.045m, -0.141m, 0.416m, 0.102m } };
		static Glyph quotedblright = new Glyph { Char = (char)8221, w0 = 0.444F, IsWordSpace = false, BBox = new decimal[] { 0.03m, 0.433m, 0.401m, 0.676m } };
		static Glyph guillemotright = new Glyph { Char = (char)187, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.044m, 0.033m, 0.458m, 0.416m } };
		static Glyph ellipsis = new Glyph { Char = (char)8230, w0 = 1F, IsWordSpace = false, BBox = new decimal[] { 0.111m, -0.011m, 0.888m, 0.1m } };
		static Glyph perthousand = new Glyph { Char = (char)8240, w0 = 1F, IsWordSpace = false, BBox = new decimal[] { 0.007m, -0.019m, 0.994m, 0.706m } };
		static Glyph questiondown = new Glyph { Char = (char)191, w0 = 0.444F, IsWordSpace = false, BBox = new decimal[] { 0.03m, -0.218m, 0.376m, 0.466m } };
		static Glyph grave = new Glyph { Char = (char)96, w0 = 0.333F, IsWordSpace = false, BBox = new decimal[] { 0.019m, 0.507m, 0.242m, 0.678m } };
		static Glyph acute = new Glyph { Char = (char)180, w0 = 0.333F, IsWordSpace = false, BBox = new decimal[] { 0.093m, 0.507m, 0.317m, 0.678m } };
		static Glyph circumflex = new Glyph { Char = (char)710, w0 = 0.333F, IsWordSpace = false, BBox = new decimal[] { 0.011m, 0.507m, 0.322m, 0.674m } };
		static Glyph tilde = new Glyph { Char = (char)732, w0 = 0.333F, IsWordSpace = false, BBox = new decimal[] { 0.001m, 0.532m, 0.331m, 0.638m } };
		static Glyph macron = new Glyph { Char = (char)175, w0 = 0.333F, IsWordSpace = false, BBox = new decimal[] { 0.011m, 0.547m, 0.322m, 0.601m } };
		static Glyph breve = new Glyph { Char = (char)728, w0 = 0.333F, IsWordSpace = false, BBox = new decimal[] { 0.026m, 0.507m, 0.307m, 0.664m } };
		static Glyph dotaccent = new Glyph { Char = (char)729, w0 = 0.333F, IsWordSpace = false, BBox = new decimal[] { 0.118m, 0.581m, 0.216m, 0.681m } };
		static Glyph dieresis = new Glyph { Char = (char)168, w0 = 0.333F, IsWordSpace = false, BBox = new decimal[] { 0.018m, 0.581m, 0.315m, 0.681m } };
		static Glyph ring = new Glyph { Char = (char)730, w0 = 0.333F, IsWordSpace = false, BBox = new decimal[] { 0.067m, 0.512m, 0.266m, 0.711m } };
		static Glyph cedilla = new Glyph { Char = (char)184, w0 = 0.333F, IsWordSpace = false, BBox = new decimal[] { 0.052m, -0.215m, 0.261m, 0m } };
		static Glyph hungarumlaut = new Glyph { Char = (char)733, w0 = 0.333F, IsWordSpace = false, BBox = new decimal[] { -0.003m, 0.507m, 0.377m, 0.678m } };
		static Glyph ogonek = new Glyph { Char = (char)731, w0 = 0.333F, IsWordSpace = false, BBox = new decimal[] { 0.062m, -0.165m, 0.243m, 0m } };
		static Glyph caron = new Glyph { Char = (char)711, w0 = 0.333F, IsWordSpace = false, BBox = new decimal[] { 0.011m, 0.507m, 0.322m, 0.674m } };
		static Glyph emdash = new Glyph { Char = (char)8212, w0 = 1F, IsWordSpace = false, BBox = new decimal[] { 0m, 0.201m, 1m, 0.25m } };
		static Glyph AE = new Glyph { Char = (char)198, w0 = 0.889F, IsWordSpace = false, BBox = new decimal[] { 0m, 0m, 0.863m, 0.662m } };
		static Glyph ordfeminine = new Glyph { Char = (char)170, w0 = 0.276F, IsWordSpace = false, BBox = new decimal[] { 0.004m, 0.394m, 0.27m, 0.676m } };
		static Glyph Lslash = new Glyph { Char = (char)321, w0 = 0.611F, IsWordSpace = false, BBox = new decimal[] { 0.012m, 0m, 0.598m, 0.662m } };
		static Glyph Oslash = new Glyph { Char = (char)216, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.034m, -0.08m, 0.688m, 0.734m } };
		static Glyph OE = new Glyph { Char = (char)338, w0 = 0.889F, IsWordSpace = false, BBox = new decimal[] { 0.03m, -0.006m, 0.885m, 0.668m } };
		static Glyph ordmasculine = new Glyph { Char = (char)186, w0 = 0.31F, IsWordSpace = false, BBox = new decimal[] { 0.006m, 0.394m, 0.304m, 0.676m } };
		static Glyph ae = new Glyph { Char = (char)230, w0 = 0.667F, IsWordSpace = false, BBox = new decimal[] { 0.038m, -0.01m, 0.632m, 0.46m } };
		static Glyph dotlessi = new Glyph { Char = (char)305, w0 = 0.278F, IsWordSpace = false, BBox = new decimal[] { 0.016m, 0m, 0.253m, 0.46m } };
		static Glyph lslash = new Glyph { Char = (char)322, w0 = 0.278F, IsWordSpace = false, BBox = new decimal[] { 0.019m, 0m, 0.259m, 0.683m } };
		static Glyph oslash = new Glyph { Char = (char)248, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.029m, -0.112m, 0.47m, 0.551m } };
		static Glyph oe = new Glyph { Char = (char)339, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.03m, -0.01m, 0.69m, 0.46m } };
		static Glyph germandbls = new Glyph { Char = (char)223, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.012m, -0.009m, 0.468m, 0.683m } };
		public static Glyph[] DefaultEncoding = new Glyph[] {
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
		public static Dictionary<string, Glyph> AllGlyphs = new Dictionary<string, Glyph>
		{
			["space"] = space,
			["exclam"] = exclam,
			["quotedbl"] = quotedbl,
			["numbersign"] = numbersign,
			["dollar"] = dollar,
			["percent"] = percent,
			["ampersand"] = ampersand,
			["quoteright"] = quoteright,
			["parenleft"] = parenleft,
			["parenright"] = parenright,
			["asterisk"] = asterisk,
			["plus"] = plus,
			["comma"] = comma,
			["hyphen"] = hyphen,
			["period"] = period,
			["slash"] = slash,
			["zero"] = zero,
			["one"] = one,
			["two"] = two,
			["three"] = three,
			["four"] = four,
			["five"] = five,
			["six"] = six,
			["seven"] = seven,
			["eight"] = eight,
			["nine"] = nine,
			["colon"] = colon,
			["semicolon"] = semicolon,
			["less"] = less,
			["equal"] = equal,
			["greater"] = greater,
			["question"] = question,
			["at"] = at,
			["A"] = A,
			["B"] = B,
			["C"] = C,
			["D"] = D,
			["E"] = E,
			["F"] = F,
			["G"] = G,
			["H"] = H,
			["I"] = I,
			["J"] = J,
			["K"] = K,
			["L"] = L,
			["M"] = M,
			["N"] = N,
			["O"] = O,
			["P"] = P,
			["Q"] = Q,
			["R"] = R,
			["S"] = S,
			["T"] = T,
			["U"] = U,
			["V"] = V,
			["W"] = W,
			["X"] = X,
			["Y"] = Y,
			["Z"] = Z,
			["bracketleft"] = bracketleft,
			["backslash"] = backslash,
			["bracketright"] = bracketright,
			["asciicircum"] = asciicircum,
			["underscore"] = underscore,
			["quoteleft"] = quoteleft,
			["a"] = a,
			["b"] = b,
			["c"] = c,
			["d"] = d,
			["e"] = e,
			["f"] = f,
			["g"] = g,
			["h"] = h,
			["i"] = i,
			["j"] = j,
			["k"] = k,
			["l"] = l,
			["m"] = m,
			["n"] = n,
			["o"] = o,
			["p"] = p,
			["q"] = q,
			["r"] = r,
			["s"] = s,
			["t"] = t,
			["u"] = u,
			["v"] = v,
			["w"] = w,
			["x"] = x,
			["y"] = y,
			["z"] = z,
			["braceleft"] = braceleft,
			["bar"] = bar,
			["braceright"] = braceright,
			["asciitilde"] = asciitilde,
			["exclamdown"] = exclamdown,
			["cent"] = cent,
			["sterling"] = sterling,
			["fraction"] = fraction,
			["yen"] = yen,
			["florin"] = florin,
			["section"] = section,
			["currency"] = currency,
			["quotesingle"] = quotesingle,
			["quotedblleft"] = quotedblleft,
			["guillemotleft"] = guillemotleft,
			["guilsinglleft"] = guilsinglleft,
			["guilsinglright"] = guilsinglright,
			["fi"] = fi,
			["fl"] = fl,
			["endash"] = endash,
			["dagger"] = dagger,
			["daggerdbl"] = daggerdbl,
			["periodcentered"] = periodcentered,
			["paragraph"] = paragraph,
			["bullet"] = bullet,
			["quotesinglbase"] = quotesinglbase,
			["quotedblbase"] = quotedblbase,
			["quotedblright"] = quotedblright,
			["guillemotright"] = guillemotright,
			["ellipsis"] = ellipsis,
			["perthousand"] = perthousand,
			["questiondown"] = questiondown,
			["grave"] = grave,
			["acute"] = acute,
			["circumflex"] = circumflex,
			["tilde"] = tilde,
			["macron"] = macron,
			["breve"] = breve,
			["dotaccent"] = dotaccent,
			["dieresis"] = dieresis,
			["ring"] = ring,
			["cedilla"] = cedilla,
			["hungarumlaut"] = hungarumlaut,
			["ogonek"] = ogonek,
			["caron"] = caron,
			["emdash"] = emdash,
			["AE"] = AE,
			["ordfeminine"] = ordfeminine,
			["Lslash"] = Lslash,
			["Oslash"] = Oslash,
			["OE"] = OE,
			["ordmasculine"] = ordmasculine,
			["ae"] = ae,
			["dotlessi"] = dotlessi,
			["lslash"] = lslash,
			["oslash"] = oslash,
			["oe"] = oe,
			["germandbls"] = germandbls,
			["Idieresis"] = new Glyph { Char = (char)207, w0 = 0.333F, IsWordSpace = false, BBox = new decimal[] { 0.018m, 0m, 0.315m, 0.835m } },
			["eacute"] = new Glyph { Char = (char)233, w0 = 0.444F, IsWordSpace = false, BBox = new decimal[] { 0.025m, -0.01m, 0.424m, 0.678m } },
			["abreve"] = new Glyph { Char = (char)259, w0 = 0.444F, IsWordSpace = false, BBox = new decimal[] { 0.037m, -0.01m, 0.442m, 0.664m } },
			["uhungarumlaut"] = new Glyph { Char = (char)369, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.009m, -0.01m, 0.501m, 0.678m } },
			["ecaron"] = new Glyph { Char = (char)283, w0 = 0.444F, IsWordSpace = false, BBox = new decimal[] { 0.025m, -0.01m, 0.424m, 0.674m } },
			["Ydieresis"] = new Glyph { Char = (char)376, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.022m, 0m, 0.703m, 0.835m } },
			["divide"] = new Glyph { Char = (char)247, w0 = 0.564F, IsWordSpace = false, BBox = new decimal[] { 0.03m, -0.01m, 0.534m, 0.516m } },
			["Yacute"] = new Glyph { Char = (char)221, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.022m, 0m, 0.703m, 0.89m } },
			["Acircumflex"] = new Glyph { Char = (char)194, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.015m, 0m, 0.706m, 0.886m } },
			["aacute"] = new Glyph { Char = (char)225, w0 = 0.444F, IsWordSpace = false, BBox = new decimal[] { 0.037m, -0.01m, 0.442m, 0.678m } },
			["Ucircumflex"] = new Glyph { Char = (char)219, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.014m, -0.014m, 0.705m, 0.886m } },
			["yacute"] = new Glyph { Char = (char)253, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.014m, -0.218m, 0.475m, 0.678m } },
			["scommaaccent"] = new Glyph { Char = (char)537, w0 = 0.389F, IsWordSpace = false, BBox = new decimal[] { 0.051m, -0.218m, 0.348m, 0.46m } },
			["ecircumflex"] = new Glyph { Char = (char)234, w0 = 0.444F, IsWordSpace = false, BBox = new decimal[] { 0.025m, -0.01m, 0.424m, 0.674m } },
			["Uring"] = new Glyph { Char = (char)366, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.014m, -0.014m, 0.705m, 0.898m } },
			["Udieresis"] = new Glyph { Char = (char)220, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.014m, -0.014m, 0.705m, 0.835m } },
			["aogonek"] = new Glyph { Char = (char)261, w0 = 0.444F, IsWordSpace = false, BBox = new decimal[] { 0.037m, -0.165m, 0.469m, 0.46m } },
			["Uacute"] = new Glyph { Char = (char)218, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.014m, -0.014m, 0.705m, 0.89m } },
			["uogonek"] = new Glyph { Char = (char)371, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.009m, -0.155m, 0.487m, 0.45m } },
			["Edieresis"] = new Glyph { Char = (char)203, w0 = 0.611F, IsWordSpace = false, BBox = new decimal[] { 0.012m, 0m, 0.597m, 0.835m } },
			["Dcroat"] = new Glyph { Char = (char)272, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.016m, 0m, 0.685m, 0.662m } },
			["commaaccent"] = new Glyph { Char = (char)63171, w0 = 0.25F, IsWordSpace = false, BBox = new decimal[] { 0.059m, -0.218m, 0.184m, -0.05m } },
			["copyright"] = new Glyph { Char = (char)169, w0 = 0.76F, IsWordSpace = false, BBox = new decimal[] { 0.038m, -0.014m, 0.722m, 0.676m } },
			["Emacron"] = new Glyph { Char = (char)274, w0 = 0.611F, IsWordSpace = false, BBox = new decimal[] { 0.012m, 0m, 0.597m, 0.813m } },
			["ccaron"] = new Glyph { Char = (char)269, w0 = 0.444F, IsWordSpace = false, BBox = new decimal[] { 0.025m, -0.01m, 0.412m, 0.674m } },
			["aring"] = new Glyph { Char = (char)229, w0 = 0.444F, IsWordSpace = false, BBox = new decimal[] { 0.037m, -0.01m, 0.442m, 0.711m } },
			["Ncommaaccent"] = new Glyph { Char = (char)325, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.012m, -0.198m, 0.707m, 0.662m } },
			["lacute"] = new Glyph { Char = (char)314, w0 = 0.278F, IsWordSpace = false, BBox = new decimal[] { 0.019m, 0m, 0.29m, 0.89m } },
			["agrave"] = new Glyph { Char = (char)224, w0 = 0.444F, IsWordSpace = false, BBox = new decimal[] { 0.037m, -0.01m, 0.442m, 0.678m } },
			["Tcommaaccent"] = new Glyph { Char = (char)354, w0 = 0.611F, IsWordSpace = false, BBox = new decimal[] { 0.017m, -0.218m, 0.593m, 0.662m } },
			["Cacute"] = new Glyph { Char = (char)262, w0 = 0.667F, IsWordSpace = false, BBox = new decimal[] { 0.028m, -0.014m, 0.633m, 0.89m } },
			["atilde"] = new Glyph { Char = (char)227, w0 = 0.444F, IsWordSpace = false, BBox = new decimal[] { 0.037m, -0.01m, 0.442m, 0.638m } },
			["Edotaccent"] = new Glyph { Char = (char)278, w0 = 0.611F, IsWordSpace = false, BBox = new decimal[] { 0.012m, 0m, 0.597m, 0.835m } },
			["scaron"] = new Glyph { Char = (char)353, w0 = 0.389F, IsWordSpace = false, BBox = new decimal[] { 0.039m, -0.01m, 0.35m, 0.674m } },
			["scedilla"] = new Glyph { Char = (char)351, w0 = 0.389F, IsWordSpace = false, BBox = new decimal[] { 0.051m, -0.215m, 0.348m, 0.46m } },
			["iacute"] = new Glyph { Char = (char)237, w0 = 0.278F, IsWordSpace = false, BBox = new decimal[] { 0.016m, 0m, 0.29m, 0.678m } },
			["lozenge"] = new Glyph { Char = (char)9674, w0 = 0.471F, IsWordSpace = false, BBox = new decimal[] { 0.013m, 0m, 0.459m, 0.724m } },
			["Rcaron"] = new Glyph { Char = (char)344, w0 = 0.667F, IsWordSpace = false, BBox = new decimal[] { 0.017m, 0m, 0.659m, 0.886m } },
			["Gcommaaccent"] = new Glyph { Char = (char)290, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.032m, -0.218m, 0.709m, 0.676m } },
			["ucircumflex"] = new Glyph { Char = (char)251, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.009m, -0.01m, 0.479m, 0.674m } },
			["acircumflex"] = new Glyph { Char = (char)226, w0 = 0.444F, IsWordSpace = false, BBox = new decimal[] { 0.037m, -0.01m, 0.442m, 0.674m } },
			["Amacron"] = new Glyph { Char = (char)256, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.015m, 0m, 0.706m, 0.813m } },
			["rcaron"] = new Glyph { Char = (char)345, w0 = 0.333F, IsWordSpace = false, BBox = new decimal[] { 0.005m, 0m, 0.335m, 0.674m } },
			["ccedilla"] = new Glyph { Char = (char)231, w0 = 0.444F, IsWordSpace = false, BBox = new decimal[] { 0.025m, -0.215m, 0.412m, 0.46m } },
			["Zdotaccent"] = new Glyph { Char = (char)379, w0 = 0.611F, IsWordSpace = false, BBox = new decimal[] { 0.009m, 0m, 0.597m, 0.835m } },
			["Thorn"] = new Glyph { Char = (char)222, w0 = 0.556F, IsWordSpace = false, BBox = new decimal[] { 0.016m, 0m, 0.542m, 0.662m } },
			["Omacron"] = new Glyph { Char = (char)332, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.034m, -0.014m, 0.688m, 0.813m } },
			["Racute"] = new Glyph { Char = (char)340, w0 = 0.667F, IsWordSpace = false, BBox = new decimal[] { 0.017m, 0m, 0.659m, 0.89m } },
			["Sacute"] = new Glyph { Char = (char)346, w0 = 0.556F, IsWordSpace = false, BBox = new decimal[] { 0.042m, -0.014m, 0.491m, 0.89m } },
			["dcaron"] = new Glyph { Char = (char)271, w0 = 0.588F, IsWordSpace = false, BBox = new decimal[] { 0.027m, -0.01m, 0.589m, 0.695m } },
			["Umacron"] = new Glyph { Char = (char)362, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.014m, -0.014m, 0.705m, 0.813m } },
			["uring"] = new Glyph { Char = (char)367, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.009m, -0.01m, 0.479m, 0.711m } },
			["threesuperior"] = new Glyph { Char = (char)179, w0 = 0.3F, IsWordSpace = false, BBox = new decimal[] { 0.015m, 0.262m, 0.291m, 0.676m } },
			["Ograve"] = new Glyph { Char = (char)210, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.034m, -0.014m, 0.688m, 0.89m } },
			["Agrave"] = new Glyph { Char = (char)192, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.015m, 0m, 0.706m, 0.89m } },
			["Abreve"] = new Glyph { Char = (char)258, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.015m, 0m, 0.706m, 0.876m } },
			["multiply"] = new Glyph { Char = (char)215, w0 = 0.564F, IsWordSpace = false, BBox = new decimal[] { 0.038m, 0.008m, 0.527m, 0.497m } },
			["uacute"] = new Glyph { Char = (char)250, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.009m, -0.01m, 0.479m, 0.678m } },
			["Tcaron"] = new Glyph { Char = (char)356, w0 = 0.611F, IsWordSpace = false, BBox = new decimal[] { 0.017m, 0m, 0.593m, 0.886m } },
			["partialdiff"] = new Glyph { Char = (char)8706, w0 = 0.476F, IsWordSpace = false, BBox = new decimal[] { 0.017m, -0.038m, 0.459m, 0.71m } },
			["ydieresis"] = new Glyph { Char = (char)255, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.014m, -0.218m, 0.475m, 0.623m } },
			["Nacute"] = new Glyph { Char = (char)323, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.012m, -0.011m, 0.707m, 0.89m } },
			["icircumflex"] = new Glyph { Char = (char)238, w0 = 0.278F, IsWordSpace = false, BBox = new decimal[] { -0.016m, 0m, 0.295m, 0.674m } },
			["Ecircumflex"] = new Glyph { Char = (char)202, w0 = 0.611F, IsWordSpace = false, BBox = new decimal[] { 0.012m, 0m, 0.597m, 0.886m } },
			["adieresis"] = new Glyph { Char = (char)228, w0 = 0.444F, IsWordSpace = false, BBox = new decimal[] { 0.037m, -0.01m, 0.442m, 0.623m } },
			["edieresis"] = new Glyph { Char = (char)235, w0 = 0.444F, IsWordSpace = false, BBox = new decimal[] { 0.025m, -0.01m, 0.424m, 0.623m } },
			["cacute"] = new Glyph { Char = (char)263, w0 = 0.444F, IsWordSpace = false, BBox = new decimal[] { 0.025m, -0.01m, 0.413m, 0.678m } },
			["nacute"] = new Glyph { Char = (char)324, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.016m, 0m, 0.485m, 0.678m } },
			["umacron"] = new Glyph { Char = (char)363, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.009m, -0.01m, 0.479m, 0.601m } },
			["Ncaron"] = new Glyph { Char = (char)327, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.012m, -0.011m, 0.707m, 0.886m } },
			["Iacute"] = new Glyph { Char = (char)205, w0 = 0.333F, IsWordSpace = false, BBox = new decimal[] { 0.018m, 0m, 0.317m, 0.89m } },
			["plusminus"] = new Glyph { Char = (char)177, w0 = 0.564F, IsWordSpace = false, BBox = new decimal[] { 0.03m, 0m, 0.534m, 0.506m } },
			["brokenbar"] = new Glyph { Char = (char)166, w0 = 0.2F, IsWordSpace = false, BBox = new decimal[] { 0.067m, -0.143m, 0.133m, 0.707m } },
			["registered"] = new Glyph { Char = (char)174, w0 = 0.76F, IsWordSpace = false, BBox = new decimal[] { 0.038m, -0.014m, 0.722m, 0.676m } },
			["Gbreve"] = new Glyph { Char = (char)286, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.032m, -0.014m, 0.709m, 0.876m } },
			["Idotaccent"] = new Glyph { Char = (char)304, w0 = 0.333F, IsWordSpace = false, BBox = new decimal[] { 0.018m, 0m, 0.315m, 0.835m } },
			["summation"] = new Glyph { Char = (char)8721, w0 = 0.6F, IsWordSpace = false, BBox = new decimal[] { 0.015m, -0.01m, 0.585m, 0.706m } },
			["Egrave"] = new Glyph { Char = (char)200, w0 = 0.611F, IsWordSpace = false, BBox = new decimal[] { 0.012m, 0m, 0.597m, 0.89m } },
			["racute"] = new Glyph { Char = (char)341, w0 = 0.333F, IsWordSpace = false, BBox = new decimal[] { 0.005m, 0m, 0.335m, 0.678m } },
			["omacron"] = new Glyph { Char = (char)333, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.029m, -0.01m, 0.47m, 0.601m } },
			["Zacute"] = new Glyph { Char = (char)377, w0 = 0.611F, IsWordSpace = false, BBox = new decimal[] { 0.009m, 0m, 0.597m, 0.89m } },
			["Zcaron"] = new Glyph { Char = (char)381, w0 = 0.611F, IsWordSpace = false, BBox = new decimal[] { 0.009m, 0m, 0.597m, 0.886m } },
			["greaterequal"] = new Glyph { Char = (char)8805, w0 = 0.549F, IsWordSpace = false, BBox = new decimal[] { 0.026m, 0m, 0.523m, 0.666m } },
			["Eth"] = new Glyph { Char = (char)208, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.016m, 0m, 0.685m, 0.662m } },
			["Ccedilla"] = new Glyph { Char = (char)199, w0 = 0.667F, IsWordSpace = false, BBox = new decimal[] { 0.028m, -0.215m, 0.633m, 0.676m } },
			["lcommaaccent"] = new Glyph { Char = (char)316, w0 = 0.278F, IsWordSpace = false, BBox = new decimal[] { 0.019m, -0.218m, 0.257m, 0.683m } },
			["tcaron"] = new Glyph { Char = (char)357, w0 = 0.326F, IsWordSpace = false, BBox = new decimal[] { 0.013m, -0.01m, 0.318m, 0.722m } },
			["eogonek"] = new Glyph { Char = (char)281, w0 = 0.444F, IsWordSpace = false, BBox = new decimal[] { 0.025m, -0.165m, 0.424m, 0.46m } },
			["Uogonek"] = new Glyph { Char = (char)370, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.014m, -0.165m, 0.705m, 0.662m } },
			["Aacute"] = new Glyph { Char = (char)193, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.015m, 0m, 0.706m, 0.89m } },
			["Adieresis"] = new Glyph { Char = (char)196, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.015m, 0m, 0.706m, 0.835m } },
			["egrave"] = new Glyph { Char = (char)232, w0 = 0.444F, IsWordSpace = false, BBox = new decimal[] { 0.025m, -0.01m, 0.424m, 0.678m } },
			["zacute"] = new Glyph { Char = (char)378, w0 = 0.444F, IsWordSpace = false, BBox = new decimal[] { 0.027m, 0m, 0.418m, 0.678m } },
			["iogonek"] = new Glyph { Char = (char)303, w0 = 0.278F, IsWordSpace = false, BBox = new decimal[] { 0.016m, -0.165m, 0.265m, 0.683m } },
			["Oacute"] = new Glyph { Char = (char)211, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.034m, -0.014m, 0.688m, 0.89m } },
			["oacute"] = new Glyph { Char = (char)243, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.029m, -0.01m, 0.47m, 0.678m } },
			["amacron"] = new Glyph { Char = (char)257, w0 = 0.444F, IsWordSpace = false, BBox = new decimal[] { 0.037m, -0.01m, 0.442m, 0.601m } },
			["sacute"] = new Glyph { Char = (char)347, w0 = 0.389F, IsWordSpace = false, BBox = new decimal[] { 0.051m, -0.01m, 0.348m, 0.678m } },
			["idieresis"] = new Glyph { Char = (char)239, w0 = 0.278F, IsWordSpace = false, BBox = new decimal[] { -0.009m, 0m, 0.288m, 0.623m } },
			["Ocircumflex"] = new Glyph { Char = (char)212, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.034m, -0.014m, 0.688m, 0.886m } },
			["Ugrave"] = new Glyph { Char = (char)217, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.014m, -0.014m, 0.705m, 0.89m } },
			["Delta"] = new Glyph { Char = (char)8710, w0 = 0.612F, IsWordSpace = false, BBox = new decimal[] { 0.006m, 0m, 0.608m, 0.688m } },
			["thorn"] = new Glyph { Char = (char)254, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.005m, -0.217m, 0.47m, 0.683m } },
			["twosuperior"] = new Glyph { Char = (char)178, w0 = 0.3F, IsWordSpace = false, BBox = new decimal[] { 0.001m, 0.27m, 0.296m, 0.676m } },
			["Odieresis"] = new Glyph { Char = (char)214, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.034m, -0.014m, 0.688m, 0.835m } },
			["mu"] = new Glyph { Char = (char)181, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.036m, -0.218m, 0.512m, 0.45m } },
			["igrave"] = new Glyph { Char = (char)236, w0 = 0.278F, IsWordSpace = false, BBox = new decimal[] { -0.008m, 0m, 0.253m, 0.678m } },
			["ohungarumlaut"] = new Glyph { Char = (char)337, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.029m, -0.01m, 0.491m, 0.678m } },
			["Eogonek"] = new Glyph { Char = (char)280, w0 = 0.611F, IsWordSpace = false, BBox = new decimal[] { 0.012m, -0.165m, 0.597m, 0.662m } },
			["dcroat"] = new Glyph { Char = (char)273, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.027m, -0.01m, 0.5m, 0.683m } },
			["threequarters"] = new Glyph { Char = (char)190, w0 = 0.75F, IsWordSpace = false, BBox = new decimal[] { 0.015m, -0.014m, 0.718m, 0.676m } },
			["Scedilla"] = new Glyph { Char = (char)350, w0 = 0.556F, IsWordSpace = false, BBox = new decimal[] { 0.042m, -0.215m, 0.491m, 0.676m } },
			["lcaron"] = new Glyph { Char = (char)318, w0 = 0.344F, IsWordSpace = false, BBox = new decimal[] { 0.019m, 0m, 0.347m, 0.695m } },
			["Kcommaaccent"] = new Glyph { Char = (char)310, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.034m, -0.198m, 0.723m, 0.662m } },
			["Lacute"] = new Glyph { Char = (char)313, w0 = 0.611F, IsWordSpace = false, BBox = new decimal[] { 0.012m, 0m, 0.598m, 0.89m } },
			["trademark"] = new Glyph { Char = (char)8482, w0 = 0.98F, IsWordSpace = false, BBox = new decimal[] { 0.03m, 0.256m, 0.957m, 0.662m } },
			["edotaccent"] = new Glyph { Char = (char)279, w0 = 0.444F, IsWordSpace = false, BBox = new decimal[] { 0.025m, -0.01m, 0.424m, 0.623m } },
			["Igrave"] = new Glyph { Char = (char)204, w0 = 0.333F, IsWordSpace = false, BBox = new decimal[] { 0.018m, 0m, 0.315m, 0.89m } },
			["Imacron"] = new Glyph { Char = (char)298, w0 = 0.333F, IsWordSpace = false, BBox = new decimal[] { 0.011m, 0m, 0.322m, 0.813m } },
			["Lcaron"] = new Glyph { Char = (char)317, w0 = 0.611F, IsWordSpace = false, BBox = new decimal[] { 0.012m, 0m, 0.598m, 0.676m } },
			["onehalf"] = new Glyph { Char = (char)189, w0 = 0.75F, IsWordSpace = false, BBox = new decimal[] { 0.031m, -0.014m, 0.746m, 0.676m } },
			["lessequal"] = new Glyph { Char = (char)8804, w0 = 0.549F, IsWordSpace = false, BBox = new decimal[] { 0.026m, 0m, 0.523m, 0.666m } },
			["ocircumflex"] = new Glyph { Char = (char)244, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.029m, -0.01m, 0.47m, 0.674m } },
			["ntilde"] = new Glyph { Char = (char)241, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.016m, 0m, 0.485m, 0.638m } },
			["Uhungarumlaut"] = new Glyph { Char = (char)368, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.014m, -0.014m, 0.705m, 0.89m } },
			["Eacute"] = new Glyph { Char = (char)201, w0 = 0.611F, IsWordSpace = false, BBox = new decimal[] { 0.012m, 0m, 0.597m, 0.89m } },
			["emacron"] = new Glyph { Char = (char)275, w0 = 0.444F, IsWordSpace = false, BBox = new decimal[] { 0.025m, -0.01m, 0.424m, 0.601m } },
			["gbreve"] = new Glyph { Char = (char)287, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.028m, -0.218m, 0.47m, 0.664m } },
			["onequarter"] = new Glyph { Char = (char)188, w0 = 0.75F, IsWordSpace = false, BBox = new decimal[] { 0.037m, -0.014m, 0.718m, 0.676m } },
			["Scaron"] = new Glyph { Char = (char)352, w0 = 0.556F, IsWordSpace = false, BBox = new decimal[] { 0.042m, -0.014m, 0.491m, 0.886m } },
			["Scommaaccent"] = new Glyph { Char = (char)536, w0 = 0.556F, IsWordSpace = false, BBox = new decimal[] { 0.042m, -0.218m, 0.491m, 0.676m } },
			["Ohungarumlaut"] = new Glyph { Char = (char)336, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.034m, -0.014m, 0.688m, 0.89m } },
			["degree"] = new Glyph { Char = (char)176, w0 = 0.4F, IsWordSpace = false, BBox = new decimal[] { 0.057m, 0.39m, 0.343m, 0.676m } },
			["ograve"] = new Glyph { Char = (char)242, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.029m, -0.01m, 0.47m, 0.678m } },
			["Ccaron"] = new Glyph { Char = (char)268, w0 = 0.667F, IsWordSpace = false, BBox = new decimal[] { 0.028m, -0.014m, 0.633m, 0.886m } },
			["ugrave"] = new Glyph { Char = (char)249, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.009m, -0.01m, 0.479m, 0.678m } },
			["radical"] = new Glyph { Char = (char)8730, w0 = 0.453F, IsWordSpace = false, BBox = new decimal[] { 0.002m, -0.06m, 0.452m, 0.768m } },
			["Dcaron"] = new Glyph { Char = (char)270, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.016m, 0m, 0.685m, 0.886m } },
			["rcommaaccent"] = new Glyph { Char = (char)343, w0 = 0.333F, IsWordSpace = false, BBox = new decimal[] { 0.005m, -0.218m, 0.335m, 0.46m } },
			["Ntilde"] = new Glyph { Char = (char)209, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.012m, -0.011m, 0.707m, 0.85m } },
			["otilde"] = new Glyph { Char = (char)245, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.029m, -0.01m, 0.47m, 0.638m } },
			["Rcommaaccent"] = new Glyph { Char = (char)342, w0 = 0.667F, IsWordSpace = false, BBox = new decimal[] { 0.017m, -0.198m, 0.659m, 0.662m } },
			["Lcommaaccent"] = new Glyph { Char = (char)315, w0 = 0.611F, IsWordSpace = false, BBox = new decimal[] { 0.012m, -0.218m, 0.598m, 0.662m } },
			["Atilde"] = new Glyph { Char = (char)195, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.015m, 0m, 0.706m, 0.85m } },
			["Aogonek"] = new Glyph { Char = (char)260, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.015m, -0.165m, 0.738m, 0.674m } },
			["Aring"] = new Glyph { Char = (char)197, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.015m, 0m, 0.706m, 0.898m } },
			["Otilde"] = new Glyph { Char = (char)213, w0 = 0.722F, IsWordSpace = false, BBox = new decimal[] { 0.034m, -0.014m, 0.688m, 0.85m } },
			["zdotaccent"] = new Glyph { Char = (char)380, w0 = 0.444F, IsWordSpace = false, BBox = new decimal[] { 0.027m, 0m, 0.418m, 0.623m } },
			["Ecaron"] = new Glyph { Char = (char)282, w0 = 0.611F, IsWordSpace = false, BBox = new decimal[] { 0.012m, 0m, 0.597m, 0.886m } },
			["Iogonek"] = new Glyph { Char = (char)302, w0 = 0.333F, IsWordSpace = false, BBox = new decimal[] { 0.018m, -0.165m, 0.315m, 0.662m } },
			["kcommaaccent"] = new Glyph { Char = (char)311, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.007m, -0.218m, 0.505m, 0.683m } },
			["minus"] = new Glyph { Char = (char)8722, w0 = 0.564F, IsWordSpace = false, BBox = new decimal[] { 0.03m, 0.22m, 0.534m, 0.286m } },
			["Icircumflex"] = new Glyph { Char = (char)206, w0 = 0.333F, IsWordSpace = false, BBox = new decimal[] { 0.011m, 0m, 0.322m, 0.886m } },
			["ncaron"] = new Glyph { Char = (char)328, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.016m, 0m, 0.485m, 0.674m } },
			["tcommaaccent"] = new Glyph { Char = (char)355, w0 = 0.278F, IsWordSpace = false, BBox = new decimal[] { 0.013m, -0.218m, 0.279m, 0.579m } },
			["logicalnot"] = new Glyph { Char = (char)172, w0 = 0.564F, IsWordSpace = false, BBox = new decimal[] { 0.03m, 0.108m, 0.534m, 0.386m } },
			["odieresis"] = new Glyph { Char = (char)246, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.029m, -0.01m, 0.47m, 0.623m } },
			["udieresis"] = new Glyph { Char = (char)252, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.009m, -0.01m, 0.479m, 0.623m } },
			["notequal"] = new Glyph { Char = (char)8800, w0 = 0.549F, IsWordSpace = false, BBox = new decimal[] { 0.012m, -0.031m, 0.537m, 0.547m } },
			["gcommaaccent"] = new Glyph { Char = (char)291, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.028m, -0.218m, 0.47m, 0.749m } },
			["eth"] = new Glyph { Char = (char)240, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.029m, -0.01m, 0.471m, 0.686m } },
			["zcaron"] = new Glyph { Char = (char)382, w0 = 0.444F, IsWordSpace = false, BBox = new decimal[] { 0.027m, 0m, 0.418m, 0.674m } },
			["ncommaaccent"] = new Glyph { Char = (char)326, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0.016m, -0.218m, 0.485m, 0.46m } },
			["onesuperior"] = new Glyph { Char = (char)185, w0 = 0.3F, IsWordSpace = false, BBox = new decimal[] { 0.057m, 0.27m, 0.248m, 0.676m } },
			["imacron"] = new Glyph { Char = (char)299, w0 = 0.278F, IsWordSpace = false, BBox = new decimal[] { 0.006m, 0m, 0.271m, 0.601m } },
			["Euro"] = new Glyph { Char = (char)8364, w0 = 0.5F, IsWordSpace = false, BBox = new decimal[] { 0m, 0m, 0m, 0m } },
		};
	}
}
