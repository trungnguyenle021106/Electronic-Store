using Microsoft.AspNetCore.Mvc;
using UserService.Application.Usecases;
using UserService.Domain.Entities;
using UserService.Infrastructure.DBContext;

namespace UserService.Interface_Adapters.APIs
{
    public static class UserAPI
    {
        public static void MapUserEndpoints(this WebApplication app)
        {
            MapCreateUserUseCaseAPIs(app);
            MapGetUserUseCaseAPIs(app);
            MapUpdateUserUseCaseAPIs(app);
        }

        #region Create User USECASE
        public static void MapCreateUserUseCaseAPIs(this WebApplication app)
        {
            MapCreateAccount(app);
            MapCreateCustomer(app);
        }

        public static void MapCreateAccount(this WebApplication app)
        {
            app.MapPost("/accounts", async (UserContext userContext, [FromBody] Account newAccount) =>
            {
                return Results.Ok(new CreateUserUC(userContext).CreateAccount(newAccount));
            });
        }

        public static void MapCreateCustomer(this WebApplication app)
        {
            app.MapPost("/customers", async (UserContext userContext, [FromBody] Customer newCustomer) =>
            {
                return Results.Ok(await new CreateUserUC(userContext).CreateCustomer(newCustomer));
            });
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
            app.MapGet("/customers/{customerID}", async (UserContext userContext, int customerID) =>
            {
                return Results.Ok(await new GetUserUC(userContext).GetCustomerByID(customerID));
            });
        }

        public static void MapGetAccountByID(this WebApplication app)
        {
            app.MapGet("/accounts/{accountID}", async (UserContext userContext, int accountID) =>
            {
                return Results.Ok(await new GetUserUC(userContext).GetAccountByID(accountID));
            });
        }

        public static void MapGetAccount(this WebApplication app)
        {
            app.MapGet("/accounts", async (UserContext userContext) =>
            {
                return Results.Ok(await new GetUserUC(userContext).GetAllAccount());
            });
        }

        public static void MapGetUser(this WebApplication app)
        {
            app.MapGet("/customers", async (UserContext userContext) =>
            {
                return Results.Ok(await new GetUserUC(userContext).GetAllAccount());
            });
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
            app.MapPatch("/customers/{customerID}", async (UserContext context, 
                int customerID, Customer newCustomer) =>
            {
                return Results.Ok(await new UpdateUserUC(context).
                    UpdateCustomerInformation(customerID, newCustomer));
            });
        }

        public static void MapUpdateAccount(this WebApplication app)
        {
            app.MapPatch("/accounts/{accountID}", async (UserContext context,
                int accountID, Account newAccount) =>
            {
                return Results.Ok(await new UpdateUserUC(context).
                    UpdateAccount(accountID, newAccount));
            });
        }
        #endregion
    }
}
