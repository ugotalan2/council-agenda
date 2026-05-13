namespace CouncilAgendaApi.DTOs;

public record HandbookSectionResponse(
    Guid Id,
    string Chapter,
    string Section,
    string Title,
    DateTime? LastUsed,
    Guid? MinistryAreaId
);

public record HandbookSectionRequest(
    string Chapter,
    string Section,
    string Title,
    string Content,
    Guid? MinistryAreaId
);

public record DiscussionQuestionsResponse(
    Guid HandbookSectionId,
    string SectionTitle,
    List<string> Questions
);