using System;
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
