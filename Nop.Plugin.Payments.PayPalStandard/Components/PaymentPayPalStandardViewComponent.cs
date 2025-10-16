using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.PayPalStandard.Components;

/// <summary>
/// Represents the view component to display payment info in the public store
/// </summary>
public class PaymentPayPalStandardViewComponent : NopViewComponent
{
    #region Methods

    /// <summary>
    /// Invoke view component
    /// </summary>
    /// <returns>The view component result</returns>
    public IViewComponentResult Invoke()
    {
        return View("~/Plugins/Payments.PayPalStandard/Views/PaymentInfo.cshtml");
    }

    #endregion
}