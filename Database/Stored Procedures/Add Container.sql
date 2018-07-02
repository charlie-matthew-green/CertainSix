CREATE PROCEDURE [dbo].[Add Container]
	@tripId bigint,
	@containerId nvarchar(255),
	@productCount int
AS
	insert into [dbo].[Containers] 
	([Id], [Trip Id], [Product Count])
	values
	(@containerId, @tripId, @productCount)
