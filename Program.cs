using Stock_Online.DataAccess.SQLite.Interface;
using Stock_Online.DataAccess.SQLite.Repositories;
using Stock_Online.Hubs;
using Stock_Online.Services.Adjustment;
using Stock_Online.Services.AnnualReview;
using Stock_Online.Services.DataUpdater;
using Stock_Online.Services.KLine;
using Stock_Online.Services.KLine.Indicators;
using Stock_Online.Services.ROILine;
using Stock_Online.Services.StockProvider;
using Stock_Online.Services.Update;
using Stock_Online.Services.UpdateOrchestrator;
using System.Text.Json.Serialization;

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
//builder.Services.AddSingleton(
//    new StockDailyPriceService("stock.db")
//);
builder.Services.AddSignalR();
builder.Services.AddScoped<IStockRepository, StockRepository>();
builder.Services.AddScoped<IStockPriceUpdateService, StockPriceUpdateService>();
builder.Services.AddScoped<IROILineChartService, ROILineChartService>();
builder.Services.AddScoped<IKLineChartService, KLineChartService>();
builder.Services.AddScoped<IStockDividendUpdateService, StockDividendUpdateService>();
builder.Services.AddScoped<IStockAnnualReviewService, StockAnnualReviewService>();
builder.Services.AddScoped<IPriceAdjustmentService, PriceAdjustmentService>();
builder.Services.AddScoped<IDividendAdjustmentService, DividendAdjustmentService>();
builder.Services.AddScoped<IStockShareholdingUpdateService, StockShareholdingUpdateService>();
builder.Services.AddScoped<IUpdateOrchestrator, UpdateOrchestrator>();
builder.Services.AddScoped<IStockProvider, StockProvider>();
builder.Services.AddScoped<IDataUpdater, PriceUpdater>();
builder.Services.AddScoped<IDataUpdater, DividendUpdater>();

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
