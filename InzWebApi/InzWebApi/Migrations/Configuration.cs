namespace InzWebApi.Migrations
{
    using Microsoft.AspNet.Identity;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<InzWebApi.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(InzWebApi.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //var pwHash = new PasswordHasher();
            //var pw = pwHash.HashPassword("user1");
            //context.Users.AddOrUpdate(u => u.UserName,
            //    new Models.ApplicationUser
            //    {
            //        Email = "user1@domain.com",
            //        UserName = "user1@domain.com",
            //        PasswordHash = pw,
            //    });

            //context.Travelers.AddOrUpdate(u => u.Id,
            //    new Models.Traveler
            //    {
            //        Name = "Piotr",
            //        Surname = "Martyka",
            //        DateOfBirth = new DateTime(1993,5,5)
            //    },
            //    new Models.Traveler
            //    {
            //        Name = "Sebastian",
            //        Surname = "Wicher",
            //        DateOfBirth = new DateTime(1993,2,6)
            //    });

            //context.FriendLists.AddOrUpdate(u => u.Id,
            //    new Models.FriendList
            //    {
            //        OwnerId = 1,
            //        FriendId = 2
            //    });

            //context.Informations.AddOrUpdate(u => u.Id,
            //    new Models.Information
            //    {
            //        TravelerId = 1,
            //        Time = DateTime.Now,
            //        StartPlace = "Tarnów Opolski",
            //        EndPlace = "Opole"
            //    });

            //context.Informations.AddOrUpdate(u => u.Id,
            //    new Models.Information
            //    {
            //        TravelerId = 2,
            //        Time = DateTime.Now.AddHours(4),
            //        StartPlace = "Opole",
            //        EndPlace = "Tarnów Opolski"
            //    });

        }
    }
}
