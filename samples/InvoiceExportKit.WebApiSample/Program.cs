using InvoiceExportKit.Abstractions.Contracts;
using InvoiceExportKit.Core.Services;
using InvoiceExportKit.Core.Validation;
using InvoiceExportKit.Excel.OpenXml.Rendering;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "InvoiceExportKit API", Version = "v1" });
});

builder.Services.AddSingleton<IInvoiceExporter, ExcelInvoiceExporter>();
builder.Services.AddSingleton<IInvoiceValidator, InvoiceValidator>();
builder.Services.AddScoped<InvoiceExportService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "InvoiceExportKit v1"));

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
