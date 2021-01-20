using GraphApi.Extensions;

using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace GraphApi.Clients
{
    public class UserClient
    {
        private readonly GraphServiceClient _graphClient;

        public UserClient(string appId, string tenantId, string clientSecret)
        {
            // Initialize the client credential auth provider
            var confidentialClientApplication = ConfidentialClientApplicationBuilder
                .Create(appId)
                .WithTenantId(tenantId)
                .WithClientSecret(clientSecret)
                .Build();
            var authProvider = new ClientCredentialProvider(confidentialClientApplication);

            // Set up the Microsoft Graph service client with client credentials
            _graphClient = new GraphServiceClient(authProvider);
        }

        public async Task<IList<User>> Users()
        {
            // Get all users (one page)
            var result = await _graphClient.Users
                .Request()
                .Select("DisplayName,Surname,GivenName,Mail,OtherMails,Id,Identities")//this will return the full user object with all OTHER properties as null.
                                                                                      //Later select these properties to display them.
                .GetAsync();

            return result.CurrentPage;
        }

        public async Task<int> CreateUsers(IList<User> users)
        {
            List<BatchResponseContent> returnedResponses = new List<BatchResponseContent>();

            while (users.Any())
            {
                //limited to 20 per request
                var userBatch = users.GetAndRemove(20);
                var batchRequestContent = new BatchRequestContent();

                //Create a json batch request
                foreach (var newUser in userBatch)
                {
                    var jsonUser = _graphClient.HttpProvider.Serializer.SerializeAsJsonContent(newUser);
                    var addUserRequest = _graphClient.Users.Request().GetHttpRequestMessage();
                    addUserRequest.Method = HttpMethod.Post;
                    addUserRequest.Content = jsonUser;
                    var addUserRequestId = batchRequestContent.AddBatchRequestStep(addUserRequest);
                }
                var returnedResponse = await _graphClient.Batch.Request().PostAsync(batchRequestContent);
                returnedResponses.Add(returnedResponse);
            }

            int count = 0;

            try
            {
                //parse the responses and check for errors
                foreach (var returnedResponse in returnedResponses)
                {
                    var response = await returnedResponse.GetResponsesAsync();
                    foreach (var responseKey in response.Keys)
                    {
                        var value = response[responseKey];
                        if (!value.IsSuccessStatusCode)
                        {
                            Console.WriteLine($"Create user failed. Response Key: {value.Content}");
                        }
                        else
                        {
                            count++;
                        }
                    }

                    Console.WriteLine($" {count} users created.");

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Create user failed: {ex.Message}");
            }
            return count;
        }

        public async Task<User> CreateUser(User user)
        {
            var newUser = await _graphClient.Users
                .Request()
                .AddAsync(user);

            return newUser;
        }

        public async Task UpdateUser(User user)
        {
            await _graphClient.Users[user.Id]
                    .Request()
                    .UpdateAsync(user);
        }

        public async Task<User> GetUser(string id)
        {
            var user = await _graphClient.Users[id]
                .Request()
                .Select("DisplayName,Surname,GivenName,Mail,OtherMails,Id,Identities")//this will return the full user object with all OTHER properties as null.
                                                                                      //Later select these properties to display them.
                .GetAsync();

            return user;
        }

        public async Task ResetPassword(string id)
        {
            //https://docs.microsoft.com/en-us/azure/active-directory-b2c/microsoft-graph-get-started?tabs=app-reg-ga#enable-user-delete-and-password-update

            var user = new User
            {
                PasswordProfile = new PasswordProfile
                {
                    ForceChangePasswordNextSignIn = true,
                }
            };

            await _graphClient.Users[id]
                .Request()
                .UpdateAsync(user);
        }

        public async Task UpdatePassword(string id, string password)
        {
            //https://docs.microsoft.com/en-us/azure/active-directory-b2c/microsoft-graph-get-started?tabs=app-reg-ga#enable-user-delete-and-password-update
            
            var user = new User
            {
                Id = id,
                PasswordProfile = new PasswordProfile
                {
                    Password = password,
                    ForceChangePasswordNextSignIn = false,
                }
            };

            await _graphClient.Users[id]
                .Request()
                .UpdateAsync(user);
        }

        public async Task DeleteUser(string id)
        {
            //https://docs.microsoft.com/en-us/azure/active-directory-b2c/microsoft-graph-get-started?tabs=app-reg-ga#enable-user-delete-and-password-update
            
            await _graphClient.Users[id]
                .Request()
                .DeleteAsync();
        }
    }
}
