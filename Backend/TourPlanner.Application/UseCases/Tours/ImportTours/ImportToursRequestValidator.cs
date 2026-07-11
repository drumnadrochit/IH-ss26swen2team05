using FluentValidation;

namespace TourPlanner.Application.UseCases.Tours.ImportTours;

public class ImportToursRequestValidator : AbstractValidator<ImportToursRequest> {
    public ImportToursRequestValidator() {
        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required.")
            .Must(name => name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Only JSON files (.json) are supported for importing configuration snapshots.");
        
        RuleFor(x => x.Content)
            .NotNull()
            .WithMessage("An import file must be provided.")
            .Must(bytes => bytes is { Length: > 0 })
            .WithMessage("The imported file cannot be empty.")
            .Must(bytes => bytes is { Length: < 10 * 1024 * 1024 })
            .WithMessage("The imported file exceeds the maximum allowed size of 10MB.");
    }
}