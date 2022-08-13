import * as fs from "node:fs";

const MAX_MAP_RANGE = 2 ** 24 - 1; // = 0xFFFFFF

class CMap {
  constructor(builtInCMap = false) {
    // Codespace ranges are stored as follows:
    // [[1BytePairs], [2BytePairs], [3BytePairs], [4BytePairs]]
    // where nBytePairs are ranges e.g. [low1, high1, low2, high2, ...]
    this.codespaceRanges = [[], [], [], []];
    this.numCodespaceRanges = 0;
    // Map entries have one of two forms.
    // - cid chars are 16-bit unsigned integers, stored as integers.
    // - bf chars are variable-length byte sequences, stored as strings, with
    //   one byte per character.
    this._map = [];
    this.name = "";
    this.vertical = false;
    this.useCMap = null;
    this.builtInCMap = builtInCMap;
  }

  addCodespaceRange(n, low, high) {
    this.codespaceRanges[n - 1].push(low, high);
    this.numCodespaceRanges++;
  }

  mapCidRange(low, high, dstLow) {
    if (high - low > MAX_MAP_RANGE) {
      throw new Error("mapCidRange - ignoring data above MAX_MAP_RANGE.");
    }
    while (low <= high) {
      this._map[low++] = dstLow++;
    }
  }

  mapBfRange(low, high, dstLow) {
    if (high - low > MAX_MAP_RANGE) {
      throw new Error("mapBfRange - ignoring data above MAX_MAP_RANGE.");
    }
    console.log("range ", low, high);
    const lastByte = dstLow.length - 1;
    while (low <= high) {
      if (low == 12359) {
        console.log("at char");
      }
      this._map[low++] = dstLow;
      // Only the last byte has to be incremented (in the normal case).
      const nextCharCode = dstLow.charCodeAt(lastByte) + 1;
      if (nextCharCode > 0xff) {
        dstLow =
          dstLow.substring(0, lastByte - 1) +
          String.fromCharCode(dstLow.charCodeAt(lastByte - 1) + 1) +
          "\x00";
        continue;
      }
      dstLow =
        dstLow.substring(0, lastByte) + String.fromCharCode(nextCharCode);
    }
  }

  mapBfRangeToArray(low, high, array) {
    if (high - low > MAX_MAP_RANGE) {
      throw new Error("mapBfRangeToArray - ignoring data above MAX_MAP_RANGE.");
    }
    const ii = array.length;
    let i = 0;
    while (low <= high && i < ii) {
      this._map[low] = array[i++];
      ++low;
    }
  }

  // This is used for both bf and cid chars.
  mapOne(src, dst) {
    this._map[src] = dst;
  }

  lookup(code) {
    return this._map[code];
  }

  contains(code) {
    return this._map[code] !== undefined;
  }

  forEach(callback) {
    // Most maps have fewer than 65536 entries, and for those we use normal
    // array iteration. But really sparse tables are possible -- e.g. with
    // indices in the *billions*. For such tables we use for..in, which isn't
    // ideal because it stringifies the indices for all present elements, but
    // it does avoid iterating over every undefined entry.
    const map = this._map;
    const length = map.length;
    if (length <= 0x10000) {
      for (let i = 0; i < length; i++) {
        if (map[i] !== undefined) {
          callback(i, map[i]);
        }
      }
    } else {
      for (const i in map) {
        callback(i, map[i]);
      }
    }
  }

  charCodeOf(value) {
    // `Array.prototype.indexOf` is *extremely* inefficient for arrays which
    // are both very sparse and very large (see issue8372.pdf).
    const map = this._map;
    if (map.length <= 0x10000) {
      return map.indexOf(value);
    }
    for (const charCode in map) {
      if (map[charCode] === value) {
        return charCode | 0;
      }
    }
    return -1;
  }

  getMap() {
    return this._map;
  }

