CREATE PROCEDURE [dbo].[Get Temperature Records]
	@containerId nvarchar(255)
AS
	SELECT [Temperature], [Recorded At] from [Temperature Records] where [Container Id] = @containerId
