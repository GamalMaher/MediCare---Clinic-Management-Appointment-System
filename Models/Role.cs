using Microsoft.AspNetCore.Identity;

namespace Medicare.Models
{
    public class Role : IdentityRole
    {
        public string Description { get; set; }
    }
}
