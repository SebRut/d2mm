using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using de.sebastianrutofski.d2mm;
using NUnit.Framework;

namespace d2mm.Test
{
    [TestFixture]
    public class ModTest
    {
        private const string EmptyModDir = "Supahh test Mod";
        private const string FilledModDir = "Senseful Spirit breaker Mod";

        [SetUp]
        public static void SetUp()
        {
            Directory.CreateDirectory(EmptyModDir);
            Directory.CreateDirectory(FilledModDir);
            Directory.CreateDirectory(Path.Combine(FilledModDir, "head"));
        }

        [TearDown]
        public static void TearDown()
        {
            Directory.Delete(EmptyModDir, true);
            Directory.Delete(FilledModDir, true);
        }

        [Test]
        public static void TestCreateFromNullDir()
        {
            Mod mod;
            Assert.Throws(typeof(ArgumentNullException),() => Mod.CreateFromDirectory(null, out mod));
        }

        [TestCase("C:\\dirs\\ßß * /\\" , TestName="Invalid Chars")]
        [TestCase(EmptyModDir, TestName="Empty Dir")]
        [TestCase(FilledModDir, TestName="Filled Dir")]
        public static void TestCreateFromDir(string dir)
        {
            Assert.NotNull(Mod.CreateFromDirectory(dir));
        }

        [TestCase("", TestName="Empty String")]
        [TestCase("{}", TestName="Only Brackets")]
        [TestCase("{\"Name\":\"Test Mod\",\"Version\":\"1.2.3.4\"}", TestName="Normal")]
        public static void TestCreateFromString(string source)
        {
            Assert.NotNull(Mod.CreateFromString(source));
        }
    }
}
