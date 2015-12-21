IF EXISTS (SELECT name FROM sys.indexes
            WHERE name = N'IX_Site_CatNo')
    DROP INDEX IX_Site_CatNo ON Site;

IF EXISTS (SELECT name FROM sys.indexes
            WHERE name = N'IX_DailyPrice_CatNo')
    DROP INDEX IX_DailyPrice_CatNo ON DailyPrice;
