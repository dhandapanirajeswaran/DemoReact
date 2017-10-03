CREATE PROCEDURE [dbo].[spGetSiteEmailSendStatuses]
	@ForDate DATE
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @ForDateNextDay DATE = DATEADD(DAY, 1, @ForDate)

	SELECT DISTINCT
		st.Id [SiteId],
		esl.EndTradeDate [EndTradeDate],
		esl.SendDate [SendDate],
		esl.IsSuccess [IsSuccess]
	FROM
		dbo.Site st
		LEFT JOIN dbo.EmailSendLog esl ON esl.IsSuccess=1 AND esl.SiteId = st.Id AND esl.EndTradeDate >= @ForDate AND esl.EndTradeDate < @ForDateNextDay
	WHERE
		st.IsSainsburysSite = 1

	RETURN 0
END

