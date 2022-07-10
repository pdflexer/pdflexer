namespace PdfLexer.Images;


// BELOW ENUM VALUES PORTED FROM:
// LibTiff.Net
// 
// Copyright(c) 2008-2022 Bit Miracle
// http://www.bitmiracle.com
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are
// met: 
//   Redistributions of source code must retain the above copyright notice,
//   this list of conditions and the following disclaimer.
// 
//   Redistributions in binary form must reproduce the above copyright
//   notice, this list of conditions and the following disclaimer in the
//   documentation and/or other materials provided with the distribution.
// 
//   Neither the name of the Bit Miracle nor the names of its contributors
//   may be used to endorse or promote products derived from this software
//   without specific prior written permission. 


//
// Summary:
//     TIFF tag definitions.
//
// Remarks:
//     Joris Van Damme maintains TIFF Tag Reference, good source of tag information.
//     It's an overview of known TIFF Tags with properties, short description, and other
//     useful information.
public enum TiffTag
{
    //
    // Summary:
    //     Tag placeholder
    IGNORE = 0,
    //
    // Summary:
    //     Subfile data descriptor. For the list of possible values, see BitMiracle.LibTiff.Classic.FileType.
    SUBFILETYPE = 254,
    //
    // Summary:
    //     [obsoleted by TIFF rev. 5.0]
    //     Kind of data in subfile. For the list of possible values, see BitMiracle.LibTiff.Classic.OFileType.
    OSUBFILETYPE = 0xFF,
    //
    // Summary:
    //     Image width in pixels.
    IMAGEWIDTH = 0x100,
    //
    // Summary:
    //     Image height in pixels.
    IMAGELENGTH = 257,
    //
    // Summary:
    //     Bits per channel (sample).
    BITSPERSAMPLE = 258,
    //
    // Summary:
    //     Data compression technique. For the list of possible values, see BitMiracle.LibTiff.Classic.Compression.
    COMPRESSION = 259,
    //
    // Summary:
    //     Photometric interpretation. For the list of possible values, see BitMiracle.LibTiff.Classic.Photometric.
    PHOTOMETRIC = 262,
    //
    // Summary:
    //     [obsoleted by TIFF rev. 5.0]
    //     Thresholding used on data. For the list of possible values, see BitMiracle.LibTiff.Classic.Threshold.
    THRESHHOLDING = 263,
    //
    // Summary:
    //     [obsoleted by TIFF rev. 5.0]
    //     Dithering matrix width.
    CELLWIDTH = 264,
    //
    // Summary:
    //     [obsoleted by TIFF rev. 5.0]
    //     Dithering matrix height.
    CELLLENGTH = 265,
    //
    // Summary:
    //     Data order within a byte. For the list of possible values, see BitMiracle.LibTiff.Classic.FillOrder.
    FILLORDER = 266,
    //
    // Summary:
    //     Name of document which holds for image.
    DOCUMENTNAME = 269,
    //
    // Summary:
    //     Information about image.
    IMAGEDESCRIPTION = 270,
    //
    // Summary:
    //     Scanner manufacturer name.
    MAKE = 271,
    //
    // Summary:
    //     Scanner model name/number.
    MODEL = 272,
    //
    // Summary:
    //     Offsets to data strips.
    STRIPOFFSETS = 273,
    //
    // Summary:
    //     [obsoleted by TIFF rev. 5.0]
    //     Image orientation. For the list of possible values, see BitMiracle.LibTiff.Classic.Orientation.
    ORIENTATION = 274,
    //
    // Summary:
    //     Samples per pixel.
    SAMPLESPERPIXEL = 277,
    //
    // Summary:
    //     Rows per strip of data.
    ROWSPERSTRIP = 278,
    //
    // Summary:
    //     Bytes counts for strips.
    STRIPBYTECOUNTS = 279,
    //
    // Summary:
    //     [obsoleted by TIFF rev. 5.0]
    //     Minimum sample value.
    MINSAMPLEVALUE = 280,
    //
    // Summary:
    //     [obsoleted by TIFF rev. 5.0]
    //     Maximum sample value.
    MAXSAMPLEVALUE = 281,
    //
    // Summary:
    //     Pixels/resolution in x.
    XRESOLUTION = 282,
    //
    // Summary:
    //     Pixels/resolution in y.
    YRESOLUTION = 283,
    //
    // Summary:
    //     Storage organization. For the list of possible values, see BitMiracle.LibTiff.Classic.PlanarConfig.
    PLANARCONFIG = 284,
    //
    // Summary:
    //     Page name image is from.
    PAGENAME = 285,
    //
    // Summary:
    //     X page offset of image lhs.
    XPOSITION = 286,
    //
    // Summary:
    //     Y page offset of image lhs.
    YPOSITION = 287,
    //
    // Summary:
    //     [obsoleted by TIFF rev. 5.0]
    //     Byte offset to free block.
    FREEOFFSETS = 288,
    //
    // Summary:
    //     [obsoleted by TIFF rev. 5.0]
    //     Sizes of free blocks.
    FREEBYTECOUNTS = 289,
    //
    // Summary:
    //     [obsoleted by TIFF rev. 6.0]
    //     Gray scale curve accuracy. For the list of possible values, see BitMiracle.LibTiff.Classic.GrayResponseUnit.
    GRAYRESPONSEUNIT = 290,
    //
    // Summary:
    //     [obsoleted by TIFF rev. 6.0]
    //     Gray scale response curve.
    GRAYRESPONSECURVE = 291,
    //
    // Summary:
    //     Options for CCITT Group 3 fax encoding. 32 flag bits. For the list of possible
    //     values, see BitMiracle.LibTiff.Classic.Group3Opt.
    GROUP3OPTIONS = 292,
    //
    // Summary:
    //     TIFF 6.0 proper name alias for GROUP3OPTIONS.
    T4OPTIONS = 292,
    //
    // Summary:
    //     Options for CCITT Group 4 fax encoding. 32 flag bits. For the list of possible
    //     values, see BitMiracle.LibTiff.Classic.Group3Opt.
    GROUP4OPTIONS = 293,
    //
    // Summary:
    //     TIFF 6.0 proper name alias for GROUP4OPTIONS.
    T6OPTIONS = 293,
    //
    // Summary:
    //     Units of resolutions. For the list of possible values, see BitMiracle.LibTiff.Classic.ResUnit.
    RESOLUTIONUNIT = 296,
    //
    // Summary:
    //     Page numbers of multi-page.
    PAGENUMBER = 297,
    //
    // Summary:
    //     [obsoleted by TIFF rev. 6.0]
    //     Color curve accuracy. For the list of possible values, see BitMiracle.LibTiff.Classic.ColorResponseUnit.
    COLORRESPONSEUNIT = 300,
    //
    // Summary:
    //     Colorimetry info.
    TRANSFERFUNCTION = 301,
    //
    // Summary:
    //     Name & release.
    SOFTWARE = 305,
    //
    // Summary:
    //     Creation date and time.
    DATETIME = 306,
    //
    // Summary:
    //     Creator of image.
    ARTIST = 315,
    //
    // Summary:
    //     Machine where created.
    HOSTCOMPUTER = 316,
    //
    // Summary:
    //     Prediction scheme w/ LZW. For the list of possible values, see BitMiracle.LibTiff.Classic.Predictor.
    PREDICTOR = 317,
    //
    // Summary:
    //     Image white point.
    WHITEPOINT = 318,
    //
    // Summary:
    //     Primary chromaticities.
    PRIMARYCHROMATICITIES = 319,
    //
    // Summary:
    //     RGB map for pallette image.
    COLORMAP = 320,
    //
    // Summary:
    //     Highlight + shadow info.
    HALFTONEHINTS = 321,
    //
    // Summary:
    //     Tile width in pixels.
    TILEWIDTH = 322,
    //
    // Summary:
    //     Tile height in pixels.
    TILELENGTH = 323,
    //
    // Summary:
    //     Offsets to data tiles.
    TILEOFFSETS = 324,
    //
    // Summary:
    //     Byte counts for tiles.
    TILEBYTECOUNTS = 325,
    //
    // Summary:
    //     Lines with wrong pixel count.
    BADFAXLINES = 326,
    //
    // Summary:
    //     Regenerated line info. For the list of possible values, see BitMiracle.LibTiff.Classic.CleanFaxData.
    CLEANFAXDATA = 327,
    //
    // Summary:
    //     Max consecutive bad lines.
    CONSECUTIVEBADFAXLINES = 328,
    //
    // Summary:
    //     Subimage descriptors.
    SUBIFD = 330,
    //
    // Summary:
    //     Inks in separated image. For the list of possible values, see BitMiracle.LibTiff.Classic.InkSet.
    INKSET = 332,
    //
    // Summary:
    //     ASCII names of inks.
    INKNAMES = 333,
    //
    // Summary:
    //     Number of inks.
    NUMBEROFINKS = 334,
    //
    // Summary:
    //     0% and 100% dot codes.
    DOTRANGE = 336,
    //
    // Summary:
    //     Separation target.
    TARGETPRINTER = 337,
    //
    // Summary:
    //     Information about extra samples. For the list of possible values, see BitMiracle.LibTiff.Classic.ExtraSample.
    EXTRASAMPLES = 338,
    //
    // Summary:
    //     Data sample format. For the list of possible values, see BitMiracle.LibTiff.Classic.SampleFormat.
    SAMPLEFORMAT = 339,
    //
    // Summary:
    //     Variable MinSampleValue.
    SMINSAMPLEVALUE = 340,
    //
    // Summary:
    //     Variable MaxSampleValue.
    SMAXSAMPLEVALUE = 341,
    //
    // Summary:
    //     ClipPath. Introduced post TIFF rev 6.0 by Adobe TIFF technote 2.
    CLIPPATH = 343,
    //
    // Summary:
    //     XClipPathUnits. Introduced post TIFF rev 6.0 by Adobe TIFF technote 2.
    XCLIPPATHUNITS = 344,
    //
    // Summary:
    //     YClipPathUnits. Introduced post TIFF rev 6.0 by Adobe TIFF technote 2.
    YCLIPPATHUNITS = 345,
    //
    // Summary:
    //     Indexed. Introduced post TIFF rev 6.0 by Adobe TIFF Technote 3.
    INDEXED = 346,
    //
    // Summary:
    //     JPEG table stream. Introduced post TIFF rev 6.0.
    JPEGTABLES = 347,
    //
    // Summary:
    //     OPI Proxy. Introduced post TIFF rev 6.0 by Adobe TIFF technote.
    OPIPROXY = 351,
    //
    // Summary:
    //     [obsoleted by Technical Note #2 which specifies a revised JPEG-in-TIFF scheme]
    //     JPEG processing algorithm. For the list of possible values, see BitMiracle.LibTiff.Classic.JpegProc.
    JPEGPROC = 0x200,
    //
    // Summary:
    //     [obsoleted by Technical Note #2 which specifies a revised JPEG-in-TIFF scheme]
    //     Pointer to SOI marker.
    JPEGIFOFFSET = 513,
    //
    // Summary:
    //     [obsoleted by Technical Note #2 which specifies a revised JPEG-in-TIFF scheme]
    //     JFIF stream length
    JPEGIFBYTECOUNT = 514,
    //
    // Summary:
    //     [obsoleted by Technical Note #2 which specifies a revised JPEG-in-TIFF scheme]
    //     Restart interval length.
    JPEGRESTARTINTERVAL = 515,
    //
    // Summary:
    //     [obsoleted by Technical Note #2 which specifies a revised JPEG-in-TIFF scheme]
    //     Lossless proc predictor.
    JPEGLOSSLESSPREDICTORS = 517,
    //
    // Summary:
    //     [obsoleted by Technical Note #2 which specifies a revised JPEG-in-TIFF scheme]
    //     Lossless point transform.
    JPEGPOINTTRANSFORM = 518,
    //
    // Summary:
    //     [obsoleted by Technical Note #2 which specifies a revised JPEG-in-TIFF scheme]
    //     Q matrice offsets.
    JPEGQTABLES = 519,
    //
    // Summary:
    //     [obsoleted by Technical Note #2 which specifies a revised JPEG-in-TIFF scheme]
    //     DCT table offsets.
    JPEGDCTABLES = 520,
    //
    // Summary:
    //     [obsoleted by Technical Note #2 which specifies a revised JPEG-in-TIFF scheme]
    //     AC coefficient offsets.
    JPEGACTABLES = 521,
    //
    // Summary:
    //     RGB -> YCbCr transform.
    YCBCRCOEFFICIENTS = 529,
    //
    // Summary:
    //     YCbCr subsampling factors.
    YCBCRSUBSAMPLING = 530,
    //
    // Summary:
    //     Subsample positioning. For the list of possible values, see BitMiracle.LibTiff.Classic.YCbCrPosition.
    YCBCRPOSITIONING = 531,
    //
    // Summary:
    //     Colorimetry info.
    REFERENCEBLACKWHITE = 532,
    //
    // Summary:
    //     XML packet. Introduced post TIFF rev 6.0 by Adobe XMP Specification, January
    //     2004.
    XMLPACKET = 700,
    //
    // Summary:
    //     OPI ImageID. Introduced post TIFF rev 6.0 by Adobe TIFF technote.
    OPIIMAGEID = 32781,
    //
    // Summary:
    //     Image reference points. Private tag registered to Island Graphics.
    REFPTS = 32953,
    //
    // Summary:
    //     Region-xform tack point. Private tag registered to Island Graphics.
    REGIONTACKPOINT = 32954,
    //
    // Summary:
    //     Warp quadrilateral. Private tag registered to Island Graphics.
    REGIONWARPCORNERS = 32955,
    //
    // Summary:
    //     Affine transformation matrix. Private tag registered to Island Graphics.
    REGIONAFFINE = 32956,
    //
    // Summary:
    //     [obsoleted by TIFF rev. 6.0]
    //     Use EXTRASAMPLE tag. Private tag registered to SGI.
    MATTEING = 32995,
    //
    // Summary:
    //     [obsoleted by TIFF rev. 6.0]
    //     Use SAMPLEFORMAT tag. Private tag registered to SGI.
    DATATYPE = 32996,
    //
    // Summary:
    //     Z depth of image. Private tag registered to SGI.
    IMAGEDEPTH = 32997,
    //
    // Summary:
    //     Z depth/data tile. Private tag registered to SGI.
    TILEDEPTH = 32998,
    //
    // Summary:
    //     Full image size in X. This tag is set when an image has been cropped out of a
    //     larger image. It reflect width of the original uncropped image. The XPOSITION
    //     tag can be used to determine the position of the smaller image in the larger
    //     one. Private tag registered to Pixar.
    PIXAR_IMAGEFULLWIDTH = 33300,
    //
    // Summary:
    //     Full image size in Y. This tag is set when an image has been cropped out of a
    //     larger image. It reflect height of the original uncropped image. The YPOSITION
    //     can be used to determine the position of the smaller image in the larger one.
    //     Private tag registered to Pixar.
    PIXAR_IMAGEFULLLENGTH = 33301,
    //
    // Summary:
    //     Texture map format. Used to identify special image modes and data used by Pixar's
    //     texture formats. Private tag registered to Pixar.
    PIXAR_TEXTUREFORMAT = 33302,
    //
    // Summary:
    //     S&T wrap modes. Used to identify special image modes and data used by Pixar's
    //     texture formats. Private tag registered to Pixar.
    PIXAR_WRAPMODES = 33303,
    //
    // Summary:
    //     Cotan(fov) for env. maps. Used to identify special image modes and data used
    //     by Pixar's texture formats. Private tag registered to Pixar.
    PIXAR_FOVCOT = 33304,
    //
    // Summary:
    //     Used to identify special image modes and data used by Pixar's texture formats.
    //     Private tag registered to Pixar.
    PIXAR_MATRIX_WORLDTOSCREEN = 33305,
    //
    // Summary:
    //     Used to identify special image modes and data used by Pixar's texture formats.
    //     Private tag registered to Pixar.
    PIXAR_MATRIX_WORLDTOCAMERA = 33306,
    //
    // Summary:
    //     Device serial number. Private tag registered to Eastman Kodak.
    WRITERSERIALNUMBER = 33405,
    //
    // Summary:
    //     Copyright string. This tag is listed in the TIFF rev. 6.0 w/ unknown ownership.
    COPYRIGHT = 33432,
    //
    // Summary:
    //     IPTC TAG from RichTIFF specifications.
    RICHTIFFIPTC = 33723,
    //
    // Summary:
    //     Site name. Reserved for ANSI IT8 TIFF/IT.
    IT8SITE = 34016,
    //
    // Summary:
    //     Color seq. [RGB, CMYK, etc]. Reserved for ANSI IT8 TIFF/IT.
    IT8COLORSEQUENCE = 34017,
    //
    // Summary:
    //     DDES Header. Reserved for ANSI IT8 TIFF/IT.
    IT8HEADER = 34018,
    //
    // Summary:
    //     Raster scanline padding. Reserved for ANSI IT8 TIFF/IT.
    IT8RASTERPADDING = 34019,
    //
    // Summary:
    //     The number of bits in short run. Reserved for ANSI IT8 TIFF/IT.
    IT8BITSPERRUNLENGTH = 34020,
    //
    // Summary:
    //     The number of bits in long run. Reserved for ANSI IT8 TIFF/IT.
    IT8BITSPEREXTENDEDRUNLENGTH = 34021,
    //
    // Summary:
    //     LW colortable. Reserved for ANSI IT8 TIFF/IT.
    IT8COLORTABLE = 34022,
    //
    // Summary:
    //     BP/BL image color switch. Reserved for ANSI IT8 TIFF/IT.
    IT8IMAGECOLORINDICATOR = 34023,
    //
    // Summary:
    //     BP/BL bg color switch. Reserved for ANSI IT8 TIFF/IT.
    IT8BKGCOLORINDICATOR = 34024,
    //
    // Summary:
    //     BP/BL image color value. Reserved for ANSI IT8 TIFF/IT.
    IT8IMAGECOLORVALUE = 34025,
    //
    // Summary:
    //     BP/BL bg color value. Reserved for ANSI IT8 TIFF/IT.
    IT8BKGCOLORVALUE = 34026,
    //
    // Summary:
    //     MP pixel intensity value. Reserved for ANSI IT8 TIFF/IT.
    IT8PIXELINTENSITYRANGE = 34027,
    //
    // Summary:
    //     HC transparency switch. Reserved for ANSI IT8 TIFF/IT.
    IT8TRANSPARENCYINDICATOR = 34028,
    //
    // Summary:
    //     Color characterization table. Reserved for ANSI IT8 TIFF/IT.
    IT8COLORCHARACTERIZATION = 34029,
    //
    // Summary:
    //     HC usage indicator. Reserved for ANSI IT8 TIFF/IT.
    IT8HCUSAGE = 34030,
    //
    // Summary:
    //     Trapping indicator (untrapped = 0, trapped = 1). Reserved for ANSI IT8 TIFF/IT.
    IT8TRAPINDICATOR = 34031,
    //
    // Summary:
    //     CMYK color equivalents.
    IT8CMYKEQUIVALENT = 34032,
    //
    // Summary:
    //     Sequence Frame Count. Private tag registered to Texas Instruments.
    FRAMECOUNT = 34232,
    //
    // Summary:
    //     Private tag registered to Adobe for PhotoShop.
    PHOTOSHOP = 34377,
    //
    // Summary:
    //     Pointer to EXIF private directory. This tag is documented in EXIF specification.
    EXIFIFD = 34665,
    //
    // Summary:
    //     ICC profile data. ?? Private tag registered to Adobe. ??
    ICCPROFILE = 34675,
    //
    // Summary:
    //     JBIG options. Private tag registered to Pixel Magic.
    JBIGOPTIONS = 34750,
    //
    // Summary:
    //     Pointer to GPS private directory. This tag is documented in EXIF specification.
    GPSIFD = 34853,
    //
    // Summary:
    //     Encoded Class 2 ses. params. Private tag registered to SGI.
    FAXRECVPARAMS = 34908,
    //
    // Summary:
    //     Received SubAddr string. Private tag registered to SGI.
    FAXSUBADDRESS = 34909,
    //
    // Summary:
    //     Receive time (secs). Private tag registered to SGI.
    FAXRECVTIME = 34910,
    //
    // Summary:
    //     Encoded fax ses. params, Table 2/T.30. Private tag registered to SGI.
    FAXDCS = 34911,
    //
    // Summary:
    //     Sample value to Nits. Private tag registered to SGI.
    STONITS = 37439,
    //
    // Summary:
    //     Private tag registered to FedEx.
    FEDEX_EDR = 34929,
    //
    // Summary:
    //     Pointer to Interoperability private directory. This tag is documented in EXIF
    //     specification.
    INTEROPERABILITYIFD = 40965,
    //
    // Summary:
    //     DNG version number. Introduced by Adobe DNG specification.
    DNGVERSION = 50706,
    //
    // Summary:
    //     DNG compatibility version. Introduced by Adobe DNG specification.
    DNGBACKWARDVERSION = 50707,
    //
    // Summary:
    //     Name for the camera model. Introduced by Adobe DNG specification.
    UNIQUECAMERAMODEL = 50708,
    //
    // Summary:
    //     Localized camera model name. Introduced by Adobe DNG specification.
    LOCALIZEDCAMERAMODEL = 50709,
    //
    // Summary:
    //     CFAPattern->LinearRaw space mapping. Introduced by Adobe DNG specification.
    CFAPLANECOLOR = 50710,
    //
    // Summary:
    //     Spatial layout of the CFA. Introduced by Adobe DNG specification.
    CFALAYOUT = 50711,
    //
    // Summary:
    //     Lookup table description. Introduced by Adobe DNG specification.
    LINEARIZATIONTABLE = 50712,
    //
    // Summary:
    //     Repeat pattern size for the BlackLevel tag. Introduced by Adobe DNG specification.
    BLACKLEVELREPEATDIM = 50713,
    //
    // Summary:
    //     Zero light encoding level. Introduced by Adobe DNG specification.
    BLACKLEVEL = 50714,
    //
    // Summary:
    //     Zero light encoding level differences (columns). Introduced by Adobe DNG specification.
    BLACKLEVELDELTAH = 50715,
    //
    // Summary:
    //     Zero light encoding level differences (rows). Introduced by Adobe DNG specification.
    BLACKLEVELDELTAV = 50716,
    //
    // Summary:
    //     Fully saturated encoding level. Introduced by Adobe DNG specification.
    WHITELEVEL = 50717,
    //
    // Summary:
    //     Default scale factors. Introduced by Adobe DNG specification.
    DEFAULTSCALE = 50718,
    //
    // Summary:
    //     Origin of the final image area. Introduced by Adobe DNG specification.
    DEFAULTCROPORIGIN = 50719,
    //
    // Summary:
    //     Size of the final image area. Introduced by Adobe DNG specification.
    DEFAULTCROPSIZE = 50720,
    //
    // Summary:
    //     XYZ->reference color space transformation matrix 1. Introduced by Adobe DNG specification.
    COLORMATRIX1 = 50721,
    //
    // Summary:
    //     XYZ->reference color space transformation matrix 2. Introduced by Adobe DNG specification.
    COLORMATRIX2 = 50722,
    //
    // Summary:
    //     Calibration matrix 1. Introduced by Adobe DNG specification.
    CAMERACALIBRATION1 = 50723,
    //
    // Summary:
    //     Calibration matrix 2. Introduced by Adobe DNG specification.
    CAMERACALIBRATION2 = 50724,
    //
    // Summary:
    //     Dimensionality reduction matrix 1. Introduced by Adobe DNG specification.
    REDUCTIONMATRIX1 = 50725,
    //
    // Summary:
    //     Dimensionality reduction matrix 2. Introduced by Adobe DNG specification.
    REDUCTIONMATRIX2 = 50726,
    //
    // Summary:
    //     Gain applied the stored raw values. Introduced by Adobe DNG specification.
    ANALOGBALANCE = 50727,
    //
    // Summary:
    //     Selected white balance in linear reference space. Introduced by Adobe DNG specification.
    ASSHOTNEUTRAL = 50728,
    //
    // Summary:
    //     Selected white balance in x-y chromaticity coordinates. Introduced by Adobe DNG
    //     specification.
    ASSHOTWHITEXY = 50729,
    //
    // Summary:
    //     How much to move the zero point. Introduced by Adobe DNG specification.
    BASELINEEXPOSURE = 50730,
    //
    // Summary:
    //     Relative noise level. Introduced by Adobe DNG specification.
    BASELINENOISE = 50731,
    //
    // Summary:
    //     Relative amount of sharpening. Introduced by Adobe DNG specification.
    BASELINESHARPNESS = 50732,
    //
    // Summary:
    //     How closely the values of the green pixels in the blue/green rows track the values
    //     of the green pixels in the red/green rows. Introduced by Adobe DNG specification.
    BAYERGREENSPLIT = 50733,
    //
    // Summary:
    //     Non-linear encoding range. Introduced by Adobe DNG specification.
    LINEARRESPONSELIMIT = 50734,
    //
    // Summary:
    //     Camera's serial number. Introduced by Adobe DNG specification.
    CAMERASERIALNUMBER = 50735,
    //
    // Summary:
    //     Information about the lens.
    LENSINFO = 50736,
    //
    // Summary:
    //     Chroma blur radius. Introduced by Adobe DNG specification.
    CHROMABLURRADIUS = 50737,
    //
    // Summary:
    //     Relative strength of the camera's anti-alias filter. Introduced by Adobe DNG
    //     specification.
    ANTIALIASSTRENGTH = 50738,
    //
    // Summary:
    //     Used by Adobe Camera Raw. Introduced by Adobe DNG specification.
    SHADOWSCALE = 50739,
    //
    // Summary:
    //     Manufacturer's private data. Introduced by Adobe DNG specification.
    DNGPRIVATEDATA = 50740,
    //
    // Summary:
    //     Whether the EXIF MakerNote tag is safe to preserve along with the rest of the
    //     EXIF data. Introduced by Adobe DNG specification.
    MAKERNOTESAFETY = 50741,
    //
    // Summary:
    //     Illuminant 1. Introduced by Adobe DNG specification.
    CALIBRATIONILLUMINANT1 = 50778,
    //
    // Summary:
    //     Illuminant 2. Introduced by Adobe DNG specification.
    CALIBRATIONILLUMINANT2 = 50779,
    //
    // Summary:
    //     Best quality multiplier. Introduced by Adobe DNG specification.
    BESTQUALITYSCALE = 50780,
    //
    // Summary:
    //     Unique identifier for the raw image data. Introduced by Adobe DNG specification.
    RAWDATAUNIQUEID = 50781,
    //
    // Summary:
    //     File name of the original raw file. Introduced by Adobe DNG specification.
    ORIGINALRAWFILENAME = 50827,
    //
    // Summary:
    //     Contents of the original raw file. Introduced by Adobe DNG specification.
    ORIGINALRAWFILEDATA = 50828,
    //
    // Summary:
    //     Active (non-masked) pixels of the sensor. Introduced by Adobe DNG specification.
    ACTIVEAREA = 50829,
    //
    // Summary:
    //     List of coordinates of fully masked pixels. Introduced by Adobe DNG specification.
    MASKEDAREAS = 50830,
    //
    // Summary:
    //     Used to map cameras's color space into ICC profile space. Introduced by Adobe
    //     DNG specification.
    ASSHOTICCPROFILE = 50831,
    //
    // Summary:
    //     Used to map cameras's color space into ICC profile space. Introduced by Adobe
    //     DNG specification.
    ASSHOTPREPROFILEMATRIX = 50832,
    //
    // Summary:
    //     Introduced by Adobe DNG specification.
    CURRENTICCPROFILE = 50833,
    //
    // Summary:
    //     Introduced by Adobe DNG specification.
    CURRENTPREPROFILEMATRIX = 50834,
    //
    // Summary:
    //     Undefined tag used by Eastman Kodak, hue shift correction data.
    DCSHUESHIFTVALUES = 0xFFFF,
    //
    // Summary:
    //     [pseudo tag. not written to file]
    //     Group 3/4 format control. For the list of possible values, see BitMiracle.LibTiff.Classic.FaxMode.
    FAXMODE = 0x10000,
    //
    // Summary:
    //     [pseudo tag. not written to file]
    //     Compression quality level. Quality level is on the IJG 0-100 scale. Default value
    //     is 75.
    JPEGQUALITY = 65537,
    //
    // Summary:
    //     [pseudo tag. not written to file]
    //     Auto RGB<=>YCbCr convert. For the list of possible values, see BitMiracle.LibTiff.Classic.JpegColorMode.
    JPEGCOLORMODE = 65538,
    //
    // Summary:
    //     [pseudo tag. not written to file]
    //     For the list of possible values, see BitMiracle.LibTiff.Classic.JpegTablesMode.
    //     Default is BitMiracle.LibTiff.Classic.JpegTablesMode.QUANT | BitMiracle.LibTiff.Classic.JpegTablesMode.HUFF.
    JPEGTABLESMODE = 65539,
    //
    // Summary:
    //     [pseudo tag. not written to file]
    //     G3/G4 fill function.
    FAXFILLFUNC = 65540,
    //
    // Summary:
    //     [pseudo tag. not written to file]
    //     PixarLogCodec I/O data sz.
    PIXARLOGDATAFMT = 65549,
    //
    // Summary:
    //     [pseudo tag. not written to file]
    //     Imager mode & filter. Allocated to Oceana Matrix (dev@oceana.com).
    DCSIMAGERTYPE = 65550,
    //
    // Summary:
    //     [pseudo tag. not written to file]
    //     Interpolation mode. Allocated to Oceana Matrix (dev@oceana.com).
    DCSINTERPMODE = 65551,
    //
    // Summary:
    //     [pseudo tag. not written to file]
    //     Color balance values. Allocated to Oceana Matrix (dev@oceana.com).
    DCSBALANCEARRAY = 65552,
    //
    // Summary:
    //     [pseudo tag. not written to file]
    //     Color correction values. Allocated to Oceana Matrix (dev@oceana.com).
    DCSCORRECTMATRIX = 65553,
    //
    // Summary:
    //     [pseudo tag. not written to file]
    //     Gamma value. Allocated to Oceana Matrix (dev@oceana.com).
    DCSGAMMA = 65554,
    //
    // Summary:
    //     [pseudo tag. not written to file]
    //     Toe & shoulder points. Allocated to Oceana Matrix (dev@oceana.com).
    DCSTOESHOULDERPTS = 65555,
    //
    // Summary:
    //     [pseudo tag. not written to file]
    //     Calibration file description.
    DCSCALIBRATIONFD = 65556,
    //
    // Summary:
    //     [pseudo tag. not written to file]
    //     Compression quality level. Quality level is on the ZLIB 1-9 scale. Default value
    //     is -1.
    ZIPQUALITY = 65557,
    //
    // Summary:
    //     [pseudo tag. not written to file]
    //     PixarLog uses same scale.
    PIXARLOGQUALITY = 65558,
    //
    // Summary:
    //     [pseudo tag. not written to file]
    //     Area of image to acquire. Allocated to Oceana Matrix (dev@oceana.com).
    DCSCLIPRECTANGLE = 65559,
    //
    // Summary:
    //     [pseudo tag. not written to file]
    //     SGILog user data format.
    SGILOGDATAFMT = 65560,
    //
    // Summary:
    //     [pseudo tag. not written to file]
    //     SGILog data encoding control.
    SGILOGENCODE = 65561,
    //
    // Summary:
    //     Exposure time.
    EXIF_EXPOSURETIME = 33434,
    //
    // Summary:
    //     F number.
    EXIF_FNUMBER = 33437,
    //
    // Summary:
    //     Exposure program.
    EXIF_EXPOSUREPROGRAM = 34850,
    //
    // Summary:
    //     Spectral sensitivity.
    EXIF_SPECTRALSENSITIVITY = 34852,
    //
    // Summary:
    //     ISO speed rating.
    EXIF_ISOSPEEDRATINGS = 34855,
    //
    // Summary:
    //     Optoelectric conversion factor.
    EXIF_OECF = 34856,
    //
    // Summary:
    //     Exif version.
    EXIF_EXIFVERSION = 36864,
    //
    // Summary:
    //     Date and time of original data generation.
    EXIF_DATETIMEORIGINAL = 36867,
    //
    // Summary:
    //     Date and time of digital data generation.
    EXIF_DATETIMEDIGITIZED = 36868,
    //
    // Summary:
    //     Meaning of each component.
    EXIF_COMPONENTSCONFIGURATION = 37121,
    //
    // Summary:
    //     Image compression mode.
    EXIF_COMPRESSEDBITSPERPIXEL = 37122,
    //
    // Summary:
    //     Shutter speed.
    EXIF_SHUTTERSPEEDVALUE = 37377,
    //
    // Summary:
    //     Aperture.
    EXIF_APERTUREVALUE = 37378,
    //
    // Summary:
    //     Brightness.
    EXIF_BRIGHTNESSVALUE = 37379,
    //
    // Summary:
    //     Exposure bias.
    EXIF_EXPOSUREBIASVALUE = 37380,
    //
    // Summary:
    //     Maximum lens aperture.
    EXIF_MAXAPERTUREVALUE = 37381,
    //
    // Summary:
    //     Subject distance.
    EXIF_SUBJECTDISTANCE = 37382,
    //
    // Summary:
    //     Metering mode.
    EXIF_METERINGMODE = 37383,
    //
    // Summary:
    //     Light source.
    EXIF_LIGHTSOURCE = 37384,
    //
    // Summary:
    //     Flash.
    EXIF_FLASH = 37385,
    //
    // Summary:
    //     Lens focal length.
    EXIF_FOCALLENGTH = 37386,
    //
    // Summary:
    //     Subject area.
    EXIF_SUBJECTAREA = 37396,
    //
    // Summary:
    //     Manufacturer notes.
    EXIF_MAKERNOTE = 37500,
    //
    // Summary:
    //     User comments.
    EXIF_USERCOMMENT = 37510,
    //
    // Summary:
    //     DateTime subseconds.
    EXIF_SUBSECTIME = 37520,
    //
    // Summary:
    //     DateTimeOriginal subseconds.
    EXIF_SUBSECTIMEORIGINAL = 37521,
    //
    // Summary:
    //     DateTimeDigitized subseconds.
    EXIF_SUBSECTIMEDIGITIZED = 37522,
    //
    // Summary:
    //     Supported Flashpix version.
    EXIF_FLASHPIXVERSION = 40960,
    //
    // Summary:
    //     Color space information.
    EXIF_COLORSPACE = 40961,
    //
    // Summary:
    //     Valid image width.
    EXIF_PIXELXDIMENSION = 40962,
    //
    // Summary:
    //     Valid image height.
    EXIF_PIXELYDIMENSION = 40963,
    //
    // Summary:
    //     Related audio file.
    EXIF_RELATEDSOUNDFILE = 40964,
    //
    // Summary:
    //     Flash energy.
    EXIF_FLASHENERGY = 41483,
    //
    // Summary:
    //     Spatial frequency response.
    EXIF_SPATIALFREQUENCYRESPONSE = 41484,
    //
    // Summary:
    //     Focal plane X resolution.
    EXIF_FOCALPLANEXRESOLUTION = 41486,
    //
    // Summary:
    //     Focal plane Y resolution.
    EXIF_FOCALPLANEYRESOLUTION = 41487,
    //
    // Summary:
    //     Focal plane resolution unit.
    EXIF_FOCALPLANERESOLUTIONUNIT = 41488,
    //
    // Summary:
    //     Subject location.
    EXIF_SUBJECTLOCATION = 41492,
    //
    // Summary:
    //     Exposure index.
    EXIF_EXPOSUREINDEX = 41493,
    //
    // Summary:
    //     Sensing method.
    EXIF_SENSINGMETHOD = 41495,
    //
    // Summary:
    //     File source.
    EXIF_FILESOURCE = 41728,
    //
    // Summary:
    //     Scene type.
    EXIF_SCENETYPE = 41729,
    //
    // Summary:
    //     CFA pattern.
    EXIF_CFAPATTERN = 41730,
    //
    // Summary:
    //     Custom image processing.
    EXIF_CUSTOMRENDERED = 41985,
    //
    // Summary:
    //     Exposure mode.
    EXIF_EXPOSUREMODE = 41986,
    //
    // Summary:
    //     White balance.
    EXIF_WHITEBALANCE = 41987,
    //
    // Summary:
    //     Digital zoom ratio.
    EXIF_DIGITALZOOMRATIO = 41988,
    //
    // Summary:
    //     Focal length in 35 mm film.
    EXIF_FOCALLENGTHIN35MMFILM = 41989,
    //
    // Summary:
    //     Scene capture type.
    EXIF_SCENECAPTURETYPE = 41990,
    //
    // Summary:
    //     Gain control.
    EXIF_GAINCONTROL = 41991,
    //
    // Summary:
    //     Contrast.
    EXIF_CONTRAST = 41992,
    //
    // Summary:
    //     Saturation.
    EXIF_SATURATION = 41993,
    //
    // Summary:
    //     Sharpness.
    EXIF_SHARPNESS = 41994,
    //
    // Summary:
    //     Device settings description.
    EXIF_DEVICESETTINGDESCRIPTION = 41995,
    //
    // Summary:
    //     Subject distance range.
    EXIF_SUBJECTDISTANCERANGE = 41996,
    //
    // Summary:
    //     Unique image ID.
    EXIF_IMAGEUNIQUEID = 42016,
    //
    // Summary:
    //     This tag is defining exact affine transformations between raster and model space.
    //     Used in interchangeable GeoTIFF files.
    GEOTIFF_MODELPIXELSCALETAG = 33550,
    //
    // Summary:
    //     This tag stores raster->model tiepoint pairs. Used in interchangeable GeoTIFF
    //     files.
    GEOTIFF_MODELTIEPOINTTAG = 33922,
    //
    // Summary:
    //     This tag is optionally provided for defining exact affine transformations between
    //     raster and model space. Used in interchangeable GeoTIFF files.
    GEOTIFF_MODELTRANSFORMATIONTAG = 34264,
    //
    // Summary:
    //     This tag may be used to store the GeoKey Directory, which defines and references
    //     the "GeoKeys". Used in interchangeable GeoTIFF files.
    GEOTIFF_GEOKEYDIRECTORYTAG = 34735,
    //
    // Summary:
    //     This tag is used to store all of the DOUBLE valued GeoKeys, referenced by the
    //     GeoKeyDirectoryTag. Used in interchangeable GeoTIFF files.
    GEOTIFF_GEODOUBLEPARAMSTAG = 34736,
    //
    // Summary:
    //     This tag is used to store all of the ASCII valued GeoKeys, referenced by the
    //     GeoKeyDirectoryTag. Used in interchangeable GeoTIFF files.
    GEOTIFF_GEOASCIIPARAMSTAG = 34737
}

