﻿using FluentValidation;
using Grand.Module.Api.DTOs.Catalog;
using Grand.Business.Core.Interfaces.Catalog.Products;
using Grand.Business.Core.Interfaces.Common.Localization;
using Grand.Infrastructure.Validators;

namespace Grand.Module.Api.Validators.Catalog;

public class ProductAttributeValidator : BaseGrandValidator<ProductAttributeDto>
{
    public ProductAttributeValidator(IEnumerable<IValidatorConsumer<ProductAttributeDto>> validators,
        ITranslationService translationService, IProductAttributeService productAttributeService)
        : base(validators)
    {
        RuleFor(x => x.Name).NotEmpty()
            .WithMessage(translationService.GetResource("Api.Catalog.ProductAttribute.Fields.Name.Required"));
        RuleFor(x => x).MustAsync(async (x, _, _) =>
        {
            if (!string.IsNullOrEmpty(x.Id))
            {
                var pa = await productAttributeService.GetProductAttributeById(x.Id);
                if (pa == null)
                    return false;
            }

            return true;
        }).WithMessage(translationService.GetResource("Api.Catalog.ProductAttribute.Fields.Id.NotExists"));
        RuleFor(x => x).Must((x, _) =>
        {
            return x.PredefinedProductAttributeValues.All(item => !string.IsNullOrEmpty(item.Name));
        }).WithMessage(
            translationService.GetResource("Api.Catalog.PredefinedProductAttributeValue.Fields.Name.Required"));
    }
}