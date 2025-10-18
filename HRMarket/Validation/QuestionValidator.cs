using HRMarket.Configuration.Types;
using System.Text.Json;
using HRMarket.Core.Questions.DTOs;
using Json.Schema;

namespace HRMarket.Validation
{
    public static class QuestionValidator
    {
        public static void ValidateCreateDto(PostQuestionDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            switch (dto.Type)
            {
                case nameof(QuestionType.SingleSelect):
                    if (dto.Options == null || dto.Options.Count == 0)
                        throw new ArgumentException("SingleSelect questions must contain options.");
                    if (dto.Options.Count != 1)
                        throw new ArgumentException("SingleSelect questions must contain exactly one option.");
                    break;

                case nameof(QuestionType.MultiSelect):
                    if (dto.Options == null || dto.Options.Count == 0)
                        throw new ArgumentException("MultiSelect questions must contain at least one option.");
                    break;

                default:
                    ValidateJsonSchemaIfPresent(dto.ValidationJson);
                    break;
            }
        }

        private static void ValidateJsonSchemaIfPresent(string? validationJson)
        {
            if (validationJson == null) return;
            

            if (string.IsNullOrWhiteSpace(validationJson) || validationJson == "null") return;

            try
            {
                _ = JsonSchema.FromText(validationJson);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Invalid validation JSON/schema.", ex);
            }
        }
    }
}