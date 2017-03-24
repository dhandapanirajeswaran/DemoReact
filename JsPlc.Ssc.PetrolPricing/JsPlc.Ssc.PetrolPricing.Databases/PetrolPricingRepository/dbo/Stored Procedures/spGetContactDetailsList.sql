CREATE PROCEDURE dbo.spGetContactDetailsList
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		cd.Id,
		cd.Heading,
		cd.Address,
		cd.PhoneNumber,
		cd.EmailName,
		cd.EmailAddress,
		cd.IsActive
	FROM
		dbo.ContactDetails cd
	ORDER BY
		cd.Heading ASC
END