﻿CREATE NONCLUSTERED INDEX IX_Site_CatNo
    ON  Site(CatNo)
	 INCLUDE (Id)
;
CREATE NONCLUSTERED INDEX IX_DailyPrice_CatNo
    ON  DailyPrice(CatNo)
;
