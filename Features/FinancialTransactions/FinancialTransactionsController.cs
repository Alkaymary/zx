using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.Application.FinancialTransactions;
using MyApi.Dtos;
using MyApi.Infrastructure.Presentation;
using MyApi.Infrastructure.Security;

namespace MyApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "AdminAPI")]
[Authorize(Policy = AuthorizationPolicies.RequireFinancialRead)]
public class FinancialTransactionsController : ControllerBase
{
    private readonly IFinancialTransactionsAppService _appService;

    public FinancialTransactionsController(IFinancialTransactionsAppService appService)
    {
        _appService = appService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FinancialTransactionResponseDto>>> GetAll([FromQuery] int? libraryId, CancellationToken cancellationToken)
    {
        return Ok(await _appService.GetAllAsync(libraryId, cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FinancialTransactionResponseDto>> GetById(int id, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await _appService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.RequireFinancialWrite)]
    public async Task<ActionResult<FinancialTransactionResponseDto>> Create(CreateFinancialTransactionDto request, CancellationToken cancellationToken)
    {
        var result = await _appService.CreateAsync(request, User.ToAdminActorContext(), cancellationToken);
        if (!result.IsSuccess || result.Value is null)
        {
            return this.ToActionResult(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = AuthorizationPolicies.RequireFinancialWrite)]
    public async Task<ActionResult<FinancialTransactionResponseDto>> Update(int id, UpdateFinancialTransactionDto request, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await _appService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = AuthorizationPolicies.RequireFinancialDelete)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await _appService.DeleteAsync(id, cancellationToken));
    }

    [HttpPost("{transactionId:int}/settlements")]
    [Authorize(Policy = AuthorizationPolicies.RequireFinancialWrite)]
    public async Task<ActionResult<TransactionSettlementResponseDto>> CreateSettlement(
        int transactionId,
        CreateTransactionSettlementDto request,
        CancellationToken cancellationToken)
    {
        var result = await _appService.CreateSettlementAsync(
            transactionId,
            request,
            User.ToAdminActorContext(),
            cancellationToken);

        if (!result.IsSuccess || result.Value is null)
        {
            return this.ToActionResult(result);
        }

        var transactionResult = await _appService.GetByIdAsync(result.Value.FinancialTransactionId, cancellationToken);
        if (transactionResult.IsSuccess && transactionResult.Value is not null)
        {
            return CreatedAtAction(
                nameof(GetStatementByLibrary),
                new { libraryId = transactionResult.Value.LibraryId },
                result.Value);
        }

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    [HttpPost("library/{libraryId:int}/settlements")]
    [Authorize(Policy = AuthorizationPolicies.RequireFinancialWrite)]
    public async Task<ActionResult<LibrarySettlementResultDto>> CreateLibrarySettlement(
        int libraryId,
        CreateLibrarySettlementDto request,
        CancellationToken cancellationToken)
    {
        return this.ToActionResult(await _appService.CreateLibrarySettlementAsync(
            libraryId,
            request,
            User.ToAdminActorContext(),
            cancellationToken));
    }

    [HttpDelete("settlements/{id:int}")]
    [Authorize(Policy = AuthorizationPolicies.RequireFinancialDelete)]
    public async Task<IActionResult> DeleteSettlement(int id, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await _appService.DeleteSettlementAsync(id, cancellationToken));
    }

    [HttpGet("library/{libraryId:int}/statement")]
    public async Task<ActionResult<LibraryFinancialStatementDto>> GetStatementByLibrary(int libraryId, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await _appService.GetStatementByLibraryAsync(libraryId, cancellationToken));
    }
}
