using Microsoft.EntityFrameworkCore;

namespace SuperHeroMinimalAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDbContext<DataContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


            var app = builder.Build();

            //use swagger middleware
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            async Task<List<SuperHero>> GetSuperHeroes(DataContext context) =>
                await context.SuperHeroes.ToListAsync();

            app.MapGet("/", () => "Hello Minimal API!");

            app.MapGet("/superhero", async (DataContext context) =>
                           await context.SuperHeroes.ToListAsync());

            app.MapGet("/superhero/{id}", async (DataContext context, int id) =>
                                      await context.SuperHeroes.FindAsync(id) is SuperHero hero ?
                                      Results.Ok(hero) :
                                      Results.NotFound("Hero under progress /"));


            app.MapPost("/superhero", async (DataContext datacontext, SuperHero hero) =>
            {
                await datacontext.SuperHeroes.AddAsync(hero);
                await datacontext.SaveChangesAsync();

                return Results.Ok(await GetSuperHeroes(datacontext));

            });

            app.MapPut("/superhero", async (DataContext context, SuperHero hero, int id) =>
            {
                var dbhero = await context.SuperHeroes.FindAsync(id);
                if (dbhero is null)
                {
                    return Results.NotFound("Hero under progress /");
                }

                dbhero.FirstName = hero.FirstName; 
                dbhero.LastName = hero.LastName;
                dbhero.HeroName = hero.HeroName;

                await context.SaveChangesAsync();

                return Results.Ok(await GetSuperHeroes(context));
            });

            //minimap api call to delete a hero

            app.MapDelete("/superhero/{id}", async (DataContext context, int id) =>
            {
                var dbhero = await context.SuperHeroes.FindAsync(id);
                if (dbhero is null)
                {
                    return Results.NotFound("Hero under progress /");
                }

                context.SuperHeroes.Remove(dbhero);
                await context.SaveChangesAsync();

                return Results.Ok(await GetSuperHeroes(context));
            });

            app.Run();
        }
    }
}