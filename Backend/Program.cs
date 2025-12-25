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
    if (order.Id == Guid.Empty)
    {
        order.Id = Guid.NewGuid();
    }

    if (order.CreatedAt == default)
    {
        order.CreatedAt = DateTime.Now;
    }

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


app.MapPut("/orders/{id}/status", async (Guid id, string status, AppDBContext db, IHubContext<OrderHub> hubContext) =>
{
  var order = await db.Orders.FindAsync(id);
  if (order is null) return Results.NotFound();

  order.Status = status;
  await db.SaveChangesAsync();

  await hubContext.Clients.All.SendAsync("UpdateOrderStatus", id ,status);

  return Results.NoContent();  
});

app.MapDelete("/orders/clear", async (AppDBContext db) =>
{
    db.Orders.RemoveRange(db.Orders);
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.MapDelete("/orders/{id}", async (Guid id, AppDBContext db, IHubContext<OrderHub> hubContext) =>
{
    var order = await db.Orders.FindAsync(id);

    if (order is null) return Results.NotFound();

    db.Orders.Remove(order);
    await db.SaveChangesAsync();

    await hubContext.Clients.All.SendAsync("ReceiveOrderDeleted", id);

    return Results.Ok();
});

app.Run();


