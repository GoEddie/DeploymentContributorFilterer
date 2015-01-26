using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgileSqlClub.SqlPackageFilter.Filter;

namespace AgileSqlClub.SqlPackageFilter.UnitTests.Config
{
    class TestDisplayMessageHandler : IDisplayMessageHandler
    {
        public void ShowMessage(string message, DisplayMessageLevel level)
        {
            Console.WriteLine(level + " : " + message);
        }

        public void SetMessageLevel(DisplayMessageLevel level)
        {
            
        }
    }
}
