# GraphApiUserClient
An example class for managing users in Azure AD B2C via the MS graph API

To instantiate a user client:

```csharp
var userClient = new UserClient(_configuration.GetClientId(), _configuration.GetTenantId(), _configuration.GetClientSecret());
```


Run the following to enable a user password reset through the api:

- Install latest Azure AD PowerShell Module.
- Run Connect-AzureAD -TenantId Your_B2CTenant.onmicrosoft.com and sign in with Global Administrator account in that tenant.
- Run Get-AzureADDirectoryRole cmd and copy the object id of the User Administrator role.
- Navigate to Azure AD > Enterprise Applications > Search the app and copy the object id of the app.
- Run Add-AzureADDirectoryRoleMember -ObjectId object_ID_copied_in_Step3 -RefObjectId object_ID_copied_in_Step4 cmdlet.
- To verify, navigate to Azure AD B2C > Roles and Administrators > User Administrator. You should see the application present under this role.
