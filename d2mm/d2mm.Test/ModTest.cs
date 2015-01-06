using System;
using System.IO;
using NUnit.Framework;

namespace de.sebastianrutofski.d2mm.Test
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

        [TestCase(EmptyModDir, TestName="Empty Dir")]
        [TestCase(FilledModDir, TestName="Filled Dir")]
        public static void TestCreateFromDir(string dir)
        {
            Mod mod = Mod.CreateFromDirectory(dir);
            Assert.NotNull(mod);
            Assert.AreEqual(mod.Name, Path.GetFileName(Path.GetDirectoryName(dir + Path.DirectorySeparatorChar)));
        }

        [TestCase("", "", "0.0", TestName = "Empty String")]
        [TestCase("{}", "", "0.0", TestName="Only Brackets")]
        [TestCase("{\"Name\":\"Test Mod\",\"Version\":\"1.2.3.4\"}", "Test Mod", "1.2.3.4", TestName="Normal")]
        public static void TestCreateFromString(string source, string modName, string modVersion)
        {
            Mod mod = Mod.CreateFromString(source);
            Assert.NotNull(mod);
            Assert.AreEqual(mod.Name, modName);
            Assert.AreEqual(mod.Version, Version.Parse(modVersion));
        }
    }
}
