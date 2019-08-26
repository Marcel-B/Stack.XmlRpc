using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using com.b_velop.XmlRpc.Constants;
using com.b_velop.XmlRpc.Models;
using com.b_velop.XmlRpc.Services.Http;
using GraphQL.Client;
using Microsoft.Extensions.Logging;

namespace com.b_velop.XmlRpc.Code
{
    public class ActiveMeasurePointServiceImpl : GraphQLService<ActiveMeasurePointService>, ActiveMeasurePointService
    {
        public ActiveMeasurePointServiceImpl(
            TokenService tokenService,
            GraphQLClient graphQLClient,
            ILogger<ActiveMeasurePointService> logger) : base(tokenService, graphQLClient, logger)
        {
        }

        public async Task<IEnumerable<ActiveMeasurePoint>> GetActiveMeasurePointsAsync()
        {
            try
            {
                var response = await PostRequestAsync(Query.ActiveMeasurePoints);
                if (response == null)
                {
                    _logger.LogError(2221, $"Error occured while request ActiveMeasurePoints. response == null");
                    return null;
                }
                else if(response.Errors != null)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine($"Error occured while request ActiveMeasurePoints");
                    sb.AppendLine();
                    foreach (var error in response.Errors)
                    {
                        sb.AppendLine(error.Message);
                    }
                    _logger.LogError(2221, sb.ToString());
                    return null;
                }

                var activeMeasurPoints = response.GetDataFieldAs<IEnumerable<ActiveMeasurePoint>>("activeMeasurePoints");

                if (activeMeasurPoints == null)
                    _logger.LogWarning(2221, $"Error occurred while request ActiveMeasurePoints.");

                return activeMeasurPoints;
            }
            catch (Exception ex)
            {
                _logger.LogError(2221, ex, $"Error occurred while downloading ActiveMeasurePoints");
                return null;
            }
        }
    }
}
