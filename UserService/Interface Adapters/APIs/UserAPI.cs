using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserService.Application.Usecases;
using UserService.Domain.Entities;
using UserService.Domain.Request;
using UserService.Domain.Response;
using UserService.Infrastructure.DBContext;
using MyJWTHandler;
using System.Text.Json;
using UserService.Infrastructure.Verify_Email;
using UserService.Application.UnitOfWorks;
using UserService.Domain.Interface.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using System.Reflection.Metadata;
using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Client;
using System.Net.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace UserService.Interface_Adapters.APIs
{
    public static class UserAPI
    {
        public static void MapUserEndpoints(this WebApplication app)
        {
            MapCreateUserUseCaseAPIs(app);
            MapGetUserUseCaseAPIs(app);
            MapUpdateUserUseCaseAPIs(app);
            MapLoginLogoutSignUpUseCaseAPIs(app);
        }

        #region Create User USECASE
        public static void MapCreateUserUseCaseAPIs(this WebApplication app)
        {
            MapCreateAccount(app);
            MapCreateCustomer(app);
        }

        public static void MapCreateAccount(this WebApplication app)
        {
            app.MapPost("/accounts", async (IUnitOfWork unitOfWork
                , [FromBody] Account newAccount, HttpContext httpContext) =>
            {
                if (!IsLogOut(httpContext)) return Results.BadRequest("Hãy đăng xuất tài khoản");

                EmailValidatorResponse emailValidator = await EmailValidator.CheckEmailValid(newAccount.Email).ConfigureAwait(false);
                if (!emailValidator.Status)
                {
                    return Results.BadRequest(emailValidator.Message);
                }

                Account? createdAccount = await new CreateUserUC(unitOfWork).CreateAccount(newAccount).ConfigureAwait(false);
                if (createdAccount != null)
                {
                    return Results.Ok(createdAccount);
                }
                return Results.BadRequest("Email đã được đăng ký");
            });
        }

        public static void MapCreateCustomer(this WebApplication app)
        {
            app.MapPost("/customers", async (IUnitOfWork userContext,
                 HttpContext httpContext, [FromBody] Customer newCustomer) =>
            {
                if (!HaveAName(httpContext))
                {
                    return Results.StatusCode(403);
                }

                return Results.Ok(await new CreateUserUC(userContext).CreateCustomer(newCustomer).ConfigureAwait(false));
            }).RequireAuthorization();
        }
        #endregion

        #region Get User USECASE
        public static void MapGetUserUseCaseAPIs(this WebApplication app)
        {
            MapGetCustomerByID(app);
            MapGetAccountByID(app);
            MapGetAccount(app);
            MapGetUser(app);
        }

        public static void MapGetCustomerByID(this WebApplication app)
        {
            app.MapGet("/customers/{customerID}", async (UserContext userContext, int customerID, HttpContext httpContext) =>
            {

                Customer? customer = await new GetUserUC(userContext).GetCustomerByID(customerID).ConfigureAwait(false);
                int accountID = customer?.AccountID ?? 0;

                var authorizationService = httpContext.RequestServices.GetRequiredService<IAuthorizationService>();
                var authorizationResult = await authorizationService.AuthorizeAsync(httpContext.User, accountID, "AdminOrSelfAccountId").ConfigureAwait(false);

                if (!authorizationResult.Succeeded)
                {
                    return Results.Forbid();
                }

                return Results.Ok(await new GetUserUC(userContext).GetCustomerByID(customerID).ConfigureAwait(false));
            }).RequireAuthorization();
        }

        public static void MapGetAccountByID(this WebApplication app)
        {
            app.MapGet("/accounts/{accountID}", async (HttpContext httpContext, UserContext userContext, int accountID) =>
            {
                var authorizationService = httpContext.RequestServices.GetRequiredService<IAuthorizationService>();
                var authorizationResult = await authorizationService.AuthorizeAsync(httpContext.User, accountID, "AdminOrSelfAccountId").ConfigureAwait(false);

                if (!authorizationResult.Succeeded)
                {
                    return Results.Forbid();
                }

                return Results.Ok(await new GetUserUC(userContext).GetAccountByID(accountID).ConfigureAwait(false));
            }).RequireAuthorization();
        }

        public static void MapGetAccount(this WebApplication app)
        {
            app.MapGet("/accounts", async (UserContext userContext) =>
            {
                return Results.Ok(await new GetUserUC(userContext).GetAllAccount().ConfigureAwait(false));
            }).RequireAuthorization("OnlyAdmin");
        }

        public static void MapGetUser(this WebApplication app)
        {
            app.MapGet("/customers", async (UserContext userContext) =>
            {
                return Results.Ok(await new GetUserUC(userContext).GetAllAccount().ConfigureAwait(false));
            }).RequireAuthorization("OnlyAdmin");
        }
        #endregion

        #region Update User USECASE
        public static void MapUpdateUserUseCaseAPIs(this WebApplication app)
        {
            MapUpdateCustomerInformation(app);
            MapUpdateAccount(app);
        }

        public static void MapUpdateCustomerInformation(this WebApplication app)
        {
            app.MapPatch("/customers/{customerID}", async (HttpContext httpContext, UserContext userContext,
                int customerID, Customer newCustomer) =>
            {
                Customer? customer = await new GetUserUC(userContext).GetCustomerByID(customerID).ConfigureAwait(false);
                int accountID = customer?.AccountID ?? 0;

                var authorizationService = httpContext.RequestServices.GetRequiredService<IAuthorizationService>();
                var authorizationResult = await authorizationService.AuthorizeAsync(httpContext.User, accountID, "SelfAccountId").ConfigureAwait(false);

                if (!authorizationResult.Succeeded)
                {
                    return Results.Forbid();
                }

                return Results.Ok(await new UpdateUserUC(userContext).
                    UpdateCustomerInformation(customerID, newCustomer).ConfigureAwait(false));
            }).RequireAuthorization();
        }

        public static void MapUpdateAccount(this WebApplication app)
        {
            app.MapPatch("/accounts/{accountID}", async (HttpContext httpContext, UserContext context,
                int accountID, Account newAccount) =>
            {

                var authorizationService = httpContext.RequestServices.GetRequiredService<IAuthorizationService>();
                var authorizationResult = await authorizationService.AuthorizeAsync(httpContext.User, accountID, "SelfAccountId").ConfigureAwait(false);

                if (!authorizationResult.Succeeded)
                {
                    return Results.Forbid();
                }

                return Results.Ok(await new UpdateUserUC(context).
                    UpdateAccount(accountID, newAccount).ConfigureAwait(false));
            }).RequireAuthorization();
        }
        #endregion

        #region Login Logout SignUp USECASE
        public static void MapLoginLogoutSignUpUseCaseAPIs(this WebApplication app)
        {
            MapLogin(app);
        }

        public static void MapLogin(this WebApplication app)
        {
            app.MapPost("/users/login", async (UserContext userContext, [FromBody] LoginRequest loginRequest
                , HttpContext httpContext) =>
            {
                if (!IsLogOut(httpContext)) return Results.BadRequest("Hãy đăng xuất tài khoản");
                LoginResponse result = await new LoginLogoutSignUpUC(userContext).LoginAccount(loginRequest.UserName, loginRequest.Password).ConfigureAwait(false);

                if (result.statusCode == 1)
                {
                    return Results.Ok(GenerateToken(result, app));
                }
                return Results.Ok(result.Message);
            });
        }

        public static void MapSignUpToken(this WebApplication app)
        {
            app.MapPost("/users/signup/token", async (UserContext userContext, [FromBody] LoginRequest loginRequest
                , HttpContext httpContext) =>
            {
                if (!IsLogOut(httpContext)) return Results.BadRequest("Hãy đăng xuất tài khoản");
                LoginResponse result = await new LoginLogoutSignUpUC(userContext).LoginAccount(loginRequest.UserName, loginRequest.Password).ConfigureAwait(false);

                if (result.statusCode == 1)
                {       
                    return Results.Ok(GenerateToken(result, app));
                }
                return Results.Ok(result.Message);
            });
        }


        #endregion

        #region Other Methods
        private static string GenerateToken(LoginResponse result, WebApplication app)
        {
            int customerId = result.customer != null ? result.customer.ID : 0;
            string customerName = result.customer != null ? result.customer.Name : "";
            string customerPhone = result.customer != null ? result.customer.Phone : "";

            string issuer = app.Configuration["Jwt:Issuer"] ?? "";
            string audience = app.Configuration["Jwt:Audience"] ?? "";
            string key = app.Configuration["Jwt:Key"] ?? "";
            return JWTHandler.GenerateAccessToken
                    (issuer, audience, key
                    , result.idAccount ?? 0, result.role ?? false, customerName, customerPhone, customerId);
        }

        private static bool HaveAName(HttpContext httpContext)
        {
            var user = httpContext.User; // Lấy thông tin người dùng từ HttpContext
            var nameClaim = user.FindFirst(ClaimTypes.Name);
            string name = nameClaim?.Value ?? "";

            if (string.IsNullOrEmpty(name))
            {
                return true; 
            }

            return false;
        }

        private static bool IsLogOut(HttpContext httpContext)
        {
            if (httpContext.User.Identity.IsAuthenticated)
            {
                return false; 
            }
            return true;
        }
        #endregion
    }
}
