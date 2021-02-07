using Microsoft.VisualStudio.TestTools.UnitTesting;
using Patrick.Helpers;

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
                new CliHelper.Option<Parameter>(Parameter.Context, "-c", "-context"),
                new CliHelper.Option<Parameter>(Parameter.Question, "-q", "--question"),
                new CliHelper.Option<Parameter>(Parameter.Options, "-o", "--options")
            );

            Assert.AreEqual(options[Parameter.Context], context);
            Assert.AreEqual(options[Parameter.Question], question);
        }
    }
}
