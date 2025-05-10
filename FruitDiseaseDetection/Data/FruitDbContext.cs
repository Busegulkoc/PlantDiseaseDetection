using FruitDiseaseDetection.Models;
using Microsoft.EntityFrameworkCore;

namespace FruitDiseaseDetection.Data
{
    public class FruitDbContext(DbContextOptions<FruitDbContext> options) : DbContext(options)
    {
        public DbSet<Fruit> Fruits => Set<Fruit>();
        public DbSet<FruitVegetableDetails> FruitVegetableDetails => Set<FruitVegetableDetails>();
        public DbSet<Disease> Diseases => Set<Disease>();
        public DbSet<User> Users => Set<User>();
        public DbSet<ImageResult> ImageResults => Set<ImageResult>();
        public DbSet<UploadedImage> UploadedImages => Set<UploadedImage>();




        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Fruit>().HasData(
                new Fruit
                {
                    Id = 1,
                    Name = "Apple",
                    Species = "Gala",
                },
                new Fruit
                {
                    Id = 2,
                    Name = "Grape",
                    Species = "Vitis",
                },
                new Fruit
                {
                    Id = 3,
                    Name = "Peach",
                    Species = "Prunus",
                }
            );

        }
    }
}
