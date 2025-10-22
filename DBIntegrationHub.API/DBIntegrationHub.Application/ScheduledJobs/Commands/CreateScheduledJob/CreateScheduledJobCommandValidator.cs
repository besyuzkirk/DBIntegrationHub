using FluentValidation;

namespace DBIntegrationHub.Application.ScheduledJobs.Commands.CreateScheduledJob;

public class CreateScheduledJobCommandValidator : AbstractValidator<CreateScheduledJobCommand>
{
    public CreateScheduledJobCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("İş adı boş olamaz.")
            .MaximumLength(200).WithMessage("İş adı en fazla 200 karakter olabilir.");

        RuleFor(x => x.CronExpression)
            .NotEmpty().WithMessage("Cron ifadesi boş olamaz.")
            .MaximumLength(100).WithMessage("Cron ifadesi en fazla 100 karakter olabilir.");
    }
}

