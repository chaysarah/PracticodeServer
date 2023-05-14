using ToDoApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

var  MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Todo API", Description = "Keep track of your tasks", Version = "v1" });
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddPolicy(MyAllowSpecificOrigins,
                          policy =>
                          {
                              policy.AllowAnyOrigin()
                                    .AllowAnyHeader()
                                    .AllowAnyMethod();
                          });
});

builder.Services.AddDbContext<ToDoDbContext>();
var app = builder.Build();

app.UseCors(MyAllowSpecificOrigins);  
app.UseSwagger();
app.UseSwaggerUI(options =>
{
   options.SwaggerEndpoint("/swagger/v1/swagger.json", "Todo API V1");
});

app.MapGet("/", () => "Hello World!");

app.MapGet("/item", (ToDoDbContext c) => {
    return c.Items.ToList();
    });

app.MapPost("/item", async(ToDoDbContext context, Item item)=>{
    context.Add(item);
    await context.SaveChangesAsync();
    return item;
});
app.MapPut("/item", async(ToDoDbContext context, [FromBody]Item item, int id)=>{
    var existItem = await context.Items.FindAsync(id);
    if(existItem is null) return Results.NotFound();

    existItem.Name = item.Name;
    existItem.IsComplete = item.IsComplete;

    await context.SaveChangesAsync();
    return Results.NoContent();
});
app.MapDelete("/item", async(ToDoDbContext context,int id) => {
    var existItem=await context.Items.FindAsync(id);
    if(existItem is null) return Results.NotFound();
      context.Items.Remove(existItem);
      await context.SaveChangesAsync();
      return Results.NoContent();
});

app.MapMethods("/options-or-head", new[] { "OPTIONS", "HEAD" }, 
                          () => "This is an options or head request ");

app.Run();

