using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polib.Net.Plurals;

namespace Polib.NetCore.Tests
{
    [TestClass]
    public class PluralFormTests
    {
        [TestMethod]
        public void Should_Evaluate_Plurals()
        {
            // ARRANGE
            var evaluators = CreateEvaluators();
            var results = new long[evaluators.Length];
            
            // ACT
            for (int i = 0; i < evaluators.Length; i++)
            {
                results[i] = evaluators[i].Evaluate(13);
            }
            
            // ASSERT
            Assert.AreEqual(2, results[0]);
            Assert.AreEqual(1, results[1]);
            Assert.AreEqual(4, results[2]);
            Assert.AreEqual(2, results[3]);
            Assert.AreEqual(3, results[4]);
        }

        static IPluralFormsEvaluator[] CreateEvaluators() => new[]
        {
            new PluralFormsEvaluator("n % 10 == 1 && n % 100 != 11 ? 0 : n % 10 >= 2 && n % 10 <= 4 && (n % 100 < 10 || n % 100 >= 20) ? 1 : 2"),
            new PluralFormsEvaluator("n == 1 ? 0 : n == 0 || (n % 100 > 0 && n % 100 < 20) ? 1 : 2"),
            new PluralFormsEvaluator("n == 0 ? 0 : n == 1 ? 1 : n == 2 ? 2 : n >= 3 && n <= 10 ? 3 : n >= 11 && n <= 99 ? 4 : 5"),
            new PluralFormsEvaluator("n == 1 || n == 11 ? 0 : n == 2 || n == 12 ? 1 : (n >= 3 && n <= 10) || (n >= 13 && n <= 19) ? 2 : 3"),
            new PluralFormsEvaluator("n % 100 == 1 ? 0 : n % 100 == 2 ? 1 : n % 100 == 3 || n % 100 == 4 ? 2 : 3"),
        };
    }
}
