using Polib.Net.Plurals;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Polib.Net
{
    /// <summary>
    /// Represents a catalog of translated items.
    /// </summary>
    public class Catalog : ICatalog
    {
        #region fields

        string _filename;
        IPluralFormsEvaluator _evaluator;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Catalog"/> class.
        /// </summary>
        public Catalog()
        {
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            Entries = new Dictionary<string, ITranslation>();
        }

        #endregion

        #region properties

        /// <summary>
        /// Gets or sets the first comments before the <see cref="Headers"/> collection.
        /// </summary>
        public string HeaderComments { get; set; }

        /// <summary>
        /// Gets the headers of this reader.
        /// </summary>
        public virtual IDictionary<string, string> Headers { get; }

        /// <summary>
        /// Gets the list of translation entries.
        /// </summary>
        public virtual IDictionary<string, ITranslation> Entries { get; }

        /// <summary>
        /// Gets the culture of the elements contained in <see cref="Entries"/>.
        /// </summary>
        public virtual System.Globalization.CultureInfo Culture { get; set; }

        /// <summary>
        /// Gets or sets the name of the file used during the read operation.
        /// </summary>
        public virtual string FileName
        {
            get => _filename;
            set
            {
                _filename = value ?? string.Empty;
                FileId = Methods.Hash(_filename, true);
            }
        }

        /// <summary>
        /// Gets the unique file identifier of the current catalog.
        /// </summary>
        public virtual string FileId { get; private set; }

        /// <summary>
        /// Gets or sets the date the file was last read.
        /// </summary>
        public virtual DateTime LastAccessTime { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the number of plural forms.
        /// </summary>
        public virtual int PluralCount { get; set; } = 2;

        /// <summary>
        /// Gets or sets the plural forms evaluator.
        /// </summary>
        public virtual IPluralFormsEvaluator PluralEvaluator
        {
            get
            {
                if (_evaluator == null)
                {
                    _evaluator = new PluralFormsEvaluator(n =>
                    {
                        var idx = PluralFormsEvaluator.GetPluralIndex(Culture, n, out int count);
                        PluralCount = count;
                        return idx;
                    });
                    // evaluate first time to initialize the PluralCount property
                    _evaluator.Evaluate(0L);
                }
                return _evaluator;
            }
            set => _evaluator = value;
        }

        #endregion

        #region public methods

        /// <summary>
        /// Attempts to determine the character set defined in the 'Content-Type' header.
        /// </summary>
        /// <param name="value">Returns the character set, if present.</param>
        /// <returns></returns>
        public virtual bool GetCharset(out string value)
        {
            if (Headers.ContainsKey("Content-Type"))
            {
                // e.g.: text/plain; charset=UTF-8
                var contentType = Headers["Content-Type"];
                var m = Regex.Match(contentType, @"charset\s*=\s*([\w-\d]+)", RegexOptions.IgnoreCase);

                if (m.Success)
                {
                    value = m.Groups[1].Value;
                    return true;
                }
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Attempts to return the value of the specified header name.
        /// </summary>
        /// <param name="name">The name of the header to return.</param>
        /// <param name="value">Returns the value of the header, if found.</param>
        /// <returns></returns>
        public virtual bool GetHeader(string name, out string value)
        {
            if (Headers.ContainsKey(name))
            {
                value = Headers[name];
                return true;
            }
            value = null;
            return false;
        }

        /// <summary>
        /// Finds the character set and returns a new instance of the corresponding <see cref="Encoding"/> class.
        /// </summary>
        /// <returns>An initialized instance of the <see cref="Encoding"/> class, or null.</returns>
        public virtual Encoding GetEncoding()
        {
            if (GetCharset(out var value))
            {
                return Encoding.GetEncoding(value);
            }
            return null;
        }
        
        #endregion
    }
}
