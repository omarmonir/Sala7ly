using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Enums;
using FluentAssertions;

namespace Salla7ly_Testing.Domain
{
    public class ServiceRequestTests
    {
        // ── Helper: creates a valid open request ──────────────────────────────
        private static ServiceRequest CreateOpenRequest() => new ServiceRequest(
            title: "Fix AC",
            description: "AC not cooling",
            imageUrls: new List<string>(),
            urgency: Urgency.low,
            bookingMode: BookingMode.bidding,
            isEmergency: false,
            scheduledAt: DateTime.UtcNow.AddDays(1),
            customerId: 1,
            addressId: 1,
            categoryId: 1
        );

        // ── Constructor Tests ─────────────────────────────────────────────────

        [Fact]
        public void Constructor_ShouldSetStatusToOpen_WhenCreated()
        {
            // Arrange & Act
            var request = CreateOpenRequest();

            // Assert
            // Why: Every new request MUST start as open — business rule
            request.Status.Should().Be(Status.open);
        }

        [Fact]
        public void Constructor_ShouldSetAllPropertiesCorrectly_WhenCreated()
        {
            // Arrange & Act
            var request = new ServiceRequest(
                title: "Fix AC",
                description: "AC not cooling",
                imageUrls: new List<string> { "img1.jpg" },
                urgency: Urgency.high,
                bookingMode: BookingMode.bidding,
                isEmergency: true,
                scheduledAt: new DateTime(2026, 8, 1),
                customerId: 5,
                addressId: 3,
                categoryId: 2
            );

            // Assert
            // Why: Verify constructor maps all parameters correctly
            request.Title.Should().Be("Fix AC");
            request.Description.Should().Be("AC not cooling");
            request.Urgency.Should().Be(Urgency.high);
            request.IsEmergency.Should().BeTrue();
            request.CustomerId.Should().Be(5);
            request.AddressId.Should().Be(3);
            request.CategoryId.Should().Be(2);
            request.ImageUrls.Should().ContainSingle();
        }

        // ── AssignBid Tests ───────────────────────────────────────────────────

        [Fact]
        public void AssignBid_ShouldSetStatusToAssigned_WhenRequestIsOpen()
        {
            // Arrange
            var request = CreateOpenRequest();

            // Act
            request.AssignBid(bidId: 42);

            // Assert
            // Why: AssignBid is the core business action — must change status
            request.Status.Should().Be(Status.assigned);
            request.SelectedBidId.Should().Be(42);
        }

        [Fact]
        public void AssignBid_ShouldThrowInvalidOperationException_WhenRequestIsNotOpen()
        {
            // Arrange
            var request = CreateOpenRequest();
            request.AssignBid(bidId: 1); // now assigned

            // Act & Assert
            // Why: Cannot assign twice — domain protection
            var act = () => request.AssignBid(bidId: 2);
            act.Should().Throw<InvalidOperationException>()
               .WithMessage("Request is not open.");
        }

        // ── MarkCompleted Tests ───────────────────────────────────────────────

        [Fact]
        public void MarkCompleted_ShouldSetStatusToCompleted_WhenRequestIsAssigned()
        {
            // Arrange
            var request = CreateOpenRequest();
            request.AssignBid(bidId: 1);

            // Act
            request.MarkCompleted();

            // Assert
            // Why: Completing from assigned state is valid
            request.Status.Should().Be(Status.completed);
            request.CompletedAt.Should().NotBeNull();
        }

        [Fact]
        public void MarkCompleted_ShouldSetStatusToCompleted_WhenRequestIsInProgress()
        {
            // Arrange
            var request = CreateOpenRequest();
            request.AssignBid(bidId: 1);
            request.MarkInProgress();

            // Act
            request.MarkCompleted();

            // Assert
            // Why: Completing from in_progress is also valid
            request.Status.Should().Be(Status.completed);
            request.CompletedAt.Should().NotBeNull();
        }

