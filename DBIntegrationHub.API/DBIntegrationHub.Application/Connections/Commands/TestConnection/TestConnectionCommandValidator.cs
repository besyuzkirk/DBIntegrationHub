using FluentValidation;

namespace DBIntegrationHub.Application.Connections.Commands.TestConnection;

public class TestConnectionCommandValidator : AbstractValidator<TestConnectionCommand>
{
    public TestConnectionCommandValidator()
    {
        RuleFor(x => x.DatabaseType)
            .NotEmpty().WithMessage("Veritabanı tipi boş olamaz")
            .Must(BeValidDatabaseType).WithMessage("Geçerli bir veritabanı tipi seçiniz (PostgreSQL, MySQL, SQLServer, MongoDB)");

        RuleFor(x => x.ConnectionString)
            .NotEmpty().WithMessage("Connection string boş olamaz");
    }

    private bool BeValidDatabaseType(string databaseType)
    {
        var validTypes = new[] { "PostgreSQL", "MySQL", "SQLServer", "MongoDB" };
        return validTypes.Contains(databaseType, StringComparer.OrdinalIgnoreCase);
    }
}

