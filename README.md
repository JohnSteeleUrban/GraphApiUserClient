# GraphApiUserClient
An example class for managing users in Azure AD B2C via the MS graph API

To instantiate a user client:

```csharp
var userClient = new UserClient(_configuration.GetClientId(), _configuration.GetTenantId(), _configuration.GetClientSecret());
```
