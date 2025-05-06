using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Application.Common.Security.Token;
public class TokenSettings
{
    public string SecretKey { get; set; }
    public int AccessTokenExpirationInMinutes { get; set; }
    public int RefreshTokenExpirationInMinutes { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
}
