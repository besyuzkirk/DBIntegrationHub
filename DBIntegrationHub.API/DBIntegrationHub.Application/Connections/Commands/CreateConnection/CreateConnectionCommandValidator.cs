using FluentValidation;

namespace DBIntegrationHub.Application.Connections.Commands.CreateConnection;

public class CreateConnectionCommandValidator : AbstractValidator<CreateConnectionCommand>
{
    public CreateConnectionCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Connection adı boş olamaz")
            .MaximumLength(100).WithMessage("Connection adı en fazla 100 karakter olabilir");

        RuleFor(x => x.ConnectionString)
            .NotEmpty().WithMessage("Connection string boş olamaz")
            .MaximumLength(500).WithMessage("Connection string en fazla 500 karakter olabilir");

        RuleFor(x => x.DatabaseType)
            .NotEmpty().WithMessage("Veritabanı tipi boş olamaz")
            .Must(BeValidDatabaseType).WithMessage("Geçerli bir veritabanı tipi seçiniz (PostgreSQL, MySQL, SQLServer, MongoDB)");
    }

    private bool BeValidDatabaseType(string databaseType)
    {
        var validTypes = new[] { "PostgreSQL", "MySQL", "SQLServer", "MongoDB" };
        return validTypes.Contains(databaseType, StringComparer.OrdinalIgnoreCase);
    }
}

