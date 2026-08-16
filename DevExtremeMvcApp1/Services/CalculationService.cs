using DevExtremeMvcApp1.Data;
using DevExtremeMvcApp1.Models;
using System;
using System.Linq;

namespace DevExtremeMvcApp1.Services
{
    public class CalculationService
    {
        public CalculationOutcome CreatePending(ApplicationDbContext db, CalculationRequest request)
        {
            CalculationOutcome validation = ValidateRequest(request);
            if (!validation.Success)
            {
                return validation;
            }

            string shapeType = NormalizeShapeType(request.ShapeType);
            var result = new CalculationResult
            {
                AppUserId = request.AppUserId,
                ShapeType = shapeType,
                CreatedByUserName = request.CreatedByUserName,
                Param1 = request.Param1,
                Param2 = RequiresParam2(shapeType) ? request.Param2 : null,
                Area = null,
                Volume = null,
                CreatedDate = DateTime.Now,
                CalculationDate = DateTime.Now
            };

            db.CalculationResults.Add(result);
            db.SaveChanges();

            return CalculationOutcome.Ok(result);
        }

        public CalculationOutcome UpdatePending(CalculationResult target, CalculationRequest request)
        {
            if (target == null)
            {
                return CalculationOutcome.Fail("Güncellenecek kayıt bulunamadı.");
            }

            CalculationOutcome validation = ValidateRequest(request);
            if (!validation.Success)
            {
                return validation;
            }

            string shapeType = NormalizeShapeType(request.ShapeType);

            target.ShapeType = shapeType;
            if (request.AppUserId.HasValue)
            {
                target.AppUserId = request.AppUserId;
            }

            if (!string.IsNullOrWhiteSpace(request.CreatedByUserName))
            {
                target.CreatedByUserName = request.CreatedByUserName;
            }

            target.Param1 = request.Param1;
            target.Param2 = RequiresParam2(shapeType) ? request.Param2 : null;
            target.Area = null;
            target.Volume = null;
            target.CalculationDate = DateTime.Now;

            return CalculationOutcome.Ok(target);
        }

        public BatchCalculationResponse CalculateAll(ApplicationDbContext db, string userName = null, int? appUserId = null)
        {
            var calculationDate = DateTime.Now;
            var recordsQuery = db.CalculationResults.AsQueryable();
            if (appUserId.HasValue)
            {
                recordsQuery = recordsQuery.Where(x => x.AppUserId == appUserId.Value
                    || x.CreatedByUserName == userName
                    || x.CreatedByUserName == null
                    || x.CreatedByUserName == "");
            }
            else if (!string.IsNullOrWhiteSpace(userName))
            {
                recordsQuery = recordsQuery.Where(x => x.CreatedByUserName == userName || x.CreatedByUserName == null || x.CreatedByUserName == "");
            }

            var records = recordsQuery.ToList();

            foreach (var record in records)
            {
                var request = new CalculationRequest
                {
                    AppUserId = record.AppUserId,
                    ShapeType = record.ShapeType,
                    CreatedByUserName = record.CreatedByUserName,
                    Param1 = record.Param1,
                    Param2 = record.Param2
                };

                CalculationOutcome outcome = Calculate(request);
                if (!outcome.Success)
                {
                    throw new InvalidOperationException("Id " + record.Id + ": " + outcome.ErrorMessage);
                }

                record.ShapeType = outcome.Result.ShapeType;
                record.AppUserId = outcome.Result.AppUserId ?? record.AppUserId;
                record.Param1 = outcome.Result.Param1;
                record.Param2 = outcome.Result.Param2;
                record.Area = outcome.Result.Area;
                record.Volume = outcome.Result.Volume;
                record.CalculationDate = calculationDate;

                if (!string.IsNullOrWhiteSpace(userName) && string.IsNullOrWhiteSpace(record.CreatedByUserName))
                {
                    record.CreatedByUserName = userName;
                }

                if (appUserId.HasValue && !record.AppUserId.HasValue)
                {
                    record.AppUserId = appUserId;
                }

                if (record.CreatedDate == DateTime.MinValue)
                {
                    record.CreatedDate = DateTime.Now;
                }
            }

            db.SaveChanges();

            return new BatchCalculationResponse
            {
                UpdatedCount = records.Count,
                CalculationDate = calculationDate,
                Message = records.Count + " kayıt hesaplandı ve veritabanı güncellendi."
            };
        }

        public CalculationOutcome CalculateExisting(ApplicationDbContext db, int id, string userName = null, int? appUserId = null)
        {
            CalculationResult record = db.CalculationResults.FirstOrDefault(x => x.Id == id);
            if (record == null)
            {
                return CalculationOutcome.Fail("Hesaplanacak kayıt bulunamadı.");
            }

            if (appUserId.HasValue && record.AppUserId.HasValue && record.AppUserId.Value != appUserId.Value)
            {
                return CalculationOutcome.Fail("Bu kayıt için işlem yetkiniz yok.");
            }

            if (!string.IsNullOrWhiteSpace(userName)
                && (!appUserId.HasValue || !record.AppUserId.HasValue)
                && !string.IsNullOrWhiteSpace(record.CreatedByUserName)
                && !string.Equals(record.CreatedByUserName, userName, StringComparison.OrdinalIgnoreCase))
            {
                return CalculationOutcome.Fail("Bu kayıt için işlem yetkiniz yok.");
            }

            var request = new CalculationRequest
            {
                AppUserId = record.AppUserId ?? appUserId,
                ShapeType = record.ShapeType,
                CreatedByUserName = string.IsNullOrWhiteSpace(record.CreatedByUserName) ? userName : record.CreatedByUserName,
                Param1 = record.Param1,
                Param2 = record.Param2
            };

            CalculationOutcome outcome = Calculate(request);
            if (!outcome.Success)
            {
                return outcome;
            }

            record.ShapeType = outcome.Result.ShapeType;
            record.AppUserId = outcome.Result.AppUserId ?? record.AppUserId;
            record.Param1 = outcome.Result.Param1;
            record.Param2 = outcome.Result.Param2;
            record.Area = outcome.Result.Area;
            record.Volume = outcome.Result.Volume;
            record.CalculationDate = DateTime.Now;

            if (!string.IsNullOrWhiteSpace(userName) && string.IsNullOrWhiteSpace(record.CreatedByUserName))
            {
                record.CreatedByUserName = userName;
            }

            if (appUserId.HasValue && !record.AppUserId.HasValue)
            {
                record.AppUserId = appUserId;
            }

            if (record.CreatedDate == DateTime.MinValue)
            {
                record.CreatedDate = DateTime.Now;
            }

            db.SaveChanges();

            return CalculationOutcome.Ok(record);
        }

