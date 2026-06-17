namespace HouseBookingRestApi.Repositories
{
    public interface IUnitOfWork
    {
        IUserRepository UserRepository { get; }

        IOwnerRepository OwnerRepository { get; }

        IRenterRepository RenterRepository { get; }

        IHouseRepository HouseRepository { get; }

        IBookingRepository BookingRepository { get; }

        IHouseImageRepository HouseImageRepository { get; }

        IRoleRepository RoleRepository { get; }

        Task<bool> SaveAsync();
    }
}
