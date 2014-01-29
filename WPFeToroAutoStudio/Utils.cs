using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;


namespace WPFeToroAutoStudio
{
    internal class Utils
    {
        public string[,] DataTableToArray(DataTable dt)
        {
            var a = dt.Rows.Count;
            var b = dt.Columns.Count;
            var strArr = new string[a, b];
            var i = 0;
            foreach (System.Data.DataRow dRow in dt.Rows)
            {
                for (var j = 0; j < dt.Columns.Count; j++)
                {
                    strArr[i, j] = dRow[j].ToString().Trim();
                }
                i++;
            }

            return strArr;
        }
    }
}
