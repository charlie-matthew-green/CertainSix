CREATE TABLE [dbo].[Trips]
(
	[Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[Name] NVARCHAR(255) NOT NULL, 
    [Spoil Temperature] decimal(9,4) NOT NULL,
	[Spoil Duration in Minutes] INT NOT NULL
)
