CREATE PROCEDURE [dbo].[spGetDiagnosticsRecentDatabaseObjectChanges]
	@DaysAgo INT = 7
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @StartDate DATE = DATEADD(DAY, -@DaysAgo, GetDate())

	--D 	DEFAULT_CONSTRAINT
	--F 	FOREIGN_KEY_CONSTRAINT
	--FN	SQL_SCALAR_FUNCTION
	--P 	SQL_STORED_PROCEDURE
	--PK	PRIMARY_KEY_CONSTRAINT
	--TF	SQL_TABLE_VALUED_FUNCTION
	--U 	USER_TABLE

	;WITH RecentObjects AS (
		SELECT 
			so.*
		FROM 
			sys.objects so
		WHERE 
			so.is_ms_shipped = 0  
			AND so.[type] IN ('D', 'F', 'FN', 'P', 'PK', 'TF', 'U')
			AND (
				so.[create_date] >= @StartDate
				OR
				so.[modify_date] >= @StartDate
			)
	)
	SELECT
		ro.type [Type],
		ro.type_desc [TypeDescription],
		SCHEMA_NAME(ro.schema_id) [SchemaName],
		ro.name [Name],
		create_date [CreatedOn],
		modify_date [ModifiedOn]
	FROM
		RecentObjects ro
	ORDER BY
		CASE WHEN ro.modify_date > ro.create_date 
			THEN ro.modify_date 
			ELSE ro.create_date
		END DESC
END
RETURN 0
