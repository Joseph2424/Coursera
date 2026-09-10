using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SecurePortal.Web.Pages;

[Authorize(Policy = "AdminOnly")]
public class AdminModel : PageModel
{
    public void OnGet()
    {
    }
}

