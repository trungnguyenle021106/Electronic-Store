using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonDto
{
    public record JWTClaim
    (int AccountID, string Role, int? CustomerID);
}
