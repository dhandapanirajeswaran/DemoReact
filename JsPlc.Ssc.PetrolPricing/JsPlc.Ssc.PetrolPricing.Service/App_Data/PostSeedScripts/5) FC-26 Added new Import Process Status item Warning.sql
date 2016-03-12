IF NOT EXISTS(SELECT 1 FROM [dbo].[ImportProcessStatus] WHERE [Status] = 'Warning')
BEGIN
INSERT INTO [dbo].[ImportProcessStatus]
           (ID, [Status])
     VALUES
           (2, 'Warning')
END
