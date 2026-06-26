using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Sala7ly.BLL.DTOs.VerificationDTOs;
using Sala7ly.BLL.Mapper;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Enums;
using Sala7ly.DAL.Repositories.Abstraction;

namespace Sala7ly.BLL.Services.Implementation
{
    public class TechnicianVerificationService : ITechnicianVerificationService
    {
        private readonly ITechnicianVerificationRepository _repository;
        private readonly ITechnicianProfileRepository _technicianRepository;
        private readonly IFilePathProvider _filePathProvider;
        private readonly INotificationService _notificationService;
        private readonly UserManager<User> _userManager;

        public TechnicianVerificationService(
            ITechnicianVerificationRepository repository,
            ITechnicianProfileRepository technicianRepository,
            IFilePathProvider filePathProvider,
            INotificationService notificationService,
            UserManager<User> userManager)
        {
            _repository = repository;
            _technicianRepository = technicianRepository;
            _filePathProvider = filePathProvider;
            _notificationService = notificationService;
            _userManager = userManager;
        }

        // ── Queries ──────────────────────────────────────────

        public async Task<VerificationDetailsDto?> GetByIdAsync(int id)
        {
            var verification = await _repository.GetByIdAsync(id);
            return verification is null ? null : VerificationMapper.ToDetailsDto(verification);
        }

        public async Task<List<VerificationDetailsDto>> GetByTechnicianIdAsync(int technicianId)
        {
            var list = await _repository.GetByTechnicianIdAsync(technicianId);
            return list.Select(VerificationMapper.ToDetailsDto).ToList();
        }

        public async Task<List<VerificationDetailsDto>> GetPendingAsync()
        {
            var list = await _repository.GetPendingAsync();
            return list.Select(VerificationMapper.ToDetailsDto).ToList();
        }

        // ── Commands ─────────────────────────────────────────

        public async Task<bool> SubmitAsync(SubmitVerificationDto dto, string userId)
        {
            if (dto.FrontImage is null || dto.FrontImage.Length == 0) return false;
            if (dto.BackImage is null || dto.BackImage.Length == 0) return false;

            var technician = await _technicianRepository.GetByUserIdAsync(userId);
            if (technician is null)
                return false;

            var frontUrl = await SaveDocumentAsync(dto.FrontImage);
            if (frontUrl is null) return false;

            var backUrl = await SaveDocumentAsync(dto.BackImage);
            if (backUrl is null) return false;

            var degreeUrls = new List<string>();
            if (dto.DegreeCertificates is not null)
            {
                foreach (var cert in dto.DegreeCertificates)
                {
                    var url = await SaveDocumentAsync(cert);
                    if (url is null) return false;
                    degreeUrls.Add(url);
                }
            }

            technician.Bio = dto.Bio;
            technician.ExperienceYears = dto.ExperienceYears;
            _technicianRepository.Update(technician);

            var verification = new TechnicianVerification
            {
                TechnicianId = technician.Id,
                IdNumber = dto.IdNumber,
                DocumentUrlFront = frontUrl,
                DocumentUrlBack = backUrl,
                DegreeCertificateUrls = degreeUrls,
                Status = VerificationStatus.Pending,
                SubmittedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(verification);
            var saved = await _repository.SaveChangesAsync();

            if (saved <= 0)
                return false;

            // notify every admin that a new verification needs review
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            foreach (var admin in admins)
            {
                await _notificationService.NotifyUserAsync(
                    userId: admin.Id,
                    type: NotificationType.verification,
                    title: "طلب توثيق جديد",
                    body: "قام أحد الفنيين بإرسال مستندات التوثيق للمراجعة.",
                    actorId: userId,
                    metadata: $"{{\"verificationId\": {verification.Id}, \"technicianId\": {technician.Id}}}");
            }

            return true;
        }

        public async Task<bool> ApproveAsync(int verificationId, string adminId)
        {
            var verification = await _repository.GetByIdAsync(verificationId);
            if (verification is null)
                return false;

            verification.Status = VerificationStatus.Approved;
            verification.ReviewedByAdminId = adminId;
            verification.ReviewedAt = DateTime.UtcNow;

            var tech = await _technicianRepository.GetByIdAsync(verification.TechnicianId);

            tech.IsApproved = true;

            _repository.Update(verification);
            _technicianRepository.Update(tech);
            await _repository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RejectAsync(RejectVerificationDto dto, string adminId)
        {
            var verification = await _repository.GetByIdAsync(dto.VerificationId);
            if (verification is null)
                return false;

            verification.Status = VerificationStatus.Rejected;
            verification.RejectionReason = dto.RejectionReason;
            verification.ReviewedByAdminId = adminId;
            verification.ReviewedAt = DateTime.UtcNow;

            _repository.Update(verification);
            await _repository.SaveChangesAsync();
            return true;
        }

        // ── Helpers ──────────────────────────────────────────

        private async Task<string?> SaveDocumentAsync(IFormFile file)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return null;

            const long maxSize = 5 * 1024 * 1024;
            if (file.Length > maxSize)
                return null;

            var webRoot = _filePathProvider.GetWebRootPath();
            var uploadsFolder = Path.Combine(webRoot, "uploads", "verifications");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/verifications/{fileName}";
        }
    }
}