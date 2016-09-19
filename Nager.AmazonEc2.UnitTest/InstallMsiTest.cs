using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nager.AmazonEc2.InstallScript;

namespace Nager.AmazonEc2.UnitTest
{
    [TestClass]
    public class InstallMsiTest
    {
        [TestMethod]
        public void InstallMsi()
        {
            var installScript = new WindowsInstallScript();
            var successful = installScript.InstallMsi("http://www.7-zip.org/a/7z1602-x64.msi");
            Assert.AreEqual(true, successful);
        }
    }
}
