namespace FamilyCare.Domain.Tests.MedicalHistory;

public class AttachmentTests
{
    private static Attachment Upload(long sizeBytes = 1024)
        => Attachment.Upload(
            ownerEntityId: Guid.NewGuid(),
            ownerType: AttachmentOwnerType.Appointment,
            fileName: "report.pdf",
            mimeType: "application/pdf",
            storagePath: "2026/05/abc123.pdf",
            sizeBytes: sizeBytes,
            uploadedByMemberId: FamilyMemberId.New());

    [Fact]
    public void Upload_ShouldCreateAttachment()
    {
        var attachment = Upload();

        Assert.Equal("report.pdf", attachment.FileName);
        Assert.Equal("application/pdf", attachment.MimeType);
        Assert.Equal(1024, attachment.SizeBytes);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Upload_WithEmptyFileName_ShouldThrow(string fileName)
    {
        Assert.Throws<InvalidEntityStateException>(() => Attachment.Upload(
            ownerEntityId: Guid.NewGuid(),
            ownerType: AttachmentOwnerType.Appointment,
            fileName: fileName,
            mimeType: "application/pdf",
            storagePath: "path",
            sizeBytes: 1024,
            uploadedByMemberId: FamilyMemberId.New()));
    }
}
