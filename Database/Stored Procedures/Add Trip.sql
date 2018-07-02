CREATE PROCEDURE [dbo].[Add Trip]
	@name nvarchar(255),
	@spoilTemperature decimal(9, 4),
	@spoilDurationInMinutes int
AS
	insert into [Trips] 
	([Name], [Spoil Temperature], [Spoil Duration in Minutes])
	OUTPUT Inserted.Id
	values
	(@name, @spoilTemperature, @spoilDurationInMinutes)
return 
