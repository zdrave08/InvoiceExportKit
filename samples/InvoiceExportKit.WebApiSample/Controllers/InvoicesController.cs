using InvoiceExportKit.Abstractions.Models;
using InvoiceExportKit.Abstractions.Options;
using InvoiceExportKit.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceExportKit.WebApiSample.Controllers;

/// <summary>Invoice export endpoints.</summary>
[ApiController]
[Route("api/invoices")]
public sealed class InvoicesController : ControllerBase
{
    private readonly InvoiceExportService _exportService;

    public InvoicesController(InvoiceExportService exportService)
        => _exportService = exportService;

    /// <summary>
    /// Exports an invoice to an Excel (.xlsx) file and returns it as a file download.
    /// </summary>
    /// <param name="request">Invoice data and optional export options.</param>
    /// <returns>An Excel file download.</returns>
    /// <response code="200">Returns the generated Excel file.</response>
    /// <response code="400">Validation errors in the invoice payload.</response>
    [HttpPost("export/excel")]
    [Produces("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult ExportExcel([FromBody] ExportRequest request)
    {
        if (request.Invoice is null)
            return BadRequest(new ProblemDetails { Title = "Invoice payload is required." });

        try
        {
            var options = request.Options ?? new ExportOptions();
            byte[] bytes = _exportService.Export(request.Invoice, options);

            string fileName = $"invoice_{request.Invoice.InvoiceNumber ?? "export"}.xlsx"
                .Replace("/", "-").Replace("\\", "-");

            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title  = "Invoice validation failed.",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }
}

/// <summary>Request body for the Excel export endpoint.</summary>
public sealed class ExportRequest
{
    /// <summary>The invoice to export.</summary>
    public InvoiceModel? Invoice { get; set; }

    /// <summary>Optional export options. Uses defaults when omitted.</summary>
    public ExportOptions? Options { get; set; }
}
