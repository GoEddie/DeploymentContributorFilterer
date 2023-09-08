using System.Configuration;
using System.IO;
using NUnit.Framework;

namespace AgileSqlClub.SqlPackageFilter.IntegrationTests
{
    [TestFixture]
    public class BasicTests
    {
        private readonly SqlGateway _gateway = new SqlGateway(new AppSettingsReader().GetValue("ConnectionString", typeof(string)) as string);

        [Test]
        public void Can_Publish_1_without_deploymentContributor()
        {
            _gateway.RunQuery(
                "IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Employees') exec sp_executesql N'delete from dbo.Employees';");

            var args =
                $"/Action:Publish /TargetServerName:(localdb)\\Filter /SourceFile:{Path.Combine(TestContext.CurrentContext.TestDirectory, "Dacpac.Dacpac")}" +
                $" /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True /p:AllowIncompatiblePlatform=true /p:BlockOnPossibleDataLoss=False ";

            var proc = new ProcessGateway(Path.Combine(TestContext.CurrentContext.TestDirectory, "SqlPackage.exe\\SqlPackage.exe"), args);
            proc.Run();
            proc.WasDeploySuccess();
            Assert.Pass(proc.Messages);

        }

        [Test]
        public void Can_Publish_2_with_deploymentContributor()
        {
            _gateway.RunQuery(
                "IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Employees') exec sp_executesql N'delete from dbo.Employees';");

            var args =
            $"/Action:Publish /TargetServerName:(localdb)\\Filter /SourceFile:{Path.Combine(TestContext.CurrentContext.TestDirectory, "Dacpac.Dacpac")} /p:AdditionalDeploymentContributors=AgileSqlClub.DeploymentFilterContributor " +
            $" /p:BlockOnPossibleDataLoss=False " +
            $" /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True /p:AllowIncompatiblePlatform=true";

            var proc = new ProcessGateway( Path.Combine(TestContext.CurrentContext.TestDirectory,   "SqlPackage.exe\\SqlPackage.exe"), args);
            proc.Run();
            proc.WasDeploySuccess();
            Assert.Pass(proc.Messages);

        }
        

    }
}
