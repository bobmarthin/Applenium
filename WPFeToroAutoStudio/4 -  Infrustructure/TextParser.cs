using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Applenium
{
    class TextParser
    {
        public string GetIis(string pageSourse)
        {

            //<!-- OB-IIS-144  -->
            //Match m = Regex.Match(pageSourse, @"<!-- OB-IIS-)(?<servername>.*) -->");
            ////Match m = Regex.Match(pageSourse, @"(?i:http://)(?<servername>[^\s/]*)/(?<username>[^\s/]*)/(?<collectionkey>\d*)(/(?<sectionkey>\d*))?(\.xml)?");
            //if (m.Success)
            //{
            //    return "OB-IIS"+ m.Groups["servername"].Value;
                
            //}
            //return string.Empty;

            int indexstart = pageSourse.IndexOf("<!-- OB-IIS", 1, System.StringComparison.Ordinal);
            if (indexstart > 0)
            {
                int endindex = pageSourse.IndexOf("-->", indexstart, System.StringComparison.Ordinal);

                string iis = pageSourse.Substring(indexstart+5, endindex - indexstart - 6);
                return iis;

            }
            return string.Empty;
        }
    }
}
