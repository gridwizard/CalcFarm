using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalcFarm.AnalyticUtil
{
    public class ExceptionUtil
    {
        public static string ExceptionToString(Exception Ex)
        {
            return "Message: " + Ex.Message + ", Stack: " + Ex.StackTrace;
        }
    }
}
