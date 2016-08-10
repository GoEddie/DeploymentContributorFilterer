using System.Configuration;
using NUnit.Framework;
using System.IO;

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
                $"/Action:Publish /TargetServerName:localhost /SourceFile:{Path.Combine(TestContext.CurrentContext.TestDirectory, "Dacpac.Dacpac")} /p:AdditionalDeploymentContributors=AgileSqlClub.DeploymentFilterContributor " +
                " /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True " +
                "/p:AdditionalDeploymentContributorArguments=\"SqlPackageFilter=KeepSecurity\" /p:AllowIncompatiblePlatform=true";

            var proc = new ProcessGateway( Path.Combine(TestContext.CurrentContext.TestDirectory,   "SqlPackage.exe\\SqlPackage.exe"), args);
            proc.Run();
            proc.WasDeploySuccess();

            count = _gateway.GetInt("SELECT COUNT(*) FROM sys.database_principals WHERE name = 'kjkjkjkj';");
            Assert.AreEqual(1, count, proc.Messages);
        
        }

    }
}
