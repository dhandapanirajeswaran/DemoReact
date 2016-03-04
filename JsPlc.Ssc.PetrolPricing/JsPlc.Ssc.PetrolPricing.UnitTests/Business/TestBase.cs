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
                        UploadDateTime = DateTime.Now,
						StoredFileName = "DailyUpload.txt",
						UploadTypeId = 1
                    }
                };

				return result;
			}
		}

        public Models.Site GetDummyCompetitor(int catNo)
        {
            var result = DummyCompetitor;
            result.CatNo = catNo;
            return result;
        }

		public string TestFileFolderPath
		{
			get
			{
				return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles");
			}
		}
    }
}
