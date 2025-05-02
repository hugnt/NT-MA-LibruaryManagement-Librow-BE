using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Application.Common.Enums;
public enum TokenErrorCode
{
    None,
    TokenExpired,
    TokenSignatureKeyNotFound,
    TokenInvalidSignature,
}
