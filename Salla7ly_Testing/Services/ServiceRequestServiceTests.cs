using FluentAssertions;
using Moq;
using Sala7ly.BLL.DTOs.ServiceRequestDTOs;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.BLL.Services.Implementation;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Enums;
using Sala7ly.DAL.Repositories.Abstraction;

namespace Salla7ly_Testing.Services
{
    public class ServiceRequestServiceTests
    {
        private readonly Mock<IServiceRequestRepository> _requestRepoMock;
        private readonly Mock<ICustomerRepository> _customerRepoMock;
        private readonly Mock<IAddressRepository> _addressRepoMock;
        private readonly Mock<INotificationService> _notificationMock;
        private readonly Mock<ITechnicianProfileRepository> _technicianRepoMock;
        private readonly Mock<IRequestDispatchService> _dispatchMock;
        private readonly ServiceRequestService _sut;

        public ServiceRequestServiceTests()
        {
            _requestRepoMock = new Mock<IServiceRequestRepository>();
            _customerRepoMock = new Mock<ICustomerRepository>();
            _addressRepoMock = new Mock<IAddressRepository>();
            _notificationMock = new Mock<INotificationService>();
            _technicianRepoMock = new Mock<ITechnicianProfileRepository>();
            _dispatchMock = new Mock<IRequestDispatchService>();

            _sut = new ServiceRequestService(
                _requestRepoMock.Object,
                _customerRepoMock.Object,
                _addressRepoMock.Object,
                _notificationMock.Object,
                _technicianRepoMock.Object,
                _dispatchMock.Object
            );
        }

        private static ServiceRequest MakeRequest(int id = 1, int customerId = 10)
        {
            var req = new ServiceRequest(
                title: "Fix AC",
                description: "Not cooling",
                imageUrls: new List<string>(),
                urgency: Urgency.low,
                bookingMode: BookingMode.bidding,
                isEmergency: false,
                scheduledAt: DateTime.UtcNow.AddDays(1),
                customerId: customerId,
                addressId: 1,
                categoryId: 2
            );
            typeof(Sala7ly.DAL.Entities.BaseEntity)
                .GetProperty("Id")!
                .SetValue(req, id);
            return req;
        }

        private static CustomerProfile MakeCustomer(int id = 10, string userId = "user-1") =>
            new CustomerProfile
            {
                Id = id,
                UserId = userId,
                User = new User { Id = userId, Name = "Test User", Email = "test@test.com", UserName = "test@test.com" }
            };

        // NOTE: adjust the Address(...) constructor arguments below if your
        // actual Address entity's constructor signature differs.
        private static Address MakeAddress(int id = 1, int customerId = 10)
        {
            var address = new Address(
                customerId: customerId,
                title: "Home",
                street: "Main St",
                city: "Cairo",
                district: "Nasr City"
            );
            typeof(Sala7ly.DAL.Entities.BaseEntity)
                .GetProperty("Id")!
                .SetValue(address, id);
            return address;
        }

        private static CreateServiceRequestDto MakeCreateDto(int? addressId = 1) =>
            new CreateServiceRequestDto
            {
                Title = "Fix AC",
                Description = "Not cooling",
                Urgency = Urgency.low,
                BookingMode = BookingMode.bidding,
                IsEmergency = false,
                ScheduledAt = DateTime.UtcNow.AddDays(1),
                CategoryId = 2,
                AddressId = addressId,
                ServiceAddress = addressId == null ? "Main St" : null,
                City = "Cairo",
                District = "Nasr City"
            };

        // ── GetByIdAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_ShouldReturnDetailsDto_WhenRequestExists()
        {
            var request = MakeRequest(id: 1);
            _requestRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(request);

            var result = await _sut.GetByIdAsync(1);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.Title.Should().Be("Fix AC");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenRequestDoesNotExist()
        {
            _requestRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((ServiceRequest?)null);

            var result = await _sut.GetByIdAsync(999);

            result.Should().BeNull();
        }

        // ── GetOpenRequestsAsync ──────────────────────────────────────────────

        [Fact]
        public async Task GetOpenRequestsAsync_ShouldReturnMappedList_WhenRequestsExist()
        {
            _requestRepoMock.Setup(r => r.GetOpenRequestsAsync())
                .ReturnsAsync(new List<ServiceRequest> { MakeRequest(1), MakeRequest(2) });

            var result = await _sut.GetOpenRequestsAsync();

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetOpenRequestsAsync_ShouldReturnEmptyList_WhenNoOpenRequests()
        {
            _requestRepoMock.Setup(r => r.GetOpenRequestsAsync())
                .ReturnsAsync(new List<ServiceRequest>());

            var result = await _sut.GetOpenRequestsAsync();

            result.Should().BeEmpty();
        }

        // ── GetMineAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetMineAsync_ShouldReturnRequests_WhenCustomerExists()
        {
            var customer = MakeCustomer(id: 10, userId: "user-1");
            _customerRepoMock.Setup(r => r.GetByUserIdAsync("user-1")).ReturnsAsync(customer);
            _requestRepoMock.Setup(r => r.GetByCustomerIdAsync(10))
                .ReturnsAsync(new List<ServiceRequest> { MakeRequest(1, customerId: 10) });

            var result = await _sut.GetMineAsync("user-1");

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetMineAsync_ShouldReturnEmpty_WhenCustomerNotFound()
        {
            _customerRepoMock.Setup(r => r.GetByUserIdAsync("ghost")).ReturnsAsync((CustomerProfile?)null);

            var result = await _sut.GetMineAsync("ghost");

            result.Should().BeEmpty();
        }

        // ── CreateAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_ShouldReturnTrue_WhenCustomerExistsAndAddressIdProvided()
        {
            var customer = MakeCustomer(id: 10, userId: "user-1");
            var address = MakeAddress(id: 1, customerId: 10);

            _customerRepoMock.Setup(r => r.GetByUserIdAsync("user-1")).ReturnsAsync(customer);
            _addressRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(address);
            _requestRepoMock.Setup(r => r.AddAsync(It.IsAny<ServiceRequest>())).Returns(Task.CompletedTask);
            _requestRepoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
            _dispatchMock.Setup(d => d.MatchAndNotifyAsync(It.IsAny<ServiceRequest>(), "user-1"))
                .ReturnsAsync(new Sala7ly.BLL.DTOs.AiDTOs.SmartMatchingResultDto());

            var result = await _sut.CreateAsync("user-1", MakeCreateDto(addressId: 1));

            result.Should().BeTrue();
            _requestRepoMock.Verify(r => r.AddAsync(It.IsAny<ServiceRequest>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnFalse_WhenCustomerNotFound()
        {
            _customerRepoMock.Setup(r => r.GetByUserIdAsync("unknown")).ReturnsAsync((CustomerProfile?)null);

            var result = await _sut.CreateAsync("unknown", MakeCreateDto());

            result.Should().BeFalse();
            _requestRepoMock.Verify(r => r.AddAsync(It.IsAny<ServiceRequest>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_ShouldDispatchToTechnicians_WhenCreatedSuccessfully()
        {
            var customer = MakeCustomer(id: 10, userId: "user-1");
            var address = MakeAddress(id: 1, customerId: 10);

            _customerRepoMock.Setup(r => r.GetByUserIdAsync("user-1")).ReturnsAsync(customer);
            _addressRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(address);
            _requestRepoMock.Setup(r => r.AddAsync(It.IsAny<ServiceRequest>())).Returns(Task.CompletedTask);
            _requestRepoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
            _dispatchMock.Setup(d => d.MatchAndNotifyAsync(It.IsAny<ServiceRequest>(), "user-1"))
                .ReturnsAsync(new Sala7ly.BLL.DTOs.AiDTOs.SmartMatchingResultDto());

            await _sut.CreateAsync("user-1", MakeCreateDto());

            _dispatchMock.Verify(d => d.MatchAndNotifyAsync(It.IsAny<ServiceRequest>(), "user-1"), Times.Once);
        }

        // ── CompleteAsync ─────────────────────────────────────────────────────

        [Fact]
        public async Task CompleteAsync_ShouldReturnTrue_WhenRequestExistsAndIsAssigned()
        {
            var request = MakeRequest(id: 1);
            request.AssignBid(bidId: 1);
            _requestRepoMock.Setup(r => r.GetByIdWithPartiesAsync(1)).ReturnsAsync(request);
            _requestRepoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _sut.CompleteAsync(1);

            result.Should().BeTrue();
            request.Status.Should().Be(Status.completed);
        }

        [Fact]
        public async Task CompleteAsync_ShouldReturnFalse_WhenRequestNotFound()
        {
            _requestRepoMock.Setup(r => r.GetByIdWithPartiesAsync(999)).ReturnsAsync((ServiceRequest?)null);

            var result = await _sut.CompleteAsync(999);

            result.Should().BeFalse();
        }

        // ── DeleteAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteAsync_ShouldReturnTrue_WhenRequestExists()
        {
            var request = MakeRequest(id: 1);
            _requestRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(request);
            _requestRepoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _sut.DeleteAsync(1);

            result.Should().BeTrue();
            _requestRepoMock.Verify(r => r.Delete(request), Times.Once);
            _requestRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenRequestNotFound()
        {
            _requestRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((ServiceRequest?)null);

            var result = await _sut.DeleteAsync(999);

            result.Should().BeFalse();
        }
    }
}