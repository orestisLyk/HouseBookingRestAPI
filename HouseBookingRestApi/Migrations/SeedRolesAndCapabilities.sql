-- =============================================
-- Seed Script for Roles and Capabilities
-- Run this script in SQL Server Management Studio (SSMS)
-- =============================================

USE [HouseBookerRest]
GO

-- =============================================
-- 1. Insert Capabilities
-- =============================================
SET IDENTITY_INSERT [dbo].[Capabilities] ON
GO

INSERT INTO [dbo].[Capabilities] ([Id], [Name], [Description])
VALUES 
	(1, 'CreateHouse', 'Ability to create a new house listing'),
	(2, 'UpdateHouse', 'Ability to update house information'),
	(3, 'DeleteHouse', 'Ability to delete a house listing'),
	(4, 'ViewHouse', 'Ability to view house listings'),
	(5, 'CreateBooking', 'Ability to create a booking'),
	(6, 'UpdateBooking', 'Ability to update booking information'),
	(7, 'CancelBooking', 'Ability to cancel a booking'),
	(8, 'ViewBooking', 'Ability to view booking details'),
	(9, 'ManageUsers', 'Ability to manage user accounts'),
	(10, 'ViewAllBookings', 'Ability to view all bookings in the system'),
	(11, 'ManageRoles', 'Ability to manage roles and capabilities'),
	(12, 'ViewReports', 'Ability to view system reports')
GO

SET IDENTITY_INSERT [dbo].[Capabilities] OFF
GO

-- =============================================
-- 2. Insert Roles
-- =============================================
SET IDENTITY_INSERT [dbo].[Roles] ON
GO

INSERT INTO [dbo].[Roles] ([Id], [Name], [Description])
VALUES 
	(1, 'Admin', 'Administrator with full system access'),
	(2, 'Owner', 'Property owner who can list and manage houses'),
	(3, 'Renter', 'User who can browse and book houses')
GO

SET IDENTITY_INSERT [dbo].[Roles] OFF
GO

-- =============================================
-- 3. Insert RolesCapabilities (Many-to-Many)
-- =============================================

-- Admin Role - All Capabilities
INSERT INTO [dbo].[RolesCapabilities] ([RolesId], [CapabilitiesId])
VALUES 
	(1, 1),   -- Admin - CreateHouse
	(1, 2),   -- Admin - UpdateHouse
	(1, 3),   -- Admin - DeleteHouse
	(1, 4),   -- Admin - ViewHouse
	(1, 5),   -- Admin - CreateBooking
	(1, 6),   -- Admin - UpdateBooking
	(1, 7),   -- Admin - CancelBooking
	(1, 8),   -- Admin - ViewBooking
	(1, 9),   -- Admin - ManageUsers
	(1, 10),  -- Admin - ViewAllBookings
	(1, 11),  -- Admin - ManageRoles
	(1, 12)   -- Admin - ViewReports
GO

-- Owner Role - House and Booking Management
INSERT INTO [dbo].[RolesCapabilities] ([RolesId], [CapabilitiesId])
VALUES 
	(2, 1),   -- Owner - CreateHouse
	(2, 2),   -- Owner - UpdateHouse
	(2, 3),   -- Owner - DeleteHouse
	(2, 4),   -- Owner - ViewHouse
	(2, 6),   -- Owner - UpdateBooking
	(2, 8)    -- Owner - ViewBooking
GO

-- Renter Role - View and Book Houses
INSERT INTO [dbo].[RolesCapabilities] ([RolesId], [CapabilitiesId])
VALUES 
	(3, 4),   -- Renter - ViewHouse
	(3, 5),   -- Renter - CreateBooking
	(3, 6),   -- Renter - UpdateBooking
	(3, 7),   -- Renter - CancelBooking
	(3, 8)    -- Renter - ViewBooking
GO

PRINT 'Seeding completed successfully!'
GO
