using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiTemplate.Common;

public class Customer
{
    //Nullable because this is used for both creation and update operations
    public string? Id { get; set; }
    public required string Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Postal { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Notes { get; set; }

    public Gender? Gender { get; set; }
    public bool? Active { get; set; }

    public string? ImageBase64 { get; set; }
}

