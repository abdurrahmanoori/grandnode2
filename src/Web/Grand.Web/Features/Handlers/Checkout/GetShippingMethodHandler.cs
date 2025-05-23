﻿using Grand.Business.Core.Interfaces.Catalog.Prices;
using Grand.Business.Core.Interfaces.Catalog.Tax;
using Grand.Business.Core.Interfaces.Checkout.Orders;
using Grand.Business.Core.Interfaces.Checkout.Shipping;
using Grand.Business.Core.Interfaces.Customers;
using Grand.Domain.Common;
using Grand.Domain.Customers;
using Grand.Domain.Shipping;
using Grand.Web.Features.Models.Checkout;
using Grand.Web.Models.Checkout;
using MediatR;

namespace Grand.Web.Features.Handlers.Checkout;

public class GetShippingMethodHandler : IRequestHandler<GetShippingMethod, CheckoutShippingMethodModel>
{
    private readonly IOrderCalculationService _orderTotalCalculationService;
    private readonly IPriceFormatter _priceFormatter;
    private readonly IShippingService _shippingService;
    private readonly ITaxService _taxService;
    private readonly ICustomerService _customerService;

    public GetShippingMethodHandler(IShippingService shippingService,
        ICustomerService customerService,
        IOrderCalculationService orderTotalCalculationService,
        ITaxService taxService,
        IPriceFormatter priceFormatter)
    {
        _shippingService = shippingService;
        _customerService = customerService;
        _orderTotalCalculationService = orderTotalCalculationService;
        _taxService = taxService;
        _priceFormatter = priceFormatter;
    }

    public async Task<CheckoutShippingMethodModel> Handle(GetShippingMethod request,
        CancellationToken cancellationToken)
    {
        var model = new CheckoutShippingMethodModel();

        var getShippingOptionResponse = await _shippingService
            .GetShippingOptions(request.Customer, request.Cart, request.ShippingAddress,
                "", request.Store);

        if (getShippingOptionResponse.Success)
        {
            //performance optimization. cache returned shipping options.
            //we'll use them later (after a customer has selected an option).
            await _customerService.UpdateUserField(request.Customer,
                SystemCustomerFieldNames.OfferedShippingOptions,
                getShippingOptionResponse.ShippingOptions,
                request.Store.Id);

            foreach (var shippingOption in getShippingOptionResponse.ShippingOptions)
            {
                var soModel = new CheckoutShippingMethodModel.ShippingMethodModel {
                    Name = shippingOption.Name,
                    Description = shippingOption.Description,
                    ShippingRateProviderSystemName = shippingOption.ShippingRateProviderSystemName,
                    ShippingOption = shippingOption
                };

                //adjust rate
                var shippingTotal = (await _orderTotalCalculationService.AdjustShippingRate(
                    shippingOption.Rate, request.Cart)).shippingRate;

                var rateBase = (await _taxService.GetShippingPrice(shippingTotal, request.Customer)).shippingPrice;
                soModel.Fee = _priceFormatter.FormatPrice(rateBase);

                model.ShippingMethods.Add(soModel);
            }

            //find a selected (previously) shipping method
            var selectedShippingOption =
                request.Customer.GetUserFieldFromEntity<ShippingOption>(SystemCustomerFieldNames.SelectedShippingOption,
                    request.Store.Id);
            if (selectedShippingOption != null)
            {
                var shippingOptionToSelect = model.ShippingMethods
                    .FirstOrDefault(so =>
                        !string.IsNullOrEmpty(so.Name) &&
                        so.Name.Equals(selectedShippingOption.Name, StringComparison.OrdinalIgnoreCase) &&
                        !string.IsNullOrEmpty(so.ShippingRateProviderSystemName) &&
                        so.ShippingRateProviderSystemName.Equals(selectedShippingOption.ShippingRateProviderSystemName,
                            StringComparison.OrdinalIgnoreCase));
                if (shippingOptionToSelect != null) shippingOptionToSelect.Selected = true;
            }

            //if no option has been selected, do it for the first one
            if (model.ShippingMethods.FirstOrDefault(so => so.Selected) != null) return model;
            {
                var shippingOptionToSelect = model.ShippingMethods.FirstOrDefault();
                if (shippingOptionToSelect != null) shippingOptionToSelect.Selected = true;
            }
        }
        else
        {
            foreach (var error in getShippingOptionResponse.Errors)
                model.Warnings.Add(error);
        }

        return model;
    }
}