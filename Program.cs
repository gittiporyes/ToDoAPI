using System.Security.Permissions;
using Microsoft.EntityFrameworkCore;
using TodoApi;
using Microsoft.OpenApi.Models; // שורה זו נוספה

var builder = WebApplication.CreateBuilder(args);

// הוספת שירות CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

// הוספת DbContext
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("ToDoDB"), 
                     new MySqlServerVersion(new Version(8, 0, 40))));

// הוספת שירות Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// הוספת Middleware ל-CORS
app.UseHttpsRedirection();
app.UseCors("AllowAll");

// הפעלת Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Todo API V1");
    c.RoutePrefix = string.Empty; // זה יספק את Swagger UI בכתובת הבית
});

app.MapGet("/", () => "Hello World!");

app.MapGet("/", async (ToDoDbContext db) =>
{
    return await db.Items.ToListAsync();
});  

app.MapGet("/todos", async (ToDoDbContext db) =>
{
    return await db.Items.ToListAsync();
});    

app.MapPost("/todos", async (ToDoDbContext db, Item newItem) =>
{
    db.Items.Add(newItem);
    await db.SaveChangesAsync();
    return Results.Created($"/todos/{newItem.Id}", newItem);
});

app.MapPut("/todos/{id}", async (int id, ToDoDbContext db, Item updatedItem) =>
{
    var existingItem = await db.Items.FindAsync(id);
    if (existingItem is null) return Results.NotFound();

    existingItem.Name = updatedItem.Name;
    existingItem.IsComplete = updatedItem.IsComplete;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/todos/{id}", async (int id, ToDoDbContext db) =>
{
    var existingItem = await db.Items.FindAsync(id);
    if (existingItem is null) return Results.NotFound();

    db.Items.Remove(existingItem);
    await db.SaveChangesAsync();

    return Results.NoContent();
});


app.MapGet("/",()=>"Auth service API is running");

app.Run();