//
// Summary:
//     Tag data type.
//
// Remarks:
//     Note: RATIONALs are the ratio of two 32-bit integer values.
public enum TiffType : short
{
    //
    // Summary:
    //     Placeholder.
    NOTYPE = 0,
    //
    // Summary:
    //     For field descriptor searching.
    ANY = 0,
    //
    // Summary:
    //     8-bit unsigned integer.
    BYTE = 1,
    //
    // Summary:
    //     8-bit bytes with last byte null.
    ASCII = 2,
    //
    // Summary:
    //     16-bit unsigned integer.
    SHORT = 3,
    //
    // Summary:
    //     32-bit unsigned integer.
    LONG = 4,
    //
    // Summary:
    //     64-bit unsigned fraction.
    RATIONAL = 5,
    //
    // Summary:
    //     8-bit signed integer.
    SBYTE = 6,
    //
    // Summary:
    //     8-bit untyped data.
    UNDEFINED = 7,
    //
    // Summary:
    //     16-bit signed integer.
    SSHORT = 8,
    //
    // Summary:
    //     32-bit signed integer.
    SLONG = 9,
    //
    // Summary:
    //     64-bit signed fraction.
    SRATIONAL = 10,
    //
    // Summary:
    //     32-bit IEEE floating point.
    FLOAT = 11,
    //
    // Summary:
    //     64-bit IEEE floating point.
    DOUBLE = 12,
    //
    // Summary:
    //     32-bit unsigned integer (offset)
    IFD = 13,
    //
    // Summary:
    //     BigTIFF 64-bit unsigned long
    LONG8 = 0x10,
    //
    // Summary:
    //     BigTIFF 64-bit signed long
    SLONG8 = 17,
    //
    // Summary:
    //     BigTIFF 64-bit unsigned integer/long (offset)
    IFD8 = 18
}

