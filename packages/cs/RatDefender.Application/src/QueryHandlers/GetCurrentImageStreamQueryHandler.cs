using System.Runtime.CompilerServices;
using Mediator;
using RatDefender.Application.Dtos;
using RatDefender.Application.Queries;
using RatDefender.Domain.Services.Abstractions;

namespace RatDefender.Application.QueryHandlers;

public class GetCurrentImageStreamQueryHandler(IImageRetriever retriever) : IStreamQueryHandler<GetCurrentImageStreamQuery, ImageDto>
{
    public async IAsyncEnumerable<ImageDto> Handle(GetCurrentImageStreamQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {

        while (true)
        {
            var img = retriever.Get();
            
            if (img is null)
            {
                continue;
            }
            
            yield return new ImageDto(img.Buffer, img.Format);
            
            if (cancellationToken.IsCancellationRequested)
            {
                yield break;
            }
            
            await Task.Delay(20, cancellationToken);
        }
    }
}