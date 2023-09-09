using System;
using System.Configuration;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Execution;

namespace AgileSqlClub.SqlPackageFilter.IntegrationTests
{
    [TestFixture]
    public class BasicTests
    {
        private readonly SqlGateway _gateway = new SqlGateway(new AppSettingsReader().GetValue("ConnectionString", typeof(string)) as string);

        [Test]
        [TestCase("SqlPackage.exe")]
        [TestCase("SqlPackage.exe.old")]

        public void Can_Publish_1_without_deploymentContributor(string SqlPackagePath)
        {
            _gateway.RunQuery(
                "IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Employees') exec sp_executesql N'delete from dbo.Employees';");

            var args =
                $"/Action:Publish /TargetServerName:(localdb)\\Filter /SourceFile:{Path.Combine(TestContext.CurrentContext.TestDirectory, "Dacpac.Dacpac")}" +
                $" /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True /p:AllowIncompatiblePlatform=true /p:BlockOnPossibleDataLoss=False ";

            var proc_ver = new ProcessGateway(Path.Combine(TestContext.CurrentContext.TestDirectory, SqlPackagePath + "\\SqlPackage.exe"), "/version");
            proc_ver.Run();
            Console.WriteLine("Version: " + proc_ver.Messages);

            var proc = new ProcessGateway(Path.Combine(TestContext.CurrentContext.TestDirectory, SqlPackagePath + "\\SqlPackage.exe"), args);
            proc.Run();
            proc.WasDeploySuccess();

            Assert.Pass(proc.Messages);

        }

        [Test]
        [TestCase("SqlPackage.exe")]
        [TestCase("SqlPackage.exe.old")]
        public void Can_Publish_2_with_deploymentContributor(string SqlPackagePath)
        {
            _gateway.RunQuery(
                "IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Employees') exec sp_executesql N'delete from dbo.Employees';");

            var args =
            $"/Action:Publish /TargetServerName:(localdb)\\Filter /SourceFile:{Path.Combine(TestContext.CurrentContext.TestDirectory, "Dacpac.Dacpac")} /p:AdditionalDeploymentContributors=AgileSqlClub.DeploymentFilterContributor " +
            $" /p:BlockOnPossibleDataLoss=False " +
            $" /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True /p:AllowIncompatiblePlatform=true";
            var proc_ver = new ProcessGateway(Path.Combine(TestContext.CurrentContext.TestDirectory, SqlPackagePath+"\\SqlPackage.exe"), "/version");
            proc_ver.Run();
            Console.WriteLine("Version: "+proc_ver.Messages);


            var proc = new ProcessGateway( Path.Combine(TestContext.CurrentContext.TestDirectory, SqlPackagePath + "\\SqlPackage.exe"), args);
            proc.Run();
            proc.WasDeploySuccess();
            Assert.Pass(proc.Messages);

        }
        

    }
}
