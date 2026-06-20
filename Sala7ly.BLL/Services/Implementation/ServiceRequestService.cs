using Sala7ly.BLL.DTOs.ServiceRequestDTOs;
using Sala7ly.BLL.Mapper;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Repositories.Abstraction;

namespace Sala7ly.BLL.Services.Implementation
{
    public class ServiceRequestService : IServiceRequestService
    {
        private readonly IServiceRequestRepository _serviceRequestRepository;
        private readonly ICustomerRepository _customerRepository;

        public ServiceRequestService(IServiceRequestRepository serviceRequestRepository, ICustomerRepository customerRepository)
        {
            _serviceRequestRepository = serviceRequestRepository;
            _customerRepository = customerRepository;
        }

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

        public async Task<bool> CreateAsync(string userId, CreateServiceRequestDto dto)
        {
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer is null) return false;

            var imageUrls = new List<string>();
            if (dto.Images != null && dto.Images.Any())
            {
                foreach (var image in dto.Images)
                {
                    imageUrls.Add(image.FileName);
                }
            }

            var request = new ServiceRequest(
                dto.Title,
                dto.Description,
                imageUrls,
                dto.Urgency,
                dto.BookingMode,
                dto.IsEmergency,
                dto.ScheduledAt,
                customer.Id,
                dto.AddressId,
                dto.CategoryId
            );

            await _serviceRequestRepository.AddAsync(request);
            await _serviceRequestRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CompleteAsync(int id)
        {
            var request = await _serviceRequestRepository.GetByIdAsync(id);
            if (request is null) return false;

            request.MarkCompleted();
            _serviceRequestRepository.Update(request);
            await _serviceRequestRepository.SaveChangesAsync();
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

        public async Task<IEnumerable<ServiceRequestListItemDto>> GetMineAsync(string userId)
        {
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer is null) return Enumerable.Empty<ServiceRequestListItemDto>();

            var requests = await _serviceRequestRepository.GetByCustomerIdAsync(customer.Id);
            return requests.Select(ServiceRequestMapper.ToListItemDto);
        }
    }
}