using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasswordManager.Shared;

namespace PasswordManager.Controllers;

[Authorize]
[ApiController]
[Route("/api/[controller]")]
public class PasswordManagerController : ControllerBase
{
    private readonly ILogger<PasswordManagerController> logger;
    private readonly IPasswordManagerAccountRepository<PasswordmanagerAccount> passwordManagerAccountRepository;

    public PasswordManagerController(ILogger<PasswordManagerController> logger, IPasswordManagerAccountRepository<PasswordmanagerAccount> passwordManagerAccountRepository)
    {
        this.logger = logger;
        this.passwordManagerAccountRepository = passwordManagerAccountRepository;
    }

    [HttpGet]
    public IActionResult Test()
    {
        return Ok("PasswordManagerController endpoint works!");
    }

    [HttpGet]
    [Route("get-passwordaccounts/{UserId}")]
    public async Task<IActionResult> GetPasswordAccounts(string UserId)
    {
        var accounts = await passwordManagerAccountRepository.GetAllAccountsAsync(UserId);

        return Ok(accounts);
    }

    [HttpPost]
    [Route("create-password")]
    public async Task<IActionResult> AddPasswordAccount(PasswordmanagerAccount model)
    {
        var response = await passwordManagerAccountRepository.CreateAsync(model);

        if (response is null)
        {
            return BadRequest();
        }

        return Ok(response);
    }

    [HttpPost("[action]/single/{UserId}")]
    [AllowAnonymous]
    public async Task<IActionResult> Upload(IFormFile file, string UserId)
    {
        var result = await passwordManagerAccountRepository.UploadCsvAsync(file, UserId);

        if (result is null)
        {
            return BadRequest("failed to upload csv");
        }

        return Ok("upload csv success!");
    }

    [HttpPut]
    [Route("update-password")]
    public async Task<IActionResult> UpdatePasswordAccount(PasswordmanagerAccount model)
    {
        var response = await passwordManagerAccountRepository.UpdateAsync(model);

        if (response is null)
        {
            return BadRequest();
        }

        return Ok(response);
    }

    [HttpPost]
    [Route("delete-password")]
    public async Task<IActionResult> DeletePasswordAccount(PasswordmanagerAccount model)
    {
        var response = await passwordManagerAccountRepository.DeleteAsync(model);

        if (response is null)
        {
            return BadRequest();
        }

        return Ok(response);
    }

}
