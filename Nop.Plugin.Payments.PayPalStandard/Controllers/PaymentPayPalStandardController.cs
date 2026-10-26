using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Http;
using Nop.Plugin.Payments.PayPalStandard.Models;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Payments.PayPalStandard.Controllers;

[AutoValidateAntiforgeryToken]
public class PaymentPayPalStandardController : BasePaymentController
{
    #region Fields

    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IOrderProcessingService _orderProcessingService;
    private readonly IOrderService _orderService;
    private readonly IPaymentPluginManager _paymentPluginManager;
    private readonly ILocalizationService _localizationService;
    private readonly ILogger _logger;
    private readonly INotificationService _notificationService;
    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;
    private readonly IWebHelper _webHelper;
    private readonly IWorkContext _workContext;
    private readonly ShoppingCartSettings _shoppingCartSettings;

    #endregion

    #region Ctor

    public PaymentPayPalStandardController(IGenericAttributeService genericAttributeService,
        IOrderProcessingService orderProcessingService,
        IOrderService orderService,
        IPaymentPluginManager paymentPluginManager,
        ILocalizationService localizationService,
        ILogger logger,
        INotificationService notificationService,
        ISettingService settingService,
        IStoreContext storeContext,
        IWebHelper webHelper,
        IWorkContext workContext,
        ShoppingCartSettings shoppingCartSettings)
    {
        _genericAttributeService = genericAttributeService;
        _orderProcessingService = orderProcessingService;
        _orderService = orderService;
        _paymentPluginManager = paymentPluginManager;
        _localizationService = localizationService;
        _logger = logger;
        _notificationService = notificationService;
        _settingService = settingService;
        _storeContext = storeContext;
        _webHelper = webHelper;
        _workContext = workContext;
        _shoppingCartSettings = shoppingCartSettings;
    }

    #endregion

