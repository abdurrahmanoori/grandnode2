﻿using Grand.Business.Common.Services.Addresses;
using Grand.Business.Common.Services.Configuration;
using Grand.Business.Common.Services.Directory;
using Grand.Business.Common.Services.ExportImport;
using Grand.Business.Common.Services.Localization;
using Grand.Business.Common.Services.Pdf;
using Grand.Business.Common.Services.Security;
using Grand.Business.Common.Services.Seo;
using Grand.Business.Common.Services.Stores;
using Grand.Business.Core.Dto;
using Grand.Business.Core.Interfaces.Common.Addresses;
using Grand.Business.Core.Interfaces.Common.Configuration;
using Grand.Business.Core.Interfaces.Common.Directory;
using Grand.Business.Core.Interfaces.Common.Localization;
using Grand.Business.Core.Interfaces.Common.Pdf;
using Grand.Business.Core.Interfaces.Common.Security;
using Grand.Business.Core.Interfaces.Common.Seo;
using Grand.Business.Core.Interfaces.Common.Stores;
using Grand.Business.Core.Interfaces.ExportImport;
using Grand.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Grand.Business.Common.Startup;

public class StartupApplication : IStartupApplication
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        RegisterCommonService(services);
        RegisterDirectoryService(services);
        RegisterConfigurationService(services);
        RegisterLocalizationService(services);
        RegisterSecurityService(services);
        RegisterSeoService(services);
        RegisterStoresService(services);
        RegisterExportImportService(services);
    }

    public void Configure(WebApplication application, IWebHostEnvironment webHostEnvironment)
    {
    }

    public int Priority => 100;
    public bool BeforeConfigure => false;

    private static void RegisterCommonService(IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IAddressAttributeParser, AddressAttributeParser>();
        serviceCollection.AddScoped<IAddressAttributeService, AddressAttributeService>();
        serviceCollection.AddScoped<IHistoryService, HistoryService>();
        serviceCollection.AddScoped<IPdfService, HtmlToPdfService>();
    }

    private static void RegisterDirectoryService(IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IDateTimeService, DateTimeService>();

        serviceCollection.AddScoped<ICountryService, CountryService>();
        serviceCollection.AddScoped<ICurrencyService, CurrencyService>();
        serviceCollection.AddScoped<IExchangeRateService, ExchangeRateService>();
        serviceCollection.AddScoped<IGroupService, GroupService>();
    }

    private static void RegisterConfigurationService(IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<ISettingService, SettingService>();
    }

    private static void RegisterLocalizationService(IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<ITranslationService, TranslationService>();
        serviceCollection.AddScoped<ILanguageService, LanguageService>();
        serviceCollection.AddScoped<IPluginTranslateResource, PluginTranslateResource>();
    }

    private static void RegisterSecurityService(IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IPermissionService, PermissionService>();
        serviceCollection.AddScoped<IAclService, AclService>();
        serviceCollection.AddScoped<IEncryptionService, EncryptionService>();
        serviceCollection.AddScoped<IPermissionProvider, PermissionProvider>();
    }

    private static void RegisterSeoService(IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<ISlugService, SlugService>();
        serviceCollection.AddScoped<ISeNameService, SeNameService>();
    }


    private static void RegisterStoresService(IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IStoreService, StoreService>();
    }

    private static void RegisterExportImportService(IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<ISchemaProperty<CountryStatesDto>, CountrySchemaProperty>();

        serviceCollection.AddScoped<IExportProvider, ExcelExportProvider>();
        serviceCollection.AddScoped(typeof(IExportManager<>), typeof(ExportManager<>));

        serviceCollection.AddScoped<IImportDataProvider, ExcelImportProvider>();
        serviceCollection.AddScoped(typeof(IImportManager<>), typeof(ImportManager<>));

        serviceCollection.AddScoped<IImportDataObject<CountryStatesDto>, CountryImportDataObject>();
    }
}