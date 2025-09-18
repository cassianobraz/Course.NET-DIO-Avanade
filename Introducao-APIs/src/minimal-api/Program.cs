using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using minimal_api.Domain.DTOs;
using minimal_api.Domain.Entity;
using minimal_api.Domain.Enuns;
using minimal_api.Domain.Interfaces;
using minimal_api.Domain.ModelViews;
using minimal_api.Domain.Servicos;
using minimal_api.Infra.Db;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var key = builder.Configuration.GetSection("Jwt").ToString();

if (string.IsNullOrEmpty(key)) key = "123456";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(option =>
{
    option.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<IAdminServico, AdminServico>();
builder.Services.AddScoped<IVehicleServico, VehicleServico>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT desta forma: Token"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

builder.Services.AddDbContext<DbContextDio>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var app = builder.Build();

#region Admin
string GerarTokenJwt(Admin admin)
{
    if (string.IsNullOrEmpty(key)) return string.Empty;

    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new List<Claim>()
    {
        new Claim("Email", admin.Email),
        new Claim("Perfil", admin.Perfil)
    };

    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.Now.AddDays(1),
        signingCredentials: credentials
        );

    return new JwtSecurityTokenHandler().WriteToken(token);
}


app.MapPost("/admin/login", ([FromBody] LoginDTO loginDTO, IAdminServico adminServico) =>
{
    var adm = adminServico.Login(loginDTO);

    if (adm != null)
    {
        var token = GerarTokenJwt(adm);
        return Results.Ok(new AdmLogado
        {
            Email = adm.Email,
            Perfil = adm.Perfil,
            Token = token
        });
    }
    else
        return Results.Unauthorized();
}).AllowAnonymous().WithTags("Administrador");

app.MapGet("/admin", ([FromQuery] int? pagina, IAdminServico adminServico) =>
{
    var adms = new List<AdministradorModelView>();
    var administradores = adminServico.Todos(pagina);
    foreach (var adm in administradores)
    {
        adms.Add(new AdministradorModelView
        {
            Id = adm.Id,
            Email = adm.Email,
            Perfil = adm.Perfil
        });
    }
    return Results.Ok(adms);
}).RequireAuthorization().WithTags("Administrador");

app.MapGet("/admin/{id}", ([FromRoute] int id, IAdminServico adminServico) =>
{
    var administrador = adminServico.BuscarPorId(id);

    if (administrador is null)
        return Results.NotFound();

    return Results.Ok(new AdministradorModelView
    {
        Id = administrador.Id,
        Email = administrador.Email,
        Perfil = administrador.Perfil
    });
}).RequireAuthorization().WithTags("Administrador");

app.MapPost("/admin", ([FromBody] AdministradorDTO administradorDTO, IAdminServico adminServico) =>
{
    var validacao = new ErrosDeValidacao
    {
        Mensagens = new List<string>()
    };

    if (string.IsNullOrEmpty(administradorDTO.Email))
        validacao.Mensagens.Add("Email não pode ser vazio");

    if (string.IsNullOrEmpty(administradorDTO.Senha))
        validacao.Mensagens.Add("Senha não pode ser vazia");

    if (administradorDTO.Perfil is null)
        validacao.Mensagens.Add("Perfil não pode ser vazio");

    var administrador = new Admin
    {
        Email = administradorDTO.Email,
        Senha = administradorDTO.Senha,
        Perfil = administradorDTO.Perfil.ToString() ?? Perfil.Editor.ToString()
    };

    adminServico.Incluir(administrador);

    return Results.Created($"/administrador/{administrador.Id}", new AdministradorModelView
    {
        Id = administrador.Id,
        Email = administrador.Email,
        Perfil = administrador.Perfil
    });

}).RequireAuthorization().WithTags("Administrador");
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
}).RequireAuthorization().WithTags("Veiculo");

app.MapGet("/vehicle", (int? pagina, IVehicleServico vehicleServico) =>
{
    var vehicle = vehicleServico.Todos(pagina);

    if (vehicle is null)
        return Results.NotFound("Nenhum veículo encontrado.");

    return Results.Ok(vehicle);
}).RequireAuthorization().WithTags("Veiculo");

app.MapGet("/vehicle/{id}", ([FromRoute] int id, IVehicleServico vehicleServico) =>
{
    var vehicles = vehicleServico.BuscarPorId(id);

    if (vehicles is null)
        return Results.NotFound();

    return Results.Ok(vehicles);
}).RequireAuthorization().WithTags("Veiculo");

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
}).RequireAuthorization().WithTags("Veiculo");

app.MapDelete("/vehicle/{id}", ([FromRoute] int id, IVehicleServico vehicleServico) =>
{
    var vehicle = vehicleServico.BuscarPorId(id);
    if (vehicle is null)
        return Results.NotFound("Nenhum veículo encontrado.");

    vehicleServico.Apagar(vehicle);

    return Results.NoContent();
}).RequireAuthorization().WithTags("Veiculo");
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

app.UseAuthentication();
app.UseAuthorization();

app.Run();
