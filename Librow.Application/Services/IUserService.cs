using Librow.Application.Models.Requests;
using Librow.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Application.Services;
public interface IUserService
{
    public Task<Result> Register(RegisterRequest registerRequest);
    public Task<Result> Login(LoginRequest loginRequest);
    public Task<Result> ExtendSession(ExtendSessionRequest reLoginRequest);

}
