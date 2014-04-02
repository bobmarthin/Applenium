using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Applenium._4____Infrustructure
{
  /// <summary>
  ///  A utility class for general helper functions
  /// </summary>
   public static class Utilities
    {

       /// <summary>
       /// Gets the current timeStamp for execution run ID.
       /// </summary>
       /// <returns>Time Stamp as an Int</returns>
     public static int getTimeStamp()
       {
           return (Int32)(DateTime.Now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
       }
    }
}
