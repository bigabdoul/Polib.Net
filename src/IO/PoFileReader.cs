using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Polib.Net.IO
{
    using Polib.Net.Plurals;
    using static Methods;

    /// <summary>
    /// Represents an object that provides capabilities to read the contents of a .po translation file.
    /// </summary>
    public static class PoFileReader
    {
        #region private constants

        const string DBLQUOTE = "\"";
        const string MSGCTXT = "msgctxt";
        const string MSGID = "msgid";
        const string MSGID_PLURAL = "msgid_plural";
        const string MSGSTR = "msgstr";
        const string MSGSTR_PLURAL = "msgstr_plural";
        const string PLURAL_FORMS= "Plural-Forms";

        const string RX_COMMENT = "^#";
        const string RX_DBLQUOTE = "\"";
        const string RX_MSGCTXT = "^msgctxt\\s+(\".*\")";
        const string RX_MSGID = "^msgid\\s+(\".*\")";
        const string RX_MSGID_PLURAL = "^msgid_plural\\s+(\".*\")";
        const string RX_MSGSTR = "^msgstr\\s+(\".*\")";
        const string RX_MSGSTR_PLURAL = "^msgstr\\[(\\d+)\\]\\s+(\".*\")";
        const string RX_LINE = "^\".*\"$";
        const string RX_NPLURALS_EXPR = @"^\s*nplurals\s*=\s*(\d+)\s*;\s+plural\s*=\s*(.+)$";

        #endregion

        #region private fields

        static readonly Dictionary<char, string> Escapes = new Dictionary<char, string>
        {
            { 't', "\t" },
            { 'n', "\n" },
            { 'r', "\r" },
            { '\\', "\\" }
        };

        static readonly char[] Colon = new[] { ':' };

        #endregion

        #region public methods

        /// <summary>
        /// Parses a .po file content and returns a new instance of the <see cref="Catalog"/> class.
        /// </summary>
        /// <param name="content">The content to parse.</param>
        /// <param name="culture">The culture of the content to parse.</param>
        /// <param name="skipComments">true to exclude metadata such as comments; otherwise, false.</param>
        /// <param name="enforceCultureInfo">
        /// true to make sure that the <paramref name="culture"/> is valid, an exception is thrown if not; 
        /// false to accept nulls. If only the header is requested, this parameter is ignored.
        /// </param>
        /// <returns></returns>
        public static Catalog Parse(string content, string culture, bool skipComments = false, bool enforceCultureInfo = true)
        {
            using (var sr = new StringReader(content))
                return Read(sr, culture, skipComments, /* headerOnly*/ false, enforceCultureInfo);
        }

        /// <summary>
        /// Reads a .po file and returns a new instance of the <see cref="Catalog"/> class.
        /// </summary>
        /// <param name="path">The fully-qualified path, including the file name, of the .po file to read.</param>
        /// <param name="culture">The culture of the content to read.</param>
        /// <param name="skipComments">true to exclude metadata such as comments; otherwise, false.</param>
        /// <param name="enc">The file encoding to use. If null, <see cref="Encoding.UTF7"/> will be used.</param>
        /// <param name="enforceCultureInfo">
        /// true to make sure that the <paramref name="culture"/> is valid, an exception is thrown if not; 
        /// false to accept nulls. If only the header is requested, this parameter is ignored.
        /// </param>
        /// <returns></returns>
        public static Catalog Read(string path, string culture, bool skipComments = false, Encoding enc = null, bool enforceCultureInfo = true)
        {
            // try to get the file content encoding from the header metadata
            if (null == enc && ReadHeader(path).GetCharset(out var charset))
            {
                // set the right encoding
                enc = Encoding.GetEncoding(charset);
            }
            else
            {
                // try to find an appropriate encoding
                enc = enc ?? GetFileEncoding(path);
            }

            using (var f = File.OpenRead(path))
            {
                using (var sr = new StreamReader(f, enc))
                {
                    var obj = Read(sr, culture, skipComments, /* headerOnly*/ false, enforceCultureInfo);
                    obj.FileName = path;
                    return obj;
                }
            }
        }

        /// <summary>
        /// Reads the specified text reader and returns a new instance of the <see cref="Catalog"/> class.
        /// </summary>
        /// <param name="tr">An object used to read some text.</param>
        /// <param name="culture">The culture of the content to read.</param>
        /// <param name="skipComments">true to exclude metadata such as comments; otherwise, false.</param>
        /// <param name="headerOnly">true to only read the PO header information; otherwise, false to read the whole catalog.</param>
        /// <param name="enforceCultureInfo">
        /// true to make sure that the <paramref name="culture"/> is valid, an exception is thrown if not; 
        /// false to accept nulls. If only the header is requested, this parameter is ignored.
        /// </param>
        /// <returns></returns>
        public static Catalog Read(TextReader tr
            , string culture
            , bool skipComments = false
            , bool headerOnly = false
            , bool enforceCultureInfo = true)
        {
            if (headerOnly) enforceCultureInfo = false;

            var catalog = new Catalog { Culture = GetCultureInfo(culture, enforceCultureInfo) };
            var headers = catalog.Headers;
            var entries = catalog.Entries;

            string line = null;
            Match match = null;
            var matchIdx = 0;
            var matchVal = string.Empty;
            var context = string.Empty;
            var entry = new Translation(catalog);

            while ((line = tr.ReadLine()) != null)
            {
                if (preg_match(RX_COMMENT))
                {
                    // comments have to be at the beginning
                    if (context != string.Empty && context != "comment")
                        invalid();

                    // context = "comment";
                    if (!skipComments)
                        AddCommentToEntry(entry, line);
                }
                else if (preg_match(RX_MSGCTXT))
                {
                    context = MSGCTXT;
                    entry.Context = matchVal;
                }
                else if (preg_match(RX_MSGID))
                {
                    if (context != string.Empty && context != MSGCTXT && context != "comment")
                        invalid();
                    context = MSGID;
                    entry.Singular = matchVal;
                }
                else if (preg_match(RX_MSGID_PLURAL))
                {
                    if (context != MSGID)
                        invalid();
                    context = MSGID_PLURAL;
                    entry.Plural = matchVal;
                }
                else if (preg_match(RX_MSGSTR))
                {
                    if (context != MSGID)
                        invalid();
                    context = MSGSTR;
                    entry.Translations.Add(matchVal);
                }
                else if (preg_match(RX_MSGSTR_PLURAL))
                {
                    if (context != MSGID_PLURAL && context != MSGSTR_PLURAL)
                        invalid();
                    context = MSGSTR_PLURAL;
                    matchIdx = int.Parse(matchVal);
                    matchVal = Unpoify(match.Groups[2].Value);

                    if (entry.Translations.Count >= matchIdx)
                        entry.Translations.Add(matchVal);
                    else
                        entry.Translations[matchIdx] = matchVal;
                }
                else if (preg_match(RX_LINE))
                {
                    switch (context)
                    {
                        case MSGID:
                            entry.Singular += matchVal; break;
                        case MSGCTXT:
                            entry.Context += matchVal; break;
                        case MSGID_PLURAL:
                            entry.Plural += matchVal; break;
                        case MSGSTR:
                            entry.Translations[0] += matchVal; break;
                        case MSGSTR_PLURAL:
                            entry.Translations[matchIdx] += matchVal; break;
                        default:
                            invalid(); break;
                    }
                }
                else if (string.IsNullOrWhiteSpace(line))
                {
                    if (string.Empty == entry.Singular)
                    {
                        SetHeaders(MakeHeaders(entry.Translations[0]), headers);

                        catalog.HeaderComments = entry.TranslatorComments;

                        if (headers.ContainsKey(PLURAL_FORMS))
                        {
                            var kvp = GetPluralsExpression(headers[PLURAL_FORMS]);
                            catalog.PluralCount = kvp.Key;
                            catalog.PluralEvaluator = new PluralFormsEvaluator(kvp.Value);
                        }

                        if (catalog.Culture == null && catalog.GetHeader("Language", out var value))
                        {
                            catalog.Culture = GetCultureInfo(value, enforceCultureInfo);
                        }

                        if (headerOnly) break;
                    }
                    else
                    {
                        add_last_entry();
                    }
                    context = string.Empty;
                    entry = new Translation(catalog);
                }
                else
                {
                    invalid();
                }
            }

            add_last_entry();

            catalog.LastAccessTime = DateTime.Now;
            return catalog;

            bool preg_match(string pattern)
            {
                match = Regex.Match(line, pattern);
                if (match.Success) {
                    if (match.Groups.Count > 1) {
                        matchVal = Unpoify(match.Groups[1].Value);
                    }
                    else {
                        matchVal = Unpoify(match.Groups[0].Value);
                    }
                    return true;
                }
                return false;
            }

            void add_last_entry()
            {
                var key = entry.Key;

                if (! string.IsNullOrWhiteSpace(key) && ! entries.ContainsKey(key))
                    entries.Add(key, entry);
            }

            void invalid() => throw new InvalidDataException();
        }

        /// <summary>
        /// Reads the header of the specified .po file.
        /// </summary>
        /// <param name="path">The fully-qualified path, including the file name, of the .po file to read.</param>
        /// <returns></returns>
        public static Catalog ReadHeader(string path)
        {
            using (var f = File.OpenRead(path))
            {
                using (var sr = new StreamReader(f))
                {
                    return Read(sr, culture: null, skipComments: true, headerOnly: true);
                }
            }
        }

        /// <summary>
        /// Recursively reads all translation files located under the specified path and subdirectories.
        /// </summary>
        /// <param name="directory">The path to the directory to search.</param>
        /// <param name="culture">The name of the culture used to retrieve translation files. Must be in a format similar to 'en-US', 'fr_FR', 'de.DE', or 'es', using any case of these characters (case-insensitive).</param>
        /// <param name="skipComments">true to exclude metadata such as comments; otherwise, false.</param>
        /// <param name="includeSubdirectories">true to recursively read subdirectories of <paramref name="directory"/>; otherwise, false.</param>
        /// <param name="includeRegionCode2">true to include the regioncode2 (e.g. 'fr', 'en', etc.) culture variation in the search; otherwise, false.</param>
        /// <returns></returns>
        /// <exception cref="CultureNotFoundException">Unsupported culture format.</exception>
        public static IEnumerable<Catalog> ReadAll(string directory, string culture, bool skipComments = false , bool includeSubdirectories = false, bool includeRegionCode2 = false)
        {
            var cinfo = GetCultureInfo(culture);
            var files = new HashSet<string>();
            var catalogs = new HashSet<Catalog>();
            var sOption = includeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            ReadFiles(directory, cinfo, catalogs, files, skipComments, sOption, includeRegionCode2, cinfo);

            files.Clear();
            return catalogs;
        }

        /// <summary>
        /// (Experimental) Merges two Uniforum style .po files together.
        /// </summary>
        /// <param name="poDefFilePath">The .po definition file, which actually contains translations referring to old sources.</param>
        /// <param name="refPotFilePath">The .pot file, which actually contains references to new sources.</param>
        /// <param name="culture">The culture of the content to read.</param>
        /// <returns></returns>
        public static Catalog Merge(string poDefFilePath, string refPotFilePath, string culture = null)
        {
            // read the PO definition file
            var po = Read(poDefFilePath, culture, enforceCultureInfo: false);

            // read the POT template file
            var pot = Read(refPotFilePath, culture, enforceCultureInfo: false);

            po.MergeWith(pot);

            return po;
        }

        #endregion

        #region helpers

        /// <summary>
        /// Adds a comment to the appropriate property of the specified <paramref name="entry"/>.
        /// </summary>
        /// <param name="entry">The translation entry to add a comment to.</param>
        /// <param name="line">The comment line to add.</param>
        static void AddCommentToEntry(ITranslation entry, string line)
        {
            var first_two = line.Substring(0, 2);
            var comment = line.Substring(2).Trim();

            if ("#:" == first_two)
            {
                foreach (var s in Regex.Split(comment, @"\s+"))
                    entry.References.Add(s);
            }
            else if ("#." == first_two)
            {
                entry.ExtractedComments = $"{entry.ExtractedComments}\n{comment}".Trim();
            }
            else if ("#," == first_two)
            {
                foreach (var s in Regex.Split(comment, @",\s*"))
                    entry.Flags.Add(s);
            }
            else
            {
                entry.TranslatorComments = $"{entry.TranslatorComments}\n{comment}".Trim();
            }
        }

        /// <summary>
        /// Gives back the original string from a PO-formatted string.
        /// </summary>
        /// <param name="value">PO-formatted string.</param>
        /// <returns>escaped string.</returns>
        /// <remarks>Inspired by the WordPress implementation.</remarks>
        static string Unpoify(string value)
        {
            var lines = value.Split('\n')
                .Select(s => s.Trim())
                .Select(s => TrimQuotes(s));

            var unpoified = string.Empty;
            var previous_is_backslash = false;

            foreach (var ln in lines)
            {
                foreach (var chr in ln)
                {
                    if (!previous_is_backslash)
                    {
                        if ('\\' == chr)
                            previous_is_backslash = true;
                        else
                            unpoified += chr;
                    }
                    else
                    {
                        previous_is_backslash = false;
                        unpoified += Escapes.ContainsKey(chr) ? Escapes[chr] : chr.ToString();
                    }
                }
            }

            // Standardise the line endings on imported content, technically PO files shouldn't contain \r
            unpoified = unpoified.Replace("\r\n", "\n").Replace("\r", "\n");

            return unpoified;
        }

        /// <summary>
        /// Removes double quotes at the beginning and the end of the specified string.
        /// </summary>
        /// <param name="s">The string to unquote.</param>
        /// <returns></returns>
        static string TrimQuotes(string s)
        {
            if (s.StartsWith(DBLQUOTE)) s = s.Substring(1);
            if (s.EndsWith(DBLQUOTE)) s = s.Substring(0, s.Length - 1);
            return s;
        }

        /// <summary>
        /// Creates a collection of key/value pairs from the specified translation.
        /// </summary>
        /// <param name="translation">The translation to transform into a dictionary.</param>
        /// <returns></returns>
        static IDictionary<string, string> MakeHeaders(string translation)
        {
            var dic = new Dictionary<string, string>();
            // sometimes \ns are used instead of real new lines
            translation = translation.Replace("\\n", "\n");
            var lines = translation.Split('\n');
            foreach (var ln in lines)
            {
                var parts = ln.Split(Colon, 2);
                if (parts.Length != 2) continue;
                dic.Add(parts[0].Trim(), parts[1].Trim());
            }
            return dic;
        }


        static void SetHeaders(IDictionary<string, string> source, IDictionary<string, string> destination)
        {
            foreach (var key in source.Keys)
            {
                var value = source[key];

                if (destination.ContainsKey(key))
                    destination[key] = value;
                else
                    destination.Add(key, value);
            }
        }

        /// <summary>
        /// Extracts the number of plurals and the plural expression from the specified header.
        /// </summary>
        /// <param name="header">The header to extract plural parts from.</param>
        /// <returns></returns>
        static KeyValuePair<int, string> GetPluralsExpression(string header)
        {
            int nplurals;
            string plural;

            var match = Regex.Match(header, RX_NPLURALS_EXPR);

            if (match.Success)
            {
                nplurals = int.Parse(match.Groups[1].Value);
                plural = match.Groups[2].Value.Trim();
            }
            else
            {
                // default plurals forms
                nplurals = 2;
                plural = "n!=1";
            }

            return new KeyValuePair<int, string>(nplurals, plural);
        }

        /// <summary>
        /// Reads (.po) translation files in the specified directory, optionally including subdirectories.
        /// </summary>
        /// <param name="directory">The directory to search for .po files.</param>
        /// <param name="culture">The culture name of the files to search.</param>
        /// <param name="catalogs">Collects all read catalogs.</param>
        /// <param name="filesRead">To keep reading any file more than once.</param>
        /// <param name="skipComments">true to omit comments; otherwise, false.</param>
        /// <param name="searchOption">Search top directories only or include subdirectories?</param>
        /// <param name="includeRegionCode2">true to search the variations of the specified culture; otherwise, false.</param>
        /// <param name="originalCulture">The original culture used to assign to the <see cref="Catalog.Culture"/> property.</param>
        private static void ReadFiles(string directory, CultureInfo culture, HashSet<Catalog> catalogs, HashSet<string> filesRead
            , bool skipComments = false
            , SearchOption searchOption = SearchOption.TopDirectoryOnly
            , bool includeRegionCode2 = false
            , CultureInfo originalCulture = null)
        {
            if (originalCulture == null)
                originalCulture = culture;

            // the default culture format is 'lang-LANG' (e.g. fr-CA)
            var lang = culture.Name;

            // store lang/culture and corresponding files
            var dicFiles = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

            // see region 'helper methods' below
            add_files(lang);

            // should we find all variations of the same culture?
            if (!culture.IsRegionCode2())
            {
                // some programs use the '_' format (e.g. 'en_US')
                add_files(lang.Replace(DASH, UNDERSCORE));

                // others use the '.' format (e.g. 'de.DE')
                add_files(lang.Replace(DASH, DOT));
            }

            foreach (var cult in dicFiles.Keys)
            {
                var files = dicFiles[cult];
                foreach (var f in files)
                {
                    if (!filesRead.Contains(f))
                    {
                        // read the catalog
                        var cat = Read(f, cult, skipComments);
                        catalogs.Add(cat);

                        // maintain a hash set to avoid reading the same file more than once
                        filesRead.Add(f);
                    }
                }
            }

            // see region 'helper methods' below
            if (includeRegionCode2 && !culture.IsRegionCode2())
                // load the regioncode2 variation of the files
                ReadFiles(directory
                    , new CultureInfo(culture.TwoLetterISOLanguageName)
                    , catalogs
                    , filesRead
                    , skipComments
                    , searchOption
                    , false
                    , originalCulture);
            
            #region helper methods
            
            void add_files(string locale)
            {
                var files = Directory.GetFiles(directory, $"*{locale}.po", searchOption);
                if (files.Length > 0 && !dicFiles.ContainsKey(locale)) dicFiles.Add(locale, files);
            }

            #endregion
        }

        #endregion
    }
}
