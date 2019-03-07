using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgileSqlClub.SqlPackageFilter.IntegrationTests.PathFixer
{
    public class CopyDll
    {
        private string _source;

        public CopyDll(string directory)
        {
            _source = directory;
        }

        public void Fix()
        {

            if(File.Exists(Path.Combine(_source, "SqlPackage.exe", "Extensions", "AgileSqlClub.SqlPackageFilter.dll")))
            {
                File.Delete(Path.Combine(_source, "SqlPackage.exe", "Extensions", "AgileSqlClub.SqlPackageFilter.dll"));
            }

            File.Copy(Path.Combine(_source, "AgileSqlClub.SqlPackageFilter.dll"), Path.Combine(_source, "SqlPackage.exe", "Extensions", "AgileSqlClub.SqlPackageFilter.dll"));

        }
    }
}
