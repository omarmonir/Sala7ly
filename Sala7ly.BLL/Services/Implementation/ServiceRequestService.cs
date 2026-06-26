using Sala7ly.BLL.DTOs.ServiceRequestDTOs;
using Sala7ly.BLL.Mapper;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Enums;
using Sala7ly.DAL.Repositories.Abstraction;

namespace Sala7ly.BLL.Services.Implementation
{
    public class ServiceRequestService : IServiceRequestService
    {
        private readonly IServiceRequestRepository _serviceRequestRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IAddressRepository _addressRepository;   // FIX: injected
        private readonly INotificationService _notificationService;

        public ServiceRequestService(
            IServiceRequestRepository serviceRequestRepository,
            ICustomerRepository customerRepository,
            IAddressRepository addressRepository,               // FIX: added
            INotificationService notificationService)
        {
            _serviceRequestRepository = serviceRequestRepository;
            _customerRepository = customerRepository;
            _addressRepository = addressRepository;
            _notificationService = notificationService;
        }

        // ── READ ──────────────────────────────────────────────────────────────

        public async Task<ServiceRequestDetailsDto?> GetByIdAsync(int id)
        {
            var request = await _serviceRequestRepository.GetByIdAsync(id);
            if (request is null) return null;
            return ServiceRequestMapper.ToDetailsDto(request);
        }

        public async Task<IEnumerable<ServiceRequestListItemDto>> GetByCustomerIdAsync(int customerId)
        {
            var requests = await _serviceRequestRepository.GetByCustomerIdAsync(customerId);
            return requests.Select(ServiceRequestMapper.ToListItemDto);
        }

        public async Task<IEnumerable<ServiceRequestListItemDto>> GetOpenRequestsAsync()
        {
            var requests = await _serviceRequestRepository.GetOpenRequestsAsync();
            return requests.Select(ServiceRequestMapper.ToListItemDto);
        }

        public async Task<IEnumerable<ServiceRequestListItemDto>> GetAllAsync()
        {
            var requests = await _serviceRequestRepository.GetAllAsync();
            return requests.Select(ServiceRequestMapper.ToListItemDto);
        }

        public async Task<IEnumerable<ServiceRequestListItemDto>> GetMineAsync(string userId)
        {
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer is null) return Enumerable.Empty<ServiceRequestListItemDto>();

            var requests = await _serviceRequestRepository.GetByCustomerIdAsync(customer.Id);
            return requests.Select(ServiceRequestMapper.ToListItemDto);
        }

        public async Task<IEnumerable<ServiceRequestListItemDto>> GetAssignedAsync(string userId)
        {
            var requests = await _serviceRequestRepository.GetAssignedByTechnicianUserIdAsync(userId);
            return requests.Select(ServiceRequestMapper.ToListItemDto);
        }

        // ── CREATE ────────────────────────────────────────────────────────────

        public async Task<bool> CreateAsync(string userId, CreateServiceRequestDto dto)
        {
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer is null) return false;

            // ------------------------------------------------------------------
            // FIX — resolve the real AddressId.
            //
            // BEFORE: the frontend always sent addressId = 1 (hardcoded), so every
            // request was linked to the same dummy address row ("s", "s", "s").
            //
            // NOW:
            //   • If dto.AddressId > 0  → the customer chose a saved address.
            //     Verify it exists and belongs to this customer, then use its ID.
            //   • Otherwise             → the customer typed a free-text address.
            //     Create a new Address row from ServiceAddress/City/District,
            //     save it, and use the generated ID.
            //
            // Note: Address.CustomerId maps to CustomerProfile.Id (not User.Id).
            // ------------------------------------------------------------------
            int resolvedAddressId;

