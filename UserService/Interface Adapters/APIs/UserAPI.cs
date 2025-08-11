using CommonDto;
using CommonDto.ResultDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using UserService.Application.Service;
using UserService.Application.Usecases;
using UserService.Domain.DTO;
using UserService.Domain.Entities;
using UserService.Domain.Request;
using UserService.Domain.Response;
using UserService.Infrastructure.Cache_Service;
using UserService.Infrastructure.SendEmail;
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
            MapCreateAccount(app);
            MapCreateCustomer(app);
            MapRefreshAccessToken(app);
            ForgetPasswoord(app);
            ResendCode(app);
            ChechVerifyCode(app);
        }

        public static void MapCreateAccount(this WebApplication app)
        {
            app.MapPost("/accounts", async (CreateUserUC createUserUC, [FromBody] Account newAccount, HttpContext httpContext, AuthService authService, EmailValidatorService emailValidatorService,
                HandleResultApi handleResultApi) =>
            {
                if (!authService.IsLogOut(httpContext)) return Results.BadRequest("Hãy đăng xuất tài khoản");

                EmailValidatorDTO emailValidator = await emailValidatorService.CheckEmailValid(newAccount.Email).ConfigureAwait(false);
                if (!emailValidator.IsValid)
                {
                    return Results.BadRequest(emailValidator.Message);
                }

                ServiceResult<Account> result = await createUserUC.CreateAccount(newAccount).ConfigureAwait(false);
                return handleResultApi.MapServiceResultToHttp(result);
            })
            .RequireAuthorization("OnlyAdmin")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Tạo tài khoản mới";
                operation.Description = "Tạo một tài khoản người dùng mới. Yêu cầu quyền admin.";
                operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Auth" } };
                return operation;
            });
        }

        public static void MapCreateCustomer(this WebApplication app)
        {
            app.MapPost("/customers", async (CreateUserUC createUserUC, HandleResultApi handleResultApi, HttpContext httpContext, [FromBody] Customer newCustomer, AuthService authService) =>
            {
                ServiceResult<Customer> result = await createUserUC.CreateCustomer(newCustomer).ConfigureAwait(false);
                return handleResultApi.MapServiceResultToHttp(result);
            })
            .RequireAuthorization("OnlyAdmin")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Tạo khách hàng mới";
                operation.Description = "Tạo một hồ sơ khách hàng mới. Yêu cầu quyền admin.";
                operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Users" } };
                return operation;
            });
        }

        public static void MapRefreshAccessToken(this WebApplication app)
        {
            app.MapPost("/auth/tokens/refresh", async (HttpContext context, CreateUserUC createUserUC, TokenService tokenService, HandleResultApi handleResultApi) =>
            {
                RefreshToken refreshToken = await tokenService.GetRefreshToken(context).ConfigureAwait(false);

                ServiceResult<string> result = await createUserUC.RefreshAccessToken(refreshToken).ConfigureAwait(false);

                return handleResultApi.MapServiceResultToHttpNoContent(result,
                    () =>
                    {
                        tokenService.SetTokenCookie(context, "AccessToken", result.Item,
                            DateTimeOffset.Now.AddMinutes(tokenService._jwtSetting.ExpirationMinutes), true, false, SameSiteMode.Lax);
                    },
                    () =>
                    {
                        tokenService.ClearTokenCookie(context, "AccessToken", secure: false, sameSite: SameSiteMode.Lax);
                        tokenService.ClearTokenCookie(context, "RefreshToken", secure: false, sameSite: SameSiteMode.Lax);
                    });
            })
            .WithOpenApi(operation =>
            {
                operation.Summary = "Làm mới Access Token";
                operation.Description = "Sử dụng Refresh Token để lấy một Access Token mới.";
                operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Auth" } };
                return operation;
            });
        }

        public static void ForgetPasswoord(this WebApplication app)
        {
            app.MapPost("/accounts/forget-password", async (CacheVerifyCodeService cacheVerifyCodeService, SendEmailService sendEmailService,
                HandleResultApi handleResultApi, [FromBody] string email, EmailValidatorService emailValidatorService) =>
            {
                var result = await emailValidatorService.CheckEmailValid(email).ConfigureAwait(false);
                if (!result.IsValid)
                {
                    return Results.BadRequest(result.Message);
                }

                try
                {
                    string verifyCode = cacheVerifyCodeService.GenerateVerifyCode(email);
                    await sendEmailService.SendVerifyCodeEmailAsync(email, verifyCode).ConfigureAwait(false);

                    return Results.Ok();
                }
                catch (Exception)
                {
                    return Results.Problem(
                         detail: "Có lỗi xảy ra trong quá trình xử lý yêu cầu. Vui lòng thử lại sau.",
                         statusCode: StatusCodes.Status500InternalServerError,
                         title: "Internal Server Error"
                     );
                }
            })
            .WithOpenApi(operation =>
            {
                operation.Summary = "Quên mật khẩu";
                operation.Description = "Gửi một mã xác thực đến email để người dùng có thể đặt lại mật khẩu.";
                operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Auth" } };
                return operation;
            });
        }

        public static void ChechVerifyCode(this WebApplication app)
        {
            app.MapPost("/accounts/forget-password/check-verify-code", async (CacheVerifyCodeService cacheVerifyCodeService, SendEmailService sendEmailService,
                HandleResultApi handleResultApi, [FromBody] VerifyRequest req, UpdateUserUC updateUserUC) =>
            {
                try
                {
                    if (cacheVerifyCodeService.VerifyCode(req.Code, req.Email))
                    {
                        ServiceResult<Account> result = await updateUserUC.UpdatePassword(req.Email, req.Password).ConfigureAwait(false);
                        return handleResultApi.MapServiceResultToHttpNoContent(result, null, null);
                    }
                    else
                    {
                        return Results.BadRequest("Mã xác thực không hợp lệ hoặc đã hết hạn.");
                    }
                }
                catch (Exception)
                {
                    return Results.Problem(
                         detail: "Có lỗi xảy ra trong quá trình xử lý yêu cầu. Vui lòng thử lại sau.",
                         statusCode: StatusCodes.Status500InternalServerError,
                         title: "Internal Server Error"
                     );
                }
            })
            .WithOpenApi(operation =>
            {
                operation.Summary = "Kiểm tra mã xác thực và đặt lại mật khẩu";
                operation.Description = "Kiểm tra mã xác thực và nếu hợp lệ, cho phép người dùng đặt lại mật khẩu mới.";
                operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Auth" } };
                return operation;
            });
        }

        public static void ResendCode(this WebApplication app)
        {
            app.MapPost("/accounts/forget-password/resend-code", async (CacheVerifyCodeService cacheVerifyCodeService, SendEmailService sendEmailService,
                HandleResultApi handleResultApi, [FromBody] string email) =>
            {
                try
                {
                    string verifyCode = cacheVerifyCodeService.GenerateVerifyCode(email);
                    await sendEmailService.SendVerifyCodeEmailAsync(email, verifyCode).ConfigureAwait(false);

                    return Results.Ok();
                }
                catch (Exception)
                {
                    return Results.Problem(
                         detail: "Có lỗi xảy ra trong quá trình xử lý yêu cầu. Vui lòng thử lại sau.",
                         statusCode: StatusCodes.Status500InternalServerError,
                         title: "Internal Server Error"
                     );
                }
            })
            .WithOpenApi(operation =>
            {
                operation.Summary = "Gửi lại mã xác thực";
                operation.Description = "Gửi lại mã xác thực đến email của người dùng.";
                operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Auth" } };
                return operation;
            });
        }
        #endregion

        #region Get User USECASE
        public static void MapGetUserUseCaseAPIs(this WebApplication app)
        {
            MapGetCustomerByID(app);
            MapGetAccountByID(app);
            MapGetCurrentCustomerInformation(app);
            MapGetStatus(app);
            MapGetCustomerInformationByCustomerID(app);
            MapGetCustomerInformationByAccountID(app);
            GetPagedAccount(app);
            test(app);
        }

        public static void GetPagedAccount(this WebApplication app)
        {
            app.MapGet("/accounts", async (GetUserUC getUserUC, int page, int pageSize, string? searchText, HandleResultApi handleResultApi) =>
            {
                ServiceResult<PagedResult<Account>> result = await getUserUC.GetPagedAccount(page, pageSize, searchText).ConfigureAwait(false);
                return handleResultApi.MapServiceResultToHttp(result);
            })
            .RequireAuthorization("OnlyAdmin")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Lấy danh sách tài khoản được phân trang";
                operation.Description = "Lấy danh sách tất cả các tài khoản với các tùy chọn phân trang và tìm kiếm. Yêu cầu quyền admin.";
                operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Users" } };
                return operation;
            });
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
            })
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "API thử nghiệm";
                operation.Description = "API này chỉ dùng cho mục đích thử nghiệm và không nên sử dụng trong môi trường sản xuất.";
                operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Testing" } };
                return operation;
            });
        }

        public static void MapGetCurrentCustomerInformation(this WebApplication app)
        {
            app.MapGet("/customers/me", async (GetUserUC getUserUC, HttpContext httpContext, TokenService tokenService, HandleResultApi handleResultApi) =>
            {
                int accountID = tokenService.GetJWTClaim(httpContext)?.AccountID ?? 0;

                ServiceResult<CustomerInformation> result = await getUserUC.GetCustomerInformationByAccountID(accountID).ConfigureAwait(false);
                return handleResultApi.MapServiceResultToHttp(result);

            })
            .RequireAuthorization("OnlyCustomer")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Lấy thông tin khách hàng hiện tại";
                operation.Description = "Lấy thông tin chi tiết của khách hàng đã đăng nhập.";
                operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Users" } };
                return operation;
            });
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

            })
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Lấy thông tin khách hàng theo ID (test)";
                operation.Description = "API này dùng để lấy thông tin khách hàng theo ID cho mục đích thử nghiệm và chỉ cho phép truy cập bởi admin hoặc chính khách hàng đó. Không nên sử dụng trong môi trường sản xuất.";
                operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Testing" } };
                return operation;
            });
        }


        public static void MapGetCustomerByID(this WebApplication app)
        {
            app.MapGet("/customers/{customerID}", async (GetUserUC getUserUC, int customerID, HttpContext httpContext, HandleResultApi handleResultApi) =>
            {
                ServiceResult<Customer> result = await getUserUC.GetCustomerByID(customerID).ConfigureAwait(false);
                return handleResultApi.MapServiceResultToHttp(result);
            })
            .RequireAuthorization("OnlyAdmin")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Lấy thông tin khách hàng theo ID";
                operation.Description = "Lấy thông tin của một khách hàng cụ thể dựa trên ID. Yêu cầu quyền admin.";
                operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Users" } };
                return operation;
            });
        }

        public static void MapGetCustomerInformationByCustomerID(this WebApplication app)
        {
            app.MapGet("/customers/{customerID}/customer-information", async (GetUserUC getUserUC, int customerID, HttpContext httpContext, HandleResultApi handleResultApi) =>
            {
                ServiceResult<CustomerInformation> result = await getUserUC.GetCustomerInformationByCustomerID(customerID).ConfigureAwait(false);
                return handleResultApi.MapServiceResultToHttp(result);
            })
            .RequireAuthorization("OnlyAdmin")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Lấy thông tin chi tiết khách hàng theo CustomerID";
                operation.Description = "Lấy thông tin chi tiết của khách hàng bao gồm cả thông tin tài khoản, dựa trên CustomerID. Yêu cầu quyền admin.";
                operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Users" } };
                return operation;
            });
        }

        public static void MapGetCustomerInformationByAccountID(this WebApplication app)
        {
            app.MapGet("/accounts/{accountID}/customer-information", async (GetUserUC getUserUC, int accountID, HttpContext httpContext, HandleResultApi handleResultApi) =>
            {
                ServiceResult<CustomerInformation> result = await getUserUC.GetCustomerInformationByAccountID(accountID).ConfigureAwait(false);
                return handleResultApi.MapServiceResultToHttp(result);
            })
            .RequireAuthorization("OnlyAdmin")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Lấy thông tin chi tiết khách hàng theo AccountID";
                operation.Description = "Lấy thông tin chi tiết của khách hàng bao gồm cả thông tin tài khoản, dựa trên AccountID. Yêu cầu quyền admin.";
                operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Users" } };
                return operation;
            });
        }

        public static void MapGetAccountByID(this WebApplication app)
        {
            app.MapGet("/accounts/{accountID}", async (GetUserUC getUserUC, HandleResultApi handleResultApi, HttpContext httpContext, int accountID) =>
            {
                ServiceResult<Account> result = await getUserUC.GetAccountByID(accountID).ConfigureAwait(false);
                return handleResultApi.MapServiceResultToHttp(result);
            })
            .RequireAuthorization("OnlyAdmin")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Lấy thông tin tài khoản theo ID";
                operation.Description = "Lấy thông tin của một tài khoản cụ thể dựa trên ID. Yêu cầu quyền admin.";
                operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Users" } };
                return operation;
            });
        }

        [Obsolete("This method is for testing purposes only and should not be used in production.")]
        public static void MapGetAllAccount(this WebApplication app)
        {
            app.MapGet("/accounts", async (GetUserUC getUserUC, HandleResultApi handleResultApi) =>
            {
                ServiceResult<Account> result = await getUserUC.GetAllAccount().ConfigureAwait(false);
                return handleResultApi.MapServiceResultToHttp(result);
            })
            .RequireAuthorization("OnlyAdmin")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Lấy tất cả tài khoản (thử nghiệm)";
                operation.Description = "Lấy tất cả các tài khoản. Chỉ dùng cho mục đích thử nghiệm và yêu cầu quyền admin.";
                operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Testing" } };
                return operation;
            });
        }

        [Obsolete("This method is for testing purposes only and should not be used in production.")]
        public static void MapGetAllUser(this WebApplication app)
        {
            app.MapGet("/customers", async (GetUserUC getUserUC, HandleResultApi handleResultApi) =>
            {
                ServiceResult<Customer> result = await getUserUC.GetAllCustomer().ConfigureAwait(false);
                return handleResultApi.MapServiceResultToHttp(result);
            })
            .RequireAuthorization("OnlyAdmin")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Lấy tất cả người dùng (thử nghiệm)";
                operation.Description = "Lấy tất cả các khách hàng. Chỉ dùng cho mục đích thử nghiệm và yêu cầu quyền admin.";
                operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Testing" } };
                return operation;
            });
        }

        public static void MapGetStatus(this WebApplication app)
        {
            app.MapGet("/auth/status", (HttpContext httpContext, TokenService tokenService) =>
            {
                JWTClaim jWTClaim = tokenService.GetJWTClaim(httpContext);
                return Results.Ok(jWTClaim.Role);
            })
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Lấy trạng thái xác thực";
                operation.Description = "Lấy vai trò của người dùng hiện tại đã được xác thực.";
                operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Auth" } };
                return operation;
            });
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
            })
            .RequireAuthorization("OnlyCustomer")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Cập nhật thông tin khách hàng";
                operation.Description = "Cập nhật thông tin cá nhân của khách hàng đã đăng nhập. Chỉ cho phép khách hàng cập nhật thông tin của chính mình.";
                operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Users" } };
                return operation;
            });
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
            })
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Cập nhật mật khẩu tài khoản";
                operation.Description = "Thay đổi mật khẩu cho tài khoản đã được xác thực. Yêu cầu mật khẩu cũ để xác thực.";
                operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Auth" } };
                return operation;
            });
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
            })
            .RequireAuthorization("OnlyAdmin")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Cập nhật trạng thái tài khoản";
                operation.Description = "Thay đổi trạng thái của một tài khoản (ví dụ: kích hoạt/vô hiệu hóa). Yêu cầu quyền admin.";
                operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Users" } };
                return operation;
            });
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
            })
            .WithOpenApi(operation =>
            {
                operation.Summary = "Đăng nhập tài khoản";
                operation.Description = "Đăng nhập bằng email và mật khẩu để nhận Access Token và Refresh Token.";
                operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Auth" } };
                return operation;
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
            })
            .WithOpenApi(operation =>
            {
                operation.Summary = "Đăng ký tài khoản mới";
                operation.Description = "Tạo một tài khoản khách hàng mới và tự động đăng nhập.";
                operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Auth" } };
                return operation;
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
            })
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Đăng xuất khỏi phiên hiện tại";
                operation.Description = "Đăng xuất khỏi phiên làm việc cụ thể đang sử dụng. Cần token xác thực.";
                operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Auth" } };
                return operation;
            });
        }
        #endregion 
    }
}
