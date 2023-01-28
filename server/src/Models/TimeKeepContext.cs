using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace TimeKeep.Models;

public class TimeKeepContext : DbContext
{
	private readonly IConfiguration configuration;

	public DbSet<Entry> Entries => Set<Entry>();
	public DbSet<Project> Projects => Set<Project>();
	public DbSet<Category> Categories => Set<Category>();

	public TimeKeepContext(IConfiguration configuration, DbContextOptions<TimeKeepContext> options) : base(options) =>
		this.configuration = configuration;

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
		optionsBuilder.UseNpgsql(configuration.GetConnectionString("TimeKeepDatabase"));
}
