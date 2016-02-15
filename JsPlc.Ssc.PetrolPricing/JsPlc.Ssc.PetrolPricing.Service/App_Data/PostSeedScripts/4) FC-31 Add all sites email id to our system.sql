--delete existing emails for provided stores

delete from siteemail where siteid in (select id from [site] where StoreNo in (524,
524,
646,
646,
2010,
2010,
662,
662,
722,
722,
634,
634,
832,
832,
2100,
2100,
2054,
2054,
2022,
2022,
666,
666,
673,
673,
537,
537,
894,
894,
642,
642,
674,
674,
608,
608,
5,
5,
569,
569,
859,
859,
426,
426,
717,
717,
752,
752,
541,
541,
53,
53,
2835,
2835,
814,
814,
2231,
2231,
224,
224,
505,
505,
669,
669,
643,
643,
667,
667,
681,
681,
686,
686,
652,
652,
411,
411,
59,
59,
559,
559,
773,
773,
775,
775,
2134,
2134,
813,
813,
795,
795,
2106,
2106,
751,
751,
2071,
2071,
772,
772,
4,
4,
721,
721,
849,
849,
6,
6,
664,
664,
687,
687,
687,
8,
8,
815,
815,
534,
534,
434,
434,
789,
789,
2013,
2013,
2073,
2073,
17,
17,
639,
639,
885,
885,
749,
749,
886,
886,
897,
897,
2067,
2067,
601,
601,
9992112,
9992112,
9992112,
523,
523,
549,
549,
2180,
2180,
611,
611,
776,
776,
797,
797,
893,
893,
443,
443,
765,
765,
560,
560,
744,
744,
7,
7,
677,
677,
444,
444,
609,
609,
672,
672,
656,
656,
535,
535,
727,
727,
685,
685,
543,
543,
670,
670,
2081,
2081,
2625,
2625,
892,
892,
422,
422,
778,
778,
803,
803,
62,
62,
709,
709,
869,
869,
402,
402,
406,
406,
711,
711,
419,
419,
630,
630,
657,
657,
556,
556,
637,
637,
519,
519,
600,
600,
2005,
2005,
2114,
2114,
391,
391,
825,
825,
2007,
2007,
2105,
2105,
847,
847,
665,
665,
757,
757,
628,
628,
640,
640,
631,
631,
683,
683,
816,
816,
871,
871,
2001,
2001,
602,
602,
28,
28,
503,
503,
695,
695,
70,
70,
421,
421,
2017,
2017,
725,
725,
2023,
2023,
567,
567,
567,
706,
706,
18,
18,
852,
852,
433,
433,
4517,
4517,
58,
58,
605,
605,
691,
691,
714,
714,
2092,
2092,
2112,
2112,
2112,
521,
521,
566,
566,
790,
790,
713,
713,
2170,
2170,
31,
31,
690,
690,
2136,
2136,
2246,
2246,
882,
882,
38,
38,
2046,
2046,
762,
762,
558,
558,
2063,
2063,
2059,
2059,
801,
801,
3,
3,
823,
823,
417,
417,
682,
682,
441,
441,
52,
52,
413,
413,
72,
72,
604,
604,
853,
853,
633,
633,
2011,
2011,
548,
548,
742,
742,
696,
696,
514,
514,
873,
873,
2080,
2080,
27,
27,
867,
867,
63,
63,
793,
793,
718,
718,
644,
644,
2079,
2079,
415,
415,
2061,
2061,
629,
629,
661,
661,
679,
679,
2168,
2168,
410,
410,
529,
529,
2113,
2113,
812,
812,
745,
745,
788,
788,
774,
774,
753,
753,
510,
510,
693,
693,
676,
676,
887,
887,
2078,
2078,
2002,
2002,
516,
516,
2324,
2324,
474,
474,
403,
403,
732,
732,
509,
509,
206,
206,
668,
668,
671,
671,
20,
20,
740,
740,
2295,
2295,
2244,
2244,
408,
408,
851,
851,
555,
555,
675,
675,
680,
680,
2040,
2040,
648,
648,
610,
610,
26,
26,
51,
51,
57,
57,
460,
460,
864,
864,
55,
55,
854,
854,
678,
678,
735,
735,
820,
820,
890,
890,
4518,
4518,
3,
4,
5,
6,
7,
8,
17,
18,
20,
2324,
26,
27,
28,
31,
38,
51,
52,
53,
55,
57,
58,
59,
62,
63,
70,
72,
206,
224,
225,
391,
402,
403,
406,
408,
410,
411,
2168,
413,
415,
417,
419,
421,
422,
426,
433,
434,
441,
443,
444,
460,
474,
2246,
503,
505,
509,
510,
514,
516,
519,
521,
523,
524,
529,
534,
535,
537,
541,
543,
548,
549,
555,
556,
558,
559,
560,
566,
569,
600,
601,
602,
604,
605,
608,
609,
610,
611,
628,
629,
630,
631,
633,
634,
637,
639,
640,
642,
643,
644,
646,
648,
652,
656,
657,
661,
662,
664,
665,
666,
667,
668,
669,
670,
671,
672,
673,
674,
675,
676,
677,
678,
679,
680,
681,
682,
683,
685,
686,
687,
690,
691,
693,
695,
696,
706,
709,
711,
713,
714,
717,
718,
721,
722,
2295,
725,
727,
732,
735,
2170,
740,
742,
744,
745,
749,
751,
752,
753,
757,
762,
765,
772,
773,
774,
775,
776,
778,
788,
789,
790,
793,
795,
797,
801,
803,
812,
813,
814,
815,
816,
820,
823,
824,
825,
832,
847,
849,
851,
852,
853,
854,
859,
864,
867,
869,
871,
873,
882,
885,
886,
887,
890,
892,
893,
894,
897,
2001,
2002,
2005,
2007,
2010,
2011,
2013,
2017,
2022,
2023,
2625,
2231,
2040,
2046,
2054,
2059,
2061,
2063,
2067,
2071,
2073,
2078,
2079,
2080,
2081,
2092,
2100,
2105,
2106,
2112,
4513,
2113,
2114,
2134,
2136,
2180,
2244,
2835,
4513,
2221,
2221,
2221,
2181,
2181,
2181,
2225,
2225,
2225,
2154,
2154,
2154,
2186,
2186,
2186,
2093,
2093,
2093,
2162,
2162,
2162,
2199,
2199,
2199,
850,
850,
850,
2169,
2169,
2169,
2097,
2097,
2097,
2247,
2247,
2247,
2249,
2249,
2249,
2196,
2196,
2196,
2288,
2288,
2288,
2268,
2268,
2268,
2248,
2248,
2248,
2255,
2255,
2255,
2274,
2274,
2274,
2200,
2200,
2200,
655,
655,
655,
2220,
2220,
2220,
2082,
2082,
2082,
2240,
2240,
2240,
2269,
2269,
2269,
2272,
2272,
2272,
2286,
2286,
2286,
2077,
2077,
2077,
2290,
2290,
2290,
2293,
2293,
2293,
2304,
2304,
2304,
2281,
2281,
2281,
507,
507,
507,
2239,
2239,
2239,
2297,
2297,
2297,
2303,
2303,
2303,
4524,
4524,
4524,
2123,
2123,
2123,
805,
805,
805,
2309,
2309,
2309,
2308,
2308,
2308,
2283,
2283,
2283,
2314,
2314,
2314,
2267,
2267,
2267,
4526,
4526,
4526,
4525,
4525,
4525,
4527,
4527,
4527,
2319,
2319,
2319,
4528,
4528,
4528,
4529,
4529,
4529,
2340,
2340,
2340,
2095,
2095,
2095,
2252,
2252,
2252,
4589,
4589,
4589,
2289,
2289,
2289,
2070,
2070,
2070,
815,
2030,
2030,
2030,
2254,
2254,
2254,
2158))

