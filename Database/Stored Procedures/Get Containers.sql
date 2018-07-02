CREATE PROCEDURE [dbo].[Get Containers]
	@tripId VARCHAR(255)
AS
	SELECT [Id], [Product Count] from [Containers] where [Trip Id] = @tripId
