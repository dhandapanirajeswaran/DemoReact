CREATE PROCEDURE [dbo].[spGetScheduleEmailTemplateForType]
	@ScheduleEmailType INT
AS
BEGIN
	SET NOCOUNT ON

	SELECT TOP 1
		sch.Id,
		sch.ScheduleEmailType,
		sch.SubjectLine,
		sch.ContactEmail,
		sch.EmailBody
	FROM
		dbo.ScheduleEmailTemplate sch
	WHERE
		sch.ScheduleEmailType = @ScheduleEmailType
END
