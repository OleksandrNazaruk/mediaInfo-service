using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.DirectoryServices.AccountManagement;

namespace _MediaInfoService.Middlewares
{
    public class SwaggerBasicAuthMiddleware
    {
        private readonly RequestDelegate next;
        public SwaggerBasicAuthMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        
        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/swagger"))
            {
                string authHeader = context.Request.Headers["Authorization"];
                if (authHeader != null && authHeader.StartsWith("Basic "))
                {
                    // Get the credentials from request header
                    var header = AuthenticationHeaderValue.Parse(authHeader);
                    var inBytes = Convert.FromBase64String(header.Parameter);
                    var credentials = Encoding.UTF8.GetString(inBytes).Split(':');
                    var username = credentials[0];
                    var password = credentials[1];

                    // validate credentials
                    if (SwaggerBasicAuthMiddleware.ValidatePassword(username, password))
                    {
                        await next.Invoke(context).ConfigureAwait(false);
                        return;
                    }
                }
                context.Response.Headers["WWW-Authenticate"] = "Basic";
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }
            else
            {
                await next.Invoke(context).ConfigureAwait(false);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        static bool ValidatePassword(string userName, string password)
        {
            bool ValidatePasswordDomain(string userName, string password, string domain)
            {
                try
                {
                    using (PrincipalContext context = new PrincipalContext(ContextType.Domain, domain))
                    {
                        /*            
                                    var user = UserPrincipal.FindByIdentity(ctx, username);
                                    if (user == null)
                                    {
                                        return Task.CompletedTask;
                                    }
                                    else
                                    {
                                        DirectoryEntry directoryEntry = user.GetUnderlyingObject() as DirectoryEntry;
                                        var userDepartment = directoryEntry.Properties\["department"\].Value.ToString();
                                        if (requirement.Department == userDepartment)
                                        {
                                            context.Succeed(requirement);
                                        }
                                    }
                        */
                        return
                                context.ValidateCredentials(userName, password);
                    }
                }
                catch (PrincipalOperationException ex)
                {
                    // Blank password - that's ok
                    return ex.ErrorCode == -2147023569 && string.IsNullOrEmpty(password);
                }
                catch (Exception)
                {
                    return false;
                }
            }

            bool ValidatePasswordLocal(string userName, string password)
            {
                try
                {
                    using (PrincipalContext context = new PrincipalContext(ContextType.Machine))
                    {
                        return
                            context.ValidateCredentials(userName, password);
                    }
                }
                catch (PrincipalOperationException ex)
                {
                    // Blank password - that's ok
                    return ex.ErrorCode == -2147023569 && string.IsNullOrEmpty(password);
                }
                catch (Exception)
                {
                    return false;
                }
            }

            (string username, string? domain) ParseUserDomainName(string fullUserName)
            {

                if (fullUserName.Contains('\\'))
                {
                    var pair = fullUserName.Split('\\');
                    return (username: pair[1], domain: pair[0]);
                }
                else
                if (fullUserName.Contains('@'))
                {
                    var pair = fullUserName.Split('@');
                    return (username: pair[0], domain: (pair.Length > 1) ? pair[1] : null!);
                }
                else
                    return (username: fullUserName, domain: null!);
            }


            bool result = false;

            result = ValidatePasswordLocal(userName, password);

            if (!result)
            {
                var userDomainName = ParseUserDomainName(userName);
                if (!string.IsNullOrEmpty(userDomainName.domain))
                {
                    result = ValidatePasswordDomain(userDomainName.username, password, userDomainName.domain);
                }
            }


            return result;
        }
    }
}
