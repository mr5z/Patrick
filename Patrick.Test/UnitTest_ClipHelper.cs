using Microsoft.VisualStudio.TestTools.UnitTesting;
using Patrick.Helpers;
using System.Linq;

namespace Patrick.Test
{
    [TestClass]
    public class UnitTest_ClipHelper
    {
        enum Parameter
        {
            Context,
            Options,
            Question
        }

        [TestMethod]
        public void TestMethod1()
        {
            var context = "CatsOrDogs";
            var question = "Testing";
            var opt = "'a b c' '1 2 3'";
            var options = CliHelper.ParseOptions($"-c {context} -o {opt} -q {question}",
                new CliHelper.Option<Parameter>(Parameter.Context, "-c", "--context"),
                new CliHelper.Option<Parameter>(Parameter.Question, "-q", "--question"),
                new CliHelper.Option<Parameter>(Parameter.Options, "-o", "--options")
            );

            var a = options[Parameter.Options];
            Assert.AreEqual(options[Parameter.Context].Single(), context);
            Assert.AreEqual(options[Parameter.Question].Single(), question);
            Assert.IsTrue(options[Parameter.Options].SequenceEqual(new[] { "a b c", "1 2 3" }));
        }
    }
}
