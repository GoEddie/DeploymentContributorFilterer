# DeploymentContributorFilterer

[![Build](https://github.com/GoEddie/DeploymentContributorFilterer/actions/workflows/dotnet-desktop.yml/badge.svg)](https://github.com/GoEddie/DeploymentContributorFilterer/actions/workflows/dotnet-desktop.yml)

Generic implementation of a DacFx deployment contributor in .net. Use this tool to filter sql objects during dacpac deployment process by SqlPackage.exe.

Original documentation and discussion adapted from:
*https://the.agilesql.club/2015/01/howto-filter-dacpac-deployments/*

## Basic Usage
Download the latest release from Github or build yourself. 

Put the AgileSqlClub.SqlPackageFilter.dll file into some folder (eg, c:\dac160tools\), and add these commmand line parameters to your deployment:

```
/p:AdditionalDeploymentContributors=AgileSqlClub.DeploymentFilterContributor 
/p:AdditionalDeploymentContributorPaths=c:\dac160tools\ 
/p:AdditionalDeploymentContributorArguments="SqlPackageFilter=IgnoreSchema(BLAH)"
```

This will neither deploy, drop or alter anything in the BLAH schema.

## Bootstrapping custom filters with SqlPackage.exe
Ok so the way the DacFx api works is it needs to load the contributor dlls. It can do this by looking in its own folder, or looking in the paths specified with the AdditionalDeploymentContributorPaths argument.  This becomes necessary when SqlPackage is installed as a dotnet global tool.

```
/p:AdditionalDeploymentContributors=AgileSqlClub.DeploymentFilterContributor 
/p:AdditionalDeploymentContributorPath=C:\dac160tools\
```

**Note:** Windows may block the AgileSqlClub.DeploymentFilterContributor.dll after downloading.  You will need to view the file properties and *Unblock* it.

## Types of Filters
There are two types of filters: **Keep** and **Ignore**.

**Keep** filters stop objects being dropped when they are in the dacpac but not the destination, if they are in the dacpac and not in the destination *or are different* then they will be created or altered.

Keep are really only ever used in combination with */p:DropObjectsInSource=True* otherwise they wouldn’t be dropped anyway.

**Ignore** filters stop any sort of operation, create, alter or drop so there is some flexibility.

Once you know what type of filter you want you need to decide what you will filter on, your choices are: **Name**, **SchemaName** and **object type** (stored procedure, function, table, user, role, rolemembership etc etc).

* Name filters work on an objects name, pretty straight forward. You can also specify a comma-separated set of names to match multipart identifiers. See the examples section for more details. Note that parts are matched from right to left.
* Schema filters work on the name of the schema so you can keep or ignore everything in a specific schema
* Object type filters work on the type of the object as the DacFx api sees it, these types are all documented as properties of the ModelSchema class: [link](http://msdn.microsoft.com/en-us/library/microsoft.sqlserver.dac.model.modelschema.aspx)

The object types are all fields, so the list starts Aggregate, ApplicationRole etc etc. Once you have decided how you will filter you specify the filter itself which is a regex, but don’t be scared it doesn’t have to be complex.

## Examples
Because of the way we pass the arguments to SqlPackage.exe and it then parses them and passes them onto the deployment contributor it is a little rigid, but essentially the filter itself look like:

To keep everything in dbo:
```
KeepSchema(dbo)
```

To ignore all Tables:
```
IgnoreType(Table)
```

To keep a table called MyTable or MyExcellentFunnyTable:
```
KeepName(.*yTabl.*)
```

Note that this will match the right-most part of a multipart identifier, so this matches: `[dbo].[MyTable]`, `[dbo].[MyExcellentFunnyTable]`,
`[dev].[MyTable]`, `[dbo].[SomeOtherTable].[MyTabletColumnId]`, etc.

You can match against multiple parts of a multipart identifier, matching from right part to left, by specifying multiple
values (regexes) separated by a comma:
```
KeepName(dbo,.*yTabl.*)
```

Using the example above, this instead matches only `[dbo].[MyTable]` or `[dbo].[MyExcellentFunnyTable]`.

Behind the scenes, matching relies on regex using the default .Net options for the Match method. 
To only deploy to the dbo schema (ie exclude non-dbo objects):
```
IgnoreSchema(^(?!\b(?i)dbo\b).*)
```

When you have decided on the filter you use need to pass it to SqlPackage.exe using:
```
/p:AdditionalDeploymentContributorArguments="SqlPackageFilter=KeepSecurity"
```

You can specify multiple filters by seperating them with a semi colon so and adding a uniqeifier to the end of each arg name:

```
/p:AdditionalDeploymentContributorArguments="SqlPackageFilter0=KeepSecurity;SqlPackageFilter1=IgnoreSchema(dev)"
```

(The reason for the uniqueifier is detailed: https://connect.microsoft.com/SQLServer/feedback/details/1112969)




### Contributing

If you would like to contribute and want to run the tests, create a sql local db instance called Filters - "sqllocaldb c Filters" - all tests are hardcoded to that.
