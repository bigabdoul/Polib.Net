using System;
using System.Collections.Generic;
using System.Globalization;

namespace Polib.Net.Plurals
{
    /// <summary>
    /// Represents a class that returns the right translation index, according to a given plural forms expression.
    /// </summary>
    public class PluralFormsEvaluator : IPluralFormsEvaluator
    {
        #region fields

        private bool _parsed;

        /// <summary>
        /// Gets the plural forms parser.
        /// </summary>
        readonly IPluralFormsParser Parser;

        /// <summary>
        /// Tokens generated from the string.
        /// </summary>
        protected IList<KeyValuePair<string, string>> Tokens;
        private Func<long, int> Evaluator;

        /// <summary>
        /// Cache for repeated calls to the <see cref="Evaluate(long)"/> function.
        /// </summary>
        protected readonly IDictionary<long, int> EvaluatorCache = new Dictionary<long, int>();

        #endregion

        #region static fields

        /// <summary>
        /// Gets the default plural forms evaluator.
        /// </summary>
        public static readonly PluralFormsEvaluator Default = new PluralFormsEvaluator("n != 1");

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PluralFormsEvaluator"/> class using the specified value.
        /// </summary>
        /// <param name="expression">The plural forms expression to parse and evaluate when needed.</param>
        public PluralFormsEvaluator(string expression) : this(new PluralFormsParser(expression))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PluralFormsEvaluator"/> class using the specified parameter.
        /// </summary>
        /// <param name="parser">The plural forms parser.</param>
        public PluralFormsEvaluator(IPluralFormsParser parser)
        {
            Parser = parser ?? throw new ArgumentNullException(nameof(parser));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PluralFormsEvaluator"/> class using the specified function.
        /// </summary>
        /// <param name="evaluator">A function that evaluates the plural form.</param>
        public PluralFormsEvaluator(Func<long, int> evaluator)
        {
            Evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
        }

        #endregion

        #region methods

        /// <summary>
        /// Evaluates the given number and returns the appriopriate plural index.
        /// </summary>
        /// <param name="n">The number whose plural form is to be evaluated.</param>
        /// <returns><see cref="Int32"/> representing the plural form value.</returns>
        public virtual int Evaluate(long n)
        {
            if (EvaluatorCache.ContainsKey(n))
            {
                return EvaluatorCache[n];
            }
            var result = Execute(n);
            EvaluatorCache.Add(n, result);
            return result;
        }

        /// <summary>
        /// Execute the plural form function.
        /// </summary>
        /// <param name="n">Variable "n" to substitute.</param>
        /// <returns>Plural form value.</returns>
        public virtual int Execute(long n)
        {
            if (!_parsed)
                parse();

            if (Evaluator != null)
                return Evaluator(n);

            var stack = new Stack<long>();
            var i = 0;
            var total = Tokens.Count;
            long v1, v2;

            while (i < total)
            {
                var next = Tokens[i];
                i++;
                if (next.Key == "var")
                {
                    stack.Push(n);
                    continue;
                }
                else if (next.Key == "value")
                {
                    stack.Push(long.Parse(next.Value));
                    continue;
                }

                // Only operators left.
                switch (next.Value)
                {
                    case "%":
                        v2 = stack.Pop();
                        v1 = stack.Pop();
                        stack.Push(v1 % v2);
                        break;

                    case "||":
                        v2 = stack.Pop();
                        v1 = stack.Pop();
                        stack.Push(v1 != 0 || v2 != 0 ? 1 : 0);
                        break;

                    case "&&":
                        v2 = stack.Pop();
                        v1 = stack.Pop();
                        stack.Push(v1 != 0 && v2 != 0 ? 1 : 0);
                        break;

                    case "<":
                        v2 = stack.Pop();
                        v1 = stack.Pop();
                        stack.Push(v1 < v2 ? 1 : 0);
                        break;

                    case "<=":
                        v2 = stack.Pop();
                        v1 = stack.Pop();
                        stack.Push(v1 <= v2 ? 1 : 0);
                        break;

                    case ">":
                        v2 = stack.Pop();
                        v1 = stack.Pop();
                        stack.Push(v1 > v2 ? 1 : 0);
                        break;

                    case ">=":
                        v2 = stack.Pop();
                        v1 = stack.Pop();
                        stack.Push(v1 >= v2 ? 1 : 0);
                        break;

                    case "!=":
                        v2 = stack.Pop();
                        v1 = stack.Pop();
                        stack.Push(v1 != v2 ? 1 : 0);
                        break;

                    case "==":
                        v2 = stack.Pop();
                        v1 = stack.Pop();
                        stack.Push(v1 == v2 ? 1 : 0);
                        break;

                    case "?:":
                        var v3 = stack.Pop();
                        v2 = stack.Pop();
                        v1 = stack.Pop();
                        stack.Push(v1 != 0 ? v2 : v3);
                        break;

                    default:
                        throw new Exception($"Unknown operator \"{next.Value}\".");
                }
            }

            if (stack.Count != 1)
                throw new Exception("Too many values remaining on the stack");

            return Convert.ToInt32(stack.Pop());

            void parse()
            {
                if (Parser != null)
                {
                    Tokens = Parser.Parse() ??
                        throw new InvalidOperationException("The specified plural forms parser must return a non-null list of tokens");
                }
                else if (Evaluator == null)
                {
                    throw new InvalidOperationException("No plural forms evaluator specified.");
                }
                _parsed = true;
            }
        }

        #endregion

        #region static methods

        /// <summary>
        /// Returns the plural forms index based on the specified culture and number <paramref name="n"/>.
        /// </summary>
        /// <param name="culture">The culture used to determine the plural form index.</param>
        /// <param name="n">The number to evaluate.</param>
        /// <returns></returns>
        public static int GetPluralIndex(CultureInfo culture, long n) => GetPluralIndex(culture, n, out var nplurals);

        /// <summary>
        /// Returns the plural forms index based on the specified culture and number <paramref name="n"/>.
        /// </summary>
        /// <param name="culture">The culture used to determine the plural form index.</param>
        /// <param name="n">The number to evaluate.</param>
        /// <param name="nplurals">Returns the number of plural forms for the specified culture.</param>
        /// <returns></returns>
        public static int GetPluralIndex(CultureInfo culture, long n, out int nplurals)
        {
            nplurals = 2;

            switch (culture.TwoLetterISOLanguageName)
            {
                // Only one form:
                case "az":
                case "bm":
                case "bo":
                case "dz":
                case "fa":
                case "id":
                case "ig":
                case "ii":
                case "hu":
                case "ja":
                case "jv":
                case "ka":
                case "kde":
                case "kea":
                case "km":
                case "kn":
                case "ko":
                case "lo":
                case "ms":
                case "my":
                case "sah":
                case "ses":
                case "sg":
                case "th":
                case "to":
                case "tr":
                case "vi":
                case "wo":
                case "yo":
                case "zh":
                    nplurals = 1;
                    return 0;

                // Two forms, singular used for one only
                case "asa":
                case "af":
                case "bem":
                case "bez":
                case "bg":
                case "bn":
                case "brx":
                case "ca":
                case "cgg":
                case "chr":
                case "da":
                case "de":
                case "dv":
                case "ee":
                case "el":
                case "en":
                case "eo":
                case "es":
                case "et":
                case "eu":
                case "fi":
                case "fo":
                case "fur":
                case "fy":
                case "gl":
                case "gsw":
                case "gu":
                case "ha":
                case "haw":
                case "he":
                case "is":
                case "it":
                case "jmc":
                case "kaj":
                case "kcg":
                case "kk":
                case "kl":
                case "ksb":
                case "ku":
                case "lb":
                case "lg":
                case "mas":
                case "ml":
                case "mn":
                case "mr":
                case "nah":
                case "nb":
                case "nd":
                case "ne":
                case "nl":
                case "nn":
                case "no":
                case "nr":
                case "ny":
                case "nyn":
                case "om":
                case "or":
                case "pa":
                case "pap":
                case "ps":
                case "pt":
                case "rof":
                case "rm":
                case "rwk":
                case "saq":
                case "seh":
                case "sn":
                case "so":
                case "sq":
                case "ss":
                case "ssy":
                case "st":
                case "sv":
                case "sw":
                case "syr":
                case "ta":
                case "te":
                case "teo":
                case "tig":
                case "tk":
                case "tn":
                case "ts":
                case "ur":
                case "wae":
                case "ve":
                case "vun":
                case "xh":
                case "xog":
                case "zu":
                    return n != 1 ? 1 : 0;

                // Two forms, singular used for zero and one
                case "ak":
                case "am":
                case "bh":
                case "fil":
                case "ff":
                case "fr":
                case "guw":
                case "hi":
                case "kab":
                case "ln":
                case "mg":
                case "nso":
                case "ti":
                case "wa":
                    return n > 1 ? 1 : 0;

                // Two forms, special cases for zero, one and numbers between 11 and 99
                case "tzm":
                    return n == 0 || n == 1 || (n >= 11 && n <= 99) ? 0 : 1;

                case "gv":
                    return n % 10 == 1 || n % 10 == 2 || n % 20 == 0 ? 0 : 1;

                case "mk":
                    return n % 10 == 1 && n != 11 ? 0 : 1;

                // Three forms, special case for zero
                case "lv":
                    nplurals = 3;
                    return n % 10 == 1 && n % 100 != 11 ? 0
                        : n != 0 ? 1 : 2;

                // Three forms, special cases for one and two
                case "ga":
                    nplurals = 3;
                    return n == 1 ? 0 : n == 2 ? 1 : 2;

                // Three forms, special case for numbers ending in 1[2-9]
                case "lt":
                    nplurals = 3;
                    return n % 10 == 1 && n % 100 != 11 ? 0
                        : n % 10 >= 2 && (n % 100 < 10 || n % 100 >= 20) ? 1 : 2;

                // Three forms, special cases for numbers ending in 1 and 2, 3, 4, except those ending in 1[1-4]
                case "be":
                case "bs":
                case "hr":
                case "ru":
                case "sh":
                case "sr":
                case "uk":
                    nplurals = 3;
                    return n % 10 == 1 && n % 100 != 11 ? 0
                        : n % 10 >= 2 && n % 10 <= 4 && (n % 100 < 10 || n % 100 >= 20) ? 1 : 2;

                // Three forms, special cases for 1 and 2, 3, 4: Slovak, Czech
                case "sk":
                case "cs":
                    nplurals = 3;
                    return n == 1 ? 0 : n >= 2 && n <= 4 ? 1 : 2;

                // Three forms, special case for one and some numbers ending in 2, 3, or 4
                case "pl":
                    nplurals = 3;
                    return n == 1 ? 0
                        : n % 10 >= 2 && n % 10 <= 4 && (n % 100 < 10 || n % 100 >= 20) ? 1 : 2;

                case "ro":
                case "mo":
                    nplurals = 3;
                    return n == 1 ? 0
                        : n == 0 || (n % 100 > 0 && n % 100 < 20) ? 1 : 2;

                case "iu":
                case "kw":
                case "naq":
                case "se":
                case "sma":
                case "smi":
                case "smj":
                case "smn":
                case "sms":
                    nplurals = 3;
                    return n == 1 ? 0 : n == 2 ? 1 : 2;

                case "lag":
                case "ksh":
                    nplurals = 3;
                    return n == 0 ? 0 : n == 1 ? 1 : 2;

                case "shi":
                    nplurals = 3;
                    return n == 0 && n == 1 ? 0
                        : n >= 2 && n <= 10 ? 1 : 2;

                // Four forms, special case for one and all numbers ending in 02, 03, or 04
                case "sl":
                    nplurals = 4;
                    return n % 100 == 1 ? 0
                        : n % 100 == 2 ? 1
                            : n % 100 == 3 || n % 100 == 4 ? 2 : 3;

                case "gd":
                    nplurals = 4;
                    return n == 1 || n == 11 ? 0
                        : n == 2 || n == 12 ? 1
                            : (n >= 3 && n <= 10) || (n >= 13 && n <= 19) ? 2 : 3;

                // Six forms
                case "ar":
                    nplurals = 6;
                    return n == 0 ? 0
                        : n == 1 ? 1
                            : n == 2 ? 2
                                : n >= 3 && n <= 10 ? 3
                                    : n >= 11 && n <= 99 ? 4 : 5;
                case "cy":
                    nplurals = 6;
                    return n == 0 ? 0
                        : n == 1 ? 1
                            : n == 2 ? 2
                                : n == 3 ? 3
                                    : n == 6 ? 4 : 5;
            }

            return n != 1 ? 1 : 0;
        }

        #endregion
    }
}
