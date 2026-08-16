using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using DevExtremeMvcApp1.Data;
using DevExtremeMvcApp1.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace DevExtremeMvcApp1.Controllers
{
    [Authorize]
    public class CalculationResultsDataController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    db.Database.Initialize(false);

                    var rows = GetGridRows(db);

                    return Request.CreateResponse(DataSourceLoader.Load(rows, loadOptions));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(
                    HttpStatusCode.InternalServerError,
                    ex.GetBaseException().Message);
            }
        }

        [HttpGet]
        [Route("api/CalculationResultsData/Flat")]
        public HttpResponseMessage Flat()
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    db.Database.Initialize(false);

                    var rows = GetGridRows(db);

                    return Request.CreateResponse(rows);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(
                    HttpStatusCode.InternalServerError,
                    ex.GetBaseException().Message);
            }
        }

        [HttpGet]
        [Route("api/CalculationResultsData/Summary")]
        public HttpResponseMessage Summary()
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    db.Database.Initialize(false);

                    var records = db.CalculationResults
                        .AsNoTracking()
                        .Select(x => new
                        {
                            x.Area,
                            x.Volume
                        })
                        .ToList();

                    int pendingCount = records.Count(x => !x.Area.HasValue || !x.Volume.HasValue);
                    int calculatedCount = records.Count - pendingCount;

                    return Request.CreateResponse(new
                    {
                        TotalCount = records.Count,
                        PendingCount = pendingCount,
                        CalculatedCount = calculatedCount
                    });
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(
                    HttpStatusCode.InternalServerError,
                    ex.GetBaseException().Message);
            }
        }

        [HttpGet]
        [Route("api/CalculationResultsData/ShapeSummary")]
        public HttpResponseMessage ShapeSummary()
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    db.Database.Initialize(false);

                    var records = db.CalculationResults
                        .AsNoTracking()
                        .AsEnumerable()
                        .GroupBy(x => GetDisplayShape(x.ShapeType))
                        .Select(group => new
                        {
                            Shape = group.Key,
                            Count = group.Count(),
                            CalculatedCount = group.Count(x => x.Area.HasValue && x.Volume.HasValue),
                            PendingCount = group.Count(x => !x.Area.HasValue || !x.Volume.HasValue)
                        })
                        .OrderByDescending(x => x.Count)
                        .ThenBy(x => x.Shape)
                        .ToList();

                    return Request.CreateResponse(records);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(
                    HttpStatusCode.InternalServerError,
                    ex.GetBaseException().Message);
            }
        }

        private static string GetDisplayUserName(CalculationResult calculationResult)
        {
            if (!string.IsNullOrWhiteSpace(calculationResult.CreatedByUserName))
            {
                return calculationResult.CreatedByUserName;
            }

            if (calculationResult.AppUser != null && !string.IsNullOrWhiteSpace(calculationResult.AppUser.UserName))
            {
                return calculationResult.AppUser.UserName;
            }

            return "-";
        }

        private static List<CalculationGridRow> GetGridRows(ApplicationDbContext db)
        {
            return db.CalculationResults
                .AsNoTracking()
                .Include(x => x.AppUser)
                .OrderByDescending(x => x.CalculationDate)
                .AsEnumerable()
                .Select(x => new CalculationGridRow
                {
                    Id = x.Id,
                    AppUserId = x.AppUserId,
                    ShapeType = x.ShapeType,
                    Shape = GetDisplayShape(x.ShapeType),
                    CreatedByUserName = GetDisplayUserName(x),
                    Param1 = x.Param1,
                    Param2 = x.Param2,
                    Area = x.Area,
                    Volume = x.Volume,
                    CreatedDate = x.CreatedDate,
                    CreatedDateText = x.CreatedDate.ToString("dd.MM.yyyy HH:mm:ss"),
                    CalculationDate = x.CalculationDate,
                    CalculationDateText = x.CalculationDate.ToString("dd.MM.yyyy HH:mm:ss"),
                    Status = x.Area.HasValue || x.Volume.HasValue ? "Hesaplandı" : "Bekliyor",
                    StatusKey = x.Area.HasValue || x.Volume.HasValue ? "done" : "pending"
                })
                .ToList();
        }

        private static string GetDisplayShape(string shapeType)
        {
            switch (shapeType)
            {
                case "Kare":
                    return "Kare";

                case "Kup":
                    return "Küp";

                case "Daire":
                    return "Daire";

                case "Dikdortgen":
                    return "Dikdörtgen";

                case "Ucgen":
                    return "Üçgen";

                case "Silindir":
                    return "Silindir";

                case "Kure":
                    return "Küre";

                case "Koni":
                    return "Koni";

                default:
                    return string.IsNullOrWhiteSpace(shapeType) ? "-" : shapeType;
            }
        }
    }
}