        [Fact]
        public void MarkCompleted_ShouldThrowInvalidOperationException_WhenRequestIsOpen()
        {
            // Arrange
            var request = CreateOpenRequest();

            // Act & Assert
            // Why: Cannot complete a request that was never assigned
            var act = () => request.MarkCompleted();
            act.Should().Throw<InvalidOperationException>()
               .WithMessage("Request must be assigned or in progress.");
        }

        // ── Cancel Tests ──────────────────────────────────────────────────────

        [Fact]
        public void Cancel_ShouldSetStatusToCancelled_WhenRequestIsOpen()
        {
            // Arrange
            var request = CreateOpenRequest();

            // Act
            request.Cancel();

            // Assert
            // Why: Cancellation of open requests is allowed
            request.Status.Should().Be(Status.cancelled);
        }

        [Fact]
        public void Cancel_ShouldThrowInvalidOperationException_WhenRequestIsCompleted()
        {
            // Arrange
            var request = CreateOpenRequest();
            request.AssignBid(bidId: 1);
            request.MarkCompleted();

            // Act & Assert
            // Why: Cannot cancel a completed request — business rule
            var act = () => request.Cancel();
            act.Should().Throw<InvalidOperationException>()
               .WithMessage("Cannot cancel a completed request.");
        }

        // ── SetAiData Tests ───────────────────────────────────────────────────

        [Fact]
        public void SetAiData_ShouldUpdateAiFields_WhenCalled()
        {
            // Arrange
            var request = CreateOpenRequest();

            // Act
            request.SetAiData("AI Summary", priceMin: 100m, priceMax: 300m);

            // Assert
            // Why: AI enrichment must be stored correctly
            request.AiSummary.Should().Be("AI Summary");
            request.AiPriceMin.Should().Be(100m);
            request.AiPriceMax.Should().Be(300m);
        }

        // ── UpdateDetails Tests ───────────────────────────────────────────────

        [Fact]
        public void UpdateDetails_ShouldUpdateTitleDescriptionAndScheduledAt_WhenCalled()
        {
            // Arrange
            var request = CreateOpenRequest();
            var newDate = DateTime.UtcNow.AddDays(5);

            // Act
            request.UpdateDetails("New Title", "New Description", newDate);

            // Assert
            // Why: Admin/customer edits must be applied correctly
            request.Title.Should().Be("New Title");
            request.Description.Should().Be("New Description");
            request.ScheduledAt.Should().Be(newDate);
        }

        // ── UpdateCategory Tests ──────────────────────────────────────────────

        [Fact]
        public void UpdateCategory_ShouldChangeCategoryId_WhenAiCorrects()
        {
            // Arrange
            var request = CreateOpenRequest();

            // Act
            request.UpdateCategory(99);

            // Assert
            // Why: AI matching may correct the category chosen by customer
            request.CategoryId.Should().Be(99);
        }

        // ── BaseEntity Tests ──────────────────────────────────────────────────

        [Fact]
        public void MarkCreated_ShouldSetCreatedByAndCreatedOn_WhenCalled()
        {
            // Arrange
            var request = CreateOpenRequest();

            // Act
            request.MarkCreated("user-123");

            // Assert
            // Why: Audit trail must be set on creation
            request.CreatedBy.Should().Be("user-123");
            request.CreatedOn.Should().NotBeNull();
        }

        [Fact]
        public void ToggaleStatus_ShouldSetIsDeletedToTrue_WhenCurrentlyNotDeleted()
        {
            // Arrange
            var request = CreateOpenRequest();

            // Act
            var result = request.ToggaleStatus("admin-user");

            // Assert
            // Why: Soft delete toggle must work correctly
            result.Should().BeTrue();
            request.IsDeleted.Should().BeTrue();
            request.DeletedBy.Should().Be("admin-user");
        }

        [Fact]
        public void ToggaleStatus_ShouldReturnFalse_WhenDeletedUserIsEmpty()
        {
            // Arrange
            var request = CreateOpenRequest();

            // Act
            var result = request.ToggaleStatus(string.Empty);

            // Assert
            // Why: Must not soft-delete without knowing who deleted it
            result.Should().BeFalse();
            request.IsDeleted.Should().BeNull();
        }
    }
}