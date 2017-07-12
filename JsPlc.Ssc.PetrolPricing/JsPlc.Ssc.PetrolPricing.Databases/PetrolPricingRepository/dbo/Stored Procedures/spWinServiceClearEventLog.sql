CREATE PROCEDURE [dbo].[spWinServiceClearEventLog]
AS
	SET NOCOUNT ON

	TRUNCATE TABLE dbo.WinServiceEventLog;

RETURN 0
