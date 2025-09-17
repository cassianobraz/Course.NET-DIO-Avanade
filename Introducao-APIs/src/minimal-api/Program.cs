using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using minimal_api.Domain.DTOs;
using minimal_api.Domain.Entity;
using minimal_api.Domain.Interfaces;
using minimal_api.Domain.ModelViews;
using minimal_api.Domain.Servicos;
using minimal_api.Infra.Db;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAdminServico, AdminServico>();
builder.Services.AddScoped<IVehicleServico, VehicleServico>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DbContextDio>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var app = builder.Build();

#region Admin
app.MapPost("/admin/login", ([FromBody] LoginDTO loginDTO, IAdminServico adminServico) =>
{
    if (adminServico.Login(loginDTO) != null)
        return Results.Ok("Login realizado com sucesso!");
    else
        return Results.Unauthorized();
}).WithTags("Administrador");

app.MapPost("/admin", ([FromBody] AdministradorDTO administradorDTO, IAdminServico adminServico) =>
{
    if (adminServico.Incluir(administradorDTO) != null)
        return Results.Ok("Login realizado com sucesso!");
    else
        return Results.Unauthorized();
}).WithTags("Administrador");
#endregion

#region Vehicles
ErrosDeValidacao validaDto(VehicleDTO vehicleDTO)
{
    var validacao = new ErrosDeValidacao { Mensagens = new List<string>() };

    if (string.IsNullOrEmpty(vehicleDTO.Nome))
        validacao.Mensagens.Add("O nome não pode ser vazio.");

    if (string.IsNullOrEmpty(vehicleDTO.Marca))
        validacao.Mensagens.Add("A marca não pode ser vazia.");

    if (vehicleDTO.Ano < 1950)
        validacao.Mensagens.Add("Veiculo muito antigo. Aceito apenas anos superior a 1950.");

    return validacao;
}

app.MapPost("/vehicle", ([FromBody] VehicleDTO vehicleDTO, IVehicleServico vehicleServico) =>
{
    var validacao = validaDto(vehicleDTO);
    if (validacao.Mensagens.Count > 0)
        return Results.BadRequest(validacao);

    var vehicle = new Vehicle
    {
        Nome = vehicleDTO.Nome,
        Marca = vehicleDTO.Marca,
        Ano = vehicleDTO.Ano
    };

    vehicleServico.Incluir(vehicle);

    return Results.Created($"/vehicle/{vehicle.Id}", vehicle);
}).WithTags("Veiculo");

app.MapGet("/vehicle", (int? pagina, IVehicleServico vehicleServico) =>
{
    var vehicle = vehicleServico.Todos(pagina);

    if (vehicle is null)
        return Results.NotFound("Nenhum veículo encontrado.");

    return Results.Ok(vehicle);
}).WithTags("Veiculo");

app.MapGet("/vehicle/{id}", ([FromRoute] int id, IVehicleServico vehicleServico) =>
{
    var vehicles = vehicleServico.BuscarPorId(id);

    return Results.Ok(vehicles);
}).WithTags("Veiculo");

app.MapPut("/vehicle/{id}", ([FromRoute] int id, VehicleDTO vehicleDTO, IVehicleServico vehicleServico) =>
{
    var vehicle = vehicleServico.BuscarPorId(id);
    if (vehicle is null)
        return Results.NotFound("Nenhum veículo encontrado.");

    var validacao = validaDto(vehicleDTO);
    if (validacao.Mensagens.Count > 0)
        return Results.BadRequest(validacao);

    vehicle.Nome = vehicleDTO.Nome;
    vehicle.Marca = vehicleDTO.Marca;
    vehicle.Ano = vehicleDTO.Ano;

    vehicleServico.Atualizar(vehicle);

    return Results.Ok(vehicle);
}).WithTags("Veiculo");

app.MapDelete("/vehicle/{id}", ([FromRoute] int id, IVehicleServico vehicleServico) =>
{
    var vehicle = vehicleServico.BuscarPorId(id);
    if (vehicle is null)
        return Results.NotFound("Nenhum veículo encontrado.");

    vehicleServico.Apagar(vehicle);

    return Results.NoContent();
}).WithTags("Veiculo");
#endregion

/// Garante subir todas as migrations pendentes
using (var scope = app.Services.CreateScope())
{
    var service = scope.ServiceProvider;
    var context = service.GetRequiredService<DbContextDio>();
    context.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();

app.Run();