            if (dto.AddressId.HasValue && dto.AddressId.Value > 0)
            {
                var existing = await _addressRepository.GetByIdAsync(dto.AddressId.Value);
                // make sure the address belongs to this customer
                if (existing is null || existing.CustomerId != customer.Id)
                    return false;

                resolvedAddressId = existing.Id;
            }
            else
            {
                // Must have something to save
                if (string.IsNullOrWhiteSpace(dto.ServiceAddress))
                    return false;

                // Use the Address parameterised constructor:
                // Address(int customerId, string title, string street, string city, string district)
                var newAddress = new Address(
                    customerId: customer.Id,                             // CustomerProfile.Id
                    title: dto.ServiceAddress.Trim(),
                    street: dto.ServiceAddress.Trim(),
                    city: dto.City?.Trim() ?? dto.ServiceAddress.Trim(),
                    district: dto.District?.Trim() ?? string.Empty
                );
                newAddress.MarkCreated(userId);

                await _addressRepository.AddAsync(newAddress);
                await _addressRepository.SaveChangesAsync();            // gets the new Id

                resolvedAddressId = newAddress.Id;
            }

            // ── images ────────────────────────────────────────────────────────
            var imageUrls = new List<string>();
            if (dto.Images != null && dto.Images.Any())
            {
                foreach (var image in dto.Images)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                    var folderPath = Path.Combine("wwwroot", "uploads", "requests");

                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    var filePath = Path.Combine(folderPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                        await image.CopyToAsync(stream);

                    imageUrls.Add($"/uploads/requests/{fileName}");
                }
            }

            // ── create request with the REAL address ID ───────────────────────
            var request = new ServiceRequest(
                dto.Title,
                dto.Description,
                imageUrls,
                dto.Urgency,
                dto.BookingMode,
                dto.IsEmergency,
                dto.ScheduledAt,
                customer.Id,
                resolvedAddressId,   // ← real ID, never hardcoded
                dto.CategoryId
            );
            request.MarkCreated(userId);

            await _serviceRequestRepository.AddAsync(request);
            await _serviceRequestRepository.SaveChangesAsync();
            return true;
        }

        // ── LIFECYCLE ─────────────────────────────────────────────────────────

        public async Task<bool> StartProgressAsync(int id)
        {
            var request = await _serviceRequestRepository.GetByIdWithPartiesAsync(id);
            if (request is null) return false;

            request.MarkInProgress();
            _serviceRequestRepository.Update(request);
            await _serviceRequestRepository.SaveChangesAsync();

            var customerUserId = request.Profile?.UserId;
            if (!string.IsNullOrEmpty(customerUserId))
            {
                await _notificationService.NotifyUserAsync(
                    userId: customerUserId,
                    type: NotificationType.system,
                    title: "بدأ العمل على طلبك 🛠️",
                    body: "قام الفني ببدء العمل على طلبك.",
                    metadata: $"{{\"requestId\": {request.Id}}}");
            }

            return true;
        }

        public async Task<bool> CompleteAsync(int id)
        {
            var request = await _serviceRequestRepository.GetByIdWithPartiesAsync(id);
            if (request is null) return false;

            request.MarkCompleted();
            _serviceRequestRepository.Update(request);
            await _serviceRequestRepository.SaveChangesAsync();

            var technicianUserId = request.SelectedBid?.Technician?.UserId;
            if (!string.IsNullOrEmpty(technicianUserId))
            {
                await _notificationService.NotifyUserAsync(
                    userId: technicianUserId,
                    type: NotificationType.system,
                    title: "تم إكمال الطلب ✅",
                    body: "تم وضع علامة \"مكتمل\" على أحد الطلبات التي قمت بها.",
                    metadata: $"{{\"requestId\": {request.Id}}}");
            }

            return true;
        }

        public async Task<bool> UpdateAsync(int id, UpdateServiceRequestDto dto)
        {
            var request = await _serviceRequestRepository.GetByIdAsync(id);
            if (request is null) return false;

            request.UpdateDetails(dto.Title, dto.Description, dto.ScheduledAt);
            _serviceRequestRepository.Update(request);
            await _serviceRequestRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var request = await _serviceRequestRepository.GetByIdAsync(id);
            if (request is null) return false;

            _serviceRequestRepository.Delete(request);
            await _serviceRequestRepository.SaveChangesAsync();
            return true;
        }
    }
}