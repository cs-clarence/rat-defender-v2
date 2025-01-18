namespace RatDefender.Application.Dtos;

public record ImageDto(
    byte[] Buffer,
    string Format
);