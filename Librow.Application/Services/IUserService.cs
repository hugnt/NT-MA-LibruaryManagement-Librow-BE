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
    public Task<Result> GetCurrentUserContext();
    public Task<Result> Logout(LogoutRequest logoutRequest);

    public Task<Result> GetAll(FilterRequest filter);
    public Task<Result> GetById(Guid id);
    public Task<Result> Add(RegisterRequest registerRequest);
    public Task<Result> Update(Guid id, UserUpdateRequest updatedUser);
    public Task<Result> Delete(Guid id);

    public Task<Result> GetActitviyLog(FilterRequest filter);

}
