using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BITE.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSqlite<AppDBContext>("Data Source=BITE.db");
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy => {
        policy.WithOrigins("http://localhost:5220")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDBContext>();
    db.Database.EnsureCreated();
}

app.UseCors("AllowBlazor");

app.MapHub<OrderHub>("/orderhub");

app.MapPost("/orders", async (Order order, AppDBContext db, IHubContext<OrderHub> hubContext) =>
{
    db.Orders.Add(order);
    await db.SaveChangesAsync();
    await hubContext.Clients.All.SendAsync("ReceiveNewOrder", order);
    return Results.Created($"/orders/{order.Id}", order);
});

app.MapGet("/orders", async (AppDBContext db) =>
    await db.Orders.ToListAsync());


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "API is running! Go to /swagger to see the UI.");

app.Run();


