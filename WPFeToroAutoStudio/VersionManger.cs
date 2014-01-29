using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Applenium
{
    internal class VersionManger
    {
        internal int AddNewTestingVersion(int basedOnversionId)
        {
            //1.add line to testing versions
            //2.add column to GuiTestSteps
            //3.add column to GuiMap
            //4. add colum to ScenarioLogic
            //5. add column to BatchLogic
            return 0;
        }

        internal DataTable GetAllversions(int projectId)
        {
            DataTable dt=new DataTable();
            return dt;
        }

    }
}
