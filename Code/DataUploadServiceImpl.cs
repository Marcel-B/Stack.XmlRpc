using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using com.b_velop.XmlRpc.BL;
using com.b_velop.XmlRpc.Constants;
using com.b_velop.XmlRpc.Models;
using com.b_velop.XmlRpc.Services.Http;
using GraphQL.Client;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;

namespace com.b_velop.XmlRpc.Code
{
    public class DataUploadServiceImpl : GraphQLService, DataUploadService
    {
        private IMemoryCache _cache;

        public DataUploadServiceImpl(
            IMemoryCache cache,
            TokenService tokenService,
            GraphQLClient graphQLClient,
            ILogger<object> logger) : base(tokenService, graphQLClient, logger)
        {
            _cache = cache;
        }

        public async Task UploadValuesAsync()
        {
            if (!_cache.TryGetValue(b_velop.XmlRpc.Constants.Strings.Values, out HomematicValueList values))
                return;

            if (!_cache.TryGetValue(b_velop.XmlRpc.Constants.Strings.MeasurePoints,
                out Dictionary<string, Guid> measurePoints))
            {
                var response = await PostRequestAsync(XmlRpc.Constants.Query.MeasurePoints);
                var mPoints = response.GetDataFieldAs<IEnumerable<MeasurePoint>>("measurePoints");
                measurePoints = new Dictionary<string, Guid>();
                foreach (var measurePoint in mPoints)
                {
                    measurePoints[measurePoint.ExternId] = measurePoint.Id;
                }
                _cache.Set(XmlRpc.Constants.Strings.MeasurePoints, measurePoints);
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
            if (uploadValues.Count == 0)
                return;

            var result = await PostRequestAsync(
                            Query.CreateMeasureValueBunch,
                            "InsertMeasureValueBunch",
                            new { points = uploadPoints, values = uploadValues });
            _logger.LogInformation($"Uploaded '{uploadValues.Count}' values with status code {result}");

        }
    }
}
