using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseInMemoryDatabase("Tarefa"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "Olá Mundo");


app.MapGet("Frases", async () => 
    await new HttpClient().GetStringAsync("https://ron-swanson-quotes.herokuapp.com/v2/quotes"));


app.MapGet("/Tarefas", async (AppDbContext db) => await db.Tarefa.ToListAsync());


app.MapGet("/Tarefa/{id}", async (int id, AppDbContext db) =>
    await db.Tarefa.FindAsync(id) is Tarefa tarefa ? Results.Ok(tarefa) : Results.NotFound()) ;


app.MapGet("/Tarefa/concluidas", async (int id, AppDbContext db) =>
                                 await db.Tarefa.Where(t => t.IsConcluida).ToListAsync());

app.MapPut("/Tarefa/{id}", async (int id, Tarefa inputTarefa, AppDbContext db) =>
{
    var tarefa = await db.Tarefa.FindAsync(id);

    if (tarefa is null) return Results.NotFound();

    tarefa.Name = inputTarefa.Name;
    tarefa.IsConcluida = inputTarefa.IsConcluida;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/Tarefa/{id}", async (int id, AppDbContext db) =>
{
    if (await db.Tarefa.FindAsync(id) is Tarefa tarefa)
    {
        db.Tarefa.Remove(tarefa);
        await db.SaveChangesAsync();
        return Results.Ok(tarefa);
    }
    return Results.NotFound();

});

app.Run();

class Tarefa
{
    public int Id { get; set; }
    
    public string? Name { get; set; }
    
    public bool IsConcluida { get; set; }
}

class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }

    public DbSet<Tarefa> Tarefa =>  Set<Tarefa>();
}

