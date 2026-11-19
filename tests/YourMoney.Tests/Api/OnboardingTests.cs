using System.ComponentModel.DataAnnotations;
using YourMoney.Api.Models;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Api;

public static class OnboardingTests
{
    public static Task RegistrationRequiresSupportedGoalAndSource()
    {
        var model = new UsuarioRegistro
        {
            Nome = "Maria",
            Email = "maria@example.com",
            Senha = "Senha123",
            SenhaConfirmacao = "Senha123",
            ObjetivoInicial = "gastos",
            OrigemConhecimento = "instagram"
        };
        TestAssert.True(IsValid(model), "Supported onboarding choices should be accepted");
        model.ObjetivoInicial = "desconhecido";
        TestAssert.True(!IsValid(model), "Unknown goal should be rejected");
        model.ObjetivoInicial = "cartao";
        model.OrigemConhecimento = "origem-forjada";
        TestAssert.True(!IsValid(model), "Unknown referral source should be rejected");
        model.OrigemConhecimento = null;
        TestAssert.True(IsValid(model), "Referral source remains optional for existing API clients");
        model.ObjetivoInicial = "gastos";
        TestAssert.True(IsValid(model), "Existing clients can use the default goal");
        return Task.CompletedTask;
    }

    private static bool IsValid(UsuarioRegistro model) => Validator.TryValidateObject(
        model, new ValidationContext(model), new List<ValidationResult>(), validateAllProperties: true);
}
