using System;
using GraphApi.Models.Shared;
using GraphApi.Models.Users;

using Microsoft.Graph;

using System.Collections.Generic;
using System.Linq;
using AzureB2CApi.Helpers;

namespace GraphApi.Extensions
{
    public static class Extensions
    {
        /// <summary>
        /// Gets then removes the first specified number of items in a List.
        /// If the specified number to remove is greater than the count of the list, then all items will
        /// be removed and returned.  The originating list will then be empty.
        /// </summary>
        /// <typeparam name="T">The Type of list</typeparam>
        /// <param name="list">A collection of Type T</param>
        /// <param name="numToRemove">The number of items to remove from the list.</param>
        /// <returns></returns>
        public static IList<T> GetAndRemove<T>(this IList<T> list, int numToRemove)
        {
            lock (list)
            {
                List<T> values = new List<T>();

                if (list.Count > numToRemove)
                {
                    while (numToRemove != 0)
                    {
                        numToRemove--;
                        values.Add(list[0]);
                        list.RemoveAt(0);
                    }
                }
                else
                {
                    values.AddRange(list);
                    list.Clear();
                }

                return values;
            }
        }

        public static User ToUser(this UserModel userModel)
        {
            var user = new User
            {
                Id = userModel.Id,
                DisplayName = userModel.DisplayName,
                Mail = userModel.Mail,
                Surname = userModel.Surname,
                GivenName = userModel.GivenName,
            };
            return user;
        }

        public static User CreateUser(this UserModel model)
        {
            return new User
            {
                AccountEnabled = true,
                DisplayName = model.DisplayName,
                OtherMails = new List<string> { model.Mail },
                Mail = model.Mail,
                MailNickname = model.DisplayName,
                UserPrincipalName = $"{Guid.NewGuid()}@{Program.Configuration.GetSection("AzureAdB2C:Domain").Value}",
                Identities = new List<ObjectIdentity> {
                    new ObjectIdentity
                        {SignInType = "userName", Issuer = "lodsaccb2c.onmicrosoft.com", IssuerAssignedId = model.DisplayName},
                    new ObjectIdentity
                        {SignInType = "emailAddress", Issuer = "lodsaccb2c.onmicrosoft.com", IssuerAssignedId = model.Mail}
                },
                GivenName = model.GivenName,
                Surname = model.Surname,
                PasswordProfile = new PasswordProfile { ForceChangePasswordNextSignIn = true, ForceChangePasswordNextSignInWithMfa = false, Password = PasswordGenerator.GeneratePassword() },
            };
        }

        public static PagerModel<UserModel> ToPagerModel(this IList<User> users, string search, int page = 1, int pageSize = 10)
        {
            var userModels = users.Select(u => (UserModel)u);
            if (!string.IsNullOrWhiteSpace(search))
            {
                userModels = userModels.Where(u =>
                    !string.IsNullOrWhiteSpace(u.DisplayName) && u.DisplayName.Contains(search) || 
                    !string.IsNullOrWhiteSpace(u.Mail) && u.Mail.Contains(search) || 
                    u.OtherMails.Any(o => !string.IsNullOrWhiteSpace(o) && o.Contains(search))
                                             );
            }

            if (page <= 0) page = 1;

            //user list to prevent multiple enumeration
            var userModelList = userModels.ToList();

            var totalCount = userModelList.Count;
            var models = userModelList.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var userPageModel = new PagerModel<UserModel>()
            {
                Data = models,
                PageSize = pageSize,
                TotalCount = totalCount,
            };

            return userPageModel;
        }
    }
}