        public CalculationOutcome CalculateAndSave(ApplicationDbContext db, CalculationRequest request)
        {
            CalculationOutcome outcome = Calculate(request);
            if (!outcome.Success)
            {
                return outcome;
            }

            db.CalculationResults.Add(outcome.Result);
            db.SaveChanges();

            return outcome;
        }

        public CalculationOutcome Calculate(CalculationRequest request)
        {
            CalculationOutcome validation = ValidateRequest(request);
            if (!validation.Success)
            {
                return validation;
            }

            string shapeType = NormalizeShapeType(request.ShapeType);
            var result = new CalculationResult
            {
                AppUserId = request.AppUserId,
                ShapeType = shapeType,
                CreatedByUserName = request.CreatedByUserName,
                Param1 = request.Param1,
                Param2 = RequiresParam2(shapeType) ? request.Param2 : null,
                CreatedDate = DateTime.Now,
                CalculationDate = DateTime.Now
            };

            switch (shapeType)
            {
                case "Kare":
                    result.Area = request.Param1 * request.Param1;
                    result.Volume = 0;
                    break;

                case "Kup":
                    result.Area = 6 * request.Param1 * request.Param1;
                    result.Volume = request.Param1 * request.Param1 * request.Param1;
                    break;

                case "Daire":
                    result.Area = Math.PI * request.Param1 * request.Param1;
                    result.Volume = 0;
                    break;

                case "Dikdortgen":
                    result.Area = request.Param1 * request.Param2.Value;
                    result.Volume = 0;
                    break;

                case "Ucgen":
                    result.Area = (request.Param1 * request.Param2.Value) / 2;
                    result.Volume = 0;
                    break;

                case "Silindir":
                    result.Area = 2 * Math.PI * request.Param1 * (request.Param1 + request.Param2.Value);
                    result.Volume = Math.PI * request.Param1 * request.Param1 * request.Param2.Value;
                    break;

                case "Kure":
                    result.Area = 4 * Math.PI * request.Param1 * request.Param1;
                    result.Volume = (4.0 / 3.0) * Math.PI * request.Param1 * request.Param1 * request.Param1;
                    break;

                case "Koni":
                    double slantHeight = Math.Sqrt((request.Param1 * request.Param1) + (request.Param2.Value * request.Param2.Value));
                    result.Area = Math.PI * request.Param1 * (request.Param1 + slantHeight);
                    result.Volume = (Math.PI * request.Param1 * request.Param1 * request.Param2.Value) / 3;
                    break;

                default:
                    return CalculationOutcome.Fail("Desteklenmeyen şekil türü.");
            }

            return CalculationOutcome.Ok(result);
        }

        private static CalculationOutcome ValidateRequest(CalculationRequest request)
        {
            if (request == null)
            {
                return CalculationOutcome.Fail("Kayıt bilgisi boş olamaz.");
            }

            string shapeType = NormalizeShapeType(request.ShapeType);
            if (string.IsNullOrWhiteSpace(shapeType))
            {
                return CalculationOutcome.Fail("Geçerli bir şekil seçiniz.");
            }

            if (request.Param1 <= 0)
            {
                return CalculationOutcome.Fail("Ölçü 1 değeri 0'dan büyük olmalıdır.");
            }

            if (RequiresParam2(shapeType) && (!request.Param2.HasValue || request.Param2.Value <= 0))
            {
                return CalculationOutcome.Fail("Bu şekil için Ölçü 2 değeri 0'dan büyük olmalıdır.");
            }

            return CalculationOutcome.Ok(null);
        }

        private static bool RequiresParam2(string shapeType)
        {
            return shapeType == "Ucgen"
                || shapeType == "Silindir"
                || shapeType == "Dikdortgen"
                || shapeType == "Koni";
        }

        private static string NormalizeShapeType(string shapeType)
        {
            string value = (shapeType ?? string.Empty).Trim().ToLowerInvariant();

            switch (value)
            {
                case "kare":
                    return "Kare";

                case "kup":
                case "küp":
                    return "Kup";

                case "daire":
                    return "Daire";

                case "dikdortgen":
                case "dikdörtgen":
                    return "Dikdortgen";

                case "ucgen":
                case "üçgen":
                    return "Ucgen";

                case "silindir":
                    return "Silindir";

                case "kure":
                case "küre":
                    return "Kure";

                case "koni":
                    return "Koni";

                default:
                    return string.Empty;
            }
        }
    }
}
