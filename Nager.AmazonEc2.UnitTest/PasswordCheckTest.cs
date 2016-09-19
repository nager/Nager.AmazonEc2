using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nager.AmazonEc2.InstallScript;

namespace Nager.AmazonEc2.UnitTest
{
    [TestClass]
    public class PasswordCheckTest
    {
        [TestMethod]
        public void CheckPassword()
        {
            var installScript = new WindowsInstallScript();
            var isComplex = installScript.IsComplexPassword("asdf");
            Assert.AreEqual(false, isComplex);

            isComplex = installScript.IsComplexPassword("asdfASDF");
            Assert.AreEqual(false, isComplex);

            isComplex = installScript.IsComplexPassword("asdfASDF123");
            Assert.AreEqual(true, isComplex);

            isComplex = installScript.IsComplexPassword("asdfASDF+");
            Assert.AreEqual(true, isComplex);

            isComplex = installScript.IsComplexPassword("asdf@ASDF");
            Assert.AreEqual(true, isComplex);

            isComplex = installScript.IsComplexPassword("asdf$1234");
            Assert.AreEqual(true, isComplex);
        }
    }
}
