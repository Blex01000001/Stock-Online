using Stock_Online.DataAccess.SQLite.Interface;
using Stock_Online.DataAccess.SQLite.Repositories;
using Stock_Online.Services.KLine;
using Stock_Online.Services.KLine.Builders;
using Stock_Online.Services.KLine.Indicators;
using Stock_Online.Services.KLine.Queries;
using Stock_Online.Services.KLine.Patterns;
using Stock_Online.Services.ROILine;
using Stock_Online.Services.Update;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//builder.Services.AddSingleton(
//    new StockDailyPriceService("stock.db")
//);
builder.Services.AddScoped<IStockDailyPriceRepository, StockDailyPriceRepository>();
builder.Services.AddScoped<IStockPriceUpdateService, StockDailyPriceService>();
builder.Services.AddScoped<IROILineChartService, ROILineChartService>();
builder.Services.AddScoped<IKLineChartService, KLineChartService>();
builder.Services.AddScoped<IMovingAverageCalculator, MovingAverageCalculator>();


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
            .AllowAnyMethod(); //允許所有 HTTP 方法 GET / POST / PUT / DELETE / OPTIONS ...
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseStaticFiles();  // 允許讀 wwwroot
app.UseDefaultFiles();  // 啟用 index.html 規則
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("DevCors");
app.UseAuthorization();
app.MapControllers();

app.Run();
