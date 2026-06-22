using HouseBookingRestApi.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace HouseBookingRestApi.Tests.Helpers
{
    public static class TestDbContextFactory
    {
        public static HouseBookingRestApiContext Create()
        {
            DbContextOptions<HouseBookingRestApiContext> options;

            options = new DbContextOptionsBuilder<HouseBookingRestApiContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new HouseBookingRestApiContext(options);
        }
    }
}
