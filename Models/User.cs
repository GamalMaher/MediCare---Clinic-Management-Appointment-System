using Microsoft.AspNetCore.Identity;
using System;

namespace Medicare.Models
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; } = true;
        
        public virtual Doctor Doctor { get; set; }
        public virtual Patient Patient { get; set; }
    }
}
