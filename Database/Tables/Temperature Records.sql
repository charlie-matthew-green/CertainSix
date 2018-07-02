CREATE TABLE [dbo].[Temperature Records]
(
	[Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[Container Id] NVARCHAR(255) NOT NULL,
	[Temperature] decimal(9, 4) NOT NULL,
	[Recorded At] DateTime2(0) NOT NULL
)
