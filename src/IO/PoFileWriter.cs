using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Polib.Net.IO
{
    using static Methods;

    /// <summary>
    /// Represents an object capable of writing .po files.
    /// </summary>
    public class PoFileWriter : IPoFileWriter
    {
        #region private const & static

        const string QUOTE = "\"";
        const string QUOTES_EMPTY = QUOTE + QUOTE;
        const string SLASH = "\\";
        const char NEWLNC = '\n';

        static readonly IDictionary<string, string> Replaces = new Dictionary<string, string>
        {
            { QUOTE, $"{SLASH}{QUOTE}" },
            { SLASH, $"{SLASH}" },
            { "\t", "\\t" },
        };

        #endregion
        
        #region public properties

        /// <summary>
        /// Gets or sets the maximum number of characters on each line in a .po file.
        /// </summary>
        public static int PoMaxLineLength { get; set; } = PO_MAX_LINE_LEN + 3;

        /// <summary>
        /// Determines whether reference comments are word-wrapped.
        /// </summary>
        public bool WordWrapReferences { get; set; }

        #endregion

        #region fields

        Encoding _currentEncoding;
        readonly Encoding _catalogEncoding;
        readonly ICatalog _catalog;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PoFileWriter"/> class using the specified parameter.
        /// </summary>
        /// <param name="catalog">The catalog used to write the file.</param>
        public PoFileWriter(ICatalog catalog)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _catalogEncoding = _catalog.GetEncoding();
        }

        #endregion

        #region Export methods

        /// <summary>
        /// Exports the whole PO catalog as a string.
        /// </summary>
        /// <returns></returns>
        public virtual string Export() => Export(false);

        /// <summary>
        /// Exports the whole PO catalog as a string.
        /// </summary>
        /// <param name="excludeHeaders">Whether to exclude the headers in the export.</param>
        /// <returns></returns>
        public virtual string Export(bool excludeHeaders)
        {
            var output = new StringBuilder();

            if (!excludeHeaders)
            {
                output.Append(ExportHeaders());
                output.Append($"{NEWLN}{NEWLN}");
            }

            output.Append(ExportEntries());
            return output.ToString();
        }

        /// <summary>
        /// Exports headers to a PO entry.
        /// </summary>
        /// <returns>msgid/msgstr PO entry for this PO file headers, doesn't contain newline at the end.</returns>
        public virtual string ExportHeaders()
        {
            var headers = new StringBuilder();

            foreach (var kvp in _catalog.Headers)
            {
                headers.Append($"{kvp.Key}: {kvp.Value}{NEWLN}");
            }

            var poified = Poify(headers.ToString());
            var beforeHeaders = string.Empty;

            if (!string.IsNullOrEmpty(_catalog.HeaderComments))
            {
                beforeHeaders = PrependEachLine(_catalog.HeaderComments.TrimEnd() + NEWLN, "# ");
            }

            return $"{beforeHeaders}msgid {QUOTES_EMPTY}{NEWLN}msgstr {poified}".TrimEnd();
        }

        /// <summary>
        /// Exports all entries to PO format.
        /// </summary>
        /// <returns>Sequence of mgsgid/msgstr PO strings, doesn't containt newline at the end.</returns>
        public virtual string ExportEntries()
        {
            var sb = new StringBuilder();

            foreach (var entry in _catalog.Entries.Values)
                sb.Append(ExportEntry(entry) + NEWLN).Append(NEWLN);

            return sb.ToString().TrimEnd() + NEWLN;
        }

        /// <summary>
        /// Builds a string from the entry for inclusion in PO file.
        /// </summary>
        /// <param name="entry">The entry to convert to po string (passed by reference).</param>
        /// <returns></returns>
        public virtual string ExportEntry(ITranslation entry)
        {
            if (string.IsNullOrEmpty(entry.Singular)) return null;

            //var sb = new StringBuilder();
            var ls = new List<string>();

            if (!string.IsNullOrEmpty(entry.TranslatorComments)) ls.Add(CommentBlock(entry.TranslatorComments));
            if (!string.IsNullOrEmpty(entry.ExtractedComments)) ls.Add(CommentBlock(entry.ExtractedComments, "."));
            if (entry.References.Count > 0) ls.Add(CommentBlock(string.Join(" ", entry.References), ":"));
            if (entry.Flags.Count > 0) ls.Add(CommentBlock(string.Join(", ", entry.Flags), ","));
            if (!string.IsNullOrEmpty(entry.Context)) ls.Add("msgctxt " + Poify(entry.Context));

            ls.Add("msgid " + Poify(entry.Singular));

            if (!entry.IsPlural)
            {
                if (entry.Translations.Count > 0)
                {
                    var translation = MatchBeginAndEndNewlines(entry.Translations[0], entry.Singular);
                    ls.Add("msgstr " + Poify(translation));
                }
                else
                {
                    ls.Add("msgstr " + QUOTES_EMPTY);
                }
            }
            else
            {
                ls.Add("msgid_plural " + Poify(entry.Plural));

                for (int i = 0; i < entry.Translations.Count; i++)
                {
                    var translation = MatchBeginAndEndNewlines(entry.Translations[i], entry.Plural);
                    ls.Add($"msgstr[{i}] " + Poify(entry.Translations[i]));
                }
            }

            return string.Join(NEWLN, ls);
        }

        #endregion

        #region SaveChanges overloads

        /// <summary>
        /// Writes the changes back to a temporary file and returns the path pointing at it.
        /// </summary>
        /// <param name="excludeHeaders">Whether to exclude the headers during the operation.</param>
        /// <param name="encoding">The text encoding to use. If null, the catalog's encoding is used.</param>
        /// <returns></returns>
        public virtual string SaveChanges(bool excludeHeaders = false, Encoding encoding = null)
        {
            var tempPath = Path.GetTempFileName();
            SaveChanges(tempPath, excludeHeaders, encoding);
            return tempPath;
        }

        /// <summary>
        /// Writes the changes back to the specified file <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The fully-qualified name, including the path, of the file changes are written to.</param>
        /// <param name="excludeHeaders">Whether to exclude the headers during the operation.</param>
        /// <param name="encoding">The text encoding to use. If null, the catalog's encoding is used.</param>
        public virtual void SaveChanges(string path, bool excludeHeaders = false, Encoding encoding = null)
        {
            using (var stream = File.Open(path, FileMode.Create, FileAccess.Write))
            {
                encoding = encoding ?? _catalogEncoding;

                // should encoding be used?
                var writer = encoding != null
                    ? new StreamWriter(stream, encoding)
                    : new StreamWriter(stream);

                using(writer) SaveChanges(writer, excludeHeaders);
            }
        }

        /// <summary>
        /// Writes the changes back to the specified <paramref name="writer"/>, optionally excluding the headers.
        /// </summary>
        /// <param name="writer">An object used to write the changes in the catalog.</param>
        /// <param name="excludeHeaders">Whether to exclude the headers during the operation.</param>
        public virtual void SaveChanges(TextWriter writer, bool excludeHeaders = false)
        {
            if (!excludeHeaders)
            {
                writer.Write(ExportHeaders());
                writer.Write($"{NEWLN}{NEWLN}");
            }

            writer.Write(ExportEntries());
            writer.Flush();
        }

        /// <summary>
        /// Writes back the changes in this catalog to the specified <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">An object used to write the changes in the catalog.</param>
        /// <param name="excludeHeaders">Whether to exclude the headers during the operation.</param>
        /// <param name="encoding">The text encoding to use. If null, the catalog's encoding is used.</param>
        public virtual void SaveChanges(Stream stream, bool excludeHeaders = false, Encoding encoding = null)
        {
            if (null == stream) throw new ArgumentNullException(nameof(stream));
            if (!stream.CanWrite) throw new InvalidOperationException("Cannot save changes to a non-writable stream.");

            // set the appropriate encoding
            SetEncoding(encoding);

            if (!excludeHeaders)
            {
                WriteText(stream, ExportHeaders());
                WriteText(stream, $"{NEWLN}{NEWLN}");
            }

            WriteText(stream, ExportEntries());
            stream.Flush();
        }

        #endregion
        
        #region protected methods
        
        /// <summary>
        /// Writes the headers to the specified text writer.
        /// </summary>
        /// <param name="writer">An object to which the headers are written.</param>
        protected virtual void WriteHeaders(TextWriter writer) => writer.Write(ExportHeaders());

        /// <summary>
        /// Writes the headers to the specified stream.
        /// </summary>
        /// <param name="stream">An object to which the headers are written.</param>
        protected virtual void WriteHeaders(Stream stream) => WriteText(stream, ExportHeaders());

        /// <summary>
        /// Writes the <see cref="ICatalog.Entries"/> dictionary to the specified text writer.
        /// </summary>
        /// <param name="writer">An object to which the entries are written.</param>
        protected virtual void WriteEntries(TextWriter writer) => writer.Write(ExportEntries());

        /// <summary>
        /// Writes the <see cref="ICatalog.Entries"/> dictionary to the specified stream.
        /// </summary>
        /// <param name="stream">An object to which the entries are written.</param>
        protected virtual void WriteEntries(Stream stream) => WriteText(stream, ExportEntries());

        /// <summary>
        /// Writes the translation entry to the specified text writer.
        /// </summary>
        /// <param name="writer">An object to which the entry is written.</param>
        /// <param name="entry">The entry to write.</param>
        protected virtual void WriteEntry(TextWriter writer, ITranslation entry) => writer.Write(ExportEntry(entry));

        /// <summary>
        /// Writes the translation entry to the specified stream.
        /// </summary>
        /// <param name="stream">An object to which the entry is written.</param>
        /// <param name="entry">The entry to write.</param>
        protected virtual void WriteEntry(Stream stream, ITranslation entry) => WriteText(stream, ExportEntry(entry));

        /// <summary>
        /// Prepare a text as a comment -- wraps the lines and prepends # and a special character to each line.
        /// </summary>
        /// <param name="text">The comment text.</param>
        /// <param name="type">Character to denote a special PO comment, like :, default is a space.</param>
        /// <returns></returns>
        protected virtual string CommentBlock(string text, string type = " ")
        {
            // don't wrap references?
            if (type != ":" || WordWrapReferences)
                text = text.WordwrapRx(PoMaxLineLength - 3).TrimEnd(NEWLNC);

            return PrependEachLine(text, $"#{type} ");
        }

        /// <summary>
        /// Sets the appropriate encoding to be used by this <see cref="PoFileWriter"/> instance.
        /// </summary>
        /// <param name="encoding">The text encoding to use.</param>
        /// <exception cref="ArgumentNullException">Cannot resolve to the appropriate encoding.</exception>
        protected virtual void SetEncoding(Encoding encoding)
        {
            if (encoding != null) _currentEncoding = encoding;
            if (null == _currentEncoding) _currentEncoding = _catalogEncoding;
            if (null == _currentEncoding)
                throw new ArgumentNullException(nameof(encoding), "Cannot resolve to the appropriate encoding.");
        }

        /// <summary>
        /// Writes the text to specified stream. 
        /// </summary>
        /// <param name="stream">The stream to write the text to.</param>
        /// <param name="text">The text to write to the stream.</param>
        protected virtual void WriteText(Stream stream, string text)
        {
            stream.Write(GetBytes(out var length), 0, length);

            byte[] GetBytes(out int len)
            {
                var bytes = _currentEncoding.GetBytes(text);
                len = bytes.Length;
                return bytes;
            }
        }

        #endregion
        
        /// <summary>
        /// Formats a string in PO-style
        /// </summary>
        /// <param name="s">The string to format.</param>
        /// <returns>The poified string.</returns>
        public static string Poify(string s)
        {
            var sb = new StringBuilder(s);

            foreach (var kvp in Replaces) sb.Replace(kvp.Key, kvp.Value);

            var po = QUOTE + string.Join($"{SLASH}n{QUOTE}{NEWLN}{QUOTE}", sb.ToString().Split(NEWLNC)) + QUOTE;

            // add empty string on first line for readbility
            if (po.IndexOf(NEWLN) > -1 && 
                (substr_count(po, NEWLN) > 1 || po.Substring(po.Length - NEWLN.Length) != NEWLN))
                po = $"{QUOTES_EMPTY}{NEWLN}{po}";

            // remove empty strings
            po = po.Replace($"{NEWLN}{QUOTES_EMPTY}", string.Empty);

            return po;

            // counts the number of substring occurrences
            int substr_count(string haystack, string needle) 
                => haystack.Length - haystack.Replace(needle, string.Empty).Length;
        }

        /// <summary>
        /// Matches the beginning and the end of new lines.
        /// </summary>
        /// <param name="translation">The translation to match.</param>
        /// <param name="original">The original translation.</param>
        /// <returns></returns>
        protected internal static string MatchBeginAndEndNewlines(string translation, string original)
        {
            if (string.IsNullOrEmpty(translation)) return translation;

            var originalBegin = original.StartsWith(NEWLN);
            var originalEnd = original.EndsWith(NEWLN);
            var translationBegin = translation.StartsWith(NEWLN);
            var translationEnd = translation.EndsWith(NEWLN);

            if (originalBegin)
            {
                if (!translationBegin)
                {
                    translation = NEWLN + translation;
                }
            }
            else if (translationBegin)
            {
                translation = translation.TrimStart(NEWLNC);
            }

            if (originalEnd)
            {
                if (!translationEnd)
                {
                    translation += NEWLN;
                }
            }
            else if (translationEnd)
            {
                translation = translation.TrimEnd(NEWLNC);
            }

            return translation;
        }

        /// <summary>
        /// Inserts <paramref name="with"/> in the beginning of every new 
        /// line of <paramref name="s"/> and returns the modified string.
        /// </summary>
        /// <param name="s">Prepend lines in this string.</param>
        /// <param name="with">Prepend lines with this string.</param>
        /// <returns></returns>
        protected internal static string PrependEachLine(string s, string with)
        {
            var lines = new List<string>(s.Split(NEWLNC));
            var append = string.Empty;

            if (s.EndsWith(NEWLN) && string.Empty == lines.Last())
            {
                // Last line might be empty because 's' was terminated
                // with a newline, remove it from the 'lines' array,
                // we'll restore state by re-terminating the string at the end
                lines.RemoveAt(lines.Count - 1);
                append = NEWLN;
            }

            for (int i = 0; i < lines.Count; i++)
            {
                lines[i] = with + lines[i];
            }

            return string.Join(NEWLN, lines) + append;
        }

        /*
         export_headers() {
		$header_string = '';
		foreach($this->headers as $header => $value) {
			$header_string.= "$header: $value\n";
		}
		$poified = PO::poify($header_string);
		if ($this->comments_before_headers)
			$before_headers = $this->prepend_each_line(rtrim($this->comments_before_headers)."\n", '# ');
		else
			$before_headers = '';
		return rtrim("{$before_headers}msgid \"\"\nmsgstr $poified");
	}
             */
    }
}
