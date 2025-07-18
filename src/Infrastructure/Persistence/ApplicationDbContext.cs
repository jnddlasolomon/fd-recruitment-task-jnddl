using System.Reflection;
using Duende.IdentityServer.EntityFramework.Options;
using MediatR;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Todo_App.Application.Common.Interfaces;
using Todo_App.Domain.Entities;
using Todo_App.Infrastructure.Identity;
using Todo_App.Infrastructure.Persistence.Interceptors;

namespace Todo_App.Infrastructure.Persistence;

public class ApplicationDbContext : ApiAuthorizationDbContext<ApplicationUser>, IApplicationDbContext
{
    private readonly IMediator _mediator;
    private readonly AuditableEntitySaveChangesInterceptor _auditableEntitySaveChangesInterceptor;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IOptions<OperationalStoreOptions> operationalStoreOptions,
        IMediator mediator,
        AuditableEntitySaveChangesInterceptor auditableEntitySaveChangesInterceptor)
        : base(options, operationalStoreOptions)
    {
        _mediator = mediator;
        _auditableEntitySaveChangesInterceptor = auditableEntitySaveChangesInterceptor;
    }

    public DbSet<TodoList> TodoLists => Set<TodoList>();

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    // Feature 2: Tags support
    public DbSet<Tag> Tags => Set<Tag>();

    public DbSet<TodoItemTag> TodoItemTags => Set<TodoItemTag>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Feature 2: Configure many-to-many relationship for TodoItems and Tags
        builder.Entity<TodoItemTag>()
            .HasKey(tt => new { tt.TodoItemId, tt.TagId });

        builder.Entity<TodoItemTag>()
            .HasOne(tt => tt.TodoItem)
            .WithMany(t => t.TodoItemTags)
            .HasForeignKey(tt => tt.TodoItemId);

        builder.Entity<TodoItemTag>()
            .HasOne(tt => tt.Tag)
            .WithMany(t => t.TodoItemTags)
            .HasForeignKey(tt => tt.TagId);

        // Ensure tag names are unique
        builder.Entity<Tag>()
            .HasIndex(t => t.Name)
            .IsUnique();

        // Feature 3: Add global query filters for soft delete
        builder.Entity<TodoItem>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<TodoList>().HasQueryFilter(e => !e.IsDeleted);

        base.OnModelCreating(builder);

    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_auditableEntitySaveChangesInterceptor);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _mediator.DispatchDomainEvents(this);

        return await base.SaveChangesAsync(cancellationToken);
    }
}