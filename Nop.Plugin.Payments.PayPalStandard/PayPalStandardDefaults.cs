using Nop.Core;

namespace Nop.Plugin.Payments.PayPalStandard;

/// <summary>
/// Represents the plugin constants
/// </summary>
public class PayPalStandardDefaults
{
    /// <summary>
    /// Gets the plugin system name
    /// </summary>
    public static string SystemName => "Payments.PayPalStandard";

    /// <summary>
    /// Gets the user agent used to request third-party services
    /// </summary>
    public static string UserAgent => $"nopCommerce-{NopVersion.FULL_VERSION}";

    /// <summary>
    /// Gets the service URL
    /// </summary>
    public static (string Sandbox, string Live) ServiceUrl => ("https://www.sandbox.paypal.com/us/cgi-bin/webscr", "https://www.paypal.com/us/cgi-bin/webscr");

    /// <summary>
    /// Gets the IPN URL
    /// </summary>
    public static (string Sandbox, string Live) IPNUrl => ("https://ipnpb.sandbox.paypal.com/cgi-bin/webscr", "https://ipnpb.paypal.com/cgi-bin/webscr");

    /// <summary>
    /// Gets nopCommerce partner code
    /// </summary>
    public static string PartnerCode => "nopCommerce_SP";

    /// <summary>
    /// Gets the generic attribute name that is used to store an order total that actually sent to PayPal (used to PDT order total validation)
    /// </summary>
    public static string OrderTotalAttributeName => "OrderTotalSentToPayPal";

    /// <summary>
    /// Gets a default period (in seconds) before the request times out
    /// </summary>
    public static int RequestTimeout => 20;

    #region Route names

    /// <summary>
    /// Represents the route names
    /// </summary>
    public class Route
    {
        /// <summary>
        /// Gets the configuration route name
        /// </summary>
        public static string Configuration => "Plugin.Payments.PayPalStandard.Configure";

        /// <summary>
        /// Gets the PDT handler route name
        /// </summary>
        public static string PDTHandler => "Plugin.Payments.PayPalStandard.PDTHandler";

        /// <summary>
        /// Gets the IPN handler route name
        /// </summary>
        public static string IPNHandler => "Plugin.Payments.PayPalStandard.IPNHandler";

        /// <summary>
        /// Gets the cancel order route name
        /// </summary>
        public static string CancelOrder => "Plugin.Payments.PayPalStandard.CancelOrder";
    }

    #endregion
}