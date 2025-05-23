using Grand.Domain;
using Grand.Domain.Common;
using Grand.Domain.Customers;
using Grand.Domain.Orders;
using System.Linq.Expressions;

namespace Grand.Business.Core.Interfaces.Customers;

/// <summary>
///     Customer service interface
/// </summary>
public interface ICustomerService
{
    #region Customers

    /// <summary>
    ///     Gets all customers
    /// </summary>
    /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
    /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
    /// <param name="affiliateId">Affiliate identifier</param>
    /// <param name="vendorId">Vendor identifier</param>
    /// <param name="storeId">Store identifier</param>
    /// <param name="ownerId">Owner identifier</param>
    /// <param name="salesEmployeeId">Sales employee identifier</param>
    /// <param name="customerGroupIds">
    ///     A list of customer group identifiers to filter by (at least one match); pass null or
    ///     empty list in order to load all customers;
    /// </param>
    /// <param name="customerTagIds"></param>
    /// <param name="email">Email; null to load all customers</param>
    /// <param name="username">Username; null to load all customers</param>
    /// <param name="firstName">First name; null to load all customers</param>
    /// <param name="lastName">Last name; null to load all customers</param>
    /// <param name="company">Company; null to load all customers</param>
    /// <param name="phone">Phone; null to load all customers</param>
    /// <param name="zipPostalCode">Phone; null to load all customers</param>
    /// <param name="loadOnlyWithShoppingCart">Value indicating whether to load customers only with shopping cart</param>
    /// <param name="sct">
    ///     Value indicating what shopping cart type to filter; user when 'loadOnlyWithShoppingCart' param is
    ///     'true'
    /// </param>
    /// <param name="pageIndex">Page index</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="orderBySelector"></param>
    /// <returns>Customers</returns>
    Task<IPagedList<Customer>> GetAllCustomers(DateTime? createdFromUtc = null,
        DateTime? createdToUtc = null, string affiliateId = "", string vendorId = "", string storeId = "",
        string ownerId = "",
        string salesEmployeeId = "", string[] customerGroupIds = null, string[] customerTagIds = null,
        string email = null, string username = null,
        string firstName = null, string lastName = null,
        string company = null, string phone = null, string zipPostalCode = null,
        bool loadOnlyWithShoppingCart = false, ShoppingCartType? sct = null,
        int pageIndex = 0, int pageSize = int.MaxValue, Expression<Func<Customer, object>> orderBySelector = null);


    /// <summary>
    ///     Gets online customers
    /// </summary>
    /// <param name="lastActivityFromUtc">Customer last activity date (from)</param>
    /// <param name="customerGroupIds">
    ///     A list of customer group identifiers to filter by (at least one match); pass null or
    ///     empty list in order to load all customers;
    /// </param>
    /// <param name="storeId">Store ident</param>
    /// <param name="salesEmployeeId">Sales employee ident</param>
    /// <param name="pageIndex">Page index</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Customers</returns>
    Task<IPagedList<Customer>> GetOnlineCustomers(DateTime lastActivityFromUtc,
        string[] customerGroupIds, string storeId = "", string salesEmployeeId = "", int pageIndex = 0,
        int pageSize = int.MaxValue);

    /// <summary>
    ///     Gets count online customers
    /// </summary>
    /// <param name="lastActivityFromUtc">Customer last activity date (from)</param>
    /// <param name="storeId">Store ident</param>
    /// <param name="salesEmployeeId">Sales employee ident</param>
    /// <returns>Int</returns>
    Task<int> GetCountOnlineShoppingCart(DateTime lastActivityFromUtc, string storeId = "",
        string salesEmployeeId = "");

    /// <summary>
    ///     Gets a customer
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    /// <returns>A customer</returns>
    Task<Customer> GetCustomerById(string customerId);

    /// <summary>
    ///     Get customers by identifiers
    /// </summary>
    /// <param name="customerIds">Customer identifiers</param>
    /// <returns>Customers</returns>
    Task<IList<Customer>> GetCustomersByIds(string[] customerIds);

    /// <summary>
    ///     Gets a customer by GUID
    /// </summary>
    /// <param name="customerGuid">Customer GUID</param>
    /// <returns>A customer</returns>
    Task<Customer> GetCustomerByGuid(Guid customerGuid);

    /// <summary>
    ///     Get customer by email
    /// </summary>
    /// <param name="email">Email</param>
    /// <returns>Customer</returns>
    Task<Customer> GetCustomerByEmail(string email);

    /// <summary>
    ///     Get customer by system group
    /// </summary>
    /// <param name="systemName">System name</param>
    /// <returns>Customer</returns>
    Task<Customer> GetCustomerBySystemName(string systemName);

