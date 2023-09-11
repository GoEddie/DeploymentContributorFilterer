CREATE TABLE [dbo].[Employees]
(
	[EmployeeId] INT NOT NULL PRIMARY KEY,
	[Name] VARCHAR(25) NOT NULL default  'aaaaa'
)

GO

CREATE INDEX [IX_Employees_EmployeeId] ON [dbo].[Employees] (EmployeeId)
