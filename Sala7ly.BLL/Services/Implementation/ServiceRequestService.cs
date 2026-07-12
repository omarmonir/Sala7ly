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
        private readonly IAddressRepository _addressRepository;
        private readonly INotificationService _notificationService;
        private readonly ITechnicianProfileRepository _technicianProfileRepository;
        private readonly IRequestDispatchService _requestDispatchService;
        private readonly IEscrowRepository _escrowRepository;
        private readonly IWalletService _walletService;

        public ServiceRequestService(
            IServiceRequestRepository serviceRequestRepository,
            ICustomerRepository customerRepository,
            IAddressRepository addressRepository,
            INotificationService notificationService,
            ITechnicianProfileRepository technicianProfileRepository,
            IRequestDispatchService requestDispatchService,
            IEscrowRepository escrowRepository,
            IWalletService walletService)
        {
            _serviceRequestRepository = serviceRequestRepository;
            _customerRepository = customerRepository;
            _addressRepository = addressRepository;
            _notificationService = notificationService;
            _technicianProfileRepository = technicianProfileRepository;
            _requestDispatchService = requestDispatchService;
            _escrowRepository = escrowRepository;
            _walletService = walletService;
            _technicianProfileRepository = technicianProfileRepository;
            _requestDispatchService = requestDispatchService;
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


        public async Task<IEnumerable<ServiceRequestListItemDto>> GetOpenRequestsForTechnicianAsync(
    string technicianUserId)
        {
            var categoryIds = await _technicianProfileRepository
                .GetCategoryIdsByUserIdAsync(technicianUserId);
            Console.WriteLine($"Categories Count = {categoryIds.Count}");

            Console.WriteLine($"TechnicianId = {technicianUserId}");
            Console.WriteLine($"Categories Count = {categoryIds.Count}");

            foreach (var cat in categoryIds)
                Console.WriteLine($"Category = {cat}");

            if (categoryIds.Count == 0)
                return Enumerable.Empty<ServiceRequestListItemDto>();

            var requests = await _serviceRequestRepository
                .GetOpenRequestsByCategoryIdsAsync(categoryIds);

            Console.WriteLine($"Requests Count = {requests.Count()}");

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

            // ── Resolve address ───────────────────────────────────────────────
            int resolvedAddressId;

            if (dto.AddressId.HasValue && dto.AddressId.Value > 0)
            {
                var existing = await _addressRepository.GetByIdAsync(dto.AddressId.Value);
                if (existing is null || existing.CustomerId != customer.Id)
                    return false;

                resolvedAddressId = existing.Id;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(dto.ServiceAddress))
                    return false;

                var newAddress = new Address(
                    customerId: customer.Id,
                    title: dto.ServiceAddress.Trim(),
                    street: dto.ServiceAddress.Trim(),
                    city: dto.City?.Trim() ?? dto.ServiceAddress.Trim(),
                    district: dto.District?.Trim() ?? string.Empty
                );
                newAddress.MarkCreated(userId);

                await _addressRepository.AddAsync(newAddress);
                await _addressRepository.SaveChangesAsync();

                resolvedAddressId = newAddress.Id;
            }

            // ── Images ────────────────────────────────────────────────────────
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

            // ── Persist the request ───────────────────────────────────────────
            var request = new ServiceRequest(
                dto.Title,
                dto.Description,
                imageUrls,
                dto.Urgency,
                dto.BookingMode,
                dto.IsEmergency,
                dto.ScheduledAt,
                customer.Id,
                resolvedAddressId,
                dto.CategoryId
            );
            request.MarkCreated(userId);

            await _serviceRequestRepository.AddAsync(request);
            await _serviceRequestRepository.SaveChangesAsync();

            // ── AI-powered smart matching + notification dispatch ──────────────
            // Runs after the request is persisted so the AI has a real request.Id
            // to embed in notifications.  Wrapped in its own try/catch inside
            // SmartMatchingService so a failure here never returns false to the
            // customer.
            await _requestDispatchService.MatchAndNotifyAsync(request, userId);

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

            // Release payment from escrow to technician wallet
            var escrow = await _escrowRepository.GetByRequestIdAsync(id);
            if (escrow != null && escrow.Status == EscrowStatus.Held)
            {
                // Mark escrow as released
                escrow.MarkReleased();
                await _escrowRepository.SaveChangesAsync();

                // Credit technician wallet with their payout (after platform fee)
                await _walletService.CreditAsync(
                    userId: escrow.Technician.UserId,
                    amount: escrow.TechnicianPayout,
                    description: $"أرباح طلب رقم #{escrow.ServiceRequestId}",
                    type: WalletTransactionType.payout,
                    escrowId: escrow.Id
                );

                // Auto-release from pending to available balance
                await _walletService.ReleasePendingBalanceAsync(escrow.Technician.UserId, escrow.TechnicianPayout);
            }

            var technicianUserId = request.SelectedBid?.Technician?.UserId;
            if (!string.IsNullOrEmpty(technicianUserId))
            {
                await _notificationService.NotifyUserAsync(
                    userId: technicianUserId,
                    type: NotificationType.system,
                    title: "تم إكمال الطلب ✅",
                    body: "تم وضع علامة \"مكتمل\" على أحد الطلبات التي قمت BitmapFactory وتم تحرير الدفع.",
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