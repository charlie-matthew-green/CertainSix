CREATE PROCEDURE [dbo].[Get Trip]
	@id BIGINT
AS
	SELECT [Id], [Name], [Spoil Temperature], [Spoil Duration in Minutes] from [Trips]
	where [Id] = @id
