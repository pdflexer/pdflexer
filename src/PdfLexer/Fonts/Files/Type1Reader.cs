using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.Fonts.Files
{
    // Type1Reader PORTED FROM PDF.JS, PDF.JS is licensed as follows:
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

    /// <summary>
    /// 
    /// 
    /// </summary>
    internal ref struct Type1Reader
    {
        public static bool IsType1File(ReadOnlySpan<byte> data)
        {
            // All Type1 font programs must begin with the comment '%!' (0x25 + 0x21).
            if (data[0] == 0x25 && data[1] == 0x21)
            {
                return true;
            }
            // ... obviously some fonts violate that part of the specification,
            // please refer to the comment in |Type1Font| below (pfb file header).
            if (data[0] == 0x80 && data[1] == 0x01)
            {
                return true;
            }
            return false;
        }
    }
}
