using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Yali.Utils
{
    /// <license>
    /// Copyright 2007 Richard Prinz
    ///
    /// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
    ///
    /// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
    ///
    /// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
    /// </license>
    /// <summary>
    /// Source: https://www.codeproject.com/Articles/19274/A-printf-implementation-in-C
    /// </summary>
    internal static class InternalStringUtils
    {
        #region Public Methods
        #region IsNumericType
        /// <summary>
        /// Determines whether the specified value is of numeric type.
        /// </summary>
        /// <param name="o">The object to check.</param>
        /// <returns>
        /// 	<c>true</c> if o is a numeric type; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNumericType(object o)
        {
            return o is byte ||
                   o is sbyte ||
                   o is short ||
                   o is ushort ||
                   o is int ||
                   o is uint ||
                   o is long ||
                   o is ulong ||
                   o is float ||
                   o is double ||
                   o is decimal;
        }
        #endregion
        #region IsPositive
        /// <summary>
        /// Determines whether the specified value is positive.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="zeroIsPositive">if set to <c>true</c> treats 0 as positive.</param>
        /// <returns>
        /// 	<c>true</c> if the specified value is positive; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsPositive(object value, bool zeroIsPositive)
        {
            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.SByte:
                    return zeroIsPositive ? (sbyte)value >= 0 : (sbyte)value > 0;
                case TypeCode.Int16:
                    return zeroIsPositive ? (short)value >= 0 : (short)value > 0;
                case TypeCode.Int32:
                    return zeroIsPositive ? (int)value >= 0 : (int)value > 0;
                case TypeCode.Int64:
                    return zeroIsPositive ? (long)value >= 0 : (long)value > 0;
                case TypeCode.Single:
                    return zeroIsPositive ? (float)value >= 0 : (float)value > 0;
                case TypeCode.Double:
                    return zeroIsPositive ? (double)value >= 0 : (double)value > 0;
                case TypeCode.Decimal:
                    return zeroIsPositive ? (decimal)value >= 0 : (decimal)value > 0;
                case TypeCode.Byte:
                    return zeroIsPositive || (byte)value > 0;
                case TypeCode.UInt16:
                    return zeroIsPositive || (ushort)value > 0;
                case TypeCode.UInt32:
                    return zeroIsPositive || (uint)value > 0;
                case TypeCode.UInt64:
                    return zeroIsPositive || (ulong)value > 0;
                case TypeCode.Char:
                    return zeroIsPositive || (char)value != '\0';
                default:
                    return false;
            }
        }
        #endregion
        #region ToUnsigned
        /// <summary>
        /// Converts the specified values boxed type to its correpsonding unsigned
        /// type.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A boxed numeric object whos type is unsigned.</returns>
        public static object ToUnsigned(object value)
        {
            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.SByte:
                    return (byte)(sbyte)value;
                case TypeCode.Int16:
                    return (ushort)(short)value;
                case TypeCode.Int32:
                    return (uint)(int)value;
                case TypeCode.Int64:
                    return (ulong)(long)value;

                case TypeCode.Byte:
                    return value;
                case TypeCode.UInt16:
                    return value;
                case TypeCode.UInt32:
                    return value;
                case TypeCode.UInt64:
                    return value;

                case TypeCode.Single:
                    return (uint)(float)value;
                case TypeCode.Double:
                    try
                    {
                        return (uint) Convert.ToInt32(value);
                    }
                    catch
                    {
                        return (ulong)(double)value;
                    }
                case TypeCode.Decimal:
                    return (ulong)(decimal)value;

                default:
                    return null;
            }
        }
        #endregion
        #region UnboxToLong
        public static int UnboxToLong(object value, bool round)
        {
            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.SByte:
                    return (sbyte)value;
                case TypeCode.Int16:
                    return (short)value;
                case TypeCode.Int32:
                    return (int)value;
                case TypeCode.Int64:
                    return (int)(long)value;

                case TypeCode.Byte:
                    return (byte)value;
                case TypeCode.UInt16:
                    return (ushort)value;
                case TypeCode.UInt32:
                    return (int)(uint)value;
                case TypeCode.UInt64:
                    return (int)(ulong)value;

                case TypeCode.Single:
                    throw new NotSupportedException();
                case TypeCode.Double:
                    try
                    {
                        return Convert.ToInt32(round ? Math.Round((double)value) : (double)value);
                    }
                    catch
                    {
                        return round ? (int)Math.Round((double)value) : (int)(double)value;
                    }
                case TypeCode.Decimal:
                    throw new NotSupportedException();

                default:
                    return 0;
            }
        }
        #endregion
        #region ReplaceMetaChars
        private static string ReplaceMetaCharsMatch(Match m)
        {
            // convert octal quotes (like \040)
            if (m.Groups[2].Length == 3)
                return Convert.ToChar(Convert.ToByte(m.Groups[2].Value, 8)).ToString();
            else
            {
                // convert all other special meta characters
                //TODO: \xhhh hex and possible dec !!
                switch (m.Groups[2].Value)
                {
                    case "0":           // null
                        return "\0";
                    case "a":           // alert (beep)
                        return "\a";
                    case "b":           // BS
                        return "\b";
                    case "f":           // FF
                        return "\f";
                    case "v":           // vertical tab
                        return "\v";
                    case "r":           // CR
                        return "\r";
                    case "n":           // LF
                        return "\n";
                    case "t":           // Tab
                        return "\t";
                    default:
                        // if neither an octal quote nor a special meta character
                        // so just remove the backslash
                        return m.Groups[2].Value;
                }
            }
        }
        #endregion
        #region sprintf
        public static string Sprintf(string format, params object[] parameters)
        {
            #region Variables
            var f = new StringBuilder();
            var r = new Regex(@"\%(\d*\$)?([\'\#\-\+ ]*)(\d*)(?:\.(\d+))?([hl])?([dioxXucsfeEaAgGpnq%])");
            //"%[parameter][flags][width][.precision][length]type"
            Match m = null;
            var w = string.Empty;
            var defaultParamIx = 0;
            int paramIx;
            object o = null;

            var flagLeft2Right = false;
            var flagAlternate = false;
            var flagPositiveSign = false;
            var flagPositiveSpace = false;
            var flagZeroPadding = false;
            var flagGroupThousands = false;

            var fieldLength = 0;
            var fieldPrecision = 0;
            var shortLongIndicator = '\0';
            var formatSpecifier = '\0';
            var paddingCharacter = ' ';
            #endregion

            // find all format parameters in format string
            f.Append(format);
            m = r.Match(f.ToString());
            while (m.Success)
            {
                #region parameter index
                paramIx = defaultParamIx;
                if (m.Groups[1] != null && m.Groups[1].Value.Length > 0)
                {
                    var val = m.Groups[1].Value.Substring(0, m.Groups[1].Value.Length - 1);
                    paramIx = Convert.ToInt32(val) - 1;
                };
                #endregion

                #region format flags
                // extract format flags
                flagAlternate = false;
                flagLeft2Right = false;
                flagPositiveSign = false;
                flagPositiveSpace = false;
                flagZeroPadding = false;
                flagGroupThousands = false;
                if (m.Groups[2] != null && m.Groups[2].Value.Length > 0)
                {
                    var flags = m.Groups[2].Value;

                    flagAlternate = flags.IndexOf('#') >= 0;
                    flagLeft2Right = flags.IndexOf('-') >= 0;
                    flagPositiveSign = flags.IndexOf('+') >= 0;
                    flagPositiveSpace = flags.IndexOf(' ') >= 0;
                    flagGroupThousands = flags.IndexOf('\'') >= 0;

                    // positive + indicator overrides a
                    // positive space character
                    if (flagPositiveSign && flagPositiveSpace)
                        flagPositiveSpace = false;
                }
                #endregion

                #region field length
                // extract field length and 
                // pading character
                paddingCharacter = ' ';
                fieldLength = int.MinValue;
                if (m.Groups[3] != null && m.Groups[3].Value.Length > 0)
                {
                    fieldLength = Convert.ToInt32(m.Groups[3].Value);
                    flagZeroPadding = m.Groups[3].Value[0] == '0';
                }
                #endregion

                if (flagZeroPadding)
                    paddingCharacter = '0';

                // left2right allignment overrides zero padding
                if (flagLeft2Right && flagZeroPadding)
                {
                    paddingCharacter = ' ';
                }

                #region field precision
                // extract field precision
                fieldPrecision = int.MinValue;
                if (m.Groups[4] != null && m.Groups[4].Value.Length > 0)
                    fieldPrecision = Convert.ToInt32(m.Groups[4].Value);
                #endregion

                #region short / long indicator
                // extract short / long indicator
                shortLongIndicator = char.MinValue;
                if (m.Groups[5] != null && m.Groups[5].Value.Length > 0)
                    shortLongIndicator = m.Groups[5].Value[0];
                #endregion

                #region format specifier
                // extract format
                formatSpecifier = char.MinValue;
                if (m.Groups[6] != null && m.Groups[6].Value.Length > 0)
                    formatSpecifier = m.Groups[6].Value[0];
                #endregion

                // default precision is 6 digits if none is specified except
                if (fieldPrecision == int.MinValue &&
                    formatSpecifier != 's' &&
                    formatSpecifier != 'q' &&
                    formatSpecifier != 'c' &&
                    char.ToUpper(formatSpecifier) != 'X' &&
                    formatSpecifier != 'o')
                    fieldPrecision = 6;

                #region get next value parameter
                // get next value parameter and convert value parameter depending on short / long indicator
                if (parameters == null || paramIx >= parameters.Length)
                    o = null;
                else
                {
                    o = parameters[paramIx];

                    if (shortLongIndicator == 'h')
                    {
                        if (o is int)
                            o = (short)(int)o;
                        else if (o is long)
                            o = (short)(long)o;
                        else if (o is uint)
                            o = (ushort)(uint)o;
                        else if (o is ulong)
                            o = (ushort)(ulong)o;
                    }
                    else if (shortLongIndicator == 'l')
                    {
                        if (o is short)
                            o = (long)(short)o;
                        else if (o is int)
                            o = (long)(int)o;
                        else if (o is ushort)
                            o = (ulong)(ushort)o;
                        else if (o is uint)
                            o = (ulong)(uint)o;
                    }
                }
                #endregion

                // convert value parameters to a string depending on the formatSpecifier
                w = string.Empty;
                switch (formatSpecifier)
                {
                    #region % - character
                    case '%':   // % character
                        w = "%";
                        break;
                    #endregion
                    #region d - integer
                    case 'd':   // integer
                        w = FormatNumber(flagGroupThousands ? "n" : "d",
                                        fieldLength, int.MinValue, flagLeft2Right,
                                        flagPositiveSign, flagPositiveSpace,
                                        paddingCharacter, Convert.ToInt32(o));
                        defaultParamIx++;
                        break;
                    #endregion
                    #region i - integer
                    case 'i':   // integer
                        goto case 'd';
                    #endregion
                    #region o - octal integer
                    case 'o':   // octal integer - no leading zero
                        w = FormatOct(flagAlternate,
                                        fieldLength, flagLeft2Right,
                                        paddingCharacter, o);
                        defaultParamIx++;
                        break;
                    #endregion

                    case 'a':
                        throw new NotImplementedException();
                    case 'A':
                        throw new NotImplementedException();

                    #region x - hex integer
                    case 'x':   // hex integer - no leading zero
                        w = FormatHex("x", flagAlternate,
                                        fieldLength, fieldPrecision, flagLeft2Right,
                                        paddingCharacter, Convert.ToInt32(o));
                        defaultParamIx++;
                        break;
                    #endregion
                    #region X - hex integer
                    case 'X':   // same as x but with capital hex characters
                        w = FormatHex("X", flagAlternate,
                                        fieldLength, fieldPrecision, flagLeft2Right,
                                        paddingCharacter, Convert.ToInt32(o));
                        defaultParamIx++;
                        break;
                    #endregion
                    #region u - unsigned integer
                    case 'u':   // unsigned integer
                        w = FormatNumber(flagGroupThousands ? "n" : "d",
                                        fieldLength, int.MinValue, flagLeft2Right,
                                        false, false,
                                        paddingCharacter, ToUnsigned(o));
                        defaultParamIx++;
                        break;
                    #endregion
                    #region c - character
                    case 'c':   // character
                        if (IsNumericType(o))
                            w = Convert.ToChar(Convert.ToInt32(o)).ToString();
                        else if (o is char c)
                            w = c.ToString();
                        else if (o is string s && s.Length > 0)
                            w = s[0].ToString();
                        defaultParamIx++;
                        break;
                    #endregion
                    #region s - string
                    case 'q':   // quoted string
                    case 's':   // string
                        w = o.ToString();
                        if (fieldPrecision >= 0)
                            w = w.Substring(0, fieldPrecision);

                        if (fieldLength != int.MinValue)
                            if (flagLeft2Right)
                                w = w.PadRight(fieldLength, paddingCharacter);
                            else
                                w = w.PadLeft(fieldLength, paddingCharacter);

                        if (formatSpecifier == 'q')
                            w = '"' + w + '"';

                        defaultParamIx++;
                        break;
                    #endregion
                    #region f - double number
                    case 'f':   // double
                        w = FormatNumber(flagGroupThousands ? "n" : "f",
                                        fieldLength, fieldPrecision, flagLeft2Right,
                                        flagPositiveSign, flagPositiveSpace,
                                        paddingCharacter, o);
                        defaultParamIx++;
                        break;
                    #endregion
                    #region e - exponent number
                    case 'e':   // double / exponent
                        w = FormatNumber("e",
                                        fieldLength, fieldPrecision, flagLeft2Right,
                                        flagPositiveSign, flagPositiveSpace,
                                        paddingCharacter, o);
                        defaultParamIx++;
                        break;
                    #endregion
                    #region E - exponent number
                    case 'E':   // double / exponent
                        w = FormatNumber("E",
                                        fieldLength, fieldPrecision, flagLeft2Right,
                                        flagPositiveSign, flagPositiveSpace,
                                        paddingCharacter, o);
                        defaultParamIx++;
                        break;
                    #endregion
                    #region g - general number
                    case 'g':   // double / exponent
                        w = FormatNumber("g",
                                        fieldLength, fieldPrecision, flagLeft2Right,
                                        flagPositiveSign, flagPositiveSpace,
                                        paddingCharacter, o);
                        defaultParamIx++;
                        break;
                    #endregion
                    #region G - general number
                    case 'G':   // double / exponent
                        w = FormatNumber("G",
                                        fieldLength, fieldPrecision, flagLeft2Right,
                                        flagPositiveSign, flagPositiveSpace,
                                        paddingCharacter, o);
                        defaultParamIx++;
                        break;
                    #endregion
                    #region p - pointer
                    case 'p':   // pointer
                        if (o is IntPtr)
                            w = "0x" + ((IntPtr)o).ToString("x");
                        defaultParamIx++;
                        break;
                    #endregion
                    #region n - number of processed chars so far
                    case 'n':   // number of characters so far
                        w = FormatNumber("d",
                                        fieldLength, int.MinValue, flagLeft2Right,
                                        flagPositiveSign, flagPositiveSpace,
                                        paddingCharacter, m.Index);
                        break;
                    #endregion
                    default:
                        w = string.Empty;
                        defaultParamIx++;
                        break;
                }

                // replace format parameter with parameter value
                // and start searching for the next format parameter
                // AFTER the position of the current inserted value
                // to prohibit recursive matches if the value also
                // includes a format specifier
                f.Remove(m.Index, m.Length);
                f.Insert(m.Index, w);
                m = r.Match(f.ToString(), m.Index + w.Length);
            }

            return f.ToString();
        }
        #endregion
        #endregion

        #region Private Methods
        #region FormatOCT
        private static string FormatOct(bool alternate,
                                            int fieldLength,
                                            bool left2Right,
                                            char padding, object value)
        {
            var w = string.Empty;
            var lengthFormat = "{0" + (fieldLength != int.MinValue ?
                                            "," + (left2Right ?
                                                    "-" :
                                                    string.Empty) + fieldLength :
                                            string.Empty) + "}";

            if (IsNumericType(value))
            {
                w = Convert.ToString(UnboxToLong(value, true), 8);

                if (left2Right || padding == ' ')
                {
                    if (alternate && w != "0")
                        w = "0" + w;
                    w = string.Format(lengthFormat, w);
                }
                else
                {
                    if (fieldLength != int.MinValue)
                        w = w.PadLeft(fieldLength - (alternate && w != "0" ? 1 : 0), padding);
                    if (alternate && w != "0")
                        w = "0" + w;
                }
            }

            return w;
        }
        #endregion
        #region FormatHEX
        public static string Float2Hex(double fNum)
        {
            MemoryStream ms = new MemoryStream(sizeof(float));
            StreamWriter sw = new StreamWriter(ms);

            // Write the float to the stream
            sw.Write(fNum);
            sw.Flush();

            // Re-read the stream
            ms.Seek(0, SeekOrigin.Begin);
            byte[] buffer = new byte[4];
            ms.Read(buffer, 0, 4);

            // Convert the buffer to Hex
            StringBuilder sb = new StringBuilder();
            foreach (byte b in buffer)
                sb.AppendFormat("{0:X2}", b);

            sw.Close();

            return sb.ToString();
        }

        private static string FormatHex(string nativeFormat, bool alternate,
                                            int fieldLength, int fieldPrecision,
                                            bool left2Right,
                                            char padding, object value)
        {
            var w = string.Empty;
            var lengthFormat = "{0" + (fieldLength != int.MinValue ?
                                            "," + (left2Right ?
                                                    "-" :
                                                    string.Empty) + fieldLength :
                                            string.Empty) + "}";
            var numberFormat = "{0:" + nativeFormat + (fieldPrecision != int.MinValue ?
                                            fieldPrecision.ToString() :
                                            string.Empty) + "}";

            if (IsNumericType(value))
            {
                w = string.Format(numberFormat, value);

                if (left2Right || padding == ' ')
                {
                    if (alternate)
                        w = (nativeFormat == "x" ? "0x" : "0X") + w;
                    w = string.Format(lengthFormat, w);
                }
                else
                {
                    if (fieldLength != int.MinValue)
                        w = w.PadLeft(fieldLength - (alternate ? 2 : 0), padding);
                    if (alternate)
                        w = (nativeFormat == "x" ? "0x" : "0X") + w;
                }
            }

            return w;
        }
        #endregion
        #region FormatNumber
        private static string FormatNumber(string nativeFormat,
                                            int fieldLength, int fieldPrecision,
                                            bool left2Right,
                                            bool positiveSign, bool positiveSpace,
                                            char padding, object value)
        {
            var w = string.Empty;
            var lengthFormat = "{0" + (fieldLength != int.MinValue ?
                                            "," + (left2Right ?
                                                    "-" :
                                                    string.Empty) + fieldLength :
                                            string.Empty) + "}";
            var numberFormat = "{0:" + nativeFormat + (fieldPrecision != int.MinValue ?
                                            fieldPrecision.ToString() :
                                            "0") + "}";

            if (IsNumericType(value))
            {
                w = string.Format(numberFormat, value);

                if (left2Right || padding == ' ')
                {
                    if (IsPositive(value, true))
                        w = (positiveSign ?
                                "+" : (positiveSpace ? " " : string.Empty)) + w;
                    w = string.Format(lengthFormat, w);
                }
                else
                {
                    if (w.StartsWith("-"))
                        w = w.Substring(1);
                    if (fieldLength != int.MinValue)
                        w = w.PadLeft(fieldLength - 1, padding);
                    if (IsPositive(value, true))
                        w = (positiveSign ?
                                "+" : (positiveSpace ?
                                        " " : (fieldLength != int.MinValue ?
                                                padding.ToString() : string.Empty))) + w;
                    else
                        w = "-" + w;
                }
            }

            return w;
        }
        #endregion
        #endregion
    }
}