  readCharCode(str, offset, out) {
    let c = 0;
    const codespaceRanges = this.codespaceRanges;
    // 9.7.6.2 CMap Mapping
    // The code length is at most 4.
    for (let n = 0, nn = codespaceRanges.length; n < nn; n++) {
      c = ((c << 8) | str.charCodeAt(offset + n)) >>> 0;
      // Check each codespace range to see if it falls within.
      const codespaceRange = codespaceRanges[n];
      for (let k = 0, kk = codespaceRange.length; k < kk; ) {
        const low = codespaceRange[k++];
        const high = codespaceRange[k++];
        if (c >= low && c <= high) {
          out.charcode = c;
          out.length = n + 1;
          return;
        }
      }
    }
    out.charcode = 0;
    out.length = 1;
  }

  getCharCodeLength(charCode) {
    const codespaceRanges = this.codespaceRanges;
    for (let n = 0, nn = codespaceRanges.length; n < nn; n++) {
      // Check each codespace range to see if it falls within.
      const codespaceRange = codespaceRanges[n];
      for (let k = 0, kk = codespaceRange.length; k < kk; ) {
        const low = codespaceRange[k++];
        const high = codespaceRange[k++];
        if (charCode >= low && charCode <= high) {
          return n + 1;
        }
      }
    }
    return 1;
  }

  get length() {
    return this._map.length;
  }

  get isIdentityCMap() {
    if (!(this.name === "Identity-H" || this.name === "Identity-V")) {
      return false;
    }
    if (this._map.length !== 0x10000) {
      return false;
    }
    for (let i = 0; i < 0x10000; i++) {
      if (this._map[i] !== i) {
        return false;
      }
    }
    return true;
  }
}

