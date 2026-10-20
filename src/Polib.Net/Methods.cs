using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Polib.Net
{
    /// <summary>
    /// Provides various helper methods used internally.
    /// </summary>
    public static class Methods
    {
        #region const & static

        internal const char DASH = '-';
        internal const char UNDERSCORE = '_';
        internal const char DOT = '.';
        internal const string NEWLN = "\n";
        internal const int PO_MAX_LINE_LEN = 75;

        /// <summary>
        /// Precompiled regex for uncut lines with default length.
        /// </summary>
        internal static readonly Regex RxWordwrap = new Regex($@"(?=\s)(.{{1,{PO_MAX_LINE_LEN}}})(?:\s|$)", RegexOptions.Compiled);

        /// <summary>
        /// Precompiled regex for cut lines with default length.
        /// </summary>
        internal static readonly Regex RxWordwrapCut = new Regex($@"(.{{1,{PO_MAX_LINE_LEN}}})(?:\s|$)|(.{{{PO_MAX_LINE_LEN}}})", RegexOptions.Compiled);

        #endregion

        #region methods

        /// <summary>
        /// Returns the appropriate culture information.
        /// </summary>
        /// <param name="culture">The name of the culture. Can be null or empty; if not, must be in a format 
        /// similar to 'en-US', 'fr_FR', 'de.DE', or 'es', using any case of these characters (case-insensitive).</param>
        /// <param name="throwIfNull">true to throw a <see cref="CultureNotFoundException"/> if the culture is not valid.</param>
        /// <returns></returns>
        internal static CultureInfo GetCultureInfo(string culture, bool throwIfNull = true)
        {
            var cult = string.IsNullOrWhiteSpace(culture) ? null : new CultureInfo(NormalizeCulture(culture));

            if (throwIfNull && null == cult)
                throw new CultureNotFoundException(nameof(culture), culture, "Unsupported culture format.");

            return cult;
        }

        /// <summary>
        /// Replaces the understore '_', or dot '.' symbols (like those in 'fr_FR', 'de.DE', etc.) with a dash '-' in the specified culture
        /// </summary>
        /// <param name="culture">The name of the culture to normalize.</param>
        /// <returns></returns>
        internal static string NormalizeCulture(string culture)
        {
            if (culture?.Length == 5)
            {
                if (culture.Contains(UNDERSCORE))
                {
                    culture = culture.Replace(UNDERSCORE, DASH);
                }
                else if (culture.Contains(DOT))
                {
                    culture = culture.Replace(DOT, DASH);
                }
            }
            return culture;
        }

        /// <summary>
        /// Detects the byte order mark of a file and returns an appropriate encoding for the file.
        /// </summary>
        /// <param name="path">The full name of the file to check.</param>
        /// <returns></returns>
        internal static Encoding GetFileEncoding(string path)
        {
            // *** Use Default of Encoding.Default (Ansi CodePage)
            Encoding enc = Encoding.UTF7;

            // *** Detect byte order mark if any - otherwise assume default
            byte[] buffer = new byte[5];

            using (var file = System.IO.File.OpenRead(path))
            {
                file.Read(buffer, 0, 5);

                if (buffer[0] == 0xef && buffer[1] == 0xbb && buffer[2] == 0xbf)
                    enc = Encoding.UTF8;
                else if (buffer[0] == 0xfe && buffer[1] == 0xff)
                    enc = Encoding.Unicode;
                else if (buffer[0] == 0 && buffer[1] == 0 && buffer[2] == 0xfe && buffer[3] == 0xff)
                    enc = Encoding.UTF32;
                else if (buffer[0] == 0x2b && buffer[1] == 0x2f && buffer[2] == 0x76)
                    enc = Encoding.UTF7;
            }

            return enc;
        }

        /// <summary>
        /// Determines whether the specified culture's name equals its two-letter ISO language name.
        /// </summary>
        /// <param name="c">The culture to check.</param>
        /// <returns></returns>
        public static bool IsRegionCode2(this CultureInfo c)
            => string.Equals(c.Name, c.TwoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Hash the specified string and return a hexa-decimal number.
        /// </summary>
        /// <param name="s">The string to hash.</param>
        /// <param name="lowerCase">true to return lower-case letters; otherwise, false.</param>
        /// <returns></returns>
        public static string Hash(this string s, bool lowerCase = false)
        {
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                return sha.ComputeHash(Encoding.UTF8.GetBytes(s)).ToHex(lowerCase);
            }
        }

        /// <summary>
        /// Transforms the one-dimensional byte array to a hexadecimal string.
        /// </summary>
        /// <param name="array">A one-dimensional byte array.</param>
        /// <param name="lowerCase">true to return lower-case letters; otherwise, false.</param>
        /// <returns>null (Nothing in Visual Basic) if <paramref name="array"/> is null, an empty string of the length of the array is zero; otherwise, a hexadecimal string.</returns>
        public static string ToHex(this byte[] array, bool lowerCase = false)
        {
            if (array == null)
            {
                return null;
            }

            if (array.Length == 0)
            {
                return string.Empty;
            }

            var output = new StringBuilder();
            var format = lowerCase ? "{0:x2}" : "{0:X2}";

            for (int i = 0; i < array.Length; i++)
            {
                output.AppendFormat(format, array[i]);
            }

            return output.ToString();
        }

        #region Wordwrap

        /// <summary>
        /// Wraps a string to a given number of characters.
        /// </summary>
        /// <param name="text">The string to wrap.</param>
        /// <param name="width">The number of characters at which the string will be wrapped.</param>
        /// <param name="separator">The line is broken using this optional parameter.</param>
        /// <param name="cut">If the cut is set to true, the string is always wrapped at or before the specified width.</param>
        /// <returns></returns>
        public static string Wordwrap(this string text, int width = PO_MAX_LINE_LEN, string separator = NEWLN, bool cut = false)
        {
            if (width < 1 || text == null || text.Length <= width) return text;
            if (cut) return WordwrapUncut(text, width, separator);

            var sb = new StringBuilder();
            var sbLines = new StringBuilder();

            // go over each char
            foreach (var c in text)
            {
                // can the character be added without overflowing?
                if (sb.Length <= width)
                {
                    // yes
                    sb.Append(c);
                }
                else
                {
                    // no, add a new line, clear the string builder
                    sbLines.Append(sb.ToString() + separator);
                    sb.Clear();

                    // append the last char, now the first of the new line
                    sb.Append(c);
                }
            }

            // is there enough space for the separator?
            if (sb.Length > width)
            {
                // no, add a new line
                sbLines.Append(sb.ToString() + separator);
                sb.Clear();
            }

            if (sb.Length > 0)
            {
                var s = sb.ToString();
                sbLines.Append(s.Substring(0, s.Length));
            }

            sb.Clear();

            return sbLines.ToString().TrimEnd(separator.ToCharArray());
        }

        /// <summary>
        /// Word wraps the given text to fit within the specified width.
        /// </summary>
        /// <param name="text">Text to be word wrapped</param>
        /// <param name="width">Width, in characters, to which the text should be word wrapped</param>
        /// <param name="separator"></param>
        /// <returns>The modified text</returns>
        public static string WordwrapUncut(this string text, int width = PO_MAX_LINE_LEN, string separator = NEWLN)
        {
            if (width < 1 || text == null || text.Length <= width) return text;

            int pos, next;
            var sb = new StringBuilder();
            separator = separator ?? Environment.NewLine;

            // Parse each line of text
            for (pos = 0; pos < text.Length; pos = next)
            {
                // Find end of line
                int eol = text.IndexOf(separator, pos);
                if (eol == -1)
                    next = eol = text.Length;
                else
                    next = eol + separator.Length;

                // Copy this line of text, breaking into smaller lines as needed
                if (eol > pos)
                {
                    do
                    {
                        int len = eol - pos;
                        if (len > width)
                            len = BreakLine(text, pos, width);
                        sb.Append(text, pos, len);
                        sb.Append(separator);

                        // Trim whitespace following break
                        pos += len;
                        while (pos < eol && char.IsWhiteSpace(text[pos]))
                            pos++;
                    } while (eol > pos);
                }
                else sb.Append(separator); // Empty line
            }
            return sb.ToString().TrimEnd(separator.ToCharArray());
        }

        /// <summary>
        /// Wraps a string to a given number of characters. Uses regular expressions.
        /// </summary>
        /// <param name="text">The string to wrap.</param>
        /// <param name="length">The maximum length.</param>
        /// <param name="separator">The string used to separate words.</param>
        /// <param name="cut">true to cut if a line exceeds <paramref name="length"/>; otherwise, false.</param>
        /// <returns></returns>
        /// <remarks>This method, even though it looks neat, is way too slow (over 4 times slower) compared to the ones above.</remarks>
        public static string WordwrapRx(this string text, int length = PO_MAX_LINE_LEN, string separator = NEWLN, bool cut = false)
        {
            string replace = cut ? "$1$2" + separator : "$1" + separator;

            // do we have the default length?
            if (length == PO_MAX_LINE_LEN)
            {
                // use one of the precompiled regex
                return cut
                    ? RxWordwrapCut.Replace(text, replace)
                    : RxWordwrap.Replace(text, replace);
            }

            string search = null;

            if (cut)
            {
                // Match anything 1 to $width chars long followed by whitespace or EOS,
                // otherwise match anything $width chars long
                search = $@"(.{{1,{length}}})(?:\s|$)|(.{{{length}}})";
            }
            else
            {
                // Anchor the beginning of the pattern with a lookahead
                // to avoid crazy backtracking when words are longer than length
                search = $@"(?=\s)(.{{1,{length}}})(?:\s|$)";
            }

            return Regex.Replace(text, search, replace).TrimEnd(separator.ToCharArray());
        }

        static int BreakLine(string text, int pos, int max)
        {
            // Find last whitespace in line
            int i = max;
            while (i >= 0 && !char.IsWhiteSpace(text[pos + i]))
                i--;

            // If no whitespace found, break at maximum length
            if (i < 0)
                return max;

            // Find start of whitespace
            while (i >= 0 && char.IsWhiteSpace(text[pos + i]))
                i--;

            // Return length of text before whitespace
            return i + 1;
        }

        /*
         function multiCol($string, $numcols)
  {
    $collength = ceil(strlen($string) / $numcols) + 3;
    $retval = explode("\n", wordwrap(strrev($string), $collength));
    if(count($retval) > $numcols) {
      $retval[$numcols-1] .= " " . $retval[$numcols];
      unset($retval[$numcols]);
    }
    $retval = array_map("strrev", $retval);
    return array_reverse($retval);
  }
         */

        /// <summary>
        /// Wraps a string to a given number of characters.
        /// </summary>
        /// <param name="text">The string to wrap.</param>
        /// <param name="lineWidth">The number of characters at which the string will be wrapped.</param>
        /// <returns></returns>
        public static string WrapText(this string text, int lineWidth = PO_MAX_LINE_LEN)
        {
            return string.Join(string.Empty, wrap(text.Split(new char[0], StringSplitOptions.RemoveEmptyEntries)));

            IEnumerable<string> wrap(IEnumerable<string> words)
            {
                var currentWidth = 0;
                foreach (var word in words)
                {
                    if (currentWidth != 0)
                    {
                        if (currentWidth + word.Length < lineWidth)
                        {
                            currentWidth++;
                            yield return " ";
                        }
                        else
                        {
                            currentWidth = 0;
                            yield return Environment.NewLine;
                        }
                    }
                    currentWidth += word.Length;
                    yield return word;
                }
            }
        }

        #endregion

        #endregion
    }
}
