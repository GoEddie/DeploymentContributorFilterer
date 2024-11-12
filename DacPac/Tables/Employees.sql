CREATE TABLE [dbo].[Employees]
(
	[EmployeeId] INT NOT NULL PRIMARY KEY,
	[Name] VARCHAR(25) NOT NULL default  'aaaaa',
	[Age] int CONSTRAINT df$EmployeeName DEFAULT (0) NOT NULL 
)

GO

CREATE INDEX [IX_Employees_EmployeeId] ON [dbo].[Employees] (EmployeeId)
