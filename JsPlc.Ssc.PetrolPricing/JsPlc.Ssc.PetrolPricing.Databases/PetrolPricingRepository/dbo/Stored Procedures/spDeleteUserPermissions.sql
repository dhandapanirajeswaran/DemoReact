CREATE PROCEDURE [dbo].[spDeleteUserPermissions]
	@PPUserId int
AS
	
	SET NOCOUNT ON

	DELETE FROM dbo.PPUserPermissions WHERE PPUserId = @PPUserId AND @PPUserId <> 0

RETURN 0
