using Mediator;
using RatDefender.Application.Dtos;
using RatDefender.Application.Queries;
using RatDefender.Domain.Services.Abstractions;

namespace RatDefender.Application.QueryHandlers;

public class GetCurrentImageQueryHandler(IImageRetriever retriever)
    : IQueryHandler<GetCurrentImageQuery, ImageDto>
{
    public async ValueTask<ImageDto> Handle(GetCurrentImageQuery query,
        CancellationToken cancellationToken)
    {
        var img = retriever.Get();
        while (img is null)
        {
            await Task.Delay(100, cancellationToken);
            img = retriever.Get();
            
            cancellationToken.ThrowIfCancellationRequested();
        }
        
        return new ImageDto(img.Buffer, img.Format);
    }
}