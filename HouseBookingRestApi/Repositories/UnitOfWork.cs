using HouseBookingRestApi.Data;

namespace HouseBookingRestApi.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly HouseBookingRestApiContext _context;

        public IUserRepository UserRepository { get; }

        public IRoleRepository RoleRepository { get; }

        public IRenterRepository RenterRepository { get; }

        public IOwnerRepository OwnerRepository { get; }

        public IHouseRepository HouseRepository { get; }

        public IBookingRepository BookingRepository { get; }

        public IHouseImageRepository HouseImageRepository { get; }

        public ICapabilityRepository CapabilityRepository { get; }

        public UnitOfWork(HouseBookingRestApiContext context)
        {
            _context = context;
            UserRepository = new UserRepository(context);
            RoleRepository = new RoleRepository(context);
            RenterRepository = new RenterRepository(context);
            OwnerRepository = new OwnerRepository(context);
            HouseRepository = new HouseRepository(context);
            HouseImageRepository = new HouseImageRepository(context);
            BookingRepository = new BookingRepository(context);
            CapabilityRepository = new CapabilityRepository(context);
        }

        public async Task<bool> SaveAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
