using System.Collections.Generic;
using Microsoft.SqlServer.Dac;

namespace AgileSql
{
    class Program
    {
        //This is I can manually see how stuff looks so I can fiddle!

        static void Main(string[] args)
        {
            var dacServices = new DacServices("SERVER=.;Integrated Security=SSPI;initial catalog=SqlPackageFilter");

            using (DacPackage dacpac = DacPackage.Load(@"..\..\..\dacpac\bin\Debug\DacPac.dacpac", DacSchemaModelStorageType.Memory))
            {
                var options = new DacDeployOptions { AdditionalDeploymentContributors = "AgileSqlClub.DeploymentFilterContributor", DropObjectsNotInSource = true};
                var mesages = new List<string>();
                dacServices.Message += (sender, eventArgs) =>
                {
                    mesages.Add(eventArgs.Message.Message);
                };

                var script = dacServices.GenerateDeployScript(dacpac, "SqlPackageFilter", options);
            }
            
        }
    }
}
