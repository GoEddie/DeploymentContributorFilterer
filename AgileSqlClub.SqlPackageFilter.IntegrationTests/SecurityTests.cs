using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace AgileSqlClub.SqlPackageFilter.IntegrationTests
{
    [TestFixture]
    public class SecurityTests
    {
        private readonly SqlGateway _gateway = new SqlGateway(new AppSettingsReader().GetValue("ConnectionString", typeof(string)) as string);

        [Test]
        public void Keeps_Users_And_Roles()
        {
            _gateway.RunQuery("IF NOT EXISTS(SELECT * FROM sys.database_principals WHERE name = 'kjkjkjkj')  CREATE USER [kjkjkjkj] WITHOUT LOGIN;");
            var count = _gateway.GetInt("SELECT COUNT(*) FROM sys.database_principals WHERE name = 'kjkjkjkj';");
            Assert.AreEqual(1, count);

            var args =
                "/Action:Publish /TargetServerName:localhost /SourceFile:DacPac.dacpac /p:AdditionalDeploymentContributors=AgileSqlClub.DeploymentFilterContributor " +
                " /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True " +
                "/p:AdditionalDeploymentContributorArguments=\"SqlPackageFilter=KeepSecurity\"";

            var proc = new ProcessGateway(".\\SqlPackage.exe\\SqlPackage.exe", args);
            proc.Run();
            proc.WasDeploySuccess();

            count = _gateway.GetInt("SELECT COUNT(*) FROM sys.database_principals WHERE name = 'kjkjkjkj';");
            Assert.AreEqual(1, count, proc.Messages);
        
        }

    }
}