    /// <summary>
    ///     Get customer by username
    /// </summary>
    /// <param name="username">Username</param>
    /// <returns>Customer</returns>
    Task<Customer> GetCustomerByUsername(string username);

    /// <summary>
    ///     Insert a guest customer
    /// </summary>
    /// <returns>Customer</returns>
    Task<Customer> InsertGuestCustomer(Customer customer);

    /// <summary>
    ///     Insert a customer
    /// </summary>
    /// <param name="customer">Customer</param>
    Task InsertCustomer(Customer customer);

    /// <summary>
    ///     Updates the customer
    /// </summary>
    /// <param name="customer">Customer</param>
    Task UpdateCustomer(Customer customer);

    /// <summary>
    ///    Updates the customer user field
    /// </summary>
    /// <param name="customer"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="storeId"></param>
    /// <typeparam name="TPropType"></typeparam>
    /// <returns></returns>
    Task UpdateUserField<TPropType>(Customer customer, string key, TPropType value, string storeId = "");
    
    /// <summary>
    ///     Delete a customer
    /// </summary>
    /// <param name="customer">Customer</param>
    /// <param name="hard">Hard delete from database</param>
    Task DeleteCustomer(Customer customer, bool hard = false);

    /// <summary>
    ///     Updates the customer field
    /// </summary>
    /// <param name="customer">Customer</param>
    /// <param name="expression"></param>
    /// <param name="value"></param>
    Task UpdateCustomerField<T>(Customer customer,
        Expression<Func<Customer, T>> expression, T value);

    /// <summary>
    ///     Updates the customer field
    /// </summary>
    /// <param name="customerId">Customer ident</param>
    /// <param name="expression"></param>
    /// <param name="value"></param>
    Task UpdateCustomerField<T>(string customerId,
        Expression<Func<Customer, T>> expression, T value);

    /// <summary>
    ///     Updates the customer
    /// </summary>
    /// <param name="customer">Customer</param>
    Task UpdateActive(Customer customer);

    /// <summary>
    ///     Update the customer
    /// </summary>
    /// <param name="customer"></param>
    Task UpdateContributions(Customer customer);

    /// <summary>
    ///     Updates the customer
    /// </summary>
    /// <param name="customer">Customer</param>
    Task UpdateCustomerLastLoginDate(Customer customer);

    /// <summary>
    ///     Updates the customer in admin panel
    /// </summary>
    /// <param name="customer">Customer</param>
    Task UpdateCustomerInAdminPanel(Customer customer);

    /// <summary>
    ///     Reset data required for checkout
    /// </summary>
    /// <param name="customer">Customer</param>
    /// <param name="storeId">Store identifier</param>
    /// <param name="clearCouponCodes">A value indicating whether to clear coupon code</param>
    /// <param name="clearCheckoutAttributes">A value indicating whether to clear selected checkout attributes</param>
    /// <param name="clearLoyaltyPoints">A value indicating whether to clear "Use loyalty points" flag</param>
    /// <param name="clearShipping">A value indicating whether to clear selected shipping method</param>
    /// <param name="clearPayment">A value indicating whether to clear selected payment method</param>
    Task ResetCheckoutData(Customer customer, string storeId,
        bool clearCouponCodes = false, bool clearCheckoutAttributes = false,
        bool clearLoyaltyPoints = true, bool clearShipping = true, bool clearPayment = true);

    /// <summary>
    ///     Delete guest customer records
    /// </summary>
    /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
    /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
    /// <param name="onlyWithoutShoppingCart">A value indicating whether to delete customers only without shopping cart</param>
    /// <returns>Number of deleted customers</returns>
    Task<int> DeleteGuestCustomers(DateTime? createdFromUtc, DateTime? createdToUtc, bool onlyWithoutShoppingCart);

    #endregion

    #region Customer Group in Customer

    Task InsertCustomerGroupInCustomer(CustomerGroup customerGroup, string customerId);

    Task DeleteCustomerGroupInCustomer(CustomerGroup customerGroup, string customerId);

    #endregion

    #region Customer address

    Task DeleteAddress(Address address, string customerId);
    Task InsertAddress(Address address, string customerId);
    Task UpdateAddress(Address address, string customerId);
    Task UpdateBillingAddress(Address address, string customerId);
    Task UpdateShippingAddress(Address address, string customerId);

    #endregion

    #region Shopping cart

    Task ClearShoppingCartItem(string customerId, IList<ShoppingCartItem> cart);
    Task DeleteShoppingCartItem(string customerId, ShoppingCartItem shoppingCartItem);
    Task InsertShoppingCartItem(string customerId, ShoppingCartItem shoppingCartItem);
    Task UpdateShoppingCartItem(string customerId, ShoppingCartItem shoppingCartItem);

    #endregion
}