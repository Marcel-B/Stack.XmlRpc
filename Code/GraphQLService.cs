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
        //private Secrets _secrets { get; }
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

        protected void HandleGraphQlError(
            string message,
            GraphQLResponse response)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(message);
            stringBuilder.AppendLine();

            foreach (var error in response.Errors)
                stringBuilder.AppendLine(error.Message);

            _logger.LogError(2222, stringBuilder.ToString());
        }

        protected async Task<GraphQLResponse> PostRequestAsync(
            string query)
        {
            var token = await _tokenService.RequestTokenAsync();
            if (token == null)
                return null;

            SetHeader(token);

            try
            {
                var graphQLRequest = new GraphQLRequest()
                {
                    Query = query
                };

                var response = await GraphQLClient.PostAsync(graphQLRequest);

                if (response.Errors != null)
                {
                    HandleGraphQlError($"Error occurred while upload HomeMatic values with GraphQL.", response);
                    return null;
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(2232, ex,
                    $"Error occurred while uploading HomeMatic values with GraphQL"); // '{content}'.");
                return null;
            }
        }


        protected async Task<GraphQLResponse> PostRequestAsync(
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

                var response = await GraphQLClient.PostAsync(graphQLRequest);

                if (response.Errors != null)
                {
                    HandleGraphQlError($"Error occurred while upload HomeMatic values with GraphQL:", response);
                    return null;
                }
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(2232, ex, $"Error occurred while uploading HomeMatic values with GraphQL:");// '{content}'.");
                return null;
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