const BinaryCMapReader = (function BinaryCMapReaderClosure() {
  function hexToInt(a, size) {
    let n = 0;
    for (let i = 0; i <= size; i++) {
      n = (n << 8) | a[i];
    }
    return n >>> 0;
  }

  function hexToStr(a, size) {
    // This code is hot. Special-case some common values to avoid creating an
    // object with subarray().
    if (size === 1) {
      if (a[0] == 48 && a[1] == 146) {
        console.log(a);
      }
      return String.fromCharCode(a[0], a[1]);
    }
    if (size === 3) {
      return String.fromCharCode(a[0], a[1], a[2], a[3]);
    }
    return String.fromCharCode.apply(null, a.subarray(0, size + 1));
  }

  function addHex(a, b, size) {
    let c = 0;
    for (let i = size; i >= 0; i--) {
      c += a[i] + b[i];
      a[i] = c & 255;
      c >>= 8;
    }
  }

  function incHex(a, size) {
    let c = 1;
    for (let i = size; i >= 0 && c > 0; i--) {
      c += a[i];
      a[i] = c & 255;
      c >>= 8;
    }
  }

  const MAX_NUM_SIZE = 16;
  const MAX_ENCODED_NUM_SIZE = 19; // ceil(MAX_NUM_SIZE * 7 / 8)

  class BinaryCMapStream {
    constructor(data) {
      this.buffer = data;
      this.pos = 0;
      this.end = data.length;
      this.tmpBuf = new Uint8Array(MAX_ENCODED_NUM_SIZE);
    }

    readByte() {
      if (this.pos >= this.end) {
        return -1;
      }
      return this.buffer[this.pos++];
    }

    readNumber() {
      let n = 0;
      let last;
      do {
        const b = this.readByte();
        if (b < 0) {
          throw new FormatError("unexpected EOF in bcmap");
        }
        last = !(b & 0x80);
        n = (n << 7) | (b & 0x7f);
      } while (!last);
      return n;
    }

    readSigned() {
      const n = this.readNumber();
      return n & 1 ? ~(n >>> 1) : n >>> 1;
    }

    readHex(num, size) {
      num.set(this.buffer.subarray(this.pos, this.pos + size + 1));
      this.pos += size + 1;
    }

    readHexNumber(num, size) {
      let last;
      const stack = this.tmpBuf;
      let sp = 0;
      do {
        const b = this.readByte();
        if (b < 0) {
          throw new FormatError("unexpected EOF in bcmap");
        }
        last = !(b & 0x80);
        stack[sp++] = b & 0x7f;
      } while (!last);
      let i = size,
        buffer = 0,
        bufferSize = 0;
      while (i >= 0) {
        while (bufferSize < 8 && stack.length > 0) {
          buffer |= stack[--sp] << bufferSize;
          bufferSize += 7;
        }
        num[i] = buffer & 255;
        i--;
        buffer >>= 8;
        bufferSize -= 8;
      }
    }

    readHexSigned(num, size) {
      this.readHexNumber(num, size);
      const sign = num[size] & 1 ? 255 : 0;
      let c = 0;
      for (let i = 0; i <= size; i++) {
        c = ((c & 1) << 8) | num[i];
        num[i] = (c >> 1) ^ sign;
      }
    }

    readString() {
      const len = this.readNumber();
      let s = "";
      for (let i = 0; i < len; i++) {
        s += String.fromCharCode(this.readNumber());
      }
      return s;
    }
  }

  // eslint-disable-next-line no-shadow
  class BinaryCMapReader {
    async process(data, cMap, extend) {
      const stream = new BinaryCMapStream(data);
      const header = stream.readByte();
      cMap.vertical = !!(header & 1);

      let useCMap = null;
      const start = new Uint8Array(MAX_NUM_SIZE);
      const end = new Uint8Array(MAX_NUM_SIZE);
      const char = new Uint8Array(MAX_NUM_SIZE);
      const charCode = new Uint8Array(MAX_NUM_SIZE);
      const tmp = new Uint8Array(MAX_NUM_SIZE);
      let code;

      let b;
      while ((b = stream.readByte()) >= 0) {
        const type = b >> 5;
        if (type === 7) {
          // metadata, e.g. comment or usecmap
          switch (b & 0x1f) {
            case 0:
              stream.readString(); // skipping comment
              break;
            case 1:
              useCMap = stream.readString();
              break;
          }
          continue;
        }
        const sequence = !!(b & 0x10);
        const dataSize = b & 15;

        if (dataSize + 1 > MAX_NUM_SIZE) {
          throw new Error("BinaryCMapReader.process: Invalid dataSize.");
        }

        const ucs2DataSize = 1;
        const subitemsCount = stream.readNumber();
        switch (type) {
          case 0: // codespacerange
            stream.readHex(start, dataSize);
            stream.readHexNumber(end, dataSize);
            addHex(end, start, dataSize);
            cMap.addCodespaceRange(
              dataSize + 1,
              hexToInt(start, dataSize),
              hexToInt(end, dataSize)
            );
            for (let i = 1; i < subitemsCount; i++) {
              incHex(end, dataSize);
              stream.readHexNumber(start, dataSize);
              addHex(start, end, dataSize);
              stream.readHexNumber(end, dataSize);
              addHex(end, start, dataSize);
              cMap.addCodespaceRange(
                dataSize + 1,
                hexToInt(start, dataSize),
                hexToInt(end, dataSize)
              );
            }
            break;
          case 1: // notdefrange
            stream.readHex(start, dataSize);
            stream.readHexNumber(end, dataSize);
            addHex(end, start, dataSize);
            stream.readNumber(); // code
            // undefined range, skipping
            for (let i = 1; i < subitemsCount; i++) {
              incHex(end, dataSize);
              stream.readHexNumber(start, dataSize);
              addHex(start, end, dataSize);
              stream.readHexNumber(end, dataSize);
              addHex(end, start, dataSize);
              stream.readNumber(); // code
              // nop
            }
            break;
          case 2: // cidchar
            stream.readHex(char, dataSize);
            code = stream.readNumber();
            cMap.mapOne(hexToInt(char, dataSize), code);
            for (let i = 1; i < subitemsCount; i++) {
              incHex(char, dataSize);
              if (!sequence) {
                stream.readHexNumber(tmp, dataSize);
                addHex(char, tmp, dataSize);
              }
              code = stream.readSigned() + (code + 1);
              cMap.mapOne(hexToInt(char, dataSize), code);
            }
            break;
          case 3: // cidrange
            stream.readHex(start, dataSize);
            stream.readHexNumber(end, dataSize);
            addHex(end, start, dataSize);
            code = stream.readNumber();
            cMap.mapCidRange(
              hexToInt(start, dataSize),
              hexToInt(end, dataSize),
              code
            );
            for (let i = 1; i < subitemsCount; i++) {
              incHex(end, dataSize);
              if (!sequence) {
                stream.readHexNumber(start, dataSize);
                addHex(start, end, dataSize);
              } else {
                start.set(end);
              }
              stream.readHexNumber(end, dataSize);
              addHex(end, start, dataSize);
              code = stream.readNumber();
              cMap.mapCidRange(
                hexToInt(start, dataSize),
                hexToInt(end, dataSize),
                code
              );
            }
            break;
          case 4: // bfchar
            stream.readHex(char, ucs2DataSize);
            stream.readHex(charCode, dataSize);
            cMap.mapOne(
              hexToInt(char, ucs2DataSize),
              hexToStr(charCode, dataSize)
            );
            for (let i = 1; i < subitemsCount; i++) {
              incHex(char, ucs2DataSize);
              if (!sequence) {
                stream.readHexNumber(tmp, ucs2DataSize);
                addHex(char, tmp, ucs2DataSize);
              }
              incHex(charCode, dataSize);
              stream.readHexSigned(tmp, dataSize);
              addHex(charCode, tmp, dataSize);
              cMap.mapOne(
                hexToInt(char, ucs2DataSize),
                hexToStr(charCode, dataSize)
              );
            }
            break;
          case 5: // bfrange
            stream.readHex(start, ucs2DataSize);
            stream.readHexNumber(end, ucs2DataSize);
            addHex(end, start, ucs2DataSize);
            stream.readHex(charCode, dataSize);
            cMap.mapBfRange(
              hexToInt(start, ucs2DataSize),
              hexToInt(end, ucs2DataSize),
              hexToStr(charCode, dataSize)
            );
            for (let i = 1; i < subitemsCount; i++) {
              incHex(end, ucs2DataSize);
              if (!sequence) {
                stream.readHexNumber(start, ucs2DataSize);
                addHex(start, end, ucs2DataSize);
              } else {
                start.set(end);
              }
              stream.readHexNumber(end, ucs2DataSize);
              addHex(end, start, ucs2DataSize);
              stream.readHex(charCode, dataSize);
              cMap.mapBfRange(
                hexToInt(start, ucs2DataSize),
                hexToInt(end, ucs2DataSize),
                hexToStr(charCode, dataSize)
              );
            }
            break;
          default:
            throw new Error(`BinaryCMapReader.process - unknown type: ${type}`);
        }
      }

      if (useCMap) {
        return extend(useCMap);
      }
      return cMap;
    }
  }

  return BinaryCMapReader;
})();

