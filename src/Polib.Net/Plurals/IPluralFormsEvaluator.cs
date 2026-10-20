namespace Polib.Net.Plurals
{
    /// <summary>
    /// Specifies the contract required by a class that returns the right translation index, according to a given plural forms parser.
    /// </summary>
    public interface IPluralFormsEvaluator
    {
        /// <summary>
        /// Evaluates the given number and returns the appriopriate plural index.
        /// </summary>
        /// <param name="n">The number whose plural form is to be evaluated.</param>
        /// <returns></returns>
        int Evaluate(long n);

        /// <summary>
        /// Executes the plural form function.
        /// </summary>
        /// <param name="n">Variable "n" to substitute in the expression.</param>
        /// <returns>A <see cref="System.Int32"/> that represents the plural form index.</returns>
        int Execute(long n);
    }
}
