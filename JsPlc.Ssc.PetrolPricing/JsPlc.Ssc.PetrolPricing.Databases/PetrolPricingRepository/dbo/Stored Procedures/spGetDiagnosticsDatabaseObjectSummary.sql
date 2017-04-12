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
	),
	AllIndexes AS (
		SELECT 
			si.*
		FROM
			sys.indexes si
			JOIN sys.objects so ON si.object_id = so.object_id
		WHERE
			so.type = 'U'
			AND
			si.type <> 0
			AND
			so.is_ms_shipped = 0
 	)
	SELECT
		(SELECT COUNT(1) FROM AllUserObjects WHERE type = 'D') [DefaultConstraintCount],
		(SELECT COUNT(1) FROM AllUserObjects WHERE type = 'F') [ForeignKeyCount],
		(SELECT COUNT(1) FROM AllUserObjects WHERE type = 'FN') [ScalarFunctionCount],
		(SELECT COUNT(1) FROM AllUserObjects WHERE type = 'P') [StoredProcedureCount],
		(SELECT COUNT(1) FROM AllUserObjects WHERE type = 'PK') [PrimaryKeyCount],
		(SELECT COUNT(1) FROM AllUserObjects WHERE type = 'TF') [TableFunctionCount],
		(SELECT COUNT(1) FROM AllUserObjects WHERE type = 'U') [UserTableCount],
		(SELECT COUNT(1) FROM AllIndexes) [TotalIndexCount],
		(SELECT COUNT(1) FROM AllIndexes WHERE type = 1) [ClusteredIndexCount],
		(SELECT COUNT(1) FROM AllIndexes WHERE type = 2) [NonClusteredIndexCount]
END
RETURN 0