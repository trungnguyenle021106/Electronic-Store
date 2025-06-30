using ApiDto.Response;
using CommonDto.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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
                , [FromBody] Account newAccount, HttpContext httpContext, AuthService authService, EmailValidator emailValidatorService) =>
            {
                if (!authService.IsLogOut(httpContext)) return Results.BadRequest("Hãy đăng xuất tài khoản");

                EmailValidatorDTO emailValidator = await emailValidatorService.CheckEmailValid(newAccount.Email).ConfigureAwait(false);
                if (!emailValidator.Status)
                {
                    return Results.BadRequest(emailValidator.Message);
                }

                CreationResult<Account> result = await createUserUC.CreateAccount(newAccount).ConfigureAwait(false);
                if (result.IsSuccess)
                {
                    Results.Created($"/accounts/{result.CreatedItem.ID}", result.CreatedItem);
                }

                return result.ErrorType switch
                {
                    CreationErrorType.AlreadyExists => Results.Conflict(new { message = result.ErrorMessage }),
                    CreationErrorType.ValidationError => Results.BadRequest(new { message = result.ErrorMessage }),
                    CreationErrorType.RepositoryTypeMismatch => Results.Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Server Configuration Error",
                        detail: result.ErrorMessage
                    ),
                    CreationErrorType.InternalError => Results.Problem(
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
            });
        }

        public static void MapCreateCustomer(this WebApplication app)
        {
            app.MapPost("/customers", async (CreateUserUC createUserUC,
                 HttpContext httpContext, [FromBody] Customer newCustomer, AuthService authService) =>
            {
                CreationResult<Customer> result = await createUserUC.CreateCustomer(newCustomer).ConfigureAwait(false);
                if (result.IsSuccess)
                {
                    Results.Created($"/customers/{result.CreatedItem.ID}", result.CreatedItem);
                }

                return result.ErrorType switch
                {
                    CreationErrorType.AlreadyExists => Results.Conflict(new { message = result.ErrorMessage }),
                    CreationErrorType.ValidationError => Results.BadRequest(new { message = result.ErrorMessage }),
                    CreationErrorType.RepositoryTypeMismatch => Results.Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Server Configuration Error",
                        detail: result.ErrorMessage
                    ),
                    CreationErrorType.InternalError => Results.Problem(
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

        public static void MapRefreshAccessToken(this WebApplication app)
        {
            app.MapPost("/auth/tokens/refresh", async (HttpContext context, CreateUserUC createUserUC, TokenService tokenService) =>
            {
                RefreshToken refreshToken = await tokenService.GetRefreshToken(context).ConfigureAwait(false);

                CreationResult<string> result = await createUserUC.RefreshAccessToken(refreshToken).ConfigureAwait(false);
                if (result.IsSuccess)
                {
                    tokenService.SetTokenCookie(context, "AccessToken", result.CreatedItem,
            DateTimeOffset.Now.AddMinutes(tokenService._jwtSetting.ExpirationMinutes), true, false, SameSiteMode.Lax);
                    return Results.Ok();
                }

                switch (result.ErrorType)
                {
                    case CreationErrorType.Invalid:
                        tokenService.ClearTokenCookie(context, "AccessToken", secure: false, sameSite: SameSiteMode.Lax);
                        tokenService.ClearTokenCookie(context, "RefreshToken", secure: false, sameSite: SameSiteMode.Lax);
                        return Results.BadRequest(new { message = result.ErrorMessage });

                    case CreationErrorType.ValidationError:
                        tokenService.ClearTokenCookie(context, "AccessToken", secure: false, sameSite: SameSiteMode.Lax);
                        tokenService.ClearTokenCookie(context, "RefreshToken", secure: false, sameSite: SameSiteMode.Lax);
                        return Results.BadRequest(new { message = result.ErrorMessage });

                    case CreationErrorType.InternalError:
                        return Results.Problem(
                            statusCode: StatusCodes.Status500InternalServerError,
                            title: "Internal Server Error",
                            detail: result.ErrorMessage
                        );

                    default: // Trường hợp mặc định cho các lỗi không xác định
                        return Results.Problem(
                            statusCode: StatusCodes.Status500InternalServerError,
                            title: "Unknown Error",
                            detail: result.ErrorMessage
                        );
                }
            });
        }
        #endregion

        #region Get User USECASE
        public static void MapGetUserUseCaseAPIs(this WebApplication app)
        {
            MapGetCustomerByIDForCustomer(app);
            MapGetCustomerByIDForAdmin(app);
            MapGetAccountByID(app);
            MapGetAllAccount(app);
            MapGetAllUser(app);
            MapGetCurrentCustomer(app);
            MapGetStatus(app);
            test(app);
        }

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

        public static void MapGetCurrentCustomer(this WebApplication app)
        {
            app.MapGet("/customers/me", async (GetUserUC getUserUC, HttpContext httpContext, TokenService tokenService) =>
            {
                int accountID = tokenService.GetJWTClaim(httpContext)?.IDAccount ?? 0;
                QueryResult<Customer> result = await getUserUC.GetCustomerByAccountID(accountID).ConfigureAwait(false);

                if (result.IsSuccess)
                {
                    return Results.Ok(result.Item);
                }

                return result.ErrorType switch
                {
                    RetrievalErrorType.NotFound => Results.NotFound(new { message = result.ErrorMessage }),
                    RetrievalErrorType.ValidationError => Results.BadRequest(new { message = result.ErrorMessage }),
                    _ => Results.Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Unknown Error",
                        detail: result.ErrorMessage
                    )
                };
            }).RequireAuthorization("OnlyCustomer");
        }

        public static void MapGetCustomerByIDForCustomer(this WebApplication app)
        {
            app.MapGet("/customers/{customerID}", async (GetUserUC getUserUC, int customerID, HttpContext httpContext) =>
            {
                QueryResult<Customer> result = await getUserUC.GetCustomerByID(customerID).ConfigureAwait(false);

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

                return result.ErrorType switch
                {
                    RetrievalErrorType.NotFound => Results.NotFound(new { message = result.ErrorMessage }),
                    RetrievalErrorType.ValidationError => Results.BadRequest(new { message = result.ErrorMessage }),
                    _ => Results.Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Unknown Error",
                        detail: result.ErrorMessage
                    )
                };

            }).RequireAuthorization();
        }

        public static void MapGetCustomerByIDForAdmin(this WebApplication app)
        {
            app.MapGet("/admin/customers/{customerID}", async (GetUserUC getUserUC, int customerID, HttpContext httpContext) =>
            {
                QueryResult<Customer> result = await getUserUC.GetCustomerByID(customerID).ConfigureAwait(false);

                if (result.IsSuccess)
                {
                    return Results.Ok(result.Item);
                }

                return result.ErrorType switch
                {
                    RetrievalErrorType.NotFound => Results.NotFound(new { message = result.ErrorMessage }),
                    RetrievalErrorType.ValidationError => Results.BadRequest(new { message = result.ErrorMessage }),
                    _ => Results.Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Unknown Error",
                        detail: result.ErrorMessage
                    )
                };

            }).RequireAuthorization("OnlyAdmin");
        }

        public static void MapGetAccountByID(this WebApplication app)
        {
            app.MapGet("/accounts/{accountID}", async (GetUserUC getUserUC, HttpContext httpContext, int accountID) =>
            {
                QueryResult<Account> result = await getUserUC.GetAccountByID(accountID).ConfigureAwait(false);

                if (result.IsSuccess)
                {
                    return Results.Ok(result.Item);
                }

                return result.ErrorType switch
                {
                    RetrievalErrorType.NotFound => Results.NotFound(new { message = result.ErrorMessage }),
                    RetrievalErrorType.ValidationError => Results.BadRequest(new { message = result.ErrorMessage }),
                    _ => Results.Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Unknown Error",
                        detail: result.ErrorMessage
                    )
                };
            }).RequireAuthorization("OnlyAdmin");
        }

        public static void MapGetAllAccount(this WebApplication app)
        {
            app.MapGet("/accounts", async (GetUserUC getUserUC) =>
            {
                QueryResult<Account> result = await getUserUC.GetAllAccount().ConfigureAwait(false);

                if (result.IsSuccess)
                {
                    return Results.Ok(result.Items);
                }
                return Results.BadRequest(new { message = result.ErrorMessage });
            }).RequireAuthorization("OnlyAdmin");
        }

        public static void MapGetAllUser(this WebApplication app)
        {
            app.MapGet("/customers", async (GetUserUC getUserUC) =>
            {
                QueryResult<Customer> result = await getUserUC.GetAllCustomer().ConfigureAwait(false);

                if (result.IsSuccess)
                {
                    return Results.Ok(result.Items);
                }
                return Results.BadRequest(new { message = result.ErrorMessage });
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
                HttpContext httpContext, int customerID, [FromBody] Customer newCustomer) =>
            {

                QueryResult<Customer> resultQuery = await getUserUC.GetCustomerByID(customerID).ConfigureAwait(false);
                if (!resultQuery.IsSuccess)
                {
                    return resultQuery.ErrorType switch
                    {
                        RetrievalErrorType.NotFound => Results.NotFound(new { message = resultQuery.ErrorMessage }),
                        RetrievalErrorType.ValidationError => Results.BadRequest(new { message = resultQuery.ErrorMessage }),
                        _ => Results.Problem(
                            statusCode: StatusCodes.Status500InternalServerError,
                            title: "Unknown Error",
                            detail: resultQuery.ErrorMessage
                        )
                    };
                }

                int accountID = resultQuery.Item?.AccountID ?? 0;

                var authorizationService = httpContext.RequestServices.GetRequiredService<IAuthorizationService>();
                var authorizationResult = await authorizationService.AuthorizeAsync(httpContext.User, accountID, "AdminOrSelfAccountId").ConfigureAwait(false);

                if (!authorizationResult.Succeeded)
                {
                    return Results.Forbid();
                }


                UpdateResult<Customer> resultUpdate = await updateUserUC.UpdateCustomerInformation(customerID, newCustomer).ConfigureAwait(false);
                if (resultUpdate.IsSuccess)
                {
                    Results.Ok(resultUpdate.UpdatedItem);
                }

                return resultUpdate.ErrorType switch
                {
                    UpdateErrorType.ValidationError => Results.BadRequest(new { message = resultUpdate.ErrorMessage }),
                    UpdateErrorType.NotFound => Results.NotFound(new { message = resultUpdate.ErrorMessage }),
                    UpdateErrorType.InternalError => Results.Problem(
                         statusCode: StatusCodes.Status500InternalServerError,
                         title: "Internal Server Error",
                         detail: resultUpdate.ErrorMessage
                     ),
                    _ => Results.Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Unknown Error",
                        detail: resultUpdate.ErrorMessage
                    )
                };
            }).RequireAuthorization();
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

                UpdateResult<Account> result = await updateUserUC.UpdateAccountPassword(accountID,
                    updatePasswordRequest.oldPassword, updatePasswordRequest.newPassword).ConfigureAwait(false);

                if (result.IsSuccess)
                {
                    Results.Ok(result.UpdatedItem);
                }

                return result.ErrorType switch
                {
                    UpdateErrorType.ValidationError => Results.BadRequest(new { message = result.ErrorMessage }),
                    UpdateErrorType.NotFound => Results.NotFound(new { message = result.ErrorMessage }),
                    UpdateErrorType.InternalError => Results.Problem(
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
                UpdateResult<Account> result = await updateUserUC.UpdateAccountStatus(accountID, status).
                ConfigureAwait(false);

                if (result.IsSuccess)
                {
                    Results.Ok(result.UpdatedItem);
                }

                return result.ErrorType switch
                {
                    UpdateErrorType.ValidationError => Results.BadRequest(new { message = result.ErrorMessage }),
                    UpdateErrorType.NotFound => Results.NotFound(new { message = result.ErrorMessage }),
                    UpdateErrorType.InternalError => Results.Problem(
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
                , HttpContext httpContext, AuthService authService, TokenService tokenService) =>
            {
                if (!authService.IsLogOut(httpContext)) return Results.BadRequest("You are not logout");
                LoginResult<LoginSignUpDTO> result = await loginLogoutSignUpUC.LoginAccount(loginRequest.Email, loginRequest.Password).ConfigureAwait(false);

                if (result.IsSuccess)
                {
                    tokenService.SetTokenCookie(httpContext, "RefreshToken", result.Value.RefreshToken.TokenHash,
                       result.Value.RefreshToken.ExpiresAt, true, false, SameSiteMode.Lax);

                    tokenService.SetTokenCookie(httpContext, "AccessToken", result.Value.AccessToken,
                        result.Value.RefreshToken.ExpiresAt, true, false, SameSiteMode.Lax);


                    if (result.Value.Account.Role.Equals("Customer"))
                    {
                        return Results.Ok("http://localhost:4200");
                    }
                    else
                    {
                        return Results.Ok("http://localhost:4300");
                    }
                }
                else
                {
                    return result.ErrorType switch
                    {
                        LoginErrorType.AccountLocked => Results.BadRequest(new { message = result.ErrorMessage }),
                        LoginErrorType.NotFound => Results.NotFound(new { message = result.ErrorMessage }),
                        LoginErrorType.InvalidCredentials => Results.BadRequest(new { message = result.ErrorMessage }),
                        LoginErrorType.ValidationError => Results.BadRequest(new { message = result.ErrorMessage }),
                        LoginErrorType.InternalError => Results.Problem(
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
                }
            });
        }

        public static void MapSignUp(this WebApplication app)
        {
            app.MapPost("/auth/sign-up", async (LoginLogoutSignUpUC loginLogoutSignUpUC, [FromBody] SignUpRequest signUpRequest
                , HttpContext httpContext, AuthService authService, TokenService tokenService) =>
            {
                if (!authService.IsLogOut(httpContext)) return Results.BadRequest("You are not logout");
                CreationResult<LoginSignUpDTO> result = await loginLogoutSignUpUC.SignUp(signUpRequest).ConfigureAwait(false);

                if (result.IsSuccess)
                {
                    tokenService.SetTokenCookie(httpContext, "RefreshToken", result.CreatedItem.RefreshToken.TokenHash,
                        result.CreatedItem.RefreshToken.ExpiresAt, true, false, SameSiteMode.Lax);

                    tokenService.SetTokenCookie(httpContext, "AccessToken", result.CreatedItem.AccessToken,
                        result.CreatedItem.RefreshToken.ExpiresAt, true, false, SameSiteMode.Lax);

                    return Results.Ok();
                }
                else
                {
                    return result.ErrorType switch
                    {
                        CreationErrorType.AlreadyExists => Results.Conflict(new { message = result.ErrorMessage }),
                        CreationErrorType.ValidationError => Results.BadRequest(new { message = result.ErrorMessage }),
                        CreationErrorType.InternalError => Results.Problem(
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
                }
            });
        }

        public static void MapLogoutSpecific(this WebApplication app)
        {
            app.MapPost("/auth/logout/specific", async (LoginLogoutSignUpUC loginLogoutSignUpUC
                , HttpContext httpContext, AuthService authService, TokenService tokenService) =>
            {

                UpdateResult<RefreshToken> result = await loginLogoutSignUpUC.LogoutSpecificSession(httpContext).ConfigureAwait(false);

                if (result.IsSuccess)
                {
                    tokenService.ClearTokenCookie(httpContext, "AccessToken", false, SameSiteMode.Lax);
                    tokenService.ClearTokenCookie(httpContext, "RefreshToken", false, SameSiteMode.Lax);
                    return Results.Ok();
                }
                else
                {
                    return result.ErrorType switch
                    {
                        UpdateErrorType.ValidationError => Results.BadRequest(new { message = result.ErrorMessage }),
                        UpdateErrorType.Unauthorized => Results.Unauthorized(),
                        UpdateErrorType.InternalError => Results.Problem(
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
                }
            }).RequireAuthorization();
        }

        #endregion
    }
}
