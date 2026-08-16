using DevExtremeMvcApp1.Data;
using DevExtremeMvcApp1.Models;
using DevExtremeMvcApp1.Services;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace DevExtremeMvcApp1.Controllers
{
    [RoutePrefix("api/input")]
    public class InputApiController : ApiController
    {
        [HttpPost]
        [Route("submit")]
        public IHttpActionResult Submit(CalculationRequest request)
        {
            if (request == null)
            {
                return BadRequest("Input bilgisi boş olamaz.");
            }

            var preparedRequest = new CalculationRequest
            {
                AppUserId = request.AppUserId,
                ShapeType = (request.ShapeType ?? string.Empty).Trim(),
                CreatedByUserName = request.CreatedByUserName,
                Param1 = request.Param1,
                Param2 = request.Param2
            };

            using (var db = new ApplicationDbContext())
            {
                var service = new CalculationService();
                CalculationOutcome outcome = service.CreatePending(db, preparedRequest);

                if (!outcome.Success)
                {
                    return BadRequest(outcome.ErrorMessage);
                }

                return Ok(CalculationResponse.FromEntity(outcome.Result));
            }
        }

        [HttpPost]
        [Route("calculate-all")]
        public async Task<IHttpActionResult> CalculateAll(BatchCalculationRequest request)
        {
            Uri calculationApiUri = new Uri(Request.RequestUri, "/api/calculation/calculate-all");

            using (var client = new HttpClient())
            {
                HttpResponseMessage response = await client.PostAsJsonAsync(calculationApiUri, request ?? new BatchCalculationRequest());

                if (!response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    return BadRequest(responseBody);
                }

                BatchCalculationResponse result = await response.Content.ReadAsAsync<BatchCalculationResponse>();
                return Ok(result);
            }
        }

        [HttpPost]
        [Route("calculate-record/{id:int}")]
        public async Task<IHttpActionResult> CalculateRecord(int id, BatchCalculationRequest request)
        {
            Uri calculationApiUri = new Uri(Request.RequestUri, "/api/calculation/calculate-record/" + id);

            using (var client = new HttpClient())
            {
                HttpResponseMessage response = await client.PostAsJsonAsync(calculationApiUri, request ?? new BatchCalculationRequest());

                if (!response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    return BadRequest(responseBody);
                }

                CalculationResponse result = await response.Content.ReadAsAsync<CalculationResponse>();
                return Ok(result);
            }
        }
    }
}
