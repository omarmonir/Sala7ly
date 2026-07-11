using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Sala7ly.DAL.DataBase;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Enums;
using Sala7ly.DAL.Repositories.Implementation;
using Salla7ly_Testing.Helpers;

namespace Salla7ly_Testing.Repositories
{
    public class ServiceRequestRepositoryTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly ServiceRequestRepository _sut;

        public ServiceRequestRepositoryTests()
        {
            _context = TestDbContextFactory.Create();
            _sut = new ServiceRequestRepository(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private ServiceRequest CreateRequest(
            int customerId = 1,
            int categoryId = 1,
            int addressId = 1)
        {
            return new ServiceRequest(
                title: "Fix AC",
                description: "Not cooling",
                imageUrls: new List<string>(),
                urgency: Urgency.low,
                bookingMode: BookingMode.bidding,
                isEmergency: false,
                scheduledAt: DateTime.UtcNow.AddDays(1),
                customerId: customerId,
                addressId: addressId,
                categoryId: categoryId
            );
        }

        private async Task SeedCustomerAsync(int id, string userId = "user-1")
        {
            if (_context.Users.Any(u => u.Id == userId)) return;

            var user = new User
            {
                Id = userId,
                Name = "Test",
                Email = $"{userId}@t.com",
                UserName = $"{userId}@t.com"
            };
            var customer = new CustomerProfile
            {
                Id = id,
                UserId = userId,
                User = user,
                TotalRequests = 0,
                TotalReviews = 0,
                TotalSpent = 0
            };
            _context.Users.Add(user);
            _context.CustomerProfiles.Add(customer);
            await _context.SaveChangesAsync();
        }

        private async Task<(ServiceCategory category, Address address)> SeedCategoryAndAddressAsync(
            int customerId = 1,
            int categoryId = 1)
        {
            // Use the existing constructor that requires parameters.
            // Signature inferred from diagnostics: ServiceCategory(string nameAr, int? parentCategoryId, bool isActive)
            var category = new ServiceCategory("سباكة", parentCategoryId: null, isActive: true)
            {
                Id = categoryId,
                
            };

            var address = new Address(customerId, "Home", "Main St", "Cairo", "Nasr City");

            if (!_context.ServiceCategories.Any(c => c.Id == categoryId))
                _context.ServiceCategories.Add(category);

            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            return (category, address);
        }

        // ══════════════════════════════════════════════════════════════════════
        // AddAsync + SaveChangesAsync
        // ══════════════════════════════════════════════════════════════════════

        [Fact]
        public async Task AddAsync_ShouldPersistRequest_WhenSaveChangesCalled()
        {
            // Arrange
            await SeedCustomerAsync(1);
            var (_, address) = await SeedCategoryAndAddressAsync(customerId: 1);
            var request = CreateRequest(customerId: 1, addressId: address.Id);

            // Act
            await _sut.AddAsync(request);
            await _sut.SaveChangesAsync();

            // Assert
            var saved = await _context.ServiceRequests.FirstOrDefaultAsync();
            saved.Should().NotBeNull();
            saved!.Title.Should().Be("Fix AC");
            saved.Status.Should().Be(Status.open);
        }

        [Fact]
        public async Task AddAsync_ShouldNotPersist_WhenSaveChangesNotCalled()
        {
            // Arrange
            await SeedCustomerAsync(1);
            var (_, address) = await SeedCategoryAndAddressAsync(customerId: 1);

            // Act
            await _sut.AddAsync(CreateRequest(customerId: 1, addressId: address.Id));
            // Intentionally NOT calling SaveChangesAsync

            // Assert
            var count = await _context.ServiceRequests.CountAsync();
            count.Should().Be(0);
        }

        // ══════════════════════════════════════════════════════════════════════
        // GetByIdAsync
        // ══════════════════════════════════════════════════════════════════════

        [Fact]
        public async Task GetByIdAsync_ShouldReturnRequest_WhenExists()
        {
            // Arrange
            await SeedCustomerAsync(1);
            var (_, address) = await SeedCategoryAndAddressAsync(customerId: 1);
            var request = CreateRequest(customerId: 1, addressId: address.Id);
            _context.ServiceRequests.Add(request);
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetByIdAsync(request.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Title.Should().Be("Fix AC");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            // Act
            var result = await _sut.GetByIdAsync(9999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenSoftDeleted()
        {
            // Arrange
            await SeedCustomerAsync(1);
            var (_, address) = await SeedCategoryAndAddressAsync(customerId: 1);
            var request = CreateRequest(customerId: 1, addressId: address.Id);
            request.ToggaleStatus("admin");
            _context.ServiceRequests.Add(request);
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetByIdAsync(request.Id);

            // Assert
            result.Should().BeNull();
        }

        // ══════════════════════════════════════════════════════════════════════
        // GetAllAsync
        // ══════════════════════════════════════════════════════════════════════

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllNonDeleted_WhenCalled()
        {
            // Arrange
            await SeedCustomerAsync(1);
            var (_, address) = await SeedCategoryAndAddressAsync(customerId: 1);

            var r1 = CreateRequest(customerId: 1, addressId: address.Id);
            var r2 = CreateRequest(customerId: 1, addressId: address.Id);
            var r3 = CreateRequest(customerId: 1, addressId: address.Id);
            r3.ToggaleStatus("admin");

            _context.ServiceRequests.AddRange(r1, r2, r3);
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmpty_WhenNoRequests()
        {
            // Act
            var result = await _sut.GetAllAsync();

            // Assert
            result.Should().BeEmpty();
        }

        // ══════════════════════════════════════════════════════════════════════
        // GetOpenRequestsAsync
        // ══════════════════════════════════════════════════════════════════════

        [Fact]
        public async Task GetOpenRequestsAsync_ShouldReturnOnlyOpenRequests()
        {
            // Arrange
            await SeedCustomerAsync(1);
            var (_, address) = await SeedCategoryAndAddressAsync(customerId: 1);

            var open1 = CreateRequest(customerId: 1, addressId: address.Id);
            var open2 = CreateRequest(customerId: 1, addressId: address.Id);
            var assigned = CreateRequest(customerId: 1, addressId: address.Id);

            _context.ServiceRequests.AddRange(open1, open2, assigned);
            await _context.SaveChangesAsync();

            assigned.AssignBid(bidId: 1);
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetOpenRequestsAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(r => r.Status.Should().Be(Status.open));
        }

        [Fact]
        public async Task GetOpenRequestsAsync_ShouldExcludeSoftDeleted()
        {
            // Arrange
            await SeedCustomerAsync(1);
            var (_, address) = await SeedCategoryAndAddressAsync(customerId: 1);

            var open = CreateRequest(customerId: 1, addressId: address.Id);
            var deleted = CreateRequest(customerId: 1, addressId: address.Id);
            deleted.ToggaleStatus("admin");

            _context.ServiceRequests.AddRange(open, deleted);
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetOpenRequestsAsync();

            // Assert
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetOpenRequestsAsync_ShouldReturnEmpty_WhenNoOpenRequests()
        {
            // Arrange
            await SeedCustomerAsync(1);
            var (_, address) = await SeedCategoryAndAddressAsync(customerId: 1);

            var assigned = CreateRequest(customerId: 1, addressId: address.Id);
            _context.ServiceRequests.Add(assigned);
            await _context.SaveChangesAsync();

            assigned.AssignBid(bidId: 1);
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetOpenRequestsAsync();

            // Assert
            result.Should().BeEmpty();
        }

        // ══════════════════════════════════════════════════════════════════════
        // GetByCustomerIdAsync
        // ══════════════════════════════════════════════════════════════════════

        [Fact]
        public async Task GetByCustomerIdAsync_ShouldReturnOnlyCustomerRequests()
        {
            // Arrange
            await SeedCustomerAsync(1, "user-1");
            await SeedCustomerAsync(2, "user-2");
            var (_, address) = await SeedCategoryAndAddressAsync(customerId: 1);

            _context.ServiceRequests.AddRange(
                CreateRequest(customerId: 1, addressId: address.Id),
                CreateRequest(customerId: 1, addressId: address.Id),
                CreateRequest(customerId: 2, addressId: address.Id)
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetByCustomerIdAsync(1);

            // Assert
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(r => r.CustomerId.Should().Be(1));
        }

        [Fact]
        public async Task GetByCustomerIdAsync_ShouldReturnEmpty_WhenNoRequestsForCustomer()
        {
            // Arrange
            await SeedCustomerAsync(1);

            // Act
            var result = await _sut.GetByCustomerIdAsync(1);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByCustomerIdAsync_ShouldExcludeSoftDeleted()
        {
            // Arrange
            await SeedCustomerAsync(1);
            var (_, address) = await SeedCategoryAndAddressAsync(customerId: 1);

            var active = CreateRequest(customerId: 1, addressId: address.Id);
            var deleted = CreateRequest(customerId: 1, addressId: address.Id);
            deleted.ToggaleStatus("admin");

            _context.ServiceRequests.AddRange(active, deleted);
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetByCustomerIdAsync(1);

            // Assert
            result.Should().HaveCount(1);
        }

        // ══════════════════════════════════════════════════════════════════════
        // Update
        // ══════════════════════════════════════════════════════════════════════

        [Fact]
        public async Task Update_ShouldPersistChanges_WhenSaveChangesCalled()
        {
            // Arrange
            await SeedCustomerAsync(1);
            var (_, address) = await SeedCategoryAndAddressAsync(customerId: 1);
            var request = CreateRequest(customerId: 1, addressId: address.Id);
            _context.ServiceRequests.Add(request);
            await _context.SaveChangesAsync();

            // Act
            request.UpdateDetails("Updated Title", "Updated Desc", DateTime.UtcNow.AddDays(2));
            _sut.Update(request);
            await _sut.SaveChangesAsync();

            // Assert
            var updated = await _context.ServiceRequests.FindAsync(request.Id);
            updated!.Title.Should().Be("Updated Title");
            updated.Description.Should().Be("Updated Desc");
        }

        // ══════════════════════════════════════════════════════════════════════
        // Delete
        // ══════════════════════════════════════════════════════════════════════

        [Fact]
        public async Task Delete_ShouldRemoveFromContext_WhenCalled()
        {
            // Arrange
            await SeedCustomerAsync(1);
            var (_, address) = await SeedCategoryAndAddressAsync(customerId: 1);
            var request = CreateRequest(customerId: 1, addressId: address.Id);
            _context.ServiceRequests.Add(request);
            await _context.SaveChangesAsync();
            var id = request.Id;

            // Act
            _sut.Delete(request);
            await _sut.SaveChangesAsync();

            // Assert
            var deleted = await _context.ServiceRequests.FindAsync(id);
            deleted.Should().BeNull();
        }

        // ══════════════════════════════════════════════════════════════════════
        // SaveChangesAsync
        // ══════════════════════════════════════════════════════════════════════

        [Fact]
        public async Task SaveChangesAsync_ShouldReturnGreaterThanZero_WhenChangesSaved()
        {
            // Arrange
            await SeedCustomerAsync(1);
            var (_, address) = await SeedCategoryAndAddressAsync(customerId: 1);
            await _sut.AddAsync(CreateRequest(customerId: 1, addressId: address.Id));

            // Act
            var result = await _sut.SaveChangesAsync();

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task SaveChangesAsync_ShouldReturnZero_WhenNoChanges()
        {
            // Act
            var result = await _sut.SaveChangesAsync();

            // Assert
            result.Should().Be(0);
        }
    }
}