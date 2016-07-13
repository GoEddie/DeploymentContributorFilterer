using System.Collections.Generic;
using Microsoft.SqlServer.Dac.Deployment;

namespace AgileSqlClub.SqlPackageFilter.VersionExecutor
{
    //[ExportDeploymentPlanModifier("AgileSqlClub.DeploymentFilterContributor", "0.1.0.0")]
    [ExportDeploymentPlanExecutor("AgileSqlClub.VersionExecutor", "0.2.0.0")]
    public class VersionExecutor : DeploymentPlanExecutor
    {
        protected override void OnExecute(DeploymentPlanContributorContext context)
        {
            
        }

        protected override void OnApplyDeploymentConfiguration(DeploymentContributorContext context, ICollection<DeploymentContributorConfigurationStream> configurationStreams)
        {
            base.OnApplyDeploymentConfiguration(context, configurationStreams);
        }

        protected override void OnEstablishDeploymentConfiguration(DeploymentContributorConfigurationSetup setup)
        {
            setup.SqlCmdVariables.Add("DacpacVersion", "999");
            base.OnEstablishDeploymentConfiguration(setup);
        }
    }
}
