using Microsoft.AspNetCore.Components;

namespace ChatApp.Web.Components;

public partial class UserSidebar : ComponentBase
{
     [Inject] private NavigationManager NavigationManager { get; set; } = default!;

     private async Task Logout()
     {
          // Redirect to the built-in logout endpoint
          NavigationManager.NavigateTo("/account/logout", forceLoad: true);
     }
} 