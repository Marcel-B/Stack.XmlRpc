using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using com.b_velop.XmlRpc.BL;
using com.b_velop.XmlRpc.Constants;
using com.b_velop.XmlRpc.Models;
using com.b_velop.XmlRpc.Services.Http;
using GraphQL.Client;
using GraphQL.Common.Response;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace com.b_velop.XmlRpc.Code
{
    public class DataUploadServiceImpl : GraphQLService<DataUploadService>, DataUploadService
    {
        private readonly IMemoryCache _cache;

        public DataUploadServiceImpl(
            IMemoryCache cache,
            TokenService tokenService,
            GraphQLClient graphQLClient,
            ILogger<DataUploadService> logger) : base(tokenService, graphQLClient, logger)
        {
            _cache = cache;
        }

        public async Task<Dictionary<string, Guid>> RequestMeasurePointsAsync()
        {
            var response = await PostRequestAsync(Query.MeasurePoints);

            if (response.Errors != null) // Error while getting MeasurePoints
            {
                HandleGraphQlError($"Error occurred while request MeasurePoints:", response);
                return null;
            }

            var mPoints = response.GetDataFieldAs<IEnumerable<MeasurePoint>>("measurePoints");
            var measurePoints = new Dictionary<string, Guid>();

            foreach (var measurePoint in mPoints)
                measurePoints[measurePoint.ExternId] = measurePoint.Id;

            return measurePoints;
        }

        public async Task UploadValuesAsync()
        {
            if (!_cache.TryGetValue(Strings.Values, out HomematicValueList values))
                return;

            if (!_cache.TryGetValue(Strings.MeasurePoints,
                out Dictionary<string, Guid> measurePoints))
            {
                measurePoints = await RequestMeasurePointsAsync();
                if (measurePoints == null)
                    return;
                _cache.Set(Strings.MeasurePoints, measurePoints);
            }

            var homeValues = values.WithdrawItems();

            var uploadValues = new List<double>();
            var uploadPoints = new List<Guid>();

            foreach (var value in homeValues)
            {
                if (!double.TryParse(value.Value, out var currentValue))
                    continue;

                var point = measurePoints[value.AllId];

                uploadValues.Add(currentValue);
                uploadPoints.Add(point);
            }

            _logger.LogInformation($"'{uploadPoints.Count}' values to upload");

            if (uploadValues.Count == 0)
                return;

            var result = await PostRequestAsync(
                            Query.CreateMeasureValueBunch,
                            "InsertMeasureValueBunch",
                            new { points = uploadPoints, values = uploadValues });

            if (result != null)
            {
                _logger.LogInformation($"Uploaded '{uploadValues.Count}' values.");
                return;
            }

            _logger.LogWarning(2222, $"Error occurred while uploading '{uploadValues.Count}' values. Add back to Collection.");
            values.AddRange(homeValues);
        }
    }
}
