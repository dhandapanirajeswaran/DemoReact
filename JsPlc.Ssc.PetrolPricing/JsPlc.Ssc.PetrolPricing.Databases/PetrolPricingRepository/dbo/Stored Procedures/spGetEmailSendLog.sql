CREATE PROCEDURE [dbo].[spGetEmailSendLog]
	@EmailSendLogId INT
AS
BEGIN
	SET NOCOUNT ON

	SELECT TOP 1
		esl.Id [Id],
		esl.SiteId [SiteId],
		st.SiteName [SiteName],
		esl.IsTest [IsTest],
		esl.EmailFrom [EmailFrom],
		esl.FixedEmailTo [FixedEmailTo],
		esl.ListOfEmailTo [ListOfEmailTo],
		esl.EmailSubject [EmailSubject],
		esl.EmailBody [EmailBody],
		esl.EndTradeDate [EndTradeDate],
		esl.SendDate [SendDate],
		esl.LoginUser [LoginUser],
		esl.IsSuccess [IsSuccess],
		esl.IsWarning [IsWarning],
		esl.IsError [IsError],
		esl.WarningMessage [WarningMessage],
		esl.ErrorMessage [ErrorMessage]
	FROM
		dbo.EmailSendLog esl
		INNER JOIN dbo.Site st ON st.Id = esl.SiteId

	WHERE
		esl.Id = @EmailSendLogId

	RETURN 0
END

