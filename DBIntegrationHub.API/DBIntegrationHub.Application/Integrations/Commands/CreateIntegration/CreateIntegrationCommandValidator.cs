using FluentValidation;

namespace DBIntegrationHub.Application.Integrations.Commands.CreateIntegration;

public class CreateIntegrationCommandValidator : AbstractValidator<CreateIntegrationCommand>
{
    public CreateIntegrationCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Integration adı boş olamaz")
            .MaximumLength(200).WithMessage("Integration adı en fazla 200 karakter olabilir");

        RuleFor(x => x.SourceConnectionId)
            .NotEmpty().WithMessage("Kaynak bağlantı seçilmelidir");

        RuleFor(x => x.TargetConnectionId)
            .NotEmpty().WithMessage("Hedef bağlantı seçilmelidir");

        RuleFor(x => x.SourceQuery)
            .NotEmpty().WithMessage("Kaynak sorgu boş olamaz")
            .Must(BeValidSourceQuery).WithMessage("Kaynak sorgu SELECT ile başlamalıdır");

        RuleFor(x => x.TargetQuery)
            .NotEmpty().WithMessage("Hedef sorgu boş olamaz")
            .Must(BeValidTargetQuery).WithMessage("Hedef sorgu INSERT veya UPDATE içermelidir");
    }

    private bool BeValidSourceQuery(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return false;

        var trimmedQuery = query.TrimStart().ToUpper();
        return trimmedQuery.StartsWith("SELECT");
    }

    private bool BeValidTargetQuery(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return false;

        var upperQuery = query.ToUpper();
        return upperQuery.Contains("INSERT") || upperQuery.Contains("UPDATE");
    }
}

