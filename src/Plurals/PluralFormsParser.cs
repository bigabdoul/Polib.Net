using System;
using System.Collections.Generic;

namespace Polib.Net.Plurals
{
    /// <summary>
    /// Represents a gettext Plural-Forms parser. Ported from WordPress 4.9.0.
    /// </summary>
    public class PluralFormsParser : IPluralFormsParser
    {
        #region constants
        
        /// <summary>
        /// Operator characters.
        /// </summary>
        const string OP_CHARS = "|&><!=%?:";

        /// <summary>
        /// Valid number characters.
        /// </summary>
        const string NUM_CHARS = "0123456789";

        #endregion

        #region private fields

        private bool _parsed;
        private readonly string _expr;

        #endregion

        #region protected fields

        /// <summary>
        /// Operator precedence from highest to lowest. Higher numbers indicate higher precedence, and are executed first.
        /// </summary>
        protected static Dictionary<string, int> OpPrecedence = new Dictionary<string, int>()
        {
            { "%", 6 },
            { "<", 5 },
            { "<=", 5 },
            { ">", 5 },
            { ">=", 5 },
            { "==", 4 },
            { "!=", 4 },
            { "&&", 3 },
            { "||", 2 },
            { "?:", 1 },
            { "?", 1 },
            { "(", 0 },
            { ")", 0 },
        };

        /// <summary>
        /// Tokens generated from the string.
        /// </summary>
        protected IList<KeyValuePair<string, string>> Tokens;

        #endregion

        #region constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="PluralFormsParser"/> class using the specified parameter.
        /// </summary>
        /// <param name="expression">The plural function to parse (just the bit after 'plural=' from Plural-Forms)</param>
        public PluralFormsParser(string expression)
        {
            _expr = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        #endregion

        #region public methods

        /// <summary>
        /// Parse a Plural-Forms string into tokens.
        /// </summary>
        public virtual IList<KeyValuePair<string, string>> Parse()
        {
            if (_parsed) return Tokens;

            var pos = 0;
            var len = _expr.Length;
            var found = false;

            // Convert infix operators to postfix using the shunting-yard algorithm.
            var output = new List<KeyValuePair<string, string>>();
            var stack = new Collections.WritableStack<string>();

            while (pos < len)
            {
                var next = _expr[pos];

                switch (next)
                {
                    case ' ':
                    case '\t':
                        pos++;
                        break;

                    // Variable (n)
                    case 'n':
                        output.Add(new KeyValuePair<string, string>("var", string.Empty));
                        pos++;
                        break;

                    // Parentheses
                    case '(':
                        stack.Push(next.ToString());
                        pos++;
                        break;

                    case ')':
                        found = false;
                        while (stack.Count > 0)
                        {
                            var o2 = stack.Peek();

                            if (o2 != "(")
                            {
                                output.Add(new KeyValuePair<string, string>("op", stack.Pop()));
                                continue;
                            }

                            // Discard open paren.
                            stack.Pop();
                            found = true;
                            break;
                        }

                        if (!found)
                            throw new Exception("Mismatched parentheses");

                        pos++;
                        break;

                    // Operators
                    case '|':
                    case '&':
                    case '>':
                    case '<':
                    case '!':
                    case '=':
                    case '%':
                    case '?':
                        var end_operator = Strspn(_expr, OP_CHARS, pos);
                        var oprator = _expr.Substring(pos, end_operator);

                        if (!OpPrecedence.ContainsKey(oprator))
                            throw new Exception(string.Format("Unknown operator \"{0}\"", oprator));

                        while (stack.Count > 0)
                        {
                            var o2 = stack.Peek();

                            // Ternary is right-associative in C
                            if (oprator == "?:" || oprator == "?")
                            {
                                if (OpPrecedence[oprator] >= OpPrecedence[o2])
                                    break;
                            }
                            else if (OpPrecedence[oprator] > OpPrecedence[o2])
                            {
                                break;
                            }

                            output.Add(new KeyValuePair<string, string>("op", stack.Pop()));
                        }

                        stack.Push(oprator);
                        pos += end_operator;
                        break;

                    // Ternary "else"
                    case ':':
                        found = false;
                        while (stack.Count > 0)
                        {
                            var o2 = stack.Peek();

                            if (o2 != "?")
                            {
                                output.Add(new KeyValuePair<string, string>("op", stack.Pop()));
                                continue;
                            }

                            // Replace.
                            stack.Set("?:");
                            found = true;
                            break;
                        }

                        if (!found)
                            throw new Exception("Missing starting \"?\" ternary operator");

                        pos++;
                        break;

                    // Default - number or invalid
                    default:
                        if (next >= '0' && next <= '9')
                        {
                            var span = Strspn(_expr, NUM_CHARS, pos);
                            output.Add(new KeyValuePair<string, string>("value", _expr.Substring(pos, span)));
                            pos += span;
                            continue;
                        }

                        if (next == ';' || next == '\n' || next == '\0')
                        {
                            pos++;
                            continue;
                        }
                        
                        throw new Exception(string.Format("Unknown symbol \"{0}\"", next));
                }
            }

            while (stack.Count > 0)
            {
                var o2 = stack.Pop();

                if (o2 == "(" || o2 == ")")
                    throw new Exception("Mismatched parentheses");

                output.Add(new KeyValuePair<string, string>("op", o2));
            }

            _parsed = true;
            return Tokens = output;
        }

        /// <summary>
        /// Parse a Plural-Forms expression string into tokens.
        /// </summary>
        /// <param name="expression">The plural function to parse (just the bit after 'plural=' from Plural-Forms)</param>
        /// <returns></returns>
        public static PluralFormsParser Parse(string expression) => new PluralFormsParser(expression);

        #endregion

        #region protected methods

        /// <summary>
        /// Finds the length of the initial segment of a string consisting 
        /// entirely of characters contained within a given mask.
        /// </summary>
        /// <param name="subject">The string to seek.</param>
        /// <param name="mask">The string within which to search.</param>
        /// <param name="start">The position within <paramref name="subject"/> at which to start seeking.</param>
        /// <param name="length">The number of characters to count. Zero or less means all characters.</param>
        /// <returns></returns>
        /// <remarks>Mimics the PHP 'strspn' function. </remarks>
        protected int Strspn(string subject, string mask, int start = 0, int length = 0)
        {
            var count = 0;
            var len = subject.Length - (length <= 0 ? 0 : length);

            for (int i = start; i < len; i++)
            {
                if (mask.Contains(subject[i].ToString()))
                {
                    count++;
                    continue;
                }
                break;
            }

            return count;
        }

        #endregion
    }
}
