using Applenium._3___DAL.DataSetAutoTestTableAdapters;
using NUnit.Framework;

namespace Applenium
{
    [TestFixture]
    class DalUnitTests
    {
        [Test]
        public void GetGuiMap()
        {
            var guiMapAdapter = new GuiMapTableAdapter();
            var guimapname = guiMapAdapter.GetGuiMapName(1).Trim();
            Assert.AreEqual("OB_SignIn",guimapname);

        }

       [Test]
        public void GetTestSteps()
        {
            var guiTestStepsAdapter = new GuiTestStepsTableAdapter();
           var stepid = guiTestStepsAdapter.GetLastGuiTestStepsID();
            Assert.GreaterOrEqual(stepid,0);
        }
    }
}
