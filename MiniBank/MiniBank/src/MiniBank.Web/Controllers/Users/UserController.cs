using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniBank.Core.Domains.Users;
using MiniBank.Core.Domains.Users.Services;
using MiniBank.Web.Controllers.Users.Dto;

namespace MiniBank.Web.Controllers.Users;

[ApiController]
[Route("user")]
public class UserController
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }


    [Authorize]
    [HttpGet("{id}")]
    public async Task<UserGetResponseDto> Get(string id, CancellationToken token)
    {
        var model = await _userService.GetByIdAsync(id, token);

        return new UserGetResponseDto
        {
            Id = model.Id,
            Login = model.Login,
            Email = model.Email
        };
    }

    [Authorize]
    [HttpGet]
    public async Task<List<UserGetResponseDto>> GetAll(CancellationToken token)
    {
        var users = await _userService.GetAllUsersAsync(token);
        return users.Select(user => new UserGetResponseDto
        {
            Id = user.Id,
            Login = user.Login,
            Email = user.Email
        }).ToList();
    }

    [Authorize]
    [HttpPost]
    public Task Create(UserCreateRequestDto model, CancellationToken token)
    {
        return _userService.CreateAsync(new User
        {
            Login = model.Login,
            Email = model.Email
        }, 
            token);
    }
    
    [Authorize]
    [HttpPut("/{id}")]
    public Task Update(string id, UserCreateRequestDto model, CancellationToken token)
    {
        return _userService.UpdateAsync(new User
        {
            Id = id,
            Login = model.Login,
            Email = model.Email
        },
            token);
    }

    [Authorize]
    [HttpDelete("/{id}")]
    public Task Delete(string id, CancellationToken token)
    {
        return _userService.DeleteByIdAsync(id, token);
    }
}