//
// Summary:
//     Compression scheme.
//     Possible values for BitMiracle.LibTiff.Classic.TiffTag.COMPRESSION tag.
public enum Compression
{
    //
    // Summary:
    //     Dump mode.
    NONE = 1,
    //
    // Summary:
    //     CCITT modified Huffman RLE.
    CCITTRLE = 2,
    //
    // Summary:
    //     CCITT Group 3 fax encoding.
    CCITTFAX3 = 3,
    //
    // Summary:
    //     CCITT T.4 (TIFF 6 name for CCITT Group 3 fax encoding).
    CCITT_T4 = 3,
    //
    // Summary:
    //     CCITT Group 4 fax encoding.
    CCITTFAX4 = 4,
    //
    // Summary:
    //     CCITT T.6 (TIFF 6 name for CCITT Group 4 fax encoding).
    CCITT_T6 = 4,
    //
    // Summary:
    //     Lempel-Ziv & Welch.
    LZW = 5,
    //
    // Summary:
    //     Original JPEG / Old-style JPEG (6.0).
    OJPEG = 6,
    //
    // Summary:
    //     JPEG DCT compression. Introduced post TIFF rev 6.0.
    JPEG = 7,
    //
    // Summary:
    //     NeXT 2-bit RLE.
    NEXT = 32766,
    //
    // Summary:
    //     CCITT RLE.
    CCITTRLEW = 32771,
    //
    // Summary:
    //     Macintosh RLE.
    PACKBITS = 32773,
    //
    // Summary:
    //     ThunderScan RLE.
    THUNDERSCAN = 32809,
    //
    // Summary:
    //     IT8 CT w/padding. Reserved for ANSI IT8 TIFF/IT.
    IT8CTPAD = 32895,
    //
    // Summary:
    //     IT8 Linework RLE. Reserved for ANSI IT8 TIFF/IT.
    IT8LW = 32896,
    //
    // Summary:
    //     IT8 Monochrome picture. Reserved for ANSI IT8 TIFF/IT.
    IT8MP = 32897,
    //
    // Summary:
    //     IT8 Binary line art. Reserved for ANSI IT8 TIFF/IT.
    IT8BL = 32898,
    //
    // Summary:
    //     Pixar companded 10bit LZW. Reserved for Pixar.
    PIXARFILM = 32908,
    //
    // Summary:
    //     Pixar companded 11bit ZIP. Reserved for Pixar.
    PIXARLOG = 32909,
    //
    // Summary:
    //     Deflate compression.
    DEFLATE = 32946,
    //
    // Summary:
    //     Deflate compression, as recognized by Adobe.
    ADOBE_DEFLATE = 8,
    //
    // Summary:
    //     Kodak DCS encoding. Reserved for Oceana Matrix (dev@oceana.com).
    DCS = 32947,
    //
    // Summary:
    //     ISO JBIG.
    JBIG = 34661,
    //
    // Summary:
    //     SGI Log Luminance RLE.
    SGILOG = 34676,
    //
    // Summary:
    //     SGI Log 24-bit packed.
    SGILOG24 = 34677,
    //
    // Summary:
    //     Leadtools JPEG2000.
    JP2000 = 34712
}
