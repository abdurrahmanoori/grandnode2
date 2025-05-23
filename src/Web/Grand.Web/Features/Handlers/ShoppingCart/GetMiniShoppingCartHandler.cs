﻿using Grand.Business.Core.Commands.Checkout.Orders;
using Grand.Business.Core.Extensions;
using Grand.Business.Core.Interfaces.Catalog.Prices;
using Grand.Business.Core.Interfaces.Catalog.Products;
using Grand.Business.Core.Interfaces.Catalog.Tax;
using Grand.Business.Core.Interfaces.Checkout.CheckoutAttributes;
using Grand.Business.Core.Interfaces.Checkout.Orders;
using Grand.Business.Core.Interfaces.Common.Directory;
using Grand.Business.Core.Interfaces.Common.Localization;
using Grand.Business.Core.Interfaces.Storage;
using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Domain.Media;
using Grand.Domain.Orders;
using Grand.Domain.Tax;
using Grand.Web.Extensions;
using Grand.Web.Features.Models.ShoppingCart;
using Grand.Web.Models.Media;
using Grand.Web.Models.ShoppingCart;
using MediatR;

namespace Grand.Web.Features.Handlers.ShoppingCart;

public class GetMiniShoppingCartHandler : IRequestHandler<GetMiniShoppingCart, MiniShoppingCartModel>
{
    private readonly ICheckoutAttributeService _checkoutAttributeService;
    private readonly IGroupService _groupService;
    private readonly LinkGenerator _linkGenerator;
    private readonly MediaSettings _mediaSettings;
    private readonly IMediator _mediator;
    private readonly OrderSettings _orderSettings;
    private readonly IOrderCalculationService _orderTotalCalculationService;
    private readonly IPictureService _pictureService;
    private readonly IPriceFormatter _priceFormatter;
    private readonly IPricingService _pricingService;
    private readonly IProductAttributeFormatter _productAttributeFormatter;
    private readonly IProductService _productService;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly ShoppingCartSettings _shoppingCartSettings;
    private readonly ITaxService _taxService;
    private readonly TaxSettings _taxSettings;
    private readonly ITranslationService _translationService;

    public GetMiniShoppingCartHandler(
        IShoppingCartService shoppingCartService,
        IOrderCalculationService orderTotalCalculationService,
        IPriceFormatter priceFormatter,
        ITaxService taxService,
        ICheckoutAttributeService checkoutAttributeService,
        IProductService productService,
        IProductAttributeFormatter productAttributeFormatter,
        ITranslationService translationService,
        IPricingService priceCalculationService,
        IGroupService groupService,
        IPictureService pictureService,
        IMediator mediator,
        LinkGenerator linkGenerator,
        ShoppingCartSettings shoppingCartSettings,
        OrderSettings orderSettings,
        TaxSettings taxSettings,
        MediaSettings mediaSettings)
    {
        _shoppingCartService = shoppingCartService;
        _orderTotalCalculationService = orderTotalCalculationService;
        _priceFormatter = priceFormatter;
        _taxService = taxService;
        _checkoutAttributeService = checkoutAttributeService;
        _productService = productService;
        _productAttributeFormatter = productAttributeFormatter;
        _translationService = translationService;
        _pricingService = priceCalculationService;
        _groupService = groupService;
        _pictureService = pictureService;
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _shoppingCartSettings = shoppingCartSettings;
        _orderSettings = orderSettings;
        _taxSettings = taxSettings;
        _mediaSettings = mediaSettings;
    }

