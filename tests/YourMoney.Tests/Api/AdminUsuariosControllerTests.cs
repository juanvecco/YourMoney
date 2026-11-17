using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using YourMoney.Api.Controllers;
using YourMoney.Infrastructure.Data;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Api;

public static class AdminUsuariosControllerTests
{
    public static async Task ListsNamesAndEmailsWithoutPasswordHashesForAdminsOnly()
    {
        var authorization = (AuthorizeAttribute?)typeof(AdminUsuariosController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), true).SingleOrDefault();
        TestAssert.Equal("Admin", authorization?.Roles, "The endpoint must require the administrator role");

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var db = new ApplicationDbContext(options);
        db.Users.AddRange(
            new IdentityUser { Id = "1", Email = "ana@example.com", NormalizedEmail = "ANA@EXAMPLE.COM", PasswordHash = "private-hash-ana" },
            new IdentityUser { Id = "2", Email = "bia@example.com", NormalizedEmail = "BIA@EXAMPLE.COM", PasswordHash = "private-hash-bia" });
        db.UserClaims.Add(new IdentityUserClaim<string> { UserId = "1", ClaimType = "name", ClaimValue = "Ana" });
        await db.SaveChangesAsync();

        var controller = new AdminUsuariosController(db, new ConfigurationBuilder().Build())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        var action = await controller.Listar(pagina: 1, tamanho: 1);
        var page = (PaginaUsuariosAdmin)((OkObjectResult)action.Result!).Value!;
        TestAssert.Equal(2, page.Total, "The server should count all users before paging");
        TestAssert.Equal(1, page.Itens.Count, "The endpoint should page the response");
        TestAssert.Equal("Ana", page.Itens[0].Nome, "The registered name should be included");
        TestAssert.Equal("ana@example.com", page.Itens[0].Email, "The registered email should be included");
        TestAssert.True(!JsonSerializer.Serialize(page).Contains("private-hash"), "Password hashes must never be returned");
        TestAssert.Equal("no-store", controller.Response.Headers.CacheControl.ToString(), "The list must not be cached");

        var invalid = await controller.Listar(pagina: 0);
        TestAssert.True(invalid.Result is BadRequestObjectResult, "Invalid pages should be rejected");
    }
}
