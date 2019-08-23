using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using com.b_velop.XmlRpc.Constants;
using com.b_velop.XmlRpc.Models;
using com.b_velop.XmlRpc.Services.Http;
using GraphQL.Client;
using Microsoft.Extensions.Logging;

namespace com.b_velop.XmlRpc.Code
{
    public class ActiveMeasurePointServiceImpl : GraphQLService, ActiveMeasurePointService
    {
        public ActiveMeasurePointServiceImpl(
            TokenService tokenService,
            GraphQLClient graphQLClient,
            ILogger<object> logger) : base(tokenService, graphQLClient, logger)
        {
        }

        public async Task<IEnumerable<ActiveMeasurePoint>> GetActiveMeasurePointsAsync()
        {
            try
            {
                var response = await PostRequestAsync(Query.ActiveMeasurePoints);
                var data = response.GetDataFieldAs<IEnumerable<ActiveMeasurePoint>>("activeMeasurePoints");
                foreach (var d in data)
                {
                    _logger.LogInformation($"{d.ExternId} == {d.HomematicId}");
                }
                if (data == null)
                    _logger.LogWarning(2221, $"Error occurred while request ActiveMeasurePoints.");

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(2221, ex, $"Error occurred while downloading ActiveMeasurePoints");
                return null;
            }
        }
    }
}
