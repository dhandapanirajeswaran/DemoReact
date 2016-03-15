using JsPlc.Ssc.PetrolPricing.Core;
using JsPlc.Ssc.PetrolPricing.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.UnitTests.Business
{
	public class TestBase
	{
		#region Properties
		protected List<Models.SiteToCompetitor> DummySiteToCompetitors
		{
			get
			{
				var result = new List<Models.SiteToCompetitor>();

				result.AddRange(new List<Models.SiteToCompetitor> { 
                    new Models.SiteToCompetitor
                    {
                         SiteId = 1,
                         DriveTime = 3.45f,
                         CompetitorId = 2,
                         Competitor = GetDummyCompetitor(5)
                    },
                     new Models.SiteToCompetitor
                {
                    SiteId = 1,
                    DriveTime = 5.75f,
                    CompetitorId = 3,
                    Competitor = GetDummyCompetitor(3)
                },
                new Models.SiteToCompetitor
                {
                    SiteId = 1,
                    DriveTime = 12.75f,
                    CompetitorId = 4,
                    Competitor = GetDummyCompetitor(7)
                },
                new Models.SiteToCompetitor
                {
                    SiteId = 1,
                    DriveTime = 13.75f,
                    CompetitorId = 9,
                    Competitor = GetDummyCompetitor(8)
                },
                new Models.SiteToCompetitor
                {
                    SiteId = 1,
                    DriveTime = 17.26f,
                    CompetitorId = 5,
                    Competitor = GetDummyCompetitor(4)
                },new Models.SiteToCompetitor
                {
                    SiteId = 1,
                    DriveTime = 21.26f,
                    CompetitorId = 6,
                    Competitor = GetDummyCompetitor(2)
                },new Models.SiteToCompetitor
                {
                    SiteId = 1,
                    DriveTime = 28.26f,
                    CompetitorId = 7,
                    Competitor = GetDummyCompetitor(6)
                },new Models.SiteToCompetitor
                {
                    SiteId = 1,
                    DriveTime = 34.26f,
                    CompetitorId = 8,
                    Competitor = GetDummyCompetitor(7)
                }
                });

				return result;
			}
		}

		public Models.Site DummyCompetitor
		{
			get
			{
				return new Models.Site
				{
					Id = 1,
					CatNo = 1
				};
			}
		}

		public List<Models.DailyPrice> DummyDailyPrices
		{
			get
			{
				var result = new List<Models.DailyPrice>();

				#region FuelTypeId = 1

				result.AddRange(new List<Models.DailyPrice>
                {
                new Models.DailyPrice
                {
                    ModalPrice = 1015,
                    CatNo = 2,
                    FuelTypeId = 1
                },
                new Models.DailyPrice
                {
                    ModalPrice = 1017,
                    CatNo = 3,
                    FuelTypeId = 1
                },
                new Models.DailyPrice
                {
                    ModalPrice = 1027,
                    CatNo = 4,
                    FuelTypeId = 1
                },
                new Models.DailyPrice
                {
                    ModalPrice = 1007,
                    CatNo = 5,
                    FuelTypeId = 1
                },
                new Models.DailyPrice
                {
                    ModalPrice = 1047,
                    CatNo = 6,
                    FuelTypeId = 1
                },
                new Models.DailyPrice
                {
                    ModalPrice = 1037,
                    CatNo = 7,
                    FuelTypeId = 1
                },
                new Models.DailyPrice
                {
                    ModalPrice = 987,
                    CatNo = 8,
                    FuelTypeId = 1
                }
                });

				#endregion

				#region FuelTypeId = 2

				result.AddRange(new List<Models.DailyPrice>
                {
                new Models.DailyPrice
                {
                    ModalPrice = 1015,
                    CatNo = 2,
                    FuelTypeId = 2
                },
                new Models.DailyPrice
                {
                    ModalPrice = 1014,
                    CatNo = 3,
                    FuelTypeId = 2
                },
                new Models.DailyPrice
                {
                    ModalPrice = 1025,
                    CatNo = 4,
                    FuelTypeId = 2
                },
                new Models.DailyPrice
                {
                    ModalPrice = 1007,
                    CatNo = 5,
                    FuelTypeId = 2
                },
                new Models.DailyPrice
                {
                    ModalPrice = 1046,
                    CatNo = 6,
                    FuelTypeId = 2
                },
                new Models.DailyPrice
                {
                    ModalPrice = 1037,
                    CatNo = 7,
                    FuelTypeId = 2
                },
                new Models.DailyPrice
                {
                    ModalPrice = 988,
                    CatNo = 8,
                    FuelTypeId = 2
                }
                });

				#endregion

				return result;
			}
		}

		public List<Models.FuelType> DummyFuelTypes
		{
			get
			{
				var result = new List<Models.FuelType>
                {
                    new Models.FuelType {
                        Id = 1
                    },
                    new Models.FuelType {
                        Id = 2
                    },
                    new Models.FuelType {
                        Id = 6
                    }
                };

				return result;
			}
		}

		public List<Models.FileUpload> DummyFileUploads
		{
			get
			{
				var result = new List<Models.FileUpload>
                {
                    new Models.FileUpload {
						Id = 1,
                        UploadDateTime = DateTime.Today,
						StoredFileName = "DailyUpload.txt",
						UploadTypeId = (int)FileUploadTypes.DailyPriceData
                    },
					new Models.FileUpload {
						Id = 2,
                        UploadDateTime = DateTime.Today,
						StoredFileName = "QuarterlyUpload.xlsx",
						UploadTypeId = (int)FileUploadTypes.QuarterlySiteData
                    }
                };

				return result;
			}
		}

		public List<Models.FileUpload> DummyInvalidFileUploads
		{
			get
			{
				var result = new List<Models.FileUpload>
                {
                    new Models.FileUpload {
						Id = 1,
                        UploadDateTime = DateTime.Today,
						StoredFileName = "InvalidDailyUpload.txt",
						UploadTypeId = (int)FileUploadTypes.DailyPriceData
                    },
					new Models.FileUpload {
						Id = 2,
                        UploadDateTime = DateTime.Today,
						StoredFileName = "InvalidQuarterlyUpload.xlsx",
						UploadTypeId = (int)FileUploadTypes.QuarterlySiteData
                    }
                };

				return result;
			}
		}

		public string TestFileFolderPath
		{
			get
			{
				return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles");
			}
		}

		public string QuarterlyFileDataSheetName
		{
			get
			{
				return "Quarterly TA Analysis V3 2015";
			}
		}

		public List<QuarterlyUploadStaging> DummyQuarterlyUploadStagingRecords
		{
			get
			{
				var result = new List<QuarterlyUploadStaging> { 
				new QuarterlyUploadStaging{
					SainsSiteName = "SAINSBURYS HENDON",
					SainsSiteTown = "LONDON",
					SainsSiteCatNo = 100,
					Rank = 1,
					DriveDist = 1.42f,
					DriveTime = 6.66f,
					CatNo = 26054,
					Brand = "ASDA",
					//to test site update added " 1" to the end
					SiteName = "ASDA COLINDALE AUTOMAT 1", 
					Addr = "CAPITOL WAY",
					Suburb = "COLINDALE",
					Town  = "LONDON",
					PostCode = "NW9 0EW",
					Company = "ASDA STORES PLC",
					Ownership = "HYPERMARKET"
				},
				new QuarterlyUploadStaging{
					SainsSiteName = "SAINSBURYS HENDON",
					SainsSiteTown = "LONDON",
					SainsSiteCatNo = 100,
					Rank = 2,
					DriveDist = 3.57f,
					DriveTime = 10.9f,
					CatNo = 1735,
					Brand = "TESCO",
					SiteName = "TESCO BRENT CROSS HENDON WAY",
					Addr = "TILLING ROAD",
					Suburb = "BRENT CROSS",
					Town  = "LONDON",
					PostCode = "NW2 1LZ",
					Company = "TESCO STORES LTD",
					Ownership = "HYPERMARKET"
				},
				//for updating CatNo
				new QuarterlyUploadStaging {
					SainsSiteName = "SAINSBURYS HAYES",
					SainsSiteTown = "HAYES",
					SainsSiteCatNo = 1291,
					Rank = 24,
					DriveDist = 10.43f,
					DriveTime = 27.47f,
					CatNo = 100,
					Brand = "SAINSBURYS",
					SiteName = "SAINSBURYS HENDON",
					Addr = "HYDE ESTATE ROAD",
					Suburb = "HENDON",
					Town  = "LONDON",
					PostCode = "NW9 6JX",
					Company = "J SAINSBURY PLC",
					Ownership = "HYPERMARKET"
				},
				//for adding new site
				new QuarterlyUploadStaging{
					SainsSiteName = "SAINSBURYS HENDON",
					SainsSiteTown = "LONDON",
					SainsSiteCatNo = 100,
					Rank = 3,
					DriveDist = 2.51f,
					DriveTime = 11.93f,
					CatNo = 108,
					Brand = "MORRISONS",
					SiteName = "MORRISONS QUEENSBURY",
					Addr = "CUMBERLAND ROAD",
					Suburb = "QUEENSBURY",
					Town  = "LONDON",
					PostCode = "NW9 6RN",
					Company = "WM MORRISONS SUPERMARKETS PLC",
					Ownership = "HYPERMARKET"
				}

				};

				return result;
			}
		}

		public List<Site> DummySites
		{
			get
			{
				var result = new List<Site> {
					new Site {
						Id = 1,
						CatNo = 100,
						Brand = "SAINSBURYS",
						SiteName = "SAINSBURYS HENDON",
						Address="HYDE ESTATE ROAD",
						Suburb = "HENDON",
						Town = "LONDON",
						PostCode = "NW9 6JX",
						Company = "J SAINSBURY PLC",
						Ownership = "HYPERMARKET",
						IsSainsburysSite = true,
						IsActive = true
					},
					new Site { //same as above but with null CatNo
						Id = 1,
						CatNo = null,
						Brand = "SAINSBURYS",
						SiteName = "SAINSBURYS HENDON", //should be matching with SainsSiteName in DummyQuarterlyUploadStagingRecords
						Address="HYDE ESTATE ROAD",
						Suburb = "HENDON",
						Town = "LONDON",
						PostCode = "NW9 6JX",
						Company = "J SAINSBURY PLC",
						Ownership = "HYPERMARKET",
						IsSainsburysSite = true,
						IsActive = true
					},
					new Site {
						Id = 2,
						CatNo = 26054,
						Brand = "ASDA",
						SiteName = "ASDA COLINDALE AUTOMAT",
						Address="CAPITOL WAY",
						Suburb = "COLINDALE",
						Town = "LONDON",
						PostCode = "NW9 0EW",
						Company = "ASDA STORES PLC",
						Ownership = "HYPERMARKET",
						IsSainsburysSite = false,
						IsActive = true
					},
					new Site {
						Id = 3,
						CatNo = 1735,
						Brand = "TESCO",
						SiteName = "TESCO BRENT CROSS HENDON WAY",
						Address="TILLING ROAD",
						Suburb = "BRENT CROSS",
						Town = "LONDON",
						PostCode = "NW2 1LZ",
						Company = "TESCO STORES LTD",
						Ownership = "HYPERMARKET",
						IsSainsburysSite = false,
						IsActive = true
					}
				};

				return result;
			}
		}
		#endregion

		#region Methods
		public Models.Site GetDummyCompetitor(int catNo)
		{
			var result = DummyCompetitor;
			result.CatNo = catNo;
			return result;
		}

		protected bool ComparePrimaryFileUploadAttributes(FileUpload testFileToUpload, FileUpload arg)
		{
			return arg.Id == testFileToUpload.Id
									&& arg.StoredFileName == testFileToUpload.StoredFileName
									&& arg.UploadDateTime == testFileToUpload.UploadDateTime
									&& arg.UploadTypeId == testFileToUpload.UploadTypeId;
		}

		#endregion
	}
}