-- inserting new email for provided stores
if exists((select Id from [site] where storeno = 524))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0524m@sainsburys.co.uk', (select Id from [site] where storeno = 524));
if exists((select Id from [site] where storeno = 524))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0524r@sainsburys.co.uk', (select Id from [site] where storeno = 524));
if exists((select Id from [site] where storeno = 646))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0646m@sainsburys.co.uk', (select Id from [site] where storeno = 646));
if exists((select Id from [site] where storeno = 646))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0646r@sainsburys.co.uk', (select Id from [site] where storeno = 646));
if exists((select Id from [site] where storeno = 2010))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2010m@sainsburys.co.uk', (select Id from [site] where storeno = 2010));
if exists((select Id from [site] where storeno = 2010))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2010r@sainsburys.co.uk', (select Id from [site] where storeno = 2010));
if exists((select Id from [site] where storeno = 662))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0662m@sainsburys.co.uk', (select Id from [site] where storeno = 662));
if exists((select Id from [site] where storeno = 662))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0662r@sainsburys.co.uk', (select Id from [site] where storeno = 662));
if exists((select Id from [site] where storeno = 722))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0722m@sainsburys.co.uk', (select Id from [site] where storeno = 722));
if exists((select Id from [site] where storeno = 722))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0722r@sainsburys.co.uk', (select Id from [site] where storeno = 722));
if exists((select Id from [site] where storeno = 634))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0634m@sainsburys.co.uk', (select Id from [site] where storeno = 634));
if exists((select Id from [site] where storeno = 634))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0634r@sainsburys.co.uk', (select Id from [site] where storeno = 634));
if exists((select Id from [site] where storeno = 832))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0832m@sainsburys.co.uk', (select Id from [site] where storeno = 832));
if exists((select Id from [site] where storeno = 832))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0832r@sainsburys.co.uk', (select Id from [site] where storeno = 832));
if exists((select Id from [site] where storeno = 2100))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2100m@sainsburys.co.uk', (select Id from [site] where storeno = 2100));
if exists((select Id from [site] where storeno = 2100))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2100r@sainsburys.co.uk', (select Id from [site] where storeno = 2100));
if exists((select Id from [site] where storeno = 2054))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2054m@sainsburys.co.uk', (select Id from [site] where storeno = 2054));
if exists((select Id from [site] where storeno = 2054))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2054r@sainsburys.co.uk', (select Id from [site] where storeno = 2054));
if exists((select Id from [site] where storeno = 2022))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2022m@sainsburys.co.uk', (select Id from [site] where storeno = 2022));
if exists((select Id from [site] where storeno = 2022))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2022r@sainsburys.co.uk', (select Id from [site] where storeno = 2022));
if exists((select Id from [site] where storeno = 666))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0666m@sainsburys.co.uk', (select Id from [site] where storeno = 666));
if exists((select Id from [site] where storeno = 666))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0666r@sainsburys.co.uk', (select Id from [site] where storeno = 666));
if exists((select Id from [site] where storeno = 673))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0673m@sainsburys.co.uk', (select Id from [site] where storeno = 673));
if exists((select Id from [site] where storeno = 673))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0673r@sainsburys.co.uk', (select Id from [site] where storeno = 673));
if exists((select Id from [site] where storeno = 537))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0537m@sainsburys.co.uk', (select Id from [site] where storeno = 537));
if exists((select Id from [site] where storeno = 537))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0537r@sainsburys.co.uk', (select Id from [site] where storeno = 537));
if exists((select Id from [site] where storeno = 894))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0894m@sainsburys.co.uk', (select Id from [site] where storeno = 894));
if exists((select Id from [site] where storeno = 894))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0894r@sainsburys.co.uk', (select Id from [site] where storeno = 894));
if exists((select Id from [site] where storeno = 642))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0642m@sainsburys.co.uk', (select Id from [site] where storeno = 642));
if exists((select Id from [site] where storeno = 642))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0642r@sainsburys.co.uk', (select Id from [site] where storeno = 642));
if exists((select Id from [site] where storeno = 674))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0674m@sainsburys.co.uk', (select Id from [site] where storeno = 674));
if exists((select Id from [site] where storeno = 674))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0674r@sainsburys.co.uk', (select Id from [site] where storeno = 674));
if exists((select Id from [site] where storeno = 608))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0608m@sainsburys.co.uk', (select Id from [site] where storeno = 608));
if exists((select Id from [site] where storeno = 608))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0608r@sainsburys.co.uk', (select Id from [site] where storeno = 608));
if exists((select Id from [site] where storeno = 5))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0005m@sainsburys.co.uk', (select Id from [site] where storeno = 5));
if exists((select Id from [site] where storeno = 5))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0005r@sainsburys.co.uk', (select Id from [site] where storeno = 5));
if exists((select Id from [site] where storeno = 569))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0569m@sainsburys.co.uk', (select Id from [site] where storeno = 569));
if exists((select Id from [site] where storeno = 569))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0569r@sainsburys.co.uk', (select Id from [site] where storeno = 569));
if exists((select Id from [site] where storeno = 859))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0859m@sainsburys.co.uk', (select Id from [site] where storeno = 859));
if exists((select Id from [site] where storeno = 859))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0859r@sainsburys.co.uk', (select Id from [site] where storeno = 859));
if exists((select Id from [site] where storeno = 426))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0426m@sainsburys.co.uk', (select Id from [site] where storeno = 426));
if exists((select Id from [site] where storeno = 426))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0426r@sainsburys.co.uk', (select Id from [site] where storeno = 426));
if exists((select Id from [site] where storeno = 717))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0717m@sainsburys.co.uk', (select Id from [site] where storeno = 717));
if exists((select Id from [site] where storeno = 717))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0717r@sainsburys.co.uk', (select Id from [site] where storeno = 717));
if exists((select Id from [site] where storeno = 752))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0752m@sainsburys.co.uk', (select Id from [site] where storeno = 752));
if exists((select Id from [site] where storeno = 752))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0752r@sainsburys.co.uk', (select Id from [site] where storeno = 752));
if exists((select Id from [site] where storeno = 541))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0541m@sainsburys.co.uk', (select Id from [site] where storeno = 541));
if exists((select Id from [site] where storeno = 541))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0541r@sainsburys.co.uk', (select Id from [site] where storeno = 541));
if exists((select Id from [site] where storeno = 53))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0053m@sainsburys.co.uk', (select Id from [site] where storeno = 53));
if exists((select Id from [site] where storeno = 53))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0053r@sainsburys.co.uk', (select Id from [site] where storeno = 53));
if exists((select Id from [site] where storeno = 2835))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2835m@sainsburys.co.uk', (select Id from [site] where storeno = 2835));
if exists((select Id from [site] where storeno = 2835))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2835r@sainsburys.co.uk', (select Id from [site] where storeno = 2835));
if exists((select Id from [site] where storeno = 814))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0814m@sainsburys.co.uk', (select Id from [site] where storeno = 814));
if exists((select Id from [site] where storeno = 814))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0814r@sainsburys.co.uk', (select Id from [site] where storeno = 814));
if exists((select Id from [site] where storeno = 2231))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2231m@sainsburys.co.uk', (select Id from [site] where storeno = 2231));
if exists((select Id from [site] where storeno = 2231))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2231r@sainsburys.co.uk', (select Id from [site] where storeno = 2231));
if exists((select Id from [site] where storeno = 224))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0224m@sainsburys.co.uk', (select Id from [site] where storeno = 224));
if exists((select Id from [site] where storeno = 224))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0224r@sainsburys.co.uk', (select Id from [site] where storeno = 224));
if exists((select Id from [site] where storeno = 505))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0505m@sainsburys.co.uk', (select Id from [site] where storeno = 505));
if exists((select Id from [site] where storeno = 505))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0505r@sainsburys.co.uk', (select Id from [site] where storeno = 505));
if exists((select Id from [site] where storeno = 669))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0669m@sainsburys.co.uk', (select Id from [site] where storeno = 669));
if exists((select Id from [site] where storeno = 669))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0669r@sainsburys.co.uk', (select Id from [site] where storeno = 669));
if exists((select Id from [site] where storeno = 643))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0643m@sainsburys.co.uk', (select Id from [site] where storeno = 643));
if exists((select Id from [site] where storeno = 643))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0643r@sainsburys.co.uk', (select Id from [site] where storeno = 643));
if exists((select Id from [site] where storeno = 667))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0667m@sainsburys.co.uk', (select Id from [site] where storeno = 667));
if exists((select Id from [site] where storeno = 667))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0667r@sainsburys.co.uk', (select Id from [site] where storeno = 667));
if exists((select Id from [site] where storeno = 681))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0681m@sainsburys.co.uk', (select Id from [site] where storeno = 681));
if exists((select Id from [site] where storeno = 681))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0681r@sainsburys.co.uk', (select Id from [site] where storeno = 681));
if exists((select Id from [site] where storeno = 686))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0686m@sainsburys.co.uk', (select Id from [site] where storeno = 686));
if exists((select Id from [site] where storeno = 686))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0686r@sainsburys.co.uk', (select Id from [site] where storeno = 686));
if exists((select Id from [site] where storeno = 652))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0652m@sainsburys.co.uk', (select Id from [site] where storeno = 652));
if exists((select Id from [site] where storeno = 652))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0652r@sainsburys.co.uk', (select Id from [site] where storeno = 652));
if exists((select Id from [site] where storeno = 411))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0411m@sainsburys.co.uk', (select Id from [site] where storeno = 411));
if exists((select Id from [site] where storeno = 411))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0411r@sainsburys.co.uk', (select Id from [site] where storeno = 411));
if exists((select Id from [site] where storeno = 59))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0059m@sainsburys.co.uk', (select Id from [site] where storeno = 59));
if exists((select Id from [site] where storeno = 59))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0059r@sainsburys.co.uk', (select Id from [site] where storeno = 59));
if exists((select Id from [site] where storeno = 559))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0559m@sainsburys.co.uk', (select Id from [site] where storeno = 559));
if exists((select Id from [site] where storeno = 559))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0559r@sainsburys.co.uk', (select Id from [site] where storeno = 559));
if exists((select Id from [site] where storeno = 773))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0773m@sainsburys.co.uk', (select Id from [site] where storeno = 773));
if exists((select Id from [site] where storeno = 773))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0773r@sainsburys.co.uk', (select Id from [site] where storeno = 773));
if exists((select Id from [site] where storeno = 775))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0775m@sainsburys.co.uk', (select Id from [site] where storeno = 775));
if exists((select Id from [site] where storeno = 775))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0775r@sainsburys.co.uk', (select Id from [site] where storeno = 775));
if exists((select Id from [site] where storeno = 2134))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2134m@sainsburys.co.uk', (select Id from [site] where storeno = 2134));
if exists((select Id from [site] where storeno = 2134))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2134r@sainsburys.co.uk', (select Id from [site] where storeno = 2134));
if exists((select Id from [site] where storeno = 813))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0813m@sainsburys.co.uk', (select Id from [site] where storeno = 813));
if exists((select Id from [site] where storeno = 813))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0813r@sainsburys.co.uk', (select Id from [site] where storeno = 813));
if exists((select Id from [site] where storeno = 795))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0795m@sainsburys.co.uk', (select Id from [site] where storeno = 795));
if exists((select Id from [site] where storeno = 795))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0795r@sainsburys.co.uk', (select Id from [site] where storeno = 795));
if exists((select Id from [site] where storeno = 2106))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2106m@sainsburys.co.uk', (select Id from [site] where storeno = 2106));
if exists((select Id from [site] where storeno = 2106))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2106r@sainsburys.co.uk', (select Id from [site] where storeno = 2106));
if exists((select Id from [site] where storeno = 751))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0751m@sainsburys.co.uk', (select Id from [site] where storeno = 751));
if exists((select Id from [site] where storeno = 751))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0751r@sainsburys.co.uk', (select Id from [site] where storeno = 751));
if exists((select Id from [site] where storeno = 2071))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2071m@sainsburys.co.uk', (select Id from [site] where storeno = 2071));
if exists((select Id from [site] where storeno = 2071))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2071r@sainsburys.co.uk', (select Id from [site] where storeno = 2071));
if exists((select Id from [site] where storeno = 772))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0772m@sainsburys.co.uk', (select Id from [site] where storeno = 772));
if exists((select Id from [site] where storeno = 772))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0772r@sainsburys.co.uk', (select Id from [site] where storeno = 772));
if exists((select Id from [site] where storeno = 4))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0004m@sainsburys.co.uk', (select Id from [site] where storeno = 4));
if exists((select Id from [site] where storeno = 4))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0004r@sainsburys.co.uk', (select Id from [site] where storeno = 4));
if exists((select Id from [site] where storeno = 721))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0721m@sainsburys.co.uk', (select Id from [site] where storeno = 721));
if exists((select Id from [site] where storeno = 721))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0721r@sainsburys.co.uk', (select Id from [site] where storeno = 721));
if exists((select Id from [site] where storeno = 849))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0849m@sainsburys.co.uk', (select Id from [site] where storeno = 849));
if exists((select Id from [site] where storeno = 849))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0849r@sainsburys.co.uk', (select Id from [site] where storeno = 849));
if exists((select Id from [site] where storeno = 6))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0006m@sainsburys.co.uk', (select Id from [site] where storeno = 6));
if exists((select Id from [site] where storeno = 6))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0006r@sainsburys.co.uk', (select Id from [site] where storeno = 6));
if exists((select Id from [site] where storeno = 664))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0664m@sainsburys.co.uk', (select Id from [site] where storeno = 664));
if exists((select Id from [site] where storeno = 664))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0664r@sainsburys.co.uk', (select Id from [site] where storeno = 664));
if exists((select Id from [site] where storeno = 687))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0687m@sainsburys.co.uk', (select Id from [site] where storeno = 687));
if exists((select Id from [site] where storeno = 687))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0687pfs@sainsburys.co.uk', (select Id from [site] where storeno = 687));
if exists((select Id from [site] where storeno = 687))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0687r@sainsburys.co.uk', (select Id from [site] where storeno = 687));
if exists((select Id from [site] where storeno = 8))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0008m@sainsburys.co.uk', (select Id from [site] where storeno = 8));
if exists((select Id from [site] where storeno = 8))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0008r@sainsburys.co.uk', (select Id from [site] where storeno = 8));
if exists((select Id from [site] where storeno = 815))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0815m@sainsburys.co.uk', (select Id from [site] where storeno = 815));
if exists((select Id from [site] where storeno = 815))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0815r@sainsburys.co.uk', (select Id from [site] where storeno = 815));
if exists((select Id from [site] where storeno = 534))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0534m@sainsburys.co.uk', (select Id from [site] where storeno = 534));
if exists((select Id from [site] where storeno = 534))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0534r@sainsburys.co.uk', (select Id from [site] where storeno = 534));
if exists((select Id from [site] where storeno = 434))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0434m@sainsburys.co.uk', (select Id from [site] where storeno = 434));
if exists((select Id from [site] where storeno = 434))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0434r@sainsburys.co.uk', (select Id from [site] where storeno = 434));
if exists((select Id from [site] where storeno = 789))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0789m@sainsburys.co.uk', (select Id from [site] where storeno = 789));
if exists((select Id from [site] where storeno = 789))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0789r@sainsburys.co.uk', (select Id from [site] where storeno = 789));
if exists((select Id from [site] where storeno = 2013))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2013m@sainsburys.co.uk', (select Id from [site] where storeno = 2013));
if exists((select Id from [site] where storeno = 2013))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2013r@sainsburys.co.uk', (select Id from [site] where storeno = 2013));
if exists((select Id from [site] where storeno = 2073))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2073m@sainsburys.co.uk', (select Id from [site] where storeno = 2073));
if exists((select Id from [site] where storeno = 2073))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2073r@sainsburys.co.uk', (select Id from [site] where storeno = 2073));
if exists((select Id from [site] where storeno = 17))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0017m@sainsburys.co.uk', (select Id from [site] where storeno = 17));
if exists((select Id from [site] where storeno = 17))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0017r@sainsburys.co.uk', (select Id from [site] where storeno = 17));
if exists((select Id from [site] where storeno = 639))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0639m@sainsburys.co.uk', (select Id from [site] where storeno = 639));
if exists((select Id from [site] where storeno = 639))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0639r@sainsburys.co.uk', (select Id from [site] where storeno = 639));
if exists((select Id from [site] where storeno = 885))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0885m@sainsburys.co.uk', (select Id from [site] where storeno = 885));
if exists((select Id from [site] where storeno = 885))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0885r@sainsburys.co.uk', (select Id from [site] where storeno = 885));
if exists((select Id from [site] where storeno = 749))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0749m@sainsburys.co.uk', (select Id from [site] where storeno = 749));
if exists((select Id from [site] where storeno = 749))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0749r@sainsburys.co.uk', (select Id from [site] where storeno = 749));
if exists((select Id from [site] where storeno = 886))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0886m@sainsburys.co.uk', (select Id from [site] where storeno = 886));
if exists((select Id from [site] where storeno = 886))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0886r@sainsburys.co.uk', (select Id from [site] where storeno = 886));
if exists((select Id from [site] where storeno = 897))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0897m@sainsburys.co.uk', (select Id from [site] where storeno = 897));
if exists((select Id from [site] where storeno = 897))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0897r@sainsburys.co.uk', (select Id from [site] where storeno = 897));
if exists((select Id from [site] where storeno = 2067))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2067m@sainsburys.co.uk', (select Id from [site] where storeno = 2067));
if exists((select Id from [site] where storeno = 2067))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2067r@sainsburys.co.uk', (select Id from [site] where storeno = 2067));
if exists((select Id from [site] where storeno = 601))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0601m@sainsburys.co.uk', (select Id from [site] where storeno = 601));
if exists((select Id from [site] where storeno = 601))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0601r@sainsburys.co.uk', (select Id from [site] where storeno = 601));
if exists((select Id from [site] where storeno = 9992112))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2112m@sainsburys.co.uk', (select Id from [site] where storeno = 9992112));
if exists((select Id from [site] where storeno = 9992112))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2112pfs@sainsburys.co.uk', (select Id from [site] where storeno = 9992112));
if exists((select Id from [site] where storeno = 9992112))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2112r@sainsburys.co.uk', (select Id from [site] where storeno = 9992112));
if exists((select Id from [site] where storeno = 523))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0523m@sainsburys.co.uk', (select Id from [site] where storeno = 523));
if exists((select Id from [site] where storeno = 523))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0523r@sainsburys.co.uk', (select Id from [site] where storeno = 523));
if exists((select Id from [site] where storeno = 549))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0549m@sainsburys.co.uk', (select Id from [site] where storeno = 549));
if exists((select Id from [site] where storeno = 549))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0549r@sainsburys.co.uk', (select Id from [site] where storeno = 549));
if exists((select Id from [site] where storeno = 2180))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2180m@sainsburys.co.uk', (select Id from [site] where storeno = 2180));
if exists((select Id from [site] where storeno = 2180))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2180r@sainsburys.co.uk', (select Id from [site] where storeno = 2180));
if exists((select Id from [site] where storeno = 611))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0611m@sainsburys.co.uk', (select Id from [site] where storeno = 611));
if exists((select Id from [site] where storeno = 611))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0611r@sainsburys.co.uk', (select Id from [site] where storeno = 611));
if exists((select Id from [site] where storeno = 776))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0776m@sainsburys.co.uk', (select Id from [site] where storeno = 776));
if exists((select Id from [site] where storeno = 776))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0776r@sainsburys.co.uk', (select Id from [site] where storeno = 776));
if exists((select Id from [site] where storeno = 797))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0797m@sainsburys.co.uk', (select Id from [site] where storeno = 797));
if exists((select Id from [site] where storeno = 797))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0797r@sainsburys.co.uk', (select Id from [site] where storeno = 797));
if exists((select Id from [site] where storeno = 893))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0893m@sainsburys.co.uk', (select Id from [site] where storeno = 893));
if exists((select Id from [site] where storeno = 893))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0893r@sainsburys.co.uk', (select Id from [site] where storeno = 893));
if exists((select Id from [site] where storeno = 443))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0443m@sainsburys.co.uk', (select Id from [site] where storeno = 443));
if exists((select Id from [site] where storeno = 443))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0443r@sainsburys.co.uk', (select Id from [site] where storeno = 443));
if exists((select Id from [site] where storeno = 765))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0765m@sainsburys.co.uk', (select Id from [site] where storeno = 765));
if exists((select Id from [site] where storeno = 765))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0765r@sainsburys.co.uk', (select Id from [site] where storeno = 765));
if exists((select Id from [site] where storeno = 560))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0560m@sainsburys.co.uk', (select Id from [site] where storeno = 560));
if exists((select Id from [site] where storeno = 560))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0560r@sainsburys.co.uk', (select Id from [site] where storeno = 560));
if exists((select Id from [site] where storeno = 744))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0744m@sainsburys.co.uk', (select Id from [site] where storeno = 744));
if exists((select Id from [site] where storeno = 744))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0744r@sainsburys.co.uk', (select Id from [site] where storeno = 744));
if exists((select Id from [site] where storeno = 7))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0007m@sainsburys.co.uk', (select Id from [site] where storeno = 7));
if exists((select Id from [site] where storeno = 7))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0007r@sainsburys.co.uk', (select Id from [site] where storeno = 7));
if exists((select Id from [site] where storeno = 677))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0677m@sainsburys.co.uk', (select Id from [site] where storeno = 677));
if exists((select Id from [site] where storeno = 677))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0677r@sainsburys.co.uk', (select Id from [site] where storeno = 677));
if exists((select Id from [site] where storeno = 444))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0444m@sainsburys.co.uk', (select Id from [site] where storeno = 444));
if exists((select Id from [site] where storeno = 444))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0444r@sainsburys.co.uk', (select Id from [site] where storeno = 444));
if exists((select Id from [site] where storeno = 609))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0609m@sainsburys.co.uk', (select Id from [site] where storeno = 609));
if exists((select Id from [site] where storeno = 609))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0609r@sainsburys.co.uk', (select Id from [site] where storeno = 609));
if exists((select Id from [site] where storeno = 672))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0672m@sainsburys.co.uk', (select Id from [site] where storeno = 672));
if exists((select Id from [site] where storeno = 672))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0672r@sainsburys.co.uk', (select Id from [site] where storeno = 672));
if exists((select Id from [site] where storeno = 656))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0656m@sainsburys.co.uk', (select Id from [site] where storeno = 656));
if exists((select Id from [site] where storeno = 656))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0656r@sainsburys.co.uk', (select Id from [site] where storeno = 656));
if exists((select Id from [site] where storeno = 535))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0535m@sainsburys.co.uk', (select Id from [site] where storeno = 535));
if exists((select Id from [site] where storeno = 535))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0535r@sainsburys.co.uk', (select Id from [site] where storeno = 535));
if exists((select Id from [site] where storeno = 727))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0727m@sainsburys.co.uk', (select Id from [site] where storeno = 727));
if exists((select Id from [site] where storeno = 727))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0727r@sainsburys.co.uk', (select Id from [site] where storeno = 727));
if exists((select Id from [site] where storeno = 685))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0685m@sainsburys.co.uk', (select Id from [site] where storeno = 685));
if exists((select Id from [site] where storeno = 685))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0685r@sainsburys.co.uk', (select Id from [site] where storeno = 685));
if exists((select Id from [site] where storeno = 543))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0543m@sainsburys.co.uk', (select Id from [site] where storeno = 543));
if exists((select Id from [site] where storeno = 543))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0543r@sainsburys.co.uk', (select Id from [site] where storeno = 543));
if exists((select Id from [site] where storeno = 670))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0670m@sainsburys.co.uk', (select Id from [site] where storeno = 670));
if exists((select Id from [site] where storeno = 670))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0670r@sainsburys.co.uk', (select Id from [site] where storeno = 670));
if exists((select Id from [site] where storeno = 2081))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2081m@sainsburys.co.uk', (select Id from [site] where storeno = 2081));
if exists((select Id from [site] where storeno = 2081))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2081r@sainsburys.co.uk', (select Id from [site] where storeno = 2081));
if exists((select Id from [site] where storeno = 2625))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2625m@sainsburys.co.uk', (select Id from [site] where storeno = 2625));
if exists((select Id from [site] where storeno = 2625))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2625r@sainsburys.co.uk', (select Id from [site] where storeno = 2625));
if exists((select Id from [site] where storeno = 892))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0892m@sainsburys.co.uk', (select Id from [site] where storeno = 892));
if exists((select Id from [site] where storeno = 892))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0892r@sainsburys.co.uk', (select Id from [site] where storeno = 892));
if exists((select Id from [site] where storeno = 422))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0422m@sainsburys.co.uk', (select Id from [site] where storeno = 422));
if exists((select Id from [site] where storeno = 422))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0422r@sainsburys.co.uk', (select Id from [site] where storeno = 422));
if exists((select Id from [site] where storeno = 778))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0778m@sainsburys.co.uk', (select Id from [site] where storeno = 778));
if exists((select Id from [site] where storeno = 778))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0778r@sainsburys.co.uk', (select Id from [site] where storeno = 778));
if exists((select Id from [site] where storeno = 803))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0803m@sainsburys.co.uk', (select Id from [site] where storeno = 803));
if exists((select Id from [site] where storeno = 803))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0803r@sainsburys.co.uk', (select Id from [site] where storeno = 803));
if exists((select Id from [site] where storeno = 62))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0062m@sainsburys.co.uk', (select Id from [site] where storeno = 62));
if exists((select Id from [site] where storeno = 62))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0062r@sainsburys.co.uk', (select Id from [site] where storeno = 62));
if exists((select Id from [site] where storeno = 709))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0709m@sainsburys.co.uk', (select Id from [site] where storeno = 709));
if exists((select Id from [site] where storeno = 709))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0709r@sainsburys.co.uk', (select Id from [site] where storeno = 709));
if exists((select Id from [site] where storeno = 869))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0869m@sainsburys.co.uk', (select Id from [site] where storeno = 869));
if exists((select Id from [site] where storeno = 869))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0869r@sainsburys.co.uk', (select Id from [site] where storeno = 869));
if exists((select Id from [site] where storeno = 402))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0402m@sainsburys.co.uk', (select Id from [site] where storeno = 402));
if exists((select Id from [site] where storeno = 402))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0402r@sainsburys.co.uk', (select Id from [site] where storeno = 402));
if exists((select Id from [site] where storeno = 406))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0406m@sainsburys.co.uk', (select Id from [site] where storeno = 406));
if exists((select Id from [site] where storeno = 406))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0406r@sainsburys.co.uk', (select Id from [site] where storeno = 406));
if exists((select Id from [site] where storeno = 711))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0711m@sainsburys.co.uk', (select Id from [site] where storeno = 711));
if exists((select Id from [site] where storeno = 711))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0711r@sainsburys.co.uk', (select Id from [site] where storeno = 711));
if exists((select Id from [site] where storeno = 419))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0419m@sainsburys.co.uk', (select Id from [site] where storeno = 419));
if exists((select Id from [site] where storeno = 419))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0419r@sainsburys.co.uk', (select Id from [site] where storeno = 419));
if exists((select Id from [site] where storeno = 630))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0630m@sainsburys.co.uk', (select Id from [site] where storeno = 630));
if exists((select Id from [site] where storeno = 630))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0630r@sainsburys.co.uk', (select Id from [site] where storeno = 630));
if exists((select Id from [site] where storeno = 657))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0657m@sainsburys.co.uk', (select Id from [site] where storeno = 657));
if exists((select Id from [site] where storeno = 657))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0657r@sainsburys.co.uk', (select Id from [site] where storeno = 657));
if exists((select Id from [site] where storeno = 556))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0556m@sainsburys.co.uk', (select Id from [site] where storeno = 556));
if exists((select Id from [site] where storeno = 556))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0556r@sainsburys.co.uk', (select Id from [site] where storeno = 556));
if exists((select Id from [site] where storeno = 637))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0637m@sainsburys.co.uk', (select Id from [site] where storeno = 637));
if exists((select Id from [site] where storeno = 637))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0637r@sainsburys.co.uk', (select Id from [site] where storeno = 637));
if exists((select Id from [site] where storeno = 519))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0519m@sainsburys.co.uk', (select Id from [site] where storeno = 519));
if exists((select Id from [site] where storeno = 519))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0519r@sainsburys.co.uk', (select Id from [site] where storeno = 519));
if exists((select Id from [site] where storeno = 600))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0600m@sainsburys.co.uk', (select Id from [site] where storeno = 600));
if exists((select Id from [site] where storeno = 600))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0600r@sainsburys.co.uk', (select Id from [site] where storeno = 600));
if exists((select Id from [site] where storeno = 2005))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2005m@sainsburys.co.uk', (select Id from [site] where storeno = 2005));
if exists((select Id from [site] where storeno = 2005))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2005r@sainsburys.co.uk', (select Id from [site] where storeno = 2005));
if exists((select Id from [site] where storeno = 2114))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2114m@sainsburys.co.uk', (select Id from [site] where storeno = 2114));
if exists((select Id from [site] where storeno = 2114))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2114r@sainsburys.co.uk', (select Id from [site] where storeno = 2114));
if exists((select Id from [site] where storeno = 391))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0391m@sainsburys.co.uk', (select Id from [site] where storeno = 391));
if exists((select Id from [site] where storeno = 391))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0391r@sainsburys.co.uk', (select Id from [site] where storeno = 391));
if exists((select Id from [site] where storeno = 825))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0825m@sainsburys.co.uk', (select Id from [site] where storeno = 825));
if exists((select Id from [site] where storeno = 825))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0825r@sainsburys.co.uk', (select Id from [site] where storeno = 825));
if exists((select Id from [site] where storeno = 2007))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2007m@sainsburys.co.uk', (select Id from [site] where storeno = 2007));
if exists((select Id from [site] where storeno = 2007))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2007r@sainsburys.co.uk', (select Id from [site] where storeno = 2007));
if exists((select Id from [site] where storeno = 2105))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2105m@sainsburys.co.uk', (select Id from [site] where storeno = 2105));
if exists((select Id from [site] where storeno = 2105))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2105r@sainsburys.co.uk', (select Id from [site] where storeno = 2105));
if exists((select Id from [site] where storeno = 847))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0847m@sainsburys.co.uk', (select Id from [site] where storeno = 847));
if exists((select Id from [site] where storeno = 847))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0847r@sainsburys.co.uk', (select Id from [site] where storeno = 847));
if exists((select Id from [site] where storeno = 665))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0665m@sainsburys.co.uk', (select Id from [site] where storeno = 665));
if exists((select Id from [site] where storeno = 665))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0665r@sainsburys.co.uk', (select Id from [site] where storeno = 665));
if exists((select Id from [site] where storeno = 757))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0757m@sainsburys.co.uk', (select Id from [site] where storeno = 757));
if exists((select Id from [site] where storeno = 757))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0757r@sainsburys.co.uk', (select Id from [site] where storeno = 757));
if exists((select Id from [site] where storeno = 628))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0628m@sainsburys.co.uk', (select Id from [site] where storeno = 628));
if exists((select Id from [site] where storeno = 628))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0628r@sainsburys.co.uk', (select Id from [site] where storeno = 628));
if exists((select Id from [site] where storeno = 640))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0640m@sainsburys.co.uk', (select Id from [site] where storeno = 640));
if exists((select Id from [site] where storeno = 640))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0640r@sainsburys.co.uk', (select Id from [site] where storeno = 640));
if exists((select Id from [site] where storeno = 631))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0631m@sainsburys.co.uk', (select Id from [site] where storeno = 631));
if exists((select Id from [site] where storeno = 631))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0631r@sainsburys.co.uk', (select Id from [site] where storeno = 631));
if exists((select Id from [site] where storeno = 683))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0683m@sainsburys.co.uk', (select Id from [site] where storeno = 683));
if exists((select Id from [site] where storeno = 683))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0683r@sainsburys.co.uk', (select Id from [site] where storeno = 683));
if exists((select Id from [site] where storeno = 816))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0816m@sainsburys.co.uk', (select Id from [site] where storeno = 816));
if exists((select Id from [site] where storeno = 816))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0816r@sainsburys.co.uk', (select Id from [site] where storeno = 816));
if exists((select Id from [site] where storeno = 871))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0871m@sainsburys.co.uk', (select Id from [site] where storeno = 871));
if exists((select Id from [site] where storeno = 871))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0871r@sainsburys.co.uk', (select Id from [site] where storeno = 871));
if exists((select Id from [site] where storeno = 2001))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2001m@sainsburys.co.uk', (select Id from [site] where storeno = 2001));
if exists((select Id from [site] where storeno = 2001))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2001r@sainsburys.co.uk', (select Id from [site] where storeno = 2001));
if exists((select Id from [site] where storeno = 602))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0602m@sainsburys.co.uk', (select Id from [site] where storeno = 602));
if exists((select Id from [site] where storeno = 602))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0602r@sainsburys.co.uk', (select Id from [site] where storeno = 602));
if exists((select Id from [site] where storeno = 28))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0028m@sainsburys.co.uk', (select Id from [site] where storeno = 28));
if exists((select Id from [site] where storeno = 28))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0028r@sainsburys.co.uk', (select Id from [site] where storeno = 28));
if exists((select Id from [site] where storeno = 503))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0503m@sainsburys.co.uk', (select Id from [site] where storeno = 503));
if exists((select Id from [site] where storeno = 503))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0503r@sainsburys.co.uk', (select Id from [site] where storeno = 503));
if exists((select Id from [site] where storeno = 695))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0695m@sainsburys.co.uk', (select Id from [site] where storeno = 695));
if exists((select Id from [site] where storeno = 695))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0695r@sainsburys.co.uk', (select Id from [site] where storeno = 695));
if exists((select Id from [site] where storeno = 421))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0421m@sainsburys.co.uk', (select Id from [site] where storeno = 421));
if exists((select Id from [site] where storeno = 421))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0421r@sainsburys.co.uk', (select Id from [site] where storeno = 421));
if exists((select Id from [site] where storeno = 2017))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2017m@sainsburys.co.uk', (select Id from [site] where storeno = 2017));
if exists((select Id from [site] where storeno = 2017))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2017r@sainsburys.co.uk', (select Id from [site] where storeno = 2017));
if exists((select Id from [site] where storeno = 725))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0725m@sainsburys.co.uk', (select Id from [site] where storeno = 725));
if exists((select Id from [site] where storeno = 725))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0725r@sainsburys.co.uk', (select Id from [site] where storeno = 725));
if exists((select Id from [site] where storeno = 2023))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2023m@sainsburys.co.uk', (select Id from [site] where storeno = 2023));
if exists((select Id from [site] where storeno = 2023))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2023r@sainsburys.co.uk', (select Id from [site] where storeno = 2023));
if exists((select Id from [site] where storeno = 567))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0567m@sainsburys.co.uk', (select Id from [site] where storeno = 567));
if exists((select Id from [site] where storeno = 567))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0567pfs@sainsburys.co.uk', (select Id from [site] where storeno = 567));
if exists((select Id from [site] where storeno = 567))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0567r@sainsburys.co.uk', (select Id from [site] where storeno = 567));
if exists((select Id from [site] where storeno = 706))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0706m@sainsburys.co.uk', (select Id from [site] where storeno = 706));
if exists((select Id from [site] where storeno = 706))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0706r@sainsburys.co.uk', (select Id from [site] where storeno = 706));
if exists((select Id from [site] where storeno = 18))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0018m@sainsburys.co.uk', (select Id from [site] where storeno = 18));
if exists((select Id from [site] where storeno = 18))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0018r@sainsburys.co.uk', (select Id from [site] where storeno = 18));
if exists((select Id from [site] where storeno = 852))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0852m@sainsburys.co.uk', (select Id from [site] where storeno = 852));
if exists((select Id from [site] where storeno = 852))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0852r@sainsburys.co.uk', (select Id from [site] where storeno = 852));
if exists((select Id from [site] where storeno = 433))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0433m@sainsburys.co.uk', (select Id from [site] where storeno = 433));
if exists((select Id from [site] where storeno = 433))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0433r@sainsburys.co.uk', (select Id from [site] where storeno = 433));
if exists((select Id from [site] where storeno = 4517))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('4517m@sainsburys.co.uk', (select Id from [site] where storeno = 4517));
if exists((select Id from [site] where storeno = 4517))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb4517r@sainsburys.co.uk', (select Id from [site] where storeno = 4517));
if exists((select Id from [site] where storeno = 58))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0058m@sainsburys.co.uk', (select Id from [site] where storeno = 58));
if exists((select Id from [site] where storeno = 58))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0058r@sainsburys.co.uk', (select Id from [site] where storeno = 58));
if exists((select Id from [site] where storeno = 605))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0605m@sainsburys.co.uk', (select Id from [site] where storeno = 605));
if exists((select Id from [site] where storeno = 605))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0605r@sainsburys.co.uk', (select Id from [site] where storeno = 605));
if exists((select Id from [site] where storeno = 691))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0691m@sainsburys.co.uk', (select Id from [site] where storeno = 691));
if exists((select Id from [site] where storeno = 691))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0691r@sainsburys.co.uk', (select Id from [site] where storeno = 691));
if exists((select Id from [site] where storeno = 714))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0714m@sainsburys.co.uk', (select Id from [site] where storeno = 714));
if exists((select Id from [site] where storeno = 714))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0714r@sainsburys.co.uk', (select Id from [site] where storeno = 714));
if exists((select Id from [site] where storeno = 2092))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2092m@sainsburys.co.uk', (select Id from [site] where storeno = 2092));
if exists((select Id from [site] where storeno = 2092))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2092r@sainsburys.co.uk', (select Id from [site] where storeno = 2092));
if exists((select Id from [site] where storeno = 2112))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2112m@sainsburys.co.uk', (select Id from [site] where storeno = 2112));
if exists((select Id from [site] where storeno = 2112))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('PFS.MeadowHallS@sainsburys.co.uk', (select Id from [site] where storeno = 2112));
if exists((select Id from [site] where storeno = 2112))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2112r@sainsburys.co.uk', (select Id from [site] where storeno = 2112));
if exists((select Id from [site] where storeno = 521))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0521m@sainsburys.co.uk', (select Id from [site] where storeno = 521));
if exists((select Id from [site] where storeno = 521))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0521r@sainsburys.co.uk', (select Id from [site] where storeno = 521));
if exists((select Id from [site] where storeno = 566))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0566m@sainsburys.co.uk', (select Id from [site] where storeno = 566));
if exists((select Id from [site] where storeno = 566))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0566r@sainsburys.co.uk', (select Id from [site] where storeno = 566));
if exists((select Id from [site] where storeno = 790))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0790m@sainsburys.co.uk', (select Id from [site] where storeno = 790));
if exists((select Id from [site] where storeno = 790))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0790r@sainsburys.co.uk', (select Id from [site] where storeno = 790));
if exists((select Id from [site] where storeno = 713))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0713m@sainsburys.co.uk', (select Id from [site] where storeno = 713));
if exists((select Id from [site] where storeno = 713))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0713r@sainsburys.co.uk', (select Id from [site] where storeno = 713));
if exists((select Id from [site] where storeno = 2170))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2170m@sainsburys.co.uk', (select Id from [site] where storeno = 2170));
if exists((select Id from [site] where storeno = 2170))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2170r@sainsburys.co.uk', (select Id from [site] where storeno = 2170));
if exists((select Id from [site] where storeno = 31))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0031m@sainsburys.co.uk', (select Id from [site] where storeno = 31));
if exists((select Id from [site] where storeno = 31))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0031r@sainsburys.co.uk', (select Id from [site] where storeno = 31));
if exists((select Id from [site] where storeno = 690))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0690m@sainsburys.co.uk', (select Id from [site] where storeno = 690));
if exists((select Id from [site] where storeno = 690))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0690r@sainsburys.co.uk', (select Id from [site] where storeno = 690));
if exists((select Id from [site] where storeno = 2136))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2136m@sainsburys.co.uk', (select Id from [site] where storeno = 2136));
if exists((select Id from [site] where storeno = 2136))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2136r@sainsburys.co.uk', (select Id from [site] where storeno = 2136));
if exists((select Id from [site] where storeno = 2246))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2246m@sainsburys.co.uk', (select Id from [site] where storeno = 2246));
if exists((select Id from [site] where storeno = 2246))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2246r@sainsburys.co.uk', (select Id from [site] where storeno = 2246));
if exists((select Id from [site] where storeno = 882))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0882m@sainsburys.co.uk', (select Id from [site] where storeno = 882));
if exists((select Id from [site] where storeno = 882))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0882r@sainsburys.co.uk', (select Id from [site] where storeno = 882));
if exists((select Id from [site] where storeno = 38))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0038m@sainsburys.co.uk', (select Id from [site] where storeno = 38));
if exists((select Id from [site] where storeno = 38))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0038r@sainsburys.co.uk', (select Id from [site] where storeno = 38));
if exists((select Id from [site] where storeno = 2046))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2046m@sainsburys.co.uk', (select Id from [site] where storeno = 2046));
if exists((select Id from [site] where storeno = 2046))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2046r@sainsburys.co.uk', (select Id from [site] where storeno = 2046));
if exists((select Id from [site] where storeno = 762))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0762m@sainsburys.co.uk', (select Id from [site] where storeno = 762));
if exists((select Id from [site] where storeno = 762))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0762r@sainsburys.co.uk', (select Id from [site] where storeno = 762));
if exists((select Id from [site] where storeno = 558))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0558m@sainsburys.co.uk', (select Id from [site] where storeno = 558));
if exists((select Id from [site] where storeno = 558))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0558r@sainsburys.co.uk', (select Id from [site] where storeno = 558));
if exists((select Id from [site] where storeno = 2063))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2063m@sainsburys.co.uk', (select Id from [site] where storeno = 2063));
if exists((select Id from [site] where storeno = 2063))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2063r@sainsburys.co.uk', (select Id from [site] where storeno = 2063));
if exists((select Id from [site] where storeno = 2059))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2059m@sainsburys.co.uk', (select Id from [site] where storeno = 2059));
if exists((select Id from [site] where storeno = 2059))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2059r@sainsburys.co.uk', (select Id from [site] where storeno = 2059));
if exists((select Id from [site] where storeno = 801))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0801m@sainsburys.co.uk', (select Id from [site] where storeno = 801));
if exists((select Id from [site] where storeno = 801))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0801r@sainsburys.co.uk', (select Id from [site] where storeno = 801));
if exists((select Id from [site] where storeno = 3))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0003m@sainsburys.co.uk', (select Id from [site] where storeno = 3));
if exists((select Id from [site] where storeno = 3))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0003r@sainsburys.co.uk', (select Id from [site] where storeno = 3));
if exists((select Id from [site] where storeno = 823))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0823m@sainsburys.co.uk', (select Id from [site] where storeno = 823));
if exists((select Id from [site] where storeno = 823))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0823r@sainsburys.co.uk', (select Id from [site] where storeno = 823));
if exists((select Id from [site] where storeno = 417))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0417m@sainsburys.co.uk', (select Id from [site] where storeno = 417));
if exists((select Id from [site] where storeno = 417))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0417r@sainsburys.co.uk', (select Id from [site] where storeno = 417));
if exists((select Id from [site] where storeno = 682))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0682m@sainsburys.co.uk', (select Id from [site] where storeno = 682));
if exists((select Id from [site] where storeno = 682))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0682r@sainsburys.co.uk', (select Id from [site] where storeno = 682));
if exists((select Id from [site] where storeno = 441))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0441m@sainsburys.co.uk', (select Id from [site] where storeno = 441));
if exists((select Id from [site] where storeno = 441))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0441r@sainsburys.co.uk', (select Id from [site] where storeno = 441));
if exists((select Id from [site] where storeno = 52))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0052m@sainsburys.co.uk', (select Id from [site] where storeno = 52));
if exists((select Id from [site] where storeno = 52))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0052r@sainsburys.co.uk', (select Id from [site] where storeno = 52));
if exists((select Id from [site] where storeno = 413))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0413m@sainsburys.co.uk', (select Id from [site] where storeno = 413));
if exists((select Id from [site] where storeno = 413))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0413r@sainsburys.co.uk', (select Id from [site] where storeno = 413));
if exists((select Id from [site] where storeno = 72))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0072m@sainsburys.co.uk', (select Id from [site] where storeno = 72));
if exists((select Id from [site] where storeno = 72))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0072r@sainsburys.co.uk', (select Id from [site] where storeno = 72));
if exists((select Id from [site] where storeno = 604))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0604m@sainsburys.co.uk', (select Id from [site] where storeno = 604));
if exists((select Id from [site] where storeno = 604))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0604r@sainsburys.co.uk', (select Id from [site] where storeno = 604));
if exists((select Id from [site] where storeno = 853))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0853m@sainsburys.co.uk', (select Id from [site] where storeno = 853));
if exists((select Id from [site] where storeno = 853))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0853r@sainsburys.co.uk', (select Id from [site] where storeno = 853));
if exists((select Id from [site] where storeno = 633))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0633m@sainsburys.co.uk', (select Id from [site] where storeno = 633));
if exists((select Id from [site] where storeno = 633))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0633r@sainsburys.co.uk', (select Id from [site] where storeno = 633));
if exists((select Id from [site] where storeno = 2011))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2011m@sainsburys.co.uk', (select Id from [site] where storeno = 2011));
if exists((select Id from [site] where storeno = 2011))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2011r@sainsburys.co.uk', (select Id from [site] where storeno = 2011));
if exists((select Id from [site] where storeno = 548))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0548m@sainsburys.co.uk', (select Id from [site] where storeno = 548));
if exists((select Id from [site] where storeno = 548))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0548r@sainsburys.co.uk', (select Id from [site] where storeno = 548));
if exists((select Id from [site] where storeno = 742))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0742m@sainsburys.co.uk', (select Id from [site] where storeno = 742));
if exists((select Id from [site] where storeno = 742))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0742r@sainsburys.co.uk', (select Id from [site] where storeno = 742));
if exists((select Id from [site] where storeno = 696))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0696m@sainsburys.co.uk', (select Id from [site] where storeno = 696));
if exists((select Id from [site] where storeno = 696))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0696r@sainsburys.co.uk', (select Id from [site] where storeno = 696));
if exists((select Id from [site] where storeno = 514))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0514m@sainsburys.co.uk', (select Id from [site] where storeno = 514));
if exists((select Id from [site] where storeno = 514))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0514r@sainsburys.co.uk', (select Id from [site] where storeno = 514));
if exists((select Id from [site] where storeno = 873))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0873m@sainsburys.co.uk', (select Id from [site] where storeno = 873));
if exists((select Id from [site] where storeno = 873))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0873r@sainsburys.co.uk', (select Id from [site] where storeno = 873));
if exists((select Id from [site] where storeno = 2080))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2080m@sainsburys.co.uk', (select Id from [site] where storeno = 2080));
if exists((select Id from [site] where storeno = 2080))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2080r@sainsburys.co.uk', (select Id from [site] where storeno = 2080));
if exists((select Id from [site] where storeno = 27))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0027m@sainsburys.co.uk', (select Id from [site] where storeno = 27));
if exists((select Id from [site] where storeno = 27))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0027r@sainsburys.co.uk', (select Id from [site] where storeno = 27));
if exists((select Id from [site] where storeno = 867))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0867m@sainsburys.co.uk', (select Id from [site] where storeno = 867));
if exists((select Id from [site] where storeno = 867))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0867r@sainsburys.co.uk', (select Id from [site] where storeno = 867));
if exists((select Id from [site] where storeno = 63))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0063m@sainsburys.co.uk', (select Id from [site] where storeno = 63));
if exists((select Id from [site] where storeno = 63))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0063r@sainsburys.co.uk', (select Id from [site] where storeno = 63));
if exists((select Id from [site] where storeno = 793))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0793m@sainsburys.co.uk', (select Id from [site] where storeno = 793));
if exists((select Id from [site] where storeno = 793))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0793r@sainsburys.co.uk', (select Id from [site] where storeno = 793));
if exists((select Id from [site] where storeno = 718))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0718m@sainsburys.co.uk', (select Id from [site] where storeno = 718));
if exists((select Id from [site] where storeno = 718))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0718r@sainsburys.co.uk', (select Id from [site] where storeno = 718));
if exists((select Id from [site] where storeno = 644))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0644m@sainsburys.co.uk', (select Id from [site] where storeno = 644));
if exists((select Id from [site] where storeno = 644))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0644r@sainsburys.co.uk', (select Id from [site] where storeno = 644));
if exists((select Id from [site] where storeno = 2079))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2079m@sainsburys.co.uk', (select Id from [site] where storeno = 2079));
if exists((select Id from [site] where storeno = 2079))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2079r@sainsburys.co.uk', (select Id from [site] where storeno = 2079));
if exists((select Id from [site] where storeno = 415))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0415m@sainsburys.co.uk', (select Id from [site] where storeno = 415));
if exists((select Id from [site] where storeno = 415))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0415r@sainsburys.co.uk', (select Id from [site] where storeno = 415));
if exists((select Id from [site] where storeno = 2061))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2061m@sainsburys.co.uk', (select Id from [site] where storeno = 2061));
if exists((select Id from [site] where storeno = 2061))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2061r@sainsburys.co.uk', (select Id from [site] where storeno = 2061));
if exists((select Id from [site] where storeno = 629))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0629m@sainsburys.co.uk', (select Id from [site] where storeno = 629));
if exists((select Id from [site] where storeno = 629))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0629r@sainsburys.co.uk', (select Id from [site] where storeno = 629));
if exists((select Id from [site] where storeno = 661))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0661m@sainsburys.co.uk', (select Id from [site] where storeno = 661));
if exists((select Id from [site] where storeno = 661))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0661r@sainsburys.co.uk', (select Id from [site] where storeno = 661));
if exists((select Id from [site] where storeno = 679))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0679m@sainsburys.co.uk', (select Id from [site] where storeno = 679));
if exists((select Id from [site] where storeno = 679))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0679r@sainsburys.co.uk', (select Id from [site] where storeno = 679));
if exists((select Id from [site] where storeno = 2168))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2168m@sainsburys.co.uk', (select Id from [site] where storeno = 2168));
if exists((select Id from [site] where storeno = 2168))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2168r@sainsburys.co.uk', (select Id from [site] where storeno = 2168));
if exists((select Id from [site] where storeno = 410))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0410m@sainsburys.co.uk', (select Id from [site] where storeno = 410));
if exists((select Id from [site] where storeno = 410))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0410r@sainsburys.co.uk', (select Id from [site] where storeno = 410));
if exists((select Id from [site] where storeno = 529))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0529m@sainsburys.co.uk', (select Id from [site] where storeno = 529));
if exists((select Id from [site] where storeno = 529))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0529r@sainsburys.co.uk', (select Id from [site] where storeno = 529));
if exists((select Id from [site] where storeno = 2113))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2113m@sainsburys.co.uk', (select Id from [site] where storeno = 2113));
if exists((select Id from [site] where storeno = 2113))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2113r@sainsburys.co.uk', (select Id from [site] where storeno = 2113));
if exists((select Id from [site] where storeno = 812))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0812m@sainsburys.co.uk', (select Id from [site] where storeno = 812));
if exists((select Id from [site] where storeno = 812))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0812r@sainsburys.co.uk', (select Id from [site] where storeno = 812));
if exists((select Id from [site] where storeno = 745))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0745m@sainsburys.co.uk', (select Id from [site] where storeno = 745));
if exists((select Id from [site] where storeno = 745))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0745r@sainsburys.co.uk', (select Id from [site] where storeno = 745));
if exists((select Id from [site] where storeno = 788))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0788m@sainsburys.co.uk', (select Id from [site] where storeno = 788));
if exists((select Id from [site] where storeno = 788))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0788r@sainsburys.co.uk', (select Id from [site] where storeno = 788));
if exists((select Id from [site] where storeno = 774))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0774m@sainsburys.co.uk', (select Id from [site] where storeno = 774));
if exists((select Id from [site] where storeno = 774))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0774r@sainsburys.co.uk', (select Id from [site] where storeno = 774));
if exists((select Id from [site] where storeno = 753))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0753m@sainsburys.co.uk', (select Id from [site] where storeno = 753));
if exists((select Id from [site] where storeno = 753))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0753r@sainsburys.co.uk', (select Id from [site] where storeno = 753));
if exists((select Id from [site] where storeno = 510))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0510m@sainsburys.co.uk', (select Id from [site] where storeno = 510));
if exists((select Id from [site] where storeno = 510))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0510r@sainsburys.co.uk', (select Id from [site] where storeno = 510));
if exists((select Id from [site] where storeno = 693))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0693m@sainsburys.co.uk', (select Id from [site] where storeno = 693));
if exists((select Id from [site] where storeno = 693))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0693r@sainsburys.co.uk', (select Id from [site] where storeno = 693));
if exists((select Id from [site] where storeno = 676))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0676m@sainsburys.co.uk', (select Id from [site] where storeno = 676));
if exists((select Id from [site] where storeno = 676))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0676r@sainsburys.co.uk', (select Id from [site] where storeno = 676));
if exists((select Id from [site] where storeno = 887))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0887m@sainsburys.co.uk', (select Id from [site] where storeno = 887));
if exists((select Id from [site] where storeno = 887))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0887r@sainsburys.co.uk', (select Id from [site] where storeno = 887));
if exists((select Id from [site] where storeno = 2078))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2078m@sainsburys.co.uk', (select Id from [site] where storeno = 2078));
if exists((select Id from [site] where storeno = 2078))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2078r@sainsburys.co.uk', (select Id from [site] where storeno = 2078));
if exists((select Id from [site] where storeno = 2002))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2002m@sainsburys.co.uk', (select Id from [site] where storeno = 2002));
if exists((select Id from [site] where storeno = 2002))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2002r@sainsburys.co.uk', (select Id from [site] where storeno = 2002));
if exists((select Id from [site] where storeno = 516))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0516m@sainsburys.co.uk', (select Id from [site] where storeno = 516));
if exists((select Id from [site] where storeno = 516))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0516r@sainsburys.co.uk', (select Id from [site] where storeno = 516));
if exists((select Id from [site] where storeno = 2324))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2324m@sainsburys.co.uk', (select Id from [site] where storeno = 2324));
if exists((select Id from [site] where storeno = 2324))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2324r@sainsburys.co.uk', (select Id from [site] where storeno = 2324));
if exists((select Id from [site] where storeno = 474))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0474m@sainsburys.co.uk', (select Id from [site] where storeno = 474));
if exists((select Id from [site] where storeno = 474))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0474r@sainsburys.co.uk', (select Id from [site] where storeno = 474));
if exists((select Id from [site] where storeno = 403))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0403m@sainsburys.co.uk', (select Id from [site] where storeno = 403));
if exists((select Id from [site] where storeno = 403))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0403r@sainsburys.co.uk', (select Id from [site] where storeno = 403));
if exists((select Id from [site] where storeno = 732))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0732m@sainsburys.co.uk', (select Id from [site] where storeno = 732));
if exists((select Id from [site] where storeno = 732))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0732r@sainsburys.co.uk', (select Id from [site] where storeno = 732));
if exists((select Id from [site] where storeno = 509))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0509m@sainsburys.co.uk', (select Id from [site] where storeno = 509));
if exists((select Id from [site] where storeno = 509))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0509r@sainsburys.co.uk', (select Id from [site] where storeno = 509));
if exists((select Id from [site] where storeno = 206))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0206m@sainsburys.co.uk', (select Id from [site] where storeno = 206));
if exists((select Id from [site] where storeno = 206))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0206r@sainsburys.co.uk', (select Id from [site] where storeno = 206));
if exists((select Id from [site] where storeno = 668))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0668m@sainsburys.co.uk', (select Id from [site] where storeno = 668));
if exists((select Id from [site] where storeno = 668))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0668r@sainsburys.co.uk', (select Id from [site] where storeno = 668));
if exists((select Id from [site] where storeno = 671))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0671m@sainsburys.co.uk', (select Id from [site] where storeno = 671));
if exists((select Id from [site] where storeno = 671))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0671r@sainsburys.co.uk', (select Id from [site] where storeno = 671));
if exists((select Id from [site] where storeno = 20))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0020m@sainsburys.co.uk', (select Id from [site] where storeno = 20));
if exists((select Id from [site] where storeno = 20))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0020r@sainsburys.co.uk', (select Id from [site] where storeno = 20));
if exists((select Id from [site] where storeno = 740))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0740m@sainsburys.co.uk', (select Id from [site] where storeno = 740));
if exists((select Id from [site] where storeno = 740))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0740r@sainsburys.co.uk', (select Id from [site] where storeno = 740));
if exists((select Id from [site] where storeno = 2295))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2295m@sainsburys.co.uk', (select Id from [site] where storeno = 2295));
if exists((select Id from [site] where storeno = 2295))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2295r@sainsburys.co.uk', (select Id from [site] where storeno = 2295));
if exists((select Id from [site] where storeno = 2244))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2244m@sainsburys.co.uk', (select Id from [site] where storeno = 2244));
if exists((select Id from [site] where storeno = 2244))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2244r@sainsburys.co.uk', (select Id from [site] where storeno = 2244));
if exists((select Id from [site] where storeno = 408))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0408m@sainsburys.co.uk', (select Id from [site] where storeno = 408));
if exists((select Id from [site] where storeno = 408))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0408r@sainsburys.co.uk', (select Id from [site] where storeno = 408));
if exists((select Id from [site] where storeno = 851))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0851m@sainsburys.co.uk', (select Id from [site] where storeno = 851));
if exists((select Id from [site] where storeno = 851))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0851r@sainsburys.co.uk', (select Id from [site] where storeno = 851));
if exists((select Id from [site] where storeno = 555))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0555m@sainsburys.co.uk', (select Id from [site] where storeno = 555));
if exists((select Id from [site] where storeno = 555))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0555r@sainsburys.co.uk', (select Id from [site] where storeno = 555));
if exists((select Id from [site] where storeno = 675))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0675m@sainsburys.co.uk', (select Id from [site] where storeno = 675));
if exists((select Id from [site] where storeno = 675))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0675r@sainsburys.co.uk', (select Id from [site] where storeno = 675));
if exists((select Id from [site] where storeno = 680))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0680m@sainsburys.co.uk', (select Id from [site] where storeno = 680));
if exists((select Id from [site] where storeno = 680))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0680r@sainsburys.co.uk', (select Id from [site] where storeno = 680));
if exists((select Id from [site] where storeno = 2040))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2040m@sainsburys.co.uk', (select Id from [site] where storeno = 2040));
if exists((select Id from [site] where storeno = 2040))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2040r@sainsburys.co.uk', (select Id from [site] where storeno = 2040));
if exists((select Id from [site] where storeno = 648))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0648m@sainsburys.co.uk', (select Id from [site] where storeno = 648));
if exists((select Id from [site] where storeno = 648))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0648r@sainsburys.co.uk', (select Id from [site] where storeno = 648));
if exists((select Id from [site] where storeno = 610))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0610m@sainsburys.co.uk', (select Id from [site] where storeno = 610));
if exists((select Id from [site] where storeno = 610))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0610r@sainsburys.co.uk', (select Id from [site] where storeno = 610));
if exists((select Id from [site] where storeno = 26))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0026m@sainsburys.co.uk', (select Id from [site] where storeno = 26));
if exists((select Id from [site] where storeno = 26))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0026r@sainsburys.co.uk', (select Id from [site] where storeno = 26));
if exists((select Id from [site] where storeno = 51))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0051m@sainsburys.co.uk', (select Id from [site] where storeno = 51));
if exists((select Id from [site] where storeno = 51))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0051r@sainsburys.co.uk', (select Id from [site] where storeno = 51));
if exists((select Id from [site] where storeno = 57))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0057m@sainsburys.co.uk', (select Id from [site] where storeno = 57));
if exists((select Id from [site] where storeno = 57))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0057r@sainsburys.co.uk', (select Id from [site] where storeno = 57));
if exists((select Id from [site] where storeno = 460))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('', (select Id from [site] where storeno = 460));
if exists((select Id from [site] where storeno = 460))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('', (select Id from [site] where storeno = 460));
if exists((select Id from [site] where storeno = 864))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0864m@sainsburys.co.uk', (select Id from [site] where storeno = 864));
if exists((select Id from [site] where storeno = 864))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0864r@sainsburys.co.uk', (select Id from [site] where storeno = 864));
if exists((select Id from [site] where storeno = 55))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0055m@sainsburys.co.uk', (select Id from [site] where storeno = 55));
if exists((select Id from [site] where storeno = 55))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0055r@sainsburys.co.uk', (select Id from [site] where storeno = 55));
if exists((select Id from [site] where storeno = 854))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0854m@sainsburys.co.uk', (select Id from [site] where storeno = 854));
if exists((select Id from [site] where storeno = 854))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0854r@sainsburys.co.uk', (select Id from [site] where storeno = 854));
if exists((select Id from [site] where storeno = 678))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0678m@sainsburys.co.uk', (select Id from [site] where storeno = 678));
if exists((select Id from [site] where storeno = 678))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0678r@sainsburys.co.uk', (select Id from [site] where storeno = 678));
if exists((select Id from [site] where storeno = 735))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0735m@sainsburys.co.uk', (select Id from [site] where storeno = 735));
if exists((select Id from [site] where storeno = 735))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0735r@sainsburys.co.uk', (select Id from [site] where storeno = 735));
if exists((select Id from [site] where storeno = 820))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0820m@sainsburys.co.uk', (select Id from [site] where storeno = 820));
if exists((select Id from [site] where storeno = 820))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0820r@sainsburys.co.uk', (select Id from [site] where storeno = 820));
if exists((select Id from [site] where storeno = 890))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0890m@sainsburys.co.uk', (select Id from [site] where storeno = 890));
if exists((select Id from [site] where storeno = 890))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0890r@sainsburys.co.uk', (select Id from [site] where storeno = 890));
if exists((select Id from [site] where storeno = 4518))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb4518r@sainsburys.co.uk', (select Id from [site] where storeno = 4518));
if exists((select Id from [site] where storeno = 4518))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('4518m@sainsburys.co.uk', (select Id from [site] where storeno = 4518));
if exists((select Id from [site] where storeno = 3))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0003pfs@sainsburys.co.uk', (select Id from [site] where storeno = 3));
if exists((select Id from [site] where storeno = 4))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0004pfs@sainsburys.co.uk', (select Id from [site] where storeno = 4));
if exists((select Id from [site] where storeno = 5))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0005pfs@sainsburys.co.uk', (select Id from [site] where storeno = 5));
if exists((select Id from [site] where storeno = 6))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0006pfs@sainsburys.co.uk', (select Id from [site] where storeno = 6));
if exists((select Id from [site] where storeno = 7))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0007pfs@sainsburys.co.uk', (select Id from [site] where storeno = 7));
if exists((select Id from [site] where storeno = 8))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0008pfs@sainsburys.co.uk', (select Id from [site] where storeno = 8));
if exists((select Id from [site] where storeno = 17))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0017pfs@sainsburys.co.uk', (select Id from [site] where storeno = 17));
if exists((select Id from [site] where storeno = 18))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0018pfs@sainsburys.co.uk', (select Id from [site] where storeno = 18));
if exists((select Id from [site] where storeno = 20))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0020pfs@sainsburys.co.uk', (select Id from [site] where storeno = 20));
if exists((select Id from [site] where storeno = 2324))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2324pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2324));
if exists((select Id from [site] where storeno = 26))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0026pfs@sainsburys.co.uk', (select Id from [site] where storeno = 26));
if exists((select Id from [site] where storeno = 27))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0027pfs@sainsburys.co.uk', (select Id from [site] where storeno = 27));
if exists((select Id from [site] where storeno = 28))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0028pfs@sainsburys.co.uk', (select Id from [site] where storeno = 28));
if exists((select Id from [site] where storeno = 31))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0031pfs@sainsburys.co.uk', (select Id from [site] where storeno = 31));
if exists((select Id from [site] where storeno = 38))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0038pfs@sainsburys.co.uk', (select Id from [site] where storeno = 38));
if exists((select Id from [site] where storeno = 51))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0051pfs@sainsburys.co.uk', (select Id from [site] where storeno = 51));
if exists((select Id from [site] where storeno = 52))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0052pfs@sainsburys.co.uk', (select Id from [site] where storeno = 52));
if exists((select Id from [site] where storeno = 53))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0053pfs@sainsburys.co.uk', (select Id from [site] where storeno = 53));
if exists((select Id from [site] where storeno = 55))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0055pfs@sainsburys.co.uk', (select Id from [site] where storeno = 55));
if exists((select Id from [site] where storeno = 57))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0057pfs@sainsburys.co.uk', (select Id from [site] where storeno = 57));
if exists((select Id from [site] where storeno = 58))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0058pfs@sainsburys.co.uk', (select Id from [site] where storeno = 58));
if exists((select Id from [site] where storeno = 59))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0059pfs@sainsburys.co.uk', (select Id from [site] where storeno = 59));
if exists((select Id from [site] where storeno = 62))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0062pfs@sainsburys.co.uk', (select Id from [site] where storeno = 62));
if exists((select Id from [site] where storeno = 63))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0063pfs@sainsburys.co.uk', (select Id from [site] where storeno = 63));
if exists((select Id from [site] where storeno = 72))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0072pfs@sainsburys.co.uk', (select Id from [site] where storeno = 72));
if exists((select Id from [site] where storeno = 206))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0206pfs@sainsburys.co.uk', (select Id from [site] where storeno = 206));
if exists((select Id from [site] where storeno = 224))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0224pfs@sainsburys.co.uk', (select Id from [site] where storeno = 224));
if exists((select Id from [site] where storeno = 225))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0225pfs@sainsburys.co.uk', (select Id from [site] where storeno = 225));
if exists((select Id from [site] where storeno = 391))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0391pfs@sainsburys.co.uk', (select Id from [site] where storeno = 391));
if exists((select Id from [site] where storeno = 402))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0402pfs@sainsburys.co.uk', (select Id from [site] where storeno = 402));
if exists((select Id from [site] where storeno = 403))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0403pfs@sainsburys.co.uk', (select Id from [site] where storeno = 403));
if exists((select Id from [site] where storeno = 406))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0406pfs@sainsburys.co.uk', (select Id from [site] where storeno = 406));
if exists((select Id from [site] where storeno = 408))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0408pfs@sainsburys.co.uk', (select Id from [site] where storeno = 408));
if exists((select Id from [site] where storeno = 410))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0410pfs@sainsburys.co.uk', (select Id from [site] where storeno = 410));
if exists((select Id from [site] where storeno = 411))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0411pfs@sainsburys.co.uk', (select Id from [site] where storeno = 411));
if exists((select Id from [site] where storeno = 2168))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2168pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2168));
if exists((select Id from [site] where storeno = 413))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0413pfs@sainsburys.co.uk', (select Id from [site] where storeno = 413));
if exists((select Id from [site] where storeno = 415))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0415pfs@sainsburys.co.uk', (select Id from [site] where storeno = 415));
if exists((select Id from [site] where storeno = 417))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0417pfs@sainsburys.co.uk', (select Id from [site] where storeno = 417));
if exists((select Id from [site] where storeno = 419))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0419pfs@sainsburys.co.uk', (select Id from [site] where storeno = 419));
if exists((select Id from [site] where storeno = 421))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0421pfs@sainsburys.co.uk', (select Id from [site] where storeno = 421));
if exists((select Id from [site] where storeno = 422))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0422pfs@sainsburys.co.uk', (select Id from [site] where storeno = 422));
if exists((select Id from [site] where storeno = 426))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0426pfs@sainsburys.co.uk', (select Id from [site] where storeno = 426));
if exists((select Id from [site] where storeno = 433))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0433pfs@sainsburys.co.uk', (select Id from [site] where storeno = 433));
if exists((select Id from [site] where storeno = 434))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0434pfs@sainsburys.co.uk', (select Id from [site] where storeno = 434));
if exists((select Id from [site] where storeno = 441))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0441pfs@sainsburys.co.uk', (select Id from [site] where storeno = 441));
if exists((select Id from [site] where storeno = 443))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0443pfs@sainsburys.co.uk', (select Id from [site] where storeno = 443));
if exists((select Id from [site] where storeno = 444))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0444pfs@sainsburys.co.uk', (select Id from [site] where storeno = 444));
if exists((select Id from [site] where storeno = 474))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0474pfs@sainsburys.co.uk', (select Id from [site] where storeno = 474));
if exists((select Id from [site] where storeno = 2246))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2246pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2246));
if exists((select Id from [site] where storeno = 503))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0503pfs@sainsburys.co.uk', (select Id from [site] where storeno = 503));
if exists((select Id from [site] where storeno = 505))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0505pfs@sainsburys.co.uk', (select Id from [site] where storeno = 505));
if exists((select Id from [site] where storeno = 509))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0509pfs@sainsburys.co.uk', (select Id from [site] where storeno = 509));
if exists((select Id from [site] where storeno = 510))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0510pfs@sainsburys.co.uk', (select Id from [site] where storeno = 510));
if exists((select Id from [site] where storeno = 514))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0514pfs@sainsburys.co.uk', (select Id from [site] where storeno = 514));
if exists((select Id from [site] where storeno = 516))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0516pfs@sainsburys.co.uk', (select Id from [site] where storeno = 516));
if exists((select Id from [site] where storeno = 519))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0519pfs@sainsburys.co.uk', (select Id from [site] where storeno = 519));
if exists((select Id from [site] where storeno = 521))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0521pfs@sainsburys.co.uk', (select Id from [site] where storeno = 521));
if exists((select Id from [site] where storeno = 523))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0523pfs@sainsburys.co.uk', (select Id from [site] where storeno = 523));
if exists((select Id from [site] where storeno = 524))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0524pfs@sainsburys.co.uk', (select Id from [site] where storeno = 524));
if exists((select Id from [site] where storeno = 529))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0529pfs@sainsburys.co.uk', (select Id from [site] where storeno = 529));
if exists((select Id from [site] where storeno = 534))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0534pfs@sainsburys.co.uk', (select Id from [site] where storeno = 534));
if exists((select Id from [site] where storeno = 535))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0535pfs@sainsburys.co.uk', (select Id from [site] where storeno = 535));
if exists((select Id from [site] where storeno = 537))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0537pfs@sainsburys.co.uk', (select Id from [site] where storeno = 537));
if exists((select Id from [site] where storeno = 541))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0541pfs@sainsburys.co.uk', (select Id from [site] where storeno = 541));
if exists((select Id from [site] where storeno = 543))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0543pfs@sainsburys.co.uk', (select Id from [site] where storeno = 543));
if exists((select Id from [site] where storeno = 548))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0548pfs@sainsburys.co.uk', (select Id from [site] where storeno = 548));
if exists((select Id from [site] where storeno = 549))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0549pfs@sainsburys.co.uk', (select Id from [site] where storeno = 549));
if exists((select Id from [site] where storeno = 555))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0555pfs@sainsburys.co.uk', (select Id from [site] where storeno = 555));
if exists((select Id from [site] where storeno = 556))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0556pfs@sainsburys.co.uk', (select Id from [site] where storeno = 556));
if exists((select Id from [site] where storeno = 558))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0558pfs@sainsburys.co.uk', (select Id from [site] where storeno = 558));
if exists((select Id from [site] where storeno = 559))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0559pfs@sainsburys.co.uk', (select Id from [site] where storeno = 559));
if exists((select Id from [site] where storeno = 560))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0560pfs@sainsburys.co.uk', (select Id from [site] where storeno = 560));
if exists((select Id from [site] where storeno = 566))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0566pfs@sainsburys.co.uk', (select Id from [site] where storeno = 566));
if exists((select Id from [site] where storeno = 569))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0569pfs@sainsburys.co.uk', (select Id from [site] where storeno = 569));
if exists((select Id from [site] where storeno = 600))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0600pfs@sainsburys.co.uk', (select Id from [site] where storeno = 600));
if exists((select Id from [site] where storeno = 601))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0601pfs@sainsburys.co.uk', (select Id from [site] where storeno = 601));
if exists((select Id from [site] where storeno = 602))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0602pfs@sainsburys.co.uk', (select Id from [site] where storeno = 602));
if exists((select Id from [site] where storeno = 604))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0604pfs@sainsburys.co.uk', (select Id from [site] where storeno = 604));
if exists((select Id from [site] where storeno = 605))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0605pfs@sainsburys.co.uk', (select Id from [site] where storeno = 605));
if exists((select Id from [site] where storeno = 608))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0608pfs@sainsburys.co.uk', (select Id from [site] where storeno = 608));
if exists((select Id from [site] where storeno = 609))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0609pfs@sainsburys.co.uk', (select Id from [site] where storeno = 609));
if exists((select Id from [site] where storeno = 610))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0610pfs@sainsburys.co.uk', (select Id from [site] where storeno = 610));
if exists((select Id from [site] where storeno = 611))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0611pfs@sainsburys.co.uk', (select Id from [site] where storeno = 611));
if exists((select Id from [site] where storeno = 628))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0628pfs@sainsburys.co.uk', (select Id from [site] where storeno = 628));
if exists((select Id from [site] where storeno = 629))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0629pfs@sainsburys.co.uk', (select Id from [site] where storeno = 629));
if exists((select Id from [site] where storeno = 630))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0630pfs@sainsburys.co.uk', (select Id from [site] where storeno = 630));
if exists((select Id from [site] where storeno = 631))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0631pfs@sainsburys.co.uk', (select Id from [site] where storeno = 631));
if exists((select Id from [site] where storeno = 633))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0633pfs@sainsburys.co.uk', (select Id from [site] where storeno = 633));
if exists((select Id from [site] where storeno = 634))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0634pfs@sainsburys.co.uk', (select Id from [site] where storeno = 634));
if exists((select Id from [site] where storeno = 637))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0637pfs@sainsburys.co.uk', (select Id from [site] where storeno = 637));
if exists((select Id from [site] where storeno = 639))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0639pfs@sainsburys.co.uk', (select Id from [site] where storeno = 639));
if exists((select Id from [site] where storeno = 640))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0640pfs@sainsburys.co.uk', (select Id from [site] where storeno = 640));
if exists((select Id from [site] where storeno = 642))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0642pfs@sainsburys.co.uk', (select Id from [site] where storeno = 642));
if exists((select Id from [site] where storeno = 643))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0643pfs@sainsburys.co.uk', (select Id from [site] where storeno = 643));
if exists((select Id from [site] where storeno = 644))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0644pfs@sainsburys.co.uk', (select Id from [site] where storeno = 644));
if exists((select Id from [site] where storeno = 646))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0646pfs@sainsburys.co.uk', (select Id from [site] where storeno = 646));
if exists((select Id from [site] where storeno = 648))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0648pfs@sainsburys.co.uk', (select Id from [site] where storeno = 648));
if exists((select Id from [site] where storeno = 652))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0652pfs@sainsburys.co.uk', (select Id from [site] where storeno = 652));
if exists((select Id from [site] where storeno = 656))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0656pfs@sainsburys.co.uk', (select Id from [site] where storeno = 656));
if exists((select Id from [site] where storeno = 657))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0657pfs@sainsburys.co.uk', (select Id from [site] where storeno = 657));
if exists((select Id from [site] where storeno = 661))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0661pfs@sainsburys.co.uk', (select Id from [site] where storeno = 661));
if exists((select Id from [site] where storeno = 662))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0662pfs@sainsburys.co.uk', (select Id from [site] where storeno = 662));
if exists((select Id from [site] where storeno = 664))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0664pfs@sainsburys.co.uk', (select Id from [site] where storeno = 664));
if exists((select Id from [site] where storeno = 665))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0665pfs@sainsburys.co.uk', (select Id from [site] where storeno = 665));
if exists((select Id from [site] where storeno = 666))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0666pfs@sainsburys.co.uk', (select Id from [site] where storeno = 666));
if exists((select Id from [site] where storeno = 667))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0667pfs@sainsburys.co.uk', (select Id from [site] where storeno = 667));
if exists((select Id from [site] where storeno = 668))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0668pfs@sainsburys.co.uk', (select Id from [site] where storeno = 668));
if exists((select Id from [site] where storeno = 669))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0669pfs@sainsburys.co.uk', (select Id from [site] where storeno = 669));
if exists((select Id from [site] where storeno = 670))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0670pfs@sainsburys.co.uk', (select Id from [site] where storeno = 670));
if exists((select Id from [site] where storeno = 671))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0671pfs@sainsburys.co.uk', (select Id from [site] where storeno = 671));
if exists((select Id from [site] where storeno = 672))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0672pfs@sainsburys.co.uk', (select Id from [site] where storeno = 672));
if exists((select Id from [site] where storeno = 673))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0673pfs@sainsburys.co.uk', (select Id from [site] where storeno = 673));
if exists((select Id from [site] where storeno = 674))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0674pfs@sainsburys.co.uk', (select Id from [site] where storeno = 674));
if exists((select Id from [site] where storeno = 675))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0675pfs@sainsburys.co.uk', (select Id from [site] where storeno = 675));
if exists((select Id from [site] where storeno = 676))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0676pfs@sainsburys.co.uk', (select Id from [site] where storeno = 676));
if exists((select Id from [site] where storeno = 677))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0677pfs@sainsburys.co.uk', (select Id from [site] where storeno = 677));
if exists((select Id from [site] where storeno = 678))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0678pfs@sainsburys.co.uk', (select Id from [site] where storeno = 678));
if exists((select Id from [site] where storeno = 679))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0679pfs@sainsburys.co.uk', (select Id from [site] where storeno = 679));
if exists((select Id from [site] where storeno = 680))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0680pfs@sainsburys.co.uk', (select Id from [site] where storeno = 680));
if exists((select Id from [site] where storeno = 681))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0681pfs@sainsburys.co.uk', (select Id from [site] where storeno = 681));
if exists((select Id from [site] where storeno = 682))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0682pfs@sainsburys.co.uk', (select Id from [site] where storeno = 682));
if exists((select Id from [site] where storeno = 683))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0683pfs@sainsburys.co.uk', (select Id from [site] where storeno = 683));
if exists((select Id from [site] where storeno = 685))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0685pfs@sainsburys.co.uk', (select Id from [site] where storeno = 685));
if exists((select Id from [site] where storeno = 686))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0686pfs@sainsburys.co.uk', (select Id from [site] where storeno = 686));
if exists((select Id from [site] where storeno = 687))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0687pfs@sainsburys.co.uk', (select Id from [site] where storeno = 687));
if exists((select Id from [site] where storeno = 690))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0690pfs@sainsburys.co.uk', (select Id from [site] where storeno = 690));
if exists((select Id from [site] where storeno = 691))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0691pfs@sainsburys.co.uk', (select Id from [site] where storeno = 691));
if exists((select Id from [site] where storeno = 693))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0693pfs@sainsburys.co.uk', (select Id from [site] where storeno = 693));
if exists((select Id from [site] where storeno = 695))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0695pfs@sainsburys.co.uk', (select Id from [site] where storeno = 695));
if exists((select Id from [site] where storeno = 696))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0696pfs@sainsburys.co.uk', (select Id from [site] where storeno = 696));
if exists((select Id from [site] where storeno = 706))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0706pfs@sainsburys.co.uk', (select Id from [site] where storeno = 706));
if exists((select Id from [site] where storeno = 709))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0709pfs@sainsburys.co.uk', (select Id from [site] where storeno = 709));
if exists((select Id from [site] where storeno = 711))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0711pfs@sainsburys.co.uk', (select Id from [site] where storeno = 711));
if exists((select Id from [site] where storeno = 713))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0713pfs@sainsburys.co.uk', (select Id from [site] where storeno = 713));
if exists((select Id from [site] where storeno = 714))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0714pfs@sainsburys.co.uk', (select Id from [site] where storeno = 714));
if exists((select Id from [site] where storeno = 717))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0717pfs@sainsburys.co.uk', (select Id from [site] where storeno = 717));
if exists((select Id from [site] where storeno = 718))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0718pfs@sainsburys.co.uk', (select Id from [site] where storeno = 718));
if exists((select Id from [site] where storeno = 721))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0721pfs@sainsburys.co.uk', (select Id from [site] where storeno = 721));
if exists((select Id from [site] where storeno = 722))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0722pfs@sainsburys.co.uk', (select Id from [site] where storeno = 722));
if exists((select Id from [site] where storeno = 2295))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2295pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2295));
if exists((select Id from [site] where storeno = 725))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0725pfs@sainsburys.co.uk', (select Id from [site] where storeno = 725));
if exists((select Id from [site] where storeno = 727))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0727pfs@sainsburys.co.uk', (select Id from [site] where storeno = 727));
if exists((select Id from [site] where storeno = 732))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0732pfs@sainsburys.co.uk', (select Id from [site] where storeno = 732));
if exists((select Id from [site] where storeno = 735))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0735pfs@sainsburys.co.uk', (select Id from [site] where storeno = 735));
if exists((select Id from [site] where storeno = 2170))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2170pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2170));
if exists((select Id from [site] where storeno = 740))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0740pfs@sainsburys.co.uk', (select Id from [site] where storeno = 740));
if exists((select Id from [site] where storeno = 742))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0742pfs@sainsburys.co.uk', (select Id from [site] where storeno = 742));
if exists((select Id from [site] where storeno = 744))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0744pfs@sainsburys.co.uk', (select Id from [site] where storeno = 744));
if exists((select Id from [site] where storeno = 745))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0745pfs@sainsburys.co.uk', (select Id from [site] where storeno = 745));
if exists((select Id from [site] where storeno = 749))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0749pfs@sainsburys.co.uk', (select Id from [site] where storeno = 749));
if exists((select Id from [site] where storeno = 751))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0751pfs@sainsburys.co.uk', (select Id from [site] where storeno = 751));
if exists((select Id from [site] where storeno = 752))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0752pfs@sainsburys.co.uk', (select Id from [site] where storeno = 752));
if exists((select Id from [site] where storeno = 753))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0753pfs@sainsburys.co.uk', (select Id from [site] where storeno = 753));
if exists((select Id from [site] where storeno = 757))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0757pfs@sainsburys.co.uk', (select Id from [site] where storeno = 757));
if exists((select Id from [site] where storeno = 762))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0762pfs@sainsburys.co.uk', (select Id from [site] where storeno = 762));
if exists((select Id from [site] where storeno = 765))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0765pfs@sainsburys.co.uk', (select Id from [site] where storeno = 765));
if exists((select Id from [site] where storeno = 772))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0772pfs@sainsburys.co.uk', (select Id from [site] where storeno = 772));
if exists((select Id from [site] where storeno = 773))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0773pfs@sainsburys.co.uk', (select Id from [site] where storeno = 773));
if exists((select Id from [site] where storeno = 774))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0774pfs@sainsburys.co.uk', (select Id from [site] where storeno = 774));
if exists((select Id from [site] where storeno = 775))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0775pfs@sainsburys.co.uk', (select Id from [site] where storeno = 775));
if exists((select Id from [site] where storeno = 776))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0776pfs@sainsburys.co.uk', (select Id from [site] where storeno = 776));
if exists((select Id from [site] where storeno = 778))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0778pfs@sainsburys.co.uk', (select Id from [site] where storeno = 778));
if exists((select Id from [site] where storeno = 788))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0788pfs@sainsburys.co.uk', (select Id from [site] where storeno = 788));
if exists((select Id from [site] where storeno = 789))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0789pfs@sainsburys.co.uk', (select Id from [site] where storeno = 789));
if exists((select Id from [site] where storeno = 790))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0790pfs@sainsburys.co.uk', (select Id from [site] where storeno = 790));
if exists((select Id from [site] where storeno = 793))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0793pfs@sainsburys.co.uk', (select Id from [site] where storeno = 793));
if exists((select Id from [site] where storeno = 795))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0795pfs@sainsburys.co.uk', (select Id from [site] where storeno = 795));
if exists((select Id from [site] where storeno = 797))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0797pfs@sainsburys.co.uk', (select Id from [site] where storeno = 797));
if exists((select Id from [site] where storeno = 801))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0801pfs@sainsburys.co.uk', (select Id from [site] where storeno = 801));
if exists((select Id from [site] where storeno = 803))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0803pfs@sainsburys.co.uk', (select Id from [site] where storeno = 803));
if exists((select Id from [site] where storeno = 812))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0812pfs@sainsburys.co.uk', (select Id from [site] where storeno = 812));
if exists((select Id from [site] where storeno = 813))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0813pfs@sainsburys.co.uk', (select Id from [site] where storeno = 813));
if exists((select Id from [site] where storeno = 814))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0814pfs@sainsburys.co.uk', (select Id from [site] where storeno = 814));
if exists((select Id from [site] where storeno = 815))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0815pfs@sainsburys.co.uk', (select Id from [site] where storeno = 815));
if exists((select Id from [site] where storeno = 816))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0816pfs@sainsburys.co.uk', (select Id from [site] where storeno = 816));
if exists((select Id from [site] where storeno = 820))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0820pfs@sainsburys.co.uk', (select Id from [site] where storeno = 820));
if exists((select Id from [site] where storeno = 823))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0823pfs@sainsburys.co.uk', (select Id from [site] where storeno = 823));
if exists((select Id from [site] where storeno = 824))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0824pfs@sainsburys.co.uk', (select Id from [site] where storeno = 824));
if exists((select Id from [site] where storeno = 825))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0825pfs@sainsburys.co.uk', (select Id from [site] where storeno = 825));
if exists((select Id from [site] where storeno = 832))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0832pfs@sainsburys.co.uk', (select Id from [site] where storeno = 832));
if exists((select Id from [site] where storeno = 847))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0847pfs@sainsburys.co.uk', (select Id from [site] where storeno = 847));
if exists((select Id from [site] where storeno = 849))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0849pfs@sainsburys.co.uk', (select Id from [site] where storeno = 849));
if exists((select Id from [site] where storeno = 851))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0851pfs@sainsburys.co.uk', (select Id from [site] where storeno = 851));
if exists((select Id from [site] where storeno = 852))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0852pfs@sainsburys.co.uk', (select Id from [site] where storeno = 852));
if exists((select Id from [site] where storeno = 853))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0853pfs@sainsburys.co.uk', (select Id from [site] where storeno = 853));
if exists((select Id from [site] where storeno = 854))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0854pfs@sainsburys.co.uk', (select Id from [site] where storeno = 854));
if exists((select Id from [site] where storeno = 859))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0859pfs@sainsburys.co.uk', (select Id from [site] where storeno = 859));
if exists((select Id from [site] where storeno = 864))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0864pfs@sainsburys.co.uk', (select Id from [site] where storeno = 864));
if exists((select Id from [site] where storeno = 867))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0867pfs@sainsburys.co.uk', (select Id from [site] where storeno = 867));
if exists((select Id from [site] where storeno = 869))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0869pfs@sainsburys.co.uk', (select Id from [site] where storeno = 869));
if exists((select Id from [site] where storeno = 871))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0871pfs@sainsburys.co.uk', (select Id from [site] where storeno = 871));
if exists((select Id from [site] where storeno = 873))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0873pfs@sainsburys.co.uk', (select Id from [site] where storeno = 873));
if exists((select Id from [site] where storeno = 882))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0882pfs@sainsburys.co.uk', (select Id from [site] where storeno = 882));
if exists((select Id from [site] where storeno = 885))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0885pfs@sainsburys.co.uk', (select Id from [site] where storeno = 885));
if exists((select Id from [site] where storeno = 886))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0886pfs@sainsburys.co.uk', (select Id from [site] where storeno = 886));
if exists((select Id from [site] where storeno = 887))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0887pfs@sainsburys.co.uk', (select Id from [site] where storeno = 887));
if exists((select Id from [site] where storeno = 890))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0890pfs@sainsburys.co.uk', (select Id from [site] where storeno = 890));
if exists((select Id from [site] where storeno = 892))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0892pfs@sainsburys.co.uk', (select Id from [site] where storeno = 892));
if exists((select Id from [site] where storeno = 893))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0893pfs@sainsburys.co.uk', (select Id from [site] where storeno = 893));
if exists((select Id from [site] where storeno = 894))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0894pfs@sainsburys.co.uk', (select Id from [site] where storeno = 894));
if exists((select Id from [site] where storeno = 897))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0897pfs@sainsburys.co.uk', (select Id from [site] where storeno = 897));
if exists((select Id from [site] where storeno = 2001))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2001pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2001));
if exists((select Id from [site] where storeno = 2002))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2002pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2002));
if exists((select Id from [site] where storeno = 2005))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2005pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2005));
if exists((select Id from [site] where storeno = 2007))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2007pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2007));
if exists((select Id from [site] where storeno = 2010))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2010pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2010));
if exists((select Id from [site] where storeno = 2011))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2011pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2011));
if exists((select Id from [site] where storeno = 2013))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2013pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2013));
if exists((select Id from [site] where storeno = 2017))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2017pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2017));
if exists((select Id from [site] where storeno = 2022))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2022pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2022));
if exists((select Id from [site] where storeno = 2023))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2023pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2023));
if exists((select Id from [site] where storeno = 2625))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2625pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2625));
if exists((select Id from [site] where storeno = 2231))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2231pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2231));
if exists((select Id from [site] where storeno = 2040))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2040pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2040));
if exists((select Id from [site] where storeno = 2046))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2046pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2046));
if exists((select Id from [site] where storeno = 2054))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2054pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2054));
if exists((select Id from [site] where storeno = 2059))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2059pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2059));
if exists((select Id from [site] where storeno = 2061))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2061pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2061));
if exists((select Id from [site] where storeno = 2063))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2063pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2063));
if exists((select Id from [site] where storeno = 2067))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2067pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2067));
if exists((select Id from [site] where storeno = 2071))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2071pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2071));
if exists((select Id from [site] where storeno = 2073))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2073pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2073));
if exists((select Id from [site] where storeno = 2078))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2078pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2078));
if exists((select Id from [site] where storeno = 2079))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2079pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2079));
if exists((select Id from [site] where storeno = 2080))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2080pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2080));
if exists((select Id from [site] where storeno = 2081))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2081pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2081));
if exists((select Id from [site] where storeno = 2092))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2092pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2092));
if exists((select Id from [site] where storeno = 2100))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2100pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2100));
if exists((select Id from [site] where storeno = 2105))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2105pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2105));
if exists((select Id from [site] where storeno = 2106))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2106pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2106));
if exists((select Id from [site] where storeno = 2112))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('PFS.MeadowHallN@sainsburys.co.uk', (select Id from [site] where storeno = 2112));
if exists((select Id from [site] where storeno = 4513))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('4513m@sainsburys.co.uk', (select Id from [site] where storeno = 4513));
if exists((select Id from [site] where storeno = 2113))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2113pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2113));
if exists((select Id from [site] where storeno = 2114))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2114pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2114));
if exists((select Id from [site] where storeno = 2134))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2134pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2134));
if exists((select Id from [site] where storeno = 2136))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2136pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2136));
if exists((select Id from [site] where storeno = 2180))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2180pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2180));
if exists((select Id from [site] where storeno = 2244))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2244pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2244));
if exists((select Id from [site] where storeno = 2835))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2835pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2835));
if exists((select Id from [site] where storeno = 4513))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb4513r@sainsburys.co.uk', (select Id from [site] where storeno = 4513));
if exists((select Id from [site] where storeno = 2221))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2221pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2221));
if exists((select Id from [site] where storeno = 2221))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2221r@sainsburys.co.uk', (select Id from [site] where storeno = 2221));
if exists((select Id from [site] where storeno = 2221))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2221m@sainsburys.co.uk', (select Id from [site] where storeno = 2221));
if exists((select Id from [site] where storeno = 2181))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2181pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2181));
if exists((select Id from [site] where storeno = 2181))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2181r@sainsburys.co.uk', (select Id from [site] where storeno = 2181));
if exists((select Id from [site] where storeno = 2181))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2181m@sainsburys.co.uk', (select Id from [site] where storeno = 2181));
if exists((select Id from [site] where storeno = 2225))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2225pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2225));
if exists((select Id from [site] where storeno = 2225))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2225r@sainsburys.co.uk', (select Id from [site] where storeno = 2225));
if exists((select Id from [site] where storeno = 2225))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2225m@sainsburys.co.uk', (select Id from [site] where storeno = 2225));
if exists((select Id from [site] where storeno = 2154))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2154pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2154));
if exists((select Id from [site] where storeno = 2154))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2154r@sainsburys.co.uk', (select Id from [site] where storeno = 2154));
if exists((select Id from [site] where storeno = 2154))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2154m@sainsburys.co.uk', (select Id from [site] where storeno = 2154));
if exists((select Id from [site] where storeno = 2186))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('pfs.strathaven@sainsburys.co.uk', (select Id from [site] where storeno = 2186));
if exists((select Id from [site] where storeno = 2186))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2186r@sainsburys.co.uk', (select Id from [site] where storeno = 2186));
if exists((select Id from [site] where storeno = 2186))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2186m@sainsburys.co.uk', (select Id from [site] where storeno = 2186));
if exists((select Id from [site] where storeno = 2093))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2093pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2093));
if exists((select Id from [site] where storeno = 2093))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2093r@sainsburys.co.uk', (select Id from [site] where storeno = 2093));
if exists((select Id from [site] where storeno = 2093))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2093m@sainsburys.co.uk', (select Id from [site] where storeno = 2093));
if exists((select Id from [site] where storeno = 2162))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2162pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2162));
if exists((select Id from [site] where storeno = 2162))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2162r@sainsburys.co.uk', (select Id from [site] where storeno = 2162));
if exists((select Id from [site] where storeno = 2162))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2162m@sainsburys.co.uk', (select Id from [site] where storeno = 2162));
if exists((select Id from [site] where storeno = 2199))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2199pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2199));
if exists((select Id from [site] where storeno = 2199))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2199r@sainsburys.co.uk', (select Id from [site] where storeno = 2199));
if exists((select Id from [site] where storeno = 2199))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2199m@sainsburys.co.uk', (select Id from [site] where storeno = 2199));
if exists((select Id from [site] where storeno = 850))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0850pfs@sainsburys.co.uk', (select Id from [site] where storeno = 850));
if exists((select Id from [site] where storeno = 850))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0850r@sainsburys.co.uk', (select Id from [site] where storeno = 850));
if exists((select Id from [site] where storeno = 850))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0850m@sainsburys.co.uk', (select Id from [site] where storeno = 850));
if exists((select Id from [site] where storeno = 2169))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2169pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2169));
if exists((select Id from [site] where storeno = 2169))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2169r@sainsburys.co.uk', (select Id from [site] where storeno = 2169));
if exists((select Id from [site] where storeno = 2169))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2169m@sainsburys.co.uk', (select Id from [site] where storeno = 2169));
if exists((select Id from [site] where storeno = 2097))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2097pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2097));
if exists((select Id from [site] where storeno = 2097))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2097r@sainsburys.co.uk', (select Id from [site] where storeno = 2097));
if exists((select Id from [site] where storeno = 2097))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2097m@sainsburys.co.uk', (select Id from [site] where storeno = 2097));
if exists((select Id from [site] where storeno = 2247))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2247pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2247));
if exists((select Id from [site] where storeno = 2247))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2247r@sainsburys.co.uk', (select Id from [site] where storeno = 2247));
if exists((select Id from [site] where storeno = 2247))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2247m@sainsburys.co.uk', (select Id from [site] where storeno = 2247));
if exists((select Id from [site] where storeno = 2249))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2249pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2249));
if exists((select Id from [site] where storeno = 2249))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2249r@sainsburys.co.uk', (select Id from [site] where storeno = 2249));
if exists((select Id from [site] where storeno = 2249))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2249m@sainsburys.co.uk', (select Id from [site] where storeno = 2249));
if exists((select Id from [site] where storeno = 2196))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2196pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2196));
if exists((select Id from [site] where storeno = 2196))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2196r@sainsburys.co.uk', (select Id from [site] where storeno = 2196));
if exists((select Id from [site] where storeno = 2196))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2196m@sainsburys.co.uk', (select Id from [site] where storeno = 2196));
if exists((select Id from [site] where storeno = 2288))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2288m@sainsburys.co.uk', (select Id from [site] where storeno = 2288));
if exists((select Id from [site] where storeno = 2288))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2288r@sainsburys.co.uk', (select Id from [site] where storeno = 2288));
if exists((select Id from [site] where storeno = 2288))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2288pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2288));
if exists((select Id from [site] where storeno = 2268))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2268m@sainsburys.co.uk', (select Id from [site] where storeno = 2268));
if exists((select Id from [site] where storeno = 2268))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2268r@sainsburys.co.uk', (select Id from [site] where storeno = 2268));
if exists((select Id from [site] where storeno = 2268))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2268pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2268));
if exists((select Id from [site] where storeno = 2248))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2248m@sainsburys.co.uk', (select Id from [site] where storeno = 2248));
if exists((select Id from [site] where storeno = 2248))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2248r@sainsburys.co.uk', (select Id from [site] where storeno = 2248));
if exists((select Id from [site] where storeno = 2248))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2248pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2248));
if exists((select Id from [site] where storeno = 2255))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2255m@sainsburys.co.uk', (select Id from [site] where storeno = 2255));
if exists((select Id from [site] where storeno = 2255))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2255r@sainsburys.co.uk', (select Id from [site] where storeno = 2255));
if exists((select Id from [site] where storeno = 2255))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2255pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2255));
if exists((select Id from [site] where storeno = 2274))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2274pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2274));
if exists((select Id from [site] where storeno = 2274))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2274r@sainsburys.co.uk', (select Id from [site] where storeno = 2274));
if exists((select Id from [site] where storeno = 2274))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2274m@sainsburys.co.uk', (select Id from [site] where storeno = 2274));
if exists((select Id from [site] where storeno = 2200))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2200pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2200));
if exists((select Id from [site] where storeno = 2200))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2200r@sainsburys.co.uk', (select Id from [site] where storeno = 2200));
if exists((select Id from [site] where storeno = 2200))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2200m@sainsburys.co.uk', (select Id from [site] where storeno = 2200));
if exists((select Id from [site] where storeno = 655))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0655pfs@sainsburys.co.uk', (select Id from [site] where storeno = 655));
if exists((select Id from [site] where storeno = 655))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0655r@sainsburys.co.uk', (select Id from [site] where storeno = 655));
if exists((select Id from [site] where storeno = 655))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0655m@sainsburys.co.uk', (select Id from [site] where storeno = 655));
if exists((select Id from [site] where storeno = 2220))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2220pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2220));
if exists((select Id from [site] where storeno = 2220))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2220r@sainsburys.co.uk', (select Id from [site] where storeno = 2220));
if exists((select Id from [site] where storeno = 2220))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2220m@sainsburys.co.uk', (select Id from [site] where storeno = 2220));
if exists((select Id from [site] where storeno = 2082))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2082pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2082));
if exists((select Id from [site] where storeno = 2082))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2082r@sainsburys.co.uk', (select Id from [site] where storeno = 2082));
if exists((select Id from [site] where storeno = 2082))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2082m@sainsburys.co.uk', (select Id from [site] where storeno = 2082));
if exists((select Id from [site] where storeno = 2240))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2240pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2240));
if exists((select Id from [site] where storeno = 2240))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2240r@sainsburys.co.uk', (select Id from [site] where storeno = 2240));
if exists((select Id from [site] where storeno = 2240))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2240m@sainsburys.co.uk', (select Id from [site] where storeno = 2240));
if exists((select Id from [site] where storeno = 2269))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2269m@sainsburys.co.uk', (select Id from [site] where storeno = 2269));
if exists((select Id from [site] where storeno = 2269))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2269r@sainsburys.co.uk', (select Id from [site] where storeno = 2269));
if exists((select Id from [site] where storeno = 2269))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2269pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2269));
if exists((select Id from [site] where storeno = 2272))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2272m@sainsburys.co.uk', (select Id from [site] where storeno = 2272));
if exists((select Id from [site] where storeno = 2272))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2272pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2272));
if exists((select Id from [site] where storeno = 2272))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2272r@sainsburys.co.uk', (select Id from [site] where storeno = 2272));
if exists((select Id from [site] where storeno = 2286))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2286m@sainsburys.co.uk', (select Id from [site] where storeno = 2286));
if exists((select Id from [site] where storeno = 2286))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2286pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2286));
if exists((select Id from [site] where storeno = 2286))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2286r@sainsburys.co.uk', (select Id from [site] where storeno = 2286));
if exists((select Id from [site] where storeno = 2077))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2077m@sainsburys.co.uk', (select Id from [site] where storeno = 2077));
if exists((select Id from [site] where storeno = 2077))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2077pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2077));
if exists((select Id from [site] where storeno = 2077))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2077r@sainsburys.co.uk', (select Id from [site] where storeno = 2077));
if exists((select Id from [site] where storeno = 2290))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2290m@sainsburys.co.uk', (select Id from [site] where storeno = 2290));
if exists((select Id from [site] where storeno = 2290))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2290pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2290));
if exists((select Id from [site] where storeno = 2290))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2290r@sainsburys.co.uk', (select Id from [site] where storeno = 2290));
if exists((select Id from [site] where storeno = 2293))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2293m@sainsburys.co.uk', (select Id from [site] where storeno = 2293));
if exists((select Id from [site] where storeno = 2293))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2293pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2293));
if exists((select Id from [site] where storeno = 2293))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2293r@sainsburys.co.uk', (select Id from [site] where storeno = 2293));
if exists((select Id from [site] where storeno = 2304))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2304m@sainsburys.co.uk', (select Id from [site] where storeno = 2304));
if exists((select Id from [site] where storeno = 2304))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2304pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2304));
if exists((select Id from [site] where storeno = 2304))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2304r@sainsburys.co.uk', (select Id from [site] where storeno = 2304));
if exists((select Id from [site] where storeno = 2281))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2281pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2281));
if exists((select Id from [site] where storeno = 2281))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2281r@sainsburys.co.uk', (select Id from [site] where storeno = 2281));
if exists((select Id from [site] where storeno = 2281))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2281m@sainsburys.co.uk', (select Id from [site] where storeno = 2281));
if exists((select Id from [site] where storeno = 507))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0507pfs@sainsburys.co.uk', (select Id from [site] where storeno = 507));
if exists((select Id from [site] where storeno = 507))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb0507r@sainsburys.co.uk', (select Id from [site] where storeno = 507));
if exists((select Id from [site] where storeno = 507))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0507m@sainsburys.co.uk', (select Id from [site] where storeno = 507));
if exists((select Id from [site] where storeno = 2239))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2239pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2239));
if exists((select Id from [site] where storeno = 2239))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2239r@sainsburys.co.uk', (select Id from [site] where storeno = 2239));
if exists((select Id from [site] where storeno = 2239))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2239m@sainsburys.co.uk', (select Id from [site] where storeno = 2239));
if exists((select Id from [site] where storeno = 2297))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2297pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2297));
if exists((select Id from [site] where storeno = 2297))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2297r@sainsburys.co.uk', (select Id from [site] where storeno = 2297));
if exists((select Id from [site] where storeno = 2297))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2297m@sainsburys.co.uk', (select Id from [site] where storeno = 2297));
if exists((select Id from [site] where storeno = 2303))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2303pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2303));
if exists((select Id from [site] where storeno = 2303))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2303r@sainsburys.co.uk', (select Id from [site] where storeno = 2303));
if exists((select Id from [site] where storeno = 2303))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2303m@sainsburys.co.uk', (select Id from [site] where storeno = 2303));
if exists((select Id from [site] where storeno = 4524))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb4524r@sainsburys.co.uk', (select Id from [site] where storeno = 4524));
if exists((select Id from [site] where storeno = 4524))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('4524pfs@sainsburys.co.uk', (select Id from [site] where storeno = 4524));
if exists((select Id from [site] where storeno = 4524))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('4524m@sainsburys.co.uk', (select Id from [site] where storeno = 4524));
if exists((select Id from [site] where storeno = 2123))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2123pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2123));
if exists((select Id from [site] where storeno = 2123))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2123r@sainsburys.co.uk', (select Id from [site] where storeno = 2123));
if exists((select Id from [site] where storeno = 2123))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2123m@sainsburys.co.uk', (select Id from [site] where storeno = 2123));
if exists((select Id from [site] where storeno = 805))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2313pfs@sainsburys.co.uk', (select Id from [site] where storeno = 805));
if exists((select Id from [site] where storeno = 805))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2313r@sainsburys.co.uk', (select Id from [site] where storeno = 805));
if exists((select Id from [site] where storeno = 805))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2313m@sainsburys.co.uk', (select Id from [site] where storeno = 805));
if exists((select Id from [site] where storeno = 2309))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2309pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2309));
if exists((select Id from [site] where storeno = 2309))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2309r@sainsburys.co.uk', (select Id from [site] where storeno = 2309));
if exists((select Id from [site] where storeno = 2309))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2309m@sainsburys.co.uk', (select Id from [site] where storeno = 2309));
if exists((select Id from [site] where storeno = 2308))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2308pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2308));
if exists((select Id from [site] where storeno = 2308))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2308r@sainsburys.co.uk', (select Id from [site] where storeno = 2308));
if exists((select Id from [site] where storeno = 2308))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2308m@sainsburys.co.uk', (select Id from [site] where storeno = 2308));
if exists((select Id from [site] where storeno = 2283))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2283pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2283));
if exists((select Id from [site] where storeno = 2283))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2283r@sainsburys.co.uk', (select Id from [site] where storeno = 2283));
if exists((select Id from [site] where storeno = 2283))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2283m@sainsburys.co.uk', (select Id from [site] where storeno = 2283));
if exists((select Id from [site] where storeno = 2314))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2314pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2314));
if exists((select Id from [site] where storeno = 2314))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2314r@sainsburys.co.uk', (select Id from [site] where storeno = 2314));
if exists((select Id from [site] where storeno = 2314))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2314m@sainsburys.co.uk', (select Id from [site] where storeno = 2314));
if exists((select Id from [site] where storeno = 2267))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2267pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2267));
if exists((select Id from [site] where storeno = 2267))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2267r@sainsburys.co.uk', (select Id from [site] where storeno = 2267));
if exists((select Id from [site] where storeno = 2267))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2267m@sainsburys.co.uk', (select Id from [site] where storeno = 2267));
if exists((select Id from [site] where storeno = 4526))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('4526pfs@sainsburys.co.uk', (select Id from [site] where storeno = 4526));
if exists((select Id from [site] where storeno = 4526))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb4526r@sainsburys.co.uk', (select Id from [site] where storeno = 4526));
if exists((select Id from [site] where storeno = 4526))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('4526m@sainsburys.co.uk', (select Id from [site] where storeno = 4526));
if exists((select Id from [site] where storeno = 4525))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('4525pfs@sainsburys.co.uk', (select Id from [site] where storeno = 4525));
if exists((select Id from [site] where storeno = 4525))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb4525r@sainsburys.co.uk', (select Id from [site] where storeno = 4525));
if exists((select Id from [site] where storeno = 4525))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('4525m@sainsburys.co.uk', (select Id from [site] where storeno = 4525));
if exists((select Id from [site] where storeno = 4527))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('4527pfs@sainsburys.co.uk', (select Id from [site] where storeno = 4527));
if exists((select Id from [site] where storeno = 4527))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb4527r@sainsburys.co.uk', (select Id from [site] where storeno = 4527));
if exists((select Id from [site] where storeno = 4527))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('4527m@sainsburys.co.uk', (select Id from [site] where storeno = 4527));
if exists((select Id from [site] where storeno = 2319))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2319pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2319));
if exists((select Id from [site] where storeno = 2319))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2319r@sainsburys.co.uk', (select Id from [site] where storeno = 2319));
if exists((select Id from [site] where storeno = 2319))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2319m@sainsburys.co.uk', (select Id from [site] where storeno = 2319));
if exists((select Id from [site] where storeno = 4528))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('4528pfs@sainsburys.co.uk', (select Id from [site] where storeno = 4528));
if exists((select Id from [site] where storeno = 4528))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb4528r@sainsburys.co.uk', (select Id from [site] where storeno = 4528));
if exists((select Id from [site] where storeno = 4528))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('4528m@sainsburys.co.uk', (select Id from [site] where storeno = 4528));
if exists((select Id from [site] where storeno = 4529))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('4529pfs@sainsburys.co.uk', (select Id from [site] where storeno = 4529));
if exists((select Id from [site] where storeno = 4529))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb4529r@sainsburys.co.uk', (select Id from [site] where storeno = 4529));
if exists((select Id from [site] where storeno = 4529))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('4529m@sainsburys.co.uk', (select Id from [site] where storeno = 4529));
if exists((select Id from [site] where storeno = 2340))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2340pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2340));
if exists((select Id from [site] where storeno = 2340))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2340r@sainsburys.co.uk', (select Id from [site] where storeno = 2340));
if exists((select Id from [site] where storeno = 2340))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2340m@sainsburys.co.uk', (select Id from [site] where storeno = 2340));
if exists((select Id from [site] where storeno = 2095))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2095pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2095));
if exists((select Id from [site] where storeno = 2095))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2095r@sainsburys.co.uk', (select Id from [site] where storeno = 2095));
if exists((select Id from [site] where storeno = 2095))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2095m@sainsburys.co.uk', (select Id from [site] where storeno = 2095));
if exists((select Id from [site] where storeno = 2252))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2252pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2252));
if exists((select Id from [site] where storeno = 2252))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2252r@sainsburys.co.uk', (select Id from [site] where storeno = 2252));
if exists((select Id from [site] where storeno = 2252))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2252m@sainsburys.co.uk', (select Id from [site] where storeno = 2252));
if exists((select Id from [site] where storeno = 4589))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('4589pfs@sainsburys.co.uk', (select Id from [site] where storeno = 4589));
if exists((select Id from [site] where storeno = 4589))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb4589r@sainsburys.co.uk', (select Id from [site] where storeno = 4589));
if exists((select Id from [site] where storeno = 4589))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('4589m@sainsburys.co.uk', (select Id from [site] where storeno = 4589));
if exists((select Id from [site] where storeno = 2289))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2289pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2289));
if exists((select Id from [site] where storeno = 2289))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2289r@sainsburys.co.uk', (select Id from [site] where storeno = 2289));
if exists((select Id from [site] where storeno = 2289))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2289m@sainsburys.co.uk', (select Id from [site] where storeno = 2289));
if exists((select Id from [site] where storeno = 2070))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2070pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2070));
if exists((select Id from [site] where storeno = 2070))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2070r@sainsburys.co.uk', (select Id from [site] where storeno = 2070));
if exists((select Id from [site] where storeno = 2070))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2070m@sainsburys.co.uk', (select Id from [site] where storeno = 2070));
if exists((select Id from [site] where storeno = 815))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('0815cmg@sainsburys.co.uk', (select Id from [site] where storeno = 815));
if exists((select Id from [site] where storeno = 2030))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2030pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2030));
if exists((select Id from [site] where storeno = 2030))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2030r@sainsburys.co.uk', (select Id from [site] where storeno = 2030));
if exists((select Id from [site] where storeno = 2030))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2030m@sainsburys.co.uk', (select Id from [site] where storeno = 2030));
if exists((select Id from [site] where storeno = 2254))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2254pfs@sainsburys.co.uk', (select Id from [site] where storeno = 2254));
if exists((select Id from [site] where storeno = 2254))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('jb2254r@sainsburys.co.uk', (select Id from [site] where storeno = 2254));
if exists((select Id from [site] where storeno = 2254))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2254m@sainsburys.co.uk', (select Id from [site] where storeno = 2254));
if exists((select Id from [site] where storeno = 2158))
INSERT INTO [dbo].[SiteEmail] ([EmailAddress] ,[SiteId]) VALUES('2158m@sainsburys.co.uk', (select Id from [site] where storeno = 2158));