using System.ComponentModel.DataAnnotations;
using YourMoney.Api.Models;
using YourMoney.Api.Services;
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
        model.ObjetivosIniciais = ["cartao", "gastos"];
        TestAssert.True(IsValid(model), "Multiple goals are accepted in registration");
        TestAssert.True(OnboardingGoals.Resolve(model.ObjetivosIniciais, model.ObjetivoInicial)!.SequenceEqual(["cartao", "gastos"]),
            "All selected goals should be retained in order");
        TestAssert.True(OnboardingGoals.Resolve(["cartao", "invalido"], "gastos") == null,
            "Unknown goals must not be silently discarded");
        TestAssert.True(OnboardingGoals.Resolve(null, "gastos")!.SequenceEqual(["gastos"]),
            "Existing clients still use their single goal");
        return Task.CompletedTask;
    }

    private static bool IsValid(UsuarioRegistro model) => Validator.TryValidateObject(
        model, new ValidationContext(model), new List<ValidationResult>(), validateAllProperties: true);
}
