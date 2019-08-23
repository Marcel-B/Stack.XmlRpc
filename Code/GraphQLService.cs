using System;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using com.b_velop.XmlRpc.Models;
using com.b_velop.XmlRpc.Services.Http;
using GraphQL.Client;
using GraphQL.Common.Request;
using GraphQL.Common.Response;
using Microsoft.Extensions.Logging;

namespace com.b_velop.XmlRpc.Code
{
    public abstract class GraphQLService<T>
    {
        protected GraphQLClient GraphQLClient { get; }
        private Secrets _secrets { get; }
        protected TokenService _tokenService { get; }
        protected ILogger<T> _logger;

        protected GraphQLService(
            TokenService tokenService,
            GraphQLClient graphQLClient,
            ILogger<T> logger)
        {
            _tokenService = tokenService;
            _logger = logger;
            GraphQLClient = graphQLClient;
        }

        protected async Task<GraphQLResponse> PostRequestAsync(
            string query)
        {
            var token = await _tokenService.RequestTokenAsync();
            SetHeader(token);

            try
            {
                var graphQLRequest = new GraphQLRequest()
                {
                    Query = query
                };

                var result = await GraphQLClient.PostAsync(graphQLRequest);

                if (result.Errors != null)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var error in result.Errors)
                    {
                        sb.AppendLine(error.Message);
                    }

                    sb.AppendLine();
                    sb.AppendLine();

                    _logger.LogWarning(2232,
                        $"Error occurred while upload HomeMatic values with GraphQL: '{sb.ToString()}'"); // '{(int)result.StatusCode}: {result.ReasonPhrase}'");
                    return null;
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(2232, ex,
                    $"Error occurred while uploading HomeMatic values with GraphQL"); // '{content}'.");
                return null;
            }
        }


        protected async Task<int> PostRequestAsync(
                string query,
                string operationName,
                dynamic variables)
        {
            var token = await _tokenService.RequestTokenAsync();
            SetHeader(token);

            try
            {
                var graphQLRequest = new GraphQLRequest()
                {
                    Query = query,
                    OperationName = operationName,
                    Variables = variables
                };

                var result = await GraphQLClient.PostAsync(graphQLRequest);
                // TODO - Loger Error to Logstash
                //var result = await _client.PostAsync("/api/values", jsonContent, CancellationToken.None);
                if (result.Errors != null)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var error in result.Errors)
                    {
                        sb.AppendLine(error.Message);
                    }
                    sb.AppendLine();
                    sb.AppendLine();

                    _logger.LogWarning(2232, $"Error occurred while upload HomeMatic values with GraphQL: '{sb.ToString()}'");// '{(int)result.StatusCode}: {result.ReasonPhrase}'");
                    return (int)500;//result.StatusCode;
                }
                return 200;
            }
            catch (Exception ex)
            {
                _logger.LogError(2232, ex, $"Error occurred while uploading HomeMatic values with GraphQL");// '{content}'.");
                return 500;
            }
        }

        protected bool SetHeader(
            Token token)
        {
            GraphQLClient.DefaultRequestHeaders.Clear();
            GraphQLClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            return true;
        }
    }
}
