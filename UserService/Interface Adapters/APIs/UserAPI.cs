using CommonDto;
using CommonDto.ResultDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.Service;
using UserService.Application.Usecases;
using UserService.Domain.DTO;
using UserService.Domain.Entities;
using UserService.Domain.Request;
using UserService.Domain.Response;
using UserService.Infrastructure.Verify_Email;

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
            //MapCreateAccount(app);
            //MapCreateCustomer(app);
            MapRefreshAccessToken(app);
        }

        public static void MapCreateAccount(this WebApplication app)
        {
            app.MapPost("/accounts", async (CreateUserUC createUserUC
                , [FromBody] Account newAccount, HttpContext httpContext, AuthService authService, EmailValidator emailValidatorService,
                HandleResultApi handleResultApi) =>
            {
                if (!authService.IsLogOut(httpContext)) return Results.BadRequest("Hãy đăng xuất tài khoản");

                EmailValidatorDTO emailValidator = await emailValidatorService.CheckEmailValid(newAccount.Email).ConfigureAwait(false);
                if (!emailValidator.Status)
                {
                    return Results.BadRequest(emailValidator.Message);
                }

                ServiceResult<Account> result = await createUserUC.CreateAccount(newAccount).ConfigureAwait(false);
                return handleResultApi.MapServiceResultToHttp(result);
            }).RequireAuthorization("OnlyAdmin");
        }

        public static void MapCreateCustomer(this WebApplication app)
        {
            app.MapPost("/customers", async (CreateUserUC createUserUC, HandleResultApi handleResultApi,
                 HttpContext httpContext, [FromBody] Customer newCustomer, AuthService authService) =>
            {
                ServiceResult<Customer> result = await createUserUC.CreateCustomer(newCustomer).ConfigureAwait(false);
                return handleResultApi.MapServiceResultToHttp(result);
            }).RequireAuthorization("OnlyAdmin");
        }

        public static void MapRefreshAccessToken(this WebApplication app)
        {
            app.MapPost("/auth/tokens/refresh", async (HttpContext context, CreateUserUC createUserUC, TokenService tokenService, HandleResultApi handleResultApi) =>
            {
                RefreshToken refreshToken = await tokenService.GetRefreshToken(context).ConfigureAwait(false);

                ServiceResult<string> result = await createUserUC.RefreshAccessToken(refreshToken).ConfigureAwait(false);

                string accessToken = result.Item;
                result = ServiceResult<string>.NoContent();

                return handleResultApi.MapServiceResultToHttp(result,
                  () =>
                  {
                      tokenService.SetTokenCookie(context, "AccessToken", accessToken,
                         DateTimeOffset.Now.AddMinutes(tokenService._jwtSetting.ExpirationMinutes), true, false, SameSiteMode.Lax);
                  },
                  () =>
                  {
                      tokenService.ClearTokenCookie(context, "AccessToken", secure: false, sameSite: SameSiteMode.Lax);
                      tokenService.ClearTokenCookie(context, "RefreshToken", secure: false, sameSite: SameSiteMode.Lax);
                  });
            });
        }
        #endregion

        #region Get User USECASE
        public static void MapGetUserUseCaseAPIs(this WebApplication app)
        {
            //MapGetCustomerByIDForCustomer(app);
            MapGetCustomerByID(app);
            MapGetAccountByID(app);
            //MapGetAllAccount(app);
            //MapGetAllUser(app);
            MapGetCurrentCustomerInformation(app);
            MapGetStatus(app);
            MapGetCustomerInformationByCustomerID(app);
            MapGetCustomerInformationByAccountID(app);
            GetPagedAccount(app);
            //test(app);
        }

        public static void GetPagedAccount(this WebApplication app)
        {
            app.MapGet("/accounts", async (GetUserUC getUserUC, int page, int pageSize, string? searchText, HandleResultApi handleResultApi) =>
            {
                ServiceResult<PagedResult<Account>> result = await getUserUC.GetPagedAccount(page, pageSize, searchText).ConfigureAwait(false);
               return handleResultApi.MapServiceResultToHttp(result);
            }).RequireAuthorization("OnlyAdmin");
        }

        [Obsolete("This method is for testing purposes only and should not be used in production.")]
        public static void test(this WebApplication app)
        {
            app.MapGet("/test", async (HttpContext httpContext, TokenService tokenService) =>
            {
                RefreshToken refreshToken = await tokenService.GetRefreshToken(httpContext).ConfigureAwait(false);
                JWTClaim jWTClaim = tokenService.GetJWTClaim(httpContext);
                var response = new
                {
                    refreshToken,
                    jWTClaim
                };
                return Results.Ok(response);
            }).RequireAuthorization();
        }

        public static void MapGetCurrentCustomerInformation(this WebApplication app)
        {
            app.MapGet("/customers/me", async (GetUserUC getUserUC, HttpContext httpContext, TokenService tokenService, HandleResultApi handleResultApi) =>
            {
                int accountID = tokenService.GetJWTClaim(httpContext)?.AccountID ?? 0;

                ServiceResult<CustomerInformation> result = await getUserUC.GetCustomerInformationByAccountID(accountID).ConfigureAwait(false);
                return handleResultApi.MapServiceResultToHttp(result);
            
            }).RequireAuthorization("OnlyCustomer");
        }

        [Obsolete("This method is for testing purposes only and should not be used in production.")]
        public static void MapGetCustomerByIDForCustomer(this WebApplication app)
        {
            app.MapGet("/customers/{customerID}", async (GetUserUC getUserUC, int customerID, HttpContext httpContext) =>
            {
                ServiceResult<Customer> result = await getUserUC.GetCustomerByID(customerID).ConfigureAwait(false);

                if (result.IsSuccess)
                {
                    int accountID = result.Item?.AccountID ?? 0;

                    var authorizationService = httpContext.RequestServices.GetRequiredService<IAuthorizationService>();
                    var authorizationResult = await authorizationService.AuthorizeAsync(httpContext.User, accountID, "AdminOrSelfAccountId").ConfigureAwait(false);

                    if (!authorizationResult.Succeeded)
                    {
                        return Results.Forbid();
                    }

                    return Results.Ok(result.Item);
                }

                return result.ServiceErrorType switch
                {
                    ServiceErrorType.NotFound => Results.NotFound(new { message = result.ErrorMessage }),
                    ServiceErrorType.ValidationError => Results.BadRequest(new { message = result.ErrorMessage }),
                    _ => Results.Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Unknown Error",
                        detail: result.ErrorMessage
                    )
                };

            }).RequireAuthorization();
        }


        public static void MapGetCustomerByID(this WebApplication app)
        {
            app.MapGet("/customers/{customerID}", async (GetUserUC getUserUC, int customerID, HttpContext httpContext, HandleResultApi handleResultApi) =>
            {
                ServiceResult<Customer> result = await getUserUC.GetCustomerByID(customerID).ConfigureAwait(false);
                return handleResultApi.MapServiceResultToHttp(result);
            }).RequireAuthorization("OnlyAdmin");
        }

        public static void MapGetCustomerInformationByCustomerID(this WebApplication app)
        {
            app.MapGet("/customers/{customerID}/customer-information", async (GetUserUC getUserUC, int customerID, HttpContext httpContext, HandleResultApi handleResultApi) =>
            {
                ServiceResult<CustomerInformation> result = await getUserUC.GetCustomerInformationByCustomerID(customerID).ConfigureAwait(false);
                return handleResultApi.MapServiceResultToHttp(result);
            }).RequireAuthorization("OnlyAdmin");
        }

        public static void MapGetCustomerInformationByAccountID(this WebApplication app)
        {
            app.MapGet("/accounts/{accountID}/customer-information", async (GetUserUC getUserUC, int accountID, HttpContext httpContext, HandleResultApi handleResultApi) =>
            {
                ServiceResult<CustomerInformation> result = await getUserUC.GetCustomerInformationByAccountID(accountID).ConfigureAwait(false);
                return handleResultApi.MapServiceResultToHttp(result);
            }).RequireAuthorization("OnlyAdmin");
        }

        public static void MapGetAccountByID(this WebApplication app)
        {
            app.MapGet("/accounts/{accountID}", async (GetUserUC getUserUC, HandleResultApi handleResultApi, HttpContext httpContext, int accountID) =>
            {
                ServiceResult<Account> result = await getUserUC.GetAccountByID(accountID).ConfigureAwait(false);
                return handleResultApi.MapServiceResultToHttp(result);         
            }).RequireAuthorization("OnlyAdmin");
        }

        [Obsolete("This method is for testing purposes only and should not be used in production.")]
        public static void MapGetAllAccount(this WebApplication app)
        {
            app.MapGet("/accounts", async (GetUserUC getUserUC, HandleResultApi handleResultApi) =>
            {
                ServiceResult<Account> result = await getUserUC.GetAllAccount().ConfigureAwait(false);
                return handleResultApi.MapServiceResultToHttp(result);
            }).RequireAuthorization("OnlyAdmin");
        }
        [Obsolete("This method is for testing purposes only and should not be used in production.")]
        public static void MapGetAllUser(this WebApplication app)
        {
            app.MapGet("/customers", async (GetUserUC getUserUC, HandleResultApi handleResultApi) =>
            {
                ServiceResult<Customer> result = await getUserUC.GetAllCustomer().ConfigureAwait(false);
                return handleResultApi.MapServiceResultToHttp(result);
            }).RequireAuthorization("OnlyAdmin");
        }

        public static void MapGetStatus(this WebApplication app)
        {
            app.MapGet("/auth/status", (HttpContext httpContext, TokenService tokenService) =>
            {
                JWTClaim jWTClaim = tokenService.GetJWTClaim(httpContext);
                return Results.Ok(jWTClaim.Role);
            }).RequireAuthorization();
        }

        #endregion

        #region Update User USECASE
        public static void MapUpdateUserUseCaseAPIs(this WebApplication app)
        {
            MapUpdateCustomerInformation(app);
            MapUpdateAccountPassword(app);
            MapUpdateAccountStatus(app);
        }

        public static void MapUpdateCustomerInformation(this WebApplication app)
        {
            app.MapPut("/customers/{customerID}", async (UpdateUserUC updateUserUC, GetUserUC getUserUC,
                HttpContext httpContext, int customerID, [FromBody] Customer newCustomer, TokenService tokenService, HandleResultApi handleResultApi) =>
            {

                int customerIDInJWT = tokenService.GetJWTClaim(httpContext)?.CustomerID ?? 0;
                if (!customerID.Equals(customerIDInJWT))
                {
                    return Results.Forbid();
                }

                ServiceResult<Customer> resultUpdate = await updateUserUC.UpdateCustomerInformation(customerID, newCustomer).ConfigureAwait(false);
                return handleResultApi.MapServiceResultToHttp(resultUpdate);
            }).RequireAuthorization("OnlyCustomer");
        }

        public static void MapUpdateAccountPassword(this WebApplication app)
        {
            app.MapPatch("/accounts/{accountID}/password", async (HttpContext httpContext, UpdateUserUC updateUserUC,
                int accountID, [FromBody] UpdatePasswordRequest updatePasswordRequest) =>
            {

                var authorizationService = httpContext.RequestServices.GetRequiredService<IAuthorizationService>();
                var authorizationResult = await authorizationService.AuthorizeAsync(httpContext.User, accountID, "SelfAccountId").ConfigureAwait(false);

                if (!authorizationResult.Succeeded)
                {
                    return Results.Forbid();
                }

                ServiceResult<Account> result = await updateUserUC.UpdateAccountPassword(accountID,
                    updatePasswordRequest.oldPassword, updatePasswordRequest.newPassword).ConfigureAwait(false);

                if (result.IsSuccess)
                {
                    return Results.Ok(result.Item);
                }

                return result.ServiceErrorType switch
                {
                    ServiceErrorType.ValidationError => Results.BadRequest(new { message = result.ErrorMessage }),
                    ServiceErrorType.NotFound => Results.NotFound(new { message = result.ErrorMessage }),
                    ServiceErrorType.InternalError => Results.Problem(
                         statusCode: StatusCodes.Status500InternalServerError,
                         title: "Internal Server Error",
                         detail: result.ErrorMessage
                     ),
                    _ => Results.Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Unknown Error",
                        detail: result.ErrorMessage
                    )
                };
            }).RequireAuthorization();
        }

        public static void MapUpdateAccountStatus(this WebApplication app)
        {
            app.MapPatch("/accounts/{accountID}/status", async (HttpContext httpContext, UpdateUserUC updateUserUC,
                int accountID, [FromBody] string status) =>
            {
                ServiceResult<Account> result = await updateUserUC.UpdateAccountStatus(accountID, status).
                ConfigureAwait(false);

                if (result.IsSuccess)
                {
                    return Results.Ok(result.Item);
                }

                return result.ServiceErrorType switch
                {
                    ServiceErrorType.ValidationError => Results.BadRequest(new { message = result.ErrorMessage }),
                    ServiceErrorType.NotFound => Results.NotFound(new { message = result.ErrorMessage }),
                    ServiceErrorType.InternalError => Results.Problem(
                         statusCode: StatusCodes.Status500InternalServerError,
                         title: "Internal Server Error",
                         detail: result.ErrorMessage
                     ),
                    _ => Results.Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Unknown Error",
                        detail: result.ErrorMessage
                    )
                };
            }).RequireAuthorization("OnlyAdmin");
        }
        #endregion

        #region Login Logout SignUp USECASE
        public static void MapLoginLogoutSignUpUseCaseAPIs(this WebApplication app)
        {
            MapLogin(app);
            MapSignUp(app);
            MapLogoutSpecific(app);
        }

        public static void MapLogin(this WebApplication app)
        {
            app.MapPost("/auth/login", async (LoginLogoutSignUpUC loginLogoutSignUpUC, [FromBody] LoginRequest loginRequest
                , HttpContext httpContext, AuthService authService, TokenService tokenService, HandleResultApi handleResultApi) =>
            {
                if (!authService.IsLogOut(httpContext)) return Results.BadRequest("You are not logout");
                ServiceResult<LoginSignUpDTO> result = await loginLogoutSignUpUC.LoginAccount(loginRequest.Email, loginRequest.Password).ConfigureAwait(false);

                return handleResultApi.MapServiceResultToHttpNoContent(result,
                     () =>
                         {
                             tokenService.SetTokenCookie(httpContext, "RefreshToken", result.Item.RefreshToken.TokenHash,
                               result.Item.RefreshToken.ExpiresAt, true, false, SameSiteMode.Lax);

                             tokenService.SetTokenCookie(httpContext, "AccessToken", result.Item.AccessToken,
                                 result.Item.RefreshToken.ExpiresAt, true, false, SameSiteMode.Lax);
                         },
                     () =>
                         {
                             if (result.Item.Account.Role.Equals("Customer"))
                             {
                                 return Results.Ok("http://localhost:4200");
                             }
                             else
                             {
                                 return Results.Ok("http://localhost:4300");
                             }
                         }
                         );
            });
        }

        public static void MapSignUp(this WebApplication app)
        {
            app.MapPost("/auth/sign-up", async (LoginLogoutSignUpUC loginLogoutSignUpUC, [FromBody] SignUpRequest signUpRequest
                , HttpContext httpContext, AuthService authService, TokenService tokenService, HandleResultApi handleResultApi) =>
            {
                if (!authService.IsLogOut(httpContext)) return Results.BadRequest("You are not logout");
                ServiceResult<LoginSignUpDTO> result = await loginLogoutSignUpUC.SignUp(signUpRequest).ConfigureAwait(false);
                return handleResultApi.MapServiceResultToHttpNoContent(result,
                    () =>
                    {
                        tokenService.SetTokenCookie(httpContext, "RefreshToken", result.Item.RefreshToken.TokenHash,
                          result.Item.RefreshToken.ExpiresAt, true, false, SameSiteMode.Lax);

                        tokenService.SetTokenCookie(httpContext, "AccessToken", result.Item.AccessToken,
                            result.Item.RefreshToken.ExpiresAt, true, false, SameSiteMode.Lax);
                    }, null
                    );
            });
        }

        public static void MapLogoutSpecific(this WebApplication app)
        {
            app.MapPost("/auth/logout/specific", async (LoginLogoutSignUpUC loginLogoutSignUpUC
                , HttpContext httpContext, AuthService authService, TokenService tokenService, HandleResultApi handleResultApi) =>
            {

                ServiceResult<RefreshToken> result = await loginLogoutSignUpUC.LogoutSpecificSession(httpContext).ConfigureAwait(false);

                return handleResultApi.MapServiceResultToHttpNoContent(result,
                    () =>
                        {
                            tokenService.ClearTokenCookie(httpContext, "AccessToken", false, SameSiteMode.Lax);
                            tokenService.ClearTokenCookie(httpContext, "RefreshToken", false, SameSiteMode.Lax);
                        },
                    null
                    );
            }).RequireAuthorization();
        }

        #endregion
    }
}
