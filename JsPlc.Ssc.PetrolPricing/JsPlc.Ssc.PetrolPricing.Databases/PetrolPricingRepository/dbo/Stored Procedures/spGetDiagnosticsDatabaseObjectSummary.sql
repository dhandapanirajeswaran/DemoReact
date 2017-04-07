CREATE PROCEDURE [dbo].[spGetDiagnosticsDatabaseObjectSummary]
AS
BEGIN
	SET NOCOUNT ON

	--D 	DEFAULT_CONSTRAINT
	--F 	FOREIGN_KEY_CONSTRAINT
	--FN	SQL_SCALAR_FUNCTION
	--P 	SQL_STORED_PROCEDURE
	--PK	PRIMARY_KEY_CONSTRAINT
	--TF	SQL_TABLE_VALUED_FUNCTION
	--U 	USER_TABLE

	;WITH AllUserObjects AS (
		SELECT
			so.*
		FROM
			sys.objects so
		WHERE
			so.is_ms_shipped = 0
			AND 
			so.[type] IN ('D', 'F', 'FN', 'P', 'PK', 'TF', 'U')
	)
	SELECT
		(SELECT COUNT(1) FROM AllUserObjects WHERE type = 'D') [DefaultConstraintCount],
		(SELECT COUNT(1) FROM AllUserObjects WHERE type = 'F') [ForeignKeyCount],
		(SELECT COUNT(1) FROM AllUserObjects WHERE type = 'FN') [ScalarFunctionCount],
		(SELECT COUNT(1) FROM AllUserObjects WHERE type = 'P') [StoredProcedureCount],
		(SELECT COUNT(1) FROM AllUserObjects WHERE type = 'PK') [PrimaryKeyCount],
		(SELECT COUNT(1) FROM AllUserObjects WHERE type = 'TF') [TableFunctionCount],
		(SELECT COUNT(1) FROM AllUserObjects WHERE type = 'U') [UserTableCount]
END
RETURN 0
