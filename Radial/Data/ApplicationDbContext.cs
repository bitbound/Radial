﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Radial.Data.Entities;
using Radial.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;

namespace Radial.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<CharacterEffect> CharacterEffects { get; set; }
        public DbSet<EventLogEntry> EventLogs { get; set; }

        public DbSet<Location> Locations { get; set; }

        public DbSet<PlayerCharacter> PlayerCharacters { get; set; }

        public DbSet<CharacterInfo> Characters { get; set; }

        public DbSet<Npc> Npcs { get; set; }

        public new DbSet<RadialUser> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<IdentityUser>().ToTable("Users");

            builder.Entity<CharacterInfo>()
                .HasOne(x => x.Location)
                .WithMany(x => x.Characters);

            builder.Entity<Location>()
                .HasMany(x => x.Characters)
                .WithOne(x => x.Location);

            builder.Entity<PlayerCharacter>()
                .HasOne(x => x.User)
                .WithOne(x => x.Character)
                .HasForeignKey<PlayerCharacter>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                // SQLite does not have proper support for DateTimeOffset via Entity Framework Core, see the limitations
                // here: https://docs.microsoft.com/en-us/ef/core/providers/sqlite/limitations#query-limitations
                // To work around this, when the SQLite database provider is used, all model properties of type DateTimeOffset
                // use the DateTimeOffsetToBinaryConverter
                // Based on: https://github.com/aspnet/EntityFrameworkCore/issues/10784#issuecomment-415769754
                // This only supports millisecond precision, but should be sufficient for most use cases.
                foreach (var entityType in builder.Model.GetEntityTypes())
                {
                    if (entityType.IsKeyless)
                    {
                        continue;
                    }
                    var properties = entityType.ClrType.GetProperties().Where(p => p.PropertyType == typeof(DateTimeOffset)
                                                                                || p.PropertyType == typeof(DateTimeOffset?));
                    foreach (var property in properties)
                    {
                        if (property.CustomAttributes.Any(x => x.AttributeType == typeof(NotMappedAttribute)))
                        {
                            continue;
                        }
                        builder
                             .Entity(entityType.Name)
                             .Property(property.Name)
                             .HasConversion(new DateTimeOffsetToStringConverter());
                    }
                }
            }
        }
    }
}