const CMapFactory = (function CMapFactoryClosure() {
  function strToInt(str) {
    let a = 0;
    for (let i = 0; i < str.length; i++) {
      a = (a << 8) | str.charCodeAt(i);
    }
    return a >>> 0;
  }

  function expectString(obj) {
    if (typeof obj !== "string") {
      throw new FormatError("Malformed CMap: expected string.");
    }
  }

  function expectInt(obj) {
    if (!Number.isInteger(obj)) {
      throw new FormatError("Malformed CMap: expected int.");
    }
  }

  function parseBfChar(cMap, lexer) {
    while (true) {
      let obj = lexer.getObj();
      if (obj === EOF) {
        break;
      }
      if (isCmd(obj, "endbfchar")) {
        return;
      }
      expectString(obj);
      const src = strToInt(obj);
      obj = lexer.getObj();
      // TODO are /dstName used?
      expectString(obj);
      const dst = obj;
      cMap.mapOne(src, dst);
    }
  }

  function parseBfRange(cMap, lexer) {
    while (true) {
      let obj = lexer.getObj();
      if (obj === EOF) {
        break;
      }
      if (isCmd(obj, "endbfrange")) {
        return;
      }
      expectString(obj);
      const low = strToInt(obj);
      obj = lexer.getObj();
      expectString(obj);
      const high = strToInt(obj);
      obj = lexer.getObj();
      if (Number.isInteger(obj) || typeof obj === "string") {
        const dstLow = Number.isInteger(obj) ? String.fromCharCode(obj) : obj;
        cMap.mapBfRange(low, high, dstLow);
      } else if (isCmd(obj, "[")) {
        obj = lexer.getObj();
        const array = [];
        while (!isCmd(obj, "]") && obj !== EOF) {
          array.push(obj);
          obj = lexer.getObj();
        }
        cMap.mapBfRangeToArray(low, high, array);
      } else {
        break;
      }
    }
    throw new FormatError("Invalid bf range.");
  }

  function parseCidChar(cMap, lexer) {
    while (true) {
      let obj = lexer.getObj();
      if (obj === EOF) {
        break;
      }
      if (isCmd(obj, "endcidchar")) {
        return;
      }
      expectString(obj);
      const src = strToInt(obj);
      obj = lexer.getObj();
      expectInt(obj);
      const dst = obj;
      cMap.mapOne(src, dst);
    }
  }

  function parseCidRange(cMap, lexer) {
    while (true) {
      let obj = lexer.getObj();
      if (obj === EOF) {
        break;
      }
      if (isCmd(obj, "endcidrange")) {
        return;
      }
      expectString(obj);
      const low = strToInt(obj);
      obj = lexer.getObj();
      expectString(obj);
      const high = strToInt(obj);
      obj = lexer.getObj();
      expectInt(obj);
      const dstLow = obj;
      cMap.mapCidRange(low, high, dstLow);
    }
  }

  function parseCodespaceRange(cMap, lexer) {
    while (true) {
      let obj = lexer.getObj();
      if (obj === EOF) {
        break;
      }
      if (isCmd(obj, "endcodespacerange")) {
        return;
      }
      if (typeof obj !== "string") {
        break;
      }
      const low = strToInt(obj);
      obj = lexer.getObj();
      if (typeof obj !== "string") {
        break;
      }
      const high = strToInt(obj);
      cMap.addCodespaceRange(obj.length, low, high);
    }
    throw new FormatError("Invalid codespace range.");
  }

  function parseWMode(cMap, lexer) {
    const obj = lexer.getObj();
    if (Number.isInteger(obj)) {
      cMap.vertical = !!obj;
    }
  }

  function parseCMapName(cMap, lexer) {
    const obj = lexer.getObj();
    if (obj instanceof Name) {
      cMap.name = obj.name;
    }
  }

  async function parseCMap(cMap, lexer, fetchBuiltInCMap, useCMap) {
    let previous, embeddedUseCMap;
    objLoop: while (true) {
      try {
        const obj = lexer.getObj();
        if (obj === EOF) {
          break;
        } else if (obj instanceof Name) {
          if (obj.name === "WMode") {
            parseWMode(cMap, lexer);
          } else if (obj.name === "CMapName") {
            parseCMapName(cMap, lexer);
          }
          previous = obj;
        } else if (obj instanceof Cmd) {
          switch (obj.cmd) {
            case "endcmap":
              break objLoop;
            case "usecmap":
              if (previous instanceof Name) {
                embeddedUseCMap = previous.name;
              }
              break;
            case "begincodespacerange":
              parseCodespaceRange(cMap, lexer);
              break;
            case "beginbfchar":
              parseBfChar(cMap, lexer);
              break;
            case "begincidchar":
              parseCidChar(cMap, lexer);
              break;
            case "beginbfrange":
              parseBfRange(cMap, lexer);
              break;
            case "begincidrange":
              parseCidRange(cMap, lexer);
              break;
          }
        }
      } catch (ex) {
        if (ex instanceof MissingDataException) {
          throw ex;
        }
        warn("Invalid cMap data: " + ex);
        continue;
      }
    }

    if (!useCMap && embeddedUseCMap) {
      // Load the useCMap definition from the file only if there wasn't one
      // specified.
      useCMap = embeddedUseCMap;
    }
    if (useCMap) {
      return extendCMap(cMap, fetchBuiltInCMap, useCMap);
    }
    return cMap;
  }
})();

var buffer = fs.readFileSync(
  `C:\\source\\Github\\pdflexer\\src\\PdfLexer.CMaps\\Resources\\Adobe-Japan1-UCS2.bcmap`,
  null
);
var reader = new BinaryCMapReader();
reader.process(buffer, new CMap(), null).then((x) => {
  let i = 0;
  x._map.map((c) => {
    if (c) {
      i++;
    }
  });
  console.log(x);
  console.log(i);
});
