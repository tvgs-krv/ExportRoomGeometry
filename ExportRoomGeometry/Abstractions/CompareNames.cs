using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ExportRoomGeometry.Abstractions
{
    public class CompareNames
    {
        public string GetBuildingName(string activeName)
        {
            if (!string.IsNullOrEmpty(activeName))
            {
                Regex regex = new Regex(@"[0-9][0-9][A-Z][A-Z][A-Z]");
                string mc = regex.Matches(activeName).OfType<Match>().ToList().Select(a => a.Value).FirstOrDefault();
                if (mc != null)
                    return mc;
            }
            return null;
        }
        public string GetBuildingLevel(string activeName)
        {
            if (!string.IsNullOrEmpty(activeName))
            {
                Regex regex = new Regex(@"[0-9][0-9][A-Z][A-Z][A-Z][0-9][0-9]");
                string mc = regex.Matches(activeName).OfType<Match>().ToList().Select(a => a.Value).FirstOrDefault();
                if (mc != null)
                    return mc;
            }
            return null;
        }

    }
}
