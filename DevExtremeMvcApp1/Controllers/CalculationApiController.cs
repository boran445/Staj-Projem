using DevExtremeMvcApp1.Data;
using DevExtremeMvcApp1.Models;
using DevExtremeMvcApp1.Services;
using System;
using System.Web.Http;

namespace DevExtremeMvcApp1.Controllers
{
    [RoutePrefix("api/calculation")]
    public class CalculationApiController : ApiController
    {
        [HttpPost]
        [Route("calculate")]
        public IHttpActionResult Calculate(CalculationRequest request)
        {
            using (var db = new ApplicationDbContext())
            {
                var service = new CalculationService();
                CalculationOutcome outcome = service.CalculateAndSave(db, request);

                if (!outcome.Success)
                {
                    return BadRequest(outcome.ErrorMessage);
                }

                return Ok(CalculationResponse.FromEntity(outcome.Result));
            }
        }

        [HttpPost]
        [Route("calculate-all")]
        public IHttpActionResult CalculateAll(BatchCalculationRequest request)
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var service = new CalculationService();
                    BatchCalculationResponse response = service.CalculateAll(
                        db,
                        request == null ? null : request.UserName,
                        request == null ? null : request.AppUserId);
                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.GetBaseException().Message);
            }
        }

        [HttpPost]
        [Route("calculate-record/{id:int}")]
        public IHttpActionResult CalculateRecord(int id, BatchCalculationRequest request)
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var service = new CalculationService();
                    CalculationOutcome outcome = service.CalculateExisting(
                        db,
                        id,
                        request == null ? null : request.UserName,
                        request == null ? null : request.AppUserId);

                    if (!outcome.Success)
                    {
                        return BadRequest(outcome.ErrorMessage);
                    }

                    return Ok(CalculationResponse.FromEntity(outcome.Result));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.GetBaseException().Message);
            }
        }
    }
}
