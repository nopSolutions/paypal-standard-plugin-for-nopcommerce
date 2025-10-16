using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace Nop.Plugin.Payments.PayPalStandard.Infrastructure;

public class RouteProvider : BaseRouteProvider, IRouteProvider
{
    /// <summary>
    /// Register routes
    /// </summary>
    /// <param name="endpointRouteBuilder">Route builder</param>
    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        var lang = GetLanguageRoutePattern();

        endpointRouteBuilder.MapControllerRoute(name: PayPalStandardDefaults.Route.Configuration,
            pattern: "Admin/PaymentPayPalStandard/Configure",
            defaults: new { controller = "PaymentPayPalStandard", action = "Configure", area = AreaNames.ADMIN });

        //PDT
        endpointRouteBuilder.MapControllerRoute(name: PayPalStandardDefaults.Route.PDTHandler,
            pattern: $"{lang}/Plugins/PaymentPayPalStandard/PDTHandler",
            defaults: new { controller = "PaymentPayPalStandard", action = "PDTHandler" });

        //IPN
        endpointRouteBuilder.MapControllerRoute(name: PayPalStandardDefaults.Route.IPNHandler,
            pattern: "Plugins/PaymentPayPalStandard/IPNHandler",
            defaults: new { controller = "PaymentPayPalStandardIpn", action = "IPNHandler" });

        //Cancel
        endpointRouteBuilder.MapControllerRoute(name: PayPalStandardDefaults.Route.CancelOrder,
            pattern: $"{lang}/Plugins/PaymentPayPalStandard/CancelOrder/{{orderId:min(0)}}",
            defaults: new { controller = "PaymentPayPalStandard", action = "CancelOrder" });
    }

    /// <summary>
    /// Gets a priority of route provider
    /// </summary>
    public int Priority => -1;
}