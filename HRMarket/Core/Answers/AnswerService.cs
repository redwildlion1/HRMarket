using HRMarket.Configuration.Types;
using HRMarket.Core.Questions;
using HRMarket.Entities.Questions;
using Json.Schema;

namespace HRMarket.Core.Answers;

public interface IAnswerService
{
    Task<CheckAnswersResult> CheckAnswersAsync(List<AnswerDto> answers);
}

public class AnswerService(IQuestionRepository questionRepository) : IAnswerService
{
    public async Task<CheckAnswersResult> CheckAnswersAsync(List<AnswerDto> answers)
    {
        var questionsIds = answers
            .Select(a => a.QuestionId)
            .Distinct()
            .ToList();

        var questions = await questionRepository.GetByIdsAsync(questionsIds);

        if (questions.Count != questionsIds.Count)
            throw new ArgumentException("One or more question IDs are invalid.");

        var result = new CheckAnswersResult();

        foreach (var answer in answers)
        {
            var question = questions.First(q => q.Id == answer.QuestionId);

            switch (answer)
            {
                case BasicAnswerDto basic:
                    ValidateBasicAnswer(basic, question);
                    break;

                case SingleChoiceAnswerDto single:
                    ValidateSingleChoiceAnswer(single, question);
                    break;

                case MultiChoiceAnswerDto multi:
                    ValidateMultiChoiceAnswer(multi, question);
                    break;

                default:
                    throw new ArgumentException("Unknown answer type.");
            }
        }

        return result;
    }

    private static void ValidateBasicAnswer(BasicAnswerDto answer, Question question)
    {
        // Validate the response type
        QuestionTypeExtensions.IsValidValueByTypeString(
            question.Type.ToString(), answer.Response);

        // Validate against the JSON schema if provided
        if (question.ValidationJson == null) return;
        var schemaText = question.ValidationJson.RootElement.GetRawText();
        if (string.IsNullOrWhiteSpace(schemaText) || schemaText == "null") return;

        var schema = JsonSchema.FromText(schemaText);
        var evaluationResults = schema.Evaluate(answer.Response);
        if (!evaluationResults.IsValid)
            throw new ArgumentException("Answer does not conform to the question's schema.");
    }


    private static void ValidateSingleChoiceAnswer(SingleChoiceAnswerDto answer, Question question)
    {
        if (question.Type != QuestionType.SingleSelect)
            throw new ArgumentException("Question type does not match SingleSelect answer.");

        var validOptionIds = question.Options.Select(o => o.Id).ToHashSet();
        if (!validOptionIds.Contains(answer.SelectedOption))
            throw new ArgumentException($"Selected option ID {answer.SelectedOption} is not valid for this question.");
    }

    private static void ValidateMultiChoiceAnswer(MultiChoiceAnswerDto answer, Question question)
    {
        if (question.Type != QuestionType.MultiSelect)
            throw new ArgumentException("Question type does not match MultiSelect answer.");

        if (answer.SelectedOptions == null || answer.SelectedOptions.Count == 0)
            throw new ArgumentException("No options selected for a multi-select question.");

        var validOptionIds = question.Options.Select(o => o.Id).ToHashSet();
        foreach (var selectedOption in answer.SelectedOptions)
        {
            if (!validOptionIds.Contains(selectedOption))
                throw new ArgumentException($"Selected option ID {selectedOption} is not valid for this question.");
        }
    }
}
