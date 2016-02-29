using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RateGain.Util
{
    /// <summary>
    /// 表示字符串实用工具类
    /// </summary>
    public static class StringUtils
    {
        /// <summary>
        /// 从字符串值开始和末尾移除所有空白字符后保留的字符串，如果字符串值为空或<c>null</c>则返回空
        /// </summary>
        /// <param name="value">字符串值</param>
        /// <returns>从字符串值开始和末尾移除所有空白字符后保留的字符串</returns>
        public static string Trim(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value.Trim();
        }

        /// <summary>
        /// 将字符串值去除首尾空格转换为指定类型的值，如果字符串值为空或null，则返回值类型对应的默认值
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="value">字符串值</param>
        /// <returns>如果能转换则返回转换后值类型的值，否则返回指定的默认值</returns>
        public static T ConvertValue<T>(string value) where T : struct
        {
            return ConvertValue(value, default(T));
        }

        /// <summary>
        /// 将字符串值去除首尾空格转换为指定类型的值，如果字符串值为空或null，则返回缺省值
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="value">字符串值</param>
        /// <param name="defaultValue">缺省值</param>
        /// <returns>如果能转换则返回转换后值类型的值，否则返回指定的默认值</returns>
        public static T ConvertValue<T>(string value, T defaultValue) where T : struct
        {
            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    value = value.Trim();

                    if (value == string.Empty)
                    {
                        return defaultValue;
                    }

                    var type = typeof(T);
                    if (type == typeof(bool) && (value == "1" || value == "0"))
                    {
                        value = (value == "1") ? "True" : "False";
                    }

                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                    return defaultValue;
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// 将字符串值去除首尾空格转换为指定类型的值，如果字符串值为空或null，则返回返回<c>null</c>
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="value">字符串值</param>
        /// <returns>如果能转换则返回转换后值类型的值，否则返回<c>null</c></returns>
        public static T? ConvertNullableValue<T>(string value) where T : struct
        {
            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    if (value == string.Empty)
                    {
                        return null;
                    }

                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }

        /// <summary>
        /// Read all the lines in the string.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static IList<string> ReadLines(string text)
        {
            // Check for empty and return empty list.
            if (string.IsNullOrEmpty(text))
                return new List<string>();

            var reader = new StringReader(text);
            var currentLine = reader.ReadLine();
            IList<string> lines = new List<string>();

            // More to read.
            while (currentLine != null)
            {
                lines.Add(currentLine);
                currentLine = reader.ReadLine();
            }
            return lines;
        }

        /// <summary>
        /// Join string enumeration other.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="delimeter"></param>
        /// <returns></returns>
        public static string Join(IList<string> items, string delimeter)
        {
            var joined = "";
            if (items != null)
            {
                int ndx;
                for (ndx = 0; ndx < items.Count - 1; ndx++)
                {
                    joined += items[ndx] + delimeter;
                }
                joined += items[ndx];
            }
            return joined;
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        public static string ConvertToString(object[] args)
        {
            if (args == null || args.Length == 0)
                return string.Empty;

            var buffer = new StringBuilder();
            foreach (var arg in args)
            {
                if (arg != null)
                    buffer.Append(arg);
            }
            return buffer.ToString();
        }

        /// <summary>
        /// Parses a delimited list of other into a string[].
        /// </summary>
        /// <param name="delimitedText">"1,2,3,4,5,6"</param>
        /// <param name="delimeter">','</param>
        /// <returns></returns>
        public static string[] ToStringArray(string delimitedText, char delimeter)
        {
            if (string.IsNullOrEmpty(delimitedText))
                return null;

            var tokens = delimitedText.Split(delimeter);
            return tokens;
        }

        /// <summary>
        /// Get the index of a spacer ( space" " or newline )
        /// </summary>
        /// <param name="txt"></param>
        /// <param name="currentPosition"></param>
        /// <returns></returns>
        public static int GetIndexOfSpacer(string txt, int currentPosition)
        {
            var isNewLine = false;
            return GetIndexOfSpacer(txt, currentPosition, out isNewLine);
        }

        /// <summary>
        /// Get the index of a spacer ( space" " or newline )
        /// </summary>
        /// <param name="txt"></param>
        /// <param name="currentPosition"></param>
        /// <param name="isNewLine"></param>
        /// <returns></returns>
        public static int GetIndexOfSpacer(string txt, int currentPosition, out bool isNewLine)
        {
            // Take the first spacer that you find. it could be eithr
            // space or newline, if space is before the newline take space
            // otherwise newline.            
            var ndxSpace = txt.IndexOf(" ", currentPosition, StringComparison.Ordinal);
            var ndxNewLine = txt.IndexOf(Environment.NewLine, currentPosition, StringComparison.Ordinal);
            var hasSpace = ndxSpace > -1;
            var hasNewLine = ndxNewLine > -1;
            isNewLine = false;

            // Found both space and newline.
            if (hasSpace && hasNewLine)
            {
                if (ndxSpace < ndxNewLine) { return ndxSpace; }
                isNewLine = true;
                return ndxNewLine;
            }

            // Found space only.
            if (hasSpace) { return ndxSpace; }

            // Found newline only.
            if (hasNewLine) { isNewLine = true; return ndxNewLine; }

            // no space or newline.
            return -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="asciiCode"></param>
        /// <returns></returns>
        public static string AsciiToUnicode(int asciiCode)
        {
            var ascii = Encoding.UTF32;
            var c = (char)asciiCode;
            var b = ascii.GetBytes(c.ToString());
            return ascii.GetString((b));
        }

        /// <summary>
        /// 从一个字符串末尾移除指定数目的字符
        /// </summary>
        /// <param name="s"></param>
        /// <param name="removeFromEnd"></param>
        /// <returns></returns>
        public static string Chop(string s, int removeFromEnd)
        {
            var result = s;
            if (s.Length > removeFromEnd - 1)
                result = result.Remove(s.Length - removeFromEnd, removeFromEnd);
            return result;
        }

        /// <summary>
        /// 从一个字符串移除字符backDownTo最后一个匹配项的索引位置开始的所有后面的字符
        /// </summary>
        /// <param name="s"></param>
        /// <param name="backDownTo"></param>
        /// <returns></returns>
        public static string Chop(string s, string backDownTo)
        {

            var removeDownTo = s.LastIndexOf(backDownTo, StringComparison.Ordinal);
            var removeFromEnd = 0;
            if (removeDownTo > 0)
                removeFromEnd = s.Length - removeDownTo;

            var result = s;
            if (s.Length > removeFromEnd - 1)
                result = result.Remove(removeDownTo, removeFromEnd);
            return result;
        }

        /// <summary>
        /// 从一个字符串首移除指定数目的字符
        /// </summary>
        /// <param name="s"></param>
        /// <param name="removeFromBeginning"></param>
        /// <returns></returns>
        public static string Clip(string s, int removeFromBeginning)
        {
            var result = s;
            if (s.Length > removeFromBeginning)
                result = result.Remove(0, removeFromBeginning);
            return result;
        }

        /// <summary>
        /// 从一个字符串移除字符removeUpTo第一个匹配项的索引位置开始的所有前面的字符
        /// </summary>
        /// <param name="s"></param>
        /// <param name="removeUpTo"></param>
        /// <returns></returns>
        public static string Clip(string s, string removeUpTo)
        {
            var removeFromBeginning = s.IndexOf(removeUpTo, StringComparison.Ordinal);
            var result = s;
            if (s.Length > removeFromBeginning && removeFromBeginning > 0)
                result = result.Remove(0, removeFromBeginning);
            return result;
        }

        /// <summary>
        /// 从字符中中移除最后一个字符
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        public static string Chop(string s)
        {
            return Chop(s, 1);

        }

        /// <summary>
        /// 从字符中中移除第一个字符
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        public static string Clip(string s)
        {
            return Clip(s, 1);

        }
        /// <summary>
        /// 返回位于文本startText和endText之间的字符
        /// </summary>
        /// <param name="input"></param>
        /// <param name="startText">The text from which to start the crop</param>
        /// <param name="endText">The endpoint of the crop</param>
        /// <returns></returns>
        public static string Crop(string input, string startText, string endText)
        {
            var sIn = input;
            string sOut;
            var cropStart = sIn.ToLower().IndexOf(startText.ToLower(), StringComparison.Ordinal);

            try
            {
                sIn = sIn.Remove(0, cropStart);
                sIn = sIn.Replace(startText, "");
                var cropEnd = sIn.ToLower().IndexOf(endText.ToLower(), StringComparison.Ordinal);

                sOut = sIn.Substring(0, cropEnd);

            }
            catch
            {
                sOut = "";
            }
            return sOut;
        }

        /// <summary>
        /// 移除单词之间的多余空格，即使单词之间只留一个空格
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Squeeze(string input)
        {
            char[] delim = { ' ' };
            var lines = input.Split(delim, StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder();
            foreach (var s in lines)
            {
                if (!String.IsNullOrEmpty(s.Trim()))
                {
                    sb.Append(s + " ");
                }
            }
            //remove the last pipe
            var result = Chop(sb.ToString());
            return result.Trim();
        }

        /// <summary>
        /// 创建一个基于词在句中的字符串数组
        /// </summary>
        /// <param name="s">The string to parse</param>
        /// <returns></returns>
        public static string[] ToWords(string s)
        {
            var result = s.Trim();
            return result.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// 清除给定字符串中的回车及换行符
        /// </summary>
        /// <param name="str">要清除的字符串</param>
        /// <returns>清除后返回的字符串</returns>
        public static string ClearBr(string str)
        {
            Match m;

            var r = new Regex(@"(\r\n)", RegexOptions.IgnoreCase);
            for (m = r.Match(str); m.Success; m = m.NextMatch())
            {
                str = str.Replace(m.Groups[0].ToString(), "");
            }


            return str;
        }

        /// <summary>
        /// 将字符文本input中包含字符stripValue中的以逗号分隔的字符过滤掉 
        /// </summary>
        /// <param name="input">The input text</param>
        /// <param name="stripValue">The strip value.</param>
        /// <returns></returns>
        public static string Strip(string input, string stripValue)
        {
            if (!String.IsNullOrEmpty(stripValue))
            {
                var replace = stripValue.Split(new[] { ',' });
                for (var i = 0; i < replace.Length; i++)
                {
                    if (!String.IsNullOrEmpty(input))
                    {
                        input = Regex.Replace(input, replace[i], String.Empty);
                    }
                }
            }
            return input;
        }

        /// <summary>
        /// 将字符list以", " 或 ","分隔为数组
        /// </summary>
        /// <param name="list">A list of values separated by either ", " or ","</param>
        public static string[] Split(string list)
        {
            string[] find;
            try
            {
                find = list.Split(new[] { ", ", "," }, StringSplitOptions.RemoveEmptyEntries);
            }
            catch
            {
                find = new[] { string.Empty };
            }
            return find;
        }

        /// <summary>
        /// 将字符strContent以strSplit分隔为数组
        /// </summary>
        public static string[] SplitString(string strContent, string strSplit)
        {
            if (!string.IsNullOrEmpty(strContent))
            {
                if (strContent.IndexOf(strSplit, StringComparison.Ordinal) < 0)
                {
                    string[] tmp = { strContent };
                    return tmp;
                }
                return Regex.Split(strContent, Regex.Escape(strSplit), RegexOptions.IgnoreCase);
            }
            else
            {
                return new string[0] { };
            }
        }

        /// <summary>
        /// 将字符strContent以strSplit分隔为指定维数的数组，多余的移除，不足以空字符填充数组
        /// </summary>
        /// <returns></returns>
        public static string[] SplitString(string strContent, string strSplit, int count)
        {
            var result = new string[count];

            var splited = SplitString(strContent, strSplit);

            for (var i = 0; i < count; i++)
            {
                if (i < splited.Length)
                    result[i] = splited[i];
                else
                    result[i] = string.Empty;
            }

            return result;
        }

        /// <summary>
        /// 快速查找替换
        /// </summary>
        /// <param name="original"></param>
        /// <param name="pattern"></param>
        /// <param name="replacement"></param>
        /// <param name="comparisonType"></param>
        /// <returns></returns>
        public static string FastReplace(string original, string pattern, string replacement, StringComparison comparisonType)
        {
            if (original == null)
            {
                return null;
            }

            if (String.IsNullOrEmpty(pattern))
            {
                return original;
            }

            var lenPattern = pattern.Length;
            var idxPattern = -1;
            var idxLast = 0;

            var result = new StringBuilder();

            while (true)
            {
                idxPattern = original.IndexOf(pattern, idxPattern + 1, comparisonType);

                if (idxPattern < 0)
                {
                    result.Append(original, idxLast, original.Length - idxLast);
                    break;
                }

                result.Append(original, idxLast, idxPattern - idxLast);
                result.Append(replacement);

                idxLast = idxPattern + lenPattern;
            }

            return result.ToString();
        }

        /// <summary>
        /// 替换任何匹配find的词为replaceWith
        /// </summary>
        /// <param name="word">The string to check against.</param>
        /// <param name="find">A comma separated list of values to replace.</param>
        /// <param name="replaceWith">The value to replace with.</param>
        /// <param name="removeUnderscores">Whether or not underscores will be kept.</param>
        public static string Replace(string word, string find, string replaceWith, bool removeUnderscores)
        {
            var findList = Split(find);
            var newWord = word;
            foreach (var f in findList)
                if (f.Length > 0)
                    newWord = newWord.Replace(f, replaceWith);
            if (removeUnderscores)
                return newWord.Replace(" ", "").Replace("_", "").Trim();
            else
                return newWord.Replace(" ", "").Trim();
        }

        /// <summary>
        /// 是否在Word中能匹配list中使用逗号分隔的字符
        /// </summary>
        /// <param name="word">The string to check against.</param>
        /// <param name="list">A comma separted list of values to find.</param>
        /// <returns>true if a match is found or list is empty, otherwise false.</returns>
        public static bool StartsWith(string word, string list)
        {
            if (string.IsNullOrEmpty(list))
                return true;

            var find = Split(list);
            foreach (var f in find)
                if (word.StartsWith(f, StringComparison.CurrentCultureIgnoreCase))
                    return true;
            return false;
        }

        /// <summary>
        /// 返回字符串字节长度, 1个汉字长度为2
        /// </summary>
        /// <returns></returns>
        public static int GetStringLength(string str)
        {
            return Encoding.Default.GetBytes(str).Length;
        }

        /// <summary>
        /// 按字节长度截取指定长度的字符串
        /// </summary>
        /// <param name="rawString"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public static string Trim(string rawString, int maxLength)
        {
            return Trim(rawString, maxLength, "...");
        }

        /// <summary>
        /// 按字节长度截取指定长度的字符串
        /// </summary>
        /// <param name="rawString">待截取的字符串</param>
        /// <param name="maxLength">截取长度</param>
        /// <param name="tailString">添加的后缀字符串</param>
        /// <returns>返回截取后的字符串</returns>
        public static string Trim(string rawString, int maxLength, string tailString)
        {
            if (string.IsNullOrEmpty(rawString))
            {
                return rawString;
            }

            var result = string.Empty;// 最终返回的结果
            var byteLen = System.Text.Encoding.Default.GetByteCount(rawString);// 单字节字符长度
            var charLen = rawString.Length;// 把字符平等对待时的字符串长度
            var byteCount = 0;// 记录读取进度
            var pos = 0;// 记录截取位置
            if (byteLen > maxLength)
            {
                for (var i = 0; i < charLen; i++)
                {
                    if (Convert.ToInt32(rawString.ToCharArray()[i]) > 255)// 按中文字符计算加2
                        byteCount += 2;
                    else// 按英文字符计算加1
                        byteCount += 1;
                    if (byteCount > maxLength)// 超出时只记下上一个有效位置
                    {
                        pos = i;
                        break;
                    }
                    else if (byteCount == maxLength)// 记下当前位置
                    {
                        pos = i + 1;
                        break;
                    }
                }

                if (pos >= 0)
                    result = rawString.Substring(0, pos) + tailString;
            }
            else
                result = rawString;

            return result;
        }

        /// <summary>
        /// 判断两个字符是否相等的
        /// </summary>
        /// <param name="stringA"></param>
        /// <param name="stringB"></param>
        /// <returns></returns>
        public static bool IsMatch(string stringA, string stringB)
        {
            return String.Equals(stringA, stringB, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// 判断两个字符是否相等的
        /// </summary>
        /// <param name="stringA"></param>
        /// <param name="stringB"></param>
        /// <param name="trimStrings">是否去除首尾空格</param>
        /// <returns></returns>
        public static bool IsMatch(string stringA, string stringB, bool trimStrings)
        {
            if (trimStrings)
            {
                return String.Equals(stringA.Trim(), stringB.Trim(), StringComparison.InvariantCultureIgnoreCase);
            }
            return String.Equals(stringA, stringB, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// 是否匹配正则表达式的
        /// </summary>
        /// <param name="inputString"></param>
        /// <param name="matchPattern"></param>
        /// <returns></returns>
        public static bool IsRegexMatch(string inputString, string matchPattern)
        {
            return Regex.IsMatch(inputString, matchPattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
        }

        /// <summary>
        /// 去除空白字符
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public static string StripWhitespace(string inputString)
        {
            if (!String.IsNullOrEmpty(inputString))
            {
                return Regex.Replace(inputString, @"\s", String.Empty);
            }
            return inputString;
        }

        /// <summary>
        /// 判断字符是否为数字
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsStringNumeric(string str)
        {
            double result;
            return (double.TryParse(str, System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.CurrentInfo, out result));
        }

        /// <summary>
        /// Escapes JavaScript with Url encoding and returns the encoded string.  
        /// </summary>
        /// <remarks>
        /// Converts quotes, single quotes and CR/LFs to their representation as an escape character.
        /// </remarks>
        /// <param name="content">The text to URL encode and escape JavaScript within.</param>
        /// <returns>The URL encoded and JavaScript escaped text.</returns>
        public static string JavaScriptEscape(string content)
        {
            // TODO: Replace by a regular expression, which should be much more efficient

            return content.Replace("\"", "\\\"").Replace("\r", "").Replace("\n", "\\n").Replace("'", "\\'");
        }

        /// <summary>
        /// 进行指定的替换(脏字过滤) 将字符文本bantext（形式为 a=b的行集合）拆分，把所有字符str中的a替换为b
        /// </summary>
        public static string StrFilter(string str, string bantext)
        {
            var text1 = "";
            var text2 = "";
            var textArray = SplitString(bantext, "\r\n");
            for (var i = 0; i < textArray.Length; i++)
            {
                text1 = textArray[i].Substring(0, textArray[i].IndexOf("=", StringComparison.Ordinal));
                text2 = textArray[i].Substring(textArray[i].IndexOf("=", StringComparison.Ordinal) + 1);
                str = str.Replace(text1, text2);
            }
            return str;
        }

        /// <summary>
        /// 用ROT13方法编码字符
        /// </summary>
        /// <param name="inputText"></param>
        /// <returns></returns>
        public static string ROT13Encode(string inputText)
        {
            int i;
            var encodedText = "";

            //Iterate through the length of the input parameter
            for (i = 0; i < inputText.Length; i++)
            {
                //Convert the current character to a char
                var currentCharacter = System.Convert.ToChar(inputText.Substring(i, 1));

                //Get the character code of the current character
                var currentCharacterCode = (int)currentCharacter;

                //Modify the character code of the character, - this
                //so that "a" becomes "n", "z" becomes "m", "N" becomes "Y" and so on
                if (currentCharacterCode >= 97 && currentCharacterCode <= 109)
                {
                    currentCharacterCode = currentCharacterCode + 13;
                }
                else

                    if (currentCharacterCode >= 110 && currentCharacterCode <= 122)
                    {
                        currentCharacterCode = currentCharacterCode - 13;
                    }
                    else

                        if (currentCharacterCode >= 65 && currentCharacterCode <= 77)
                        {
                            currentCharacterCode = currentCharacterCode + 13;
                        }
                        else

                            if (currentCharacterCode >= 78 && currentCharacterCode <= 90)
                            {
                                currentCharacterCode = currentCharacterCode - 13;
                            }

                //Add the current character to the string to be returned
                encodedText = encodedText + (char)currentCharacterCode;
            }

            return encodedText;
        }

        /// <summary>
        /// Converts a hexadecimal string to a byte array representation.
        /// </summary>
        /// <param name="txt">Hexadecimal string to convert to byte array.</param>
        /// <returns>Byte array representation of the string.</returns>
        /// <remarks>The string is assumed to be of even size.</remarks>
        public static byte[] HexToByteArray(string txt)
        {
            var b = new byte[txt.Length / 2];
            for (var i = 0; i < txt.Length; i += 2)
            {
                b[i / 2] = Convert.ToByte(txt.Substring(i, 2), 16);
            }
            return b;
        }

        /// <summary>
        /// Converts a byte array to a hexadecimal string representation.
        /// </summary>
        /// <param name="b">Byte array to convert to hexadecimal string.</param>
        /// <returns>String representation of byte array.</returns>
        public static string ByteArrayToHex(byte[] b)
        {
            return BitConverter.ToString(b).Replace("-", "");
        }

        /// <summary>
        /// Returns the hexadecimal representation of a decimal number.
        /// </summary>
        /// <param name="txt">Hexadecimal string to convert to decimal.</param>
        /// <returns>Decimal representation of string.</returns>
        public static string DecimalToHex(string txt)
        {
            return Convert.ToString(Convert.ToInt32(txt), 16);
        }

        /// <summary>
        /// Determines whether the string contains valid hexadecimal characters only.
        /// </summary>
        /// <param name="txt">String to check.</param>
        /// <returns>True if the string contains valid hexadecimal characters.</returns>
        /// <remarks>An empty or null string is considered to <b>not</b> contain
        /// valid hexadecimal characters.</remarks>
        public static bool IsHex(string txt)
        {
            return (!string.IsNullOrEmpty(txt)) && string.IsNullOrEmpty(ReplaceChars(txt, "0123456789ABCDEFabcdef", "                      ").Trim());
        }

        /// <summary>
        /// Replaces the characters in the originalChars string with the
        /// corresponding characters of the newChars string.
        /// </summary>
        /// <param name="txt">String to operate on.</param>
        /// <param name="originalChars">String with original characters.</param>
        /// <param name="newChars">String with replacement characters.</param>
        /// <example>For an original string equal to "123456654321" and originalChars="35" and
        /// newChars "AB", the result will be "12A4B66B4A21".</example>
        /// <returns>String with replaced characters.</returns>
        public static string ReplaceChars(string txt, string originalChars, string newChars)
        {
            var returned = "";

            for (var i = 0; i < txt.Length; i++)
            {
                var pos = originalChars.IndexOf(txt.Substring(i, 1), StringComparison.Ordinal);

                if (-1 != pos)
                    returned += newChars.Substring(pos, 1);
                else
                    returned += txt.Substring(i, 1);
            }
            return returned;
        }
        /// <summary>
        /// remove the html tag
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static string Html2Text(string txt)
        {
            if (txt == null)
                return "";
            var s = txt.Replace("•", "; ")
                .Replace("", "")
                .Replace("\t", "")
                .Replace("\n", "")
                .Replace("•", "")
                .Replace("<br>", "; ")
                .Replace("</li>", "; ")
                .Replace("; ;", ";");
            s = s.Trim(' ', ';');
            s = Regex.Replace(s, @"<[^>]*>", "");
            return s;
        }

        public static string SpllitBr(string txt, short index = 0)
        {
            if (txt == null)
                return "";
            var splits = txt.Split(new[] {'\n'}, 2);
            if (index > 1)
            {
                index = 1;
            }
            return splits.Length != 2 ? "" : splits[index];
        }

        public static string Replace2Br(string txt)
        {
            return txt.Replace("\r\n", "<br/>").Replace("\r", "<br/>").Replace("\n", "<br/>");
        }

        /// <summary>
        /// 获取包含字符串
        /// </summary>
        /// <param name="source"></param>
        /// <param name="containStr"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static string GetContainString(string source, string containStr, int len)
        {
            source = Html2Text(source);
            var index = source.IndexOf(containStr, StringComparison.Ordinal);
            if (index >= 0)
            {
                if (source.Length >= index + len && index - len >= 0)
                {
                    return "..."+source.Substring(index - len, len * 2)+"...";
                }
                else if (index - len >= 0)
                {
                    return "..."+source.Substring(index - len);
                }
                else if (source.Length >= index + len * 2)
                {
                    return source.Substring(0, len * 2)+"...";
                }
                else if (source.Length >= index + len)
                {
                    return source.Substring(0, source.Length - 1) + "...";
                }
                else
                {
                    return source;
                }
            }
            return source;
        }

        /// <summary>
        /// 替换多个字符串
        /// </summary>
        /// <param name="source"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static string ReplaceString(string source, string[] keys)
        {
            var str = source;
            if (keys != null && keys.Length > 0)
            {
                str = keys.Aggregate(str, (current, key) => current.Replace(key, string.Format("<span class=red>{0}</span>", key)));
            }
            return str;
        }

        /// <summary>
        /// 获取后几位数
        /// </summary>
        /// <param name="str">要截取的字符串</param>
        /// <param name="num">返回的具体位数</param>
        /// <returns>返回结果的字符串</returns>
        public static string GetLastStr(string str, int num)
        {
            if (str.Length > num)
            {
                var count = str.Length - num;
                str = str.Substring(count, num);
            }
            return str;
        }

        public static bool StringStartsWithIgnoreCase(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2) || s2.Length > s1.Length)
                return false;
            return string.Compare(s1, 0, s2, 0, s2.Length, StringComparison.OrdinalIgnoreCase) == 0;
        }
    }
}