    #region Methods

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PAYMENT_METHODS)]
    public async Task<IActionResult> Configure()
    {
        //load settings for a chosen store scope
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var payPalStandardPaymentSettings = await _settingService.LoadSettingAsync<PayPalStandardPaymentSettings>(storeScope);

        var model = new ConfigurationModel
        {
            UseSandbox = payPalStandardPaymentSettings.UseSandbox,
            BusinessEmail = payPalStandardPaymentSettings.BusinessEmail,
            PdtToken = payPalStandardPaymentSettings.PdtToken,
            PassProductNamesAndTotals = payPalStandardPaymentSettings.PassProductNamesAndTotals,
            AdditionalFee = payPalStandardPaymentSettings.AdditionalFee,
            AdditionalFeePercentage = payPalStandardPaymentSettings.AdditionalFeePercentage,
            ActiveStoreScopeConfiguration = storeScope,
            PdtUrl = Url.RouteUrl(PayPalStandardDefaults.Route.PDTHandler, null, _webHelper.GetCurrentRequestProtocol()),
            IsActive = await _paymentPluginManager.IsPluginActiveAsync(PayPalStandardDefaults.SystemName)
        };

        if (storeScope <= 0)
            return View("~/Plugins/Payments.PayPalStandard/Views/Configure.cshtml", model);

        model.UseSandbox_OverrideForStore = await _settingService.SettingExistsAsync(payPalStandardPaymentSettings, x => x.UseSandbox, storeScope);
        model.BusinessEmail_OverrideForStore = await _settingService.SettingExistsAsync(payPalStandardPaymentSettings, x => x.BusinessEmail, storeScope);
        model.PdtToken_OverrideForStore = await _settingService.SettingExistsAsync(payPalStandardPaymentSettings, x => x.PdtToken, storeScope);
        model.PassProductNamesAndTotals_OverrideForStore = await _settingService.SettingExistsAsync(payPalStandardPaymentSettings, x => x.PassProductNamesAndTotals, storeScope);
        model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(payPalStandardPaymentSettings, x => x.AdditionalFee, storeScope);
        model.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(payPalStandardPaymentSettings, x => x.AdditionalFeePercentage, storeScope);

        return View("~/Plugins/Payments.PayPalStandard/Views/Configure.cshtml", model);
    }

    [HttpPost]
    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PAYMENT_METHODS)]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!ModelState.IsValid)
            return await Configure();

        //load settings for a chosen store scope
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var payPalStandardPaymentSettings = await _settingService.LoadSettingAsync<PayPalStandardPaymentSettings>(storeScope);

        //save settings
        payPalStandardPaymentSettings.UseSandbox = model.UseSandbox;
        payPalStandardPaymentSettings.BusinessEmail = model.BusinessEmail;
        payPalStandardPaymentSettings.PdtToken = model.PdtToken;
        payPalStandardPaymentSettings.PassProductNamesAndTotals = model.PassProductNamesAndTotals;
        payPalStandardPaymentSettings.AdditionalFee = model.AdditionalFee;
        payPalStandardPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;

        /* We do not clear cache after each setting update.
         * This behavior can increase performance because cached settings will not be cleared 
         * and loaded from database after each update */
        await _settingService.SaveSettingOverridablePerStoreAsync(payPalStandardPaymentSettings, x => x.UseSandbox, model.UseSandbox_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(payPalStandardPaymentSettings, x => x.BusinessEmail, model.BusinessEmail_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(payPalStandardPaymentSettings, x => x.PdtToken, model.PdtToken_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(payPalStandardPaymentSettings, x => x.PassProductNamesAndTotals, model.PassProductNamesAndTotals_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(payPalStandardPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(payPalStandardPaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);

        //now clear settings cache
        await _settingService.ClearCacheAsync();

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return await Configure();
    }

    //action displaying notification (warning) to a store owner about inaccurate PayPal rounding
    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PAYMENT_METHODS)]
    public async Task<IActionResult> RoundingWarning(bool passProductNamesAndTotals)
    {
        //prices and total aren't rounded, so display warning
        if (passProductNamesAndTotals && !_shoppingCartSettings.RoundPricesDuringCalculation)
            return Json(new { Result = await _localizationService.GetResourceAsync("Plugins.Payments.PayPalStandard.RoundingWarning") });

        return Json(new { Result = string.Empty });
    }

    [WwwRequirement]
    [CheckLanguageSeoCode]
    [CheckAccessPublicStore]
    [CheckAccessClosedStore]
    public async Task<IActionResult> PDTHandler()
    {
        var tx = _webHelper.QueryString<string>("tx");

        var paymentPlugin = await _paymentPluginManager.LoadPluginBySystemNameAsync(PayPalStandardDefaults.SystemName);
        if (paymentPlugin is not PayPalStandardPaymentProcessor processor)
            throw new NopException($"{PayPalStandardDefaults.SystemName} module cannot be loaded");

        if (!_paymentPluginManager.IsPluginActive(processor))
            throw new NopException($"{PayPalStandardDefaults.SystemName} module cannot be loaded");

        var (result, values, response) = await processor.GetPdtDetailsAsync(tx);

        async Task<Order> getOrderAsync(string guid) => Guid.TryParse(guid, out var orderGuid) ? await _orderService.GetOrderByGuidAsync(orderGuid) : null;

        if (result)
        {
            var order = values.TryGetValue("custom", out var guid) ? await getOrderAsync(guid) : null;
            if (order is null)
                return RedirectToRoute(NopRouteNames.General.HOMEPAGE);

            var mcGross = values.TryGetValue("mc_gross", out var mcGrossString)
                && decimal.TryParse(mcGrossString, new CultureInfo("en-US"), out var mcGrossValue)
                ? mcGrossValue
                : decimal.Zero;

            values.TryGetValue("payer_status", out var payerStatus);
            values.TryGetValue("payment_status", out var paymentStatus);
            values.TryGetValue("pending_reason", out var pendingReason);
            values.TryGetValue("mc_currency", out var mcCurrency);
            values.TryGetValue("txn_id", out var txnId);
            values.TryGetValue("payment_type", out var paymentType);
            values.TryGetValue("payer_id", out var payerId);
            values.TryGetValue("receiver_id", out var receiverId);
            values.TryGetValue("invoice", out var invoice);
            values.TryGetValue("mc_fee", out var mcFee);

            var sb = new StringBuilder();
            sb.AppendLine("PayPal PDT:");
            sb.AppendLine("mc_gross: " + mcGross);
            sb.AppendLine("Payer status: " + payerStatus);
            sb.AppendLine("Payment status: " + paymentStatus);
            sb.AppendLine("Pending reason: " + pendingReason);
            sb.AppendLine("mc_currency: " + mcCurrency);
            sb.AppendLine("txn_id: " + txnId);
            sb.AppendLine("payment_type: " + paymentType);
            sb.AppendLine("payer_id: " + payerId);
            sb.AppendLine("receiver_id: " + receiverId);
            sb.AppendLine("invoice: " + invoice);
            sb.AppendLine("mc_fee: " + mcFee);

            var newPaymentStatus = PayPalHelper.GetPaymentStatus(paymentStatus, string.Empty);
            sb.AppendLine("New payment status: " + newPaymentStatus);

            await _orderService.InsertOrderNoteAsync(new()
            {
                OrderId = order.Id,
                Note = sb.ToString(),
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });

            //validate order total
            var orderTotalSentToPayPal = await _genericAttributeService
                .GetAttributeAsync<decimal?>(order, PayPalStandardDefaults.OrderTotalAttributeName);
            if (orderTotalSentToPayPal.HasValue && mcGross != orderTotalSentToPayPal.Value)
            {
                var errorStr = $"{PayPalStandardDefaults.SystemName} PDT. Returned order total {mcGross} doesn't equal order total {order.OrderTotal}. Order# {order.Id}.";
                await _logger.ErrorAsync(errorStr);

                await _orderService.InsertOrderNoteAsync(new()
                {
                    OrderId = order.Id,
                    Note = errorStr,
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });

                return RedirectToRoute(NopRouteNames.General.HOMEPAGE);
            }

            //clear attribute
            if (orderTotalSentToPayPal.HasValue)
                await _genericAttributeService.SaveAttributeAsync<decimal?>(order, PayPalStandardDefaults.OrderTotalAttributeName, null);

            if (newPaymentStatus != PaymentStatus.Paid)
                return RedirectToRoute(NopRouteNames.Standard.CHECKOUT_COMPLETED, new { orderId = order.Id });

            if (!_orderProcessingService.CanMarkOrderAsPaid(order))
                return RedirectToRoute(NopRouteNames.Standard.CHECKOUT_COMPLETED, new { orderId = order.Id });

            //mark order as paid
            order.AuthorizationTransactionId = txnId;
            await _orderService.UpdateOrderAsync(order);
            await _orderProcessingService.MarkOrderAsPaidAsync(order);

            return RedirectToRoute(NopRouteNames.Standard.CHECKOUT_COMPLETED, new { orderId = order.Id });
        }
        else
        {
            if (!values.TryGetValue("custom", out var guid))
                guid = _webHelper.QueryString<string>("cm");

            var order = await getOrderAsync(guid);
            if (order is null)
                return RedirectToRoute(NopRouteNames.General.HOMEPAGE);

            await _orderService.InsertOrderNoteAsync(new()
            {
                OrderId = order.Id,
                Note = "PayPal PDT failed. " + response,
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });

            return RedirectToRoute(NopRouteNames.Standard.CHECKOUT_COMPLETED, new { orderId = order.Id });
        }
    }

    [WwwRequirement]
    [CheckLanguageSeoCode]
    [CheckAccessPublicStore]
    [CheckAccessClosedStore]
    public async Task<IActionResult> CancelOrder(int orderId)
    {
        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order is null || order.Deleted)
            return RedirectToRoute(NopRouteNames.General.HOMEPAGE);

        var customer = await _workContext.GetCurrentCustomerAsync();
        if (customer.Id != order.CustomerId)
            return Challenge();

        var store = await _storeContext.GetCurrentStoreAsync();
        if (store.Id != order.StoreId)
            return RedirectToRoute(NopRouteNames.General.HOMEPAGE);

        return RedirectToRoute(NopRouteNames.Standard.ORDER_DETAILS, new { orderId = order.Id });
    }

    #endregion
}