    public async Task<MiniShoppingCartModel> Handle(GetMiniShoppingCart request,
        CancellationToken cancellationToken)
    {
        var model = new MiniShoppingCartModel {
            ShowProductImages = _shoppingCartSettings.ShowImagesInsidebarCart,
            DisplayShoppingCartButton = true,
            CurrentCustomerIsGuest = await _groupService.IsGuest(request.Customer),
            AnonymousCheckoutAllowed = _orderSettings.AnonymousCheckoutAllowed
        };

        if (!request.Customer.ShoppingCartItems.Any()) return model;

        var shoppingCartTypes = new List<ShoppingCartType> {
            ShoppingCartType.ShoppingCart,
            ShoppingCartType.Auctions
        };
        if (_shoppingCartSettings.AllowOnHoldCart)
            shoppingCartTypes.Add(ShoppingCartType.OnHoldCart);

        var cart = await _shoppingCartService.GetShoppingCart(request.Store.Id, shoppingCartTypes.ToArray());
        model.TotalProducts = cart.Sum(x => x.Quantity);
        if (!cart.Any()) return model;

        //subtotal
        var subTotalIncludingTax = request.TaxDisplayType == TaxDisplayType.IncludingTax &&
                                   !_taxSettings.ForceTaxExclusionFromOrderSubtotal;
        var shoppingCartSubTotal =
            await _orderTotalCalculationService.GetShoppingCartSubTotal(cart, subTotalIncludingTax);

        model.SubTotalIncludingTax = subTotalIncludingTax;
        model.SubTotal = _priceFormatter.FormatPrice(shoppingCartSubTotal.subTotalWithoutDiscount, request.Currency);

        var requiresShipping = cart.RequiresShipping();
        var checkoutAttributesExist =
            (await _checkoutAttributeService.GetAllCheckoutAttributes(request.Store.Id, !requiresShipping))
            .Any();

        var minOrderSubtotalAmountOk = await _mediator.Send(new ValidateMinShoppingCartSubtotalAmountCommand {
            Customer = request.Customer,
            Cart = cart.Where
                (x => x.ShoppingCartTypeId is ShoppingCartType.ShoppingCart or ShoppingCartType.Auctions).ToList()
        }, cancellationToken);

        model.DisplayCheckoutButton = !_orderSettings.TermsOfServiceOnShoppingCartPage &&
                                      minOrderSubtotalAmountOk &&
                                      !checkoutAttributesExist;

        foreach (var sci in cart
                     .OrderByDescending(x => x.Id)
                     .Take(_shoppingCartSettings.MiniCartProductNumber)
                     .ToList())
        {
            var product = await _productService.GetProductById(sci.ProductId);
            if (product == null)
                continue;

            var sename = product.GetSeName(request.Language.Id);
            var cartItemModel = new MiniShoppingCartModel.ShoppingCartItemModel {
                Id = sci.Id,
                ProductId = product.Id,
                ProductName = product.GetTranslation(x => x.Name, request.Language.Id),
                ProductSeName = sename,
                ProductUrl = _linkGenerator.GetPathByRouteValues("Product", new { SeName = sename }),
                Quantity = sci.Quantity,
                AttributeInfo = await _productAttributeFormatter.FormatAttributes(product, sci.Attributes)
            };
            if (product.ProductTypeId == ProductType.Reservation)
            {
                var reservation = "";
                if (sci.RentalEndDateUtc == default(DateTime) || sci.RentalEndDateUtc == null)
                    reservation =
                        string.Format(_translationService.GetResource("ShoppingCart.Reservation.StartDate"),
                            sci.RentalStartDateUtc?.ToString(_shoppingCartSettings.ReservationDateFormat));
                else
                    reservation = string.Format(
                        _translationService.GetResource("ShoppingCart.Reservation.Date"),
                        sci.RentalStartDateUtc?.ToString(_shoppingCartSettings.ReservationDateFormat),
                        sci.RentalEndDateUtc?.ToString(_shoppingCartSettings.ReservationDateFormat));

                if (!string.IsNullOrEmpty(sci.Parameter))
                    reservation += "<br>" +
                                   string.Format(
                                       _translationService.GetResource("ShoppingCart.Reservation.Option"),
                                       sci.Parameter);

                if (!string.IsNullOrEmpty(sci.Duration))
                    reservation += "<br>" +
                                   string.Format(
                                       _translationService.GetResource("ShoppingCart.Reservation.Duration"),
                                       sci.Duration);

                if (string.IsNullOrEmpty(cartItemModel.AttributeInfo))
                    cartItemModel.AttributeInfo = reservation;
                else
                    cartItemModel.AttributeInfo += "<br>" + reservation;
            }

            //unit prices
            if (product.CallForPrice)
            {
                cartItemModel.UnitPrice = _translationService.GetResource("Products.CallForPrice");
            }
            else
            {
                var productprices = await _taxService.GetProductPrice(product,
                    (await _pricingService.GetUnitPrice(sci, product)).unitprice);
                cartItemModel.UnitPrice = _priceFormatter.FormatPrice(productprices.productprice);
            }

            //picture
            if (_shoppingCartSettings.ShowImagesInsidebarCart)
                cartItemModel.Picture = await PrepareCartItemPicture(product, sci.Attributes);

            model.Items.Add(cartItemModel);
        }

        return model;
    }

    private async Task<PictureModel> PrepareCartItemPicture(Product product, IList<CustomAttribute> attributes)
    {
        var sciPicture = await product.GetProductPicture(attributes, _productService, _pictureService);
        return new PictureModel {
            Id = sciPicture?.Id,
            ImageUrl = await _pictureService.GetPictureUrl(sciPicture, _mediaSettings.MiniCartThumbPictureSize),
            Title = string.Format(_translationService.GetResource("Media.Product.ImageLinkTitleFormat"),
                product.Name),
            AlternateText = string.Format(_translationService.GetResource("Media.Product.ImageAlternateTextFormat"),
                product.Name)
        };
    }
}