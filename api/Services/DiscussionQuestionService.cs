using OpenAI.Chat;
using CouncilAgendaApi.Data;
using Microsoft.EntityFrameworkCore;

namespace CouncilAgendaApi.Services;

public class DiscussionQuestionService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public DiscussionQuestionService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<List<string>> GenerateQuestions(Guid handbookSectionId)
    {
        var section = await _db.HandbookSections.FindAsync(handbookSectionId)
            ?? throw new Exception("Handbook section not found");

        var apiKey = _config["OpenAI:ApiKey"]
            ?? throw new Exception("OpenAI API key not configured");

        var client = new ChatClient("gpt-4o-mini", apiKey);

        var prompt = $"""
            You are helping a church leader prepare a ward council meeting.
            
            Below is a section from the Church of Jesus Christ of Latter-day Saints handbook:
            
            Chapter {section.Chapter}, Section {section.Section}: {section.Title}
            
            {section.Content}
            
            Generate exactly 3 discussion questions that would help a ward council:
            1. Understand this principle more deeply
            2. Discuss how to apply it practically in their ward
            3. Identify specific actions they could take
            
            Format your response as a JSON array of 3 strings, nothing else.
            Example: ["Question 1?", "Question 2?", "Question 3?"]
            """;

        var response = await client.CompleteChatAsync(prompt);
        var content = response.Value.Content[0].Text;

        // Parse the JSON array response
        var questions = System.Text.Json.JsonSerializer.Deserialize<List<string>>(content)
            ?? throw new Exception("Failed to parse questions from AI response");

        return questions;
    }
}