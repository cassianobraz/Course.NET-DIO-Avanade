using API.EF.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.EF.Api.Context;

public class AgendaContext : DbContext
{
    public AgendaContext(DbContextOptions<AgendaContext> options) : base(options)
    {
    }

    public DbSet<Contato> Contatos { get; set; }

    }
