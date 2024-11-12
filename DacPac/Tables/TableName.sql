CREATE TABLE [dbo].[TableName] (
    [IdentityColumn]      BIGINT       IDENTITY (1, 1) NOT NULL,
    [ForeignKeyColumn] UNIQUEIDENTIFIER NULL,
    [TableInfoColumn]     NVARCHAR (40) NULL,
    [DataCodeColumn]    INT
);
GO

CREATE CLUSTERED COLUMNSTORE INDEX [CIDX_ColumnStoreIndexName] ON [dbo].[TableName] WITH (DROP_EXISTING = OFF, COMPRESSION_DELAY = 0);
GO