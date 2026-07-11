using FluentValidation;

namespace TourPlanner.Application.UseCases.Tours.UploadTourImage;

public sealed class UploadTourImageRequestValidator : AbstractValidator<UploadTourImageRequest>
{
    public UploadTourImageRequestValidator()
    {
        RuleFor(x => x.TourId)
            .NotEmpty().WithMessage("Tour identifier is required.");

        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name cannot be empty.")
            .Must(name => name.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || 
                          name.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) || 
                          name.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Only image files (.jpg, .jpeg, .png) are supported.");

        RuleFor(x => x.FileContent)
            .NotEmpty().WithMessage("Image file content cannot be empty.");
    }
}