CREATE PROCEDURE [dbo].[Add Temperature Record]
	@containerId nvarchar(255),
	@temperature decimal(9, 4),
	@recordedAt DATETIME2(0)
AS
	insert into [Temperature Records] ([Container Id], [Temperature], [Recorded At])
	values
	(@containerId, @temperature, @recordedAt)
