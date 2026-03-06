//using Stock_Online.Services.KLine.Indicators;
using System.Text.Json.Serialization;
using Stock_Online.Domain.Interfaces.Repositories;
using Stock_Online.Infrastructure.Persistence.SQLite;
using Stock_Online.WebAPI.Hubs;
using Stock_Online.Application.Services.KLine;
using Stock_Online.Application.Services.ROILine;
using Stock_Online.Application.Services.PatternRecognition;
using Stock_Online.Application.Services.Adjustment;
using Stock_Online.Application.Services.DataUpdater;
using Stock_Online.Application.Services.StockProvider;
using Stock_Online.Application.Services.AnnualReview;
using Stock_Online.Application.Services.UpdateOrchestrator;
using Stock_Online.Application.Services.PatternRecognition.PatternType;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        // 可選：允許大小寫不敏感（System.Text.Json 本身多數情況已不敏感，但保險）
        // o.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IStockRepository, StockRepository>();
builder.Services.AddScoped<IROILineChartService, ROILineChartService>();
builder.Services.AddScoped<IKLineChartService, KLineChartService>();
builder.Services.AddScoped<IStockAnnualReviewService, StockAnnualReviewService>();
builder.Services.AddScoped<IPriceAdjustmentService, PriceAdjustmentService>();
builder.Services.AddScoped<IDividendAdjustmentService, DividendAdjustmentService>();
builder.Services.AddScoped<IUpdateOrchestrator, UpdateOrchestrator>();
builder.Services.AddScoped<IStockProvider, StockProvider>();
builder.Services.AddScoped<IDataUpdater, PriceUpdater>();
builder.Services.AddScoped<IDataUpdater, DividendUpdater>();
builder.Services.AddScoped<IDataUpdater, ForeignShareHoldingUpdater>();
builder.Services.AddScoped<IDataUpdater, InstitutionalInvestorsBuySellUpdater>();
builder.Services.AddScoped<IInstitutionalInvestorsAdjustmentService, InstitutionalInvestorsAdjustmentService>();
builder.Services.AddScoped<IKLinePattern, BullishEngulfingPattern>();
builder.Services.AddScoped<IPatternRecognitionService, PatternRecognitionService>();

// 註冊 CORS 服務，這裡先定義一個全開的 Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy//.AllowAnyOrigin() //允許全部
            .WithOrigins(
                "http://127.0.0.1:5500", //白名單來源（Origin）指定『哪些網頁（前端）有資格用 JavaScript 呼叫你的後端 API
                "http://localhost:5500"
            )
            .AllowAnyHeader() // 允許前端送出 任何 HTTP Header
            .AllowAnyMethod() //允許所有 HTTP 方法 GET / POST / PUT / DELETE / OPTIONS ...
        .AllowCredentials();
    });
});

var app = builder.Build();
app.UseRouting();
app.UseCors("DevCors");


app.MapHub<StockUpdateHub>("/stockUpdateHub");
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseStaticFiles();  // 允許讀 wwwroot
app.UseDefaultFiles();  // 啟用 index.html 規則
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
